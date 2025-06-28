// Hàm xác nhận quay lại danh sách đơn
function confirmBack() {
    if (confirm("Bạn có chắc chắn muốn quay lại danh sách không?")) {
        window.location.href = backToListUrl;
    }
}

// Định dạng tiền tệ không có chữ VND
function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN').format(value);
}

// Xử lý chức năng tìm kiếm linh kiện
$(document).ready(function() {
    const searchInput = $('#searchPartsInput');
    const partsDropdown = $('#partsDropdown');
    const selectedPartsContainer = $('#selectedPartsContainer');
    let typingTimer;
    const doneTypingInterval = 500;    
    let selectedParts = [];
    
    // Add event handler for the add-part-btn
    $(document).on('click', '.add-part-btn', function() {
        // Implement add part functionality if needed
    });    
    
    $(document).on('click', '.remove-part-btn', function() {
        // Implement remove part functionality if needed
    });
    
    // Bắt sự kiện khi người dùng gõ vào ô tìm kiếm
    searchInput.on('input', function() {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(function() {
            const keyword = searchInput.val();
            if (keyword && keyword.length >= 2) {
                searchParts(keyword);
            }
        }, doneTypingInterval);
    });

    // Hàm tìm kiếm linh kiện
    function searchParts(keyword) {
        $.ajax({
            url: searchLinhKienUrl,
            type: 'GET',
            data: { keyword: keyword },
            success: function(response) {
                displaySearchResults(response);
            },
            error: function() {
                partsDropdown.html('<div class="loading-indicator">Lỗi khi tìm kiếm</div>');
            }
        });
    }

    // Hiển thị kết quả tìm kiếm
    function displaySearchResults(results) {
        // Hiển thị kết quả tìm kiếm
    }

    // Đóng dropdown khi click ra ngoài
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-parts-container').length) {
            partsDropdown.hide();
        }
    });
});

// Xử lý tải ảnh thiết bị và ảnh phiếu bảo hành        
$(document).ready(function() {
    // Kiểm tra và cập nhật giá khi trang đã tải xong
    const loiSelect = document.getElementById('IdLoi');
    if (loiSelect && loiSelect.value) {
        layGiaTheoLoi(loiSelect.value);
    }
    
    // Xử lý tải ảnh thiết bị bị lỗi
    const deviceImageInput = document.getElementById('deviceImageInput');
    const deviceImageButton = document.getElementById('deviceImageButton');
    const deviceImagePreview = document.getElementById('deviceImagePreview');
    
    // Xử lý tải ảnh phiếu bảo hành
    const warrantyImageInput = document.getElementById('warrantyImageInput');
    const warrantyImageButton = document.getElementById('warrantyImageButton');
    const warrantyImagePreview = document.getElementById('warrantyImagePreview');
    
    // Khi nhấn nút "Chọn tệp" cho ảnh thiết bị
    if(deviceImageButton) {
        deviceImageButton.addEventListener('click', function() {
            deviceImageInput.click();
        });
    }
    
    // Khi nhấn nút "Chọn tệp" cho ảnh phiếu bảo hành
    if(warrantyImageButton) {
        warrantyImageButton.addEventListener('click', function() {
            warrantyImageInput.click();
        });
    }
    
    // Khi chọn ảnh thiết bị
    if(deviceImageInput) {
        deviceImageInput.addEventListener('change', function(event) {
            handleImageUpload(event, deviceImagePreview);
        });
    }
    
    // Khi chọn ảnh phiếu bảo hành
    if(warrantyImageInput) {
        warrantyImageInput.addEventListener('change', function(event) {
            handleImageUpload(event, warrantyImagePreview);
        });
    }

    // Xử lý ban đầu cho các ảnh demo đã có sẵn
    document.querySelectorAll('.file-preview .delete-file').forEach(function(deleteBtn) {
        deleteBtn.addEventListener('click', function() {
            this.parentElement.remove();
        });
    });
    
    // Xử lý đóng modal
    const modal = document.getElementById('imageModal');
    const closeButton = document.querySelector('.modal-close');
    
    // Đóng modal khi nhấn nút X
    if (closeButton) {
        closeButton.addEventListener('click', function() {
            modal.style.display = "none";
        });
    }
    
    // Đóng modal khi nhấn bên ngoài ảnh
    if (modal) {
        modal.addEventListener('click', function(event) {
            if (event.target === modal) {
                modal.style.display = "none";
            }
        });
    }
});

