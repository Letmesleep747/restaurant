document.addEventListener("DOMContentLoaded", function () {

    const toggleButton = document.getElementById("sidebarToggle");

    if (!toggleButton) {
        return;
    }

    toggleButton.addEventListener("click", function () {

        if (window.innerWidth <= 900) {

            document.body.classList.toggle("sidebar-open");

        } else {

            document.body.classList.toggle("sidebar-collapsed");

        }

    });

});