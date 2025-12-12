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

		// Tạo comment mới (yêu cầu đăng nhập)
		[AbpAuthorize]
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

			// Lấy thông tin biến thể sản phẩm
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

			// Lấy thông tin user hiện tại
			var currentUser = await _userRepository.GetAsync(currentUserId.Value);
			var userName = $"{currentUser.Name} {currentUser.Surname}".Trim();

			// Kiểm tra user hiện tại có phải admin không
			var isCurrentUserAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

			// Gửi thông báo chéo
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
				// User thường comment → Gửi thông báo cho admin
				await SendCommentNotificationToAdmins(
					productVariantId: productVariant.Id,
					productName: product.Name + " " + productVariant.Ram + " " + productVariant.Color + " " + productVariant.Storage,
					userName: userName,
					commentContent: input.Content
				);
			}

			// Load lại để map sang DTO
			var createdComment = await _commentRepository.GetAsync(comment.Id);
			var result = await MapCommentsToDto(new List<ProductComment> { createdComment });
			var commentDto = result.FirstOrDefault();

			// Phát qua SignalR để realtime - SỬA: dùng productVariant.Id thay vì product.Id
			await BroadcastNewComment(productVariant.Id, commentDto);

			return commentDto;
		}

		// Phát comment mới qua SignalR
		private async Task BroadcastNewComment(int productVariantId, ProductCommentDto commentDto)
		{
			try
			{
				await _commentBroadcaster.BroadcastNewComment(productVariantId, commentDto);
				Logger.Info($"Đã phát bình luận mới {commentDto.Id} cho biến thể sản phẩm {productVariantId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Phát bình luận qua SignalR thất bại", ex);
			}
		}

		// Gửi notification khi có reply tới người được reply
		private async Task SendReplyNotification(long recipientUserId, string replierName, int productVariantId, string productName, string commentContent, int commentId)
		{
			try
			{
				// Không gửi thông báo nếu reply cho chính mình
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

				// Lấy thông tin recipient
				var recipient = await _userRepository.GetAsync(recipientUserId);

				await _notificationPublisher.PublishAsync(
					notificationName: "App.CommentReply",
					data: notificationData,
					severity: NotificationSeverity.Info,
					userIds: new[] { new Abp.UserIdentifier(recipient.TenantId, recipient.Id) }
				);

				Logger.Info($"Đã gửi thông báo trả lời đến user {recipientUserId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Gửi thông báo trả lời thất bại", ex);
			}
		}

		// Gửi notification cho tất cả admin khi có comment mới
		private async Task SendCommentNotificationToAdmins(int productVariantId, string productName, string userName, string commentContent)
		{
			try
			{
				// Lấy role admin
				var adminRole = await _roleManager.GetRoleByNameAsync("Admin");
				if (adminRole == null)
				{
					Logger.Warn("Không tìm thấy role Admin");
					return;
				}

				// Lấy tất cả user trong role Admin
				var adminUsers = await _userManager.GetUsersInRoleAsync(adminRole.Name);

				if (adminUsers != null && adminUsers.Any())
				{
					// Tạo dữ liệu notification
					var notificationData = new Abp.Notifications.NotificationData();
					notificationData["productVariantId"] = productVariantId.ToString();
					notificationData["ProductName"] = productName;
					notificationData["UserName"] = userName;
					notificationData["CommentContent"] = commentContent.Length > 50
						? commentContent.Substring(0, 50) + "..."
						: commentContent;
					notificationData["Message"] = $"{userName} đã bình luận về sản phẩm '{productName}'";
					notificationData["Url"] = $"/HomeCustomer/DetailProductCustomer?id={productVariantId}";

					// Gửi notification tới tất cả admin
					var userIdentifiers = adminUsers.Select(u => new Abp.UserIdentifier(u.TenantId, u.Id)).ToArray();

					await _notificationPublisher.PublishAsync(
						notificationName: "App.NewProductComment",
						data: notificationData,
						severity: NotificationSeverity.Info,
						userIds: userIdentifiers
					);

					Logger.Info($"Đã gửi thông báo comment tới {adminUsers.Count()} admin");
				}
			}
			catch (Exception ex)
			{
				// Ghi log lỗi nhưng không ném ra ngoài - lỗi notification không làm ảnh hưởng đến việc tạo comment
				Logger.Error("Gửi thông báo comment tới admin thất bại", ex);
			}
		}

		// Lấy tất cả comments (có phân trang)
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

		// Lấy comments dạng tree structure
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

		// Map comments to DTO và load thông tin User thủ công
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

		// Cập nhật comment (yêu cầu đăng nhập)
		[AbpAuthorize]
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

			// Phát update qua SignalR
			await BroadcastCommentUpdate(comment.ProductVariantId, commentDto);

			return commentDto;
		}

		// Phát update qua SignalR
		private async Task BroadcastCommentUpdate(int productVariantId, ProductCommentDto commentDto)
		{
			try
			{
				await _commentBroadcaster.BroadcastCommentUpdate(productVariantId, commentDto.Id, commentDto);
				Logger.Info($"Đã phát cập nhật comment {commentDto.Id} tới sản phẩm {productVariantId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Không thể phát cập nhật comment qua SignalR", ex);
			}
		}

		// Xóa comment
		[AbpAuthorize] // Yêu cầu đăng nhập để xóa comment
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

			// Phát xóa qua SignalR
			await BroadcastCommentDelete(productVariantId, id);
		}

		// Phát xóa qua SignalR
		private async Task BroadcastCommentDelete(int productVariantId, int commentId)
		{
			try
			{
				await _commentBroadcaster.BroadcastCommentDelete(productVariantId, commentId);
				Logger.Info($"Đã phát xóa comment {commentId} khỏi sản phẩm {productVariantId}");
			}
			catch (Exception ex)
			{
				Logger.Error("Không thể phát xóa comment qua SignalR", ex);
			}
		}

		// duyệt hoặc từ chối comment - admin
		[AbpAuthorize(PermissionNames.Pages_Roles)]
		public async Task ApproveComment(int id, bool isApproved)
		{
			var comment = await _commentRepository.GetAsync(id);
			comment.IsApproved = isApproved;
			await _commentRepository.UpdateAsync(comment);
		}
	}
}
