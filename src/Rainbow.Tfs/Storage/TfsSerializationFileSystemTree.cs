using System;
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
			_sourceControlManager.EditPreProcessing(path);

			base.WriteItem(item, path);

			_sourceControlManager.EditPostProcessing(path);
		}

		protected override void BeforeFilesystemDelete(string path)
		{
			_sourceControlManager.DeletePreProcessing(path);
			
			base.BeforeFilesystemDelete(path);
		}

		/// <summary>
		/// Relies on TFS PendEdit to initiate the removal on the file system
		/// </summary>
		/// <param name="item">Item to be removed</param>
		/// <returns></returns>
		public override bool Remove(IItemMetadata item)
        {
			Assert.ArgumentNotNull(item, "item");

			using (new SfsDuplicateIdCheckingDisabler())
			{
				IItemMetadata itemToRemove = GetItemForGlobalPath(item.Path, item.Id);

				if (itemToRemove == null) return false;

				var descendants = GetDescendants(item, true).Concat(new[] { itemToRemove }).OrderByDescending(desc => desc.Path).ToArray();

				foreach (var descendant in descendants)
				{
					lock (FileUtil.GetFileLock(descendant.SerializedItemId))
					{
						BeforeFilesystemDelete(descendant.SerializedItemId);

						if (!_sourceControlManager.FileExistsInSourceControl(descendant.SerializedItemId))
						{
							try
							{
								File.Delete(descendant.SerializedItemId);
							}
							catch (Exception exception)
							{
								throw new SfsDeleteException("Error deleting SFS item " + descendant.SerializedItemId, exception);
							}
						}

						AfterFilesystemDelete(descendant.SerializedItemId);

						var childrenDirectory = Path.ChangeExtension(descendant.SerializedItemId, null);

						if (Directory.Exists(childrenDirectory))
						{
							BeforeFilesystemDelete(childrenDirectory);

							if (!_sourceControlManager.FileExistsInSourceControl(childrenDirectory))
							{
								try
								{
									Directory.Delete(childrenDirectory, true);
								}
								catch (Exception exception)
								{
									throw new SfsDeleteException("Error deleting SFS directory " + childrenDirectory, exception);
								}
							}

							AfterFilesystemDelete(childrenDirectory);
						}

						var shortChildrenDirectory = Path.Combine(PhysicalRootPath, descendant.Id.ToString());
						if (Directory.Exists(shortChildrenDirectory))
						{
							BeforeFilesystemDelete(shortChildrenDirectory);

							if (!_sourceControlManager.FileExistsInSourceControl(childrenDirectory))
							{
								try
								{
									Directory.Delete(shortChildrenDirectory);
								}
								catch (Exception exception)
								{
									throw new SfsDeleteException("Error deleting SFS directory " + shortChildrenDirectory, exception);
								}
							}

							AfterFilesystemDelete(shortChildrenDirectory);
						}
					}
				}
			}

			return true;
		}
	}
}