// Hàm xử lý tải ảnh
function handleImageUpload(event, previewElement) {
    const files = event.target.files;
    
    if (files) {
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const reader = new FileReader();
            
            reader.onload = function(e) {
                const fileItem = document.createElement('div');
                fileItem.className = 'file-item';
                
                const img = document.createElement('img');
                img.src = e.target.result;
                img.onclick = function() {
                    showImageInModal(e.target.result);
                };
                
                const fileInfo = document.createElement('div');
                fileInfo.className = 'file-info';
                fileInfo.textContent = file.name;
                
                const deleteBtn = document.createElement('span');
                deleteBtn.className = 'delete-file';
                deleteBtn.innerHTML = '&times;';
                deleteBtn.onclick = function() {
                    fileItem.remove();
                };
                
                fileItem.appendChild(img);
                fileItem.appendChild(fileInfo);
                fileItem.appendChild(deleteBtn);
                previewElement.appendChild(fileItem);
            };
            
            reader.readAsDataURL(file);
        }
    }
}

// Hàm để hiển thị ảnh trong modal
function showImageInModal(imageSrc) {
    const modal = document.getElementById('imageModal');
    const modalImg = document.getElementById('modalImage');
    
    // Hiển thị modal và thiết lập nguồn ảnh
    modalImg.src = imageSrc;
    modal.style.display = "block";
}

// Hàm để lấy giá theo lỗi
function layGiaTheoLoi(idLoi, targetId) {
    // Cập nhật hidden input cho IdLoi
    document.getElementById('ErrorLoi').value = idLoi;
    
    // Nếu không có targetId, sử dụng ID mặc định 'donGia'
    const priceFieldId = targetId || 'donGia';
    
    if (!idLoi) {
        document.getElementById(priceFieldId).value = "0";
        updateTotalPrice();
        return;
    }
    
    fetch(layGiaTheoLoiUrl + '?idLoi=' + idLoi)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById(priceFieldId).value = data.giaFormatted;
                updateTotalPrice();
            } else {
                console.error('Lỗi:', data.message);
                document.getElementById(priceFieldId).value = "0";
                updateTotalPrice();
            }
        })
        .catch(error => {
            console.error('Lỗi khi lấy giá theo lỗi:', error);
            document.getElementById(priceFieldId).value = "0";
            updateTotalPrice();
        });
}

// Hàm để cập nhật tổng tiền
function updateTotalPrice() {
    // Tìm tất cả các trường đơn giá (bao gồm các trường được thêm động)
    const donGiaFields = document.querySelectorAll('.price-field');
    let totalPrice = 0;
    
    // Tính tổng giá từ tất cả các thành phần
    donGiaFields.forEach(field => {
        let value = field.value.replace(/[^\d]/g, '');
        totalPrice += parseInt(value) || 0;
    });
    
    // Cập nhật input "Tiền lỗi" (chính là tổng của các price-field)
    const errorPriceInput = document.querySelectorAll('.price-input')[0];
    if (errorPriceInput) {
        errorPriceInput.value = formatCurrency(totalPrice);
    }
    
    // Tính tổng tiền = Tiền lỗi + Tiền linh kiện (nếu có)
    let partPrice = 0;
    const selectedParts = document.querySelectorAll('.selected-part');
    selectedParts.forEach(part => {
        const priceText = part.querySelector('.part-price').textContent;
        const price = parseInt(priceText.replace(/[^\d]/g, '')) || 0;
        partPrice += price;
    });
    
    // Tổng tiền = Tiền lỗi + Tiền linh kiện
    const finalPrice = totalPrice + partPrice;
    
    // Cập nhật input "Tổng tiền"
    const totalPriceInput = document.querySelectorAll('.price-input')[1];
    if (totalPriceInput) {
        totalPriceInput.value = formatCurrency(finalPrice);
    }
    
    // Tính tiền còn lại = Tổng tiền - Tiền ứng trước
    const advancePaymentInput = document.getElementById('advancePayment');
    const advancePayment = parseInt(advancePaymentInput.value) || 0;
    const remainingAmount = finalPrice - advancePayment;
    
    // Cập nhật hiển thị tiền còn lại
    const remainingAmountSpan = document.getElementById('remainingAmount');
    if (remainingAmountSpan) {
        remainingAmountSpan.textContent = formatCurrency(remainingAmount);
    }
}

