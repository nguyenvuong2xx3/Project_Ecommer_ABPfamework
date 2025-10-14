(function ($) {
	app.modals.CropImageModal = function () {
		var _modalManager;
		var $cropperJsApi = null;

		var _options = {
			serviceMethod: null,
			title: app.localize('UpdateImageAspectRatio'),
			imgData: null,
			//loadOnStartup: true,
			//onSelectionDone: function () { }
		};

		this.init = function (modalManager) {
			_modalManager = modalManager;
			_options = $.extend(_options, _modalManager.getOptions().cropImageOptions);
			//console.log(_options);

			$aspectHeight = _modalManager.getModal().find('#RatioHeight');
			$aspectWidth = _modalManager.getModal().find('#RatioWidth');
			$aspectWidth = _modalManager.getModal().find('#RatioWidth');

			var $profilePictureResize = $('#CommonImageResize');
			if (_options.imgData)
				$profilePictureResize.attr('src', _options.imgData);

			_modalManager.setBusy(true);
			setTimeout(function () {

				//console.log($aspectWidth, $aspectHeight);
				$cropperJsApi = $profilePictureResize.cropper({
					aspectRatio: $aspectWidth.val() / $aspectHeight.val(),
					viewMode: 1,
					autoCropArea: 1
				});
				_modalManager.setBusy(false);
			}, 500);



		};

		this.save = function () {
			//$('.preview-primaryimg').attr('src', $cropperJsApi.cropper('getCroppedCanvas').toDataURL("image/png"));
			//_modalManager.setResult($cropperJsApi.cropper('getCroppedCanvas').toDataURL("image/png"));
			//_modalManager.setResult($cropperJsApi.cropper('getCroppedCanvas').toBlob());
			//_modalManager.close();

			$cropperJsApi.cropper('getCroppedCanvas').toBlob(function (blob) {
				_modalManager.setResult(blob);

				// clear 
				if ($cropperJsApi) {
					$cropperJsApi = null;
				}

				_modalManager.close();

			});


		};
	};

	app.modals.CropImageModal.create = function (cropImageOptions) {
		return new app.ModalManager({
			viewUrl: abp.appPath + 'App/Common/CropImageModal',
			scriptUrl: abp.appPath + 'view-resources/App/Common/Modals/_CropImageModal.js',
			modalClass: 'CropImageModal',
			cropImageOptions: cropImageOptions,
		});
	};

})(jQuery);
