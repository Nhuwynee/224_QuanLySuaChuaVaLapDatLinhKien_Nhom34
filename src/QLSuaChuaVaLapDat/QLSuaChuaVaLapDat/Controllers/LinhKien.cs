using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLSuaChuaVaLapDat.Models;

namespace QLSuaChuaVaLapDat.Controllers
{
    public class LinhKienController : Controller
    {
        private readonly QuanLySuaChuaVaLapDatContext _context;

        public LinhKienController(QuanLySuaChuaVaLapDatContext context)
        {
            _context = context;
        }

        // GET: LinhKien
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
            return View(await PaginatedList<LinhKien>.CreateAsync(linhKiens.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: LinhKien/Details/5
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

        // GET: LinhKien/Create
        public IActionResult Create()
        {
            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien");
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx");
            return View();
        }

        // POST: LinhKien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLinhKien,IdNsx,IdLoaiLinhKien,TenLinhKien,Gia,SoLuong,Anh,ThoiGianBaoHanh,DieuKienBaoHanh")] LinhKien linhKien)
        {
            if (ModelState.IsValid)
            {
                // Check if ID already exists
                var existingLinhKien = await _context.LinhKiens.FindAsync(linhKien.IdLinhKien);
                if (existingLinhKien != null)
                {
                    ModelState.AddModelError("IdLinhKien", "ID linh kiện đã tồn tại.");
                }
                else
                {
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

        // GET: LinhKien/Edit/5
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

        // POST: LinhKien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdLinhKien,IdNsx,IdLoaiLinhKien,TenLinhKien,Gia,SoLuong,Anh,ThoiGianBaoHanh,DieuKienBaoHanh")] LinhKien linhKien)
        {
            if (id != linhKien.IdLinhKien)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(linhKien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật linh kiện thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinhKienExists(linhKien.IdLinhKien))
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
            ViewData["IdLoaiLinhKien"] = new SelectList(_context.LoaiLinhKiens, "IdLoaiLinhKien", "TenLoaiLinhKien", linhKien.IdLoaiLinhKien);
            ViewData["IdNsx"] = new SelectList(_context.NhaSanXuats, "IdNsx", "TenNsx", linhKien.IdNsx);
            return View(linhKien);
        }

        // GET: LinhKien/Delete/5
        public async Task<IActionResult> Delete(string id)
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

        // POST: LinhKien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var linhKien = await _context.LinhKiens.FindAsync(id);
            if (linhKien != null)
            {
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
