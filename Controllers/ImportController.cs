using LibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace LibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("excel")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file Excel");

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Chỉ hỗ trợ file .xlsx");

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportBooksFromExcelAsync(stream);

            return Ok(result);
        }

        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("LibraryManagement");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Danh sách sách");

            // ── Định nghĩa cột ────────────────────────────────────────────────
            var columns = new[]
            {
                ("Tên sách",        "Conan (Thám Tử Lừng Danh)",  true),
                ("Tác giả",         "Gosho Aoyama",                false),
                ("Thể loại",        "Truyện tranh",                false),
                ("NXB",             "Kim Đồng",                    false),
                ("Tổng số tập",     "107",                         false),
                ("Tập đã có",       "1-50,55,60",                  false),
                ("Trạng thái đọc",  "Đang đọc",                    false),
                ("Ghi chú",         "Đang chờ mua thêm tập 51-54", false),
            };

            // ── Header row ───────────────────────────────────────────────────
            for (int i = 0; i < columns.Length; i++)
            {
                var cell = ws.Cells[1, i + 1];
                cell.Value = columns[i].Item1;

                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 11;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(columns[i].Item3
                    ? Color.FromArgb(249, 168, 212)   // pink — bắt buộc
                    : Color.FromArgb(186, 230, 253));  // blue — tuỳ chọn
                cell.Style.Font.Color.SetColor(Color.FromArgb(74, 74, 106));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(200, 200, 220));
            }

            // ── Dòng ví dụ 1 (bộ nhiều tập) ────────────────────────────────
            var sample1 = new[] { "Conan (Thám Tử Lừng Danh)", "Gosho Aoyama", "Truyện tranh", "Kim Đồng", "107", "1-50,55,60", "Đang đọc", "Đang chờ mua thêm tập 51-54" };
            for (int i = 0; i < sample1.Length; i++)
            {
                var cell = ws.Cells[2, i + 1];
                cell.Value = sample1[i];
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 241, 249));
                cell.Style.Border.BorderAround(ExcelBorderStyle.Hair, Color.FromArgb(230, 220, 240));
            }

            // ── Dòng ví dụ 2 (sách lẻ — để trống Tổng số tập và Tập đã có) ─
            var sample2 = new[] { "Cô Gái Từ Hôm Qua", "Nguyễn Nhật Ánh", "Văn học Việt Nam", "NXB Trẻ", "1", "", "Đã đọc", "Sách lẻ, rất hay!" };
            for (int i = 0; i < sample2.Length; i++)
            {
                var cell = ws.Cells[3, i + 1];
                cell.Value = sample2[i];
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 241, 249));
                cell.Style.Border.BorderAround(ExcelBorderStyle.Hair, Color.FromArgb(230, 220, 240));
            }

            // ── Dòng hướng dẫn (italic, màu xám) ───────────────────────────
            var notes = new[]
            {
                "← Bắt buộc",
                "← Tuỳ chọn",
                "← Tên đúng thể loại đã có hoặc tự tạo mới",
                "← Tuỳ chọn",
                "← Số nguyên ≥ 1. Để trống = 1 tập (sách lẻ)",
                "← VD: 1-5,7,9 | Để trống = sở hữu tất cả",
                "← Chưa đọc / Đang đọc / Đã đọc",
                "← Tuỳ chọn",
            };
            for (int i = 0; i < notes.Length; i++)
            {
                var cell = ws.Cells[4, i + 1];
                cell.Value = notes[i];
                cell.Style.Font.Italic = true;
                cell.Style.Font.Size = 9;
                cell.Style.Font.Color.SetColor(Color.FromArgb(150, 130, 170));
            }

            // ── Auto-fit columns ─────────────────────────────────────────────
            ws.Cells[ws.Dimension.Address].AutoFitColumns(12, 40);
            ws.Row(1).Height = 24;
            ws.Row(4).Height = 18;

            // ── Freeze header row ────────────────────────────────────────────
            ws.View.FreezePanes(2, 1);

            var bytes = package.GetAsByteArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "template_import_sach.xlsx");
        }
    }
}
