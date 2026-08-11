using System;
using System.Collections.Generic;
using System.IO;

namespace SOACSForgeWorks.Core
{
    public static class AttachmentManager
    {
        public static string SaveItemAttachment(string forgeId, string sourcePath)
        {
            RepositoryManager.EnsureRepository();
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) return string.Empty;
            string itemDir = RepositoryManager.GetItemAttachmentFolder(forgeId);
            string safe = MakeSafeFileName(Path.GetFileName(sourcePath));
            string destination = GetUniqueFilePath(Path.Combine(itemDir, safe));
            if (!StringEqualsPath(sourcePath, destination)) File.Copy(sourcePath, destination, false);
            return destination;
        }

        public static string Resolve(AttachmentRecord attachment)
        {
            if (attachment == null) return string.Empty;
            if (!string.IsNullOrWhiteSpace(attachment.RelativePath))
            {
                string resolved = RepositoryManager.ResolvePath(attachment.RelativePath);
                if (File.Exists(resolved)) return resolved;
            }
            if (!string.IsNullOrWhiteSpace(attachment.FilePath)) return attachment.FilePath;
            return string.Empty;
        }

        public static string GetItemFolder(string forgeId)
        {
            return RepositoryManager.GetItemAttachmentFolder(forgeId);
        }

        public static void DeleteAttachmentFile(AttachmentRecord attachment)
        {
            string path = Resolve(attachment);
            if (File.Exists(path))
            {
                try { File.Delete(path); } catch { }
            }
        }

        public static string MakeRelativePath(string fullPath)
        {
            return RepositoryManager.MakeRelativePath(fullPath);
        }

        public static string GuessDocumentCategory(string fileName)
        {
            string ext = Path.GetExtension(fileName ?? string.Empty).TrimStart('.').ToLowerInvariant();
            if (ext == "pdf") return "PDF / Drawing / Manual";
            if (ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "bmp" || ext == "gif") return "Photo";
            if (ext == "doc" || ext == "docx") return "Word Document";
            if (ext == "xls" || ext == "xlsx" || ext == "csv") return "Spreadsheet";
            if (ext == "stl") return "3D Print File";
            if (ext == "step" || ext == "stp" || ext == "iges" || ext == "igs") return "CAD Model";
            if (ext == "dxf" || ext == "dwg") return "Engineering Drawing";
            if (ext == "zip" || ext == "7z") return "Archive";
            return string.IsNullOrWhiteSpace(ext) ? "Other" : ext.ToUpperInvariant();
        }

        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Attachment";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name.Trim();
        }

        private static string GetUniqueFilePath(string path)
        {
            if (!File.Exists(path)) return path;
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            for (int i = 1; i < 10000; i++)
            {
                string candidate = Path.Combine(dir, name + "_" + i + ext);
                if (!File.Exists(candidate)) return candidate;
            }
            return Path.Combine(dir, name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext);
        }

        private static bool StringEqualsPath(string a, string b)
        {
            try { return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase); }
            catch { return string.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
        }
    }
}
