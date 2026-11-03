
document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    const box = document.getElementById("authBox");

    const loginSide = document.getElementById("loginSide");
    const registerSide = document.getElementById("registerSide");

    const panelRegister = document.getElementById("panelRegister");
    const panelLogin = document.getElementById("panelLogin");

    const switchRegister = document.getElementById("switchRegister");
    const switchLogin = document.getElementById("switchLogin");

    const registerForm = document.getElementById("registerForm");

    if (
        !box ||
        !loginSide ||
        !registerSide ||
        !panelRegister ||
        !panelLogin
    ) {
        console.error("Không tìm thấy giao diện Login/Register.");
        return;
    }

    // CHUYỂN LOGIN / REGISTER
    function setMode(mode) {
        const isRegister = mode === "register";

        box.classList.toggle("show-register", isRegister);

        // Kích hoạt đúng form
        loginSide.inert = isRegister;
        registerSide.inert = !isRegister;

        loginSide.setAttribute(
            "aria-hidden",
            String(isRegister)
        );

        registerSide.setAttribute(
            "aria-hidden",
            String(!isRegister)
        );

        // Nội dung trong mảng xanh
        panelRegister.inert = isRegister;
        panelLogin.inert = !isRegister;

        panelRegister.setAttribute(
            "aria-hidden",
            String(isRegister)
        );

        panelLogin.setAttribute(
            "aria-hidden",
            String(!isRegister)
        );
    }

    // BẤM REGISTER
    switchRegister.addEventListener("click", function () {
        setMode("register");
    });

    // BẤM LOGIN
    switchLogin.addEventListener("click", function () {
        setMode("login");
    });

    // NÚT CHUYỂN Ở DƯỚI FORM
    document.querySelectorAll("[data-mode]").forEach(
        function (button) {
            button.addEventListener("click", function () {
                setMode(button.dataset.mode);
            });
        }
    );

    // HIỆN / ẨN MẬT KHẨU
    document.querySelectorAll(".password-toggle").forEach(
        function (button) {
            button.addEventListener("click", function () {
                const input = document.getElementById(
                    button.dataset.target
                );

                if (!input) return;

                const show = input.type === "password";

                input.type = show ? "text" : "password";

                button.textContent = show ? "◎" : "◉";

                button.setAttribute(
                    "aria-label",
                    show ? "Ẩn mật khẩu" : "Hiện mật khẩu"
                );

                input.focus();
            });
        }
    );

    // FORM ĐĂNG KÝ CHƯA CÓ BACKEND
    if (registerForm) {
        registerForm.addEventListener(
            "submit",
            function (event) {
                event.preventDefault();
            }
        );
    }

    // MẶC ĐỊNH HIỆN LOGIN
    setMode("login");
});