$(document).ready(function() {
    // Thêm sự kiện để cập nhật tổng tiền khi tiền ứng trước thay đổi
    $('#advancePayment').on('input', function() {
        updateTotalPrice();
    });
});

// Staff Assignment Popup Functionality
$(document).ready(function() {
    const selectStaffBtn = document.querySelector('.select-staff-btn');
    const clearStaffBtn = document.querySelector('.clear-staff-btn');
    const staffPopup = document.getElementById('staffPopup');
    const specialtySelect = document.querySelector('.specialty-dropdown select');
    const staffListSection = document.querySelector('.staff-list');
    const selectedStaffDisplay = document.querySelector('.selected-staff');
    const selectedStaffIdInput = document.getElementById('selectedStaffId');
    
    // Clear and initialize the specialty dropdown when the page loads
    fetchAndPopulateSpecialties();
    
    // Open popup when clicking the select staff button
    if (selectStaffBtn) {
        selectStaffBtn.addEventListener('click', function() {
            staffPopup.style.display = 'flex';
        });
    }
    
    // Close popup when clicking outside
    if (staffPopup) {
        staffPopup.addEventListener('click', function(event) {
            if (event.target === staffPopup) {
                staffPopup.style.display = 'none';
            }
        });
    }
    
    // Handle specialty selection change
    if (specialtySelect) {
        specialtySelect.addEventListener('change', function() {
            const selectedSpecialty = this.value;
            if (selectedSpecialty) {
                fetchStaffBySpecialty(selectedSpecialty);
            } else {
                staffListSection.innerHTML = '<p>Vui lòng chọn chuyên môn</p>';
            }
        });
    }
    
    // Fetch available specialties from the server
    function fetchAndPopulateSpecialties() {
        // This would be an API call in a real implementation
        // For now, we'll just use the mock data
        // populateSpecialtyDropdown(mockSpecialties);
    }
    
    // Populate the specialty dropdown with data from the server
    function populateSpecialtyDropdown(specialties) {
        // Populate dropdown based on fetched data
    }
    
    // Fetch staff by specialty from the server
    function fetchStaffBySpecialty(specialty) {
        // This would be an API call in a real implementation
        // For now, we'll just use mock data
        // populateStaffList(mockStaff);
    }
    
    function populateStaffList(staffList) {
        // Populate staff list based on fetched data
    }
});

