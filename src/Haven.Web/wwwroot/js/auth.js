(() => {
    document.querySelectorAll("[data-password-toggle]").forEach((button) => {
        const inputId = button.getAttribute("aria-controls");
        const input = inputId ? document.getElementById(inputId) : null;
        if (!(input instanceof HTMLInputElement)) return;

        button.addEventListener("click", () => {
            const reveal = input.type === "password";
            input.type = reveal ? "text" : "password";
            button.textContent = reveal ? "Ẩn" : "Hiện";
            button.setAttribute("aria-pressed", String(reveal));
            button.setAttribute("aria-label", reveal ? "Ẩn mật khẩu" : "Hiện mật khẩu");
        });
    });
})();
