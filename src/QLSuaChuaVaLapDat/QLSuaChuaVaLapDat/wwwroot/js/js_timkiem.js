        // Dynamic district and ward selection based on city
        document.getElementById('cus-city').addEventListener('change', function() {
            const city = this.value;
            const districtSelect = document.getElementById('cus-district');
            const wardSelect = document.getElementById('cus-ward');

            // Reset districts and wards
            districtSelect.innerHTML = '<option value="">Chọn quận/huyện</option>';
            wardSelect.innerHTML = '<option value="">Chọn phường/xã</option>';
            districtSelect.disabled = true;
            wardSelect.disabled = true;

            if (city) {
                districtSelect.disabled = false;

                // Simulate loading districts based on city
                let districts = [];
                if (city === 'hanoi') {
                    districts = [
                        {id: 'cg', name: 'Cầu Giấy'},
                        {id: 'hk', name: 'Hoàn Kiếm'},
                        {id: 'bd', name: 'Ba Đình'}
                    ];
                } else if (city === 'hcm') {
                    districts = [
                        {id: 'q1', name: 'Quận 1'},
                        {id: 'q3', name: 'Quận 3'},
                        {id: 'bt', name: 'Bình Thạnh'}
                    ];
                } else if (city === 'danang') {
                    districts = [
                        {id: 'hc', name: 'Hải Châu'},
                        {id: 'st', name: 'Sơn Trà'},
                        {id: 'nt', name: 'Ngũ Hành Sơn'}
                    ];
                }

                districts.forEach(district => {
                    const option = document.createElement('option');
                    option.value = district.id;
                    option.textContent = district.name;
                    districtSelect.appendChild(option);
                });
            }
        });

        // Xử lý khi click vào các nút giá nhanh
        document.querySelectorAll('.price-quick-btn').forEach(btn => {
            btn.addEventListener('click', function() {
                const price = this.textContent.replace(/[^\d]/g, ''); // Lấy số từ text (bỏ ký tự không phải số)
                document.getElementById('comp-price-to').value = price;
                document.getElementById('comp-price-from').value = '';
            });
        });

        // Dynamic ward selection based on district
        document.getElementById('cus-district').addEventListener('change', function() {
            const district = this.value;
            const wardSelect = document.getElementById('cus-ward');

            // Reset wards
            wardSelect.innerHTML = '<option value="">Chọn phường/xã</option>';
            wardSelect.disabled = true;

            if (district) {
                wardSelect.disabled = false;

                // Simulate loading wards based on district
                let wards = [];
                if (district === 'cg' || district === 'hk' || district === 'bd') {
                    // Hanoi districts
                    wards = [
                        {id: 'p1', name: 'Phường 1'},
                        {id: 'p2', name: 'Phường 2'},
                        {id: 'p3', name: 'Phường 3'}
                    ];
                } else if (district === 'q1' || district === 'q3' || district === 'bt') {
                    // HCM districts
                    wards = [
                        {id: 'p1', name: 'Phường Bến Nghé'},
                        {id: 'p2', name: 'Phường Bến Thành'},
                        {id: 'p3', name: 'Phường Nguyễn Thái Bình'}
                    ];
                } else if (district === 'hc' || district === 'st' || district === 'nt') {
                    // Da Nang districts
                    wards = [
                        {id: 'p1', name: 'Phường Hòa Cường'},
                        {id: 'p2', name: 'Phường Hòa Thuận'},
                        {id: 'p3', name: 'Phường Thanh Bình'}
                    ];
                }

                wards.forEach(ward => {
                    const option = document.createElement('option');
                    option.value = ward.id;
                    option.textContent = ward.name;
                    wardSelect.appendChild(option);
                });
            }
        });

        // Select All functionality
        document.querySelectorAll('.select-all').forEach(selectAll => {
            selectAll.addEventListener('change', function() {
                const tableId = this.getAttribute('data-table');
                const checkboxes = document.querySelectorAll(`#${tableId} .row-checkbox`);
                checkboxes.forEach(checkbox => {
                    checkbox.checked = this.checked;
                    const row = checkbox.closest('tr');
                    row.classList.toggle('selected', this.checked);
                });
            });
        });

        // Individual checkbox functionality
        document.querySelectorAll('.row-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', function() {
                const table = this.closest('.search-container');
                const selectAll = table.querySelector('.select-all');
                const allCheckboxes = table.querySelectorAll('.row-checkbox');
                const row = this.closest('tr');

                // Toggle selected class for the row
                row.classList.toggle('selected', this.checked);

                // Update select-all checkbox state
                selectAll.checked = Array.from(allCheckboxes).every(cb => cb.checked);
            });
        });

