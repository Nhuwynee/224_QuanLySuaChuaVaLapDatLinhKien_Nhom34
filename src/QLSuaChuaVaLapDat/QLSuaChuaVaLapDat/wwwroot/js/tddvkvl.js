// JavaScript for taodonsuachuaKVL.html
document.addEventListener('DOMContentLoaded', function () {
    // Danh sách các lỗi và đơn giá tương ứng
    const errorPrices = {
        "Không lên nguồn": 350000,
        "Màn hình nứt": 1200000,
        "Màn hình không hiển thị": 850000,
        "Pin yếu": 500000,
        "Lỗi phần mềm": 300000,
        "Không nhận sạc": 400000,
        "Khác": 200000
    };

    // Dropdown functionality
    const dropdowns = document.querySelectorAll('.dropdown');

    dropdowns.forEach(dropdown => {
        const input = dropdown.querySelector('input');
        const dropdownContent = dropdown.querySelector('.dropdown-content');

        // Toggle dropdown visibility when clicking on the input
        input.addEventListener('click', function (e) {
            e.stopPropagation();
            closeAllDropdowns();
            dropdownContent.classList.toggle('show');
        });

        // Handle item selection
        const items = dropdown.querySelectorAll('.dropdown-item');
        items.forEach(item => {
            item.addEventListener('click', function () {
                input.value = this.textContent;
                dropdownContent.classList.remove('show');

                // Kiểm tra nếu đây là dropdown lỗi và cập nhật đơn giá
                const label = dropdown.closest('.form-group').querySelector('label');
                if (label && label.textContent.includes('Lỗi:')) {
                    // Tìm input đơn giá gần nhất để cập nhật
                    const formGroups = dropdown.closest('.form-group').parentElement.querySelectorAll('.form-group');
                    formGroups.forEach(group => {
                        const label = group.querySelector('label');
                        if (label && label.textContent.includes('Đơn giá')) {
                            const priceInput = group.querySelector('input[type="text"]');
                            const errorName = item.textContent;
                            if (priceInput && errorPrices[errorName]) {
                                priceInput.value = new Intl.NumberFormat('vi-VN').format(errorPrices[errorName]);
                            }
                        }
                    });

                }
            });
        });
    });

    // Close dropdowns when clicking outside
    document.addEventListener('click', function () {
        closeAllDropdowns();
    });

    function closeAllDropdowns() {
        const dropdownContents = document.querySelectorAll('.dropdown-content');
        dropdownContents.forEach(content => {
            content.classList.remove('show');
        });
    }

    // File upload preview
    const fileButtons = document.querySelectorAll('.file-btn');
    fileButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            console.log('File upload button clicked');
            // File upload functionality can be implemented here
        });
    });

    // Date picker functionality
    const dateInputs = document.querySelectorAll('.date-time-input');
    dateInputs.forEach(input => {
        const inputField = input.querySelector('input');
        const icon = input.querySelector('.date-icon');

        // Click handler for the calendar icon
        icon.addEventListener('click', function () {
            inputField._flatpickr.toggle();
        });
    });

    // Create order button
    const createOrderBtn = document.querySelector('.create-order-btn');
    createOrderBtn.addEventListener('click', function () {
        alert('Đơn sửa chữa đã được tạo!');
        // Order creation functionality can be implemented here
    });

    // Initialize Flatpickr for the datetime fields
    flatpickr("#startDatetimePicker", {
        enableTime: true,
        dateFormat: "Y-m-d H:i:S", // yyyy-MM-dd HH:mm:ss
        time_24hr: true,
        locale: "vn",
        allowInput: true,
        clickOpens: true,
        position: "auto"
    });

    flatpickr("#endDatetimePicker", {
        enableTime: true,
        dateFormat: "Y-m-d H:i:S",
        time_24hr: true,
        locale: "vn",
        allowInput: true,
        clickOpens: true,
        position: "auto"
    });

    // Danh sách linh kiện mẫu - trong thực tế có thể lấy từ cơ sở dữ liệu
    const availableParts = [
        { id: 1, name: "Màn hình iPhone 12", price: 2500000 },
        { id: 2, name: "Pin Samsung Galaxy S20", price: 850000 },
        { id: 3, name: "Camera Xiaomi Mi 11", price: 1200000 },
        { id: 4, name: "Màn hình Oppo Reno 6", price: 1800000 },
        { id: 5, name: "Pin iPhone 13 Pro", price: 1300000 },
        { id: 6, name: "Loa Xiaomi Redmi Note 10", price: 450000 },
        { id: 7, name: "Touch ID iPhone 8", price: 750000 },
        { id: 8, name: "Cảm ứng Samsung A52", price: 950000 },
        { id: 9, name: "Main board iPhone X", price: 3500000 },
        { id: 10, name: "Cụm cảm biến Huawei P40", price: 1150000 },
        { id: 11, name: "Màn hình Realme 8", price: 1650000 },
        { id: 12, name: "Pin Vivo Y15", price: 550000 }
    ];

    // Danh sách các linh kiện đã chọn
    let selectedParts = [];

    const searchInput = document.getElementById('partSearchInput');
    const partsDropdown = document.getElementById('partsDropdown');
    const selectedPartsContainer = document.getElementById('selectedParts');

    // Sự kiện khi gõ vào ô tìm kiếm
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const searchText = this.value.toLowerCase().trim();

            // Nếu không có nội dung tìm kiếm, ẩn dropdown
            if (searchText === '') {
                partsDropdown.classList.remove('show');
                partsDropdown.innerHTML = '';
                return;
            }

            // Lọc các linh kiện phù hợp với từ khóa tìm kiếm
            const matchedParts = availableParts.filter(part =>
                part.name.toLowerCase().includes(searchText)
            );

            // Hiển thị kết quả trong dropdown
            partsDropdown.innerHTML = '';

            if (matchedParts.length > 0) {
                partsDropdown.classList.add('parts-dropdown-content');
                partsDropdown.classList.add('show');

                matchedParts.forEach(part => {
                    const item = document.createElement('div');
                    item.classList.add('parts-dropdown-item');
                    item.textContent = `${part.name} - ${formatPrice(part.price)}`;
                    item.dataset.id = part.id;
                    item.dataset.name = part.name;
                    item.dataset.price = part.price;

                    // Xử lý khi chọn một linh kiện
                    item.addEventListener('click', function () {
                        const partId = parseInt(this.dataset.id);

                        // Kiểm tra xem linh kiện đã được chọn chưa
                        if (!selectedParts.some(part => part.id === partId)) {
                            const selectedPart = {
                                id: partId,
                                name: this.dataset.name,
                                price: parseInt(this.dataset.price)
                            };

                            selectedParts.push(selectedPart);
                            renderSelectedParts();
                            updateTotalPrice();
                        }

                        // Xóa giá trị tìm kiếm và ẩn dropdown
                        searchInput.value = '';
                        partsDropdown.classList.remove('show');
                    });

                    partsDropdown.appendChild(item);
                });
            } else {
                // Nếu không tìm thấy kết quả nào
                partsDropdown.classList.add('parts-dropdown-content');
                partsDropdown.classList.add('show');

                const noResult = document.createElement('div');
                noResult.classList.add('parts-dropdown-item');
                noResult.textContent = 'Không tìm thấy linh kiện phù hợp';
                partsDropdown.appendChild(noResult);
            }
        });

        // Đóng dropdown khi click bên ngoài
        document.addEventListener('click', function (e) {
            if (searchInput && partsDropdown && !searchInput.contains(e.target) && !partsDropdown.contains(e.target)) {
                partsDropdown.classList.remove('show');
            }
        });
    }

    // Hàm hiển thị các linh kiện đã chọn
    function renderSelectedParts() {
        if (!selectedPartsContainer) return;

        selectedPartsContainer.innerHTML = '';

        selectedParts.forEach(part => {
            const partElement = document.createElement('div');
            partElement.classList.add('selected-part');
            partElement.innerHTML = `
                <span>${part.name}</span>
                <span>${formatPrice(part.price)}</span>
                <i class="fa-solid fa-xmark remove-part" data-id="${part.id}"></i>
            `;

            selectedPartsContainer.appendChild(partElement);
        });

        // Thêm sự kiện xóa cho các nút xóa
        document.querySelectorAll('.remove-part').forEach(button => {
            button.addEventListener('click', function () {
                const partId = parseInt(this.dataset.id);
                selectedParts = selectedParts.filter(part => part.id !== partId);
                renderSelectedParts();
                updateTotalPrice();
            });
        });
    }

    // Hàm cập nhật tổng giá tiền
    function updateTotalPrice() {
        const totalPrice = selectedParts.reduce((sum, part) => sum + part.price, 0);
        const priceInput = document.querySelector('.price-input');

        if (priceInput) {
            priceInput.value = formatPrice(totalPrice);
        }
    }

    // Hàm định dạng giá tiền
    function formatPrice(price) {
        return new Intl.NumberFormat('vi-VN').format(price) + ' VND';
    }
});