// Initialize datepickers when the document is ready
$(document).ready(function() {
    // Initialize flatpickr for date/time pickers
    if (typeof flatpickr !== 'undefined') {
        flatpickr("#startDatetimePicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i",
            time_24hr: true,
            locale: "vn"
        });
        
        flatpickr("#endDatetimePicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i",
            time_24hr: true,
            locale: "vn"
        });
    }
    
    // Initialize dropdowns
    $('.dropdown input[readonly]').click(function() {
        const dropdown = $(this).siblings('.dropdown-content');
        dropdown.slideToggle(200);
    });
    
    $('.dropdown-item').click(function() {
        const value = $(this).text();
        const input = $(this).parent().siblings('input[readonly]');
        input.val(value);
        $(this).parent().slideUp(200);
        
        // Nếu đây là dropdown trạng thái, cập nhật nút thanh toán
        if ($(this).closest('#statusDropdown').length > 0) {
            if (value === "Đã hoàn thành") {
                $('.payment-btn').addClass('payment-btn-visible');
            } else {
                $('.payment-btn').removeClass('payment-btn-visible');
            }
        }
    });
    
    // Close dropdowns when clicking outside
    $(document).click(function(e) {
        if (!$(e.target).closest('.dropdown').length) {
            $('.dropdown-content').slideUp(200);
        }
    });
    
    // Xử lý chọn địa chỉ theo thứ tự thành phố > quận/huyện > phường/xã
    initLocationSelectors();
    
    // Thêm sự kiện submit cho form
    $('#serviceOrderForm').on('submit', function(e) {
        if (!validateOrderForm()) {
            e.preventDefault();
            return false;
        }
        
        // Chuẩn bị giá trị tổng tiền
        const tongTienInput = $('#tongTienInput');
        if (tongTienInput.length > 0) {
            // Chuyển từ định dạng 123,456 thành 123456
            const valueText = tongTienInput.val().replace(/[^\d]/g, '');
            tongTienInput.val(valueText);
        }
        
        // Chuẩn bị giá trị tiền ứng trước
        const advancePaymentInput = $('#advancePayment');
        if (advancePaymentInput.length > 0) {
            // Đảm bảo giá trị là số
            const valueText = advancePaymentInput.val().trim();
            if (valueText === '') {
                advancePaymentInput.val('0');
            }
        }
        
        // Lưu IdLoi và IdLinhKien từ dropdown chọn
        document.getElementById('ErrorLoi').value = document.getElementById('IdLoi').value;
        // Có thể thêm logic lưu IdLinhKien ở đây nếu có dropdown chọn linh kiện
        
        // Form hợp lệ, cho phép submit
        return true;
    });
});

// Khởi tạo hệ thống chọn địa chỉ
function initLocationSelectors() {
    const cityInput = document.getElementById('cityInput');
    const districtInput = document.getElementById('districtInput');
    const wardInput = document.getElementById('wardInput');
    
    const cityIdInput = document.getElementById('cityId');
    const districtIdInput = document.getElementById('districtId');
    const wardIdInput = document.getElementById('wardId');
    
    const cityDropdown = document.getElementById('cityDropdown');
    const districtDropdown = document.getElementById('districtDropdown');
    const wardDropdown = document.getElementById('wardDropdown');
    
    const cityDropdownIcon = document.getElementById('cityDropdownIcon');
    const districtDropdownIcon = document.getElementById('districtDropdownIcon');
    const wardDropdownIcon = document.getElementById('wardDropdownIcon');

    // Lấy danh sách thành phố từ API khi trang tải
    fetchCitiesFromDatabase();
    
    // Xử lý các events và hiển thị dropdown
    // (code hiện tại đã đủ)
}

// Đóng tất cả các dropdown
function closeAllDropdowns() {
    document.querySelectorAll('.custom-dropdown').forEach(dropdown => {
        dropdown.style.display = 'none';
    });
    
    document.querySelectorAll('.location-dropdown-icon').forEach(icon => {
        icon.classList.remove('active');
    });
    
    document.querySelectorAll('.input-with-icon input').forEach(input => {
        input.classList.remove('input-active');
    });
}

// Hiển thị thông báo lỗi
function showErrorMessage(errorId, message) {
    const errorElement = document.getElementById(errorId);
    if (errorElement) {
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    }
}

// Ẩn thông báo lỗi
function hideErrorMessage(errorId) {
    const errorElement = document.getElementById(errorId);
    if (errorElement) {
        errorElement.style.display = 'none';
    }
}

// Lấy danh sách thành phố từ database
function fetchCitiesFromDatabase() {
    fetch(layDanhSachThanhPhoUrl)
        .then(response => response.json())
        .then(cities => {
            populateCityDropdown(cities);
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách thành phố:', error);
        });
}

