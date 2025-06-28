import { Chart } from "@/components/ui/chart"
document.addEventListener("DOMContentLoaded", () => {
    // Initialize chart
    initializeChart()

    // Initialize filters
    initializeFilters()

    // Initialize pagination
    initializePagination()

    // Initialize export functionality
    initializeExport()
})

let currentChart = null
let currentChartType = "pie"
let currentPage = 1
let pageSize = 25
let filteredData = []
const bootstrap = window.bootstrap // Declare bootstrap variable

// Chart functionality
function initializeChart() {
    const ctx = document.getElementById("revenueChart")
    if (!ctx) {
        console.log("Canvas element not found")
        return
    }

    const customerData = getCustomerData()
    if (customerData.length === 0) {
        console.log("No customer data found")
        return
    }

    const top10Data = customerData.slice(0, 10)

    const chartData = {
        labels: top10Data.map((c) => c.name),
        datasets: [
            {
                label: "Doanh thu (VNĐ)",
                data: top10Data.map((c) => c.revenue),
                backgroundColor: [
                    "#FF6384",
                    "#36A2EB",
                    "#FFCE56",
                    "#4BC0C0",
                    "#9966FF",
                    "#FF9F40",
                    "#FF6384",
                    "#C9CBCF",
                    "#4BC0C0",
                    "#FF6384",
                ],
                borderWidth: 2,
                borderColor: "#fff",
            },
        ],
    }

    try {
        currentChart = new Chart(ctx, {
            type: currentChartType,
            data: chartData,
            options: getChartOptions(),
        })
        console.log("Chart initialized successfully")
    } catch (error) {
        console.error("Error initializing chart:", error)
    }
}

function changeChartType(type) {
    currentChartType = type

    // Update button states
    document.querySelectorAll(".chart-controls .btn").forEach((btn) => {
        btn.classList.remove("active")
    })
    const targetBtn = document.getElementById(`chartType${type.charAt(0).toUpperCase() + type.slice(1)}`)
    if (targetBtn) {
        targetBtn.classList.add("active")
    }

    // Update chart
    if (currentChart) {
        currentChart.destroy()
        initializeChart()
    }
}

function getChartOptions() {
    const baseOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: "bottom",
                labels: {
                    padding: 20,
                    usePointStyle: true,
                },
            },
            tooltip: {
                callbacks: {
                    label: (context) => {
                        const label = context.label || ""
                        const value = formatCurrency(context.parsed.y || context.parsed)
                        return `${label}: ${value}`
                    },
                },
            },
        },
    }

    if (currentChartType === "bar") {
        return {
            ...baseOptions,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: (value) => formatCurrency(value),
                    },
                },
            },
        }
    }

    return baseOptions
}

// Filter functionality
function initializeFilters() {
    const searchInput = document.getElementById("searchCustomer")
    const sortSelect = document.getElementById("sortBy")
    const pageSizeSelect = document.getElementById("pageSize")

    if (searchInput) {
        searchInput.addEventListener("input", debounce(filterAndSort, 300))
    }

    if (sortSelect) {
        sortSelect.addEventListener("change", filterAndSort)
    }

    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function () {
            pageSize = this.value === "all" ? 999999 : Number.parseInt(this.value)
            currentPage = 1
            filterAndSort()
        })
    }

    // Initialize with current data
    filterAndSort()
}

function filterAndSort() {
    const searchTerm = document.getElementById("searchCustomer")?.value.toLowerCase() || ""
    const sortBy = document.getElementById("sortBy")?.value || "revenue-desc"

    // Get all customer rows
    const rows = Array.from(document.querySelectorAll(".customer-row"))

    // Filter
    filteredData = rows.filter((row) => {
        const name = row.dataset.name || ""
        return name.includes(searchTerm)
    })

    // Sort
    filteredData.sort((a, b) => {
        const aRevenue = Number.parseFloat(a.dataset.revenue) || 0
        const bRevenue = Number.parseFloat(b.dataset.revenue) || 0
        const aName = a.dataset.name || ""
        const bName = b.dataset.name || ""

        switch (sortBy) {
            case "revenue-desc":
                return bRevenue - aRevenue
            case "revenue-asc":
                return aRevenue - bRevenue
            case "name-asc":
                return aName.localeCompare(bName)
            case "name-desc":
                return bName.localeCompare(aName)
            default:
                return 0
        }
    })

    updateTable()
    updatePagination()
}

function updateTable() {
    const tbody = document.getElementById("customerTableBody")
    if (!tbody) return

    // Hide all rows
    const allRows = document.querySelectorAll(".customer-row")
    allRows.forEach((row) => (row.style.display = "none"))

    // Calculate pagination
    const startIndex = (currentPage - 1) * pageSize
    const endIndex = Math.min(startIndex + pageSize, filteredData.length)

    // Show filtered and paginated rows
    for (let i = startIndex; i < endIndex; i++) {
        if (filteredData[i]) {
            filteredData[i].style.display = ""
            // Update row number
            const firstCell = filteredData[i].querySelector("td:first-child")
            if (firstCell) {
                firstCell.textContent = i + 1
            }
        }
    }

    // Update table info
    const tableInfo = document.getElementById("tableInfo")
    if (tableInfo) {
        tableInfo.textContent = `Hiển thị ${startIndex + 1}-${endIndex} của ${filteredData.length} khách hàng`
    }
}

