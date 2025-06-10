$(document).ready(function () {
    console.log("Document ready - loading order data");

    // 1. Lấy ID đơn từ form hidden input
    var orderId = $('#IdDonDichVu').val();
    console.log("Order ID from input:", orderId);

    if (orderId) {
        // 2. Lấy dữ liệu đơn từ API
        $.ajax({
            url: layThongTinDonUrl,
            type: 'GET',
            data: { id: orderId },
            dataType: 'json',
            success: function (data) {
                console.log("API đã trả về dữ liệu:", data);

                // 3. Điền dữ liệu vào form với wrapper để đảm bảo DOM sẵn sàng
                fillFormDataSafely(data);
            },
            error: function (xhr, status, error) {
                console.error("Lỗi khi gọi API:", error);
                console.error("Thông tin chi tiết:", xhr.responseText);
            }
        });
    } else {
        // Fallback tới localStorage nếu không có orderId
        var editData = localStorage.getItem('editOrderData');
        console.log("Không tìm thấy order ID trong form, kiểm tra localStorage:", editData);

        if (editData) {
            try {
                var data = JSON.parse(editData);
                console.log("Dữ liệu từ localStorage:", data);
                fillFormDataSafely(data);
                localStorage.removeItem('editOrderData');
            } catch (error) {
                console.error("Lỗi khi xử lý dữ liệu từ localStorage:", error);
            }
        }
    }
});

// Đăng ký event khi click vào nút chọn nhân viên
$(document).ready(function () {
    // Nút chọn nhân viên
    $('.select-staff-btn').click(function () {
        // Tải danh sách chuyên môn và nhân viên
        loadStaffPopup();
    });

    // Nút xóa nhân viên đã chọn
    $('.clear-staff-btn').click(function () {
        $('#selectedStaffId').val('');
        $('.selected-staff .staff-name').text('Chưa chọn nhân viên');
        $('.select-staff-btn').show();
        $('.clear-staff-btn').hide();
    });

    // Đóng popup khi click vào nút đóng
    $(document).on('click', '.popup-close', function () {
        $('#staffPopup').hide();
    });

    // Chọn chuyên môn để lọc nhân viên
    $(document).on('change', '.specialty-dropdown select', function () {
        var chuyenMon = $(this).val();
        loadStaffList(chuyenMon);
    });

    // Chọn nhân viên từ danh sách
    $(document).on('click', '.staff-item.available', function () {
        var idNhanVien = $(this).data('id');
        var hoVaTen = $(this).data('name');

        $('#selectedStaffId').val(idNhanVien);
        $('.selected-staff .staff-name').text(hoVaTen);
        $('.select-staff-btn').hide();
        $('.clear-staff-btn').show();

        $('#staffPopup').hide();
    });
});

// Hàm tải popup chọn nhân viên
function loadStaffPopup() {
    $('#staffPopup').show();

    // Reset dropdown chuyên môn về giá trị mặc định
    $('.specialty-dropdown select').val('');

    // Tải danh sách nhân viên mặc định (không lọc theo chuyên môn)
    loadStaffList();
}

// Hàm tải danh sách nhân viên theo chuyên môn
function loadStaffList(chuyenMon = null) {
    // Hiển thị loading
    $('.staff-list').html('<div class="loading-indicator"><div class="loading-spinner"></div>Đang tải danh sách nhân viên...</div>');

    // Gọi API lấy danh sách nhân viên
    $.ajax({
        url: layDanhSachNhanVienKyThuatUrl,
        type: 'GET',
        data: chuyenMon ? { chuyenMon: chuyenMon } : {},
        success: function (data) {
            var staffList = $('.staff-list');
            staffList.empty();

            if (data && data.length > 0) {
                data.forEach(function (staff) {
                    var statusClass = staff.trangThai === 'Sẵn sàng' ? 'available' : 'busy';
                    //var statusText = staff.trangThai === 'Sẵn sàng' ? 'Sẵn sàng' : 'Đang bận';

                    var staffItem = $('<div class="staff-item ' + statusClass + '"></div>');
                    staffItem.data('id', staff.idNhanVien);
                    staffItem.data('name', staff.hoVaTen);
                    staffItem.data('specialty', staff.chuyenMon);

                    // Thay thế trạng thái bằng chuyên môn
                    var specialtyText = staff.chuyenMon || 'Không có chuyên môn';
                    staffItem.html('<span>' + staff.hoVaTen + ' - ' + specialtyText + '</span>');


                    staffList.append(staffItem);
                });
            } else {
                staffList.html('<div class="no-staff">Không tìm thấy nhân viên kỹ thuật phù hợp</div>');
            }
        },
        error: function () {
            $('.staff-list').html('<div class="error-message">Không thể tải danh sách nhân viên. Vui lòng thử lại sau.</div>');
        }
    });
}

