// JavaScript cho trang danh sách đơn dịch vụ

document.addEventListener('DOMContentLoaded', function() {

    const addButton = document.querySelector('.add-button');
    addButton.addEventListener('click', function() {
        alert('Thêm đơn dịch vụ mới');
        // Functionality to add new registration
    });

    // Xử lý sự kiện khi click vào các nút bộ lọc
    const filterGroups = document.querySelectorAll('.filter-group');
    filterGroups.forEach(group => {
        group.addEventListener('click', function() {
            // Hiển thị dropdown filter (chưa triển khai đầy đủ)
            console.log('Filter clicked:', this.querySelector('.filter-label').textContent);
        });
    });

    // Xử lý sự kiện khi tìm kiếm
    const searchInput = document.querySelector('.search-filter input');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            const searchValue = this.value.toLowerCase().trim();
            filterOrders(searchValue);
        });
    }

    // Xử lý sự kiện khi click vào biểu tượng chỉnh sửa
    const editIcons = document.querySelectorAll('.fa-pen-to-square');
    editIcons.forEach(icon => {
        icon.addEventListener('click', function() {
            const orderId = this.closest('.list-item').querySelector('div:first-child').textContent;
            console.log('Edit order:', orderId);
            // Có thể thêm mã để chuyển hướng đến trang chỉnh sửa đơn
            // window.location.href = `edit-order.html?id=${orderId}`;
        });
    });

    // Xử lý sự kiện khi click vào biểu tượng xem chi tiết
    const viewIcons = document.querySelectorAll('.fa-eye');
    viewIcons.forEach(icon => {
        icon.addEventListener('click', function() {
            const orderId = this.closest('.list-item').querySelector('div:first-child').textContent;
            console.log('View order details:', orderId);
            // Có thể thêm mã để hiển thị modal chi tiết đơn hàng hoặc chuyển hướng
            // window.location.href = `view-order.html?id=${orderId}`;
        });
    });
});

// Hàm lọc danh sách đơn dựa trên giá trị tìm kiếm
function filterOrders(searchValue) {
    const orderItems = document.querySelectorAll('.list-item');
    
    orderItems.forEach(item => {
        let orderText = '';
        // Tổng hợp tất cả nội dung text trong list item để tìm kiếm
        item.querySelectorAll('div').forEach(div => {
            if (!div.classList.contains('action-buttons')) {
                orderText += div.textContent.toLowerCase() + ' ';
            }
        });
        
        // Hiển thị hoặc ẩn dựa trên kết quả tìm kiếm
        if (orderText.includes(searchValue)) {
            item.style.display = '';
        } else {
            item.style.display = 'none';
        }
    });
}