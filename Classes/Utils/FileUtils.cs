using System.IO;

namespace Koinonia
{
    public static class FileUtils
    {

        public static void DeleteFolder(DirectoryInfo source)
        {
            if (source == null) return;
            if(source.Exists)
            source.Delete(true);
        }

        // http://stackoverflow.com/a/58779
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            if(!target.Exists)target.Create();
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public static void EnsureFolderFor(string configPath)
        {

            var path = Path.GetDirectoryName(configPath);
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);

        }
    }
}