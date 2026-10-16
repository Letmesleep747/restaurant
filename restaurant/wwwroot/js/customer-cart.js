// Simple client-side cart backed by localStorage. Kept deliberately simple
// (no server session state) so it can't throw mid-order if the session
// expires or cookies are blocked; the cart only needs to survive until the
// user clicks "Place Order" on the Checkout page, where it's sent to the
// server and turned into a real Order.
(function () {
    const CART_KEY = "rroms_cart";

    function getCart() {
        try {
            const raw = localStorage.getItem(CART_KEY);
            return raw ? JSON.parse(raw) : [];
        } catch {
            return [];
        }
    }

    function saveCart(cart) {
        localStorage.setItem(CART_KEY, JSON.stringify(cart));
        updateCartBadge();
    }

    function addToCart(id, name, price, qty) {
        const cart = getCart();
        const existing = cart.find(i => i.id === id);
        if (existing) {
            existing.qty += qty;
        } else {
            cart.push({ id, name, price, qty });
        }
        saveCart(cart);
    }

    function removeFromCart(id) {
        saveCart(getCart().filter(i => i.id !== id));
    }

    function clearCart() {
        localStorage.removeItem(CART_KEY);
        updateCartBadge();
    }

    function updateCartBadge() {
        const badge = document.getElementById("cart-count");
        if (badge) {
            const count = getCart().reduce((sum, i) => sum + i.qty, 0);
            badge.textContent = count;
        }
    }

    // --- Menu page: add-to-cart buttons ---
    document.querySelectorAll(".add-to-cart-btn").forEach(btn => {
        btn.addEventListener("click", function () {
            const id = parseInt(this.dataset.id, 10);
            const name = this.dataset.name;
            const price = parseFloat(this.dataset.price);
            const qtyInput = document.querySelector(`.qty-input[data-id="${id}"]`);
            const qty = qtyInput ? Math.max(1, parseInt(qtyInput.value, 10) || 1) : 1;
            addToCart(id, name, price, qty);

            const original = this.textContent;
            this.textContent = "Added ✓";
            this.disabled = true;
            setTimeout(() => { this.textContent = original; this.disabled = false; }, 900);
        });
    });

    // --- Checkout page: render cart table + build hidden inputs on submit ---
    const cartTableBody = document.getElementById("cart-table-body");
    if (cartTableBody) {
        renderCheckoutCart();
    }

    function renderCheckoutCart() {
        const cart = getCart();
        cartTableBody.innerHTML = "";
        let total = 0;

        if (cart.length === 0) {
            cartTableBody.innerHTML = '<tr><td colspan="5" class="text-muted">Your cart is empty. Add items from the Menu page.</td></tr>';
        }

        cart.forEach(item => {
            const subtotal = item.price * item.qty;
            total += subtotal;
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${item.name}</td>
                <td>${item.price.toFixed(2)}</td>
                <td style="width:90px">
                    <input type="number" min="1" value="${item.qty}" class="form-control form-control-sm cart-qty" data-id="${item.id}">
                </td>
                <td>${subtotal.toFixed(2)}</td>
                <td><button type="button" class="btn btn-sm btn-outline-danger remove-cart-item" data-id="${item.id}">Remove</button></td>
            `;
            cartTableBody.appendChild(row);
        });

        const totalEl = document.getElementById("cart-total");
        if (totalEl) totalEl.textContent = total.toFixed(2);

        cartTableBody.querySelectorAll(".remove-cart-item").forEach(btn => {
            btn.addEventListener("click", function () {
                removeFromCart(parseInt(this.dataset.id, 10));
                renderCheckoutCart();
            });
        });

        cartTableBody.querySelectorAll(".cart-qty").forEach(input => {
            input.addEventListener("change", function () {
                const cart = getCart();
                const item = cart.find(i => i.id === parseInt(this.dataset.id, 10));
                if (item) {
                    item.qty = Math.max(1, parseInt(this.value, 10) || 1);
                    saveCart(cart);
                    renderCheckoutCart();
                }
            });
        });
    }

    const placeOrderForm = document.getElementById("place-order-form");
    if (placeOrderForm) {
        placeOrderForm.addEventListener("submit", function (e) {
            const cart = getCart();
            if (cart.length === 0) {
                e.preventDefault();
                alert("Your cart is empty.");
                return;
            }
            const container = document.getElementById("cart-hidden-inputs");
            container.innerHTML = "";
            cart.forEach((item, index) => {
                container.innerHTML += `
                    <input type="hidden" name="CartItems[${index}].MenuItemId" value="${item.id}" />
                    <input type="hidden" name="CartItems[${index}].Quantity" value="${item.qty}" />
                `;
            });
            // Cart data is already copied into the form — clear local cart now
            // so returning to Menu/Checkout after a successful order is empty.
            // Mark pending so if the server rejects the order we can restore.
            sessionStorage.setItem("rroms_cart_backup", JSON.stringify(cart));
            clearCart();
        });
    }

    // If checkout page reloads with a validation error, restore the cart.
    const validationSummary = document.querySelector("[asp-validation-summary], .validation-summary-errors, .text-danger");
    if (document.getElementById("place-order-form") && sessionStorage.getItem("rroms_cart_backup")) {
        const hasServerError = document.querySelector(".validation-summary-errors li, div.text-danger ul li");
        if (hasServerError) {
            try {
                const backup = JSON.parse(sessionStorage.getItem("rroms_cart_backup"));
                if (Array.isArray(backup) && backup.length) {
                    saveCart(backup);
                    if (cartTableBody) renderCheckoutCart();
                }
            } catch { /* ignore */ }
        } else if (getCart().length === 0) {
            // Successful redirect path — drop the backup
            sessionStorage.removeItem("rroms_cart_backup");
        }
    }

    // Expose clearCart globally so the Checkout success handler can wipe it.
    window.rromsCart = { clearCart, getCart, updateCartBadge };

    updateCartBadge();
})();