// Enhanced wrapper function to ensure safe form filling
function fillFormDataSafely(data) {
    console.log("=== WRAPPER: PREPARING SAFE FORM FILL ===");
    
    // Wait for DOM to be completely ready
    $(document).ready(function() {
        // Additional delay to ensure all scripts and components are loaded
        setTimeout(function() {
            console.log("=== WRAPPER: DOM READY - PROCEEDING WITH FORM FILL ===");
            fillFormData(data);
        }, 100);
    });
}


// Format date for display (dd/MM/yyyy)
function formatDateDisplay(dateTimeString) {
    if (!dateTimeString) return '';

    try {
        var date = new Date(dateTimeString);
        var day = String(date.getDate()).padStart(2, '0');
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var year = date.getFullYear();

        return day + '/' + month + '/' + year;
    } catch (error) {
        console.error("Error formatting date for display:", error);
        return '';
    }
}

function fillFormData(data) {
    console.log("=== BẮT ĐẦU ĐIỀN FORM ===");
    console.log("Cấu trúc dữ liệu nhận được:", JSON.stringify(data, null, 2));
    
    // Debug: Kiểm tra các elements có tồn tại không
    console.log("=== KIỂM TRA FORM ELEMENTS ===");
    console.log("HoVaTen input exists:", $('input[name="HoVaTen"]').length);
    console.log("Sdt input exists:", $('input[name="Sdt"]').length);
    console.log("Email input exists:", $('input[name="Email"]').length);
    console.log("TenThietBi input exists:", $('input[name="TenThietBi"]').length);
    console.log("IdLoaiThietBi select exists:", $('select[name="IdLoaiThietBi"]').length);
    if (data.don && data.don.ngayTaoDon) {
        // Hiển thị ngày tạo đơn trong định dạng dd/MM/yyyy
        var formattedDate = formatDateDisplay(data.don.ngayTaoDon);
        $('.order-date').text('Ngày tạo: ' + formattedDate);
    }
    // Hiển thị thông tin nhân viên kỹ thuật
    if (data.don && data.don.nhanVienKyThuat) {
        var selectedStaff = data.don.nhanVienKyThuat;
        $('#selectedStaffId').val(selectedStaff.idNhanVien);
        
        // Update: Display both name and specialty
        var staffDisplayText = selectedStaff.hoVaTen;
        if (selectedStaff.chuyenMon) {
            staffDisplayText += ' - Chuyên môn: ' + selectedStaff.chuyenMon;
            // Store specialty as data attribute
            $('.selected-staff').attr('data-specialty', selectedStaff.chuyenMon);
        }
        
        $('.selected-staff .staff-name').text(staffDisplayText);
        $('.selected-staff').addClass('has-staff'); // Add this class for styling
        $('.selected-staff').attr('data-staff-id', selectedStaff.idNhanVien);
        $('.select-staff-btn').hide();
        $('.clear-staff-btn').show();
        
        console.log("✓ Filled nhân viên kỹ thuật:", staffDisplayText);
    } else {
        $('.selected-staff .staff-name').text('Chưa chọn nhân viên');
        $('.selected-staff').removeClass('has-staff'); // Remove class if no staff
        $('.selected-staff').removeAttr('data-staff-id');
        $('.selected-staff').removeAttr('data-specialty');
        $('.select-staff-btn').show();
        $('.clear-staff-btn').hide();
        console.log("✗ Không có nhân viên kỹ thuật được phân công");
    }
    // 1. Thông tin khách hàng
    if (data.khach) {
        console.log("=== ĐIỀN THÔNG TIN KHÁCH HÀNG ===");
        console.log("Khách data:", data.khach);

        // Fill Họ và tên
        var hoTenInputs = $('input[name="HoVaTen"]');
        console.log("HoVaTen inputs found:", hoTenInputs.length);
        if (hoTenInputs.length > 0) {
            hoTenInputs.val(data.khach.hoVaTen || '');
            hoTenInputs.trigger('input').trigger('change'); // Trigger events
            console.log("✓ Filled HoVaTen:", data.khach.hoVaTen);
            console.log("✓ Current value:", hoTenInputs.val());
        } else {
            console.warn("✗ Không tìm thấy input HoVaTen");
        }

        // Fill SĐT
        var sdtInputs = $('input[name="Sdt"]');
        console.log("Sdt inputs found:", sdtInputs.length);
        if (sdtInputs.length > 0) {
            sdtInputs.val(data.khach.sdt || '');
            sdtInputs.trigger('input').trigger('change'); // Trigger events
            console.log("✓ Filled Sdt:", data.khach.sdt);
            console.log("✓ Current value:", sdtInputs.val());
        } else {
            console.warn("✗ Không tìm thấy input Sdt");
        }

        // Fill Email
        //var emailInputs = $('input[name="Email"]');
        //console.log("Email inputs found:", emailInputs.length);
        //if (emailInputs.length > 0) {
        //    emailInputs.val(data.khach.Email || '');
        //    emailInputs.trigger('input').trigger('change'); // Trigger events
        //    console.log("✓ Filled Email:", data.khach.Email);
        //    console.log("✓ Current value:", emailInputs.val());
        //} else {
        //    console.warn("✗ Không tìm thấy input Email");
        //}

        // Fill Đường/Số nhà
        var duongSoNhaInputs = $('input[name="DuongSoNha"]');
        console.log("DuongSoNha inputs found:", duongSoNhaInputs.length);
        if (duongSoNhaInputs.length > 0) {
            duongSoNhaInputs.val(data.khach.duongSoNha || '');
            duongSoNhaInputs.trigger('input').trigger('change'); // Trigger events
            console.log("✓ Filled DuongSoNha:", data.khach.duongSoNha);
            console.log("✓ Current value:", duongSoNhaInputs.val());
        } else {
            console.warn("✗ Không tìm thấy input DuongSoNha");
        }

        // Fill customer search input
        var customerIdInputs = $('#customerIdInput');
        console.log("CustomerIdInput found:", customerIdInputs.length);
        if (customerIdInputs.length > 0) {
            if (data.khach.idUser) {
                customerIdInputs.val(data.khach.idUser + ' - ' + data.khach.hoVaTen);
                $('#IdUser').val(data.khach.idUser);
                console.log("✓ Filled customer search with user ID:", data.khach.idUser);
            } else {
                customerIdInputs.val(data.khach.hoVaTen || '');
                console.log("✓ Filled customer search without user ID");
            }
            customerIdInputs.trigger('input').trigger('change'); // Trigger events
        }

        // Fill địa chỉ với timeout để đảm bảo API sẵn sàng
        setTimeout(function() {
            fillAddressData(data.khach);
        }, 300);
    }

    // 2. Thông tin thiết bị
    if (data.don) {
        console.log("=== ĐIỀN THÔNG TIN THIẾT BỊ ===");
        console.log("Don data:", data.don);

        // Fill loại thiết bị
        var loaiThietBiSelects = $('select[name="IdLoaiThietBi"]');
        console.log("IdLoaiThietBi selects found:", loaiThietBiSelects.length);
        if (loaiThietBiSelects.length > 0) {
            loaiThietBiSelects.val(data.don.idLoaiThietBi || '');
            loaiThietBiSelects.trigger('change'); // Trigger change event for selects
            console.log("✓ Filled IdLoaiThietBi:", data.don.idLoaiThietBi);
            console.log("✓ Current value:", loaiThietBiSelects.val());
        } else {
            console.warn("✗ Không tìm thấy select IdLoaiThietBi");
        }

        // Fill tên thiết bị
        var tenThietBiInputs = $('input[name="TenThietBi"]');
        console.log("TenThietBi inputs found:", tenThietBiInputs.length);
        if (tenThietBiInputs.length > 0) {
            tenThietBiInputs.val(data.don.tenThietBi || '');
            tenThietBiInputs.trigger('input').trigger('change'); // Trigger events
            console.log("✓ Filled TenThietBi:", data.don.tenThietBi);
            console.log("✓ Current value:", tenThietBiInputs.val());
        } else {
            console.warn("✗ Không tìm thấy input TenThietBi");
        }

        // Fill loại dịch vụ
        var loaiDonInputs = $('input[name="LoaiDonDichVu"]');
        console.log("LoaiDonDichVu inputs found:", loaiDonInputs.length);
        if (loaiDonInputs.length > 0) {
            loaiDonInputs.val(data.don.loaiDonDichVu || '');
            loaiDonInputs.trigger('input').trigger('change'); // Trigger events
            console.log("✓ Filled LoaiDonDichVu:", data.don.loaiDonDichVu);
        }

        // Fill hình thức dịch vụ
        var hinhThucInputs = $('input[name="HinhThucDichVu"]');
        console.log("HinhThucDichVu inputs found:", hinhThucInputs.length);
        if (hinhThucInputs.length > 0) {
            hinhThucInputs.val(data.don.hinhThucDichVu || '');
            hinhThucInputs.trigger('input').trigger('change'); // Trigger events
            console.log("✓ Filled HinhThucDichVu:", data.don.hinhThucDichVu);
        }

        // Fill tiền với timeout để đảm bảo DOM sẵn sàng
        setTimeout(function() {
            var tongTienInputs = $('#tongTienInput');
            if (tongTienInputs.length > 0) {
                tongTienInputs.val(data.don.tongTien || 0);
                tongTienInputs.trigger('input').trigger('change'); // Trigger events
                console.log("✓ Filled TongTien:", data.don.tongTien);
            }

            //var advanceInputs = $('#advancePayment');
            //if (advanceInputs.length > 0) {
            //    advanceInputs.val(data.don.TienKhachHangUngTruoc || 0);
            //    advanceInputs.trigger('input').trigger('change'); // Trigger events
            //    console.log("✓ Filled TienKhachHangUngTruoc:", data.don.TienKhachHangUngTruoc);
            //}

            var statusInputs = $('#statusInput');
            if (statusInputs.length > 0) {
                statusInputs.val(data.don.trangThaiDon || 'Chưa hoàn thành');
                statusInputs.trigger('input').trigger('change'); // Trigger events
                console.log("✓ Filled TrangThaiDon:", data.don.trangThaiDon);
            }
        }, 100);

        // Fill ngày giờ với flatpickr integration
        setTimeout(function() {
            //if (data.don.NgayBatDau) {
            //    var startInputs = $('#startDatetimePicker');
            //    if (startInputs.length > 0) {
            //        var formattedStart = formatDateTime(data.don.NgayBatDau);
            //        startInputs.val(formattedStart);
                    
            //        // Try to update flatpickr if it exists
            //        if (startInputs[0]._flatpickr) {
            //            startInputs[0]._flatpickr.setDate(new Date(data.don.NgayBatDau));
            //        }
                    
            //        startInputs.trigger('change'); // Trigger change event
            //        console.log("✓ Filled NgayBatDau:", data.don.NgayBatDau, "formatted:", formattedStart);
            //    }
            //}

            if (data.don.ngayHoanThanh) {
                var endInputs = $('#endDatetimePicker');
                if (endInputs.length > 0) {
                    var formattedEnd = formatDateTime(data.don.ngayHoanThanh);
                    endInputs.val(formattedEnd);
                    
                    // Try to update flatpickr if it exists
                    if (endInputs[0]._flatpickr) {
                        endInputs[0]._flatpickr.setDate(new Date(data.don.ngayHoanThanh));
                    }
                    
                    endInputs.trigger('change'); // Trigger change event
                    console.log("✓ Filled NgayHoanThanh:", data.don.ngayHoanThanh, "formatted:", formattedEnd);
                }
            }
        }, 200);
    }

    // 3. Chi tiết lỗi và linh kiện với timeout
    setTimeout(function() {
        if (data.chiTiet && data.chiTiet.length > 0) {
            console.log("=== ĐIỀN CHI TIẾT LỖI ===");
            console.log("ChiTiet data:", data.chiTiet);
            fillErrorDetails(data.chiTiet);
        }
    }, 250);

    // 4. Hiển thị ảnh với timeout
    setTimeout(function() {
        if (data.deviceImages && data.deviceImages.length > 0) {
            console.log("=== ĐIỀN ẢNH THIẾT BỊ ===");
            fillDeviceImages(data.deviceImages);
        }

        if (data.warrantyImages && data.warrantyImages.length > 0) {
            console.log("=== ĐIỀN ẢNH BẢO HÀNH ===");
            fillWarrantyImages(data.warrantyImages);
        }
    }, 350);

    // 5. Cập nhật UI với timeout để đảm bảo tất cả đã được fill
    setTimeout(function () {
        var trangThaiDon = $('#statusInput').val();
        updatePaymentButtonVisibility(trangThaiDon); var trangThaiDon = $('#statusInput').val();
        updatePaymentButtonVisibility(trangThaiDon);
        // Trigger updateTotalPrice if it exists
        if (typeof updateTotalPrice === 'function') {
            updateTotalPrice();
        }
        
        console.log("=== HOÀN THÀNH ĐIỀN FORM ===");
        
        // Kiểm tra lại các giá trị sau khi fill với timeout lớn hơn
        setTimeout(function() {
            console.log("=== KIỂM TRA SAU KHI FILL ===");
            console.log("HoVaTen final value:", $('input[name="HoVaTen"]').val());
            console.log("Sdt final value:", $('input[name="Sdt"]').val());
            console.log("Email final value:", $('input[name="Email"]').val());
            console.log("TenThietBi final value:", $('input[name="TenThietBi"]').val());
            console.log("IdLoaiThietBi final value:", $('select[name="IdLoaiThietBi"]').val());
            console.log("TongTien final value:", $('#tongTienInput').val());
            console.log("StartDate final value:", $('#startDatetimePicker').val());
            console.log("EndDate final value:", $('#endDatetimePicker').val());
            
            // Trigger any additional UI updates
            $('form').trigger('formUpdated');
        }, 1000);
    }, 300);
}

