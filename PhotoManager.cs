using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SOACSForgeWorks.Core
{
    public static class PhotoManager
    {
        public static string SaveItemPhoto(string forgeId, string sourcePath)
        {
            RepositoryManager.EnsureRepository();
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) return string.Empty;
            string folder = RepositoryManager.GetPhotoFolder(forgeId);
            string ext = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
            string destination = Path.Combine(folder, "Photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext);
            File.Copy(sourcePath, destination, true);
            return destination;
        }

        public static string SaveItemPhoto(string forgeId, Image image)
        {
            RepositoryManager.EnsureRepository();
            if (image == null) return string.Empty;
            string folder = RepositoryManager.GetPhotoFolder(forgeId);
            string destination = Path.Combine(folder, "Photo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
            image.Save(destination, ImageFormat.Jpeg);
            return destination;
        }
    }
}
