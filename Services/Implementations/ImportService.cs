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
            ExcelPackage.License.SetNonCommercialPersonal("LibraryManagement");
            var result = new ImportResultDto();

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
            {
                result.Errors.Add("File Excel không có dữ liệu");
                return result;
            }

            // Đọc header (không phân biệt hoa thường)
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= worksheet.Dimension!.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                    headers[header] = col;
            }

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
                    var seriesName = GetCell(worksheet, row, headers, "Tên sách");
                    if (string.IsNullOrWhiteSpace(seriesName))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    // Bỏ qua nếu bộ sách đã tồn tại (theo tên)
                    var existing = await _context.Series
                        .FirstOrDefaultAsync(s => s.Name == seriesName);
                    if (existing != null)
                    {
                        result.SkippedCount++;
                        result.SkippedBooks.Add($"{seriesName} (đã tồn tại)");
                        continue;
                    }

                    var author = NullIfEmpty(GetCell(worksheet, row, headers, "Tác giả"));
                    var categoryName = GetCell(worksheet, row, headers, "Thể loại");
                    var publisher = NullIfEmpty(GetCell(worksheet, row, headers, "NXB"))
                                 ?? NullIfEmpty(GetCell(worksheet, row, headers, "Nhà xuất bản"));
                    var totalVolumesStr = GetCell(worksheet, row, headers, "Tổng số tập");
                    var ownedVolumesStr = GetCell(worksheet, row, headers, "Tập đã có");
                    var statusStr = GetCell(worksheet, row, headers, "Trạng thái đọc");
                    var notes = NullIfEmpty(GetCell(worksheet, row, headers, "Ghi chú"));

                    // Tổng số tập (mặc định 1 cho sách lẻ)
                    int totalVolumes = 1;
                    if (int.TryParse(totalVolumesStr, out var tv) && tv >= 1)
                        totalVolumes = tv;

                    // Các tập đã có (để trống = tất cả)
                    var ownedVolumes = ParseOwnedVolumes(ownedVolumesStr, totalVolumes);

                    var category = await GetOrCreateCategoryAsync(categoryName);
                    var readingStatus = ParseReadingStatus(statusStr);

                    // Tạo Series
                    var series = new Series
                    {
                        Name = seriesName,
                        Author = author,
                        Publisher = publisher,
                        TotalVolumes = totalVolumes,
                        Notes = notes,
                        CategoryId = category.Id,
                        IsOngoing = false
                    };
                    _context.Series.Add(series);
                    await _context.SaveChangesAsync();

                    // Tạo Book record cho mỗi tập đã có
                    foreach (var vol in ownedVolumes)
                    {
                        _context.Books.Add(new Book
                        {
                            Title = totalVolumes == 1
                                ? seriesName
                                : $"{seriesName} - Tập {vol}",
                            Author = author,
                            Publisher = publisher,
                            CategoryId = category.Id,
                            SeriesId = series.Id,
                            VolumeNumber = vol,
                            Edition = BookEdition.Standard,
                            ReadingStatus = readingStatus,
                            Notes = notes,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        result.TotalBooksCreated++;
                    }
                    await _context.SaveChangesAsync();
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Dòng {row}: {ex.Message}");
                }
            }

            return result;
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private static string GetCell(ExcelWorksheet ws, int row,
            Dictionary<string, int> headers, string columnName)
        {
            if (!headers.TryGetValue(columnName, out var col)) return string.Empty;
            return ws.Cells[row, col].Text.Trim();
        }

        private static string? NullIfEmpty(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        // Phân tích danh sách tập: "1-5,7,9" hoặc để trống = tất cả
        private static List<int> ParseOwnedVolumes(string? input, int totalVolumes)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Enumerable.Range(1, totalVolumes).ToList();

            var result = new HashSet<int>();
            foreach (var part in input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (part.Contains('-'))
                {
                    var sides = part.Split('-');
                    if (sides.Length == 2 &&
                        int.TryParse(sides[0].Trim(), out var start) &&
                        int.TryParse(sides[1].Trim(), out var end))
                    {
                        for (int i = start; i <= end && i <= totalVolumes; i++)
                            if (i >= 1) result.Add(i);
                    }
                }
                else if (int.TryParse(part, out var n) && n >= 1 && n <= totalVolumes)
                {
                    result.Add(n);
                }
            }
            return result.OrderBy(v => v).ToList();
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

        private static ReadingStatus ParseReadingStatus(string? status) =>
            status?.Trim() switch
            {
                "Đã đọc" => ReadingStatus.Completed,
                "Đang đọc" => ReadingStatus.Reading,
                _ => ReadingStatus.NotStarted
            };
    }
}