// Thêm hàm mới để cập nhật hiển thị nút thanh toán dựa vào trạng thái
function updatePaymentButtonVisibility(trangThaiDon) {
    if (trangThaiDon === 'Đã hoàn thành') {
        // Hiển thị nút thanh toán nếu trạng thái là "Đã hoàn thành"
        $('.payment-btn').addClass('payment-btn-visible');
        console.log("✓ Hiển thị nút thanh toán do trạng thái là: Đã hoàn thành");
    } else {
        // Ẩn nút thanh toán nếu trạng thái không phải là "Đã hoàn thành"
        $('.payment-btn').removeClass('payment-btn-visible');
        console.log("✗ Ẩn nút thanh toán do trạng thái là:", trangThaiDon);
    }
}


// Helper function để format DateTime
function formatDateTime(dateTimeString) {
    if (!dateTimeString) return '';
    
    try {
        var date = new Date(dateTimeString);
        var formattedDate = date.getFullYear() + '-' +
            String(date.getMonth() + 1).padStart(2, '0') + '-' +
            String(date.getDate()).padStart(2, '0') + ' ' +
            String(date.getHours()).padStart(2, '0') + ':' +
            String(date.getMinutes()).padStart(2, '0');
        return formattedDate;
    } catch (error) {
        console.error("Error formatting date:", error);
        return '';
    }
}

