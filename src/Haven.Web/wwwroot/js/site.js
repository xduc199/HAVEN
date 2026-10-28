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

/* ==========================================
   HAVEN - CHAT WIDGET
   Mở/đóng, kéo thả và xử lý biểu mẫu
   ========================================== */

document.addEventListener("DOMContentLoaded", function () {
    const chat = document.getElementById("havenChat");

    if (!chat || chat.dataset.initialized === "true") return;
    chat.dataset.initialized = "true";

    const toggle = document.getElementById("havenChatToggle");
    const toggleIcon = document.getElementById("havenChatToggleIcon");
    const close = document.getElementById("havenChatClose");
    const cancel = document.getElementById("havenChatCancel");
    const form = document.getElementById("havenChatForm");
    const status = document.getElementById("havenChatStatus");
    const dragHandle = document.getElementById("havenChatDrag");

    function setOpen(open) {
        chat.classList.toggle("is-open", open);
        toggle.setAttribute("aria-expanded", String(open));
        toggle.setAttribute(
            "aria-label",
            open ? "Thu gọn khung chat" : "Mở khung chat"
        );
        toggleIcon.textContent = open ? "⌄" : "☏";
    }

    toggle.addEventListener("click", function () {
        setOpen(!chat.classList.contains("is-open"));
    });

    close.addEventListener("click", function (event) {
        event.stopPropagation();
        setOpen(false);
    });

    cancel.addEventListener("click", function () {
        form.reset();
        status.classList.remove("is-visible");
        status.textContent = "";
        setOpen(false);
    });

    form.addEventListener("submit", function (event) {
        event.preventDefault();

        if (!form.reportValidity()) return;

        const name = document.getElementById("havenChatName").value.trim();

        status.textContent =
            "Xin chào " + name +
            "! Biểu mẫu đã được kiểm tra trên giao diện. " +
            "Để nhân viên HAVEN nhận được yêu cầu, cần kết nối hệ thống chat với máy chủ.";

        status.classList.add("is-visible");
    });

    /* Kéo khung chat bằng chuột hoặc thao tác cảm ứng */
    let dragging = false;
    let startX = 0;
    let startY = 0;
    let originalLeft = 0;
    let originalTop = 0;

    dragHandle.addEventListener("pointerdown", function (event) {
        if (event.target.closest("button")) return;
        if (event.button !== 0 && event.pointerType === "mouse") return;

        const rect = chat.getBoundingClientRect();

        dragging = true;
        startX = event.clientX;
        startY = event.clientY;
        originalLeft = rect.left;
        originalTop = rect.top;

        chat.style.left = rect.left + "px";
        chat.style.top = rect.top + "px";
        chat.style.right = "auto";
        chat.style.bottom = "auto";

        dragHandle.setPointerCapture(event.pointerId);
        event.preventDefault();
    });

    dragHandle.addEventListener("pointermove", function (event) {
        if (!dragging) return;

        const windowElement = document.getElementById("havenChatWindow");
        const rect = chat.getBoundingClientRect();
        const panelHeight = chat.classList.contains("is-open")
            ? windowElement.getBoundingClientRect().height + 68
            : 0;

        let left = originalLeft + event.clientX - startX;
        let top = originalTop + event.clientY - startY;

        const minLeft = 8;
        const maxLeft = Math.max(minLeft, window.innerWidth - rect.width - 8);
        const minTop = 8;
        const maxTop = Math.max(
            minTop,
            window.innerHeight - Math.max(rect.height, panelHeight) - 8
        );

        left = Math.min(Math.max(left, minLeft), maxLeft);
        top = Math.min(Math.max(top, minTop), maxTop);

        chat.style.left = left + "px";
        chat.style.top = top + "px";
    });

    function stopDragging() {
        dragging = false;
    }

    dragHandle.addEventListener("pointerup", stopDragging);
    dragHandle.addEventListener("pointercancel", stopDragging);
});
