(() => {
    const tokenUrl = "/api/auth/antiforgery";

    async function getRequestToken() {
        const response = await fetch(tokenUrl, {
            credentials: "same-origin",
            headers: { Accept: "application/json" }
        });
        if (!response.ok) {
            throw new Error("Không lấy được mã bảo vệ yêu cầu. Hãy tải lại trang và thử lại.");
        }

        const body = await response.json();
        if (!body.requestToken) {
            throw new Error("API không trả về mã bảo vệ yêu cầu.");
        }

        return body.requestToken;
    }

    async function sendJson(url, method, body) {
        const requestToken = await getRequestToken();
        const response = await fetch(url, {
            method,
            credentials: "same-origin",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
                RequestVerificationToken: requestToken
            },
            body: body === undefined ? undefined : JSON.stringify(body)
        });

        let payload = null;
        if (response.status !== 204) {
            const contentType = response.headers.get("content-type") || "";
            if (contentType.includes("json")) {
                payload = await response.json();
            }
        }

        if (!response.ok) {
            throw new ApiRequestError(response.status, payload);
        }

        return payload;
    }

    class ApiRequestError extends Error {
        constructor(status, payload) {
            super(formatError(status, payload));
            this.status = status;
            this.payload = payload;
        }
    }

    function formatError(status, payload) {
        if (status === 401) {
            return "Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng kiểm tra thông tin và thử lại.";
        }
        if (status === 403) {
            return "Bạn không được phép thực hiện thao tác này.";
        }

        const errors = payload && payload.errors;
        if (errors && typeof errors === "object") {
            const messages = Object.values(errors)
                .flat()
                .filter(value => typeof value === "string" && value.trim());
            if (messages.length) {
                return [...new Set(messages)].join(" ");
            }
        }

        if (payload && (payload.detail || payload.title)) {
            return payload.detail || payload.title;
        }

        return `Yêu cầu thất bại (HTTP ${status}). Vui lòng thử lại.`;
    }

    function field(formData, name) {
        for (const [key, value] of formData.entries()) {
            if (key.toLowerCase() === name.toLowerCase()) {
                return value;
            }
        }
        return "";
    }

    function showMessage(form, message, isError) {
        const summary = form.querySelector("[data-api-message]");
        if (!summary) {
            return;
        }
        summary.textContent = message;
        summary.hidden = !message;
        summary.setAttribute("role", isError ? "alert" : "status");
        summary.classList.toggle("api-form-message--error", isError);
        summary.classList.toggle("api-form-message--success", !isError);
    }

    function destinationForLogin(account, formData) {
        if (Array.isArray(account.roles) && account.roles.includes("Admin")) {
            return "/Admin";
        }

        const returnUrl = field(formData, "ReturnUrl");
        if (!returnUrl || !returnUrl.startsWith("/") || returnUrl.startsWith("//")) {
            return "/";
        }

        let destination;
        try {
            destination = new URL(returnUrl, window.location.origin);
        } catch {
            return "/";
        }
        if (destination.origin !== window.location.origin) {
            return "/";
        }

        let path;
        try {
            path = decodeURIComponent(destination.pathname);
        } catch {
            return "/";
        }
        const firstSegment = path.replaceAll("\\", "/").replace(/^\/+/, "").split("/", 1)[0];
        return firstSegment.toLowerCase() === "admin" ? "/" : destination.href;
    }

    async function submitApiForm(form) {
        const action = form.dataset.apiAction;
        const formData = new FormData(form);
        let redirectTo = null;
        let request;

        switch (action) {
            case "login":
                request = sendJson("/api/auth/login", "POST", {
                    email: field(formData, "Email"),
                    password: field(formData, "Password"),
                    rememberMe: field(formData, "RememberMe").toLowerCase() === "true"
                }).then(account => { redirectTo = destinationForLogin(account, formData); });
                break;
            case "register":
                request = sendJson("/api/auth/register", "POST", {
                    displayName: field(formData, "DisplayName"),
                    email: field(formData, "Email"),
                    password: field(formData, "Password")
                }).then(() => { redirectTo = "/"; });
                break;
            case "profile":
                request = sendJson("/api/auth/me", "PUT", {
                    displayName: field(formData, "DisplayName")
                });
                break;
            case "change-password": {
                const newPassword = field(formData, "NewPassword");
                if (newPassword !== field(formData, "ConfirmPassword")) {
                    showMessage(form, "Mật khẩu xác nhận không khớp.", true);
                    return;
                }
                request = sendJson("/api/auth/change-password", "POST", {
                    currentPassword: field(formData, "CurrentPassword"),
                    newPassword
                });
                break;
            }
            case "logout":
                request = sendJson("/api/auth/logout", "POST")
                    .then(() => { redirectTo = "/"; });
                break;
            default:
                return;
        }

        const submitButtons = [...form.querySelectorAll('button[type="submit"], input[type="submit"]')];
        submitButtons.forEach(button => { button.disabled = true; });
        form.setAttribute("aria-busy", "true");
        showMessage(form, "", false);

        try {
            await request;
            if (redirectTo) {
                window.location.assign(redirectTo);
                return;
            }
            showMessage(
                form,
                action === "profile" ? "Hồ sơ đã được cập nhật." : "Mật khẩu đã được cập nhật.",
                false);
            if (action === "change-password") {
                form.reset();
            }
        } catch (error) {
            showMessage(
                form,
                error instanceof Error ? error.message : "Không thể hoàn tất yêu cầu.",
                true);
        } finally {
            form.removeAttribute("aria-busy");
            submitButtons.forEach(button => { button.disabled = false; });
        }
    }

    document.addEventListener("submit", event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.dataset.apiAction) {
            return;
        }

        event.preventDefault();
        if (typeof form.reportValidity === "function" && !form.reportValidity()) {
            return;
        }
        void submitApiForm(form);
    });
})();
