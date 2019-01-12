﻿using System;
using System.Net;
using System.Web;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace Rainbow.Tfs.SourceControl
{
	public class FileSyncTfs : ISourceControlSync
	{
		private readonly WorkspaceInfo _workspaceInfo;
		private readonly NetworkCredential _networkCredential;

		public bool AllowFileSystemClear { get { return false; } }

		public FileSyncTfs(string username, string password, string domain, string tfsUrl)
		{
			_networkCredential = new NetworkCredential(username, password, domain);

            //fix for VS2017 / tfs 15
		    Microsoft.VisualStudio.Services.Common.WindowsCredential winCred = new Microsoft.VisualStudio.Services.Common.WindowsCredential(_networkCredential);
		    VssCredentials vssCred = new VssClientCredentials(winCred)
            {
		        PromptType = CredentialPromptType.DoNotPrompt
		    };
		    Uri tfsUri = new Uri(tfsUrl);
		    TfsTeamProjectCollection teamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri, vssCred);
		    VersionControlServer tfServer = teamProjectCollection.GetService<VersionControlServer>();
		    Workstation.Current.EnsureUpdateWorkspaceInfoCache(tfServer, tfServer.AuthorizedUser);

            var applicationRootPath = HttpContext.Current.Server.MapPath("/");
			_workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(applicationRootPath);
			AssertWorkspace(_workspaceInfo, applicationRootPath);

			EnsureUpdateWorkspaceInfoCache();
		}

		private void AssertWorkspace(WorkspaceInfo workspaceInfo, string filename)
		{
			if (workspaceInfo != null) return;
			throw new Exception("[Rainbow.Tfs] No workspace is available or defined for the path. Verify your ASP.NET impersonation credentials in IIS for local TFS cache access. File " + filename);
		}

		private TfsPersistentConnection GetTfsPersistentConnection()
		{
			return TfsPersistentConnection.Instance(_workspaceInfo.ServerUri, _networkCredential);
		}

		/// <summary>
		/// Kick local TFS workspace to ensure TFS cache has been updated
		/// </summary>
		private void EnsureUpdateWorkspaceInfoCache()
		{
			var connection = GetTfsPersistentConnection();
			var versionControlServer = (VersionControlServer)connection.TfsTeamProjectCollection.GetService(typeof(VersionControlServer));

			Workstation.Current.UpdateWorkspaceInfoCache(versionControlServer, versionControlServer.AuthorizedUser);
			Workstation.Current.EnsureUpdateWorkspaceInfoCache(versionControlServer, versionControlServer.AuthorizedUser);
		}

		public bool FileExistsInSourceControl(string filename)
		{
			var connection = GetTfsPersistentConnection();
			var handler = new TfsFileHandler(connection.TfsTeamProjectCollection, filename);
			return handler.GetFileExistsOnServer();
		}

		public bool DeletePreProcessing(string filename)
		{
			var connection = GetTfsPersistentConnection();
			var handler = new TfsFileHandler(connection.TfsTeamProjectCollection, filename);
			return handler.CheckoutFileForDelete();
		}

		public bool EditPreProcessing(string filename)
		{
			var connection = GetTfsPersistentConnection();
			var handler = new TfsFileHandler(connection.TfsTeamProjectCollection, filename);

			// nothing to do if the file has yet to be added to TFS
			if (!handler.FileExistsOnServer) return true;
			
			return handler.CheckoutFileForEdit();
		}

		public bool EditPostProcessing(string filename)
		{
			var connection = GetTfsPersistentConnection();
			var handler = new TfsFileHandler(connection.TfsTeamProjectCollection, filename);

			// nothing to do if the file does not exist on the file system
			if (!handler.FileExistsOnFileSystem) return true;

			return handler.FileExistsOnServer ? handler.CheckoutFileForEdit() : handler.AddFile();
		}
	}
}