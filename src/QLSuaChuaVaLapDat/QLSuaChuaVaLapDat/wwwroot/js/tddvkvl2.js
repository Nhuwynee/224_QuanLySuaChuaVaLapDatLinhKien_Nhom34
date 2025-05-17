// Hàm xác nhận quay lại danh sách đơn
function confirmBack() {
    if (confirm("Bạn có chắc chắn muốn quay lại danh sách không?")) {
        window.location.href = backToListUrl;
    }
}

// Xử lý chức năng tìm kiếm linh kiện
$(document).ready(function() {
    const searchInput = $('#searchPartsInput');
    const partsDropdown = $('#partsDropdown');
    const selectedPartsContainer = $('#selectedPartsContainer');
    let typingTimer;
    const doneTypingInterval = 500; // 500ms
    let selectedParts = [];
    
    // Add event handler for the add-part-btn
    $(document).on('click', '.add-part-btn', function() {
        // Clone the first split-with-button element (without its events)
        let newComponentSection = $('.split-with-button').first().clone(false);
        
        // Clear any input values in the cloned section
        newComponentSection.find('input[type="text"]').val('');        newComponentSection.find('textarea').val('');
        newComponentSection.find('input[type="radio"]').prop('checked', false);
        newComponentSection.find('select').val('');
        newComponentSection.find('input[type="text"]').val('0');
        
        // Fix radio button IDs and names to make them unique
        let timestamp = new Date().getTime();
        newComponentSection.find('input[type="radio"]').each(function() {
            let oldId = $(this).attr('id');
            let newId = oldId + '-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('name', 'warranty-' + timestamp);
            $(this).next('label').attr('for', newId);
        });
        
        // Update select elements to have unique IDs and add event handlers
        newComponentSection.find('select#IdLoi').each(function() {
            let newId = 'IdLoi-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('onchange', 'layGiaTheoLoi(this.value, "donGia-' + timestamp + '")');
        });
          // Update price input to have a unique ID
        newComponentSection.find('input#donGia').each(function() {
            let newId = 'donGia-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('name', 'donGia');
            $(this).addClass('price-field');
        });
        
        // Add event to recalculate total price when a new component is added
        setTimeout(function() {
            updateTotalPrice();
        }, 100);
        
        // Create a buttons container with both + and X buttons
        let buttonsContainer = $('<div class="buttons-container"></div>');
        let addBtn = $('<button type="button" class="add-part-btn" title="Thêm phần mới"><i class="fa-solid fa-plus"></i></button>');
        let removeBtn = $('<button type="button" class="remove-part-btn" title="Xóa phần này"><i class="fa-solid fa-xmark"></i></button>');
        
        // Add both buttons to the container
        buttonsContainer.append(addBtn).append(removeBtn);
        
        // Replace the existing buttons container with the new one
        newComponentSection.find('.buttons-container').replaceWith(buttonsContainer);
        
        // Insert the new section after the last split-with-button
        $('.split-with-button').last().after(newComponentSection);
        
        // Initialize dropdowns in the new section
        initDropdowns(newComponentSection);
    });    // Event delegation for remove buttons (for dynamically added elements)
    $(document).on('click', '.remove-part-btn', function() {
        // Only remove if it's not the first section
        if ($('.split-with-button').length > 1) {
            $(this).closest('.split-with-button').remove();
            
            // Cập nhật tổng giá sau khi xóa thành phần
            setTimeout(function() {
                if (typeof updateTotalPrice === 'function') {
                    updateTotalPrice();
                }
            }, 100);
        } else {
            alert('Không thể xóa phần đầu tiên');
        }
    });
    
    // Function to initialize dropdowns in a container
    function initDropdowns(container) {
        container.find('.dropdown input[readonly]').click(function() {
            $(this).siblings('.dropdown-content').toggle();
        });
        
        container.find('.dropdown-item').click(function() {
            let selectedValue = $(this).text();
            $(this).closest('.dropdown').find('input').val(selectedValue);
            $(this).closest('.dropdown-content').hide();
        });
    }

    // Bắt sự kiện khi người dùng gõ vào ô tìm kiếm
    searchInput.on('input', function() {
        clearTimeout(typingTimer);
        const keyword = $(this).val().trim();
        
        if (keyword.length > 0) {
            typingTimer = setTimeout(() => searchParts(keyword), doneTypingInterval);
        } else {
            partsDropdown.empty().hide();
        }
    });

    // Hàm tìm kiếm linh kiện
    function searchParts(keyword) {
        $.ajax({
            url: searchLinhKienUrl,
            type: 'GET',
            data: { keyword: keyword },
            success: function(data) {
                displaySearchResults(data);
            },
            error: function(err) {
                console.error("Lỗi khi tìm kiếm linh kiện:", err);
            }
        });
    }

    // Hiển thị kết quả tìm kiếm
    function displaySearchResults(results) {
        partsDropdown.empty();
        
        if (results && results.length > 0) {
            results.forEach(item => {
                const resultItem = $('<div class="part-result-item"></div>');
                resultItem.html(`
                    <div class="part-name">${item.ten}</div>
                    <div class="part-info">
                        <span class="part-quantity">SL: ${item.soLuong}</span> | 
                        <span class="part-price">Giá: ${formatCurrency(item.gia)}</span> |
                        <span class="part-manufacturer">NSX: ${item.tenNSX}</span>
                    </div>
                `);

                // Thêm sự kiện khi click vào kết quả
                resultItem.on('click', function() {
                    addSelectedPart(item);
                    searchInput.val('');
                    partsDropdown.hide();
                });

                partsDropdown.append(resultItem);
            });
            partsDropdown.show();
        } else {
            partsDropdown.html('<div class="no-results">Không tìm thấy linh kiện nào</div>');
            partsDropdown.show();
        }
    }

    // Thêm linh kiện đã chọn vào danh sách
    function addSelectedPart(part) {
        if (!selectedParts.some(p => p.id === part.id)) {
            selectedParts.push(part);
            
            const partElement = $('<div class="selected-part"></div>');
            partElement.attr('data-id', part.id);
            partElement.html(`
                <span class="part-name">${part.ten}</span>
                <span class="part-details">
                    <span class="part-quantity">SL: ${part.soLuong}</span> | 
                    <span class="part-price">Giá: ${formatCurrency(part.gia)}</span> | 
                    <span class="part-manufacturer">NSX: ${part.tenNSX}</span>
                </span>
                <button class="remove-part" title="Xóa linh kiện này"><i class="fas fa-times"></i></button>
            `);

            // Thêm sự kiện xóa linh kiện
            partElement.find('.remove-part').on('click', function(e) {
                e.stopPropagation();
                removeSelectedPart(part.id);
            });

            selectedPartsContainer.append(partElement);
        }
    }

    // Xóa linh kiện khỏi danh sách đã chọn
    function removeSelectedPart(partId) {
        selectedParts = selectedParts.filter(p => p.id !== partId);
        selectedPartsContainer.find(`[data-id="${partId}"]`).remove();
    }

    // Định dạng tiền tệ
    function formatCurrency(value) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(value);
    }    // Đóng dropdown khi click ra ngoài
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
    deviceImageButton.addEventListener('click', function() {
        deviceImageInput.click();
    });
    
    // Khi nhấn nút "Chọn tệp" cho ảnh phiếu bảo hành
    warrantyImageButton.addEventListener('click', function() {
        warrantyImageInput.click();
    });
    
    // Khi chọn ảnh thiết bị
    deviceImageInput.addEventListener('change', function(event) {
        handleImageUpload(event, deviceImagePreview);
    });
    
    // Khi chọn ảnh phiếu bảo hành
    warrantyImageInput.addEventListener('change', function(event) {
        handleImageUpload(event, warrantyImagePreview);
    });

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
            
            if (file.type.match('image.*')) {
                const reader = new FileReader();
                
                reader.onload = function(e) {
                    const fileItem = document.createElement('div');
                    fileItem.className = 'file-item';
                    
                    // Tạo ảnh xem trước
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    // Thêm sự kiện click để xem ảnh lớn
                    img.addEventListener('click', function() {
                        showImageInModal(e.target.result);
                    });
                    fileItem.appendChild(img);
                    
                    // Tạo thông tin file
                    const fileInfo = document.createElement('div');
                    fileInfo.className = 'file-info';
                    fileInfo.textContent = file.name;
                    fileItem.appendChild(fileInfo);
                    
                    // Tạo nút xóa
                    const deleteButton = document.createElement('div');
                    deleteButton.className = 'delete-file';
                    deleteButton.innerHTML = '<i class="fa-solid fa-xmark"></i>';
                    deleteButton.addEventListener('click', function(e) {
                        e.stopPropagation(); // Ngăn chặn sự kiện click lan đến ảnh
                        fileItem.remove();
                    });
                    fileItem.appendChild(deleteButton);
                    
                    // Thêm vào khu vực xem trước
                    previewElement.appendChild(fileItem);
                };
                
                reader.readAsDataURL(file);
            }
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
    // Nếu không có targetId, sử dụng ID mặc định 'donGia'
    const priceFieldId = targetId || 'donGia';
    
    if (!idLoi) {
        document.getElementById(priceFieldId).value = "0";
        return;
    }
    
    fetch(layGiaTheoLoiUrl + '?idLoi=' + idLoi)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById(priceFieldId).value = data.giaFormatted;
                
                // Cập nhật tổng tiền
                updateTotalPrice();
            } else {
                document.getElementById(priceFieldId).value = "0";
                console.error(data.message);
            }
        })
        .catch(error => {
            console.error('Lỗi khi lấy giá:', error);
            document.getElementById(priceFieldId).value = "0";
        });
}