function fillDeviceImages(images) {
    var devicePreview = $('#deviceImagePreview').empty();

    images.forEach(function (imageUrl) {
        var fileItem = $('<div class="file-item"></div>');
        var img = $('<img src="' + imageUrl + '" onclick="showImageInModal(this.src)" />');
        var fileInfo = $('<div class="file-info">Ảnh thiết bị</div>');
        var deleteBtn = $('<div class="delete-file" onclick="deleteImage(this)"><i class="fa-solid fa-times"></i></div>');

        fileItem.append(img).append(fileInfo).append(deleteBtn);
        devicePreview.append(fileItem);
    });
}

function fillWarrantyImages(images) {
    var warrantyPreview = $('#warrantyImagePreview').empty();

    images.forEach(function (imageUrl) {
        var fileItem = $('<div class="file-item"></div>');
        var img = $('<img src="' + imageUrl + '" onclick="showImageInModal(this.src)" />');
        var fileInfo = $('<div class="file-info">Ảnh bảo hành</div>');
        var deleteBtn = $('<div class="delete-file" onclick="deleteImage(this)"><i class="fa-solid fa-times"></i></div>');

        fileItem.append(img).append(fileInfo).append(deleteBtn);
        warrantyPreview.append(fileItem);
    });
}


function fillAddressData(khach) {
    console.log("=== ĐIỀN ĐỊA CHỈ ===");
    console.log("Địa chỉ data:", khach);

    // Load danh sách thành phố trước
    $.get(layDanhSachThanhPhoUrl, function (cities) {
        var cityDropdown = $('#cityDropdown');
        cityDropdown.empty();

        cities.forEach(function (city) {
            var item = $('<div class="dropdown-item"></div>');
            item.text(city.tenThanhPho);
            item.data('id', city.idThanhPho);
            item.click(function () {
                $('#cityInput').val(city.tenThanhPho);
                $('#cityId').val(city.idThanhPho);
                cityDropdown.hide();

                // Load quận sau khi chọn thành phố
                loadDistrictsForEdit(city.idThanhPho, khach);
            });
            cityDropdown.append(item);
        });

        // Set thành phố hiện tại - FIX: sửa ThanhPho thành thanhPho
        if (khach.thanhPho) {
            $('#cityInput').val(khach.thanhPho);
            $('#cityId').val(khach.idThanhPho || '');
            console.log("✓ Filled thành phố:", khach.thanhPho);
            console.log("✓ Current city value:", $('#cityInput').val());
        }

        // Load quận của thành phố đã chọn - FIX: sửa IdThanhPho thành idThanhPho
        if (khach.idThanhPho) {
            loadDistrictsForEdit(khach.idThanhPho, khach);
            console.log("✓ Loading quận cho thành phố:", khach.idThanhPho);
        }
    }).fail(function () {
        console.error("Không thể tải danh sách thành phố");
    });
}

