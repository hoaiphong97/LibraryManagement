using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Models;

namespace LibraryManagement.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.Books.Count))
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());

            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // Series mappings
            CreateMap<Series, SeriesDto>()
                .ForMember(dest => dest.CurrentVolumes, opt => opt.MapFrom(src =>
                    src.Books.Count(b => b.VolumeNumber.HasValue)))
                .ForMember(dest => dest.OwnedVolumes, opt => opt.MapFrom(src =>   // ← THÊM
                    src.Books
                        .Where(b => b.VolumeNumber.HasValue)
                        .Select(b => b.VolumeNumber!.Value)
                        .OrderBy(v => v)
                        .ToList()))
                .ForMember(dest => dest.MissingVolumes, opt => opt.MapFrom(src =>
                    GetMissingVolumes(src)));

            CreateMap<CreateSeriesDto, Series>();
            CreateMap<UpdateSeriesDto, Series>();
            // Book mappings
            CreateMap<Book, BookDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.CategoryPath, opt => opt.MapFrom(src => GetCategoryPath(src.Category)))
                .ForMember(dest => dest.SeriesName, opt => opt.MapFrom(src => src.Series != null ? src.Series.Name : null))
                .ForMember(dest => dest.ReadingStatusText, opt => opt.MapFrom(src => GetReadingStatusText(src.ReadingStatus)));

            CreateMap<CreateBookDto, Book>();
            CreateMap<UpdateBookDto, Book>();
        }

        private static List<int> GetMissingVolumes(Series series)
        {
            var existingVolumes = series.Books
                .Where(b => b.VolumeNumber.HasValue)
                .Select(b => b.VolumeNumber!.Value)
                .OrderBy(v => v)
                .ToList();

            var missingVolumes = new List<int>();
            if (series.TotalVolumes.HasValue)
            {
                for (int i = 1; i <= series.TotalVolumes.Value; i++)
                {
                    if (!existingVolumes.Contains(i))
                    {
                        missingVolumes.Add(i);
                    }
                }
            }

            return missingVolumes;
        }

        private static string GetCategoryPath(Category category)
        {
            if (category == null) return string.Empty;

            // Nếu có parent thì ghép "Parent > Current"
            if (category.ParentCategory != null)
                return $"{category.ParentCategory.Name} > {category.Name}";

            return category.Name;
        }

        private static string GetReadingStatusText(ReadingStatus status)
        {
            return status switch
            {
                ReadingStatus.NotStarted => "Chưa đọc",
                ReadingStatus.Reading => "Đang đọc",
                ReadingStatus.Completed => "Đã đọc",
                _ => "Không xác định"
            };
        }
    }
}
