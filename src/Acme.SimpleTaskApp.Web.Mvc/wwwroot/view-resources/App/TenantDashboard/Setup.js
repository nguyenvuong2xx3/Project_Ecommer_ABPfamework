(function () {
	$(function () {
		function chooseTemplate(templateId) {
			var NewSetupTenantMockModel = {
				mockSetupId: templateId,
			}
			abp.ajax({
				url: abp.appPath + 'App/TenantDashboard/Setup',
				data: JSON.stringify(NewSetupTenantMockModel),

				success: function () {
					abp.ui.clearBusy('body');
					window.location.href = abp.appPath;
				},
			});
		}

		$('a').click(function () {
			var $element = $(this);
			var templateId = $element.attr("data-template");
			if (templateId) {
				abp.ui.setBusy('body');
				chooseTemplate(templateId);
			}
		});

		$('#SetupInfoPortal').click(() => {
			abp.ui.setBusy('body');

			abp.ajax({
				url: abp.appPath + 'App/TenantDashboard/SetupInfoPortal',
				data: JSON.stringify({ mockSetupId: 0 }),
				success: function () {
					window.location.href = '/';
				},

			}).always(() => {
				abp.ui.clearBusy('body');
			});
		});

	});
})();