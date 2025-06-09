using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using System.IO;
using System;

namespace QLSuaChuaVaLapDat.Controllers
{
    public class LinhKienController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<LinhKienController> _logger;

        public LinhKienController(QuanLySuaChuaVaLapDatContext context, IWebHostEnvironment webHostEnvironment, ILogger<LinhKienController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: LinhKien
        public async Task<IActionResult> Index(string searchString, string sortOrder, string currentFilter, int? pageNumber)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
                ViewData["QuantitySortParm"] = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";

                if (searchString != null)
                {
                    pageNumber = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewData["CurrentFilter"] = searchString;

                var linhKiens = from s in _context.LinhKiens
                                   .Include(l => l.IdLoaiLinhKienNavigation)
                                   .Include(l => l.IdNsxNavigation)
                                select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    linhKiens = linhKiens.Where(s => s.TenLinhKien.Contains(searchString)
                                           || s.IdLinhKien.Contains(searchString));
                }

                switch (sortOrder)
                {
                    case "name_desc":
                        linhKiens = linhKiens.OrderByDescending(s => s.TenLinhKien);
                        break;
                    case "Price":
                        linhKiens = linhKiens.OrderBy(s => s.Gia);
                        break;
                    case "price_desc":
                        linhKiens = linhKiens.OrderByDescending(s => s.Gia);
                        break;
                    case "Quantity":
                        linhKiens = linhKiens.OrderBy(s => s.SoLuong);
                        break;
                    case "quantity_desc":
                        linhKiens = linhKiens.OrderByDescending(s => s.SoLuong);
                        break;
                    default:
                        linhKiens = linhKiens.OrderBy(s => s.TenLinhKien);
                        break;
                }

                int pageSize = 10;
                return View(await PaginatedList<LinhKien>.CreateAsync(linhKiens.AsNoTracking(), pageNumber ?? 1, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách linh kiện");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách linh kiện. Vui lòng kiểm tra cấu hình database.";
                return View(new PaginatedList<LinhKien>(new List<LinhKien>(), 0, 1, 10));
            }
        }

        // GET: LinhKien/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var linhKien = await _context.LinhKiens
                    .Include(l => l.IdLoaiLinhKienNavigation)
                    .Include(l => l.IdNsxNavigation)
                    .FirstOrDefaultAsync(m => m.IdLinhKien == id);

                if (linhKien == null)
                {
                    return NotFound();
                }

                return View(linhKien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết linh kiện {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin linh kiện.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LinhKien/Create
        public IActionResult Create()
        {
            try
            {
                ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien");
                ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form tạo linh kiện");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LinhKien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLinhKien,IdNsx,IdLoaiLinhKien,TenLinhKien,Gia,SoLuong,ThoiGianBaoHanh,DieuKienBaoHanh,ImageFile")] LinhKien linhKien)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingLinhKien = await _context.LinhKiens.FindAsync(linhKien.IdLinhKien);
                    if (existingLinhKien != null)
                    {
                        ModelState.AddModelError("IdLinhKien", "ID linh kiện đã tồn tại.");
                    }
                    else
                    {
                        if (linhKien.ImageFile != null && linhKien.ImageFile.Length > 0)
                        {
                            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(linhKien.ImageFile.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await linhKien.ImageFile.CopyToAsync(fileStream);
                            }
                            linhKien.Anh = "/img/" + uniqueFileName;
                        }
                        else
                        {
                            linhKien.Anh = null;
                        }

                        _context.Add(linhKien);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Thêm linh kiện thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo linh kiện");
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu. Vui lòng thử lại.");
            }

            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKien.IdLoaiLinhKien);
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKien.IdNsx);
            return View(linhKien);
        }

        // GET: LinhKien/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var linhKien = await _context.LinhKiens.FindAsync(id);
                if (linhKien == null)
                {
                    return NotFound();
                }
                ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKien.IdLoaiLinhKien);
                ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKien.IdNsx);
                return View(linhKien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form chỉnh sửa linh kiện {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin linh kiện.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LinhKien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdLinhKien,IdNsx,IdLoaiLinhKien,TenLinhKien,Gia,SoLuong,ThoiGianBaoHanh,DieuKienBaoHanh,ImageFile")] LinhKien linhKienViewModel)
        {
            if (id != linhKienViewModel.IdLinhKien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var linhKienToUpdate = await _context.LinhKiens.FirstOrDefaultAsync(lk => lk.IdLinhKien == id);

                    if (linhKienToUpdate == null)
                    {
                        return NotFound();
                    }

                    string oldImagePath = linhKienToUpdate.Anh;

                    // Update scalar properties
                    linhKienToUpdate.IdNsx = linhKienViewModel.IdNsx;
                    linhKienToUpdate.IdLoaiLinhKien = linhKienViewModel.IdLoaiLinhKien;
                    linhKienToUpdate.TenLinhKien = linhKienViewModel.TenLinhKien;
                    linhKienToUpdate.Gia = linhKienViewModel.Gia;
                    linhKienToUpdate.SoLuong = linhKienViewModel.SoLuong;
                    linhKienToUpdate.ThoiGianBaoHanh = linhKienViewModel.ThoiGianBaoHanh;
                    linhKienToUpdate.DieuKienBaoHanh = linhKienViewModel.DieuKienBaoHanh;

                    if (linhKienViewModel.ImageFile != null && linhKienViewModel.ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(linhKienViewModel.ImageFile.FileName);
                        string newFilePathOnServer = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(newFilePathOnServer, FileMode.Create))
                        {
                            await linhKienViewModel.ImageFile.CopyToAsync(fileStream);
                        }
                        linhKienToUpdate.Anh = "/img/" + uniqueFileName;

                        // Delete old image
                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != linhKienToUpdate.Anh)
                        {
                            string fullOldPathOnServer = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullOldPathOnServer))
                            {
                                System.IO.File.Delete(fullOldPathOnServer);
                            }
                        }
                    }

                    _context.Update(linhKienToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật linh kiện thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinhKienExists(linhKienViewModel.IdLinhKien))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật linh kiện {Id}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật dữ liệu. Vui lòng thử lại.");
                }
            }

            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKienViewModel.IdLoaiLinhKien);
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKienViewModel.IdNsx);
            return View(linhKienViewModel);
        }

        // GET: LinhKien/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var linhKien = await _context.LinhKiens
                    .Include(l => l.IdLoaiLinhKienNavigation)
                    .Include(l => l.IdNsxNavigation)
                    .FirstOrDefaultAsync(m => m.IdLinhKien == id);

                if (linhKien == null)
                {
                    return NotFound();
                }

                return View(linhKien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form xóa linh kiện {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin linh kiện.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LinhKien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var linhKien = await _context.LinhKiens.FindAsync(id);
                if (linhKien != null)
                {
                    // Delete associated image file
                    if (!string.IsNullOrEmpty(linhKien.Anh))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, linhKien.Anh.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    _context.LinhKiens.Remove(linhKien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa linh kiện thành công!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa linh kiện {Id}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa linh kiện. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LinhKienExists(string id)
        {
            return _context.LinhKiens.Any(e => e.IdLinhKien == id);
        }
    }
}