// Hàm để cập nhật tổng tiền
function updateTotalPrice() {
    const priceInput = document.querySelector('.price-input');
    if (priceInput) {
        // Tìm tất cả các trường đơn giá (bao gồm các trường được thêm động)
        const donGiaFields = document.querySelectorAll('.price-field');
        let totalPrice = 0;
        
        // Tính tổng giá từ tất cả các thành phần
        donGiaFields.forEach(field => {
            // Chuyển đổi chuỗi giá có định dạng (ví dụ: "1.000.000") thành số
            const priceText = field.value.replace(/\./g, '').replace(/,/g, '');
            const price = parseInt(priceText) || 0;
            totalPrice += price;
        });
        
        // Định dạng lại tổng tiền
        priceInput.value = new Intl.NumberFormat('vi-VN').format(totalPrice);
    }
}

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
            staffPopup.classList.add('show');
        });
    }
    
    // Clear selected staff when clicking the clear button
    if (clearStaffBtn) {
        clearStaffBtn.addEventListener('click', function() {
            // Reset the staff selection
            selectedStaffDisplay.querySelector('.staff-name').textContent = 'Chưa chọn nhân viên';
            selectedStaffDisplay.classList.remove('has-staff');
            selectedStaffDisplay.removeAttribute('data-staff-id');
            selectedStaffDisplay.removeAttribute('data-specialty');
            selectedStaffDisplay.removeAttribute('data-staff-info');
            
            // Clear the hidden input
            if (selectedStaffIdInput) {
                selectedStaffIdInput.value = '';
            }
            
            // Hide the clear button
            clearStaffBtn.style.display = 'none';
        });
    }
    
    // Close popup when clicking outside
    if (staffPopup) {
        staffPopup.addEventListener('click', function(e) {
            if (e.target === staffPopup) {
                staffPopup.classList.remove('show');
            }
        });
    }
    
    // Handle specialty selection change
    if (specialtySelect) {
        specialtySelect.addEventListener('change', function() {
            const specialty = this.value;
            
            if (specialty) {
                fetchStaffBySpecialty(specialty);
            } else {
                staffListSection.innerHTML = '<div class="no-staff">Vui lòng chọn chuyên môn</div>';
            }
        });
    }
    
    // Fetch available specialties from the server
    function fetchAndPopulateSpecialties() {
        $.ajax({
            url: '/TaoDonDichVuKVL/LayDanhSachChuyenMon',
            method: 'GET',
            success: function(data) {
                populateSpecialtyDropdown(data);
            },
            error: function(err) {
                console.error('Lỗi khi lấy danh sách chuyên môn:', err);
            }
        });
    }
    
    // Populate the specialty dropdown with data from the server
    function populateSpecialtyDropdown(specialties) {
        if (specialtySelect && specialties && specialties.length > 0) {
            // Clear existing options except the first one
            while (specialtySelect.options.length > 1) {
                specialtySelect.remove(1);
            }
            
            // Add new options
            specialties.forEach(specialty => {
                if (specialty) { // Make sure specialty is not null or empty
                    const option = document.createElement('option');
                    option.value = specialty;
                    option.textContent = specialty;
                    specialtySelect.appendChild(option);
                }
            });
        }
    }
    
    // Fetch staff by specialty from the server
    function fetchStaffBySpecialty(specialty) {
        $.ajax({
            url: '/TaoDonDichVuKVL/LayNhanVienTheoChuyenMon',
            method: 'GET',
            data: { chuyenMon: specialty },
            success: function(data) {
                populateStaffList(data);
            },
            error: function(err) {
                console.error('Lỗi khi lấy danh sách nhân viên:', err);
            }
        });
    }    // Populate the staff list with data from the server
    function populateStaffList(staffList) {
        if (staffListSection) {
            staffListSection.innerHTML = '';
            
            if (staffList && staffList.length > 0) {
                staffList.forEach(staff => {
                    const staffItem = document.createElement('div');
                    staffItem.className = 'staff-item available';
                    staffItem.setAttribute('data-id', staff.idUser);
                    staffItem.setAttribute('data-specialty', staff.chuyenMon);
                    
                    // Create staff details with name, specialty and contact
                    const staffSpan = document.createElement('span');
                    const staffInfo = document.createElement('div');
                    staffInfo.className = 'staff-info';
                    
                    const staffName = document.createElement('div');
                    staffName.className = 'staff-name-info';
                    staffName.textContent = staff.hoVaTen;
                    
                    const staffSpecialty = document.createElement('div');
                    staffSpecialty.className = 'staff-specialty';
                    staffSpecialty.textContent = `Chuyên môn: ${staff.chuyenMon}`;
                    
                    const staffContact = document.createElement('div');
                    staffContact.className = 'staff-contact';
                    staffContact.textContent = `SĐT: ${staff.sdt}`;
                    
                    staffInfo.appendChild(staffName);
                    staffInfo.appendChild(staffSpecialty);
                    staffInfo.appendChild(staffContact);
                    staffSpan.appendChild(staffInfo);
                    
                    staffItem.appendChild(staffSpan);
                    staffListSection.appendChild(staffItem);
                      // Add click event to select staff
                    staffItem.addEventListener('click', function() {
                        const staffId = this.getAttribute('data-id');
                        const staffNameElem = this.querySelector('.staff-name-info');
                        const staffSpecialtyText = this.querySelector('.staff-specialty').textContent;
                        const specialty = staffSpecialtyText.replace('Chuyên môn: ', '');
                        
                        // Format the display text: Name - Specialty
                        const displayText = `${staffNameElem.textContent} - Chuyên môn: ${specialty}`;
                        
                        selectedStaffDisplay.querySelector('.staff-name').textContent = displayText;
                        selectedStaffDisplay.classList.add('has-staff');
                        selectedStaffDisplay.setAttribute('data-staff-id', staffId);
                        selectedStaffDisplay.setAttribute('data-specialty', specialty);
                        
                        // Store complete staff information for form submission
                        const staffData = {
                            id: staffId,
                            name: staffNameElem.textContent,
                            specialty: specialty,
                            contact: this.querySelector('.staff-contact').textContent.replace('SĐT: ', '')
                        };
                          // Store as data attribute (serialized)
                        selectedStaffDisplay.setAttribute('data-staff-info', JSON.stringify(staffData));
                        
                        // Set the hidden input field value
                        const selectedStaffIdInput = document.getElementById('selectedStaffId');
                        if (selectedStaffIdInput) {
                            selectedStaffIdInput.value = staffId;
                        }
                        
                        // Show the clear button
                        const clearStaffBtn = document.querySelector('.clear-staff-btn');
                        if (clearStaffBtn) {
                            clearStaffBtn.style.display = 'block';
                        }
                        
                        // Close the popup
                        staffPopup.classList.remove('show');
                    });
                });            } else {
                staffListSection.innerHTML = '<div class="no-staff">Không tìm thấy nhân viên với chuyên môn này</div>';
            }
        }
    }
});

