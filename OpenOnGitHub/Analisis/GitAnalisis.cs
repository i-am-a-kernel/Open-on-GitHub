using LibGit2Sharp;
using System;
using System.Linq;

namespace OpenOnGitHub
{
    public abstract class GitAnalisis : IDisposable
    {
        protected Repository _repository;
        protected string _targetFullPath;

        public bool IsDiscoveredGitRepository => _repository != null;

        public abstract string BuildGitUrl(UrlTypes type, Tuple<int, int> slectionLineRange);

        public string GetGitTargetDescription(UrlTypes type)
        {
            switch (type)
            {
                case UrlTypes.CurrentBranch:
                    return string.Format("Branch: {0}", _repository.Head.CanonicalName.Replace("origin/", ""));
                case UrlTypes.CurrentRevision:
                    return string.Format("Revision: {0}", _repository.Commits.First().Id.ToString(8));
                case UrlTypes.CurrentRevisionFull:
                    return string.Format("Revision: {0}... (Full ID)", _repository.Commits.First().Id.ToString(8));
                case UrlTypes.Master:
                default:
                    return "master";
            }
        }

        public string GetGitTargetPath(UrlTypes type)
        {
            switch (type)
            {
                case UrlTypes.CurrentBranch:
                    return _repository.Head.CanonicalName.Replace("origin/", "");
                case UrlTypes.CurrentRevision:
                    return _repository.Commits.First().Id.ToString(8);
                case UrlTypes.CurrentRevisionFull:
                    return _repository.Commits.First().Id.Sha;
                case UrlTypes.Master:
                default:
                    return "master";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && _repository != null)
            {
                _repository.Dispose();
            }
        }

        ~GitAnalisis()
        {
            Dispose(false);
        }
    }
}
