(function () {
	app.modals.SortingModal = function () {
		var _modalManager;
		var _dataTable;
		var _$table;
		var _$sortinglist;
		var _$filterInput;
		var _$onUpdating = false;

		var _options = {
			serviceMethod: null,
			title: app.localize('SelectAnItem'),
			loadOnStartup: true,
			showFilter: true,
			filterText: '',
			excludeCurrentUser: false,
			pageSize: app.consts.grid.defaultPageSize,
			onSelectionDone: function () { },
			canSelect: function (item) {
				/* This method can return boolean or a promise which returns boolean.
				 * A false value is used to prevent selection.
				 */
				return true;
			},
		};

		this.init = function (modalManager) {
			_modalManager = modalManager;
			_options = $.extend(_options, _modalManager.getOptions().lookupOptions);

			_$filterInput = _modalManager.getArgs().extraFilters;
			if (_$filterInput == null) {
				_options.serviceMethod().done(function (results) {
					DrawSortableList(results);
				});
			}
			else {
				_options.serviceMethod(_modalManager.getArgs().extraFilters).done(function (results) {
					DrawSortableList(results);
				});
			}

			function DrawSortableList(results) {
				$.each(results.items, function (index, data) {
					$('#sortableList').append(
						$('<li data-id="' + data.value + '">').addClass('inbox-data list-group-item d-flex align-items-center').append(
							$("<div>").addClass('moveHandler').append($('<i class="fa fa-arrows-alt moveWidget"></i> '))
						).append(
							$("<div>").addClass('sorting-item').append(
								$("<div>").addClass('ms-1').append(
									$("<span>").html(data.name)
								)
							)
						)
					);
				});
			}

			var sortingListEle = document.getElementById('sortableList');
			_$sortinglist = new Sortable(sortingListEle, {
				animation: 150,
				handle: '.moveHandler',
			});

			_modalManager.onBeforeClose(function () {
				if (typeof _options.onSelectionDone == 'function' && _$onUpdating) {
					var itemIds = [];

					$('#sortableList').find('li').each(function () {
						itemIds.push($(this)[0].dataset.id);
					});

					_options.onSelectionDone(_modalManager.getArgs().extraFilters, itemIds);
				}
			});

			$('.save-button').click(function (e) {
				_$onUpdating = true;
			});

			$('.close-button').click(function (e) {
				_$onUpdating = false;
			});

		};
	};

	app.modals.SortingModal.create = function (lookupOptions) {
		return new app.ModalManager({
			viewUrl: abp.appPath + 'App/Common/SortingModal',
			scriptUrl: abp.appPath + 'view-resources/App/Common/Modals/_SortingModal.js',
			modalClass: 'SortingModal',
			lookupOptions: lookupOptions,
		});
	};
})();
