namespace Rainbow.Tfs.SourceControl
{
	public interface ISourceControlSync
	{
		bool AllowFileSystemClear { get; }
		bool FileExistsInSourceControl(string filename);
		bool DeletePreProcessing(string filename);
		bool EditPreProcessing(string filename);
		bool EditPostProcessing(string filename);
	}
}