using System;
using System.IO;
using Rainbow.Formatting;
using Rainbow.Storage;
using Rainbow.Tfs.SourceControl;

namespace Rainbow.Tfs.Storage
{
	public class TfsSerializationFileSystemDataStore : SerializationFileSystemDataStore
	{
		private readonly ISourceControlManager _sourceControlManager;

		public TfsSerializationFileSystemDataStore(string physicalRootPath, bool useDataCache, ITreeRootFactory rootFactory, ISerializationFormatter formatter) : base(physicalRootPath, useDataCache, rootFactory, formatter)
		{
			_sourceControlManager = new SourceControlManager();
		}

		protected override SerializationFileSystemTree CreateTree(TreeRoot root, ISerializationFormatter formatter, bool useDataCache)
		{
			var tree = new TfsSerializationFileSystemTree(root.Name, root.Path, root.DatabaseName, Path.Combine(PhysicalRootPath, root.Name), formatter, useDataCache);
			tree.TreeItemChanged += metadata =>
			{
				foreach (var watcher in ChangeWatchers) watcher(metadata, tree.DatabaseName);
			};

			return tree;
		}

		public override void Clear()
		{
			if (!Directory.Exists(PhysicalRootPath)) return;

			if (!_sourceControlManager.AllowFileSystemClear)
			{
				throw new InvalidOperationException("Cannot clear the local file system. The serialization tree must first be cleared in source control before continuing.");
			}

			base.Clear();
		}
	}
}