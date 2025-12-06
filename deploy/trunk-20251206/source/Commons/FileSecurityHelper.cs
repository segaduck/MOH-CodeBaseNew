using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace EECOnline.Commons
{
    /// <summary>
    /// 檔案上傳安全性驗證工具類
    /// 提供集中式的檔案驗證功能，包含副檔名、MIME 類型、Magic Bytes（檔案頭）驗證
    /// </summary>
    public static class FileSecurityHelper
    {
        #region Magic Bytes 定義

        /// <summary>
        /// 檔案類型 Magic Bytes 對照表
        /// Key: 副檔名（小寫），Value: 可能的 Magic Bytes 陣列
        /// </summary>
        private static readonly Dictionary<string, List<byte[]>> MagicBytesMap = new Dictionary<string, List<byte[]>>
        {
            // PDF
            { ".pdf", new List<byte[]> { 
                new byte[] { 0x25, 0x50, 0x44, 0x46 }  // %PDF
            }},
            
            // JPEG
            { ".jpg", new List<byte[]> { 
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },  // JFIF
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },  // Exif
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },  // Canon
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }   // Samsung
            }},
            { ".jpeg", new List<byte[]> { 
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }
            }},
            
            // PNG
            { ".png", new List<byte[]> { 
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }},
            
            // GIF
            { ".gif", new List<byte[]> { 
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },  // GIF87a
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }   // GIF89a
            }},
            
            // BMP
            { ".bmp", new List<byte[]> { 
                new byte[] { 0x42, 0x4D }  // BM
            }},
            
            // TIFF
            { ".tif", new List<byte[]> { 
                new byte[] { 0x49, 0x49, 0x2A, 0x00 },  // Little-endian
                new byte[] { 0x4D, 0x4D, 0x00, 0x2A }   // Big-endian
            }},
            { ".tiff", new List<byte[]> { 
                new byte[] { 0x49, 0x49, 0x2A, 0x00 },
                new byte[] { 0x4D, 0x4D, 0x00, 0x2A }
            }},
            
            // ZIP (also used by Office 2007+ formats)
            { ".zip", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 },  // PK
                new byte[] { 0x50, 0x4B, 0x05, 0x06 },  // Empty archive
                new byte[] { 0x50, 0x4B, 0x07, 0x08 }   // Spanned archive
            }},
            
            // Office 2007+ (all use ZIP format)
            { ".xlsx", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            { ".docx", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            { ".pptx", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            
            // Office 97-2003 (OLE2 format)
            { ".doc", new List<byte[]> { 
                new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }
            }},
            { ".xls", new List<byte[]> { 
                new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }
            }},
            { ".ppt", new List<byte[]> { 
                new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }
            }},
            
            // OpenDocument formats
            { ".odt", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            { ".ods", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            { ".odp", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            { ".odg", new List<byte[]> { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }
            }},
            
            // Text files
            { ".txt", new List<byte[]> { 
                // Text files don't have reliable magic bytes
                // Will skip magic bytes check for .txt
            }},
            { ".csv", new List<byte[]> { 
                // CSV files don't have magic bytes
            }},
            { ".dat", new List<byte[]> { 
                // DAT files vary by application
            }}
        };

        /// <summary>
        /// 危險副檔名黑名單（絕對不允許上傳）
        /// </summary>
        private static readonly HashSet<string> BlacklistedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".aspx", ".ashx", ".asmx", ".asax",  // ASP.NET executable files
            ".exe", ".dll", ".com", ".bat",       // Windows executables
            ".cmd", ".vbs", ".js", ".ps1",        // Scripts
            ".config", ".cer", ".pfx",            // Configuration/Certificates
            ".msi", ".msp", ".msu",               // Installers
            ".scr", ".vbe", ".wsf", ".wsh",       // More scripts
            ".reg", ".inf", ".cpl",               // System files
            ".jar", ".war", ".ear",               // Java archives
            ".php", ".jsp", ".jspx",              // Other web scripts
            ".app", ".deb", ".rpm"                // Installers
        };

        #endregion

        #region 檔案驗證結果類別

        /// <summary>
        /// 檔案上傳驗證結果
        /// </summary>
        public class FileUploadValidationResult
        {
            /// <summary>
            /// 是否驗證通過
            /// </summary>
            public bool IsValid { get; set; }

            /// <summary>
            /// 錯誤訊息（驗證失敗時）
            /// </summary>
            public string ErrorMessage { get; set; }

            /// <summary>
            /// 詳細驗證錯誤清單
            /// </summary>
            public List<string> ValidationErrors { get; set; }

            /// <summary>
            /// 安全的檔案名稱（已清理特殊字元）
            /// </summary>
            public string SafeFileName { get; set; }

            /// <summary>
            /// 偵測到的 MIME 類型
            /// </summary>
            public string DetectedMimeType { get; set; }

            /// <summary>
            /// 偵測到的副檔名（根據 Magic Bytes）
            /// </summary>
            public string DetectedExtension { get; set; }

            public FileUploadValidationResult()
            {
                ValidationErrors = new List<string>();
                IsValid = true;
            }
        }

        #endregion

        #region 公開驗證方法

        /// <summary>
        /// 驗證上傳的檔案（完整驗證：副檔名、MIME、Magic Bytes、大小）
        /// </summary>
        /// <param name="file">上傳的檔案</param>
        /// <param name="allowedExtensions">允許的副檔名清單（含點，例如 ".pdf"）</param>
        /// <param name="maxSizeBytes">最大檔案大小（bytes）</param>
        /// <returns>驗證結果</returns>
        public static FileUploadValidationResult ValidateUploadedFile(
            HttpPostedFileBase file, 
            List<string> allowedExtensions, 
            long maxSizeBytes)
        {
            var result = new FileUploadValidationResult();

            // 1. 基本檢查
            if (file == null || file.ContentLength == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "請選擇上傳檔案";
                result.ValidationErrors.Add("檔案為空或未選擇");
                return result;
            }

            // 2. 檔案大小檢查
            if (file.ContentLength > maxSizeBytes)
            {
                result.IsValid = false;
                result.ErrorMessage = $"檔案大小超過限制（最大 {FormatFileSize(maxSizeBytes)}）";
                result.ValidationErrors.Add($"檔案大小 {FormatFileSize(file.ContentLength)} 超過限制 {FormatFileSize(maxSizeBytes)}");
                return result;
            }

            // 3. 副檔名檢查
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrEmpty(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案必須包含副檔名";
                result.ValidationErrors.Add("檔案名稱缺少副檔名");
                return result;
            }

            // 4. 檢查是否為黑名單副檔名
            if (IsBlacklistedExtension(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "此檔案類型不允許上傳（安全性限制）";
                result.ValidationErrors.Add($"副檔名 {extension} 為危險類型，已被封鎖");
                return result;
            }

            // 5. 檢查是否在允許清單內
            if (allowedExtensions != null && allowedExtensions.Any())
            {
                var normalizedAllowed = allowedExtensions.Select(e => e.ToLower()).ToList();
                if (!normalizedAllowed.Contains(extension))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"不允許的檔案類型，僅接受：{string.Join(", ", allowedExtensions)}";
                    result.ValidationErrors.Add($"副檔名 {extension} 不在允許清單中");
                    return result;
                }
            }

            // 6. 檢查雙重副檔名（例如 shell.aspx.pdf）
            if (HasDoubleExtension(file.FileName))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案名稱包含多個副檔名，可能為惡意檔案";
                result.ValidationErrors.Add("偵測到雙重副檔名");
                return result;
            }

            // 7. MIME 類型檢查
            result.DetectedMimeType = file.ContentType;
            if (!ValidateMimeType(extension, file.ContentType))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案的 MIME 類型與副檔名不符";
                result.ValidationErrors.Add($"MIME 類型 {file.ContentType} 與副檔名 {extension} 不匹配");
                return result;
            }

            // 8. Magic Bytes 檢查（檔案頭驗證）
            var magicBytesValid = ValidateMagicBytes(file.InputStream, extension);
            if (!magicBytesValid)
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案內容與副檔名不符（可能為偽造檔案）";
                result.ValidationErrors.Add("Magic Bytes 驗證失敗");
                return result;
            }

            // 9. 生成安全檔名
            result.SafeFileName = GenerateSecureFileName(file.FileName);
            result.DetectedExtension = extension;

            return result;
        }

        /// <summary>
        /// 驗證 Base64 編碼的檔案（用於 API）
        /// </summary>
        /// <param name="base64Content">Base64 編碼的檔案內容</param>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="allowedExtensions">允許的副檔名清單</param>
        /// <param name="maxSizeBytes">最大檔案大小</param>
        /// <returns>驗證結果</returns>
        public static FileUploadValidationResult ValidateBase64File(
            string base64Content,
            string fileName,
            List<string> allowedExtensions,
            long maxSizeBytes)
        {
            var result = new FileUploadValidationResult();

            // 1. 驗證 Base64 格式
            if (!IsValidBase64(base64Content))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案格式錯誤（Base64 驗證失敗）";
                result.ValidationErrors.Add("Base64 格式無效");
                return result;
            }

            // 2. 解碼 Base64
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64Content);
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案解碼失敗";
                result.ValidationErrors.Add($"Base64 解碼錯誤: {ex.Message}");
                return result;
            }

            // 3. 檔案大小檢查
            if (fileBytes.Length > maxSizeBytes)
            {
                result.IsValid = false;
                result.ErrorMessage = $"檔案大小超過限制（最大 {FormatFileSize(maxSizeBytes)}）";
                result.ValidationErrors.Add($"檔案大小 {FormatFileSize(fileBytes.Length)} 超過限制");
                return result;
            }

            // 4. 副檔名檢查
            var extension = Path.GetExtension(fileName)?.ToLower();
            if (string.IsNullOrEmpty(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案必須包含副檔名";
                result.ValidationErrors.Add("檔案名稱缺少副檔名");
                return result;
            }

            // 5. 黑名單檢查
            if (IsBlacklistedExtension(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "此檔案類型不允許上傳";
                result.ValidationErrors.Add($"副檔名 {extension} 為危險類型");
                return result;
            }

            // 6. 允許清單檢查
            if (allowedExtensions != null && allowedExtensions.Any())
            {
                var normalizedAllowed = allowedExtensions.Select(e => e.ToLower()).ToList();
                if (!normalizedAllowed.Contains(extension))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"不允許的檔案類型，僅接受：{string.Join(", ", allowedExtensions)}";
                    result.ValidationErrors.Add($"副檔名 {extension} 不在允許清單中");
                    return result;
                }
            }

            // 7. Magic Bytes 驗證
            using (var ms = new MemoryStream(fileBytes))
            {
                if (!ValidateMagicBytes(ms, extension))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "檔案內容與副檔名不符";
                    result.ValidationErrors.Add("Magic Bytes 驗證失敗");
                    return result;
                }
            }

            result.SafeFileName = GenerateSecureFileName(fileName);
            result.DetectedExtension = extension;

            return result;
        }

        #endregion

        #region 私有驗證方法

        /// <summary>
        /// 驗證 MIME 類型
        /// </summary>
        private static bool ValidateMimeType(string extension, string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return false;

            var expectedMimeTypes = GetExpectedMimeTypes(extension);
            return expectedMimeTypes.Any(m => mimeType.ToLower().Contains(m.ToLower()));
        }

        /// <summary>
        /// 根據副檔名取得預期的 MIME 類型
        /// </summary>
        private static List<string> GetExpectedMimeTypes(string extension)
        {
            var mimeMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { ".pdf", new List<string> { "application/pdf" }},
                { ".jpg", new List<string> { "image/jpeg", "image/pjpeg" }},
                { ".jpeg", new List<string> { "image/jpeg", "image/pjpeg" }},
                { ".png", new List<string> { "image/png" }},
                { ".gif", new List<string> { "image/gif" }},
                { ".bmp", new List<string> { "image/bmp", "image/x-windows-bmp" }},
                { ".tif", new List<string> { "image/tiff" }},
                { ".tiff", new List<string> { "image/tiff" }},
                { ".doc", new List<string> { "application/msword" }},
                { ".docx", new List<string> { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }},
                { ".xls", new List<string> { "application/vnd.ms-excel" }},
                { ".xlsx", new List<string> { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }},
                { ".ppt", new List<string> { "application/vnd.ms-powerpoint" }},
                { ".pptx", new List<string> { "application/vnd.openxmlformats-officedocument.presentationml.presentation" }},
                { ".txt", new List<string> { "text/plain" }},
                { ".csv", new List<string> { "text/csv", "application/csv" }},
                { ".zip", new List<string> { "application/zip", "application/x-zip-compressed" }}
            };

            return mimeMap.ContainsKey(extension) ? mimeMap[extension] : new List<string> { "application/octet-stream" };
        }

        /// <summary>
        /// 驗證 Magic Bytes（檔案頭）
        /// </summary>
        private static bool ValidateMagicBytes(Stream fileStream, string extension)
        {
            // 某些檔案類型不需要 Magic Bytes 驗證
            if (extension == ".txt" || extension == ".csv" || extension == ".dat")
                return true;

            if (!MagicBytesMap.ContainsKey(extension))
                return true; // 未定義 Magic Bytes 的格式，跳過驗證

            var expectedMagicBytes = MagicBytesMap[extension];
            if (expectedMagicBytes == null || !expectedMagicBytes.Any())
                return true;

            // 讀取檔案前 20 bytes
            var buffer = new byte[20];
            var originalPosition = fileStream.Position;
            fileStream.Position = 0;
            var bytesRead = fileStream.Read(buffer, 0, 20);
            fileStream.Position = originalPosition;

            if (bytesRead == 0)
                return false;

            // 檢查是否符合任一 Magic Bytes
            foreach (var magicBytes in expectedMagicBytes)
            {
                if (magicBytes.Length == 0) continue;
                
                if (bytesRead >= magicBytes.Length)
                {
                    var matches = true;
                    for (int i = 0; i < magicBytes.Length; i++)
                    {
                        if (buffer[i] != magicBytes[i])
                        {
                            matches = false;
                            break;
                        }
                    }
                    if (matches)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 檢查是否為黑名單副檔名
        /// </summary>
        public static bool IsBlacklistedExtension(string extension)
        {
            return BlacklistedExtensions.Contains(extension);
        }

        /// <summary>
        /// 檢查是否有雙重副檔名
        /// </summary>
        private static bool HasDoubleExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var parts = fileName.Split('.');
            if (parts.Length <= 2)
                return false;

            // 檢查倒數第二個是否為可執行副檔名
            if (parts.Length >= 3)
            {
                var secondLastPart = "." + parts[parts.Length - 2].ToLower();
                if (BlacklistedExtensions.Contains(secondLastPart))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 驗證 Base64 格式
        /// </summary>
        public static bool IsValidBase64(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 生成安全的檔案名稱（GUID + 時間戳 + 副檔名）
        /// </summary>
        public static string GenerateSecureFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{timestamp}_{guid}{extension}";
        }

        /// <summary>
        /// 清理檔案名稱（移除特殊字元）
        /// </summary>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName
                .Where(c => !invalidChars.Contains(c) && c != '/' && c != '\\')
                .ToArray());

            return sanitized;
        }

        /// <summary>
        /// 格式化檔案大小顯示
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "Bytes", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
}
