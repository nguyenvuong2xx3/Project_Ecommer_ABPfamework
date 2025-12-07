(function ($) {
	var _commentService = abp.services.app.productComment;
	var _productId = $('#ProductId').val();
	var _replyToCommentId = null;
	var _replyToUserName = '';
	var _commentHubConnection = null;
	
	// ✅ NEW: Pagination variables
	var _allComments = [];
	var _displayedCount = 10;
	var _pageSize = 10;

	$(document).ready(function () {
		initializeCommentForm();
		initializeCommentActions();
		initializeSignalR();
		
		// ✅ NEW: Initialize pagination
		initializePagination();
	});

	// ✅ NEW: Initialize pagination system
	function initializePagination() {
		// Load all comments data from hidden script tag
		try {
			var commentsJson = $('#allCommentsData').text();
			_allComments = JSON.parse(commentsJson) || [];
			
			console.log('[Pagination] Loaded', _allComments.length, 'total comments');
			
			// Initialize Load More button
			$('#btnLoadMore').on('click', loadMoreComments);
			
			// Update displayed count
			updateDisplayedStats();
		} catch (error) {
			console.error('[Pagination] Error loading comments data:', error);
		}
	}

	// ✅ NEW: Load more comments
	function loadMoreComments() {
		var $btn = $('#btnLoadMore');
		var $container = $('#comments-list');
		
		// Show loading state
		$btn.prop('disabled', true);
		$btn.html('<i class="fas fa-spinner fa-spin"></i> Đang tải...');
		
		// Calculate next batch
		var nextBatch = _allComments.slice(_displayedCount, _displayedCount + _pageSize);
		
		if (nextBatch.length === 0) {
			$('#loadMoreContainer').hide();
			return;
		}
		
		// Render next batch
		setTimeout(function() {
			nextBatch.forEach(function(comment) {
				var html = buildCommentHtml(comment);
				$container.append(html);
				
				// Animate new comment
				var $newComment = $(`.comment-item[data-comment-id="${comment.id}"]`);
				$newComment.hide().fadeIn(300);
			});
			
			// Update counter
			_displayedCount += nextBatch.length;
			updateDisplayedStats();
			
			// Check if there are more comments
			if (_displayedCount >= _allComments.length) {
				$('#loadMoreContainer').hide();
			} else {
				var remaining = _allComments.length - _displayedCount;
				$btn.html(`<i class="fas fa-chevron-down"></i> Xem thêm bình luận (${remaining})`);
				$btn.prop('disabled', false);
			}
		}, 300);
	}

	// ✅ NEW: Update displayed stats
	function updateDisplayedStats() {
		$('#displayedCount').text(_displayedCount);
		$('#totalCommentCount').text(_allComments.length);
	}

	// Initialize SignalR connection
	function initializeSignalR() {
		try {
			_commentHubConnection = new signalR.HubConnectionBuilder()
				.withUrl("/signalr-productCommentHub")
				.withAutomaticReconnect()
				.build();

			_commentHubConnection.on("ReceiveComment", function (data) {
				console.log('[SignalR] Received new comment:', data);
				handleNewCommentReceived(data.comment);
			});

			_commentHubConnection.on("CommentUpdated", function (data) {
				console.log('[SignalR] Comment updated:', data);
				handleCommentUpdated(data.commentId, data.comment);
			});

			_commentHubConnection.on("CommentDeleted", function (data) {
				console.log('[SignalR] Comment deleted:', data);
				handleCommentDeleted(data.commentId);
			});

			_commentHubConnection.on("UserTyping", function (userName) {
				showUserTyping(userName);
			});

			_commentHubConnection.on("UserStoppedTyping", function (userName) {
				hideUserTyping(userName);
			});

			_commentHubConnection.start()
				.then(function () {
					console.log('[SignalR] Connected successfully');
					return _commentHubConnection.invoke("JoinProductGroup", parseInt(_productId));
				})
				.then(function () {
					console.log('[SignalR] Joined product comment group:', _productId);
				})
				.catch(function (err) {
					console.error('[SignalR] Connection error:', err);
				});

			_commentHubConnection.onreconnected(function () {
				console.log('[SignalR] Reconnected');
				_commentHubConnection.invoke("JoinProductGroup", parseInt(_productId));
			});

		} catch (error) {
			console.error('[SignalR] Initialization error:', error);
		}
	}

	// ✅ IMPROVED: Handle new comment received via SignalR
	function handleNewCommentReceived(comment) {
		if ($(`[data-comment-id="${comment.id}"]`).length > 0) {
			return;
		}

		var isOwnComment = abp.session.userId && comment.userId == abp.session.userId;

		// ✅ Add to allComments array at the beginning (newest first)
		_allComments.unshift(comment);
		_displayedCount++;

		// Render new comment at the top
		renderComment(comment);

		// Update stats
		updateDisplayedStats();

		// Show stats bar if it was hidden
		if ($('.comments-stats').length === 0 && _allComments.length > 0) {
			var statsHtml = `
				<div class="comments-stats">
					<span class="stats-text">
						Hiển thị <strong id="displayedCount">${_displayedCount}</strong> / <strong>${_allComments.length}</strong> bình luận
					</span>
					<span class="stats-badge">Mới nhất</span>
				</div>
			`;
			$('#comments-list').before(statsHtml);
		}

		// Remove empty state if exists
		$('.empty-state').remove();

		if (!isOwnComment) {
			console.log('[Comment] New comment received from another user via SignalR');
		} else {
			console.log('[Comment] Own comment received via SignalR - notification skipped');
		}
	}

	// Handle comment updated via SignalR
	function handleCommentUpdated(commentId, comment) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		if ($commentItem.length > 0) {
			$commentItem.find('.comment-text').first().text(comment.content);

			var $commentTime = $commentItem.find('.comment-time');
			if (!$commentTime.find('.edited-badge').length) {
				$commentTime.append('<span class="edited-badge">• đã sửa</span>');
			}
		}
		
		// ✅ Update in allComments array
		var index = _allComments.findIndex(c => c.id === commentId);
		if (index !== -1) {
			_allComments[index] = comment;
		}
	}

	// ✅ IMPROVED: Handle comment deleted via SignalR
	function handleCommentDeleted(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		if ($commentItem.length > 0) {
			$commentItem.fadeOut(300, function () {
				$(this).remove();
				
				// ✅ Remove from allComments array
				_allComments = _allComments.filter(c => c.id !== commentId);
				_displayedCount = Math.max(0, _displayedCount - 1);
				
				// Update stats
				updateDisplayedStats();
				
				// Show empty state if no comments left
				if (_allComments.length === 0) {
					$('#comments-list').html(`
						<div class="empty-state">
							<i class="fas fa-comment-dots"></i>
							<p>Chưa có bình luận nào</p>
							<small>Hãy là người đầu tiên bình luận!</small>
						</div>
					`);
					$('.comments-stats').remove();
					$('#loadMoreContainer').hide();
				}
			});
		}
	}

	// Render new comment to DOM
	function renderComment(comment) {
		if ($(`[data-comment-id="${comment.id}"]`).length > 0) {
			return;
		}

		var html = buildCommentHtml(comment);

		if (comment.parentCommentId) {
			var $parentComment = $(`.comment-item[data-comment-id="${comment.parentCommentId}"]`);
			var $repliesContainer = $parentComment.next('.replies-container');

			if ($repliesContainer.length === 0) {
				$repliesContainer = $('<div class="replies-container"></div>');
				$parentComment.after($repliesContainer);
			}

			$repliesContainer.append(html);
		} else {
			$('#comments-list').prepend(html);
		}

		var $newComment = $(`.comment-item[data-comment-id="${comment.id}"]`);
		$newComment.hide().fadeIn(500);
	}

	// Build comment HTML
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
                    </span>`;

		if (isOwner) {
			html += `
                    <div class="comment-menu">
                        <button class="btn-edit-comment" type="button" data-comment-id="${comment.id}">
                            <i class="fas fa-pen"></i>
                        </button>
                        <button class="btn-delete-comment" type="button" data-comment-id="${comment.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>`;
		}

		html += `
                </div>
                <div class="comment-content" data-comment-id="${comment.id}">
                    <p class="comment-text">${comment.content}</p>
                </div>
                <div class="comment-edit-form" style="display: none;">
                    <textarea class="edit-textarea" rows="2" maxlength="1000">${comment.content}</textarea>
                    <div class="edit-actions">
                        <button class="btn-save-edit" data-comment-id="${comment.id}">Lưu</button>
                        <button class="btn-cancel-edit">Hủy</button>
                    </div>
                </div>`;

		if (abp.session.userId) {
			html += `
                <div class="comment-actions">
                    <button class="btn-reply" data-comment-id="${comment.id}" data-user-name="${comment.userFullName}">
                        <i class="fas fa-reply"></i> Trả lời
                    </button>
                </div>`;
		}

		html += `
            </div>
        </div>`;

		return html;
	}

	// Format comment time
	function formatCommentTime(dateString) {
		var date = new Date(dateString);
		var day = ('0' + date.getDate()).slice(-2);
		var month = ('0' + (date.getMonth() + 1)).slice(-2);
		var year = date.getFullYear();
		var hours = ('0' + date.getHours()).slice(-2);
		var minutes = ('0' + date.getMinutes()).slice(-2);
		return day + '/' + month + '/' + year + ' ' + hours + ':' + minutes;
	}

	// Show user typing indicator
	function showUserTyping(userName) {
		var $indicator = $('#typing-indicator');
		if ($indicator.length === 0) {
			$indicator = $('<div id="typing-indicator" class="text-muted small mb-2"><i class="fas fa-circle-notch fa-spin"></i> <span class="typing-user"></span> đang nhập...</div>');
			$('#commentForm').before($indicator);
		}
		$indicator.find('.typing-user').text(userName);
		$indicator.show();
	}

	// Hide user typing indicator
	function hideUserTyping(userName) {
		var $indicator = $('#typing-indicator');
		if ($indicator.find('.typing-user').text() === userName) {
			$indicator.hide();
		}
	}

	// Initialize comment form
	function initializeCommentForm() {
		$('#CommentContent').on('input', function () {
			var length = $(this).val().length;
			$('#charCount').text(length);

			if (_commentHubConnection && _commentHubConnection.state === signalR.HubConnectionState.Connected) {
				_commentHubConnection.invoke("UserTyping", parseInt(_productId), abp.session.userName);
			}
		});

		$('#submitComment').on('click', function () {
			submitComment();
		});

		$('#btnCancelReply').on('click', function () {
			cancelReply();
		});
	}

	// Initialize comment actions (reply, edit, delete)
	function initializeCommentActions() {
		$(document).on('click', '.btn-reply', function () {
			var commentId = $(this).data('comment-id');
			var userName = $(this).data('user-name');
			setReplyTo(commentId, userName);
		});

		$(document).on('click', '.btn-edit-comment', function () {
			var commentId = $(this).data('comment-id');
			showEditForm(commentId);
		});

		$(document).on('click', '.btn-save-edit', function () {
			var commentId = $(this).data('comment-id');
			saveEditComment(commentId);
		});

		$(document).on('click', '.btn-cancel-edit', function () {
			var commentId = $(this).closest('.comment-item').data('comment-id');
			hideEditForm(commentId);
		});

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

		$('html, body').animate({
			scrollTop: $('#commentForm').offset().top - 100
		}, 500);
	}

	// Cancel reply
	function cancelReply() {
		_replyToCommentId = null;
		_replyToUserName = '';

		$('#ParentCommentId').val('');
		$('#CommentContent').attr('placeholder', 'Viết bình luận của bạn...');
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
