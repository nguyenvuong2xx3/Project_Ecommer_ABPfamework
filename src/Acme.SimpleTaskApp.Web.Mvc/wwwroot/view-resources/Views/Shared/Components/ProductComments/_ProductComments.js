(function ($) {
	var _commentService = abp.services.app.productComment;
	var _productId = $('#ProductId').val();
	var _replyToCommentId = null;
	var _replyToUserName = '';

	$(document).ready(function () {
		initializeCommentForm();
		initializeCommentActions();
	});

	// Initialize comment form
	function initializeCommentForm() {
		// Character counter
		$('#CommentContent').on('input', function () {
			var length = $(this).val().length;
			$('#charCount').text(length);
		});

		// Submit comment
		$('#commentForm').on('submit', function (e) {
			e.preventDefault();
			submitComment();
		});

		// Cancel reply
		$('#btnCancelReply').on('click', function () {
			cancelReply();
		});
	}

	// Initialize comment actions (reply, edit, delete)
	function initializeCommentActions() {
		// Reply button
		$(document).on('click', '.btn-reply', function () {
			var commentId = $(this).data('comment-id');
			var userName = $(this).data('user-name');
			setReplyTo(commentId, userName);
		});

		// Edit button
		$(document).on('click', '.btn-edit-comment', function () {
			var commentId = $(this).data('comment-id');
			showEditForm(commentId);
		});

		// Save edit
		$(document).on('click', '.btn-save-edit', function () {
			var commentId = $(this).data('comment-id');
			saveEditComment(commentId);
		});

		// Cancel edit
		$(document).on('click', '.btn-cancel-edit', function () {
			var commentId = $(this).closest('.comment-item').data('comment-id');
			hideEditForm(commentId);
		});

		// Delete button
		$(document).on('click', '.btn-delete-comment', function () {
			var commentId = $(this).data('comment-id');
			deleteComment(commentId);
		});
	}

	// Submit comment
	function submitComment() {
		var content = $('#CommentContent').val().trim();

		if (!content) {
			abp.notify.warn('Vui lòng nhập nội dung bình luận');
			return;
		}

		if (content.length > 1000) {
			abp.notify.warn('Bình luận không được vượt quá 1000 ký tự');
			return;
		}

		var input = {
			productId: parseInt(_productId),
			content: content,
			parentCommentId: _replyToCommentId
		};

		abp.ui.setBusy($('#commentForm'));

		_commentService.createComment(input)
			.done(function (result) {
				abp.notify.success('Đã gửi bình luận thành công!');
				$('#CommentContent').val('');
				$('#charCount').text('0');
				cancelReply();
				
				// ✅ FIX: Reload only comment section via AJAX
				reloadCommentsAjax();
			})
			.fail(function (error) {
				abp.notify.error(error.message || 'Có lỗi xảy ra khi gửi bình luận');
			})
			.always(function () {
				abp.ui.clearBusy($('#commentForm'));
			});
	}

	// Set reply to comment
	function setReplyTo(commentId, userName) {
		_replyToCommentId = commentId;
		_replyToUserName = userName;
		
		$('#ParentCommentId').val(commentId);
		$('#CommentContent').attr('placeholder', `Trả lời ${userName}...`);
		$('#CommentContent').focus();
		$('#btnCancelReply').show();
		
		// Scroll to form
		$('html, body').animate({
			scrollTop: $('#commentForm').offset().top - 100
		}, 500);
	}

	// Cancel reply
	function cancelReply() {
		_replyToCommentId = null;
		_replyToUserName = '';
		
		$('#ParentCommentId').val('');
		$('#CommentContent').attr('placeholder', 'Nhập bình luận của bạn...');
		$('#btnCancelReply').hide();
	}

	// Show edit form
	function showEditForm(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		var $content = $commentItem.find('.comment-content').first();
		var $editForm = $commentItem.find('.comment-edit-form').first();
		
		$content.hide();
		$editForm.show();
		$editForm.find('textarea').focus();
	}

	// Hide edit form
	function hideEditForm(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		var $content = $commentItem.find('.comment-content').first();
		var $editForm = $commentItem.find('.comment-edit-form').first();
		
		$editForm.hide();
		$content.show();
	}

	// Save edit comment
	function saveEditComment(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		var $editForm = $commentItem.find('.comment-edit-form').first();
		var newContent = $editForm.find('textarea').val().trim();

		if (!newContent) {
			abp.notify.warn('Nội dung bình luận không được để trống');
			return;
		}

		if (newContent.length > 1000) {
			abp.notify.warn('Bình luận không được vượt quá 1000 ký tự');
			return;
		}

		var input = {
			id: commentId,
			content: newContent
		};

		abp.ui.setBusy($editForm);

		_commentService.updateComment(input)
			.done(function (result) {
				abp.notify.success('Đã cập nhật bình luận');
				
				// Update comment text
				$commentItem.find('.comment-text').first().text(newContent);
				
				// Add edited label if not exists
				var $timeInfo = $commentItem.find('.text-muted small').first();
				if (!$timeInfo.find('.text-info').length) {
					$timeInfo.append(' <span class="text-info">(đã chỉnh sửa)</span>');
				}
				
				hideEditForm(commentId);
			})
			.fail(function (error) {
				abp.notify.error(error.message || 'Có lỗi khi cập nhật bình luận');
			})
			.always(function () {
				abp.ui.clearBusy($editForm);
			});
	}

	// Delete comment
	function deleteComment(commentId) {
		abp.message.confirm(
			'Bạn có chắc chắn muốn xóa bình luận này?',
			'Xác nhận xóa',
			function (isConfirmed) {
				if (isConfirmed) {
					abp.ui.setBusy($('#comments-list'));

					_commentService.deleteComment(commentId)
						.done(function () {
							abp.notify.success('Đã xóa bình luận');
							
							// ✅ FIX: Reload only comment section via AJAX
							reloadCommentsAjax();
						})
						.fail(function (error) {
							abp.notify.error(error.message || 'Có lỗi khi xóa bình luận');
						})
						.always(function () {
							abp.ui.clearBusy($('#comments-list'));
						});
				}
			}
		);
	}

	// ✅ NEW: Reload comments via AJAX (không reload page)
	function reloadCommentsAjax() {
		$('#comments-loading').show();
		$('#comments-list').hide();

		$.ajax({
			url: abp.appPath + 'HomeCustomer/LoadCommentsPartial',
			type: 'GET',
			data: { productId: _productId },
			success: function (html) {
				$('#comments-list').html(html);
				
				// Update comment count
				var commentCount = $('#comments-list .comment-item').length;
				$('#product-comments-section h4 .badge').text(commentCount);
				
				$('#comments-loading').hide();
				$('#comments-list').show();
				
				// Scroll to comments section smoothly
				$('html, body').animate({
					scrollTop: $('#product-comments-section').offset().top - 100
				}, 500);
			},
			error: function () {
				abp.notify.error('Không thể tải danh sách bình luận');
				$('#comments-loading').hide();
				$('#comments-list').show();
			}
		});
	}

	// Count total comments (including replies) - helper function
	function countComments(comments) {
		var count = comments.length;
		comments.forEach(function (comment) {
			if (comment.replies && comment.replies.length > 0) {
				count += countComments(comment.replies);
			}
		});
		return count;
	}

})(jQuery);
