
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
        newComponentSection.find('input[type="text"]').val('');
        newComponentSection.find('textarea').val('');
        newComponentSection.find('input[type="radio"]').prop('checked', false);
        newComponentSection.find('select').val('');
        
        // Fix radio button IDs and names to make them unique
        let timestamp = new Date().getTime();
        newComponentSection.find('input[type="radio"]').each(function() {
            let oldId = $(this).attr('id');
            let newId = oldId + '-' + timestamp;
            $(this).attr('id', newId);
            $(this).attr('name', 'warranty-' + timestamp);
            $(this).next('label').attr('for', newId);
        });
        
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
    });
      // Event delegation for remove buttons (for dynamically added elements)
    $(document).on('click', '.remove-part-btn', function() {
        // Only remove if it's not the first section
        if ($('.split-with-button').length > 1) {
            $(this).closest('.split-with-button').remove();
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
                });
            } else {
                staffListSection.innerHTML = '<div class="no-staff">Không tìm thấy nhân viên với chuyên môn này</div>';
            }
        }
    }
});