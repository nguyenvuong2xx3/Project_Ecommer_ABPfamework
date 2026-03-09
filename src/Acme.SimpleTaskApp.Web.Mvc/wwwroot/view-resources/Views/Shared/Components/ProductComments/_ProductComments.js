(function ($) {
	var _commentService = abp.services.app.productComment;
	var _productVariantId = $('#ProductVariantId').val();
	var _replyToCommentId = null;
	var _replyToUserName = '';
	var _commentHubConnection = null;
	
	// MỚI: Biến phân trang
	var _allComments = [];
	var _displayedCount = 10;
	var _pageSize = 10;

	$(document).ready(function () {
		initializeCommentForm();
		initializeCommentActions();
		// Kết nối SignalR
		initializeSignalR();
		
		// Load comment ban đầu với phân trang
		initializePagination();
	});

	// Load comment với phân trang dữ liệu ban đầu
	function initializePagination() {
		try {
			var commentsJson = $('#allCommentsData').text();
			_allComments = JSON.parse(commentsJson) || [];
			
			// xử lý click xem thêm bình luận
			$('#btnLoadMore').on('click', loadMoreComments);
			
			// Cập nhật số lượng đã hiển thị
			updateDisplayedStats();
		} catch (error) {
		}
	}

	// Xem thêm bình luận
	function loadMoreComments() {
		var $btn = $('#btnLoadMore');
		var $container = $('#comments-list');
		
		// hiển thị trạng thái đang tải
		$btn.prop('disabled', true);
		$btn.html('<i class="fas fa-spinner fa-spin"></i> Đang tải...');

		// Lấy trang tiếp theo
		var nextBatch = _allComments.slice(_displayedCount, _displayedCount + _pageSize);
		
		if (nextBatch.length === 0) {
			$('#loadMoreContainer').hide();
			return;
		}
		
		// Render trang tiếp
		setTimeout(function() {
			nextBatch.forEach(function(comment) {
				var html = buildCommentHtml(comment);
				$container.append(html);
				
				// Hiệu ứng cho bình luận mới
				var $newComment = $(`.comment-item[data-comment-id="${comment.id}"]`);
				$newComment.hide().fadeIn(300);
			});
			
			// Cập nhật bộ đếm
			_displayedCount += nextBatch.length;
			updateDisplayedStats();
			
			// Kiểm tra nếu còn bình luận
			if (_displayedCount >= _allComments.length) {
				$('#loadMoreContainer').hide();
			} else {
				var remaining = _allComments.length - _displayedCount;
				$btn.html(`<i class="fas fa-chevron-down"></i> Xem thêm bình luận (${remaining})`);
				$btn.prop('disabled', false);
			}
		}, 300);
	}

	// Cập nhật thống kê hiển thị
	function updateDisplayedStats() {
		$('#displayedCount').text(_displayedCount);
		$('#totalCommentCount').text(_allComments.length);
	}

	// Kết nối SignalR để nhận bình luận mới, cập nhật và xóa
	function initializeSignalR() {
		try {

			_commentHubConnection = new signalR.HubConnectionBuilder()
				.withUrl("/signalr-productCommentHub")
				.withAutomaticReconnect()
				.build();
			// nhận bình luận mới
			_commentHubConnection.on("ReceiveComment", function (data) {
				handleNewCommentReceived(data.comment);
			});
			// cập nhật
			_commentHubConnection.on("CommentUpdated", function (data) {
				handleCommentUpdated(data.commentId, data.comment);
			});
			// xóa
			_commentHubConnection.on("CommentDeleted", function (data) {
				handleCommentDeleted(data.commentId);
			});
			// nhận biết người dùng đang nhập
			_commentHubConnection.on("UserTyping", function (userName) {
				showUserTyping(userName);
			});
			// nhận biết người dùng ngừng nhập
			_commentHubConnection.on("UserStoppedTyping", function (userName) {
				hideUserTyping(userName);
			});

			_commentHubConnection.start()
				.then(function () {
					console.log('[SignalR] Đã kết nối thành công');
					return _commentHubConnection.invoke("JoinProductGroup", parseInt(_productVariantId));
				})
				.then(function () {
					console.log('[SignalR] Đã tham gia nhóm bình luận sản phẩm:', _productVariantId);
				})
				.catch(function (err) {
					console.error('[SignalR] Lỗi kết nối SignalR:', err);
				});

			_commentHubConnection.onreconnected(function () {
				console.log('[SignalR] Đã kết nối lại');
				_commentHubConnection.invoke("JoinProductGroup", parseInt(_productVariantId));
			});

		} catch (error) {
			console.error('[SignalR] Lỗi khởi tạo SignalR:', error);
		}
	}

	// Xử lý bình luận mới nhận qua SignalR
	function handleNewCommentReceived(comment) {
		if ($(`[data-comment-id="${comment.id}"]`).length > 0) {
			return;
		}

		var isOwnComment = abp.session.userId && comment.userId == abp.session.userId;

		// Thêm vào mảng allComments ở đầu (mới nhất trước)
		_allComments.unshift(comment);
		_displayedCount++;

		// Render bình luận mới lên đầu
		renderComment(comment);

		// Cập nhật thống kê
		updateDisplayedStats();

		// Hiển thị thanh thống kê nếu trước đó bị ẩn
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

		// Xóa trạng thái rỗng nếu có
		$('.empty-state').remove();

		if (!isOwnComment) {
			console.log('[Bình luận] Nhận bình luận mới từ người dùng khác qua SignalR');
		} else {
			console.log('[Bình luận] Bình luận của chính bạn đã được nhận qua SignalR - bỏ qua thông báo');
		}





	}

	// Xử lý comment được cập nhật qua SignalR
	function handleCommentUpdated(commentId, comment) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		if ($commentItem.length > 0) {
			$commentItem.find('.comment-text').first().text(comment.content);

			var $commentTime = $commentItem.find('.comment-time');
			if (!$commentTime.find('.edited-badge').length) {
				$commentTime.append('<span class="edited-badge">• đã sửa</span>');
			}
		}
		
		// Cập nhật trong mảng allComments
		var index = _allComments.findIndex(c => c.id === commentId);
		if (index !== -1) {
			_allComments[index] = comment;
		}
	}

	// Xử lý comment bị xóa qua SignalR
	function handleCommentDeleted(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		if ($commentItem.length > 0) {
			$commentItem.fadeOut(300, function () {
				$(this).remove();
				
				// Loại khỏi mảng allComments
				_allComments = _allComments.filter(c => c.id !== commentId);
				_displayedCount = Math.max(0, _displayedCount - 1);
				
				// Cập nhật thống kê
				updateDisplayedStats();
				
				// Hiển thị trạng thái rỗng nếu không còn bình luận
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

	// Render bình luận mới lên DOM
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

	// Xây dựng HTML cho comment
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

	// Định dạng thời gian bình luận
	function formatCommentTime(dateString) {
		var date = new Date(dateString);
		var day = ('0' + date.getDate()).slice(-2);
		var month = ('0' + (date.getMonth() + 1)).slice(-2);
		var year = date.getFullYear();
		var hours = ('0' + date.getHours()).slice(-2);
		var minutes = ('0' + date.getMinutes()).slice(-2);
		return day + '/' + month + '/' + year + ' ' + hours + ':' + minutes;
	}

	// Hiển thị chỉ báo người dùng đang gõ
	function showUserTyping(userName) {
		var $indicator = $('#typing-indicator');
		if ($indicator.length === 0) {
			$indicator = $('<div id="typing-indicator" class="text-muted small mb-2"><i class="fas fa-circle-notch fa-spin"></i> <span class="typing-user"></span> đang nhập...</div>');
			$('#commentForm').before($indicator);
		}
		$indicator.find('.typing-user').text(userName);
		$indicator.show();
	}

	// Ẩn chỉ báo khi đúng người dùng
	function hideUserTyping(userName) {
		var $indicator = $('#typing-indicator');
		if ($indicator.find('.typing-user').text() === userName) {
			$indicator.hide();
		}
	}

	// Khởi tạo form gửi bình luận
	function initializeCommentForm() {
		$('#CommentContent').on('input', function () {
			var length = $(this).val().length;
			$('#charCount').text(length);

			if (_commentHubConnection && _commentHubConnection.state === signalR.HubConnectionState.Connected) {
				_commentHubConnection.invoke("UserTyping", parseInt(_productVariantId), abp.session.userName);
			}
		});

		$('#submitComment').on('click', function () {
			submitComment();
		});

		$('#btnCancelReply').on('click', function () {
			cancelReply();
		});
	}

	// Khởi tạo hành động cho comment (reply, edit, delete)
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

	// Gửi bình luận
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
			productVariantId: parseInt(_productVariantId),
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

	// Đặt reply cho comment
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

	// Hủy reply
	function cancelReply() {
		_replyToCommentId = null;
		_replyToUserName = '';

		$('#ParentCommentId').val('');
		$('#CommentContent').attr('placeholder', 'Viết bình luận của bạn...');
		$('#btnCancelReply').hide();
	}

	// Hiển thị form sửa
	function showEditForm(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		var $content = $commentItem.find('.comment-content').first();
		var $editForm = $commentItem.find('.comment-edit-form').first();

		$content.hide();
		$editForm.show();
		$editForm.find('textarea').focus();
	}

	// Ẩn form sửa
	function hideEditForm(commentId) {
		var $commentItem = $(`.comment-item[data-comment-id="${commentId}"]`);
		var $content = $commentItem.find('.comment-content').first();
		var $editForm = $commentItem.find('.comment-edit-form').first();

		$editForm.hide();
		$content.show();
	}

	// Lưu sửa bình luận
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

	// Xóa bình luận
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

	// Ngắt kết nối SignalR khi rời trang
	$(window).on('beforeunload', function () {
		if (_commentHubConnection) {
			_commentHubConnection.invoke("LeaveProductGroup", parseInt(_productVariantId));
			_commentHubConnection.stop();
		}
	});

})(jQuery);
