const rangeMin = document.getElementById("rangeMin");
const rangeMax = document.getElementById("rangeMax");
const minPrice = document.getElementById("minPrice");
const maxPrice = document.getElementById("maxPrice");
const sliderRange = document.getElementById("slider-range");

function formatCurrency(value) {
    return parseInt(value).toLocaleString('vi-VN') + "₫";
}

function updateSlider() {
    let minVal = parseInt(rangeMin.value);
    let maxVal = parseInt(rangeMax.value);

    if (minVal > maxVal - 1000000) {
        minVal = maxVal - 1000000;
        rangeMin.value = minVal;
    }
    if (maxVal < minVal + 1000000) {
        maxVal = minVal + 1000000;
        rangeMax.value = maxVal;
    }

    const rangeWidth = parseInt(rangeMin.max) - parseInt(rangeMin.min);
    const leftPercent = ((minVal - parseInt(rangeMin.min)) / rangeWidth) * 100;
    const rightPercent = ((maxVal - parseInt(rangeMin.min)) / rangeWidth) * 100;

    sliderRange.style.left = leftPercent + "%";
    sliderRange.style.width = (rightPercent - leftPercent) + "%";

    minPrice.value = formatCurrency(minVal);
    maxPrice.value = formatCurrency(maxVal);
}

rangeMin.addEventListener("input", updateSlider);
rangeMax.addEventListener("input", updateSlider);
window.addEventListener("DOMContentLoaded", updateSlider);