// Pagination functionality
function initializePagination() {
    updatePagination()
}

function updatePagination() {
    const container = document.getElementById("paginationContainer")
    if (!container) return

    const totalPages = Math.ceil(filteredData.length / pageSize)

    if (totalPages <= 1) {
        container.innerHTML = ""
        return
    }

    let paginationHTML = '<div class="pagination">'

    // Previous button
    paginationHTML += `<button class="page-btn" ${currentPage === 1 ? "disabled" : ""} onclick="changePage(${currentPage - 1})">
        <i class="fas fa-chevron-left"></i>
    </button>`

    // Page numbers
    const startPage = Math.max(1, currentPage - 2)
    const endPage = Math.min(totalPages, currentPage + 2)

    if (startPage > 1) {
        paginationHTML += `<button class="page-btn" onclick="changePage(1)">1</button>`
        if (startPage > 2) {
            paginationHTML += `<span class="page-ellipsis">...</span>`
        }
    }

    for (let i = startPage; i <= endPage; i++) {
        paginationHTML += `<button class="page-btn ${i === currentPage ? "active" : ""}" onclick="changePage(${i})">${i}</button>`
    }

    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            paginationHTML += `<span class="page-ellipsis">...</span>`
        }
        paginationHTML += `<button class="page-btn" onclick="changePage(${totalPages})">${totalPages}</button>`
    }

    // Next button
    paginationHTML += `<button class="page-btn" ${currentPage === totalPages ? "disabled" : ""} onclick="changePage(${currentPage + 1})">
        <i class="fas fa-chevron-right"></i>
    </button>`

    paginationHTML += "</div>"
    container.innerHTML = paginationHTML
}

function changePage(page) {
    const totalPages = Math.ceil(filteredData.length / pageSize)
    if (page < 1 || page > totalPages) return

    currentPage = page
    updateTable()
    updatePagination()

    // Scroll to top of table
    document.querySelector(".table-section")?.scrollIntoView({ behavior: "smooth" })
}

// Export functionality
function initializeExport() {
    const exportBtn = document.getElementById("exportDetailBtn")
    if (exportBtn) {
        exportBtn.addEventListener("click", exportToExcel)
    }
}

function exportToExcel() {
    // Get current month/year from the page
    const title = document.querySelector(".page-title")?.textContent || "Chi tiết doanh thu"
    const monthYear = document.querySelector(".page-subtitle")?.textContent || ""

    // Create export URL
    const url = `/ExcelExport/ExportChiTietThang?monthYear=${encodeURIComponent(monthYear)}`

    // Download file
    window.location.href = url
}

// Customer details functionality
function viewCustomerDetails(customerName) {
    // Check if bootstrap is available
    if (typeof bootstrap === "undefined") {
        alert(`Chi tiết khách hàng: ${customerName}`)
        return
    }

    const modal = new bootstrap.Modal(document.getElementById("customerDetailsModal"))
    const content = document.getElementById("customerDetailsContent")

    content.innerHTML = `
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
            <p class="mt-2">Đang tải thông tin chi tiết...</p>
        </div>
    `

    modal.show()

    // Simulate API call
    setTimeout(() => {
        content.innerHTML = `
            <h5>Thông tin khách hàng: ${customerName}</h5>
            <div class="row">
                <div class="col-md-6">
                    <p><strong>Tên:</strong> ${customerName}</p>
                    <p><strong>Số đơn hàng:</strong> 5</p>
                    <p><strong>Tổng doanh thu:</strong> 2,500,000 VNĐ</p>
                </div>
                <div class="col-md-6">
                    <p><strong>Lần mua cuối:</strong> 15/12/2024</p>
                    <p><strong>Trung bình/đơn:</strong> 500,000 VNĐ</p>
                    <p><strong>Loại khách hàng:</strong> VIP</p>
                </div>
            </div>
            <hr>
            <h6>Lịch sử đơn hàng</h6>
            <div class="table-responsive">
                <table class="table table-sm">
                    <thead>
                        <tr>
                            <th>Ngày</th>
                            <th>Mã đơn</th>
                            <th>Giá trị</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>15/12/2024</td>
                            <td>DH001</td>
                            <td>500,000 VNĐ</td>
                        </tr>
                        <tr>
                            <td>10/12/2024</td>
                            <td>DH002</td>
                            <td>750,000 VNĐ</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        `
    }, 1000)
}

// Utility functions
function getCustomerData() {
    const rows = document.querySelectorAll(".customer-row")
    return Array.from(rows).map((row) => ({
        name: row.querySelector(".customer-name")?.textContent || "",
        revenue: Number.parseFloat(row.dataset.revenue) || 0,
    }))
}

function formatCurrency(value) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(value)
}

function debounce(func, wait) {
    let timeout
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout)
            func(...args)
        }
        clearTimeout(timeout)
        timeout = setTimeout(later, wait)
    }
}

// Make functions global for onclick handlers
window.changePage = changePage
window.changeChartType = changeChartType
window.viewCustomerDetails = viewCustomerDetails
