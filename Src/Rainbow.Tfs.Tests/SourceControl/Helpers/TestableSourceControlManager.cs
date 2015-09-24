using Rainbow.Tfs.SourceControl;

namespace Rainbow.Tfs.Tests.SourceControl.Helpers
{
	public class TestableSourceControlManager : SourceControlManager
	{
		public TestableSourceControlManager(bool success) : base(new TestableTfsFileSync(success)) { }
	}
}