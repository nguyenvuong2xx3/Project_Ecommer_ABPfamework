using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Acme.SimpleTaskApp.Authorization;
using Acme.SimpleTaskApp.Authorization.Users;
using Acme.SimpleTaskApp.ProductComments.Dtos;
using Acme.SimpleTaskApp.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Notifications;
using Acme.SimpleTaskApp.Authorization.Roles;

namespace Acme.SimpleTaskApp.ProductComments
{
	[AbpAuthorize] // Yêu cầu đăng nhập để comment
	public class ProductCommentAppService : ApplicationService, IProductCommentAppService
	{
		private readonly IRepository<ProductComment, int> _commentRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly INotificationPublisher _notificationPublisher;
		private readonly UserManager _userManager;
		private readonly RoleManager _roleManager;

		public ProductCommentAppService(
			IRepository<ProductComment, int> commentRepository,
			IRepository<User, long> userRepository,
			IRepository<Product, int> productRepository,
			INotificationPublisher notificationPublisher,
			UserManager userManager,
			RoleManager roleManager)
		{
			_commentRepository = commentRepository;
			_userRepository = userRepository;
			_productRepository = productRepository;
			_notificationPublisher = notificationPublisher;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		/// <summary>
		/// Tạo comment mới
		/// </summary>
		public async Task<ProductCommentDto> CreateComment(CreateProductCommentDto input)
		{
			if (string.IsNullOrWhiteSpace(input.Content))
			{
				throw new UserFriendlyException("Nội dung comment không được để trống");
			}

			if (input.Content.Length > 1000)
			{
				throw new UserFriendlyException("Nội dung comment không được vượt quá 1000 ký tự");
			}

			var currentUserId = AbpSession.UserId;
			if (!currentUserId.HasValue)
			{
				throw new UserFriendlyException("Bạn cần đăng nhập để comment");
			}

			// Get product info
			var product = await _productRepository.GetAsync(input.ProductId);
			if (product == null)
			{
				throw new UserFriendlyException("Sản phẩm không tồn tại");
			}

			// Validate ParentCommentId nếu có
			if (input.ParentCommentId.HasValue)
			{
				var parentExists = await _commentRepository.GetAll()
					.AnyAsync(c => c.Id == input.ParentCommentId.Value);
				
				if (!parentExists)
				{
					throw new UserFriendlyException("Comment cha không tồn tại");
				}
			}

			var comment = new ProductComment
			{
				UserId = currentUserId.Value,
				ProductId = input.ProductId,
				Content = input.Content.Trim(),
				ParentCommentId = input.ParentCommentId,
				IsApproved = true, // Mặc định approve
				IsEdited = false
			};

			await _commentRepository.InsertAsync(comment);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Get current user info
			var currentUser = await _userRepository.GetAsync(currentUserId.Value);
			var userName = $"{currentUser.Name} {currentUser.Surname}".Trim();

			// Send notification to admins
			await SendCommentNotificationToAdmins(
				productId: product.Id,
				productName: product.Name,
				userName: userName,
				commentContent: input.Content
			);

			// Load lại để map sang DTO
			var createdComment = await _commentRepository.GetAsync(comment.Id);
			var result = await MapCommentsToDto(new List<ProductComment> { createdComment });

			return result.FirstOrDefault();
		}

		/// <summary>
		/// Send notification to all admin users when new comment is created
		/// </summary>
		private async Task SendCommentNotificationToAdmins(int productId, string productName, string userName, string commentContent)
		{
			try
			{
				// Get admin role
				var adminRole = await _roleManager.GetRoleByNameAsync("Admin");
				if (adminRole == null)
				{
					Logger.Warn("Admin role not found");
					return;
				}

				// Get all users in Admin role
				var adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name);
				
				if (adminUsers != null && adminUsers.Any())
				{
					// Create notification data
					var notificationData = new Abp.Notifications.NotificationData();
					notificationData["ProductId"] = productId.ToString();
					notificationData["ProductName"] = productName;
					notificationData["UserName"] = userName;
					notificationData["CommentContent"] = commentContent.Length > 50 
						? commentContent.Substring(0, 50) + "..." 
						: commentContent;
					notificationData["Message"] = $"{userName} đã bình luận về sản phẩm '{productName}'";
					notificationData["Url"] = $"/HomeCustomer/DetailProductCustomer?id={productId}#product-comments-section";
					
					// Publish notification to all admins
					var userIdentifiers = adminUsers.Select(u => new Abp.UserIdentifier(u.TenantId, u.Id)).ToArray();
					
					await _notificationPublisher.PublishAsync(
						notificationName: "App.NewProductComment",
						data: notificationData,
						severity: NotificationSeverity.Info,
						userIds: userIdentifiers
					);
					
					Logger.Info($"Sent comment notification to {adminUsers.Count()} admin(s)");
				}
			}
			catch (Exception ex)
			{
				// Log error but don't throw - notification failure shouldn't block comment creation
				Logger.Error("Failed to send comment notification to admins", ex);
			}
		}

