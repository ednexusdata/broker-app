// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('alpine:init', () => {
    // Live countdown shown on request pages via the _RetentionCountdown partial,
    // ticking down to when an inactive request is auto-deleted.
    Alpine.data('retentionCountdown', (destructionAtIso) => ({
        destructionAt: new Date(destructionAtIso),
        expired: false,
        display: '',
        interval: null,
        tick() {
            const msRemaining = this.destructionAt.getTime() - Date.now();
            if (msRemaining <= 0) {
                this.expired = true;
                this.display = '';
                clearInterval(this.interval);
                return;
            }
            const totalSeconds = Math.floor(msRemaining / 1000);
            const days = Math.floor(totalSeconds / 86400);
            const hours = Math.floor((totalSeconds % 86400) / 3600);
            const minutes = Math.floor((totalSeconds % 3600) / 60);
            const seconds = totalSeconds % 60;
            const parts = [];
            if (days > 0) parts.push(days + 'd');
            if (days > 0 || hours > 0) parts.push(hours + 'h');
            if (days > 0 || hours > 0 || minutes > 0) parts.push(minutes + 'm');
            parts.push(seconds + 's');
            this.display = parts.join(' ');
        },
        init() {
            this.tick();
            this.interval = setInterval(() => this.tick(), 1000);
        },
        destroy() {
            clearInterval(this.interval);
        },
    }))
});
