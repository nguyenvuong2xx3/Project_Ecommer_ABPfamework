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
	public class ProductCommentAppService : ApplicationService, IProductCommentAppService
	{
		private readonly IRepository<ProductComment, int> _commentRepository;
		private readonly IRepository<User, long> _userRepository;
		private readonly IRepository<Product, int> _productRepository;
		private readonly IRepository<ProductVariant, int> _productVariantRepository;
		private readonly INotificationPublisher _notificationPublisher;
		private readonly UserManager _userManager;
		private readonly RoleManager _roleManager;
		private readonly IProductCommentBroadcaster _commentBroadcaster;

		public ProductCommentAppService(
			IRepository<ProductComment, int> commentRepository,
			IRepository<ProductVariant, int> productVariantRepository,
			IRepository<User, long> userRepository,
			IRepository<Product, int> productRepository,
			INotificationPublisher notificationPublisher,
			UserManager userManager,
			RoleManager roleManager,
			IProductCommentBroadcaster commentBroadcaster)
		{
			_productVariantRepository = productVariantRepository;
			_commentRepository = commentRepository;
			_userRepository = userRepository;
			_productRepository = productRepository;
			_notificationPublisher = notificationPublisher;
			_userManager = userManager;
			_roleManager = roleManager;
			_commentBroadcaster = commentBroadcaster;
		}

		/// <summary>
		/// Tạo comment mới
		/// </summary>
		[AbpAuthorize] // Yêu cầu đăng nhập để comment
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
			var productVariant = await _productVariantRepository.GetAsync(input.ProductVariantId);
			if (productVariant == null)
			{
				throw new UserFriendlyException("Sản phẩm không tồn tại");
			}
			var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == productVariant.ProductId);

			// Validate ParentCommentId nếu có
			ProductComment parentComment = null;
			if (input.ParentCommentId.HasValue)
			{
				parentComment = await _commentRepository.FirstOrDefaultAsync(c => c.Id == input.ParentCommentId.Value);

				if (parentComment == null)
				{
					throw new UserFriendlyException("Comment cha không tồn tại");
				}
			}

			var comment = new ProductComment
			{
				UserId = currentUserId.Value,
				ProductVariantId = input.ProductVariantId,
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

			// Check if current user is admin
			var isCurrentUserAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

			// ✅ Cross notifications
			if (parentComment != null)
			{
				// Đây là reply → Gửi thông báo cho người được reply
				await SendReplyNotification(
					recipientUserId: parentComment.UserId,
					replierName: userName,
					productVariantId: productVariant.Id,
					productName: product.Name + " " + productVariant.Ram + " " + productVariant.Color + " " + productVariant.Storage,
					commentContent: input.Content,
					commentId: comment.Id
				);
			}
			else if (!isCurrentUserAdmin)
			{
				// User bình thường comment → Gửi thông báo cho admin
				await SendCommentNotificationToAdmins(
					productVariantId: productVariant.Id,
					productName: product.Name + " " + productVariant.Ram + " " + productVariant.Color + " " + productVariant.Storage,
					userName: userName,
					commentContent: input.Content
				);
			}
			// Nếu admin comment gốc thì không gửi notification

			// Load lại để map sang DTO
			var createdComment = await _commentRepository.GetAsync(comment.Id);
			var result = await MapCommentsToDto(new List<ProductComment> { createdComment });
			var commentDto = result.FirstOrDefault();

			// ✅ Broadcast qua SignalR để real-time - SỬA: dùng productVariant.Id thay vì product.Id
			await BroadcastNewComment(productVariant.Id, commentDto);

			return commentDto;
		}

		/// <summary>
		/// ✅ Broadcast new comment via SignalR
		/// </summary>
		private async Task BroadcastNewComment(int productVariantId, ProductCommentDto commentDto)
		{
			try
			{
				await _commentBroadcaster.BroadcastNewComment(productVariantId, commentDto);
				Logger.Info($"Broadcasted new comment {commentDto.Id} to product variant {productVariantId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to broadcast comment via SignalR", ex);
			}
		}

		/// <summary>
		/// ✅ Send notification to specific user when someone replies to their comment
		/// </summary>
		private async Task SendReplyNotification(long recipientUserId, string replierName, int productVariantId, string productName, string commentContent, int commentId)
		{
			try
			{
				// Don't send notification if replying to own comment
				if (recipientUserId == AbpSession.UserId)
				{
					return;
				}

				var notificationData = new Abp.Notifications.NotificationData();
				notificationData["productVariantId"] = productVariantId.ToString();
				notificationData["ProductName"] = productName;
				notificationData["ReplierName"] = replierName;
				notificationData["CommentContent"] = commentContent.Length > 50
					? commentContent.Substring(0, 50) + "..."
					: commentContent;
				notificationData["Message"] = $"{replierName} đã trả lời bình luận của bạn về sản phẩm '{productName}'";
				notificationData["Url"] = $"/HomeCustomer/DetailProductCustomer?id={productVariantId}#comment-{commentId}";

				// Get recipient user info
				var recipient = await _userRepository.GetAsync(recipientUserId);

				await _notificationPublisher.PublishAsync(
					notificationName: "App.CommentReply",
					data: notificationData,
					severity: NotificationSeverity.Info,
					userIds: new[] { new Abp.UserIdentifier(recipient.TenantId, recipient.Id) }
				);

				Logger.Info($"Sent reply notification to user {recipientUserId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to send reply notification", ex);
			}
		}

		/// <summary>
		/// Send notification to all admin users when new comment is created
		/// </summary>
		private async Task SendCommentNotificationToAdmins(int productVariantId, string productName, string userName, string commentContent)
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
					notificationData["productVariantId"] = productVariantId.ToString();
					notificationData["ProductName"] = productName;
					notificationData["UserName"] = userName;
					notificationData["CommentContent"] = commentContent.Length > 50
						? commentContent.Substring(0, 50) + "..."
						: commentContent;
					notificationData["Message"] = $"{userName} đã bình luận về sản phẩm '{productName}'";
					notificationData["Url"] = $"/HomeCustomer/DetailProductCustomer?id={productVariantId}";

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
				.WhereIf(input.ProductVariantId.HasValue, c => c.ProductVariantId == input.ProductVariantId.Value)
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
		public async Task<List<ProductCommentDto>> GetProductVariantCommentsTree(int productVariantId)
		{
			var allComments = await _commentRepository.GetAll()
				.Where(c => c.ProductVariantId == productVariantId && c.IsApproved)
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
		[AbpAuthorize] // ✅ Yêu cầu đăng nhập để sửa comment
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
			var commentDto = result.FirstOrDefault();

			// ✅ Broadcast update via SignalR
			await BroadcastCommentUpdate(comment.ProductVariantId, commentDto);

			return commentDto;
		}

		/// <summary>
		/// ✅ Broadcast comment update via SignalR
		/// </summary>
		private async Task BroadcastCommentUpdate(int productVariantId, ProductCommentDto commentDto)
		{
			try
			{
				await _commentBroadcaster.BroadcastCommentUpdate(productVariantId, commentDto.Id, commentDto);
				Logger.Info($"Broadcasted comment update {commentDto.Id} to product {productVariantId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to broadcast comment update via SignalR", ex);
			}
		}

		/// <summary>
		/// Xóa comment
		/// </summary>
		[AbpAuthorize] // ✅ Yêu cầu đăng nhập để xóa comment
		public async Task DeleteComment(int id)
		{
			var comment = await _commentRepository.GetAsync(id);
			var productVariantId = comment.ProductVariantId;

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

			// ✅ Broadcast delete via SignalR
			await BroadcastCommentDelete(productVariantId, id);
		}

		/// <summary>
		/// ✅ Broadcast comment deletion via SignalR
		/// </summary>
		private async Task BroadcastCommentDelete(int productVariantId, int commentId)
		{
			try
			{
				await _commentBroadcaster.BroadcastCommentDelete(productVariantId, commentId);
				Logger.Info($"Broadcasted comment deletion {commentId} from product {productVariantId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to broadcast comment deletion via SignalR", ex);
			}
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
