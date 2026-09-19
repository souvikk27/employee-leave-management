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

    // Segmented control behavior
    document.querySelectorAll('.nav-pills').forEach(function (seg) {
        seg.addEventListener('click', function (e) {
            if (e.target.classList.contains('nav-link')) {
                seg.querySelectorAll('.nav-link').forEach(function (b) {
                    b.classList.remove('active');
                });
                e.target.classList.add('active');
            }
        });
    });

    // Approve / reject quick interaction
    document.querySelectorAll('.btn-action.approve').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var row = btn.closest('tr');
            if (row) {
                var badge = row.querySelector('.badge-pill');
                if (badge) {
                    badge.className = 'badge-pill badge-approved';
                    badge.innerHTML = '<span class="badge-dot"></span>Approved';
                }
                var actions = row.querySelector('.row-actions');
                if (actions) {
                    actions.innerHTML = '<button class="btn-action" title="View"><i class="bi bi-eye"></i></button>';
                }
            }
        });
    });

    document.querySelectorAll('.btn-action.reject').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var row = btn.closest('tr');
            if (row) {
                var badge = row.querySelector('.badge-pill');
                if (badge) {
                    badge.className = 'badge-pill badge-rejected';
                    badge.innerHTML = '<span class="badge-dot"></span>Rejected';
                }
                var actions = row.querySelector('.row-actions');
                if (actions) {
                    actions.innerHTML = '<button class="btn-action" title="View"><i class="bi bi-eye"></i></button>';
                }
            }
        });
    });
});
