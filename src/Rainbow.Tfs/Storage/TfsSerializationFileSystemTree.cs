using System.IO;
using System.Linq;
using Rainbow.Formatting;
using Rainbow.Model;
using Rainbow.Storage;
using Rainbow.Tfs.SourceControl;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Rainbow.Tfs.Storage
{
	public class TfsSerializationFileSystemTree : SerializationFileSystemTree
	{
		private readonly ISourceControlManager _sourceControlManager;

		public TfsSerializationFileSystemTree(string name, string globalRootItemPath, string databaseName, string physicalRootPath, ISerializationFormatter formatter, bool useDataCache) : base(name, globalRootItemPath, databaseName, physicalRootPath, formatter, useDataCache)
		{
			_sourceControlManager = new SourceControlManager();
		}

		protected override void WriteItem(IItemData item, string path)
		{
			EditPreProcessing(path);

			base.WriteItem(item, path);
	
			EditPostProcessing(path);
		}

		public override bool Remove(IItemData item)
		{
			Assert.ArgumentNotNull(item, "item");

			IItemMetadata itemToRemove = GetItemForGlobalPath(item.Path, item.Id);

			if (itemToRemove == null) return false;

			var descendants = GetDescendants(item, true).Concat(new[] { itemToRemove }).OrderByDescending(desc => desc.Path).ToArray();

			foreach (var descendant in descendants)
			{
				lock (FileUtil.GetFileLock(descendant.SerializedItemId))
				{
					_sourceControlManager.DeletePreProcessing(descendant.SerializedItemId);
					File.Delete(descendant.SerializedItemId);

					var childrenDirectory = Path.ChangeExtension(descendant.SerializedItemId, null);
					if (Directory.Exists(childrenDirectory))
					{
						_sourceControlManager.DeletePreProcessing(childrenDirectory);
						Directory.Delete(childrenDirectory, true);
					}

					var shortChildrenDirectory = Path.Combine(PhysicalRootPath, descendant.Id.ToString());
					if (Directory.Exists(shortChildrenDirectory))
					{
						_sourceControlManager.DeletePreProcessing(shortChildrenDirectory);
						Directory.Delete(shortChildrenDirectory);
					}
				}
			}

			return true;
		}

		private bool EditPreProcessing(string filename)
		{
			return _sourceControlManager.EditPreProcessing(filename);
		}

		private bool EditPostProcessing(string filename)
		{
			return _sourceControlManager.EditPostProcessing(filename);
		}
	}
}