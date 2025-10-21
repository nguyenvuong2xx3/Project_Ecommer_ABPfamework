using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.SimpleTaskApp.UploadFile
{
	public class UploadFileAppService : IUploadFileAppService
	{
		private readonly IWebHostEnvironment _env;
		private readonly ILogger<UploadFileAppService> _logger;
		public UploadFileAppService(IWebHostEnvironment env, ILogger<UploadFileAppService> logger)
		{
			_logger = logger;
			_env = env;
		}

		public async Task RemoveImage(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				throw new ArgumentException("Image URL cannot be null or empty.", nameof(imageUrl));
			}

			try
			{
				// Resolve the physical path of the image
				string filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));

				if (File.Exists(filePath))
				{
					// Delete the file
					File.Delete(filePath);
					_logger.LogInformation($"Image successfully deleted: {imageUrl}");
				}
				else
				{
					_logger.LogWarning($"Image not found: {imageUrl}");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"An error occurred while trying to delete the image: {imageUrl}");
				throw new Exception("An error occurred while trying to delete the image. See inner exception for details.", ex);
			}
		}

		public async Task<string> UploadImageAsync(IFormFile file, string subFolder = "products")
		{
			if (file == null || file.Length == 0)
			{
				return null;
			}

			// Kiểm tra định dạng ảnh hợp lệ
			string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif" };
			string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(fileExtension))
			{
				throw new Exception($"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}");
			}

			// Kiểm tra kích thước file (giới hạn 5MB)
			if (file.Length > 5 * 1024 * 1024)
			{
				throw new Exception("Kích thước file vượt quá 5MB");
			}

			// Tạo đường dẫn thư mục lưu trữ
			string uploadsFolder = Path.Combine(_env.WebRootPath, "img", subFolder);
			Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

			// Tạo tên file unique với mã hóa:
			// Format: {timestamp}_{guid}_{original-filename-hash}{extension}
			// Ví dụ: 20231225103045_a1b2c3d4_abc123.jpg
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			string uniqueGuid = Guid.NewGuid().ToString("N").Substring(0, 8); // Lấy 8 ký tự đầu

			// Tạo hash từ tên file gốc để tránh trùng lặp nhưng vẫn có thể trace back
			string originalFileNameHash = GetFileNameHash(Path.GetFileNameWithoutExtension(file.FileName));

			string uniqueFileName = $"{timestamp}_{uniqueGuid}_{originalFileNameHash}{fileExtension}";
			string filePath = Path.Combine(uploadsFolder, uniqueFileName);

			// Lưu file vào đĩa
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}

			// Trả về đường dẫn tương đối (dùng cho web)
			return $"/img/{subFolder}/{uniqueFileName}";
		}

		/// <summary>
		/// Tạo hash ngắn gọn từ tên file để tránh trùng lặp
		/// </summary>
		private string GetFileNameHash(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return "noname";
			}

			// Chỉ lấy các ký tự alphanumeric
			string cleaned = new string(fileName.Where(c => char.IsLetterOrDigit(c)).ToArray());

			if (cleaned.Length > 10)
			{
				cleaned = cleaned.Substring(0, 10);
			}

			// Tạo hash code từ chuỗi
			int hashCode = fileName.GetHashCode();
			string hash = Math.Abs(hashCode).ToString("X").Substring(0, Math.Min(6, Math.Abs(hashCode).ToString("X").Length));

			return $"{cleaned}_{hash}".ToLower();
		}
		
	}
}
