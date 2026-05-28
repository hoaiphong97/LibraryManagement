const API = '/api';

// Wrapper fetch với error handling chuẩn
async function apiFetch(url, options = {}) {
    const res = await fetch(url, options);
    if (res.status === 204) return null;
    const data = await res.json().catch(() => null);
    if (!res.ok) {
        const msg = data?.error || data?.message || data?.title || `Lỗi HTTP ${res.status}`;
        throw new Error(msg);
    }
    return data;
}

function app() {
  return {
    // ── State ──────────────────────────────────
    currentPage: 'dashboard',
    categories: [],
    seriesList: [],
    recentSeries: [],
    stats: { totalSeries: 0, totalVolumes: 0, missingSeries: 0, totalCategories: 0 },

    categorySearch: '',
    seriesSearch: '',

    showCategoryModal: false,
    showSeriesModal: false,
    editingCategory: {},
    editingSeries: {},

    importing: false,
    importResult: null,
    selectedSeries: null,
    showVolumeMap: false,

    // ── Init ───────────────────────────────────
    async init() {
        await Promise.all([this.loadCategories(), this.loadSeries()]);
        this.loadDashboard();
    },

    // ── Dashboard ──────────────────────────────
    loadDashboard() {
        this.stats.totalSeries     = this.seriesList.length;
        this.stats.totalVolumes    = this.seriesList.reduce((sum, s) => sum + (s.currentVolumes || 0), 0);
        this.stats.missingSeries   = this.seriesList.filter(s => s.missingVolumes && s.missingVolumes.length > 0).length;
        this.stats.totalCategories = this.categories.length;
        this.recentSeries = [...this.seriesList].sort((a, b) => b.id - a.id).slice(0, 6);
    },

    // ── Categories ─────────────────────────────
    async loadCategories() {
        try {
            const params = new URLSearchParams();
            if (this.categorySearch) params.set('search', this.categorySearch);
            this.categories = await apiFetch(`${API}/categories?${params}`) || [];
        } catch (e) {
            alert('Lỗi tải thể loại: ' + e.message);
            this.categories = [];
        }
    },

    openCategoryModal(cat = null) {
        this.editingCategory = cat ? { ...cat } : {};
        this.showCategoryModal = true;
    },

    async saveCategory() {
        if (!this.editingCategory.name?.trim()) {
            alert('Vui lòng nhập tên thể loại');
            return;
        }
        try {
            const method = this.editingCategory.id ? 'PUT' : 'POST';
            const url = this.editingCategory.id
                ? `${API}/categories/${this.editingCategory.id}`
                : `${API}/categories`;
            await apiFetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.editingCategory)
            });
            this.showCategoryModal = false;
            await this.loadCategories();
            this.loadDashboard();
        } catch (e) {
            alert('Lỗi lưu thể loại: ' + e.message);
        }
    },

    async deleteCategory(id) {
        if (!confirm('Xoá thể loại này?')) return;
        try {
            await apiFetch(`${API}/categories/${id}`, { method: 'DELETE' });
            await this.loadCategories();
            this.loadDashboard();
        } catch (e) {
            alert('Không thể xóa: ' + e.message);
        }
    },

    // ── Series ─────────────────────────────────
    async loadSeries() {
        try {
            const params = new URLSearchParams();
            if (this.seriesSearch) params.set('search', this.seriesSearch);
            this.seriesList = await apiFetch(`${API}/series?${params}`) || [];
        } catch (e) {
            alert('Lỗi tải bộ sách: ' + e.message);
            this.seriesList = [];
        }
    },

    openVolumeMap(s) {
        this.selectedSeries = { ...s };
        this.showVolumeMap = true;
    },

    openSeriesModal(s = null) {
        this.editingSeries = s
            ? { ...s, ownedVolumeNumbers: [] }
            : {
                totalVolumes: 1,
                notes: '',
                isOngoing: false,
                ownedVolumeNumbers: [1],
                categoryId: ''
              };
        this.showSeriesModal = true;
    },

    async saveSeries() {
        if (!this.editingSeries.name?.trim()) {
            alert('Vui lòng nhập tên bộ sách');
            return;
        }
        try {
            const method = this.editingSeries.id ? 'PUT' : 'POST';
            const url = this.editingSeries.id
                ? `${API}/series/${this.editingSeries.id}`
                : `${API}/series`;
            await apiFetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.editingSeries)
            });
            this.showSeriesModal = false;
            await this.loadSeries();
            this.loadDashboard();
        } catch (e) {
            alert('Lỗi lưu bộ sách: ' + e.message);
        }
    },

    async saveSeriesNotes() {
        try {
            await apiFetch(`${API}/series/${this.selectedSeries.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this.selectedSeries)
            });
            await this.loadSeries();
            this.selectedSeries = this.seriesList.find(s => s.id === this.selectedSeries.id) || this.selectedSeries;
            alert('✅ Đã lưu ghi chú!');
        } catch (e) {
            alert('Lỗi lưu ghi chú: ' + e.message);
        }
    },

    async deleteSeries(id) {
        if (!confirm('Xoá bộ sách này và tất cả các tập đã có?')) return;
        try {
            await apiFetch(`${API}/series/${id}`, { method: 'DELETE' });
            await this.loadSeries();
            this.loadDashboard();
        } catch (e) {
            alert('Lỗi xóa bộ sách: ' + e.message);
        }
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
        if (!file.name.endsWith('.xlsx')) {
            alert('Vui lòng chọn file .xlsx');
            return;
        }
        this.importing = true;
        this.importResult = null;
        const formData = new FormData();
        formData.append('file', file);
        try {
            const res = await fetch(`${API}/import/excel`, { method: 'POST', body: formData });
            const data = await res.json().catch(() => null);
            if (!res.ok) {
                throw new Error(data?.error || data?.title || `Lỗi HTTP ${res.status}`);
            }
            this.importResult = data;
            await this.loadCategories();
            await this.loadSeries();
            this.loadDashboard();
        } catch (e) {
            alert('Import thất bại: ' + e.message);
        } finally {
            this.importing = false;
        }
    },

    // ── Series Modal Helpers ─────────────────────
    toggleOwnedVolume(volumeNumber) {
        if (!this.editingSeries.ownedVolumeNumbers)
            this.editingSeries.ownedVolumeNumbers = [];
        const idx = this.editingSeries.ownedVolumeNumbers.indexOf(volumeNumber);
        if (idx >= 0) {
            this.editingSeries.ownedVolumeNumbers.splice(idx, 1);
        } else {
            this.editingSeries.ownedVolumeNumbers.push(volumeNumber);
        }
    },

    selectAllVolumes() {
        const total = this.editingSeries.totalVolumes || 1;
        this.editingSeries.ownedVolumeNumbers = Array.from({ length: total }, (_, i) => i + 1);
    },

    clearAllVolumes() {
        this.editingSeries.ownedVolumeNumbers = [];
    },

    updateOwnedVolumesArray() {
        if (!this.editingSeries.ownedVolumeNumbers) return;
        const total = this.editingSeries.totalVolumes || 0;
        this.editingSeries.ownedVolumeNumbers =
            this.editingSeries.ownedVolumeNumbers.filter(v => v <= total);
    },

    // ── Volume Map ──────────────────────────────
    async toggleVolumeInSeries(volumeNumber) {
        try {
            await apiFetch(`${API}/series/${this.selectedSeries.id}/toggle-volume`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ volumeNumber, edition: 0 })
            });
            await this.loadSeries();
            this.loadDashboard();
            this.selectedSeries = this.seriesList.find(s => s.id === this.selectedSeries.id) || this.selectedSeries;
        } catch (e) {
            alert('Lỗi cập nhật tập: ' + e.message);
        }
    },

    async addNewVolume() {
        try {
            const updated = {
                ...this.selectedSeries,
                totalVolumes: (this.selectedSeries.totalVolumes || 0) + 1
            };
            await apiFetch(`${API}/series/${this.selectedSeries.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updated)
            });
            await this.loadSeries();
            this.selectedSeries = this.seriesList.find(s => s.id === this.selectedSeries.id) || this.selectedSeries;
        } catch (e) {
            alert('Lỗi thêm tập mới: ' + e.message);
        }
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
