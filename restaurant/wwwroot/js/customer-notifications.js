// Live notification bell for signed-in customers. Requires the SignalR
// client script to already be loaded on the page (see README for the CDN
// tag) and the user to be authenticated — the hub is [Authorize]-protected,
// so this simply won't connect for anonymous visitors, which is fine.
(function () {
    const bellCountEl = document.getElementById("notif-bell-count");
    if (!bellCountEl || typeof signalR === "undefined") return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", function (notification) {
        const current = parseInt(bellCountEl.textContent || "0", 10);
        bellCountEl.textContent = current + 1;
        bellCountEl.classList.remove("d-none");

        // Small toast so the update is noticeable even off the notifications page.
        const toastContainer = document.getElementById("toast-container");
        if (toastContainer) {
            const toast = document.createElement("div");
            toast.className = "toast align-items-center text-bg-primary border-0 show mb-2";
            toast.innerHTML = `
                <div class="d-flex">
                    <div class="toast-body">
                        <strong>${notification.title}</strong><br>${notification.message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" onclick="this.closest('.toast').remove()"></button>
                </div>`;
            toastContainer.appendChild(toast);
            setTimeout(() => toast.remove(), 8000);
        }
    });

    // Starting the connection can legitimately fail (user offline, server
    // restarting) — never let that bubble up as an unhandled page error.
    connection.start().catch(err => console.warn("Notification hub connection failed:", err));
})();