// Hiển thị danh sách thành phố trong dropdown
function populateCityDropdown(cities) {
    const cityDropdown = document.getElementById('cityDropdown');
    if (!cityDropdown) return;
    
    cityDropdown.innerHTML = '';
    
    if (cities.length === 0) {
        cityDropdown.innerHTML = '<div class="loading-indicator">Không có dữ liệu</div>';
        return;
    }
    
    cities.forEach(city => {
        const item = document.createElement('div');
        item.className = 'dropdown-item';
        item.textContent = city.ten;
        item.dataset.id = city.id;
        
        item.addEventListener('click', function() {
            document.getElementById('cityInput').value = city.ten;
            document.getElementById('cityId').value = city.id;
            cityDropdown.style.display = 'none';
            
            // Reset quận/huyện và phường/xã khi chọn thành phố mới
            resetDistrictAndWard();
            
            // Lấy danh sách quận/huyện theo thành phố
            fetchDistrictsByCity(city.id);
        });
        
        cityDropdown.appendChild(item);
    });
}

// Reset quận/huyện và phường/xã khi chọn thành phố mới
function resetDistrictAndWard() {
    document.getElementById('districtInput').value = '';
    document.getElementById('districtId').value = '';
    document.getElementById('wardInput').value = '';
    document.getElementById('wardId').value = '';
}

// Lấy danh sách quận/huyện theo thành phố từ database
function fetchDistrictsByCity(cityId) {
    fetch(layDanhSachQuanUrl + '?idThanhPho=' + cityId)
        .then(response => response.json())
        .then(data => {
            if (data.success === false) {
                console.error('Lỗi:', data.message);
                return;
            }
            populateDistrictDropdown(data);
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách quận/huyện:', error);
        });
}

// Hiển thị danh sách quận/huyện trong dropdown
function populateDistrictDropdown(districts) {
    const districtDropdown = document.getElementById('districtDropdown');
    if (!districtDropdown) return;
    
    districtDropdown.innerHTML = '';
    
    if (districts.length === 0) {
        districtDropdown.innerHTML = '<div class="loading-indicator">Không có dữ liệu</div>';
        return;
    }
    
    districts.forEach(district => {
        const item = document.createElement('div');
        item.className = 'dropdown-item';
        item.textContent = district.ten;
        item.dataset.id = district.id;
        
        item.addEventListener('click', function() {
            document.getElementById('districtInput').value = district.ten;
            document.getElementById('districtId').value = district.id;
            districtDropdown.style.display = 'none';
            
            // Reset phường/xã khi chọn quận/huyện mới
            document.getElementById('wardInput').value = '';
            document.getElementById('wardId').value = '';
            
            // Lấy danh sách phường/xã theo quận/huyện
            fetchWardsByDistrict(district.id);
        });
        
        districtDropdown.appendChild(item);
    });
}

// Lấy danh sách phường/xã theo quận/huyện từ database
function fetchWardsByDistrict(districtId) {
    fetch(layDanhSachPhuongUrl + '?idQuan=' + districtId)
        .then(response => response.json())
        .then(data => {
            if (data.success === false) {
                console.error('Lỗi:', data.message);
                return;
            }
            populateWardDropdown(data);
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách phường/xã:', error);
        });
}

// Hiển thị danh sách phường/xã trong dropdown
function populateWardDropdown(wards) {
    const wardDropdown = document.getElementById('wardDropdown');
    if (!wardDropdown) return;
    
    wardDropdown.innerHTML = '';
    
    if (wards.length === 0) {
        wardDropdown.innerHTML = '<div class="loading-indicator">Không có dữ liệu</div>';
        return;
    }
    
    wards.forEach(ward => {
        const item = document.createElement('div');
        item.className = 'dropdown-item';
        item.textContent = ward.ten;
        item.dataset.id = ward.id;
        
        item.addEventListener('click', function() {
            document.getElementById('wardInput').value = ward.ten;
            document.getElementById('wardId').value = ward.id;
            wardDropdown.style.display = 'none';
        });
        
        wardDropdown.appendChild(item);
    });
}