// Initialize datepickers when the document is ready
$(document).ready(function() {
    // Initialize flatpickr for date/time pickers
    if (typeof flatpickr !== 'undefined') {
        flatpickr("#startDatetimePicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i:S",
            time_24hr: true,
            locale: "vn"
        });
        
        flatpickr("#endDatetimePicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i:S", 
            time_24hr: true,
            locale: "vn"
        });
    }
    
    // Initialize dropdowns
    $('.dropdown input[readonly]').click(function() {
        $(this).siblings('.dropdown-content').toggle();
    });
    
    $('.dropdown-item').click(function() {
        let selectedValue = $(this).text();
        $(this).closest('.dropdown').find('input').val(selectedValue);
        $(this).closest('.dropdown-content').hide();
    });
    
    // Close dropdowns when clicking outside
    $(document).click(function(e) {
        if (!$(e.target).closest('.dropdown').length) {
            $('.dropdown-content').hide();
        }
    });
    
    // Xử lý chọn địa chỉ theo thứ tự thành phố > quận/huyện > phường/xã
    initLocationSelectors();
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
    fetchCitiesFromDatabase();    // Hàm mở dropdown thành phố
    function toggleCityDropdown() {
        // Đóng tất cả các dropdown khác
        closeAllDropdowns();
        // Hiển thị dropdown thành phố
        cityDropdown.style.display = 'block';
        // Thêm lớp active cho input và icon
        cityInput.classList.add('input-active');
        cityDropdownIcon.classList.add('active');
    }
    
    // Hàm mở dropdown quận/huyện
    function toggleDistrictDropdown() {
        if (!cityIdInput.value) {
            showErrorMessage('districtError', 'Vui lòng chọn Thành phố/Tỉnh trước');
            return;
        }
        
        // Đóng tất cả các dropdown khác
        closeAllDropdowns();
        hideErrorMessage('districtError');
        
        // Nếu chưa có dữ liệu quận/huyện, tải từ API
        if (districtDropdown.children.length === 0) {
            fetchDistrictsByCity(cityIdInput.value);
        }
        
        // Hiển thị dropdown quận/huyện
        districtDropdown.style.display = 'block';
        // Thêm lớp active cho input và icon
        districtInput.classList.add('input-active');
        districtDropdownIcon.classList.add('active');
    }
    
    // Hàm mở dropdown phường/xã
    function toggleWardDropdown() {
        if (!districtIdInput.value) {
            showErrorMessage('wardError', 'Vui lòng chọn Quận/Huyện trước');
            return;
        }
        
        // Đóng tất cả các dropdown khác
        closeAllDropdowns();
        hideErrorMessage('wardError');
        
        // Nếu chưa có dữ liệu phường/xã, tải từ API
        if (wardDropdown.children.length === 0) {
            fetchWardsByDistrict(districtIdInput.value);
        }
        
        // Hiển thị dropdown phường/xã
        wardDropdown.style.display = 'block';
        // Thêm lớp active cho input và icon
        wardInput.classList.add('input-active');
        wardDropdownIcon.classList.add('active');
    }
    
    // Event listeners cho việc nhấn vào icon dropdown
    cityDropdownIcon.addEventListener('click', toggleCityDropdown);
    districtDropdownIcon.addEventListener('click', toggleDistrictDropdown);
    wardDropdownIcon.addEventListener('click', toggleWardDropdown);
    
    // Event listeners cho việc nhấn vào input
    cityInput.addEventListener('click', toggleCityDropdown);
    districtInput.addEventListener('click', toggleDistrictDropdown);
    wardInput.addEventListener('click', toggleWardDropdown);
    
    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', function(event) {
        if (!event.target.closest('.input-with-icon')) {
            closeAllDropdowns();
        }
    });
    
    // Ngăn chặn sự kiện click trên dropdown để không lan đến document
    cityDropdown.addEventListener('click', function(event) {
        event.stopPropagation();
    });
    
    districtDropdown.addEventListener('click', function(event) {
        event.stopPropagation();
    });
    
    wardDropdown.addEventListener('click', function(event) {
        event.stopPropagation();
    });
}

