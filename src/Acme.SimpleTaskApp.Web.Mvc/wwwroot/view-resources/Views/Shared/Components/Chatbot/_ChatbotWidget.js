(function ($) {
	'use strict';

	var ChatbotWidget = function () {
		// ✅ ABP Service - tự động map từ IChatbotAppService
		var _chatbotService = abp.services.app.chatbot;
		
		// Variables
		var $toggle = $('#chatbot-toggle');
		var $window = $('#chatbot-window');
		var $messages = $('#chatbot-messages');
		var $input = $('#chatbot-input');
		var $sendBtn = $('#send-message');
		var $clearBtn = $('.btn-clear-chat');
		var $minimizeBtn = $('.btn-minimize');
		var $charCount = $('#char-count');
		var $recommendations = $('#product-recommendations');

		var conversationId = null;
		var isProcessing = false;

		// Initialize
		var init = function () {
			// Load conversation ID from localStorage
			conversationId = localStorage.getItem('chatbot_conversation_id');
			if (!conversationId) {
				conversationId = generateUUID();
				localStorage.setItem('chatbot_conversation_id', conversationId);
			}

			bindEvents();
			setupAutoResize();
		};

		// Generate UUID
		var generateUUID = function () {
			return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
				return v.toString(16);
			});
		};

		// Bind events
		var bindEvents = function () {
			// Toggle chat window
			$toggle.on('click', toggleWindow);
			$minimizeBtn.on('click', toggleWindow);

			// Send message
			$sendBtn.on('click', sendMessage);
			$input.on('keydown', function (e) {
				if (e.key === 'Enter' && !e.shiftKey) {
					e.preventDefault();
					sendMessage();
				}
			});

			// Character count
			$input.on('input', function () {
				var length = $(this).val().length;
				$charCount.text(length);
			});

			// Clear chat
			$clearBtn.on('click', clearChat);

			// Quick suggestions
			$(document).on('click', '.quick-btn', function () {
				var question = $(this).data('question');
				$input.val(question);
				sendMessage();
			});

			// Product card click
			$(document).on('click', '.product-card-mini', function () {
				var url = $(this).data('url');
				if (url) {
					window.location.href = url;
				}
			});
		};

		// Setup auto-resize for textarea
		var setupAutoResize = function () {
			$input.on('input', function () {
				this.style.height = 'auto';
				this.style.height = (this.scrollHeight) + 'px';
			});
		};

		// Toggle chat window
		var toggleWindow = function () {
			$window.toggleClass('open');

			if ($window.hasClass('open')) {
				$input.focus();
				scrollToBottom();
			}
		};

		// Send message
		var sendMessage = function () {
			if (isProcessing) return;

			var message = $input.val().trim();
			if (!message) {
				abp.notify.warn('Vui lòng nhập câu hỏi');
				return;
			}

			// Add user message to UI
			addMessage('user', message);

			// Clear input
			$input.val('');
			$charCount.text('0');
			$input.css('height', 'auto');

			// Show typing indicator
			showTypingIndicator();

			// Disable input
			isProcessing = true;
			$sendBtn.prop('disabled', true);

			// ✅ Call ABP Service
			_chatbotService.sendMessage({
				message: message,
				conversationId: conversationId,
				categoryId: null,
				minPrice: null,
				maxPrice: null
			}).done(function (response) {
				hideTypingIndicator();

				// Add bot response
				addMessage('bot', response.message);

				// Show product recommendations
				if (response.recommendedProducts && response.recommendedProducts.length > 0) {
					showProductRecommendations(response.recommendedProducts);
				} else {
					$recommendations.hide();
				}

				// Update suggested questions
				if (response.suggestedQuestions && response.suggestedQuestions.length > 0) {
					updateSuggestedQuestions(response.suggestedQuestions);
				}

				// Update conversation ID
				conversationId = response.conversationId;
				localStorage.setItem('chatbot_conversation_id', conversationId);
			}).fail(function (error) {
				hideTypingIndicator();
				var errorMsg = error.message || 'Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại sau.';
				addMessage('bot', '❌ ' + errorMsg);
				console.error('Chatbot error:', error);
			}).always(function () {
				isProcessing = false;
				$sendBtn.prop('disabled', false);
				$input.focus();
			});
		};

		// Add message to UI
		var addMessage = function (role, text) {
			var messageHtml = '';
			var avatar = role === 'user' ? '<i class="fas fa-user"></i>' : '<i class="fas fa-robot"></i>';
			var messageClass = role === 'user' ? 'user-message' : 'bot-message';
			var time = formatTime(new Date());

			messageHtml = `
				<div class="message-item ${messageClass}">
					<div class="message-avatar">${avatar}</div>
					<div class="message-content">
						<div class="message-text">
							${formatMessage(text)}
						</div>
						<div class="message-time">${time}</div>
					</div>
				</div>
			`;

			// Remove quick suggestions if this is user message
			if (role === 'user') {
				$('#quick-suggestions').remove();
			}

			$messages.append(messageHtml);
			scrollToBottom();
		};

		// Format message text (convert line breaks, links, etc.)
		var formatMessage = function (text) {
			// Convert line breaks
			text = text.replace(/\n/g, '<br>');

			// Convert URLs to links
			var urlPattern = /(https?:\/\/[^\s]+)/g;
			text = text.replace(urlPattern, '<a href="$1" target="_blank">$1</a>');

			return text;
		};

		// Format time
		var formatTime = function (date) {
			var hours = date.getHours().toString().padStart(2, '0');
			var minutes = date.getMinutes().toString().padStart(2, '0');
			return hours + ':' + minutes;
		};

		// Show typing indicator
		var showTypingIndicator = function () {
			var typingHtml = `
				<div class="message-item bot-message typing-indicator-message">
					<div class="message-avatar">
						<i class="fas fa-robot"></i>
					</div>
					<div class="message-content">
						<div class="message-text">
							<div class="typing-indicator">
								<span></span>
								<span></span>
								<span></span>
							</div>
						</div>
					</div>
				</div>
			`;
			$messages.append(typingHtml);
			scrollToBottom();
		};

		// Hide typing indicator
		var hideTypingIndicator = function () {
			$('.typing-indicator-message').remove();
		};

		// Show product recommendations
		var showProductRecommendations = function (products) {
			var html = '';

			products.forEach(function (product) {
				var specs = [];
				if (product.ram) specs.push(product.ram);
				if (product.storage) specs.push(product.storage);
				if (product.color) specs.push(product.color);

				html += `
					<div class="product-card-mini" data-url="${product.url}">
						<img src="${product.imageUrl || '/img/products/default.png'}" 
						     alt="${product.name}" 
						     class="product-img-mini"
						     onerror="this.src='/img/products/default.png'">
						<div class="product-info-mini">
							<div class="product-name-mini">${product.name}</div>
							${specs.length > 0 ? `<div class="product-specs-mini">${specs.join(' / ')}</div>` : ''}
							<div class="product-price-mini">${formatPrice(product.price)}₫</div>
							${product.rating > 0 ? `<div class="product-rating-mini">⭐ ${product.rating.toFixed(1)}</div>` : ''}
						</div>
					</div>
				`;
			});

			$recommendations.html(html).show();
			scrollToBottom();
		};

		// Update suggested questions
		var updateSuggestedQuestions = function (questions) {
			var html = '';
			questions.forEach(function (question) {
				html += `<button type="button" class="quick-btn" data-question="${question}">💬 ${question}</button>`;
			});

			// Remove old suggestions
			$('#quick-suggestions').remove();

			// Add new suggestions
			var suggestionsHtml = `<div class="quick-suggestions" id="quick-suggestions">${html}</div>`;
			$messages.append(suggestionsHtml);
			scrollToBottom();
		};

		// Clear chat history
		var clearChat = function () {
			abp.message.confirm(
				'Bạn có chắc chắn muốn xóa toàn bộ lịch sử trò chuyện?',
				'Xác nhận',
				function (result) {
					if (result) {
						// ✅ Call ABP Service
						_chatbotService.clearChatHistory(conversationId)
							.done(function () {
								// Clear UI
								$messages.empty();
								$recommendations.hide().empty();

								// Generate new conversation ID
								conversationId = generateUUID();
								localStorage.setItem('chatbot_conversation_id', conversationId);

								// Show welcome message again
								addWelcomeMessage();
								showQuickSuggestions();
								
								abp.notify.success('Đã xóa lịch sử trò chuyện');
							})
							.fail(function () {
								abp.notify.error('Không thể xóa lịch sử');
							});
					}
				}
			);
		};

		// Add welcome message
		var addWelcomeMessage = function () {
			var welcomeHtml = `
				<div class="message-item bot-message welcome-message">
					<div class="message-avatar">
						<i class="fas fa-robot"></i>
					</div>
					<div class="message-content">
						<div class="message-text">
							<p>👋 Xin chào! Tôi là trợ lý AI của cửa hàng.</p>
							<p>Tôi có thể giúp bạn:</p>
							<ul>
								<li>🔍 Tìm kiếm sản phẩm phù hợp</li>
								<li>💰 So sánh giá cả và tính năng</li>
								<li>📱 Tư vấn lựa chọn điện thoại</li>
								<li>❓ Trả lời các câu hỏi về sản phẩm</li>
							</ul>
							<p>Bạn đang tìm kiếm điện thoại như thế nào?</p>
						</div>
						<div class="message-time">${formatTime(new Date())}</div>
					</div>
				</div>
			`;
			$messages.append(welcomeHtml);
		};

		// Show quick suggestions
		var showQuickSuggestions = function () {
			var suggestionsHtml = `
				<div class="quick-suggestions" id="quick-suggestions">
					<button type="button" class="quick-btn" data-question="Sản phẩm bán chạy nhất là gì?">
						🔥 Sản phẩm bán chạy
					</button>
					<button type="button" class="quick-btn" data-question="Điện thoại giá rẻ dưới 5 triệu">
						💰 Giá rẻ dưới 5 triệu
					</button>
					<button type="button" class="quick-btn" data-question="Điện thoại camera tốt">
						📷 Camera tốt
					</button>
					<button type="button" class="quick-btn" data-question="iPhone mới nhất">
						🍎 iPhone mới nhất
					</button>
				</div>
			`;
			$messages.append(suggestionsHtml);
		};

		// Scroll to bottom
		var scrollToBottom = function () {
			setTimeout(function () {
				$messages.scrollTop($messages[0].scrollHeight);
			}, 100);
		};

		// Format price
		var formatPrice = function (price) {
			return price.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
		};

		// Public methods
		return {
			init: init
		};
	};

	// Initialize on document ready
	$(document).ready(function () {
		var chatbot = new ChatbotWidget();
		chatbot.init();
	});

})(jQuery);
