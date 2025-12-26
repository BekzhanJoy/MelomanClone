using MelomanClone.Data;
using MelomanClone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;



namespace MelomanClone.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();

            return View(products);
        }
        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["Success"] = "Product created successfully";

            return RedirectToAction("Index");
        }
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            TempData["Success"] = "Product updated successfully";

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["Success"] = "Product deleted successfully";

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult ExportProductsToExcel()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Title";
            worksheet.Cells[1, 3].Value = "Category";
            worksheet.Cells[1, 4].Value = "Price";
            worksheet.Cells[1, 5].Value = "Image URL";

            using (var range = worksheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var product in products)
            {
                worksheet.Cells[row, 1].Value = product.Id;
                worksheet.Cells[row, 2].Value = product.Title;
                worksheet.Cells[row, 3].Value = product.Category?.Name;
                worksheet.Cells[row, 4].Value = product.Price;
                worksheet.Cells[row, 5].Value = product.ImageUrl;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Products.xlsx"
            );
        }
        [HttpGet]
        public IActionResult DownloadOrderReceipt(int id)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            using var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            document.Add(new Paragraph("Order Receipt")
                .SetFont(bold)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            document.Add(new Paragraph($"Order ID: {order.Id}")
                .SetFont(regular));

            document.Add(new Paragraph($"Date: {order.CreatedAt.ToLocalTime()}")
                .SetFont(regular));

            document.Add(new Paragraph($"Status: {order.Status}")
                .SetFont(regular)
                .SetMarginBottom(15));

            var table = new Table(4).UseAllAvailableWidth();

            table.AddHeaderCell(new Cell().Add(new Paragraph("Product").SetFont(bold)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Price").SetFont(bold)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Qty").SetFont(bold)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Total").SetFont(bold)));

            foreach (var item in order.Items)
            {
                table.AddCell(item.ProductTitle);
                table.AddCell($"{item.Price} ₸");
                table.AddCell(item.Quantity.ToString());
                table.AddCell($"{item.Price * item.Quantity} ₸");
            }

            document.Add(table);

            document.Add(new Paragraph($"\nGrand Total: {order.TotalPrice} ₸")
                .SetFont(bold)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Add(new Paragraph("\nThank you for your purchase!")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY));

            document.Close();

            return File(
                stream.ToArray(),
                "application/pdf",
                $"Order_{order.Id}_Receipt.pdf"
            );
        }
    }
}