// Function to validate order form
function validateOrderForm() {
    // Kiểm tra thông tin cá nhân
    const customerName = $('.form-group:contains("Họ và tên khách hàng") input').val();
    if (!customerName) {
        alert('Vui lòng nhập họ và tên khách hàng!');
        return false;
    }
    
    const phone = $('.form-group:contains("Số điện thoại") input').val();
    if (!phone) {
        alert('Vui lòng nhập số điện thoại!');
        return false;
    }
    
    // Kiểm tra địa chỉ
    const wardId = $('#wardId').val();
    if (!wardId) {
        alert('Vui lòng chọn đầy đủ thành phố, quận/huyện và phường/xã!');
        return false;
    }
    
    const address = $('input[placeholder="Số nhà, đường"]').val();
    if (!address) {
        alert('Vui lòng nhập địa chỉ cụ thể (đường/số nhà)!');
        return false;
    }
    
    // Kiểm tra thông tin thiết bị
    const deviceType = $('#IdLoaiThietBi').val();
    if (!deviceType) {
        alert('Vui lòng chọn loại thiết bị!');
        return false;
    }
    
    const deviceName = $('.form-group:contains("Tên thiết bị") input').val();
    if (!deviceName) {
        alert('Vui lòng nhập tên thiết bị!');
        return false;
    }
    
    // Kiểm tra lỗi
    const errorType = $('.form-group:contains("Lỗi") select').val();
    if (!errorType) {
        alert('Vui lòng chọn loại lỗi!');
        return false;
    }
    
    const errorDescription = $('.form-group:contains("Mô tả lỗi") textarea').val();
    if (!errorDescription) {
        alert('Vui lòng nhập mô tả lỗi!');
        return false;
    }
    
    // Kiểm tra loại dịch vụ
    const serviceType = $('.section:contains("Loại dịch vụ") .dropdown input').eq(0).val();
    if (!serviceType) {
        alert('Vui lòng chọn loại dịch vụ!');
        return false;
    }
    
    const serviceLocation = $('.section:contains("Loại dịch vụ") .dropdown input').eq(1).val();
    if (!serviceLocation) {
        alert('Vui lòng chọn nơi thực hiện dịch vụ!');
        return false;
    }
    
    // Kiểm tra nhân viên kỹ thuật
    const technicianId = $('#selectedStaffId').val();
    if (!technicianId) {
        alert('Vui lòng chọn nhân viên kỹ thuật!');
        return false;
    }
    
    // Kiểm tra ảnh thiết bị
    const deviceImagesCount = $('#deviceImagePreview .file-item').length;
    const deviceImageFiles = $('#deviceImageInput')[0]?.files.length || 0;
    if (deviceImagesCount === 0 && deviceImageFiles === 0) {
        alert('Vui lòng tải lên ít nhất một ảnh thiết bị!');
        return false;
    }
    
    return true;
}

// Lấy dữ liệu từ form và điền vào hóa đơn
function populateInvoiceData() {
    // Thông tin đơn hàng
    $('#invoice-order-id').text($('#IdDonDichVu').val());
    $('#invoice-order-date').text($('.order-date').text().replace('Ngày tạo: ', ''));
    $('#invoice-staff-name').text($('.staff-name').text().replace('Nhân viên: ', ''));

    // Thông tin khách hàng
    $('#invoice-customer-name').text($('.form-group:contains("Họ và tên khách hàng") input').val());
    $('#invoice-customer-phone').text($('.form-group:contains("Số điện thoại") input').val());
    const address = `${$('#wardInput').val()}, ${$('#districtInput').val()}, ${$('#cityInput').val()}, ${$('input[name="DuongSoNha"]').val()}`;
    $('#invoice-customer-address').text(address);

    // Thông tin thiết bị
    $('#invoice-device-type').text($('#IdLoaiThietBi option:selected').text());
    $('#invoice-error-type').text($('.error-loi-select option:selected').text());
    $('#invoice-error-description').text($('textarea[name="ErrorDetails[0].MoTaLoi"]').val());

    // Thông tin dịch vụ
    $('#invoice-service-type').text($('#serviceType option:selected').text());
    $('#invoice-service-location').text($('#serviceLocation option:selected').text());
    $('#invoice-technician').text($('.selected-staff .staff-name').text());
    $('#invoice-start-time').text($('#startDatetimePicker').val());
    $('#invoice-end-time').text($('#endDatetimePicker').val());

    // Linh kiện
    const partsList = $('#invoice-parts-list');
    partsList.empty();

    $('.selected-part').each(function() {
        const partName = $(this).find('.part-name').text();
        const partPrice = $(this).find('.part-price').text();
        
        partsList.append(`
            <div class="payment-part-item">
                <div class="payment-part-name">${partName}</div>
                <div class="payment-part-price">${partPrice}</div>
            </div>
        `);
    });

    // Thông tin thanh toán
    $('#invoice-error-cost').text($('.price-input').eq(0).val() + ' VND');
    $('#invoice-parts-cost').text($('.price-input').eq(1).val() + ' VND');
    $('#invoice-advance-payment').text($('#advancePayment').val() + ' VND');
    $('#invoice-remaining-amount').text($('#remainingAmount').text().replace('Tiền còn lại mà khách phải trả là: ', ''));
}

