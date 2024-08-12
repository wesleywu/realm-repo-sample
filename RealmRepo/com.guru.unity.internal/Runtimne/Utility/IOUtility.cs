#nullable disable
namespace Guru.Internal
{
    public static class IOUtility
    {
        public static void MakeSureDirectoryExist(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (directory == null) return;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void ClearFolder(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                DirectoryInfo dir = new DirectoryInfo(folderName);

                foreach(FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    ClearFolder(di.FullName);
                    di.Delete();
                }   
            }
        }
        
        public static void CopyFolder( string srcPath, string tarPath )
        {
            CopyFile (srcPath, tarPath);
            string[] directionName = Directory.GetDirectories (srcPath);
            foreach (string dirPath in directionName) 
            {
                string directionPathTemp = tarPath + "\\" + dirPath.Substring (srcPath.Length + 1);
                CopyFolder (dirPath, directionPathTemp);
            }
        }
        
        public static void CopyFile( string srcPath, string tarPath )
        {
            if(!Directory.Exists(tarPath))
            {
                if (tarPath != null) Directory.CreateDirectory(tarPath);
            }
            
            string[] filesList = Directory.GetFiles (srcPath);
            foreach (string f in filesList) 
            {
                string fTarPath = tarPath + "/" + f.Substring (srcPath.Length + 1);
                if (File.Exists (fTarPath)) {
                    File.Copy (f, fTarPath, true);
                } else {
                    File.Copy (f, fTarPath);
                }
            }
        }
    }
}