namespace Rainbow.Tfs.SourceControl
{
	public interface ISourceControlManager
	{
		ISourceControlSync SourceControlSync { get; }
		bool AllowFileSystemClear { get; }
		bool FileExistsInSourceControl(string filename);
		bool EditPreProcessing(string filename);
		bool EditPostProcessing(string filename);
		bool DeletePreProcessing(string filename);
	}
}