using System;
using System.Web;
using Sitecore.Configuration;

namespace Rainbow.Tfs.SourceControl
{
	public class SourceControlManager : ISourceControlManager
	{
		public ISourceControlSync SourceControlSync { get; private set; }
		public bool AllowFileSystemClear { get { return SourceControlSync.AllowFileSystemClear; } }

		private string _username;
		private string _password;
		private string _domain;
	    private string _tfsUrl;

        private const string UsernameKey = "Rainbow.Tfs.Login";
		private const string PasswordKey = "Rainbow.Tfs.Password";
		private const string DomainKey = "Rainbow.Tfs.Domain";
	    private const string TfsUrlKey = "Rainbow.Tfs.TfsUrl";

        protected string Username
		{
			get
			{
				if (!string.IsNullOrEmpty(_username)) return _username;

				var configSetting = Sitecore.Configuration.Settings.GetSetting(UsernameKey);
				_username = configSetting;

				return _username;
			}
		}

		protected string Password
		{
			get
			{
				if (!string.IsNullOrEmpty(_password)) return _password;

				var configSetting = Sitecore.Configuration.Settings.GetSetting(PasswordKey);
				_password = configSetting;

				return _password;
			}
		}

		protected string Domain
		{
			get
			{
				if (!string.IsNullOrEmpty(_domain)) return _domain;

				var configSetting = Sitecore.Configuration.Settings.GetSetting(DomainKey);
				_domain = configSetting;

				return _domain;
			}
		}

	    protected string TfsUrl
        {
	        get
	        {
	            if (!string.IsNullOrEmpty(_tfsUrl)) return _tfsUrl;

	            var configSetting = Sitecore.Configuration.Settings.GetSetting(TfsUrlKey);
	            _tfsUrl = configSetting;

	            return _tfsUrl;
	        }
	    }

        private ScmSettings GetSettings()
		{
			return new ScmSettings()
			{
				Domain = Domain,
				Password = Password,
				Username = Username,
                TfsUrl = TfsUrl,
				ApplicationRootPath = HttpContext.Current.Server.MapPath("/")
			};
		}

		public SourceControlManager()
		{
			var settings = GetSettings();
			SourceControlSync = new FileSyncTfs(settings.Username, settings.Password, settings.Domain, settings.TfsUrl);
		}

		public SourceControlManager(ISourceControlSync sourceControlSync)
		{
			SourceControlSync = sourceControlSync;
		}

		public bool FileExistsInSourceControl(string filename)
		{
			return SourceControlSync.FileExistsInSourceControl(filename);
		}

		public bool EditPreProcessing(string filename)
		{
			bool success = SourceControlSync.EditPreProcessing(filename);
			if (success) return true;

			throw new Exception("[Rainbow] Edit pre-processing failed for " + filename);
		}

		public bool EditPostProcessing(string filename)
		{
			bool success = SourceControlSync.EditPostProcessing(filename);
			if (success) return true;

			throw new Exception("[Rainbow] Edit post-processing failed for " + filename);
		}

		public bool DeletePreProcessing(string filename)
		{
			bool success = SourceControlSync.DeletePreProcessing(filename);
			if (success) return true;

			throw new Exception("[Rainbow] Delete pre-processing failed for " + filename);
		}
	}
}