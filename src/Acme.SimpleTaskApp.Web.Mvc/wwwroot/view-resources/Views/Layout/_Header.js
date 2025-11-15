(function ($) {
	$(function () {
		// Back to my account

		$('#UserProfileBackToMyAccountButton').click(function (e) {
			e.preventDefault();
			abp.ajax({
				url: abp.appPath + 'Account/BackToImpersonator',
				success: function () {
					if (!app.supportsTenancyNameInUrl) {
						abp.multiTenancy.setTenantIdCookie(abp.session.impersonatorTenantId);
					}
				},
			});
		});

		//My settings

		var mySettingsModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/MySettingsModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_MySettingsModal.js',
			modalClass: 'MySettingsModal',
		});

		$('#UserProfileMySettingsLink').click(function (e) {
			e.preventDefault();
			mySettingsModal.open();
		});

		$('#UserDownloadCollectedDataLink').click(function (e) {
			e.preventDefault();
			abp.services.app.profile.prepareCollectedData().done(function () {
				abp.message.success(app.localize('GdprDataPrepareStartedNotification'));
			});
		});

		// Change password

		var changePasswordModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/ChangePasswordModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_ChangePasswordModal.js',
			modalClass: 'ChangePasswordModal',
		});

		$('#UserProfileChangePasswordLink').click(function (e) {
			e.preventDefault();
			changePasswordModal.open();
		});

		// Change profile picture

		var changeProfilePictureModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/ChangePictureModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_ChangePictureModal.js',
			modalClass: 'ChangeProfilePictureModal',
		});

		$('#UserProfileChangePictureLink').click(function (e) {
			e.preventDefault();
			changeProfilePictureModal.open();
		});

		// Manage linked accounts
		var _userLinkService = abp.services.app.userLink;

		var manageLinkedAccountsModal = new app.ModalManager({
			viewUrl: abp.appPath + 'App/Profile/LinkedAccountsModal',
			scriptUrl: abp.appPath + 'view-resources/App/Profile/_LinkedAccountsModal.js',
			modalClass: 'LinkedAccountsModal',
		});

		$('.manage-linked-accounts-link').click(function (e) {
			e.preventDefault();
			manageLinkedAccountsModal.open();
		});

	});
})(jQuery);