// Xử lý khi nhấn nút xác nhận thanh toán
$(document).ready(function() {
    // Xử lý khi nhấn nút xác nhận thanh toán
    $('.payment-confirm-btn').click(function () {
        // Ẩn modal thanh toán và hiển thị modal chọn phương thức thanh toán
        $('#paymentModal').fadeOut(100, function() {
            // Sau khi ẩn modal thanh toán, hiển thị modal phương thức thanh toán
            $('#paymentMethodModal').fadeIn(100);
        });
    });
      // Xử lý khi chọn phương thức thanh toán
    $('.payment-method-btn').click(function() {
        // Thêm lớp selected vào button được chọn và xóa khỏi các button khác
        $('.payment-method-btn').removeClass('selected');
        $(this).addClass('selected');
        
        const paymentMethod = $(this).data('method');
        let paymentMethodText = paymentMethod === 'cash' ? 'tiền mặt' : 'chuyển khoản';
        
        // Hiển thị thông báo xác nhận
        if(confirm('Bạn có chắc chắn muốn thanh toán bằng ' + paymentMethodText + '?')) {
            // Xử lý logic thanh toán ở đây
            alert('Thanh toán bằng ' + paymentMethodText + ' thành công!');
            $('#paymentMethodModal').fadeOut(200);
            
            // Cập nhật số tiền ứng trước bằng tổng tiền để hiển thị tiền còn lại = 0
            const totalPrice = $('.price-input').eq(1).val().replace(/[^\d]/g, '');
            $('#advancePayment').val(totalPrice).trigger('input');
            
            // Lưu phương thức thanh toán vào một trường ẩn để có thể gửi lên server
            if (!$('#paymentMethod').length) {
                $('<input>').attr({
                    type: 'hidden',
                    id: 'paymentMethod',
                    name: 'PhuongThucThanhToan',
                    value: paymentMethod
                }).appendTo('#serviceOrderForm');
            } else {
                $('#paymentMethod').val(paymentMethod);
            }
        } else {
            // Nếu người dùng chọn Cancel, xóa lớp selected
            $(this).removeClass('selected');
        }
    });
    
    // Đóng popup phương thức thanh toán
    $('.payment-method-popup-close').click(function() {
        $('#paymentMethodModal').fadeOut(200);
    });
    
    // Đóng payment popup khi nhấn nút đóng hoặc nút Hủy
    $('.payment-popup-close, .payment-cancel-btn').click(function () {
        $('#paymentModal').fadeOut(200);
    });

    // Đóng popup khi click bên ngoài
    $(window).click(function (event) {
        const paymentModal = $('#paymentModal')[0];
        const methodModal = $('#paymentMethodModal')[0];
        
        if (event.target === paymentModal) {
            $('#paymentModal').fadeOut(200);
        }
        
        if (event.target === methodModal) {
            $('#paymentMethodModal').fadeOut(200);
        }
    });
});
