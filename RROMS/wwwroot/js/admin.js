function initializeAdminLayout() {
    const toggleButton = document.getElementById("sidebarToggle");
    const sidebar = document.getElementById("adminSidebar");

    // Rebind sidebar toggle
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

    // Update active navigation without replacing the sidebar
    const currentPath = decodeURIComponent(window.location.pathname)
        .replace(/\/+$/, "")
        .toLowerCase();

    if (sidebar) {
        const links = sidebar.querySelectorAll(".sidebar-link");

        links.forEach(link => {
            if (!link.href) return;

            const linkPath = decodeURIComponent(
                new URL(link.href).pathname
            ).replace(/\/+$/, "").toLowerCase();

            const isActive = linkPath === currentPath;

            link.classList.toggle("active", isActive);

            if (isActive) {
                link.setAttribute("aria-current", "page");
            } else {
                link.removeAttribute("aria-current");
            }
        });
    }
}

// Initial page load
document.addEventListener("DOMContentLoaded", initializeAdminLayout);

// Update active navigation after Turbo navigation
document.addEventListener("turbo:load", initializeAdminLayout);
