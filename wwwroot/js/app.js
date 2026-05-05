const API = '/api';

function app() {
  return {
    // ── State ──────────────────────────────────
    currentPage: 'dashboard',
    books: [], categories: [], seriesList: [],
    recentBooks: [],
    stats: { totalBooks: 0, completedBooks: 0, readingBooks: 0, notStartedBooks: 0 },

    bookSearch: '', bookCategoryFilter: '', bookStatusFilter: '',
    categorySearch: '', seriesSearch: '',

    showBookModal: false, showCategoryModal: false, showSeriesModal: false,
    editingBook: {}, editingCategory: {}, editingSeries: {},

    importing: false, importResult: null,
    selectedSeries: null,    // Series đang xem mục lục
    showVolumeMap: false,    // Hiển thị modal mục lục
    
    // ── Init ───────────────────────────────────
    async init() {
      await Promise.all([this.loadBooks(), this.loadCategories(), this.loadSeries()]);
      await this.loadDashboard();
    },

    // ── Dashboard ──────────────────────────────
    async loadDashboard() {
      this.stats.totalBooks       = this.books.length;
      this.stats.completedBooks   = this.books.filter(b => b.readingStatus === 2).length;
      this.stats.readingBooks     = this.books.filter(b => b.readingStatus === 1).length;
      this.stats.notStartedBooks  = this.books.filter(b => b.readingStatus === 0).length;
      this.recentBooks = [...this.books].sort((a, b) =>
        new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 5);
    },

    // ── Books ──────────────────────────────────
    async loadBooks() {
      const params = new URLSearchParams();
      if (this.bookSearch)         params.set('search', this.bookSearch);
      if (this.bookCategoryFilter) params.set('categoryId', this.bookCategoryFilter);
      if (this.bookStatusFilter)   params.set('status', this.bookStatusFilter);

      const res = await fetch(`${API}/books?${params}`);
      this.books = await res.json();
      this.loadDashboard();
    },

    openBookModal(book = null) {
      this.editingBook = book ? { ...book } : { readingStatus: 0 };
      this.showBookModal = true;
    },

    async saveBook() {
      const method = this.editingBook.id ? 'PUT' : 'POST';
      const url = this.editingBook.id ? `${API}/books/${this.editingBook.id}` : `${API}/books`;
      await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this.editingBook)
      });
      this.showBookModal = false;
      await this.loadBooks();
    },

    async deleteBook(id) {
      if (!confirm('Xoá cuốn sách này?')) return;
      await fetch(`${API}/books/${id}`, { method: 'DELETE' });
      this.showBookModal = false;
      await this.loadBooks();
    },

    // ── Categories ─────────────────────────────
    async loadCategories() {
      const params = new URLSearchParams();
      if (this.categorySearch) params.set('search', this.categorySearch);
      const res = await fetch(`${API}/categories?${params}`);
      this.categories = await res.json();
    },

    openCategoryModal(cat = null) {
      this.editingCategory = cat ? { ...cat } : {};
      this.showCategoryModal = true;
    },

    async saveCategory() {
      const method = this.editingCategory.id ? 'PUT' : 'POST';
      const url = this.editingCategory.id
        ? `${API}/categories/${this.editingCategory.id}` : `${API}/categories`;
      await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this.editingCategory)
      });
      this.showCategoryModal = false;
      await this.loadCategories();
    },

    async deleteCategory(id) {
      if (!confirm('Xoá thể loại này?')) return;
      const res = await fetch(`${API}/categories/${id}`, { method: 'DELETE' });
      if (!res.ok) { const d = await res.json(); alert(d.error); return; }
      await this.loadCategories();
    },

    // ── Series ─────────────────────────────────
    async loadSeries() {
      const params = new URLSearchParams();
      if (this.seriesSearch) params.set('search', this.seriesSearch);
      const res = await fetch(`${API}/series?${params}`);
      this.seriesList = await res.json();
    },
    
    // Hàm mở mục lục
    openVolumeMap(s) {
        this.selectedSeries = s;
        this.showVolumeMap = true;
    },
    openSeriesModal(s = null) {
        this.editingSeries = s ? { ...s } : { totalVolumes: null, notes: '' };
        this.showSeriesModal = true;
    },    
    
    async saveSeries() {
      const method = this.editingSeries.id ? 'PUT' : 'POST';
      const url = this.editingSeries.id
        ? `${API}/series/${this.editingSeries.id}` : `${API}/series`;
      await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(this.editingSeries)
      });
      this.showSeriesModal = false;
      await this.loadSeries();
    },
    async saveSeriesNotes() {
        await fetch(`${API}/series/${this.selectedSeries.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(this.selectedSeries)
        });
        // Cập nhật lại list
        await this.loadSeries();
        // Sync lại selectedSeries
        this.selectedSeries = this.seriesList.find(s => s.id === this.selectedSeries.id);
        alert('✅ Đã lưu ghi chú!');
    },
    async deleteSeries(id) {
      if (!confirm('Xoá bộ sách này?')) return;
      const res = await fetch(`${API}/series/${id}`, { method: 'DELETE' });
      if (!res.ok) { const d = await res.json(); alert(d.error); return; }
      await this.loadSeries();
    },

    // ── Import ─────────────────────────────────
    async handleFileSelect(e) {
      const file = e.target.files[0];
      if (file) await this.uploadFile(file);
    },

    async handleFileDrop(e) {
      const file = e.dataTransfer.files[0];
      if (file) await this.uploadFile(file);
    },

    async uploadFile(file) {
      this.importing = true;
      this.importResult = null;
      const formData = new FormData();
      formData.append('file', file);
      try {
        const res = await fetch(`${API}/import/excel`, { method: 'POST', body: formData });
        this.importResult = await res.json();
        await this.loadBooks();
        await this.loadCategories();
        await this.loadSeries();
      } catch (e) {
        alert('Import thất bại: ' + e.message);
      }
      this.importing = false;
    },

    // ── Helpers ────────────────────────────────
    getCoverStyle(categoryName) {
      const map = {
        'Đam mỹ':             'background: linear-gradient(135deg,#fce7f3,#fbcfe8)',
        'Ngôn tình':          'background: linear-gradient(135deg,#fef3c7,#fed7aa)',
        'Văn học Việt Nam':   'background: linear-gradient(135deg,#dcfce7,#bbf7d0)',
        'Văn học Châu Á':     'background: linear-gradient(135deg,#e0f2fe,#bae6fd)',
        'Văn học nước ngoài': 'background: linear-gradient(135deg,#ede9fe,#ddd6fe)',
        'Sách ngoại văn':     'background: linear-gradient(135deg,#fff7ed,#fed7aa)',
        'Truyện tranh':       'background: linear-gradient(135deg,#fdf4ff,#f5d0fe)',
      };
      return map[categoryName] || 'background: linear-gradient(135deg,#f3f4f6,#e5e7eb)';
    }
  };
}