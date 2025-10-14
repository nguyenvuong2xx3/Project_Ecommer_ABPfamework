(function () {
  app.modals.AddMemberModal = function () {
    var _modalManager;
    var selectedObjs = []; // Biến toàn cục lưu trữ tất cả đối tượng đã chọn từ mọi trang
    var _options = {
      serviceMethod: null, //Required
      title: app.localize('SelectAnItem'),
      loadOnStartup: true,
      showFilter: true,
      filterText: '',
      pageSize: app.consts.grid.defaultPageSize,
    };

    var _$table;
    var _$filterInput;
    var dataTable;


     // Lấy dữ liệu từ localStorage khi khởi tạo
     function loadSelectedObjsFromStorage() {
        var saved = localStorage.getItem('selectedMembers');
        if (saved) {
           selectedObjs = JSON.parse(saved);
        }
     }

     // Lưu dữ liệu vào localStorage
     function saveSelectedObjsToStorage() {
        localStorage.setItem('selectedMembers', JSON.stringify(selectedObjs));
     }

     // Kiểm tra xem một đối tượng có trong mảng selectedObjs chưa
     function isObjSelected(obj) {
        return selectedObjs.some(function (item) {
           return item.value === obj.value;
        });
     }

     // Thêm/xóa đối tượng khỏi mảng selectedObjs
     function toggleObjSelection(obj, isSelected) {
        if (isSelected) {
           if (!isObjSelected(obj)) {
              selectedObjs.push(obj);
           }
        } else {
           selectedObjs = selectedObjs.filter(function (item) {
              return item.value !== obj.value;
           });
        }
        saveSelectedObjsToStorage();
        updateSaveButtonState();
     }

     // Cập nhật trạng thái checkbox khi tải dữ liệu
     function updateCheckboxStates() {
        dataTable.rows().every(function () {
           var rowData = this.data();
           var checkbox = $('input[type="checkbox"][id="checkbox_' + rowData.value + '"]');

           if (isObjSelected(rowData)) {
              checkbox.prop('checked', true);
              this.select();
           } else {
              checkbox.prop('checked', false);
              this.deselect();
           }
        });
        updateSaveButtonState();
     }

    function refreshTable() {
       //dataTable.ajax.reload();
       dataTable.ajax.reload(function () {
          // Sau khi tải lại dữ liệu, cập nhật trạng thái checkbox
          updateCheckboxStates();
       });
    }

    function updateSaveButtonState() {
      //var rowData = dataTable.rows({ selected: true }).data().toArray();
      var $saveButton = _modalManager.getModal().find('#btnAddUsersToOrganization');
      if (selectedObjs.length > 0) {
        $saveButton.removeAttr('disabled');
      } else {
         $saveButton.attr('disabled', 'disabled');
         loadSelectedObjsFromStorage();
      }
    }

    this.init = function (modalManager) {
      _modalManager = modalManager;
      _options = $.extend(_options, _modalManager.getOptions().addMemberOptions);

      _$table = _modalManager.getModal().find('#addMemberModalTable');

      _$filterInput = _modalManager.getModal().find('.add-member-filter-text');
      _$filterInput.val(_options.filterText);

      dataTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        processing: true,
        deferLoading: 0,
        listAction: {
          ajaxFunction: _options.serviceMethod,
          inputFilter: function () {
            return {
              filter: _$filterInput.val(),
              organizationUnitId: _modalManager.getArgs().organizationUnitId,
            };
          },
        },
        columnDefs: [
          {
            targets: 0,
            data: null,
            orderable: false,
            defaultContent: '',
            render: function (data) {
              return (
                '<label for="checkbox_' +
                data.value +
                '" class="checkbox form-check">' +
                '<input type="checkbox" id="checkbox_' +
                data.value +
                '" class="form-check-input" />&nbsp;' +
                '<span class="form-check-label"></span>' +
                '</label>'
              );
            },
          },
          {
            targets: 1,
            data: 'name',
          },
          {
            targets: 2,
            visible: false,
            data: 'value',
          },
        ],
        select: {
          style: 'multi',
          info: false,
          selector: 'td:first-child label.checkbox input',
        },
      });

      dataTable
        .on('select', function (e, dt, type, indexes) {
           //updateSaveButtonState();
           var rowData = dataTable.row(indexes).data();
           toggleObjSelection(rowData, true);
        })
        .on('deselect', function (e, dt, type, indexes) {
           //updateSaveButtonState();
           var rowData = dataTable.row(indexes).data();
           toggleObjSelection(rowData, false);
        })
         .on('draw', function () {
            // Cập nhật trạng thái checkbox sau mỗi lần vẽ lại bảng
            updateCheckboxStates();
        });

      _modalManager
        .getModal()
        .find('.add-member-filter-button')
        .click(function (e) {
          e.preventDefault();
          refreshTable();
        });

      _modalManager
        .getModal()
        .find('.modal-body')
        .keydown(function (e) {
          if (e.which === 13) {
            e.preventDefault();
            refreshTable();
          }
        });

      if (_options.loadOnStartup) {
        refreshTable();
      }

      _modalManager
        .getModal()
        .find('#btnAddUsersToOrganization')
        .click(function () {
           //_modalManager.setResult(dataTable.rows({ selected: true }).data().toArray());
           // trả về toàn bộ selectedObjs đã được lưu
           _modalManager.setResult(selectedObjs);
          _modalManager.close();
        });
       // Refresh objs sau khi đóng modal
       _modalManager.onClose(function () {
          localStorage.removeItem('selectedMembers');
       });
     };
     
  };
})();
