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

		private const string UsernameKey = "Rainbow.Tfs.Login";
		private const string PasswordKey = "Rainbow.Tfs.Password";
		private const string DomainKey = "Rainbow.Tfs.Domain";

		protected string Username
		{
			get
			{
				if (!string.IsNullOrEmpty(_username)) return _username;

				var configSetting = Settings.GetSetting(UsernameKey);
				_username = configSetting;

				return _username;
			}
		}

		protected string Password
		{
			get
			{
				if (!string.IsNullOrEmpty(_password)) return _password;

				var configSetting = Settings.GetSetting(PasswordKey);
				_password = configSetting;

				return _password;
			}
		}

		protected string Domain
		{
			get
			{
				if (!string.IsNullOrEmpty(_domain)) return _domain;

				var configSetting = Settings.GetSetting(DomainKey);
				_domain = configSetting;

				return _domain;
			}
		}

		private ScmSettings GetSettings()
		{
			return new ScmSettings()
			{
				Domain = Domain,
				Password = Password,
				Username = Username,
				ApplicationRootPath = HttpContext.Current.Server.MapPath("/")
			};
		}

		public SourceControlManager()
		{
			var settings = GetSettings();
			SourceControlSync = new FileSyncTfs(settings.Username, settings.Password, settings.Domain);
		}

		public SourceControlManager(ISourceControlSync sourceControlSync)
		{
			SourceControlSync = sourceControlSync;
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