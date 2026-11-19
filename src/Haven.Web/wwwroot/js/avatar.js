document.addEventListener("error", (event) => {
    const image = event.target;
    if (image instanceof HTMLImageElement && image.matches("[data-avatar-image]")) {
        image.hidden = true;
    }
}, true);

document.querySelectorAll("[data-avatar-upload]").forEach((form) => {
    const input = form.querySelector("input[type=file]");
    const preview = form.querySelector("[data-avatar-preview]");
    const cancel = form.querySelector("[data-avatar-cancel]");
    if (!(input instanceof HTMLInputElement) || !(preview instanceof HTMLImageElement)) return;
    const original = preview.src;
    let objectUrl;
    input.addEventListener("change", () => {
        const file = input.files?.[0];
        if (!file) return;
        if (objectUrl) URL.revokeObjectURL(objectUrl);
        objectUrl = URL.createObjectURL(file);
        preview.src = objectUrl;
        form.querySelector("[data-avatar-save]").disabled = false;
        if (cancel) cancel.hidden = false;
    });
    cancel?.addEventListener("click", () => {
        input.value = "";
        preview.src = original;
        form.querySelector("[data-avatar-save]").disabled = true;
        cancel.hidden = true;
    });
    form.addEventListener("submit", () => {
        form.setAttribute("aria-busy", "true");
        form.querySelector("[data-avatar-save]").disabled = true;
    });
});
