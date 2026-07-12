(function () {
    "use strict";

    const root = document.documentElement;
    const toggleButton = document.getElementById("theme-toggle");
    const STORAGE_KEY = "advanced-calculator-theme";

    function applyTheme(theme) {
        root.setAttribute("data-theme", theme);
        toggleButton.textContent = theme === "dark" ? "☀️" : "🌙";
    }

    const savedTheme = localStorage.getItem(STORAGE_KEY);
    if (savedTheme === "dark" || savedTheme === "light") {
        applyTheme(savedTheme);
    }

    toggleButton.addEventListener("click", () => {
        const isDark = root.getAttribute("data-theme") === "dark";
        const nextTheme = isDark ? "light" : "dark";
        applyTheme(nextTheme);
        localStorage.setItem(STORAGE_KEY, nextTheme);
    });
})();