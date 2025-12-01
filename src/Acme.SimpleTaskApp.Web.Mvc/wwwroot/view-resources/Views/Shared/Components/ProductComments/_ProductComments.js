(function ($) {
	var _commentService = abp.services.app.productComment;
	var _productId = $('#ProductId').val();
	var _replyToCommentId = null;
	var _replyToUserName = '';
	var _commentHubConnection = null;

	$(document).ready(function () {
		initializeCommentForm();
		initializeCommentActions();
		// ✅ NEW: Initialize SignalR
		initializeSignalR();
	});

	// ✅ NEW: Initialize SignalR connection
	function initializeSignalR() {
		try {
			// Create SignalR connection
			_commentHubConnection = new signalR.HubConnectionBuilder()
				.withUrl("/signalr-productCommentHub")
				.withAutomaticReconnect()
				.build();

			// Handle new comment received
			_commentHubConnection.on("ReceiveComment", function (data) {
				console.log('[SignalR] Received new comment:', data);
				handleNewCommentReceived(data.comment);
			});

			// Handle comment updated
			_commentHubConnection.on("CommentUpdated", function (data) {
				console.log('[SignalR] Comment updated:', data);
				handleCommentUpdated(data.commentId, data.comment);
			});

			// Handle comment deleted
			_commentHubConnection.on("CommentDeleted", function (data) {
				console.log('[SignalR] Comment deleted:', data);
				handleCommentDeleted(data.commentId);
			});

			// Handle user typing (optional)
			_commentHubConnection.on("UserTyping", function (userName) {
				showUserTyping(userName);
			});

			_commentHubConnection.on("UserStoppedTyping", function (userName) {
				hideUserTyping(userName);
			});

			// Start connection and join product group
			_commentHubConnection.start()
				.then(function () {
					console.log('[SignalR] Connected successfully');
					// Join product comment group
					return _commentHubConnection.invoke("JoinProductGroup", parseInt(_productId));
				})
				.then(function () {
					console.log('[SignalR] Joined product comment group:', _productId);
				})
				.catch(function (err) {
					console.error('[SignalR] Connection error:', err);
				});

			// Handle reconnection
			_commentHubConnection.onreconnected(function () {
				console.log('[SignalR] Reconnected');
				_commentHubConnection.invoke("JoinProductGroup", parseInt(_productId));
			});

		} catch (error) {
			console.error('[SignalR] Initialization error:', error);
		}
	}

	// ✅ NEW: Handle new comment received via SignalR
	function handleNewCommentReceived(comment) {
		// Check if comment already exists (to avoid duplicates)
		if ($(`[data-comment-id="${comment.id}"]`).length > 0) {
			return;
		}

		// Render new comment
		renderComment(comment);

		// Update count
		updateCommentCount(1);

		// Show notification
		abp.notify.info('Có bình luận mới');
	}

	// ✅ NEW: Handle comment updated via SignalR
	function handleCommentUpdated(commentId, comment) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		if ($commentItem.length > 0) {
			// Update content
			$commentItem.find('.comment-text').first().text(comment.content);

			// Add edited label if not exists
			var $timeInfo = $commentItem.find('.text-muted small').first();
			if (!$timeInfo.find('.text-info').length) {
				$timeInfo.append(' <span class="text-info">(đã chỉnh sửa)</span>');
			}
		}
	}

	// ✅ NEW: Handle comment deleted via SignalR
	function handleCommentDeleted(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		if ($commentItem.length > 0) {
			$commentItem.fadeOut(300, function () {
				$(this).remove();
				updateCommentCount(-1);
			});
		}
	}

	// ✅ NEW: Render new comment to DOM
	function renderComment(comment) {
		// Build comment HTML
		var html = buildCommentHtml(comment);

		if (comment.parentCommentId) {
			// This is a reply
			var $parentComment = $(`.comment-item[data-comment-id="${comment.parentCommentId}"]`);
			var $repliesContainer = $parentComment.find('.replies').first();

			if ($repliesContainer.length === 0) {
				$repliesContainer = $('<div class="replies mt-2"></div>');
				$parentComment.append($repliesContainer);
			}

			$repliesContainer.append(html);
		} else {
			// Root comment
			$('#comments-list').prepend(html);
		}

		// Animate in
		var $newComment = $(`.comment-item[data-comment-id="${comment.id}"]`);
		$newComment.hide().fadeIn(500);
	}

	// ✅ NEW: Build comment HTML
	function buildCommentHtml(comment) {
		var isOwner = abp.session.userId && comment.userId == abp.session.userId;
		var editedLabel = comment.isEdited ? '<span class="text-info">(đã chỉnh sửa)</span>' : '';

		var html = `
            <div class="comment-item mb-3 ${comment.parentCommentId ? 'ms-5' : ''}" data-comment-id="${comment.id}">
                <div class="card">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <div class="d-flex align-items-center">
                                <div class="avatar me-2">
                                    <div class="avatar-circle bg-primary text-white">
                                        ${comment.userFullName.substring(0, 1).toUpperCase()}
                                    </div>
                                </div>
                                <div>
                                    <h6 class="mb-0 fw-bold">${comment.userFullName}</h6>
                                    <small class="text-muted">
                                        <i class="fas fa-clock"></i> 
                                        ${formatCommentTime(comment.creationTime)}
                                        ${editedLabel}
                                    </small>
                                </div>
                            </div>
        `;

		if (isOwner) {
			html += `
                            <div class="dropdown">
                                <button class="btn btn-sm btn-link text-muted" type="button" data-bs-toggle="dropdown">
                                    <i class="fas fa-ellipsis-v"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li><a class="dropdown-item btn-edit-comment" href="javascript:void(0)" data-comment-id="${comment.id}">
                                        <i class="fas fa-edit text-primary"></i> Sửa
                                    </a></li>
                                    <li><a class="dropdown-item btn-delete-comment" href="javascript:void(0)" data-comment-id="${comment.id}">
                                        <i class="fas fa-trash text-danger"></i> Xóa
                                    </a></li>
                                </ul>
                            </div>
            `;
		}

		html += `
                        </div>
                        <div class="comment-content" data-comment-id="${comment.id}">
                            <p class="mb-2 comment-text">${comment.content}</p>
                        </div>
                        <div class="comment-edit-form" style="display: none;">
                            <textarea class="form-control mb-2" rows="3" maxlength="1000">${comment.content}</textarea>
                            <div class="d-flex gap-2">
                                <button class="btn btn-sm btn-primary btn-save-edit" data-comment-id="${comment.id}">
                                    <i class="fas fa-save"></i> Lưu
                                </button>
                                <button class="btn btn-sm btn-secondary btn-cancel-edit">
                                    <i class="fas fa-times"></i> Hủy
                                </button>
                            </div>
                        </div>
        `;

		if (abp.session.userId) {
			html += `
                        <div class="comment-actions mt-2">
                            <button class="btn btn-sm btn-link text-primary btn-reply" 
                                    data-comment-id="${comment.id}"
                                    data-user-name="${comment.userFullName}">
                                <i class="fas fa-reply"></i> Trả lời
                            </button>
                        </div>
            `;
		}

		html += `
                    </div>
                </div>
            </div>
        `;

		return html;
	}

	// ✅ NEW: Format comment time
	function formatCommentTime(dateString) {
		var date = new Date(dateString);
		return date.toLocaleString('vi-VN', {
			year: 'numeric',
			month: '2-digit',
			day: '2-digit',
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	// ✅ NEW: Update comment count
	function updateCommentCount(delta) {
		var $badge = $('#product-comments-section h4 .badge');
		var currentCount = parseInt($badge.text()) || 0;
		$badge.text(currentCount + delta);
	}

	// ✅ NEW: Show user typing indicator
	function showUserTyping(userName) {
		var $indicator = $('#typing-indicator');
		if ($indicator.length === 0) {
			$indicator = $('<div id="typing-indicator" class="text-muted small mb-2"><i class="fas fa-circle-notch fa-spin"></i> <span class="typing-user"></span> đang nhập...</div>');
			$('#commentForm').before($indicator);
		}
		$indicator.find('.typing-user').text(userName);
		$indicator.show();
	}

	// ✅ NEW: Hide user typing indicator
	function hideUserTyping(userName) {
		var $indicator = $('#typing-indicator');
		if ($indicator.find('.typing-user').text() === userName) {
			$indicator.hide();
		}
	}

	// Initialize comment form
	function initializeCommentForm() {
		// Character counter
		$('#CommentContent').on('input', function () {
			var length = $(this).val().length;
			$('#charCount').text(length);

			// ✅ Optional: Send typing indicator via SignalR
			if (_commentHubConnection && _commentHubConnection.state === signalR.HubConnectionState.Connected) {
				_commentHubConnection.invoke("UserTyping", parseInt(_productId), abp.session.userName);
			}
		});

		// Submit comment
		$('#submitComment').on('click', function () {
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

				// ✅ SignalR will handle displaying the comment, no need to reload
				// The comment will appear via SignalR broadcast
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
				hideEditForm(commentId);
				// ✅ SignalR will handle updating for other users
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
							// ✅ SignalR will handle removing for other users
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

	// Cleanup on page unload
	$(window).on('beforeunload', function () {
		if (_commentHubConnection) {
			_commentHubConnection.invoke("LeaveProductGroup", parseInt(_productId));
			_commentHubConnection.stop();
		}
	});

})(jQuery);
