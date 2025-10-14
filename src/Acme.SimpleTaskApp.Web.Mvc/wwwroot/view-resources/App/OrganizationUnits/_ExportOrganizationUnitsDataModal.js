
(function () {
	app.modals.ExportOrganizationUnitsDataModal = function () {
		var _modalManager;
		var _organizationUnitsExporterService = abp.services.app.organizationUnitExporter
		var _$form = null;

		this.init = function (modalManager) {
			_modalManager = modalManager;
			_$form = _modalManager.getModal().find('form[name=ExportOrganizationUnitsForm]');
			_$form.validate({
				validClass: "valid",
				errorClass: "invalid-feedback",
				highlight: function (element, errorClass, validClass) {
					$(element).addClass('is-invalid').removeClass('is-valid');
				},
				unhighlight: function (element, errorClass, validClass) {
					$(element).addClass('is-valid').removeClass('is-invalid');
				},
				rules: {
					SelectFileFormat: {
						required: true,
					},
				},
				messages: {
					SelectFileFormat: {
						required: 'Định dạng file phải được chọn',
					},
				},
			});
			_$form.inputMaskForm
		};

		this.save = function () {
			if (!_$form.valid()) return;
			console.log("click")
			_modalManager.setBusy(true);
			_organizationUnitsExporterService.exportData($("#SelectFileFormat").val())
				.done(function (result) {
					app.downloadTempFile(result);
				})
				.always(function () {
					_modalManager.setBusy(false);
				});
		};
	};
})();


