using AutoMapper;
using LibraryManagement.DTOs;
using LibraryManagement.Exceptions;
using LibraryManagement.Models;
using LibraryManagement.Repositories.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public BookService(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetBooksWithDetailsAsync();
            // Log tạm để debug
            foreach (var b in books)
            {
                Console.WriteLine($"Book: {b.Title}, Category: {b.Category?.Name}, Parent: {b.Category?.ParentCategory?.Name}");
            }
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetBookWithDetailsAsync(id);

            if (book == null)
                throw new NotFoundException($"Không tìm thấy sách với ID {id}");

            return _mapper.Map<BookDto>(book);
        }

        public async Task<IEnumerable<BookDto>> SearchBooksAsync(
            string? search,
            int? categoryId,
            int? seriesId,
            ReadingStatus? status)
        {
             var books = await _bookRepository.SearchBooksAsync(search, categoryId, seriesId, status);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
        {
            // Validate category exists
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId))
                throw new BadRequestException($"Không tìm thấy thể loại với ID {dto.CategoryId}");

            var book = _mapper.Map<Book>(dto);
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;

            var createdBook = await _bookRepository.AddAsync(book);
            return await GetBookByIdAsync(createdBook.Id);
        }

        public async Task UpdateBookAsync(int id, UpdateBookDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book == null)
                throw new NotFoundException($"Không tìm thấy sách với ID {id}");

            // Validate category exists
            if (!await _categoryRepository.ExistsAsync(dto.CategoryId))
                throw new BadRequestException($"Không tìm thấy thể loại với ID {dto.CategoryId}");

            _mapper.Map(dto, book);
            book.UpdatedAt = DateTime.UtcNow;

            await _bookRepository.UpdateAsync(book);
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book == null)
                throw new NotFoundException($"Không tìm thấy sách với ID {id}");

            await _bookRepository.DeleteAsync(book);
        }
    }
}
