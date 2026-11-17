(() => {
    document.querySelectorAll("[data-image-preview-input]").forEach((input) => {
        const target = input.closest(".haven-field")?.querySelector("[data-image-preview]");
        if (!target) return;
        input.addEventListener("change", () => {
            target.replaceChildren();
            for (const file of Array.from(input.files ?? [])) {
                const figure = document.createElement("figure");
                const image = document.createElement("img");
                const caption = document.createElement("figcaption");
                image.src = URL.createObjectURL(file);
                image.alt = file.name;
                image.onload = () => URL.revokeObjectURL(image.src);
                caption.textContent = `${file.name} · ${(file.size / 1024 / 1024).toFixed(1)} MB`;
                figure.append(image, caption);
                target.append(figure);
            }
        });
    });
    document.querySelectorAll("[data-video-preview-input]").forEach((input) => {
        const target = input.closest(".haven-field")?.querySelector("[data-video-preview]");
        if (!target) return;
        input.addEventListener("change", () => {
            target.replaceChildren();
            for (const file of Array.from(input.files ?? [])) {
                const item = document.createElement("p");
                item.textContent = `${file.name} · ${(file.size / 1024 / 1024).toFixed(1)} MB`;
                target.append(item);
            }
        });
    });
    document.querySelectorAll("[data-location-province]").forEach((province) => {
        const container = province.closest("form") ?? province.parentElement?.parentElement;
        const ward = container?.querySelector("[data-location-ward]");
        if (!ward) return;
        province.addEventListener("change", async () => {
            const selected = ward.dataset.selected ?? "";
            ward.replaceChildren(new Option("Chọn xã / phường", ""));
            ward.disabled = true;
            if (!province.value) { ward.dataset.selected = ""; return; }
            try {
                const response = await fetch(`/api/locations/provinces/${encodeURIComponent(province.value)}/wards`, { headers: { Accept: "application/json" } });
                if (!response.ok) throw new Error("Không tải được danh mục địa điểm.");
                const items = await response.json();
                for (const item of items) ward.add(new Option(item.name, item.code, false, item.code === selected));
                ward.disabled = false;
                ward.dataset.selected = "";
            } catch {
                ward.replaceChildren(new Option("Không thể tải danh mục xã/phường", ""));
            }
        });
        if (province.value && ward.options.length <= 1) province.dispatchEvent(new Event("change"));
    });
})();
