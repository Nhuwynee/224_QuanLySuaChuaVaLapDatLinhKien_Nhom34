
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
    }

    // Đóng dropdown khi click ra ngoài
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.search-parts-container').length) {
            partsDropdown.hide();
        }
    });
});