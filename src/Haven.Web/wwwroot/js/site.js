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
