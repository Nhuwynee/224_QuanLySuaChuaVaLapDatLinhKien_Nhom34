document.getElementById('partSearchInput').addEventListener('input', function () {
    const keyword = this.value;

    if (keyword.length >= 1) {
        fetch(`/TaoDonDichVuKVL/TimKiemLinhKien?keyword=${encodeURIComponent(keyword)}`)
            .then(response => response.json())
            .then(data => {
                const dropdown = document.getElementById('partsDropdown');
                dropdown.innerHTML = '';

                if (data.length === 0) {
                    dropdown.innerHTML = '<div class="dropdown-item">Không tìm thấy linh kiện</div>';
                    return;
                }

                data.forEach(item => {
                    const div = document.createElement('div');
                    div.classList.add('dropdown-item');
                    div.style.padding = '8px';
                    div.style.cursor = 'pointer';
                    div.innerHTML = `
                        <strong>${item.ten}</strong><br>
                        Giá: ${item.gia} đ - SL: ${item.soLuong} - NSX: ${item.tenNSX}
                    `;
                    div.addEventListener('click', function () {
                        addSelectedPart(item);
                        dropdown.innerHTML = '';
                        document.getElementById('partSearchInput').value = '';
                    });
                    dropdown.appendChild(div);
                });
            });
    } else {
        document.getElementById('partsDropdown').innerHTML = '';
    }
});

function addSelectedPart(item) {
    const selectedParts = document.getElementById('selectedParts');
    const partDiv = document.createElement('div');
    partDiv.classList.add('selected-part');
    partDiv.style.margin = '5px 0';
    partDiv.style.display = 'flex';
    partDiv.style.justifyContent = 'space-between';
    partDiv.style.alignItems = 'center';

    partDiv.innerHTML = `
        <span>
            <strong>${item.ten}</strong> - ${item.gia} đ - SL: ${item.soLuong} - NSX: ${item.tenNSX}
        </span>
        <i class="fa fa-times remove-icon" style="cursor:pointer; color:red; margin-left: 10px;"></i>
    `;

    partDiv.querySelector('.remove-icon').addEventListener('click', function () {
        partDiv.remove();
    });

    selectedParts.appendChild(partDiv);
}