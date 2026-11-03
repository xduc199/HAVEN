(() => {
    const toggle = document.querySelector(".admin-menu-toggle");
    if (!toggle) return;

    toggle.addEventListener("click", () => {
        const expanded = document.body.classList.toggle("admin-sidebar-open");
        toggle.setAttribute("aria-expanded", String(expanded));
    });
})();
