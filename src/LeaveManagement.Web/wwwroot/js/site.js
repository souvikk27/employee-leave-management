// Initialize Bootstrap Tooltips & UI interactions
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[data-bs-toggle="tooltip"]')
    );
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Sidebar Toggle Logic
    var sidebarToggle = document.getElementById('sidebarToggle');
    var appShell = document.getElementById('appShell');

    if (sidebarToggle && appShell) {
        sidebarToggle.addEventListener('click', function () {
            appShell.classList.toggle('collapsed');

            // Slightly delay tooltip hide/show to ensure layout transitions cleanly
            setTimeout(function () {
                if (appShell.classList.contains('collapsed')) {
                    tooltipList.forEach(function (t) { t.enable(); });
                } else {
                    tooltipList.forEach(function (t) { t.disable(); });
                }
            }, 100);
        });

        // Initially disable tooltips if not collapsed (they are only needed for collapsed sidebar)
        if (!appShell.classList.contains('collapsed')) {
            tooltipList.forEach(function (t) { t.disable(); });
        }
    }
});
