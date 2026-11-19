(() => {
    document.querySelectorAll("[data-home-image]").forEach((image) => {
        image.addEventListener("error", () => {
            image.closest(".carousel-item")?.classList.add("home-image-fallback");
            image.remove();
        }, { once: true });
    });

    const root = document.querySelector("#haven-home-carousel");
    if (!root || !window.bootstrap?.Carousel) return;

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const toggle = root.querySelector("[data-carousel-toggle]");
    const items = [...root.querySelectorAll(".carousel-item")];
    const indicators = [...root.querySelectorAll(".haven-home__hero-indicators button")];
    let userPaused = reducedMotion.matches;
    let focusPaused = false;
    let hoverPaused = false;

    const carousel = window.bootstrap.Carousel.getOrCreateInstance(root, {
        interval: reducedMotion.matches ? false : 6000,
        pause: "hover",
        keyboard: true,
        touch: true,
        wrap: true
    });

    const setButtonState = (playing) => {
        if (!toggle) return;
        toggle.setAttribute("aria-pressed", String(!playing));
        toggle.setAttribute("aria-label", playing ? "Tạm dừng trình chiếu" : "Tiếp tục trình chiếu");
        toggle.querySelector("span").textContent = playing ? "Ⅱ" : "▶";
    };

    const syncSlideAccessibility = () => {
        items.forEach((item) => {
            const isActive = item.classList.contains("active");
            item.toggleAttribute("inert", !isActive);
            item.setAttribute("aria-hidden", String(!isActive));
        });
        const activeIndex = items.findIndex((item) => item.classList.contains("active"));
        indicators.forEach((indicator, index) => {
            if (index === activeIndex) indicator.setAttribute("aria-current", "true");
            else indicator.removeAttribute("aria-current");
        });
    };

    syncSlideAccessibility();
    if (userPaused) carousel.pause();
    else carousel.cycle();
    setButtonState(!userPaused);
    if (toggle) toggle.hidden = reducedMotion.matches;

    root.addEventListener("slide.bs.carousel", () => carousel.pause());
    root.addEventListener("slid.bs.carousel", () => {
        syncSlideAccessibility();
        if (!userPaused && !focusPaused && !hoverPaused && !reducedMotion.matches) carousel.cycle();
    });

    root.addEventListener("pointerenter", () => {
        hoverPaused = true;
        carousel.pause();
    });

    root.addEventListener("pointerleave", () => {
        hoverPaused = false;
        if (!userPaused && !focusPaused && !reducedMotion.matches) carousel.cycle();
    });

    root.addEventListener("focusin", () => {
        focusPaused = true;
        carousel.pause();
    });

    root.addEventListener("focusout", (event) => {
        if (root.contains(event.relatedTarget)) return;
        focusPaused = false;
        if (!userPaused && !hoverPaused && !reducedMotion.matches) carousel.cycle();
    });

    toggle?.addEventListener("click", () => {
        userPaused = !userPaused;
        if (userPaused) carousel.pause();
        else if (!focusPaused && !hoverPaused && !reducedMotion.matches) carousel.cycle();
        setButtonState(!userPaused && !reducedMotion.matches);
    });

    reducedMotion.addEventListener("change", (event) => {
        if (event.matches) {
            userPaused = true;
            carousel.pause();
            setButtonState(false);
            if (toggle) toggle.hidden = true;
        } else {
            userPaused = false;
            if (toggle) toggle.hidden = false;
            if (!focusPaused && !hoverPaused) carousel.cycle();
            setButtonState(true);
        }
    });
})();
