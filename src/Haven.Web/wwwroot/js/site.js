// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const track = document.getElementById("havenPartnerTrack");

    if (!track) return;

    const buttons = document.querySelectorAll("[data-partner-scroll]");

    buttons.forEach(function (button) {
        button.addEventListener("click", function () {
            const direction = Number(
                button.getAttribute("data-partner-scroll")
            );

            const card = track.querySelector(".haven-partner-card");
            if (!card) return;

            const styles = window.getComputedStyle(track);
            const gap = parseFloat(styles.columnGap) || 0;
            const distance = card.getBoundingClientRect().width + gap;

            track.scrollBy({
                left: direction * distance,
                behavior: "smooth"
            });
        });
    });
});

// Shared lightweight motion: sticky-header state and one-time reveal.
document.addEventListener("DOMContentLoaded", function () {
    const header = document.querySelector(".app-header");
    const mobileMenu = document.querySelector(".app-mobile-menu");
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (header) {
        const updateHeader = () => header.classList.toggle("is-scrolled", window.scrollY > 12);
        updateHeader();
        window.addEventListener("scroll", updateHeader, { passive: true });
    }

    if (mobileMenu) {
        mobileMenu.querySelectorAll("a").forEach((link) => link.addEventListener("click", () => { mobileMenu.open = false; }));
        mobileMenu.addEventListener("keydown", (event) => { if (event.key === "Escape") mobileMenu.open = false; });
    }

    if (!reduceMotion && "IntersectionObserver" in window) {
        const targets = document.querySelectorAll(".haven-home section, .property-card, .page-heading, .content-card");
        if (targets.length) {
            targets.forEach((target, index) => {
                target.setAttribute("data-motion-pending", "");
                if (target.matches(".property-card")) target.style.setProperty("--motion-delay", `${Math.min(index % 3, 2) * 80}ms`);
            });
            document.documentElement.classList.add("has-scroll-motion");
            const observer = new IntersectionObserver((entries, currentObserver) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add("is-visible");
                        currentObserver.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.12, rootMargin: "0px 0px -36px 0px" });
            targets.forEach((target) => observer.observe(target));
            window.addEventListener("pagehide", () => observer.disconnect(), { once: true });
        }
    }
});
