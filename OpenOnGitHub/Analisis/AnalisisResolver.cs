using LibGit2Sharp;
using System;
using System.Linq;

namespace OpenOnGitHub
{
    public class AnalisisResolver
    {
        private string _path;

        public AnalisisResolver() { }

        public AnalisisResolver(string path)
        {
            _path = path;
        }

        public GitAnalisis Resolve()
        {
            if (string.IsNullOrEmpty(_path))
            {
                throw new ArgumentException("Full path is not define. Use overload of Resolve method, or constructor with params.");
            }

            return Resolve(_path);
        }

        public GitAnalisis Resolve(string fullPath)
        {
            var repositoryPath = Repository.Discover(fullPath);
            if (repositoryPath == null)
            {
                throw new ArgumentException(string.Format("Can't discover git repository by path: {0}", fullPath));
            }
            var url = new Repository(repositoryPath).Network.Remotes.First().Url;

            if (url.Contains("github.com")) return new GitHubAnalisis(fullPath);
            if (url.Contains("bitbucket.org")) return new BitbucketAnalisis(fullPath);

            throw new InvalidOperationException("Can't select git analisis implementation.");
        }
    }
}