// Đóng tất cả các dropdown
function closeAllDropdowns() {
    const dropdowns = document.querySelectorAll('.custom-dropdown');
    dropdowns.forEach(dropdown => {
        dropdown.style.display = 'none';
    });
    
    // Xóa các lớp active
    document.querySelectorAll('.input-active').forEach(input => {
        input.classList.remove('input-active');
    });
    
    document.querySelectorAll('.location-dropdown-icon.active').forEach(icon => {
        icon.classList.remove('active');
    });
}

// Hiển thị thông báo lỗi
function showErrorMessage(errorId, message) {
    const errorSpan = document.getElementById(errorId);
    if (errorSpan) {
        errorSpan.textContent = message;
        errorSpan.style.display = 'block';
    }
}

// Ẩn thông báo lỗi
function hideErrorMessage(errorId) {
    const errorSpan = document.getElementById(errorId);
    if (errorSpan) {
        errorSpan.style.display = 'none';
    }
}

// Lấy danh sách thành phố từ database
function fetchCitiesFromDatabase() {
    const cityDropdown = document.getElementById('cityDropdown');
    
    // Hiển thị loading indicator
    cityDropdown.innerHTML = '<div class="loading-indicator"><div class="loading-spinner"></div>Đang tải dữ liệu...</div>';
    
    fetch(layDanhSachThanhPhoUrl)
        .then(response => response.json())
        .then(data => {
            populateCityDropdown(data);
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách thành phố:', error);
            cityDropdown.innerHTML = '<div class="dropdown-item" style="color: #ff4d4f;">Không thể tải dữ liệu. Vui lòng thử lại.</div>';
        });
}

