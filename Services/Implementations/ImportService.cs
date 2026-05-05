using LibraryManagement.Data;
using LibraryManagement.DTOs;
using LibraryManagement.Models;
using LibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LibraryManagement.Services.Implementations
{
    public class ImportService : IImportService
    {
        private readonly BookDbContext _context;

        public ImportService(BookDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResultDto> ImportBooksFromExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var result = new ImportResultDto();

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
            {
                result.Errors.Add("File Excel không có dữ liệu");
                return result;
            }

            // Đọc header row để map cột
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                    headers[header] = col;
            }

            // Validate headers
            if (!headers.ContainsKey("Tên sách"))
            {
                result.Errors.Add("File Excel thiếu cột 'Tên sách'");
                return result;
            }

            result.TotalRows = rowCount - 1;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var bookName = GetCell(worksheet, row, headers, "Tên sách");
                    if (string.IsNullOrWhiteSpace(bookName))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    // Lấy các giá trị từ Excel
                    var author = GetCell(worksheet, row, headers, "Tác giả");
                    var categoryName = GetCell(worksheet, row, headers, "Thể loại");
                    var seriesName = GetCell(worksheet, row, headers, "Bộ sách");
                    var volumeStr = GetCell(worksheet, row, headers, "Tập số");
                    var statusStr = GetCell(worksheet, row, headers, "Trạng thái đọc");
                    var notes = GetCell(worksheet, row, headers, "Ghi chú");

                    // Lấy hoặc tạo Category
                    var category = await GetOrCreateCategoryAsync(categoryName);

                    // Lấy hoặc tạo Series
                    Series? series = null;
                    if (!string.IsNullOrWhiteSpace(seriesName))
                        series = await GetOrCreateSeriesAsync(seriesName, author);

                    // Parse volume number
                    int? volumeNumber = null;
                    if (int.TryParse(volumeStr, out var vol))
                        volumeNumber = vol;

                    // Parse reading status
                    var readingStatus = ParseReadingStatus(statusStr);

                    // Kiểm tra sách đã tồn tại chưa
                    var existingBook = await _context.Books
                        .FirstOrDefaultAsync(b => b.Title == bookName && b.CategoryId == category.Id);

                    if (existingBook != null)
                    {
                        result.SkippedCount++;
                        result.SkippedBooks.Add($"{bookName} (đã tồn tại)");
                        continue;
                    }

                    // Tạo sách mới
                    var book = new Book
                    {
                        Title = bookName,
                        Author = author,
                        CategoryId = category.Id,
                        SeriesId = series?.Id,
                        VolumeNumber = volumeNumber,
                        ReadingStatus = readingStatus,
                        Notes = notes,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Books.Add(book);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Dòng {row}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private static string GetCell(ExcelWorksheet ws, int row,
            Dictionary<string, int> headers, string columnName)
        {
            if (!headers.TryGetValue(columnName, out var col)) return string.Empty;
            return ws.Cells[row, col].Text.Trim();
        }

        private async Task<Category> GetOrCreateCategoryAsync(string? name)
        {
            var safeName = string.IsNullOrWhiteSpace(name) ? "Chưa phân loại" : name.Trim();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == safeName);

            if (category != null) return category;

            category = new Category { Name = safeName };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        private async Task<Series> GetOrCreateSeriesAsync(string name, string? author)
        {
            var series = await _context.Series
                .FirstOrDefaultAsync(s => s.Name == name);

            if (series != null) return series;

            series = new Series { Name = name, Author = author };
            _context.Series.Add(series);
            await _context.SaveChangesAsync();
            return series;
        }

        private static ReadingStatus ParseReadingStatus(string? status) =>
            status?.Trim() switch
            {
                "Đã đọc" => ReadingStatus.Completed,
                "Đang đọc" => ReadingStatus.Reading,
                _ => ReadingStatus.NotStarted
            };
    }
}
