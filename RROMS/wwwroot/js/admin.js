(function () {
    function initializeAdminLayout() {
        // Rebind the sidebar toggle after each Turbo navigation.
        const toggleButton = document.getElementById("sidebarToggle");

        if (toggleButton) {
            toggleButton.onclick = function () {
                if (window.innerWidth <= 900) {
                    document.body.classList.toggle("sidebar-open");
                } else {
                    document.body.classList.toggle("sidebar-collapsed");
                }
            };
        }

        // Update the active sidebar link because the sidebar is preserved.
        const currentPath = normalizePath(window.location.pathname);

        document.querySelectorAll("#adminSidebar .sidebar-link").forEach(link => {
            if (!link.href || link.getAttribute("href") === "#") {
                return;
            }

            const linkPath = normalizePath(new URL(link.href).pathname);
            const isActive = linkPath === currentPath;

            link.classList.toggle("active", isActive);

            if (isActive) {
                link.setAttribute("aria-current", "page");
            } else {
                link.removeAttribute("aria-current");
            }
        });
    }

    function normalizePath(path) {
        const decoded = decodeURIComponent(path);

        if (decoded.length > 1) {
            return decoded.replace(/\/+$/, "").toLowerCase();
        }

        return decoded.toLowerCase();
    }

    // Initial page load.
    document.addEventListener("DOMContentLoaded", initializeAdminLayout);

    // Subsequent Turbo navigations.
    document.addEventListener("turbo:load", initializeAdminLayout);
})();
