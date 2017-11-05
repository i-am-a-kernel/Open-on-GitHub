using LibGit2Sharp;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace OpenOnGitHub
{
    public sealed class BitbucketAnalisis : GitAnalisis
    {
        public BitbucketAnalisis(string fullPath)
        {
            _targetFullPath = fullPath;
            var repositoryPath = Repository.Discover(fullPath);
            if (repositoryPath != null)
            {
                _repository = new Repository(repositoryPath);
            }
        }

        public override string BuildGitUrl(UrlTypes type, Tuple<int, int> selectionLineRange)
        {
            var originUrl = _repository.Config.Get<string>("remote.origin.url");
            if (originUrl == null)
            {
                throw new InvalidOperationException("Origin url can't found");
            }

            var rootUrl = (originUrl.Value.EndsWith(".git", StringComparison.InvariantCultureIgnoreCase))
                ? originUrl.Value.Substring(0, originUrl.Value.Length - 4) // remove .git
                : originUrl.Value;

            rootUrl = Regex.Replace(rootUrl, "^git@(.+):(.+)/(.+)$", match => "http://" + string.Join("/", match.Groups.OfType<Group>().Skip(1).Select(group => group.Value)), RegexOptions.IgnoreCase);

            rootUrl = Regex.Replace(rootUrl, "([^@/]+)@", string.Empty);

            // foo/bar.cs
            var rootDir = _repository.Info.WorkingDirectory;
            var fileIndexPath = _targetFullPath.Substring(rootDir.Length).Replace("\\", "/");

            var targetRepository = GetGitTargetPath(type);

            // Line selection
            var fragment = string.Empty;
            if(selectionLineRange != null)
            {
                var lines = string.Join(",", Enumerable.Range(selectionLineRange.Item1, selectionLineRange.Item2 - selectionLineRange.Item1 + 1));
                fragment = string.Format("#{0}-{1}", fileIndexPath.Split('/').Last(), lines);
            }

            var fileUrl = string.Format("{0}/src/{1}/{2}?fileviewer=file-view-default{3}",
                rootUrl.Trim('/'),
                WebUtility.UrlEncode(targetRepository.Trim('/')),
                fileIndexPath.Trim('/'),
                fragment);
            return fileUrl;
        }
    }
}
