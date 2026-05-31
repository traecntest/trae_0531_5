class WallpaperApp {
    constructor() {
        this.currentPage = 1;
        this.currentQuery = '';
        this.isLoading = false;
        this.favorites = this.loadFavorites();
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadWallpapers();
        this.updateFavCount();
    }

    bindEvents() {
        document.getElementById('searchBtn').addEventListener('click', () => this.search());
        document.getElementById('searchInput').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.search();
        });

        document.querySelectorAll('.tag').forEach(tag => {
            tag.addEventListener('click', (e) => {
                document.querySelectorAll('.tag').forEach(t => t.classList.remove('active'));
                e.target.classList.add('active');
                this.currentQuery = e.target.dataset.tag;
                this.currentPage = 1;
                document.getElementById('wallpaperGrid').innerHTML = '';
                this.loadWallpapers();
            });
        });

        document.getElementById('loadMoreBtn').addEventListener('click', () => {
            this.currentPage++;
            this.loadWallpapers();
        });

        document.querySelectorAll('.nav-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.querySelectorAll('.nav-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');
                const view = e.target.dataset.view;
                if (view === 'favorites') {
                    document.getElementById('browseView').style.display = 'none';
                    document.getElementById('favoritesView').style.display = 'block';
                    this.renderFavorites();
                } else {
                    document.getElementById('browseView').style.display = 'block';
                    document.getElementById('favoritesView').style.display = 'none';
                }
            });
        });

        document.getElementById('modalClose').addEventListener('click', () => {
            document.getElementById('wallpaperModal').classList.remove('show');
        });

        document.getElementById('wallpaperModal').addEventListener('click', (e) => {
            if (e.target.id === 'wallpaperModal') {
                document.getElementById('wallpaperModal').classList.remove('show');
            }
        });

        window.addEventListener('scroll', () => {
            if (this.isLoading) return;
            const scrollTop = window.scrollY;
            const windowHeight = window.innerHeight;
            const documentHeight = document.documentElement.scrollHeight;

            if (scrollTop + windowHeight >= documentHeight - 500) {
                const browseView = document.getElementById('browseView');
                if (browseView.style.display !== 'none') {
                    this.currentPage++;
                    this.loadWallpapers();
                }
            }
        });
    }

    search() {
        const query = document.getElementById('searchInput').value.trim();
        this.currentQuery = query;
        this.currentPage = 1;
        document.getElementById('wallpaperGrid').innerHTML = '';
        document.querySelectorAll('.tag').forEach(t => t.classList.remove('active'));
        this.loadWallpapers();
    }

    async loadWallpapers() {
        if (this.isLoading) return;
        this.isLoading = true;
        this.showLoading(true);

        try {
            let url;
            if (this.currentQuery) {
                url = `/api/wallpapers/search?query=${encodeURIComponent(this.currentQuery)}&page=${this.currentPage}&perPage=20`;
            } else {
                url = `/api/wallpapers/popular?page=${this.currentPage}&perPage=20`;
            }

            const response = await fetch(url);
            const data = await response.json();

            if (data.results && data.results.length > 0) {
                this.renderWallpapers(data.results);
                document.getElementById('noResults').style.display = 'none';
            } else if (this.currentPage === 1) {
                document.getElementById('noResults').style.display = 'block';
            }

            if (this.currentPage >= data.totalPages) {
                document.getElementById('loadMore').style.display = 'none';
            }
        } catch (error) {
            console.error('Failed to load wallpapers:', error);
        } finally {
            this.isLoading = false;
            this.showLoading(false);
        }
    }

    renderWallpapers(wallpapers) {
        const grid = document.getElementById('wallpaperGrid');
        wallpapers.forEach(wallpaper => {
            grid.appendChild(this.createWallpaperCard(wallpaper));
        });
    }

    createWallpaperCard(wallpaper) {
        const card = document.createElement('div');
        card.className = 'wallpaper-card';
        const isFavorited = this.favorites.some(f => f.id === wallpaper.id);

        card.innerHTML = `
            <img src="${wallpaper.thumbUrl}" alt="${wallpaper.description || 'Wallpaper'}" loading="lazy">
            <div class="wallpaper-overlay">
                <div class="wallpaper-actions">
                    <button class="action-btn favorite-btn ${isFavorited ? 'favorited' : ''}" data-id="${wallpaper.id}">
                        ${isFavorited ? '❤️' : '🤍'}
                    </button>
                    <button class="action-btn download-btn-card" data-id="${wallpaper.id}">⬇️</button>
                </div>
                <div class="wallpaper-info">
                    <p>${wallpaper.description || 'Beautiful wallpaper'}</p>
                    <span class="photographer">📷 ${wallpaper.photographer}</span>
                </div>
            </div>
        `;

        card.querySelector('img').addEventListener('click', () => this.openModal(wallpaper));
        card.querySelector('.favorite-btn').addEventListener('click', (e) => {
            e.stopPropagation();
            this.toggleFavorite(wallpaper, e.target);
        });
        card.querySelector('.download-btn-card').addEventListener('click', (e) => {
            e.stopPropagation();
            this.downloadWallpaper(wallpaper.id, 1920, 1080);
        });

        return card;
    }

    openModal(wallpaper) {
        const modal = document.getElementById('wallpaperModal');
        const body = document.getElementById('modalBody');
        const isFavorited = this.favorites.some(f => f.id === wallpaper.id);

        body.innerHTML = `
            <img src="${wallpaper.url}" alt="${wallpaper.description || 'Wallpaper'}">
            <div class="modal-details">
                <h3>${wallpaper.description || 'Beautiful wallpaper'}</h3>
                <p>📷 <a href="${wallpaper.photographerUrl}" target="_blank" style="color: #667eea;">${wallpaper.photographer}</a></p>
                <p>📐 ${wallpaper.width} x ${wallpaper.height}</p>
                <div class="download-options">
                    <button class="download-btn" data-resolution="1920x1080">1920 x 1080 (Full HD)</button>
                    <button class="download-btn" data-resolution="2560x1440">2560 x 1440 (2K)</button>
                    <button class="download-btn" data-resolution="3840x2160">3840 x 2160 (4K)</button>
                    <button class="download-btn" data-resolution="original">原始尺寸</button>
                </div>
            </div>
        `;

        body.querySelectorAll('.download-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const resolution = btn.dataset.resolution;
                if (resolution === 'original') {
                    this.downloadWallpaper(wallpaper.id, wallpaper.width, wallpaper.height);
                } else {
                    const [width, height] = resolution.split('x').map(Number);
                    this.downloadWallpaper(wallpaper.id, width, height);
                }
            });
        });

        modal.classList.add('show');
    }

    async downloadWallpaper(id, width, height) {
        try {
            window.location.href = `/api/wallpapers/${id}/download?width=${width}&height=${height}`;
        } catch (error) {
            console.error('Failed to download wallpaper:', error);
        }
    }

    toggleFavorite(wallpaper, btn) {
        const index = this.favorites.findIndex(f => f.id === wallpaper.id);
        if (index > -1) {
            this.favorites.splice(index, 1);
            btn.textContent = '🤍';
            btn.classList.remove('favorited');
        } else {
            this.favorites.push(wallpaper);
            btn.textContent = '❤️';
            btn.classList.add('favorited');
        }
        this.saveFavorites();
        this.updateFavCount();
    }

    loadFavorites() {
        try {
            return JSON.parse(localStorage.getItem('wallpaper_favorites') || '[]');
        } catch {
            return [];
        }
    }

    saveFavorites() {
        localStorage.setItem('wallpaper_favorites', JSON.stringify(this.favorites));
    }

    updateFavCount() {
        document.getElementById('favCount').textContent = this.favorites.length;
    }

    renderFavorites() {
        const grid = document.getElementById('favoritesGrid');
        grid.innerHTML = '';

        if (this.favorites.length === 0) {
            document.getElementById('noFavorites').style.display = 'block';
            return;
        }

        document.getElementById('noFavorites').style.display = 'none';
        this.favorites.forEach(wallpaper => {
            const card = this.createWallpaperCard(wallpaper);
            grid.appendChild(card);
        });
    }

    showLoading(show) {
        document.getElementById('loading').classList.toggle('show', show);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new WallpaperApp();
});
