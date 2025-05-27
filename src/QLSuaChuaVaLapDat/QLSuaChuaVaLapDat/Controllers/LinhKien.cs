using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;
using System.IO; // Required for Path and File operations
using System;     // Required for Guid

namespace QLSuaChuaVaLapDat.Controllers
{
    public class LinhKienController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LinhKienController(QuanLySuaChuaVaLapDatContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: LinhKien (Index - No changes from your provided code)
        public async Task<IActionResult> Index(string searchString, string sortOrder, string currentFilter, int? pageNumber)
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
            // Ensure PaginatedList class is correctly implemented for your LinhKien model
            return View(await PaginatedList<LinhKien>.CreateAsync(linhKiens.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: LinhKien/Details/5 (No changes from your provided code)
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

        // GET: LinhKien/Create (No changes from your provided code)
        public IActionResult Create()
        {
            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien");
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx");
            return View();
        }

        // POST: LinhKien/Create (MODIFIED)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLinhKien,IdNsx,IdLoaiLinhKien,TenLinhKien,Gia,SoLuong,ThoiGianBaoHanh,DieuKienBaoHanh,ImageFile")] LinhKien linhKien)
        {
            // Removed "Anh" from Bind as it will be populated from ImageFile
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
                        linhKien.Anh = "/img/" + uniqueFileName; // Store the relative path
                    }
                    else
                    {
                        linhKien.Anh = null; // Or a default image path if you have one
                    }

                    _context.Add(linhKien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm linh kiện thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKien.IdLoaiLinhKien);
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKien.IdNsx);
            return View(linhKien);
        }

        // GET: LinhKien/Edit/5 (No changes from your provided code)
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linhKien = await _context.LinhKiens.FindAsync(id);
            if (linhKien == null)
            {
                return NotFound();
            }
            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKien.IdLoaiLinhKien);
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKien.IdNsx);
            return View(linhKien);
        }

        // POST: LinhKien/Edit/5 (MODIFIED SIGNIFICANTLY)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdLinhKien,IdNsx,IdLoaiLinhKien,TenLinhKien,Gia,SoLuong,ThoiGianBaoHanh,DieuKienBaoHanh,ImageFile")] LinhKien linhKienViewModel)
        {
            // Note: Parameter name changed to linhKienViewModel to distinguish from the entity loaded from DB.
            // "Anh" is intentionally NOT bound here; we manage it manually based on ImageFile and DB state.

            if (id != linhKienViewModel.IdLinhKien)
            {
                return NotFound();
            }

            // If you have client-side validation that makes ImageFile required,
            // but it's optional for Edit, you might need to clear its ModelState error.
            // ModelState.Remove("ImageFile"); // Example if ImageFile is not mandatory for update

            if (ModelState.IsValid)
            {
                var linhKienToUpdate = await _context.LinhKiens.FirstOrDefaultAsync(lk => lk.IdLinhKien == id);

                if (linhKienToUpdate == null)
                {
                    return NotFound();
                }

                string oldImagePath = linhKienToUpdate.Anh; // Store old image path

                // Update scalar properties from the view model
                linhKienToUpdate.IdNsx = linhKienViewModel.IdNsx;
                linhKienToUpdate.IdLoaiLinhKien = linhKienViewModel.IdLoaiLinhKien;
                linhKienToUpdate.TenLinhKien = linhKienViewModel.TenLinhKien;
                linhKienToUpdate.Gia = linhKienViewModel.Gia;
                linhKienToUpdate.SoLuong = linhKienViewModel.SoLuong;
                linhKienToUpdate.ThoiGianBaoHanh = linhKienViewModel.ThoiGianBaoHanh;
                linhKienToUpdate.DieuKienBaoHanh = linhKienViewModel.DieuKienBaoHanh;

                if (linhKienViewModel.ImageFile != null && linhKienViewModel.ImageFile.Length > 0)
                {
                    // New file is uploaded, process it
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
                    linhKienToUpdate.Anh = "/img/" + uniqueFileName; // Set new image path

                    // Delete the old image if it existed and is different from the new one
                    if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != linhKienToUpdate.Anh)
                    {
                        string fullOldPathOnServer = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(fullOldPathOnServer))
                        {
                            System.IO.File.Delete(fullOldPathOnServer);
                        }
                    }
                }
                // else: No new file uploaded. linhKienToUpdate.Anh retains its value from the database.
                // The hidden field for "Anh" in the form is not strictly necessary for this server-side logic
                // as we load the current value from the database (linhKienToUpdate.Anh).

                try
                {
                    _context.Update(linhKienToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật linh kiện thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinhKienExists(linhKienToUpdate.IdLinhKien))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, repopulate ViewData and return view
            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKienViewModel.IdLoaiLinhKien);
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKienViewModel.IdNsx);
            return View(linhKienViewModel);
        }

        // GET: LinhKien/Delete/5 (No changes from your provided code)
        public async Task<IActionResult> Delete(string id)
        {
            // ... (your existing code)
            if (id == null)
            {
                return NotFound();
            }

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

        // POST: LinhKien/Delete/5 (Consider deleting the image file)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var linhKien = await _context.LinhKiens.FindAsync(id);
            if (linhKien != null)
            {
                // Optionally, delete the associated image file
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

            return RedirectToAction(nameof(Index));
        }

        private bool LinhKienExists(string id)
        {
            return _context.LinhKiens.Any(e => e.IdLinhKien == id);
        }
    }
}