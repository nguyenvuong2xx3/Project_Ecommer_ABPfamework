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

			// Show edited badge if not exists
			var $commentTime = $commentItem.find('.comment-time');
			if (!$commentTime.find('.edited-badge').length) {
				$commentTime.append('<span class="edited-badge">• đã sửa</span>');
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
		// Check if comment already exists (to avoid duplicates)
		if ($(`[data-comment-id="${comment.id}"]`).length > 0) {
			return;
		}

		// Build comment HTML
		var html = buildCommentHtml(comment);

		if (comment.parentCommentId) {
			// This is a reply
			var $parentComment = $(`.comment-item[data-comment-id="${comment.parentCommentId}"]`);
			var $repliesContainer = $parentComment.next('.replies-container');

			// If replies container doesn't exist, create one
			if ($repliesContainer.length === 0) {
				$repliesContainer = $('<div class="replies-container"></div>');
				$parentComment.after($repliesContainer);
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
		var isEdited = comment.isEdited;
		var isReply = comment.parentCommentId != null;

		var html = `
        <div class="comment-item ${isReply ? 'is-reply' : ''}" data-comment-id="${comment.id}">
            <div class="d-flex"></div>
            <div class="comment-body">
                <div class="comment-header">
                    <div class="comment-avatar">
                        ${comment.userFullName ? comment.userFullName.substring(0, 1).toUpperCase() : ''}
                    </div>
                    <span class="comment-author">${comment.userFullName}</span>
                    <span class="comment-time">
                        ${formatCommentTime(comment.creationTime)}
                        ${isEdited ? '<span class="edited-badge">• đã sửa</span>' : ''}
                    </span>

        `;

		if (isOwner) {
			html += `
                    <div class="comment-menu">
                        <button class="btn-edit-comment" type="button" data-comment-id="${comment.id}">
                            <i class="fas fa-pen"></i>
                        </button>
                        <button class="btn-delete-comment" type="button" data-comment-id="${comment.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
        `;
		}

		html += `
                </div>

                <!-- Content -->
                <div class="comment-content" data-comment-id="${comment.id}">
                    <p class="comment-text">${comment.content}</p>
                </div>

                <!-- Edit Form -->
                <div class="comment-edit-form" style="display: none;">
                    <textarea class="edit-textarea" rows="2" maxlength="1000">${comment.content}</textarea>
                    <div class="edit-actions">
                        <button class="btn-save-edit" data-comment-id="${comment.id}">Lưu</button>
                        <button class="btn-cancel-edit">Hủy</button>
                    </div>
                </div>

                <!-- Actions -->
        `;

		if (abp.session.userId) {
			html += `
                <div class="comment-actions">
                    <button class="btn-reply" data-comment-id="${comment.id}" data-user-name="${comment.userFullName}">
                        <i class="fas fa-reply"></i> Trả lời
                    </button>
                </div>
        `;
		}

		html += `
            </div>
        </div>
    `;

		return html;
	}

	// ✅ NEW: Format comment time
	function formatCommentTime(dateString) {
		var date = new Date(dateString);
		var day = ('0' + date.getDate()).slice(-2);
		var month = ('0' + (date.getMonth() + 1)).slice(-2);
		var year = date.getFullYear();
		var hours = ('0' + date.getHours()).slice(-2);
		var minutes = ('0' + date.getMinutes()).slice(-2);
		return day + '/' + month + '/' + year + ' ' + hours + ':' + minutes;
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