function loadDistrictsForEdit(cityId, khach) {
    console.log("=== LOADING QUẬN HUYỆN ===");
    console.log("cityId:", cityId);
    console.log("khach.idQuan:", khach.idQuan);

    $.get(layDanhSachQuanUrl, { idThanhPho: cityId }, function (districts) {
        var districtDropdown = $('#districtDropdown');
        districtDropdown.empty();

        districts.forEach(function (district) {
            var item = $('<div class="dropdown-item"></div>');
            item.text(district.tenQuan);
            item.data('id', district.idQuan);
            item.click(function () {
                $('#districtInput').val(district.tenQuan);
                $('#districtId').val(district.idQuan);
                districtDropdown.hide();

                // Load phường sau khi chọn quận
                loadWardsForEdit(district.idQuan, khach);
            });
            districtDropdown.append(item);
        });

        // Set quận hiện tại
        $('#districtInput').val(khach.quan || '');
        $('#districtId').val(khach.idQuan || '');
        console.log("✓ Filled quận:", khach.quan);
        console.log("✓ Current district value:", $('#districtInput').val());

        // Load phường của quận đã chọn - FIX: sửa IdQuan thành idQuan
        if (khach.idQuan) {
            loadWardsForEdit(khach.idQuan, khach);
            console.log("✓ Loading phường cho quận:", khach.idQuan);
        }
    });
}