// Staff Assignment Popup Functionality
document.addEventListener('DOMContentLoaded', function () {
    const selectStaffBtn = document.querySelector('.select-staff-btn');
    const staffPopup = document.getElementById('staffPopup');
    const specialtySelect = document.querySelector('.specialty-dropdown select');
    const staffItems = document.querySelectorAll('.staff-item');
    const selectedStaffDisplay = document.querySelector('.selected-staff .staff-name');

    // Open popup when clicking the select staff button
    selectStaffBtn.addEventListener('click', function () {
        staffPopup.classList.add('show');
    });

    // Close popup when clicking outside
    staffPopup.addEventListener('click', function (e) {
        if (e.target === staffPopup) {
            staffPopup.classList.remove('show');
        }
    });

    // Handle specialty selection
    specialtySelect.addEventListener('change', function () {
        // Here you would typically filter the staff list based on specialty
        // For now, we'll just log the selected specialty
        console.log('Selected specialty:', this.value);
    });

    // Handle staff selection
    staffItems.forEach(item => {
        item.addEventListener('click', function () {
            if (!item.classList.contains('busy')) {
                const staffName = item.textContent;
                const specialty = specialtySelect.value;
                // selectedStaffDisplay.textContent = `${staffName} - Chuyên môn: ${specialty}`;
                const selectedStaffDisplay = document.querySelector('.selected-staff');
                selectedStaffDisplay.querySelector('.staff-name').textContent = `${staffName} - Chuyên môn: ${specialty}`;
                selectedStaffDisplay.classList.add('has-staff');
                staffPopup.classList.remove('show');
            }
        });
    });
});