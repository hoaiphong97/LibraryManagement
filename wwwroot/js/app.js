const API = '/api';

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

    toasts: [],
    _toastId: 0,

    // ── Init ───────────────────────────────────
    async init() {
        await Promise.all([this.loadCategories(), this.loadSeries()]);
        this.loadDashboard();
    },

    // ── Toast ──────────────────────────────────
    showToast(message, type = 'success', duration = 3500) {
        const id = ++this._toastId;
        this.toasts.push({ id, message, type });
        setTimeout(() => this.removeToast(id), duration);
    },

    removeToast(id) {
        this.toasts = this.toasts.filter(t => t.id !== id);
    },

    // ── Computed (client-side filtering) ───────
    get filteredSeriesList() {
        if (!this.seriesSearch?.trim()) return this.seriesList;
        const q = this.seriesSearch.trim().toLowerCase();
        return this.seriesList.filter(s =>
            s.name?.toLowerCase().includes(q) ||
            s.author?.toLowerCase().includes(q) ||
            s.categoryName?.toLowerCase().includes(q)
        );
    },

    get filteredCategories() {
        if (!this.categorySearch?.trim()) return this.categories;
        const q = this.categorySearch.trim().toLowerCase();
        return this.categories.filter(c =>
            c.name?.toLowerCase().includes(q) ||
            c.description?.toLowerCase().includes(q)
        );
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
            this.categories = await apiFetch(`${API}/categories`) || [];
        } catch (e) {
            this.showToast('Lỗi tải thể loại: ' + e.message, 'error');
            this.categories = [];
        }
    },

    openCategoryModal(cat = null) {
        this.editingCategory = cat ? { ...cat } : {};
        this.showCategoryModal = true;
    },

    async saveCategory() {
        if (!this.editingCategory.name?.trim()) {
            this.showToast('Vui lòng nhập tên thể loại', 'warning');
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
            this.showToast(this.editingCategory.id ? 'Đã cập nhật thể loại!' : 'Đã thêm thể loại mới!');
        } catch (e) {
            this.showToast('Lỗi lưu thể loại: ' + e.message, 'error');
        }
    },

    async deleteCategory(id) {
        if (!confirm('Xoá thể loại này?')) return;
        try {
            await apiFetch(`${API}/categories/${id}`, { method: 'DELETE' });
            await this.loadCategories();
            this.loadDashboard();
            this.showToast('Đã xoá thể loại.');
        } catch (e) {
            this.showToast('Không thể xóa: ' + e.message, 'error');
        }
    },

    // ── Series ─────────────────────────────────
    async loadSeries() {
        try {
            this.seriesList = await apiFetch(`${API}/series`) || [];
        } catch (e) {
            this.showToast('Lỗi tải bộ sách: ' + e.message, 'error');
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
            this.showToast('Vui lòng nhập tên bộ sách', 'warning');
            return;
        }
        try {
            const isEdit = !!this.editingSeries.id;
            const method = isEdit ? 'PUT' : 'POST';
            const url = isEdit
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
            this.showToast(isEdit ? 'Đã cập nhật bộ sách!' : 'Đã thêm bộ sách mới!');
        } catch (e) {
            this.showToast('Lỗi lưu bộ sách: ' + e.message, 'error');
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
            this.showToast('Đã lưu ghi chú!');
        } catch (e) {
            this.showToast('Lỗi lưu ghi chú: ' + e.message, 'error');
        }
    },

    async deleteSeries(id) {
        if (!confirm('Xoá bộ sách này và tất cả các tập đã có?')) return;
        try {
            await apiFetch(`${API}/series/${id}`, { method: 'DELETE' });
            await this.loadSeries();
            this.loadDashboard();
            this.showToast('Đã xoá bộ sách.');
        } catch (e) {
            this.showToast('Lỗi xóa bộ sách: ' + e.message, 'error');
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
            this.showToast('Vui lòng chọn file .xlsx', 'warning');
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
            this.showToast('Import thất bại: ' + e.message, 'error');
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
            this.showToast('Lỗi cập nhật tập: ' + e.message, 'error');
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
            this.showToast('Đã thêm tập mới!');
        } catch (e) {
            this.showToast('Lỗi thêm tập mới: ' + e.message, 'error');
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
    },

    toastClass(type) {
        const map = {
            success: 'bg-green-50 border-green-300 text-green-700',
            error:   'bg-red-50 border-red-300 text-red-600',
            warning: 'bg-orange-50 border-orange-300 text-orange-600',
            info:    'bg-blue-50 border-blue-300 text-blue-600',
        };
        return map[type] || map.info;
    },

    toastIcon(type) {
        const map = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' };
        return map[type] || map.info;
    }
  };
}
