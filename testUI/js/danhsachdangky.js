// JavaScript for danhsachdangky.html
document.addEventListener('DOMContentLoaded', function() {
    // Add event listeners for buttons and interactive elements
    const addButton = document.querySelector('.add-button');
    const filterGroups = document.querySelectorAll('.filter-group');
    
    addButton.addEventListener('click', function() {
        alert('Thêm đơn đăng ký mới');
        // Functionality to add new registration
    });
    
    filterGroups.forEach(group => {
        group.addEventListener('click', function() {
            // Show dropdown for filtering
            console.log('Filter clicked:', this.querySelector('.filter-label').textContent);
        });
    });
});