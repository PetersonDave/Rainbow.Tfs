namespace Rainbow.Tfs.SourceControl
{
	public interface ISourceControlSync
	{
		bool AllowFileSystemClear { get; }
		bool DeletePreProcessing(string filename);
		bool EditPreProcessing(string filename);
		bool EditPostProcessing(string filename);
	}
}