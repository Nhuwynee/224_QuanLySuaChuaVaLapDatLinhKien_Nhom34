// Hàm xác nhận quay lại danh sách đơn
function confirmBack() {
    if (confirm("Bạn có chắc chắn muốn quay lại danh sách không?")) {
        window.location.href = backToListUrl;
    }
}
// Định dạng tiền tệ
function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(value);
}
// Xử lý chức năng tìm kiếm linh kiện
$(document).ready(function () {
    const searchInput = $('#searchPartsInput');
    const partsDropdown = $('#partsDropdown');
    const selectedPartsContainer = $('#selectedPartsContainer');
    let typingTimer;
    const doneTypingInterval = 500; // 500ms
    let selectedParts = [];

    // Thêm API endpoint để lấy linh kiện theo loại
    $(document).ready(function () {
        // Xử lý sự kiện khi thay đổi loại linh kiện
        $(document).on('change', '.select-linhkien', function () {
            const selectedTypeId = $(this).val();
            const linhKienSelect = $(this).closest('.form-group').parent().find('.part-select');

            if (selectedTypeId) {
                // Tải danh sách linh kiện theo loại
                $.ajax({
                    url: layLinhKienUrl,
                    type: 'GET',
                    data: { idLoaiLinhKien: selectedTypeId },
                    success: function (data) {
                        // Xóa các option cũ
                        linhKienSelect.empty();

                        // Thêm option mặc định
                        linhKienSelect.append('<option value="">Chọn linh kiện</option>');

                        // Thêm các linh kiện mới
                        $.each(data, function (index, item) {
                            linhKienSelect.append(`<option value="${item.idLinhKien}" 
                            data-gia="${item.gia}" 
                            data-nsx="${item.tenNsx}"
                            data-soluong="${item.soLuong}">${item.tenLinhKien}</option>`);
                        });
                    },
                    error: function () {
                        console.log('Lỗi khi tải danh sách linh kiện');
                    }
                });
            } else {
                // Nếu không chọn loại, xóa danh sách linh kiện
                linhKienSelect.empty();
                linhKienSelect.append('<option value="">Chọn linh kiện</option>');
            }
        });
    });

    // Add event handler for the add-part-btn
    $(document).on('click', '.add-part-btn', function () {
        // Clone the first split-with-button element (without its events)
        let newComponentSection = $('.split-with-button').first().clone(true);

        // Clear any input values in the cloned section
        newComponentSection.find('input[type="text"]').val('');
        newComponentSection.find('textarea').val('');
        newComponentSection.find('input[type="radio"]').prop('checked', false);
        newComponentSection.find('select').val('');
        newComponentSection.find('input[type="text"]').val('0');

        // Fix radio button IDs and names to make them unique
        let timestamp = new Date().getTime();
        newComponentSection.find('input[type="radio"]').each(function () {
            let oldId = $(this).attr('id');
            let newId = oldId + '-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('name', 'warranty-' + timestamp);
            $(this).next('label').attr('for', newId);
            // Set default "hết bảo hành" option as checked
            if ($(this).val() === 'false') {
                $(this).prop('checked', true);
            }
        });

        // Cập nhật ID cho select loại linh kiện
        newComponentSection.find('select.part-type-select').each(function () {
            let newId = 'partType-' + timestamp;
            $(this).attr('id', newId);
            $(this).val('');
        });

        // Cập nhật ID cho select linh kiện
        newComponentSection.find('select.part-select').each(function () {
            let newId = 'part-' + timestamp;
            $(this).attr('id', newId);
            $(this).val('');
            $(this).empty().append('<option value="">Chọn linh kiện</option>');
        });


        // Update select elements to have unique IDs and add event handlers
        newComponentSection.find('select#IdLoi').each(function () {
            let newId = 'IdLoi-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('name', `ErrorDetails[${$('.split-with-button').length}].IdLoi`);
            $(this).attr('onchange', `layGiaTheoLoi(this.value, "donGia-${timestamp}")`);
        });
        // Update price input to have a unique ID
        newComponentSection.find('input#donGia').each(function () {
            let newId = 'donGia-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('name', 'donGia');
            $(this).addClass('price-field');
        });

        // Add event to recalculate total price when a new component is added
        setTimeout(function () {
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
    $(document).on('click', '.remove-part-btn', function () {
        // Only remove if it's not the first section
        if ($('.split-with-button').length > 1) {
            $(this).closest('.split-with-button').remove();

            // Cập nhật tổng giá sau khi xóa thành phần
            setTimeout(function () {
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
        container.find('.dropdown input[readonly]').click(function () {
            $(this).siblings('.dropdown-content').toggle();
        });

        container.find('.dropdown-item').click(function () {
            let selectedValue = $(this).text();
            $(this).closest('.dropdown').find('input').val(selectedValue);
            $(this).closest('.dropdown-content').hide();
        });
    }

    // Bắt sự kiện khi người dùng gõ vào ô tìm kiếm
    searchInput.on('input', function () {
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
            success: function (data) {
                displaySearchResults(data);
            },
            error: function (err) {
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
                resultItem.on('click', function () {
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
            partElement.find('.remove-part').on('click', function (e) {
                e.stopPropagation();
                removeSelectedPart(part.id);
            });

            selectedPartsContainer.append(partElement);
            // Cập nhật tổng tiền sau khi thêm linh kiện
            updateTotalPrice();
        }
    }

    // Xóa linh kiện khỏi danh sách đã chọn
    function removeSelectedPart(partId) {
        selectedParts = selectedParts.filter(p => p.id !== partId);
        selectedPartsContainer.find(`[data-id="${partId}"]`).remove();

        // Cập nhật tổng tiền sau khi xóa linh kiện
        updateTotalPrice();
    }


    // Đóng dropdown khi click ra ngoài
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.search-parts-container').length) {
            partsDropdown.hide();
        }
    });
});

// Xử lý tải ảnh thiết bị và ảnh phiếu bảo hành        
$(document).ready(function () {
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
    deviceImageButton.addEventListener('click', function () {
        deviceImageInput.click();
    });

    // Khi nhấn nút "Chọn tệp" cho ảnh phiếu bảo hành
    warrantyImageButton.addEventListener('click', function () {
        warrantyImageInput.click();
    });

    // Khi chọn ảnh thiết bị
    deviceImageInput.addEventListener('change', function (event) {
        handleImageUpload(event, deviceImagePreview);
    });

    // Khi chọn ảnh phiếu bảo hành
    warrantyImageInput.addEventListener('change', function (event) {
        handleImageUpload(event, warrantyImagePreview);
    });

    // Xử lý ban đầu cho các ảnh demo đã có sẵn
    document.querySelectorAll('.file-preview .delete-file').forEach(function (deleteBtn) {
        deleteBtn.addEventListener('click', function () {
            this.parentElement.remove();
        });
    });

    // Xử lý đóng modal
    const modal = document.getElementById('imageModal');
    const closeButton = document.querySelector('.modal-close');

    // Đóng modal khi nhấn nút X
    if (closeButton) {
        closeButton.addEventListener('click', function () {
            modal.style.display = "none";
        });
    }

    // Đóng modal khi nhấn bên ngoài ảnh
    if (modal) {
        modal.addEventListener('click', function (event) {
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

                reader.onload = function (e) {
                    const fileItem = document.createElement('div');
                    fileItem.className = 'file-item';

                    // Tạo ảnh xem trước
                    const img = document.createElement('img');
                    img.src = e.target.result;
                    // Thêm sự kiện click để xem ảnh lớn
                    img.addEventListener('click', function () {
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
                    deleteButton.addEventListener('click', function (e) {
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
    // Tìm tất cả các trường đơn giá (bao gồm các trường được thêm động)
    const donGiaFields = document.querySelectorAll('.price-field');
    let totalPrice = 0;

    // Tính tổng giá từ tất cả các thành phần
    donGiaFields.forEach(field => {
        // Chuyển đổi chuỗi giá Việt (ví dụ: "1.000.000") thành số nguyên
        const raw = field.value || '';
        const priceText = raw.replace(/\./g, '').replace(/,/g, ''); // bỏ định dạng
        const price = parseInt(priceText) || 0;
        totalPrice += price;
    });

    // Cập nhật input "Tiền lỗi" (chính là tổng của các price-field)
    const errorPriceInput = document.querySelectorAll('.price-input')[0];
    if (errorPriceInput) {
        errorPriceInput.value = new Intl.NumberFormat('vi-VN').format(totalPrice);
    }

    // Tính tiền linh kiện (nếu có)
    let partPrice = 0;
    const selectedParts = document.querySelectorAll('.selected-part');
    selectedParts.forEach(part => {
        const priceText = part.querySelector('.part-price').textContent;
        const priceMatch = priceText.match(/[\d.,]+/);
        if (priceMatch) {
            // Xử lý chuỗi giá linh kiện từ định dạng Việt → số thực
            const raw = priceMatch[0];
            const normalized = raw.replace(/\./g, '').replace(/,/g, ''); // ví dụ: 1.200.000 → 1200000
            const price = parseFloat(normalized) || 0;
            partPrice += price;
        }
    });

    // Tổng tiền = Tiền lỗi + Tiền linh kiện
    const finalPrice = totalPrice + partPrice;

    // Cập nhật input "Tổng tiền"
    const totalPriceInput = document.querySelectorAll('.price-input')[1];
    if (totalPriceInput) {
        totalPriceInput.value = new Intl.NumberFormat('vi-VN').format(finalPrice);
    }

    // Tính tiền còn lại = Tổng tiền - Tiền ứng trước
    const advancePaymentInput = document.getElementById('advancePayment');
    const advanceText = advancePaymentInput?.value || '';
    const advanceNormalized = advanceText.replace(/\./g, '').replace(/,/g, '');
    const advancePayment = parseInt(advanceNormalized) || 0;

    const remainingAmount = finalPrice - advancePayment;

    // Cập nhật hiển thị tiền còn lại
    const remainingAmountSpan = document.getElementById('remainingAmount');
    if (remainingAmountSpan) {
        remainingAmountSpan.textContent = `Tiền còn lại mà khách phải trả là: ${new Intl.NumberFormat('vi-VN').format(remainingAmount)} VND`;
    }
}

$(document).ready(function () {
    // Thêm sự kiện để cập nhật tổng tiền khi tiền ứng trước thay đổi
    $('#advancePayment').on('input', function () {
        updateTotalPrice();
    });
});



// Đảm bảo mỗi lỗi đều có input hidden IdLinhKien trước khi submit
function updateErrorDetailPartIds() {
    const errorDetailContainers = document.querySelectorAll('.split-with-button');
    const selectedPartIds = [];
    $('#selectedPartsContainer .selected-part').each(function () {
        selectedPartIds.push($(this).attr('data-id'));
    });

    errorDetailContainers.forEach((container, index) => {
        let idLinhKien = '';
        if (index < selectedPartIds.length) {
            idLinhKien = selectedPartIds[index];
        }
        let hiddenField = container.querySelector(`input[name="ErrorDetails[${index}].IdLinhKien"]`);
        if (!hiddenField) {
            hiddenField = document.createElement('input');
            hiddenField.type = 'hidden';
            hiddenField.name = `ErrorDetails[${index}].IdLinhKien`;
            container.appendChild(hiddenField);
        }
        hiddenField.value = idLinhKien;
    });
}

// Gọi hàm này trước khi submit form
$(document).ready(function () {
    $('#serviceOrderForm').on('submit', function () {
        updateErrorDetailPartIds();
    });
});






// Staff Assignment Popup Functionality
$(document).ready(function () {
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
        selectStaffBtn.addEventListener('click', function () {
            staffPopup.classList.add('show');
        });
    }

    // Clear selected staff when clicking the clear button
    if (clearStaffBtn) {
        clearStaffBtn.addEventListener('click', function () {
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
        staffPopup.addEventListener('click', function (e) {
            if (e.target === staffPopup) {
                staffPopup.classList.remove('show');
            }
        });
    }

    // Handle specialty selection change
    if (specialtySelect) {
        specialtySelect.addEventListener('change', function () {
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
            success: function (data) {
                populateSpecialtyDropdown(data);
            },
            error: function (err) {
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
            success: function (data) {
                populateStaffList(data);
            },
            error: function (err) {
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
                    staffItem.addEventListener('click', function () {
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
                });
            } else {
                staffListSection.innerHTML = '<div class="no-staff">Không tìm thấy nhân viên với chuyên môn này</div>';
            }
        }
    }
});

// Initialize datepickers when the document is ready
$(document).ready(function () {
    // Initialize flatpickr for date/time pickers
    if (typeof flatpickr !== 'undefined') {
        // Default date for both pickers
        const now = new Date();
        const nowStr = now.toISOString().slice(0, 16).replace('T', ' ');

        // Initialize start date picker
        flatpickr("#startDatetimePicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i",  // Format: day-month-year hour:minute
            time_24hr: true,
            locale: "vn",
            allowInput: true,
            clickOpens: true,
            defaultDate: nowStr,
            onChange: function (selectedDates, dateStr) {
                // Update the input value when the date changes
                $('#startDatetimePicker').val(dateStr);
                console.log('Start date changed to: ' + dateStr);
            }
        });

        // Initialize end date picker with date 2 days later
        const twoDaysLater = new Date();
        twoDaysLater.setDate(now.getDate() + 2);
        const twoDaysLaterStr = twoDaysLater.toISOString().slice(0, 16).replace('T', ' ');

        flatpickr("#endDatetimePicker", {
            enableTime: true,
            dateFormat: "Y-m-d H:i",  // Format: day-month-year hour:minute
            time_24hr: true,
            locale: "vn",
            allowInput: true,
            clickOpens: true,
            defaultDate: twoDaysLaterStr,
            onChange: function (selectedDates, dateStr) {
                // Update the input value when the date changes
                $('#endDatetimePicker').val(dateStr);
                console.log('End date changed to: ' + dateStr);
            }
        });

        // Set initial values after initialization
        setTimeout(function () {
            // Force the input values to be updated with formatted dates
            const startPicker = document.getElementById('startDatetimePicker')._flatpickr;
            const endPicker = document.getElementById('endDatetimePicker')._flatpickr;

            if (startPicker) {
                const startFormatted = startPicker.formatDate(startPicker.selectedDates[0], "Y-m-d H:i");
                $('#startDatetimePicker').val(startFormatted);
            }

            if (endPicker) {
                const endFormatted = endPicker.formatDate(endPicker.selectedDates[0], "Y-m-d H:i");
                $('#endDatetimePicker').val(endFormatted);
            }
            // Thêm 2 dòng này ngay sau khi set value
            $('#startDatetimePicker').trigger('change');
            $('#endDatetimePicker').trigger('change');
        }, 100);
    }

    // Initialize dropdowns
    $('.dropdown input[readonly]').click(function () {
        $(this).siblings('.dropdown-content').toggle();
    });

    $('.dropdown-item').click(function () {
        let selectedValue = $(this).text();
        $(this).closest('.dropdown').find('input').val(selectedValue);
        $(this).closest('.dropdown-content').hide();
    });

    // Close dropdowns when clicking outside
    $(document).click(function (e) {
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
    document.addEventListener('click', function (event) {
        if (!event.target.closest('.input-with-icon')) {
            closeAllDropdowns();
        }
    });

    // Ngăn chặn sự kiện click trên dropdown để không lan đến document
    cityDropdown.addEventListener('click', function (event) {
        event.stopPropagation();
    });

    districtDropdown.addEventListener('click', function (event) {
        event.stopPropagation();
    });

    wardDropdown.addEventListener('click', function (event) {
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
        item.addEventListener('click', function () {
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
        item.addEventListener('click', function () {
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
        item.addEventListener('click', function () {
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
document.addEventListener('DOMContentLoaded', function () {
    const createOrderBtn = document.querySelector('.create-order-btn');
    if (createOrderBtn) {
        // Thay thế addEventListener bằng code mới
        $(createOrderBtn).on('click', function (e) {
            //e.preventDefault();

            // Lấy tất cả ID linh kiện đã chọn
            const selectedPartIds = [];
            $('#selectedPartsContainer .selected-part').each(function () {
                selectedPartIds.push($(this).attr('data-id'));
            });

            // Gắn linh kiện vào các lỗi
            $('.split-with-button').each(function (index) {
                // Nếu có linh kiện tương ứng với index này
                if (index < selectedPartIds.length) {
                    // Tạo hoặc cập nhật hidden field
                    let hiddenField = $(this).find(`input[name="ErrorDetails[${index}].IdLinhKien"]`);
                    if (hiddenField.length === 0) {
                        hiddenField = $(`<input type="hidden" name="ErrorDetails[${index}].IdLinhKien" value="${selectedPartIds[index]}">`);
                        $(this).append(hiddenField);
                    } else {
                        hiddenField.val(selectedPartIds[index]);
                    }
                }
            });

            // Get the service order ID from the hidden input
            const serviceOrderId = document.getElementById('IdDonDichVu').value;


            // Validate form fields
            if (!validateOrderForm()) {
                return; // Stop if validation fails
            }

            // For demonstration, show a confirmation that includes the ID
            if (confirm(`Xác nhận tạo đơn dịch vụ ${serviceOrderId}?`)) {
                // Collect form data
                const formData = new FormData();


                // Add service order ID
                formData.append('IdDonDichVu', serviceOrderId);

                // Customer information
                formData.append('HoVaTen', $('.form-group:contains("Họ và tên khách hàng") input').val());
                formData.append('Sdt', $('.form-group:contains("Số điện thoại") input').val());
                formData.append('Email', $('.form-group:contains("Email") input').val());
                formData.append('IdPhuong', $('#wardId').val());
                formData.append('DuongSoNha', $('input[placeholder="Số nhà, đường"]').val());

                // Device information
                formData.append('IdLoaiThietBi', $('#IdLoaiThietBi').val());
                formData.append('TenThietBi', $('.form-group:contains("Tên thiết bị") input').val());

                // Service type information
                formData.append('LoaiDonDichVu', $('.section:contains("Loại dịch vụ") .dropdown input').eq(0).val());
                formData.append('HinhThucDichVu', $('.section:contains("Loại dịch vụ") .dropdown input').eq(1).val());

                // Staff information: LẤY ĐÚNG ID NHÂN VIÊN KỸ THUẬT ĐÃ CHỌN
                formData.append('IdNhanVienKyThuat', $('#selectedStaffId').val());

                // Collect error details
                const selectedLinhKiens = [];
                $('#selectedPartsContainer .selected-part').each(function () {
                    const partId = $(this).attr('data-id');
                    if (partId) {
                        selectedLinhKiens.push(partId);
                    }
                });

                // Tạo biến để đếm số linh kiện đã sử dụng
                let linhKienIndex = 0;

                // Collect error details
                $('.split-with-button').each(function (index) {
                    // Lấy IdLoi từ select box 
                    const selectLoi = $(this).find('select[id^="IdLoi"]');
                    const idLoi = selectLoi.val();
                    console.log("Select element found:", selectLoi.length, "ID:", selectLoi.attr('id'));
                    console.log("IdLoi value:", idLoi);

                    // Lấy IdLinhKien từ danh sách linh kiện đã chọn (nếu còn)
                    let idLinhKien = '';
                    if (linhKienIndex < selectedLinhKiens.length) {
                        idLinhKien = selectedLinhKiens[linhKienIndex];
                        linhKienIndex++; // Tăng chỉ số để dùng linh kiện tiếp theo cho lỗi tiếp theo
                    }

                    console.log(`Gán linh kiện ${idLinhKien} cho lỗi ${idLoi}`);

                    const conBaoHanh = $(this).find('input[type="radio"][name^="warranty"]:checked').val();
                    console.log(`Trạng thái bảo hành cho lỗi ${idLoi}: ${conBaoHanh}`);

                    // Thêm trực tiếp vào FormData
                    formData.append(`ErrorDetails[${index}].IdLoi`, idLoi || '');
                    formData.append(`ErrorDetails[${index}].MoTaLoi`, $(this).find('textarea').first().val() || '');
                    formData.append(`ErrorDetails[${index}].IdLinhKien`, idLinhKien || ''); // Gán IdLinhKien vào ErrorDetails
                    formData.append(`ErrorDetails[${index}].SoLuong`, 1);
                    formData.append(`ErrorDetails[${index}].LoaiDichVu`, $('.section:contains("Loại dịch vụ") .dropdown input').eq(0).val() || '');
                    formData.append(`ErrorDetails[${index}].ConBaoHanh`, conBaoHanh);
                });


                const selectedPartIds = [];
                const selectedPartLoiIds = [];

                $('#selectedPartsContainer .selected-part').each(function () {
                    const partId = $(this).attr('data-id');
                    if (partId) {
                        selectedPartIds.push(partId);
                        // Get associated error ID if any (or empty string)
                        selectedPartLoiIds.push($(this).attr('data-error-id') || '');
                    }
                });
                // Add selected parts as JSON strings
                formData.append('SelectedPartIds', JSON.stringify(selectedPartIds));
                formData.append('SelectedPartLoiIds', JSON.stringify(selectedPartLoiIds));
                // Handle device images
                if ($('#deviceImageInput')[0]?.files.length > 0) {
                    for (let i = 0; i < $('#deviceImageInput')[0].files.length; i++) {
                        formData.append('DeviceImages', $('#deviceImageInput')[0].files[i]);
                    }
                }

                // Handle warranty images
                if ($('#warrantyImageInput')[0]?.files.length > 0) {
                    for (let i = 0; i < $('#warrantyImageInput')[0].files.length; i++) {
                        formData.append('WarrantyImages', $('#warrantyImageInput')[0].files[i]);
                    }
                }

                formData.append('NgayHoanThanh', $('#endDatetimePicker').val());
                let tongTien = $('#tongTienInput').val() || $('.price-input').eq(1).val() || '0';
                tongTien = tongTien.replace(/\./g, '').replace(/,/g, '').replace(/\s/g, '');
                const tongTienNumber = parseInt(tongTien) || 0;
                formData.append('TongTien', tongTienNumber); // lưu 300000.00

            }
        });
    }
});

// Xử lý sự kiện khi chọn trạng thái đơn hàng
$(document).ready(function () {
    // Trạng thái dropdown
    const statusDropdown = $('#statusDropdown');
    const statusInput = statusDropdown.find('input');
    const statusDropdownItems = statusDropdown.find('.dropdown-item');
    const paymentBtn = $('.payment-btn');

    // Đặt trạng thái ban đầu
    // checkStatusAndTogglePaymentButton(statusInput.val());
    statusInput.val("Chưa hoàn thành");


    // Xử lý khi click vào các item trong dropdown trạng thái
    statusDropdownItems.click(function () {
        const selectedStatus = $(this).text().trim();
        statusInput.val(selectedStatus);
        //checkStatusAndTogglePaymentButton(selectedStatus);
        // Hiển thị nút thanh toán chỉ khi chọn "Đã hoàn thành"
        if (selectedStatus === "Đã hoàn thành") {
            paymentBtn.addClass('payment-btn-visible');
        } else {
            paymentBtn.removeClass('payment-btn-visible');
        }
    });
    // Thêm xử lý khi nhấn nút cập nhật đơn
    $(document).ready(function () {
        $('.update-order-btn').click(function () {
            // Lấy thông tin cần thiết
            const orderId = $('#IdDonDichVu').val();
            const paymentMethod = $('#paymentMethod').val() || '';
            const status = $('#statusInput').val() || 'Chưa hoàn thành';
            const statusToSave = (status === "Đã hoàn thành") ? "Hoàn thành" : status;
            const selectedStaffId = $('#selectedStaffId').val() || '';

            // Hiển thị thông báo xác nhận
            if (confirm('Bạn có chắc chắn muốn cập nhật đơn dịch vụ này?')) {
                // Hiển thị loading
                showLoadingOverlay('Đang cập nhật đơn dịch vụ...');

                // Gửi yêu cầu cập nhật lên server
                $.ajax({
                    url: updateServiceOrderUrl,
                    type: 'POST',
                    data: {
                        IdDonDichVu: orderId,
                        PhuongThucThanhToan: paymentMethod,
                        TrangThaiDon: statusToSave,
                        IdNhanVienKyThuat: selectedStaffId
                    },
                    success: function (response) {
                        // Ẩn loading
                        hideLoadingOverlay();

                        if (response.success) {
                            alert('Cập nhật đơn dịch vụ thành công!');

                            // Nếu muốn chuyển về trang danh sách
                            if (confirm('Bạn có muốn quay về danh sách đơn dịch vụ không?')) {
                                window.location.href = backToListUrl;
                            }
                        } else {
                            alert('Lỗi: ' + (response.error || 'Không thể cập nhật đơn dịch vụ'));
                        }
                    },
                    error: function (xhr, status, error) {
                        // Ẩn loading
                        hideLoadingOverlay();

                        alert('Đã xảy ra lỗi khi cập nhật đơn dịch vụ. Vui lòng thử lại.');
                        console.error('Error:', error);
                    }
                });
            }
        });
    });
    // Hàm kiểm tra trạng thái và hiển thị/ẩn nút thanh toán
    function checkStatusAndTogglePaymentButton(status) {
        if (status === "Đã hoàn thành") {
            paymentBtn.show();
        } else {
            paymentBtn.hide();
        }
    }

    // Xử lý khi click vào nút thanh toán
    paymentBtn.click(function () {
        // Kiểm tra tiền còn lại
        const remainingText = $('#remainingAmount').text();
        const remainingAmount = parseInt(remainingText.match(/\d+/g).join('')) || 0;

        if (remainingAmount <= 1000) {
            alert("Đơn hàng phải nhập đầy đủ các thông tin bắt buộc!");
        } else {
            // Hiển thị popup hóa đơn thanh toán
            showPaymentPopup();
        }
    });    // Hiển thị popup hóa đơn thanh toán
    function showPaymentPopup() {
        console.log('Opening payment popup...');

        // Log date values before populating
        console.log('Start date before populate:', $('#startDatetimePicker').val());
        console.log('End date before populate:', $('#endDatetimePicker').val());

        // Lấy thông tin từ form để hiển thị trong hóa đơn
        populateInvoiceData();

        // Log date values after populating
        console.log('Start date displayed:', $('#invoice-start-time').text());
        console.log('End date displayed:', $('#invoice-end-time').text());

        // Hiển thị popup
        $('#paymentModal').fadeIn(100);
    }

    // Lấy dữ liệu từ form và điền vào hóa đơn
    function populateInvoiceData() {
        // Hiển thị ảnh thiết bị trong hóa đơn
        const deviceImagesPreview = $('#deviceImagePreview img');
        const invoiceDeviceImages = $('#invoice-device-images');
        invoiceDeviceImages.empty();
        if (deviceImagesPreview.length > 0) {
            deviceImagesPreview.each(function () {
                const img = $('<img>').attr('src', $(this).attr('src')).css({
                    width: '80px',
                    height: '80px',
                    objectFit: 'cover',
                    margin: '5px',
                    borderRadius: '4px',
                    cursor: 'pointer'
                });
                img.on('click', function () {
                    showImageInModal($(this).attr('src'));
                });
                invoiceDeviceImages.append(img);
            });
        } else {
            invoiceDeviceImages.html('<div style="color:#888;">Không có ảnh thiết bị</div>');
        }

        // Hiển thị ảnh phiếu bảo hành trong hóa đơn
        const warrantyImagesPreview = $('#warrantyImagePreview img');
        const invoiceWarrantyImages = $('#invoice-warranty-images');
        invoiceWarrantyImages.empty();
        if (warrantyImagesPreview.length > 0) {
            warrantyImagesPreview.each(function () {
                const img = $('<img>').attr('src', $(this).attr('src')).css({
                    width: '80px',
                    height: '80px',
                    objectFit: 'cover',
                    margin: '5px',
                    borderRadius: '4px',
                    cursor: 'pointer'
                });
                img.on('click', function () {
                    showImageInModal($(this).attr('src'));
                });
                invoiceWarrantyImages.append(img);
            });
        } else {
            invoiceWarrantyImages.html('<div style="color:#888;">Không có ảnh phiếu bảo hành</div>');
        }
        // Đảm bảo flatpickr đã cập nhật value cho input
        if (window.flatpickr) {
            const startPicker = document.getElementById('startDatetimePicker')._flatpickr;
            const endPicker = document.getElementById('endDatetimePicker')._flatpickr;
            if (startPicker && startPicker.selectedDates[0]) {
                $('#startDatetimePicker').val(startPicker.formatDate(startPicker.selectedDates[0], "Y-m-d H:i"));
            }
            if (endPicker && endPicker.selectedDates[0]) {
                $('#endDatetimePicker').val(endPicker.formatDate(endPicker.selectedDates[0], "Y-m-d H:i"));
            }
        }
        // Thông tin đơn hàng
        $('#invoice-order-id').text($('#IdDonDichVu').val());
        $('#invoice-order-date').text($('.order-date').text().replace('Ngày tạo: ', ''));
        //$('#invoice-staff-name').text($('.staff-name').text().replace('Nhân viên: ', ''));        
        const allStaff = $('.staff-name').map(function () {
            return $(this).text().replace('Nhân viên: ', '').trim();
        }).get().join(', ');

        $('#invoice-staff-name').text(allStaff);


        // Thông tin khách hàng
        const customerName = $('.form-group:contains("Họ và tên khách hàng") input').val() || 'Chưa có thông tin';
        const customerPhone = $('.form-group:contains("Số điện thoại") input').val() || 'Chưa có thông tin';
        $('#invoice-customer-name').text(customerName);
        $('#invoice-customer-phone').text(customerPhone);

        // Địa chỉ khách hàng
        const city = $('#cityInput').val();
        const district = $('#districtInput').val();
        const ward = $('#wardInput').val();
        const streetAddress = $('input[placeholder="Số nhà, đường"]').val() || '';
        const fullAddress = [streetAddress, ward, district, city].filter(item => item).join(', ');
        $('#invoice-customer-address').text(fullAddress || 'Chưa có thông tin');

        // Thông tin thiết bị
        const deviceTypes = [];
        const errorTypes = [];
        const errorDescriptions = [];
        const selectedParts = [];

        // Collect all device types, errors, and descriptions from all sections
        $('.section').each(function () {
            // Collect device types - look for both ID and class selectors to handle dynamic additions
            $(this).find('select[id*="IdLoaiThietBi"], select.device-type-select').each(function () {
                const deviceText = $(this).find('option:selected').text();
                if (deviceText && deviceText !== 'Chọn loại thiết bị') {
                    deviceTypes.push(deviceText.trim());
                }
            });

            // collect selected parts from the selected parts container
            //$('.section').each(function () {
            //    $(this).find('select[id*="IdLinhKien"], select.error-type-select').each(function () {
            //        const errorText = $(this).find('option:selected').text();
            //        if (errorText && errorText !== 'Chọn lỗi') {
            //            errorTypes.push(errorText.trim());
            //        }
            //    });
            //});

            // Collect error types - look for both ID and class selectors
            $(this).find('select[id*="IdLoi"], select.error-type-select').each(function () {
                const errorText = $(this).find('option:selected').text();
                if (errorText && errorText !== 'Chọn lỗi') {
                    errorTypes.push(errorText.trim());
                }
            });



            // Collect error descriptions from all textareas in the form
            $(this).find('.form-group:contains("Mô tả lỗi") textarea, textarea.error-desc-textarea').each(function () {
                const descText = $(this).val().trim();
                if (descText) {
                    errorDescriptions.push(descText);
                }
            });
        });

        // Also collect from specific error containers if any
        $('.selected-error').each(function () {
            const errorText = $(this).text().trim();
            if (errorText) {
                errorDescriptions.push(errorText);
            }
        });

        // If no devices found
        if (deviceTypes.length === 0) {
            deviceTypes.push('Chưa chọn');
        }

        // If no parts selected
        if (selectedParts.length === 0) {
            selectedParts.push('Không có linh kiện');
        }

        // If no errors found
        if (errorTypes.length === 0) {
            errorTypes.push('Chưa chọn');
        }

        // If no descriptions found
        if (errorDescriptions.length === 0) {
            errorDescriptions.push('Không có mô tả');
        }

        // Format them with bullet points for display
        const formattedDeviceTypes = deviceTypes.map(type => `• ${type}`).join('<br>');
        const formattedParts = selectedParts.map(part => `• ${part}`).join('<br>');
        const formattedErrorTypes = errorTypes.map(error => `• ${error}`).join('<br>');
        const formattedDescriptions = errorDescriptions.map(desc => `• ${desc}`).join('<br>');

        // Update the invoice
        $('#invoice-device-type').html(formattedDeviceTypes);
        $('#invoice-error-type').html(formattedErrorTypes);
        $('#invoice-error-description').html(formattedDescriptions);
        // Thông tin dịch vụ
        const serviceType = $('.dropdown input[placeholder="Sửa chữa hoặc lắp đặt"]').val() || 'Chưa chọn';
        $('#invoice-service-type').text(serviceType);

        const serviceLocation = $('.dropdown input[placeholder="Trực tiếp hoặc tại nhà"]').val() || 'Chưa chọn';
        $('#invoice-service-location').text(serviceLocation);

        // Kỹ thuật viên
        const technician = $('.selected-staff .staff-name').text();
        $('#invoice-technician').text(technician);
        // Thông tin ngày giờ
        let startDateTime = $('#startDatetimePicker').val() || 'Chưa xác định';
        let endDateTime = $('#endDatetimePicker').val() || 'Chưa xác định';

        // Make sure we get the actual current values from the inputs
        if ($('#startDatetimePicker').length) {
            startDateTime = $('#startDatetimePicker').val() || 'Chưa xác định';
        }

        if ($('#endDatetimePicker').length) {
            endDateTime = $('#endDatetimePicker').val() || 'Chưa xác định';
        }

        // Format the date for better display
        if (startDateTime !== 'Chưa xác định') {
            startDateTime = startDateTime.trim();
            console.log('Start date-time value: ' + startDateTime);
        }

        if (endDateTime !== 'Chưa xác định') {
            endDateTime = endDateTime.trim();
            console.log('End date-time value: ' + endDateTime);
        }

        // Set the values in the payment popup
        $('#invoice-start-time').text(startDateTime);
        $('#invoice-end-time').text(endDateTime);
        //         $('#invoice-start-time').text($('#startDatetimePicker').val() || 'Chưa xác định');
        // $('#invoice-end-time').text($('#endDatetimePicker').val() || 'Chưa xác định');

        // Hiển thị danh sách linh kiện
        const partsList = $('#invoice-parts-list');
        partsList.empty();

        let partsCost = 0;

        // Lấy các linh kiện đã chọn
        $('#selectedPartsContainer .selected-part').each(function () {
            const partName = $(this).find('.part-name').text();

            // Extract price from text content instead of data-attribute
            const priceText = $(this).find('.part-price').text();
            const priceMatch = priceText.match(/[\d.,]+/); // Extract numeric part
            const raw = priceMatch ? priceMatch[0].replace(/\./g, '').replace(/,/g, '') : '0';
            const partPrice = parseFloat(raw);
            //const formattedPrice = formatCurrency(parseInt(partPrice));
            const formattedPrice = new Intl.NumberFormat('vi-VN').format(parseInt(partPrice));
            partsList.append(
                `<div class="payment-part-item">
                    <div class="payment-part-name">${partName}</div>
                    <div class="payment-part-price">${formattedPrice} VND</div>
                </div>`
            );

            partsCost += parseInt(partPrice);
        });

        if (partsList.children().length === 0) {
            partsList.append('<div class="payment-part-item">Không có linh kiện thay thế</div>');
        }

        // Thông tin thanh toán
        const errorCost = $('.price-input').eq(0).val() || '0 VND';
        const totalCost = $('.price-input').eq(1).val() || '0 VND';
        const advancePayment = $('#advancePayment').val() || '0';
        // const formattedAdvancePayment = formatCurrency(parseInt(advancePayment));
        // const formattedPartsCost = formatCurrency(partsCost);
        // Replace formatCurrency with a simpler format that doesn't include the ₫ symbol
        const formattedAdvancePayment = new Intl.NumberFormat('vi-VN').format(parseInt(advancePayment));
        const formattedPartsCost = new Intl.NumberFormat('vi-VN').format(partsCost);

        $('#invoice-error-cost').text(errorCost + ' VND');
        $('#invoice-parts-cost').text(formattedPartsCost + ' VND');
        $('#invoice-advance-payment').text(formattedAdvancePayment + ' VND');
        //$('#invoice-remaining-amount').text($('#remainingAmount').text());
        const rawText = $('#remainingAmount').text();
        // const amountMatch = rawText.match(/[\d.,]+\s*VND/);
        // const amountOnly = amountMatch ? amountMatch[0] : '0 VND';
        const amountMatch = rawText.match(/[\d.,]+/);
        const amountOnly = amountMatch ? new Intl.NumberFormat('vi-VN').format(parseInt(amountMatch[0].replace(/\./g, '').replace(/,/g, ''))) + ' VND' : '0 VND';


        $('#invoice-remaining-amount').text(amountOnly);
    }

    // Đóng payment popup khi nhấn nút đóng hoặc nút Hủy
    $('.payment-popup-close, .payment-cancel-btn').click(function () {
        $('#paymentModal').fadeOut(200);
    });    // Xử lý khi nhấn nút xác nhận thanh toán
    $('.payment-confirm-btn').click(function () {
        // Ẩn modal thanh toán và hiển thị modal chọn phương thức thanh toán
        $('#paymentModal').fadeOut(100, function () {
            // Sau khi ẩn modal thanh toán, hiển thị modal phương thức thanh toán
            $('#paymentMethodModal').fadeIn(100);
        });
    });
    // Cập nhật hàm xử lý khi chọn phương thức thanh toán
    $('.payment-method-btn').click(function () {
        // Code hiện tại giữ nguyên
        $('.payment-method-btn').removeClass('selected');
        $(this).addClass('selected');

        const paymentMethod = $(this).data('method');
        let paymentMethodText = paymentMethod === 'Tiền mặt' ? 'Tiền mặt' : 'Chuyển Khoản';

        // Hiển thị thông báo xác nhận
        if (confirm('Bạn có chắc chắn muốn thanh toán bằng ' + paymentMethodText + '?')) {
            // Lấy ID đơn dịch vụ
            const orderId = $('#IdDonDichVu').val();

            // Gửi yêu cầu cập nhật lên server
            $.ajax({
                url: updateServiceOrderUrl,
                type: 'POST',
                data: {
                    IdDonDichVu: orderId,
                    PhuongThucThanhToan: paymentMethod,
                    TrangThaiDon: 'Hoàn thành' // Tự động cập nhật trạng thái thành "Đã hoàn thành"
                },
                success: function (response) {
                    if (response.success) {
                        // Xử lý logic thanh toán ở đây
                        alert('Thanh toán bằng ' + paymentMethodText + ' thành công!');
                        $('#paymentMethodModal').fadeOut(200);

                        // Cập nhật số tiền ứng trước bằng tổng tiền để hiển thị tiền còn lại = 0
                        const totalPrice = $('.price-input').eq(1).val().replace(/[^\d]/g, '');
                        $('#advancePayment').val(totalPrice).trigger('input');

                        // Cập nhật trạng thái hiển thị
                        $('#statusInput').val('Hoàn thành');

                        // Cập nhật UI (nếu có dropdown trạng thái)
                        const statusDropdown = $('#statusDropdown');
                        if (statusDropdown.length) {
                            statusDropdown.find('input').val('Đã hoàn thành');
                        }

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
                        // Thêm dòng chuyển hướng về danh sách đơn dịch vụ
                        window.location.href = backToListUrl;
                    } else {
                        alert('Lỗi: ' + (response.error || 'Không thể cập nhật đơn hàng'));
                    }
                },
                error: function (xhr, status, error) {
                    alert('Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng thử lại.');
                    console.error('Error:', error);
                }
            });
        } else {
            // Nếu người dùng chọn Cancel, xóa lớp selected
            $(this).removeClass('selected');
        }
    });
    // Đóng popup phương thức thanh toán
    $('.payment-method-popup-close').click(function () {
        $('#paymentMethodModal').fadeOut(200);
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

// Function to validate order form
function validateOrderForm() {
    let isValid = true;
    let errorMessage = '';

    // Validate customer information
    if ($('.form-group:contains("Họ và tên khách hàng") input').val().trim() === '') {
        errorMessage += '- Vui lòng nhập họ và tên khách hàng\n';
        isValid = false;
    }

    if ($('.form-group:contains("Số điện thoại") input').val().trim() === '') {
        errorMessage += '- Vui lòng nhập số điện thoại\n';
        isValid = false;
    }

    if ($('#wardId').val() === '') {
        errorMessage += '- Vui lòng chọn phường/xã\n';
        isValid = false;
    }

    // Validate device information
    if ($('#IdLoaiThietBi').val() === '' || $('#IdLoaiThietBi option:selected').text() === 'Chọn loại thiết bị') {
        errorMessage += '- Vui lòng chọn loại thiết bị\n';
        isValid = false;
    }

    if ($('.form-group:contains("Tên thiết bị") input').val().trim() === '') {
        errorMessage += '- Vui lòng nhập tên thiết bị\n';
        isValid = false;
    }

    // Validate error details
    let hasValidError = false;
    $('.split-with-button').each(function () {
        const errorSelect = $(this).find('select[id^="IdLoi"]');
        const errorDesc = $(this).find('textarea').first();

        if (errorSelect.val() && errorSelect.val() !== '' && errorDesc.val().trim() !== '') {
            hasValidError = true;
            // Store the error ID in a data attribute for later submission
            $(this).attr('data-error-id', errorSelect.val());
        }
    });

    if (!hasValidError) {
        errorMessage += '- Vui lòng chọn ít nhất một loại lỗi và mô tả lỗi\n';
        isValid = false;
    }

    // Validate service type
    if ($('.section:contains("Loại dịch vụ") .dropdown input').eq(0).val().trim() === '') {
        errorMessage += '- Vui lòng chọn loại dịch vụ\n';
        isValid = false;
    }

    if ($('.section:contains("Loại dịch vụ") .dropdown input').eq(1).val().trim() === '') {
        errorMessage += '- Vui lòng chọn hình thức dịch vụ\n';
        isValid = false;
    }

    // Validate staff assignment
    if ($('#selectedStaffId').val() === '') {
        errorMessage += '- Vui lòng chọn nhân viên kỹ thuật\n';
        isValid = false;
    }

    // Show error messages if validation fails
    if (!isValid) {
        alert('Vui lòng điền đầy đủ thông tin:\n' + errorMessage);
    }

    return isValid;
}

// Function to show loading overlay
function showLoadingOverlay(message) {
    // Create loading overlay if it doesn't exist
    if ($('#loadingOverlay').length === 0) {
        $('body').append(`
            <div id="loadingOverlay" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 9999; display: flex; justify-content: center; align-items: center;">
                <div style="background: white; padding: 20px; border-radius: 5px; text-align: center;">
                    <div class="loading-spinner" style="margin: 0 auto 10px;"></div>
                    <p id="loadingMessage">${message || 'Đang xử lý...'}</p>
                </div>
            </div>
        `);
    } else {
        $('#loadingMessage').text(message || 'Đang xử lý...');
        $('#loadingOverlay').show();
    }
}

// Function to hide loading overlay
function hideLoadingOverlay() {
    $('#loadingOverlay').hide();
}