function loadWardsForEdit(districtId, khach) {
    console.log("=== LOADING PHƯỜNG XÃ ===");
    console.log("districtId:", districtId);
    console.log("khach.idPhuong:", khach.idPhuong);

    $.get(layDanhSachPhuongUrl, { idQuan: districtId }, function (wards) {
        var wardDropdown = $('#wardDropdown');
        wardDropdown.empty();

        wards.forEach(function (ward) {
            var item = $('<div class="dropdown-item"></div>');
            item.text(ward.tenPhuong);
            item.data('id', ward.idPhuong);
            item.click(function () {
                $('#wardInput').val(ward.tenPhuong);
                $('#wardId').val(ward.idPhuong);
                wardDropdown.hide();
            });
            wardDropdown.append(item);
        });

        // Set phường hiện tại
        $('#wardInput').val(khach.phuong || '');
        $('#wardId').val(khach.idPhuong || '');
        console.log("✓ Filled phường:", khach.phuong);
        console.log("✓ Current ward value:", $('#wardInput').val());
    });
}

function fillErrorDetails(details) {
    // Xóa các phần chi tiết thừa trừ phần đầu tiên
    $('.split-with-button').slice(1).remove();

    var firstSection = $('.split-with-button').first();

    details.forEach(function (detail, index) {
        var section = index === 0 ? firstSection : firstSection.clone(true);

        // Cập nhật name attributes cho section mới
        if (index > 0) {
            section.find('select, input, textarea').each(function () {
                var name = $(this).attr('name');
                if (name && name.includes('[0]')) {
                    $(this).attr('name', name.replace('[0]', '[' + index + ']'));
                }
                var id = $(this).attr('id');
                if (id && id.includes('-0')) {
                    $(this).attr('id', id.replace('-0', '-' + index));
                }
            });
        }

        // Trường hợp khi có idLinhKien nhưng không có idLoaiLinhKien
        if (detail.idLinhKien && !detail.idLoaiLinhKien) {
            console.log("Đang lấy thông tin linh kiện từ API cho idLinhKien:", detail.idLinhKien);

            // Gọi API để lấy thông tin linh kiện và loại linh kiện
            $.ajax({
                url: layThongTinLinhKienUrl,
                type: 'GET',
                data: { idLinhKien: detail.idLinhKien },
                dataType: 'json',
                async: false, // Đảm bảo xử lý tuần tự
                success: function (linhKienData) {
                    console.log("API trả về thông tin linh kiện:", linhKienData);

                    // Gán idLoaiLinhKien từ API, đảm bảo loại bỏ khoảng trắng
                    detail.idLoaiLinhKien = linhKienData.idLoaiLinhKien.trim();

                    // LƯU tên linh kiện và tên loại linh kiện để sử dụng sau
                    detail.tenLinhKien = linhKienData.tenLinhKien;
                    detail.tenLoaiLinhKien = linhKienData.tenLoaiLinhKien;

                    // Tìm select loại linh kiện một cách chính xác hơn - lấy select đầu tiên trong section
                    var loaiLinhKienSelect = section.find('select[name="IdLoaiLinhKien"], select.select-linhkien');
                    console.log("Tìm thấy select loại linh kiện:", loaiLinhKienSelect.length);

                    if (loaiLinhKienSelect.length) {
                        // Kiểm tra xem option có tồn tại không
                        var optionExists = false;
                        loaiLinhKienSelect.find('option').each(function () {
                            if ($(this).val() === detail.idLoaiLinhKien) {
                                optionExists = true;
                                return false; // break loop
                            }
                        });

                        // Nếu không tìm thấy option, thêm mới
                        if (!optionExists && detail.tenLoaiLinhKien) {
                            console.log("Thêm option mới cho loại linh kiện:", detail.idLoaiLinhKien, "-", detail.tenLoaiLinhKien);
                            loaiLinhKienSelect.append(new Option(detail.tenLoaiLinhKien, detail.idLoaiLinhKien, true, true));
                        }

                        // Đặt giá trị cho dropdown
                        loaiLinhKienSelect.val(detail.idLoaiLinhKien);
                        console.log("✓ Filled loại linh kiện:", detail.idLoaiLinhKien, "với tên:", detail.tenLoaiLinhKien);

                        // Trigger change để load danh sách linh kiện con
                        loaiLinhKienSelect.trigger('change');

                        // Dùng setTimeout để đảm bảo linh kiện con đã được load
                        setTimeout(function () {
                            var partSelect = section.find('.part-select, select[name*="IdLinhKien"]:not(.select-linhkien)');
                            console.log("Tìm thấy select linh kiện:", partSelect.length);

                            if (partSelect.length) {
                                // Kiểm tra xem option linh kiện đã tồn tại chưa
                                var optionExists = false;
                                partSelect.find('option').each(function () {
                                    if ($(this).val() === detail.idLinhKien) {
                                        optionExists = true;
                                        return false; // break loop
                                    }
                                });

                                // Nếu chưa có option, thêm mới
                                if (!optionExists && detail.tenLinhKien) {
                                    console.log("Thêm option mới cho linh kiện:", detail.idLinhKien, "-", detail.tenLinhKien);
                                    partSelect.append(new Option(detail.tenLinhKien, detail.idLinhKien, true, true));
                                }

                                // Chọn option và trigger change
                                partSelect.val(detail.idLinhKien);
                                partSelect.trigger('change');
                                console.log("✓ Filled linh kiện:", detail.idLinhKien, "với tên:", detail.tenLinhKien);
                            } else {
                                console.warn("Không tìm thấy select linh kiện");
                            }
                        }, 800);
                    } else {
                        console.warn("Không tìm thấy select loại linh kiện");
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Lỗi khi lấy thông tin linh kiện:", error);
                }
            });
        } else {
            // Xử lý bình thường nếu đã có đủ thông tin
            // Fill loại linh kiện
            if (detail.idLoaiLinhKien) {
                var loaiLinhKienSelect = section.find('select[name="IdLoaiLinhKien"], select.select-linhkien');
                if (loaiLinhKienSelect.length) {
                    loaiLinhKienSelect.val(detail.idLoaiLinhKien);
                    loaiLinhKienSelect.trigger('change');
                    console.log("✓ Filled loại linh kiện (từ data):", detail.idLoaiLinhKien);
                }
            }

            // Fill linh kiện cụ thể
            if (detail.idLinhKien) {
                // Đợi để combobox thứ nhất load xong danh sách
                setTimeout(function () {
                    var partSelect = section.find('.part-select, select[name*="ErrorDetails"][name*="IdLinhKien"]');
                    if (partSelect.length) {
                        partSelect.val(detail.idLinhKien);
                        partSelect.trigger('change');
                        console.log("✓ Filled linh kiện (từ data):", detail.idLinhKien);
                    }
                }, 800);
            }
        }

        // Fill loại lỗi
        if (detail.idLoi) {
            var loiSelect = section.find('select[name*="IdLoi"]');
            if (loiSelect.length) {
                loiSelect.val(detail.idLoi);
                loiSelect.trigger('change');
                // Trigger layGiaTheoLoi để load giá
                layGiaTheoLoi(detail.idLoi);
            }
        }

        // Fill mô tả lỗi
        if (detail.moTa) {
            var moTaTextarea = section.find('textarea[name*="MoTaLoi"]');
            moTaTextarea.val(detail.moTa);
        }

        // Fill trạng thái bảo hành
        if (detail.hanBaoHanh !== undefined) {
            var warrantyRadio = section.find('input[name*="HanBaoHanh"][value="' + detail.hanBaoHanh + '"]');
            warrantyRadio.prop('checked', true);
        }

        // Thêm section mới nếu không phải phần đầu tiên
        if (index > 0) {
            firstSection.parent().append(section);
        }
    });
}
//function fillImages(data) {
//    // Fill ảnh thiết bị
//    if (data.DeviceImages && data.DeviceImages.length > 0) {
//        console.log("Điền ảnh thiết bị:", data.DeviceImages);
//        var devicePreview = $('#deviceImagePreview').empty();
//        data.DeviceImages.forEach(function (url) {
//            devicePreview.append('<div class="file-item"><img src="' + url + '" onclick="showImageInModal(this.src)" /><div class="file-info">Ảnh thiết bị</div></div>');
//        });
//    }

//    // Fill ảnh bảo hành
//    if (data.WarrantyImages && data.WarrantyImages.length > 0) {
//        console.log("Điền ảnh bảo hành:", data.WarrantyImages);
//        var warrantyPreview = $('#warrantyImagePreview').empty();
//        data.WarrantyImages.forEach(function (url) {
//            warrantyPreview.append('<div class="file-item"><img src="' + url + '" onclick="showImageInModal(this.src)" /><div class="file-info">Ảnh bảo hành</div></div>');
//        });
//    }
//}

function updateTotalPrice() {
    // Code cập nhật tổng tiền
    // Giả sử tổng tiền được lấy từ input
    var tongTien = parseInt($('#tongTienInput').val()) || 0;
    var advancePay = parseInt($('#advancePayment').val()) || 0;
    var remaining = tongTien - advancePay;

    // Hiển thị số tiền còn lại
    $('#remainingAmount').text(remaining.toLocaleString('vi-VN') + ' VND');

    // Lấy trạng thái đơn từ input
    var trangThaiDon = $('#statusInput').val() || 'Chưa hoàn thành';

    // Hiển thị nút thanh toán nếu còn tiền phải trả và trạng thái là "Đã hoàn thành"
    if (remaining > 0 && trangThaiDon === 'Đã hoàn thành') {
        $('.payment-btn').addClass('payment-btn-visible');
    } else {
        $('.payment-btn').removeClass('payment-btn-visible');
    }
}

// Gọi API để lấy giá theo loại lỗi
function layGiaTheoLoi(idLoi) {
    if (!idLoi) return;

    $.ajax({
        url: layGiaTheoLoiUrl,
        type: 'GET',
        data: { idLoi: idLoi },
        success: function (data) {
            $('#donGia').val(data.gia);
            updateTotalPrice();
        },
        error: function () {
            console.error("Không thể lấy giá cho loại lỗi này");
        }
    });
}


function showImageInModal(src) {
    var modal = document.getElementById('imageModal');
    var modalImg = document.getElementById('modalImage');

    modal.style.display = "block";
    modalImg.src = src;
}
// Đăng ký sự kiện để đóng modal khi bấm nút đóng
$(document).on('click', '.modal-close', function () {
    document.getElementById('imageModal').style.display = "none";
});

// Export các hàm cần thiết ra global scope
window.showImageInModal = showImageInModal;
window.layGiaTheoLoi = layGiaTheoLoi;
window.deleteImage = function (element) {
    $(element).parent('.file-item').remove();
};