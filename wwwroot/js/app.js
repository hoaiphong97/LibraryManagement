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
    seriesStatusFilter: '',   // '' = tất cả, '0'=chưa đọc, '1'=đang đọc, '2'=đã đọc
    seriesCategoryFilter: '', // '' = tất cả, hoặc categoryId

    showCategoryModal: false,
    showSeriesModal: false,
    editingCategory: {},
    editingSeries: {},

    // Category detail (click để xem series)
    selectedCategory: null,

    importing: false,
    importResult: null,
    selectedSeries: null,
    showVolumeMap: false,

    preOrders: [],
    preOrderFilter: null,
    showPreOrderModal: false,
    editingPreOrder: {},

    // WishList
    wishList: [],
    showWishListModal: false,
    editingWishItem: {},

    toasts: [],
    _toastId: 0,

    // ── Init ───────────────────────────────────
    async init() {
        this.initParticles();
        await Promise.all([this.loadCategories(), this.loadSeries(), this.loadPreOrders(), this.loadWishList()]);
        this.loadDashboard();
    },

    // ── Particles (always moving) ───────────
    initParticles() {
        const container = document.getElementById('particles');
        if (!container) return;
        const emojis = ['🌸','📚','⭐','💕','✨','🌷','🎀','📖','🌟','🌺','💫','🦋','🍀','🌙'];
        for (let i = 0; i < 24; i++) {
            const el = document.createElement('span');
            el.textContent = emojis[Math.floor(Math.random() * emojis.length)];
            const size     = 13 + Math.random() * 14;
            const duration = 14 + Math.random() * 20;
            const delay    = -(Math.random() * duration);
            const left     = Math.random() * 100;
            Object.assign(el.style, {
                position:        'absolute',
                left:            `${left}%`,
                bottom:          '0',
                fontSize:        `${size}px`,
                animation:       `floatUp ${duration}s linear infinite`,
                animationDelay:  `${delay}s`,
                opacity:         '0',
                pointerEvents:   'none',
                userSelect:      'none',
                filter:          'drop-shadow(0 2px 4px rgba(249,168,212,0.25))',
            });
            container.appendChild(el);
        }
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
        let list = this.seriesList;
        if (this.seriesSearch?.trim()) {
            const q = this.seriesSearch.trim().toLowerCase();
            list = list.filter(s =>
                s.name?.toLowerCase().includes(q) ||
                s.author?.toLowerCase().includes(q) ||
                s.categoryName?.toLowerCase().includes(q)
            );
        }
        if (this.seriesStatusFilter !== '') {
            const status = parseInt(this.seriesStatusFilter);
            list = list.filter(s => s.readingStatus === status);
        }
        if (this.seriesCategoryFilter !== '') {
            const catId = parseInt(this.seriesCategoryFilter);
            list = list.filter(s => s.categoryId === catId);
        }
        return list;
    },

    get filteredCategories() {
        if (!this.categorySearch?.trim()) return this.categories;
        const q = this.categorySearch.trim().toLowerCase();
        return this.categories.filter(c =>
            c.name?.toLowerCase().includes(q) ||
            c.description?.toLowerCase().includes(q)
        );
    },

    get filteredPreOrders() {
        if (this.preOrderFilter === null) return this.preOrders;
        return this.preOrders.filter(p => p.status === this.preOrderFilter);
    },

    get pendingPreOrderCount() {
        return this.preOrders.filter(p => p.status === 0).length;
    },

    get selectedCategorySeries() {
        if (!this.selectedCategory) return [];
        return this.seriesList.filter(s => s.categoryId === this.selectedCategory.id);
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
            if (this.selectedCategory?.id === id) this.selectedCategory = null;
            await this.loadCategories();
            this.loadDashboard();
            this.showToast('Đã xoá thể loại.');
        } catch (e) {
            this.showToast('Không thể xóa: ' + e.message, 'error');
        }
    },

    selectCategory(cat) {
        this.selectedCategory = this.selectedCategory?.id === cat.id ? null : cat;
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
                categoryId: '',
                readingStatus: 0
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
            await Promise.all([this.loadSeries(), this.loadCategories()]);
            this.loadDashboard();
            this.showToast('Đã xoá bộ sách.');
        } catch (e) {
            this.showToast('Lỗi xóa bộ sách: ' + e.message, 'error');
        }
    },

    readingStatusLabel(status) {
        return ['Chưa đọc', 'Đang đọc', 'Đã đọc'][status] || 'Chưa đọc';
    },

    readingStatusClass(status) {
        return [
            'bg-gray-100 text-gray-500',
            'bg-blue-100 text-blue-600',
            'bg-green-100 text-green-600'
        ][status] || 'bg-gray-100 text-gray-500';
    },

    readingStatusEmoji(status) {
        return ['📖', '📗', '✅'][status] || '📖';
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

    // ── Pre-order ──────────────────────────────
    async loadPreOrders() {
        try {
            this.preOrders = await apiFetch(`${API}/preorder`) || [];
        } catch (e) {
            this.showToast('Lỗi tải pre-order: ' + e.message, 'error');
            this.preOrders = [];
        }
    },

    openPreOrderModal(po = null) {
        this.editingPreOrder = po
            ? { ...po, expectedDate: po.expectedDate ? po.expectedDate.substring(0, 10) : '' }
            : { title: '', author: '', publisher: '', seriesId: '', volumeNumber: '', notes: '', expectedDate: '', status: 0 };
        this.showPreOrderModal = true;
    },

    async savePreOrder() {
        if (!this.editingPreOrder.title?.trim()) {
            this.showToast('Vui lòng nhập tên sách', 'warning');
            return;
        }
        try {
            const isEdit = !!this.editingPreOrder.id;
            const body = {
                ...this.editingPreOrder,
                seriesId:     this.editingPreOrder.seriesId     || null,
                volumeNumber: this.editingPreOrder.volumeNumber || null,
                expectedDate: this.editingPreOrder.expectedDate || null,
            };
            await apiFetch(
                isEdit ? `${API}/preorder/${body.id}` : `${API}/preorder`,
                { method: isEdit ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }
            );
            this.showPreOrderModal = false;
            await this.loadPreOrders();
            this.showToast(isEdit ? 'Đã cập nhật pre-order!' : 'Đã thêm pre-order mới! 🛒');
        } catch (e) {
            this.showToast('Lỗi lưu: ' + e.message, 'error');
        }
    },

    async deletePreOrder(id) {
        if (!confirm('Xóa pre-order này?')) return;
        try {
            await apiFetch(`${API}/preorder/${id}`, { method: 'DELETE' });
            await this.loadPreOrders();
            this.showToast('Đã xóa pre-order.');
        } catch (e) {
            this.showToast('Lỗi xóa: ' + e.message, 'error');
        }
    },

    async cancelPreOrder(id) {
        if (!confirm('Hủy pre-order này?')) return;
        try {
            const po = this.preOrders.find(p => p.id === id);
            if (!po) return;
            await apiFetch(`${API}/preorder/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ ...po, status: 2, expectedDate: po.expectedDate || null })
            });
            await this.loadPreOrders();
            this.showToast('Đã hủy pre-order.');
        } catch (e) {
            this.showToast('Lỗi hủy: ' + e.message, 'error');
        }
    },

    // ── WishList ───────────────────────────────
    async loadWishList() {
        try {
            this.wishList = await apiFetch(`${API}/wishlist`) || [];
        } catch (e) {
            this.showToast('Lỗi tải wish list: ' + e.message, 'error');
            this.wishList = [];
        }
    },

    openWishListModal(item = null) {
        this.editingWishItem = item
            ? { ...item }
            : { title: '', author: '', publisher: '', notes: '', categoryId: '' };
        this.showWishListModal = true;
    },

    async saveWishItem() {
        if (!this.editingWishItem.title?.trim()) {
            this.showToast('Vui lòng nhập tên sách', 'warning');
            return;
        }
        try {
            const isEdit = !!this.editingWishItem.id;
            const body = {
                ...this.editingWishItem,
                categoryId: this.editingWishItem.categoryId || null,
            };
            await apiFetch(
                isEdit ? `${API}/wishlist/${body.id}` : `${API}/wishlist`,
                { method: isEdit ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) }
            );
            this.showWishListModal = false;
            await this.loadWishList();
            this.showToast(isEdit ? 'Đã cập nhật!' : 'Đã thêm vào wish list! 🌟');
        } catch (e) {
            this.showToast('Lỗi lưu: ' + e.message, 'error');
        }
    },

    async deleteWishItem(id) {
        if (!confirm('Xóa khỏi wish list?')) return;
        try {
            await apiFetch(`${API}/wishlist/${id}`, { method: 'DELETE' });
            await this.loadWishList();
            this.showToast('Đã xóa khỏi wish list.');
        } catch (e) {
            this.showToast('Lỗi xóa: ' + e.message, 'error');
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

    // ── Category Chinese-art background ────────
    normalizeVi(s) {
        return (s || '').toLowerCase()
            .replace(/đ/g,'d')
            .replace(/[áàảãạăắặằẳẵâấầẩẫậ]/g,'a')
            .replace(/[éèẻẽẹêếềểễệ]/g,'e')
            .replace(/[íìỉĩị]/g,'i')
            .replace(/[óòỏõọôốồổỗộơớờởỡợ]/g,'o')
            .replace(/[úùủũụưứừửữự]/g,'u')
            .replace(/[ýỳỷỹỵ]/g,'y')
            .replace(/\s+/g,'');
    },

    getCategoryStyle(name) {
        const k   = this.normalizeVi(name);
        const has = (...ws) => ws.every(w => k.includes(w));

        const SVG = {
            plum: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><circle cx='155' cy='45' r='32' fill='%23F3E8FF' opacity='0.7'/><circle cx='155' cy='45' r='24' fill='%23EDE9FE' opacity='0.5'/><path d='M20 190 Q70 140 95 100 Q115 72 152 48' stroke='%237C3AED' stroke-width='2.5' fill='none' opacity='0.28' stroke-linecap='round'/><path d='M20 190 Q55 158 72 132' stroke='%237C3AED' stroke-width='2' fill='none' opacity='0.25' stroke-linecap='round'/><circle cx='90' cy='105' r='7' fill='%23F9A8D4' opacity='0.7'/><circle cx='80' cy='120' r='5' fill='%23FBCFE8' opacity='0.65'/><circle cx='105' cy='90' r='6' fill='%23F0ABFC' opacity='0.65'/><circle cx='118' cy='78' r='5' fill='%23F9A8D4' opacity='0.6'/><circle cx='132' cy='65' r='4' fill='%23FBCFE8' opacity='0.55'/><circle cx='60' cy='140' r='4' fill='%23E879F9' opacity='0.5'/></svg>`,
            lotus: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><ellipse cx='100' cy='178' rx='78' ry='12' fill='%2386EFAC' opacity='0.35'/><ellipse cx='65' cy='173' rx='45' ry='9' fill='%234ADE80' opacity='0.28'/><ellipse cx='140' cy='175' rx='38' ry='8' fill='%234ADE80' opacity='0.25'/><ellipse cx='100' cy='142' rx='11' ry='36' fill='%23FBCFE8' opacity='0.65'/><ellipse cx='78' cy='150' rx='10' ry='33' fill='%23F9A8D4' opacity='0.55' transform='rotate(-22 78 150)'/><ellipse cx='122' cy='150' rx='10' ry='33' fill='%23F9A8D4' opacity='0.55' transform='rotate(22 122 150)'/><ellipse cx='60' cy='162' rx='9' ry='28' fill='%23FCE7F3' opacity='0.5' transform='rotate(-42 60 162)'/><ellipse cx='140' cy='162' rx='9' ry='28' fill='%23FCE7F3' opacity='0.5' transform='rotate(42 140 162)'/><circle cx='100' cy='120' r='14' fill='%23FDE68A' opacity='0.75'/></svg>`,
            bamboo: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><rect x='50' y='0' width='9' height='200' rx='4' fill='%2386EFAC' opacity='0.5'/><rect x='72' y='15' width='9' height='185' rx='4' fill='%234ADE80' opacity='0.45'/><rect x='94' y='0' width='9' height='200' rx='4' fill='%2386EFAC' opacity='0.5'/><rect x='116' y='8' width='9' height='192' rx='4' fill='%234ADE80' opacity='0.45'/><rect x='138' y='0' width='9' height='200' rx='4' fill='%2386EFAC' opacity='0.45'/><rect x='50' y='55' width='9' height='4' fill='%2322C55E' opacity='0.55'/><rect x='50' y='120' width='9' height='4' fill='%2322C55E' opacity='0.55'/><rect x='72' y='75' width='9' height='4' fill='%2322C55E' opacity='0.55'/><rect x='72' y='148' width='9' height='4' fill='%2322C55E' opacity='0.55'/><rect x='94' y='50' width='9' height='4' fill='%2322C55E' opacity='0.55'/><rect x='116' y='90' width='9' height='4' fill='%2322C55E' opacity='0.55'/><ellipse cx='35' cy='80' rx='24' ry='6' fill='%2316A34A' opacity='0.38' transform='rotate(-38 35 80)'/><ellipse cx='84' cy='35' rx='22' ry='6' fill='%2316A34A' opacity='0.35' transform='rotate(28 84 35)'/><ellipse cx='155' cy='90' rx='22' ry='5' fill='%2316A34A' opacity='0.35' transform='rotate(-32 155 90)'/></svg>`,
            mountain: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><circle cx='158' cy='38' r='28' fill='%23FEF9C3' opacity='0.55'/><circle cx='166' cy='32' r='20' fill='%23FFFBEB' opacity='0.4'/><polygon points='0,200 65,55 130,200' fill='%23BAE6FD' opacity='0.45'/><polygon points='60,200 135,72 200,200' fill='%2393C5FD' opacity='0.42'/><polygon points='0,200 42,105 88,200' fill='%23DBEAFE' opacity='0.35'/><polygon points='118,200 168,88 200,200' fill='%23BFDBFE' opacity='0.35'/><circle cx='48' cy='95' r='4' fill='%23FBCFE8' opacity='0.55'/><circle cx='68' cy='72' r='3' fill='%23F9A8D4' opacity='0.5'/><circle cx='35' cy='112' r='3' fill='%23FBCFE8' opacity='0.5'/></svg>`,
            scroll: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><rect x='35' y='55' width='130' height='90' rx='6' fill='%23FEF3C7' opacity='0.55'/><ellipse cx='35' cy='100' rx='14' ry='45' fill='%23FDE68A' opacity='0.55'/><ellipse cx='165' cy='100' rx='14' ry='45' fill='%23FDE68A' opacity='0.55'/><line x1='58' y1='78' x2='142' y2='78' stroke='%23D97706' stroke-width='1.5' opacity='0.4'/><line x1='58' y1='92' x2='142' y2='92' stroke='%23D97706' stroke-width='1.5' opacity='0.4'/><line x1='58' y1='106' x2='135' y2='106' stroke='%23D97706' stroke-width='1.5' opacity='0.35'/><line x1='58' y1='120' x2='142' y2='120' stroke='%23D97706' stroke-width='1.5' opacity='0.4'/><line x1='58' y1='134' x2='118' y2='134' stroke='%23D97706' stroke-width='1.5' opacity='0.35'/><rect x='124' y='118' width='18' height='18' rx='3' fill='%23EF4444' opacity='0.45'/></svg>`,
            brush: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><rect x='93' y='5' width='14' height='108' rx='7' fill='%234B5563' opacity='0.32'/><rect x='93' y='5' width='14' height='22' rx='7' fill='%23D97706' opacity='0.42'/><polygon points='93,113 107,113 100,158' fill='%231F2937' opacity='0.42'/><ellipse cx='100' cy='168' rx='20' ry='13' fill='%231F2937' opacity='0.14'/><path d='M28 118 C60 90 140 90 172 118' stroke='%231F2937' stroke-width='3' fill='none' opacity='0.16' stroke-linecap='round'/><path d='M38 138 C68 112 132 112 162 138' stroke='%231F2937' stroke-width='2.5' fill='none' opacity='0.13' stroke-linecap='round'/><line x1='48' y1='158' x2='152' y2='158' stroke='%231F2937' stroke-width='2' opacity='0.1'/></svg>`,
            dragon: `<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'><circle cx='45' cy='45' r='22' fill='%23FEF9C3' opacity='0.48'/><circle cx='68' cy='38' r='20' fill='%23FFFBEB' opacity='0.42'/><circle cx='28' cy='55' r='16' fill='%23FEF9C3' opacity='0.4'/><circle cx='155' cy='78' r='18' fill='%23FEF9C3' opacity='0.42'/><circle cx='175' cy='68' r='15' fill='%23FFFBEB' opacity='0.38'/><path d='M18 148 C48 122 78 138 108 118 C138 98 163 112 185 92' stroke='%23EF4444' stroke-width='5' fill='none' opacity='0.3' stroke-linecap='round'/><path d='M18 160 C48 134 78 150 108 130 C138 110 163 124 185 104' stroke='%23DC2626' stroke-width='3' fill='none' opacity='0.2' stroke-linecap='round'/><circle cx='20' cy='146' r='7' fill='%23EF4444' opacity='0.38'/><path d='M105 115 L118 103 L112 110 L125 98' stroke='%23EF4444' stroke-width='2.5' fill='none' opacity='0.32' stroke-linecap='round'/></svg>`,
        };

        let svgKey, grad;
        if (has('dammy') || (has('dam') && has('my')))           { svgKey='plum';     grad='linear-gradient(135deg,#fce7f3,#ede9fe)'; }
        else if (has('ngontinh') || (has('ngon') && has('tinh'))){ svgKey='lotus';    grad='linear-gradient(135deg,#fdf4ff,#fce7f3)'; }
        else if (has('vietnam') || (has('viet') && has('nam')))  { svgKey='bamboo';   grad='linear-gradient(135deg,#dcfce7,#ecfdf5)'; }
        else if (has('chau'))                                     { svgKey='mountain'; grad='linear-gradient(135deg,#dbeafe,#e0f2fe)'; }
        else if (has('sach') && has('ngoai'))                     { svgKey='brush';    grad='linear-gradient(135deg,#f0fdfa,#ccfbf1)'; }
        else if (has('nuoc') || has('ngoai'))                     { svgKey='scroll';   grad='linear-gradient(135deg,#fffbeb,#fef3c7)'; }
        else if (has('truyen') || has('tranh'))                   { svgKey='dragon';   grad='linear-gradient(135deg,#fff7ed,#ffedd5)'; }

        if (!svgKey) return 'background: linear-gradient(135deg,#f9fafb,#f3f4f6);';
        return `background: url("data:image/svg+xml,${SVG[svgKey]}") no-repeat right -10px bottom -10px / 160px, ${grad};`;
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
