(() => {
    const toggle = document.querySelector(".admin-menu-toggle");
    const sidebar = document.querySelector(".admin-sidebar");
    const backdrop = document.querySelector(".admin-sidebar-backdrop");
    const setSidebarOpen = (open) => {
        document.body.classList.toggle("admin-sidebar-open", open);
        toggle?.setAttribute("aria-expanded", String(open));
        backdrop?.setAttribute("aria-hidden", String(!open));
    };

    toggle?.addEventListener("click", () => {
        setSidebarOpen(!document.body.classList.contains("admin-sidebar-open"));
    });

    backdrop?.addEventListener("click", () => setSidebarOpen(false));
    sidebar?.querySelectorAll("a").forEach((link) => link.addEventListener("click", () => setSidebarOpen(false)));
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") setSidebarOpen(false);
    });

    document.querySelectorAll("[data-admin-lockout-form]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            const confirmation = form.dataset.confirm;
            if (confirmation && !window.confirm(confirmation)) {
                event.preventDefault();
                return;
            }

            const button = form.querySelector("button[type='submit']");
            if (button instanceof HTMLButtonElement) {
                button.disabled = true;
                button.setAttribute("aria-busy", "true");
                button.textContent = form.dataset.loading || "Đang xử lý…";
            }
        });
    });
})();
