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
        const provinceName = container.querySelector('input[name="Province"]');
        const wardName = container.querySelector('input[name="District"]');
        const status = container.querySelector("[data-location-status]") ?? document.createElement("span");
        if (!status.isConnected) {
            status.dataset.locationStatus = "";
            status.className = "location-filter-status";
            status.setAttribute("role", "status");
            status.setAttribute("aria-live", "polite");
            ward.insertAdjacentElement("afterend", status);
        }

        const placeholder = ward.options[0]?.textContent ?? "Chọn xã / phường";
        let requestId = 0;
        let activeRequest;

        const updateLegacyNames = () => {
            if (provinceName) provinceName.value = province.value ? province.selectedOptions[0]?.textContent?.trim() ?? "" : "";
            if (wardName) wardName.value = ward.value ? ward.selectedOptions[0]?.textContent?.trim() ?? "" : "";
        };

        const loadWards = async (selectedWardCode = "") => {
            activeRequest?.abort();
            const currentRequestId = ++requestId;
            const provinceCode = province.value;
            ward.replaceChildren(new Option(provinceCode ? "Đang tải xã / phường…" : placeholder, ""));
            ward.disabled = true;
            if (wardName) wardName.value = "";

            if (!provinceCode) {
                if (status) status.textContent = "";
                return;
            }

            if (status) status.textContent = "Đang tải danh sách xã / phường…";
            const controller = new AbortController();
            activeRequest = controller;

            try {
                const response = await fetch(`/api/locations/provinces/${encodeURIComponent(provinceCode)}/wards`, {
                    headers: { Accept: "application/json" },
                    signal: controller.signal
                });
                if (!response.ok) throw new Error("Không tải được danh mục địa phương.");
                const items = await response.json();
                if (!Array.isArray(items)) throw new Error("Dữ liệu địa phương không hợp lệ.");
                if (currentRequestId !== requestId || province.value !== provinceCode) return;

                ward.replaceChildren(new Option(placeholder, ""));
                for (const item of items) {
                    if (typeof item.code !== "string" || typeof item.name !== "string") continue;
                    ward.add(new Option(item.name, item.code, false, item.code === selectedWardCode));
                }
                ward.disabled = ward.options.length <= 1;
                if (status) status.textContent = ward.disabled
                    ? "Không có xã / phường trong danh mục cho tỉnh / thành phố này."
                    : "";
                updateLegacyNames();
            } catch (error) {
                if (error.name === "AbortError" || currentRequestId !== requestId) return;
                ward.replaceChildren(new Option("Không thể tải danh mục xã / phường", ""));
                ward.disabled = true;
                if (status) status.textContent = "Không tải được danh sách xã / phường. Hãy đổi tỉnh / thành phố hoặc thử lại.";
            } finally {
                if (currentRequestId === requestId) activeRequest = undefined;
            }
        };

        ward.addEventListener("change", updateLegacyNames);
        province.addEventListener("change", () => {
            updateLegacyNames();
            void loadWards();
        });

        if (!province.value) {
            ward.replaceChildren(new Option(placeholder, ""));
            ward.disabled = true;
            if (status) status.textContent = "";
        } else if (ward.options.length <= 1) {
            void loadWards(ward.dataset.selected ?? ward.value);
        } else if (status) {
            status.textContent = "";
        }
    });
})();