		/// <summary>
		/// Lấy tất cả comments (có phân trang)
		/// </summary>
		public async Task<PagedResultDto<ProductCommentDto>> GetAllComments(GetAllProductCommentsInput input)
		{
			var query = _commentRepository.GetAll()
				.WhereIf(input.ProductId.HasValue, c => c.ProductId == input.ProductId.Value)
				.WhereIf(input.UserId.HasValue, c => c.UserId == input.UserId.Value)
				.WhereIf(input.IsApproved.HasValue, c => c.IsApproved == input.IsApproved.Value)
				.Where(c => c.ParentCommentId == null); // Chỉ lấy comments gốc

			var totalCount = await query.CountAsync();

			var comments = await query
				.OrderByDescending(c => c.CreationTime)
				.PageBy(input)
				.ToListAsync();

			var commentDtos = await MapCommentsToDto(comments);

			// Load replies cho mỗi comment
			foreach (var commentDto in commentDtos)
			{
				commentDto.Replies = await GetReplies(commentDto.Id);
			}

			return new PagedResultDto<ProductCommentDto>(totalCount, commentDtos);
		}

		/// <summary>
		/// Lấy comments dạng tree structure
		/// </summary>
		public async Task<List<ProductCommentDto>> GetProductCommentsTree(int productId)
		{
			var allComments = await _commentRepository.GetAll()
				.Where(c => c.ProductId == productId && c.IsApproved)
				.OrderBy(c => c.CreationTime)
				.ToListAsync();

			var commentDtos = await MapCommentsToDto(allComments);

			// Build tree structure
			var rootComments = commentDtos.Where(c => c.ParentCommentId == null).ToList();

			foreach (var root in rootComments)
			{
				BuildCommentTree(root, commentDtos);
			}

			return rootComments;
		}

		private void BuildCommentTree(ProductCommentDto parent, List<ProductCommentDto> allComments)
		{
			var children = allComments.Where(c => c.ParentCommentId == parent.Id).ToList();
			parent.Replies = children;

			foreach (var child in children)
			{
				BuildCommentTree(child, allComments);
			}
		}

		private async Task<List<ProductCommentDto>> GetReplies(int parentCommentId)
		{
			var replies = await _commentRepository.GetAll()
				.Where(c => c.ParentCommentId == parentCommentId && c.IsApproved)
				.OrderBy(c => c.CreationTime)
				.ToListAsync();

			return await MapCommentsToDto(replies);
		}

		/// <summary>
		/// Map comments to DTO và load thông tin User thủ công
		/// </summary>
		private async Task<List<ProductCommentDto>> MapCommentsToDto(List<ProductComment> comments)
		{
			var commentDtos = new List<ProductCommentDto>();
			
			// Lấy tất cả UserIds unique
			var userIds = comments.Select(c => c.UserId).Distinct().ToList();
			
			// Load tất cả Users cùng lúc (batch loading để tối ưu performance)
			var users = await _userRepository.GetAll()
				.Where(u => userIds.Contains(u.Id))
				.Select(u => new { u.Id, u.UserName, u.Name, u.Surname, u.EmailAddress })
				.ToListAsync();
			
			var userDict = users.ToDictionary(u => u.Id);

			foreach (var comment in comments)
			{
				var dto = ObjectMapper.Map<ProductCommentDto>(comment);
				
				// Gán thông tin User từ dictionary
				if (userDict.TryGetValue(comment.UserId, out var user))
				{
					dto.UserName = user.UserName;
					dto.UserFullName = $"{user.Name} {user.Surname}".Trim();
					dto.UserEmail = user.EmailAddress;
				}
				
				commentDtos.Add(dto);
			}

			return commentDtos;
		}

		/// <summary>
		/// Cập nhật comment
		/// </summary>
		public async Task<ProductCommentDto> UpdateComment(UpdateProductCommentDto input)
		{
			var comment = await _commentRepository.GetAsync(input.Id);

			// Kiểm tra quyền: chỉ người tạo mới được sửa
			if (comment.UserId != AbpSession.UserId)
			{
				throw new UserFriendlyException("Bạn không có quyền sửa comment này");
			}

			if (string.IsNullOrWhiteSpace(input.Content))
			{
				throw new UserFriendlyException("Nội dung comment không được để trống");
			}

			comment.Content = input.Content.Trim();
			comment.IsEdited = true;
			comment.EditedTime = DateTime.Now;

			await _commentRepository.UpdateAsync(comment);
			await CurrentUnitOfWork.SaveChangesAsync();

			// Load lại để map sang DTO
			var updatedComment = await _commentRepository.GetAsync(comment.Id);
			var result = await MapCommentsToDto(new List<ProductComment> { updatedComment });

			return result.FirstOrDefault();
		}

		/// <summary>
		/// Xóa comment
		/// </summary>
		public async Task DeleteComment(int id)
		{
			var comment = await _commentRepository.GetAsync(id);

			// Kiểm tra quyền: người tạo hoặc admin mới được xóa
			var isAdmin = await PermissionChecker.IsGrantedAsync(PermissionNames.Pages_Roles);
			if (comment.UserId != AbpSession.UserId && !isAdmin)
			{
				throw new UserFriendlyException("Bạn không có quyền xóa comment này");
			}

			// Xóa tất cả replies trước (cascade delete thủ công)
			var replies = await _commentRepository.GetAll()
				.Where(c => c.ParentCommentId == id)
				.ToListAsync();

			foreach (var reply in replies)
			{
				await _commentRepository.DeleteAsync(reply);
			}

			// Xóa comment chính
			await _commentRepository.DeleteAsync(comment);
		}

		/// <summary>
		/// Approve/Reject comment (Admin only)
		/// </summary>
		[AbpAuthorize(PermissionNames.Pages_Roles)]
		public async Task ApproveComment(int id, bool isApproved)
		{
			var comment = await _commentRepository.GetAsync(id);
			comment.IsApproved = isApproved;
			await _commentRepository.UpdateAsync(comment);
		}
	}
}
