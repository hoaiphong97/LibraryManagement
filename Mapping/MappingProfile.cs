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
                .ForMember(dest => dest.SeriesCount, opt => opt.MapFrom(src =>
                    src.Books.Select(b => b.SeriesId).Distinct().Count()))
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());

            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // Series mappings
            CreateMap<Series, SeriesDto>()
                .ForMember(d => d.OwnedVolumes, opt => opt.MapFrom(s =>
                    s.Books.Where(b => b.VolumeNumber > 0)
                           .Select(b => b.VolumeNumber)
                           .Distinct()
                           .OrderBy(v => v)
                           .ToList()))
                .ForMember(d => d.MissingVolumes, opt => opt.MapFrom(s => GetMissingVolumes(s)))
                .ForMember(d => d.CurrentVolumes, opt => opt.MapFrom(s =>
                    s.Books.Select(b => b.VolumeNumber).Distinct().Count()))
                .ForMember(d => d.Volumes, opt => opt.MapFrom(s => BuildVolumeInfos(s)))
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s =>
                    s.Category != null ? s.Category.Name : null));

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
                .Where(b => b.VolumeNumber > 0)
                .Select(b => b.VolumeNumber)
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

        private static List<VolumeInfoDto> BuildVolumeInfos(Series series)
        {
            var volumes = new List<VolumeInfoDto>();
            for (int i = 1; i <= series.TotalVolumes; i++)
            {
                var booksOfVolume = series.Books.Where(b => b.VolumeNumber == i).ToList();
                volumes.Add(new VolumeInfoDto
                {
                    VolumeNumber = i,
                    IsOwned = booksOfVolume.Any(),
                    Editions = booksOfVolume.Select(b => new BookEditionDto
                    {
                        BookId = b.Id,
                        Edition = b.Edition,
                        EditionText = GetEditionText(b.Edition),
                        ReadingStatus = b.ReadingStatus
                    }).ToList()
                });
            }
            return volumes;
        }

        private static string GetEditionText(BookEdition edition) => edition switch
        {
            BookEdition.Standard => "Bản thường",
            BookEdition.Special => "Bản đặc biệt",
            BookEdition.Limited => "Bản giới hạn",
            BookEdition.Collector => "Bản sưu tầm",
            _ => "Khác"
        };

    }
}
