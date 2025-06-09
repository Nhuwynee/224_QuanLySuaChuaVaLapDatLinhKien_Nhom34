import { Chart } from "@/components/ui/chart"
document.addEventListener("DOMContentLoaded", () => {
    // Kiểm tra Chart.js đã load chưa
    if (typeof Chart === "undefined") {
        console.error("Chart.js chưa được load!")
        return
    }

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
const bootstrap = window.bootstrap // Declare the bootstrap variable

// Chart functionality
function initializeChart() {
    const ctx = document.getElementById("revenueChart")
    if (!ctx) {
        console.log("Canvas element not found")
        return
    }

    const customerData = getCustomerData()
    console.log("Customer data:", customerData)

    if (customerData.length === 0) {
        console.log("No customer data found")
        // Hiển thị thông báo không có dữ liệu
        ctx.getContext("2d").fillText("Không có dữ liệu để hiển thị", 10, 50)
        return
    }

    const top10Data = customerData.slice(0, 10)
    console.log("Top 10 data:", top10Data)

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
                    "#FF6B6B",
                    "#4ECDC4",
                    "#45B7D1",
                    "#96CEB4",
                ],
                borderWidth: 2,
                borderColor: "#fff",
            },
        ],
    }

    try {
        // Destroy existing chart if exists
        if (currentChart) {
            currentChart.destroy()
        }

        currentChart = new Chart(ctx, {
            type: currentChartType,
            data: chartData,
            options: getChartOptions(),
        })
        console.log("Chart initialized successfully with type:", currentChartType)
    } catch (error) {
        console.error("Error initializing chart:", error)
    }
}

function changeChartType(type) {
    console.log("Changing chart type to:", type)
    currentChartType = type

    // Update button states
    document.querySelectorAll(".chart-controls .btn").forEach((btn) => {
        btn.classList.remove("active")
    })

    const targetBtn = document.getElementById(`chartType${type.charAt(0).toUpperCase() + type.slice(1)}`)
    if (targetBtn) {
        targetBtn.classList.add("active")
        console.log("Button activated:", targetBtn.id)
    } else {
        console.error("Button not found:", `chartType${type.charAt(0).toUpperCase() + type.slice(1)}`)
    }

    // Reinitialize chart with new type
    initializeChart()
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
                        let value

                        if (currentChartType === "pie") {
                            value = formatCurrency(context.parsed)
                        } else {
                            value = formatCurrency(context.parsed.y)
                        }

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
                x: {
                    ticks: {
                        maxRotation: 45,
                        minRotation: 0,
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
    // Simple alert if bootstrap is not available
    if (typeof bootstrap === "undefined") {
        alert(`Chi tiết khách hàng: ${customerName}\n\nChức năng này cần Bootstrap để hiển thị modal.`)
        return
    }

    const modalElement = document.getElementById("customerDetailsModal")
    if (!modalElement) {
        alert(`Chi tiết khách hàng: ${customerName}`)
        return
    }

    const modal = new bootstrap.Modal(modalElement)
    const content = document.getElementById("customerDetailsContent")

    if (content) {
        content.innerHTML = `
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Đang tải...</span>
            </div>
            <p class="mt-2">Đang tải thông tin chi tiết...</p>
        </div>
    `
    }

    modal.show()

    // Simulate API call
    setTimeout(() => {
        if (content) {
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
        }
    }, 1000)
}

// Utility functions
function getCustomerData() {
    const rows = document.querySelectorAll(".customer-row")
    console.log("Found customer rows:", rows.length)

    const data = Array.from(rows)
        .map((row) => {
            const name = row.querySelector(".customer-name")?.textContent?.trim() || ""
            const revenue = Number.parseFloat(row.dataset.revenue) || 0
            console.log("Customer:", name, "Revenue:", revenue)
            return { name, revenue }
        })
        .filter((item) => item.name && item.revenue > 0)

    console.log("Processed customer data:", data)
    return data
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

// Debug function
window.debugChart = () => {
    console.log("Chart.js available:", typeof Chart !== "undefined")
    console.log("Canvas element:", document.getElementById("revenueChart"))
    console.log("Customer data:", getCustomerData())
    console.log("Current chart:", currentChart)
}