// Hiển thị danh sách thành phố trong dropdown
function populateCityDropdown(cities) {
    const cityDropdown = document.getElementById('cityDropdown');
    
    // Xóa các item hiện có
    cityDropdown.innerHTML = '';
    
    // Thêm các item mới
    cities.forEach(city => {
        const item = document.createElement('div');
        item.className = 'dropdown-item';
        item.textContent = city.ten;
        item.setAttribute('data-id', city.id);
        
        // Xử lý sự kiện khi chọn thành phố
        item.addEventListener('click', function() {
            // Cập nhật input và id
            document.getElementById('cityInput').value = city.ten;
            document.getElementById('cityId').value = city.id;
            
            // Đánh dấu item được chọn
            const items = cityDropdown.querySelectorAll('.dropdown-item');
            items.forEach(i => i.classList.remove('selected'));
            item.classList.add('selected');
            
            // Đóng dropdown
            cityDropdown.style.display = 'none';
            
            // Reset quận/huyện và phường/xã
            resetDistrictAndWard();
            
            // Tải danh sách quận/huyện
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
    
    document.getElementById('districtDropdown').innerHTML = '';
    document.getElementById('wardDropdown').innerHTML = '';
    
    hideErrorMessage('districtError');
    hideErrorMessage('wardError');
}

// Lấy danh sách quận/huyện theo thành phố từ database
function fetchDistrictsByCity(cityId) {
    const districtDropdown = document.getElementById('districtDropdown');
    
    // Hiển thị loading indicator
    districtDropdown.innerHTML = '<div class="loading-indicator"><div class="loading-spinner"></div>Đang tải dữ liệu...</div>';
    
    fetch(`${layDanhSachQuanUrl}?idThanhPho=${cityId}`)
        .then(response => response.json())
        .then(data => {
            populateDistrictDropdown(data);
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách quận/huyện:', error);
            districtDropdown.innerHTML = '<div class="dropdown-item" style="color: #ff4d4f;">Không thể tải dữ liệu. Vui lòng thử lại.</div>';
        });
}

// Hiển thị danh sách quận/huyện trong dropdown
function populateDistrictDropdown(districts) {
    const districtDropdown = document.getElementById('districtDropdown');
    
    // Xóa các item hiện có
    districtDropdown.innerHTML = '';
    
    if (districts.length === 0) {
        const noData = document.createElement('div');
        noData.className = 'dropdown-item';
        noData.textContent = 'Không có dữ liệu';
        noData.style.fontStyle = 'italic';
        noData.style.color = '#999';
        districtDropdown.appendChild(noData);
        return;
    }
    
    // Thêm các item mới
    districts.forEach(district => {
        const item = document.createElement('div');
        item.className = 'dropdown-item';
        item.textContent = district.ten;
        item.setAttribute('data-id', district.id);
        
        // Xử lý sự kiện khi chọn quận/huyện
        item.addEventListener('click', function() {
            // Cập nhật input và id
            document.getElementById('districtInput').value = district.ten;
            document.getElementById('districtId').value = district.id;
            
            // Đánh dấu item được chọn
            const items = districtDropdown.querySelectorAll('.dropdown-item');
            items.forEach(i => i.classList.remove('selected'));
            item.classList.add('selected');
            
            // Đóng dropdown
            districtDropdown.style.display = 'none';
            
            // Reset phường/xã
            document.getElementById('wardInput').value = '';
            document.getElementById('wardId').value = '';
            document.getElementById('wardDropdown').innerHTML = '';
            
            // Tải danh sách phường/xã
            fetchWardsByDistrict(district.id);
        });
        
        districtDropdown.appendChild(item);
    });
}

// Lấy danh sách phường/xã theo quận/huyện từ database
function fetchWardsByDistrict(districtId) {
    const wardDropdown = document.getElementById('wardDropdown');
    
    // Hiển thị loading indicator
    wardDropdown.innerHTML = '<div class="loading-indicator"><div class="loading-spinner"></div>Đang tải dữ liệu...</div>';
    
    fetch(`${layDanhSachPhuongUrl}?idQuan=${districtId}`)
        .then(response => response.json())
        .then(data => {
            populateWardDropdown(data);
        })
        .catch(error => {
            console.error('Lỗi khi lấy danh sách phường/xã:', error);
            wardDropdown.innerHTML = '<div class="dropdown-item" style="color: #ff4d4f;">Không thể tải dữ liệu. Vui lòng thử lại.</div>';
        });
}

// Hiển thị danh sách phường/xã trong dropdown
function populateWardDropdown(wards) {
    const wardDropdown = document.getElementById('wardDropdown');
    
    // Xóa các item hiện có
    wardDropdown.innerHTML = '';
    
    if (wards.length === 0) {
        const noData = document.createElement('div');
        noData.className = 'dropdown-item';
        noData.textContent = 'Không có dữ liệu';
        noData.style.fontStyle = 'italic';
        noData.style.color = '#999';
        wardDropdown.appendChild(noData);
        return;
    }
    
    // Thêm các item mới
    wards.forEach(ward => {
        const item = document.createElement('div');
        item.className = 'dropdown-item';
        item.textContent = ward.ten;
        item.setAttribute('data-id', ward.id);
        
        // Xử lý sự kiện khi chọn phường/xã
        item.addEventListener('click', function() {
            // Cập nhật input và id
            document.getElementById('wardInput').value = ward.ten;
            document.getElementById('wardId').value = ward.id;
            
            // Đánh dấu item được chọn
            const items = wardDropdown.querySelectorAll('.dropdown-item');
            items.forEach(i => i.classList.remove('selected'));
            item.classList.add('selected');
            
            // Đóng dropdown
            wardDropdown.style.display = 'none';
        });
        
        wardDropdown.appendChild(item);
    });
}

// Event handler for the Create Order button
document.addEventListener('DOMContentLoaded', function() {
    const createOrderBtn = document.querySelector('.create-order-btn');    if (createOrderBtn) {
        createOrderBtn.addEventListener('click', function() {
            // Get the service order ID from the hidden input
            const serviceOrderId = document.getElementById('IdDonDichVu').value;
            
            // Collect all form data
            const formData = new FormData();
            formData.append('IdDonDichVu', serviceOrderId);
            
            // Add other form fields (this would need to be expanded based on the actual form fields)
            // Example: Customer info, device info, service details, etc.
            const customerName = document.getElementById('customerName').value;
            const customerPhone = document.getElementById('customerPhone').value;
            const customerEmail = document.getElementById('customerEmail').value;
            
            // For demonstration, show a confirmation that includes the ID
            if(confirm(`Xác nhận tạo đơn dịch vụ ${serviceOrderId}?`)) {
                // In a production environment, we would submit all the form data
                // For now, we'll just show success and redirect
                
                // Example of form submission (commented out since we don't have the complete form)
                /*
                fetch(createServiceOrderUrl, {
                    method: 'POST',
                    body: formData
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert('Đơn dịch vụ đã được tạo thành công!');
                        window.location.href = backToListUrl;
                    } else {
                        alert('Có lỗi xảy ra: ' + data.message);
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('Có lỗi xảy ra khi tạo đơn dịch vụ!');
                });
                */
                
                // For this demonstration, we'll just show a success message and redirect
                alert(`Đơn dịch vụ ${serviceOrderId} đã được tạo thành công!`);
                window.location.href = backToListUrl;
            }
        });
    }
});