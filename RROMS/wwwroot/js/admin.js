(function () {
    function normalizePath(path) {
        const decoded = decodeURIComponent(path);

        if (decoded.length > 1) {
            return decoded.replace(/\\/+$/, "").toLowerCase();
        }

        return decoded.toLowerCase();
    }

    function initializeAdminLayout() {
        const toggleButton = document.getElementById("sidebarToggle");

        // Rebind the toggle after both normal loads and Turbo visits.
        if (toggleButton) {
            toggleButton.onclick = function () {
                if (window.innerWidth <= 900) {
                    document.body.classList.toggle("sidebar-open");
                    toggleButton.setAttribute(
                        "aria-expanded",
                        document.body.classList.contains("sidebar-open") ? "true" : "false"
                    );
                } else {
                    document.body.classList.toggle("sidebar-collapsed");
                    toggleButton.setAttribute(
                        "aria-expanded",
                        document.body.classList.contains("sidebar-collapsed") ? "false" : "true"
                    );
                }
            };
        }

        // The sidebar is permanent across Turbo visits, so update its active item.
        const currentPath = normalizePath(window.location.pathname);

        document.querySelectorAll("#adminSidebar .sidebar-link").forEach(link => {
            if (!link.href || link.getAttribute("href") === "#") {
                return;
            }

            let linkPath;

            try {
                linkPath = normalizePath(new URL(link.href, window.location.origin).pathname);
            } catch {
                return;
            }

            const isActive = linkPath === currentPath;
            link.classList.toggle("active", isActive);

            if (isActive) {
                link.setAttribute("aria-current", "page");
            } else {
                link.removeAttribute("aria-current");
            }
        });

        // Start every newly rendered admin page with the full desktop layout.
        // This also clears stale classes restored by browser/Turbo page caching.
        document.body.classList.remove("sidebar-collapsed", "sidebar-open");

        if (toggleButton) {
            toggleButton.setAttribute("aria-expanded", "true");
        }
    }

    function resetLayoutBeforeCache() {
        // Turbo snapshots the body. Do not cache a temporarily collapsed sidebar.
        document.body.classList.remove("sidebar-collapsed", "sidebar-open");

        const toggleButton = document.getElementById("sidebarToggle");
        if (toggleButton) {
            toggleButton.setAttribute("aria-expanded", "true");
        }
    }

    document.addEventListener("DOMContentLoaded", initializeAdminLayout);
    document.addEventListener("turbo:load", initializeAdminLayout);
    document.addEventListener("turbo:before-cache", resetLayoutBeforeCache);
})();
