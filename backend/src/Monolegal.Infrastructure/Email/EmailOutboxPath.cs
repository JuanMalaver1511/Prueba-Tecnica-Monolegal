namespace Monolegal.Infrastructure.Email;

internal static class EmailOutboxPath
{
    public static string Resolve(EmailSettings settings, string contentRootPath, params string[] segments)
    {
        var root = settings.OutputDirectory;

        var outputDirectory = Path.IsPathRooted(root)
            ? root
            : Path.Combine(FindRepositoryRoot(contentRootPath), root);

        return segments.Length == 0
            ? outputDirectory
            : Path.Combine(new[] { outputDirectory }.Concat(segments).ToArray());
    }

    private static string FindRepositoryRoot(string contentRootPath)
    {
        var directory = new DirectoryInfo(contentRootPath);

        while (directory is not null)
        {
            var hasSolutionMarker = File.Exists(Path.Combine(directory.FullName, "Monolegal.sln"));

            if (hasSolutionMarker && directory.Parent is not null)
            {
                return directory.Parent.FullName;
            }

            directory = directory.Parent;
        }

        return contentRootPath;
    }
}
