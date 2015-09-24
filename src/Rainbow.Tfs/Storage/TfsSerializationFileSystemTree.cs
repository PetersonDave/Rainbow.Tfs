using Rainbow.Formatting;
using Rainbow.Model;
using Rainbow.Storage;
using Rainbow.Tfs.SourceControl;

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
	}
}