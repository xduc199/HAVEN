(() => {
    document.querySelectorAll("[data-home-image]").forEach((image) => {
        image.addEventListener("error", () => {
            image.closest("figure")?.classList.add("home-image-fallback");
            image.remove();
        }, { once: true });
    });
})();
