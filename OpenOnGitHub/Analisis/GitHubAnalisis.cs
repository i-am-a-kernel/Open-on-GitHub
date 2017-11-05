using LibGit2Sharp;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;


namespace OpenOnGitHub
{
    public sealed class GitHubAnalisis : GitAnalisis
    {
        
        public GitHubAnalisis(string fullPath)
        {
            _targetFullPath = fullPath;
            var repositoryPath = Repository.Discover(fullPath);
            if(repositoryPath != null)
            {
                _repository = new Repository(repositoryPath);
            }
        }

        public override string BuildGitUrl(UrlTypes type, Tuple<int, int> selectionLineRange)
        {
            // https://github.com/user/repo.git
            var originUrl = _repository.Config.Get<string>("remote.origin.url");
            if (originUrl == null)
            {
                throw new InvalidOperationException("Origin url can't found");
            }

            // https://github.com/user/repo
            var rootUrl = (originUrl.Value.EndsWith(".git", StringComparison.InvariantCultureIgnoreCase))
                ? originUrl.Value.Substring(0, originUrl.Value.Length - 4) // remove .git
                : originUrl.Value;

            // git@github.com:user/repo -> http://github.com/user/repo
            rootUrl = Regex.Replace(rootUrl, "^git@(.+):(.+)/(.+)$", match => "http://" + string.Join("/", match.Groups.OfType<Group>().Skip(1).Select(group => group.Value)), RegexOptions.IgnoreCase);

            // https://user@github.com/user/repo -> https://github.com/user/repo
            rootUrl = Regex.Replace(rootUrl, "(?<=^https?://)([^@/]+)@", string.Empty);

            // foo/bar.cs
            var rootDir = _repository.Info.WorkingDirectory;
            var fileIndexPath = _targetFullPath.Substring(rootDir.Length).Replace("\\", "/");

            var targetRepository = GetGitTargetPath(type);

            // Line selection
            var fragment = (selectionLineRange != null)
                ? (selectionLineRange.Item1 == selectionLineRange.Item2)
                    ? string.Format("#L{0}", selectionLineRange.Item1)
                    : string.Format("#L{0}-L{1}", selectionLineRange.Item1, selectionLineRange.Item2)
                : string.Empty;

            var fileUrl = string.Format("{0}/blob/{1}/{2}{3}", rootUrl.Trim('/'), WebUtility.UrlEncode(targetRepository.Trim('/')), fileIndexPath.Trim('/'), fragment);
            return fileUrl;
        }
    }
}
