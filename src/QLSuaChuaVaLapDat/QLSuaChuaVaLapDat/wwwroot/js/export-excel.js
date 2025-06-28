document.addEventListener("DOMContentLoaded", () => {
    let availableYears = []
    let currentYear = new Date().getFullYear()

    // Lấy danh sách năm từ API
    async function loadAvailableYears() {
        try {
            const response = await fetch("/api/YearData/available-years")
            const data = await response.json()
            availableYears = data.years
            currentYear = data.currentYear
        } catch (error) {
            console.error("Lỗi khi tải danh sách năm:", error)
            // Fallback: sử dụng danh sách năm mặc định
            availableYears = generateDefaultYears()
        }
    }

    // Tạo danh sách năm mặc định nếu API không hoạt động
    function generateDefaultYears() {
        const years = []
        for (let i = currentYear - 5; i <= currentYear + 1; i++) {
            years.push(i)
        }
        return years.sort((a, b) => b - a)
    }

    // Thêm nút xuất Excel vào trang
    const chartNavigation = document.querySelector(".chart-navigation")
    if (chartNavigation) {
        const exportButton = document.createElement("button")
        exportButton.className = "export-btn"
        exportButton.id = "exportExcelBtn"
        exportButton.innerHTML = `
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <line x1="12" y1="18" x2="12" y2="12"></line>
                <line x1="9" y1="15" x2="15" y2="15"></line>
            </svg>
            Xuất Excel
        `
        chartNavigation.appendChild(exportButton)

        // Tạo modal
        const modalHTML = `
            <div class="export-modal-overlay" id="exportModalOverlay" style="display: none;">
                <div class="export-modal">
                    <div class="export-modal-header">
                        <h3 class="export-modal-title">
                            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                                <polyline points="14 2 14 8 20 8"></polyline>
                                <line x1="12" y1="18" x2="12" y2="12"></line>
                                <line x1="9" y1="15" x2="15" y2="15"></line>
                            </svg>
                            Xuất báo cáo Excel
                        </h3>
                        <button class="export-modal-close" id="closeExportModal">&times;</button>
                    </div>
                    <div class="export-modal-body">
                        <div class="export-radio-group">
                            <div class="export-radio-option">
                                <input type="radio" id="exportTypeSingle" name="exportType" value="single" checked>
                                <label for="exportTypeSingle">Xuất theo tháng</label>
                            </div>
                            <div class="export-radio-option">
                                <input type="radio" id="exportTypeRange" name="exportType" value="range">
                                <label for="exportTypeRange">Xuất theo khoảng thời gian</label>
                            </div>
                        </div>
                        
                        <div id="singleMonthFields" class="export-fields">
                            <div class="export-option">
                                <label class="export-option-label">Chọn tháng:</label>
                                <div class="export-date-field">
                                    <select id="singleMonth" class="export-select">
                                        ${generateMonthOptions()}
                                    </select>
                                </div>
                            </div>
                            <div class="export-option">
                                <label class="export-option-label">Chọn năm:</label>
                                <div class="year-input-container">
                                    <div class="year-select-wrapper">
                                        <input type="text" id="singleYear" class="year-custom-input" placeholder="Nhập năm..." />
                                        <div id="singleYearSuggestions" class="year-suggestions"></div>
                                    </div>
                                    <div class="year-input-hint">Nhập năm hoặc chọn từ danh sách gợi ý</div>
                                </div>
                            </div>
                        </div>
                        
                        <div id="rangeMonthFields" class="export-fields" style="display: none;">
                            <div class="export-option">
                                <label class="export-option-label">Từ tháng:</label>
                                <div class="export-date-range">
                                    <div class="export-date-field">
                                        <select id="fromMonth" class="export-select">
                                            ${generateMonthOptions()}
                                        </select>
                                    </div>
                                    <div class="export-date-field">
                                        <div class="year-input-container">
                                            <div class="year-select-wrapper">
                                                <input type="text" id="fromYear" class="year-custom-input" placeholder="Nhập năm..." />
                                                <div id="fromYearSuggestions" class="year-suggestions"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="export-option">
                                <label class="export-option-label">Đến tháng:</label>
                                <div class="export-date-range">
                                    <div class="export-date-field">
                                        <select id="toMonth" class="export-select">
                                            ${generateMonthOptions()}
                                        </select>
                                    </div>
                                    <div class="export-date-field">
                                        <div class="year-input-container">
                                            <div class="year-select-wrapper">
                                                <input type="text" id="toYear" class="year-custom-input" placeholder="Nhập năm..." />
                                                <div id="toYearSuggestions" class="year-suggestions"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="export-modal-footer">
                        <button class="btn-cancel" id="cancelExport">Hủy</button>
                        <button class="btn-export" id="confirmExport">Xuất Excel</button>
                    </div>
                </div>
            </div>
        `

        // Thêm modal vào body
        document.body.insertAdjacentHTML("beforeend", modalHTML)

        // Khởi tạo các year input sau khi modal được tạo
        initializeYearInputs()

        // Xử lý sự kiện cho nút xuất Excel
        document.getElementById("exportExcelBtn").addEventListener("click", async () => {
            // Hiển thị loading
            const btn = document.getElementById("exportExcelBtn")
            const originalHTML = btn.innerHTML
            btn.innerHTML = '<div class="loading-spinner"></div> Đang tải...'
            btn.disabled = true

            // Tải danh sách năm
            await loadAvailableYears()

            // Cập nhật gợi ý năm
            updateYearSuggestions()

            // Khôi phục nút và hiển thị modal
            btn.innerHTML = originalHTML
            btn.disabled = false
            document.getElementById("exportModalOverlay").style.display = "flex"
        })

        // Xử lý sự kiện đóng modal
        document.getElementById("closeExportModal").addEventListener("click", closeModal)
        document.getElementById("cancelExport").addEventListener("click", closeModal)
        document.getElementById("exportModalOverlay").addEventListener("click", function (e) {
            if (e.target === this) {
                closeModal()
            }
        })

        // Xử lý chuyển đổi giữa các loại xuất
        document.getElementById("exportTypeSingle").addEventListener("change", toggleExportFields)
        document.getElementById("exportTypeRange").addEventListener("change", toggleExportFields)

        // Xử lý sự kiện xuất Excel
        document.getElementById("confirmExport").addEventListener("click", exportExcel)
    }

    // Khởi tạo các year input với autocomplete
    function initializeYearInputs() {
        const yearInputs = ["singleYear", "fromYear", "toYear"]

        yearInputs.forEach((inputId) => {
            const input = document.getElementById(inputId)
            const suggestionsId = inputId + "Suggestions"
            const suggestions = document.getElementById(suggestionsId)

            if (input && suggestions) {
                setupYearInput(input, suggestions)
            }
        })
    }

    // Thiết lập year input với autocomplete
    function setupYearInput(input, suggestions) {
        let highlightedIndex = -1

        // Đặt giá trị mặc định
        input.value = currentYear

        // Xử lý sự kiện input
        input.addEventListener("input", function () {
            const value = this.value
            showYearSuggestions(value, suggestions)
            highlightedIndex = -1
        })

        // Xử lý sự kiện focus
        input.addEventListener("focus", function () {
            showYearSuggestions(this.value, suggestions)
        })

        // Xử lý sự kiện blur
        input.addEventListener("blur", () => {
            setTimeout(() => {
                suggestions.style.display = "none"
            }, 200)
        })

        // Xử lý phím điều hướng
        input.addEventListener("keydown", (e) => {
            const items = suggestions.querySelectorAll(".year-suggestion-item")

            if (e.key === "ArrowDown") {
                e.preventDefault()
                highlightedIndex = Math.min(highlightedIndex + 1, items.length - 1)
                updateHighlight(items, highlightedIndex)
            } else if (e.key === "ArrowUp") {
                e.preventDefault()
                highlightedIndex = Math.max(highlightedIndex - 1, -1)
                updateHighlight(items, highlightedIndex)
            } else if (e.key === "Enter") {
                e.preventDefault()
                if (highlightedIndex >= 0 && items[highlightedIndex]) {
                    selectYear(items[highlightedIndex].textContent, input, suggestions)
                }
            } else if (e.key === "Escape") {
                suggestions.style.display = "none"
                highlightedIndex = -1
            }
        })
    }

    // Hiển thị gợi ý năm
    function showYearSuggestions(value, suggestions) {
        const filteredYears = availableYears.filter((year) => year.toString().includes(value) || value === "")

        if (filteredYears.length > 0) {
            suggestions.innerHTML = filteredYears
                .map(
                    (year) =>
                        `<div class="year-suggestion-item" onclick="selectYear('${year}', this.parentElement.previousElementSibling, this.parentElement)">${year}</div>`,
                )
                .join("")
            suggestions.style.display = "block"
        } else {
            suggestions.style.display = "none"
        }
    }

    // Cập nhật highlight
    function updateHighlight(items, index) {
        items.forEach((item, i) => {
            if (i === index) {
                item.classList.add("highlighted")
            } else {
                item.classList.remove("highlighted")
            }
        })
    }

    // Chọn năm
    function selectYear(year, input, suggestions) {
        input.value = year
        suggestions.style.display = "none"
    }

    // Cập nhật gợi ý năm sau khi tải từ API
    function updateYearSuggestions() {
        const yearInputs = ["singleYear", "fromYear", "toYear"]

        yearInputs.forEach((inputId) => {
            const input = document.getElementById(inputId)
            if (input && !input.value) {
                input.value = currentYear
            }
        })
    }

    // Hàm đóng modal
    function closeModal() {
        document.getElementById("exportModalOverlay").style.display = "none"
    }

    // Hàm chuyển đổi giữa các loại xuất
    function toggleExportFields() {
        const isSingle = document.getElementById("exportTypeSingle").checked
        document.getElementById("singleMonthFields").style.display = isSingle ? "block" : "none"
        document.getElementById("rangeMonthFields").style.display = isSingle ? "none" : "block"
    }

    // Hàm xuất Excel
    function exportExcel() {
        const exportType = document.querySelector('input[name="exportType"]:checked').value
        let url = "/ExcelExport/ExportDoanhThu?exportType=" + exportType

        if (exportType === "single") {
            const month = document.getElementById("singleMonth").value
            const year = document.getElementById("singleYear").value

            if (!year || !isValidYear(year)) {
                alert("Vui lòng nhập năm hợp lệ!")
                return
            }

            url += `&fromMonth=${month}&fromYear=${year}`
        } else {
            const fromMonth = document.getElementById("fromMonth").value
            const fromYear = document.getElementById("fromYear").value
            const toMonth = document.getElementById("toMonth").value
            const toYear = document.getElementById("toYear").value

            if (!fromYear || !toYear || !isValidYear(fromYear) || !isValidYear(toYear)) {
                alert("Vui lòng nhập năm hợp lệ!")
                return
            }

            url += `&fromMonth=${fromMonth}&fromYear=${fromYear}&toMonth=${toMonth}&toYear=${toYear}`

            // Kiểm tra tính hợp lệ của khoảng thời gian
            const fromDate = new Date(fromYear, fromMonth - 1)
            const toDate = new Date(toYear, toMonth - 1)

            if (fromDate > toDate) {
                alert("Thời gian bắt đầu không thể sau thời gian kết thúc!")
                return
            }
        }

        // Chuyển hướng để tải file
        window.location.href = url

        // Đóng modal
        closeModal()
    }

    // Kiểm tra năm hợp lệ
    function isValidYear(year) {
        const yearNum = Number.parseInt(year)
        return !isNaN(yearNum) && yearNum >= 1900 && yearNum <= 2100
    }

    // Hàm tạo options cho tháng
    function generateMonthOptions() {
        let options = ""
        const currentMonth = new Date().getMonth() + 1

        for (let i = 1; i <= 12; i++) {
            options += `<option value="${i}" ${i === currentMonth ? "selected" : ""}>Tháng ${i}</option>`
        }

        return options
    }

    // Tạo hàm global để có thể gọi từ onclick
    window.selectYear = selectYear
})
