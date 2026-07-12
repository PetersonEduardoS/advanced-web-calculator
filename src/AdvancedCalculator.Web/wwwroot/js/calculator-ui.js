(function () {
    "use strict";

    const expressionDisplay = document.getElementById("display-expression");
    const resultDisplay = document.getElementById("display-result");
    const errorBanner = document.getElementById("calculator-error");
    const standardGrid = document.getElementById("standard-grid");
    const scientificGrid = document.getElementById("scientific-grid");
    const modeButtons = document.querySelectorAll(".mode-btn");
    const angleModeToggle = document.getElementById("angle-mode-toggle");

    let currentExpression = "";
    let lastResult = null;
    let currentMode = "standard";
    let angleMode = "radians"; // "radians" | "degrees"

    function render() {
        expressionDisplay.textContent = currentExpression || "\u00A0";
        resultDisplay.textContent = lastResult !== null ? formatNumber(lastResult) : "0";
    }

    function formatNumber(value) {
        if (!isFinite(value)) {
            return "Error";
        }
        // Avoid excessive floating-point noise (e.g. 0.1 + 0.2 = 0.30000000000000004)
        const rounded = Math.round(value * 1e10) / 1e10;
        return rounded.toString();
    }

    function showError(message) {
        errorBanner.textContent = message;
        errorBanner.hidden = false;
    }

    function clearError() {
        errorBanner.hidden = true;
        errorBanner.textContent = "";
    }

    function appendToExpression(text) {
        clearError();
        currentExpression += text;
        render();
    }

    function clearAll() {
        clearError();
        currentExpression = "";
        lastResult = null;
        render();
    }

    function backspace() {
        clearError();
        currentExpression = currentExpression.slice(0, -1);
        render();
    }

    function toggleSign() {
        clearError();
        if (currentExpression.trim() === "") {
            return;
        }
        currentExpression = `-(${currentExpression})`;
        render();
    }

    function applyPercent() {
        clearError();
        if (currentExpression.trim() === "") {
            return;
        }
        currentExpression = `(${currentExpression})/100`;
        render();
    }

    function insertFunction(functionName) {
        clearError();
        currentExpression += `${functionName}(`;
        render();
    }

    function insertPower() {
        clearError();
        currentExpression += "^";
        render();
    }

    function insertFactorial() {
        clearError();
        currentExpression += "!";
        render();
    }

    function insertLiteral(literal) {
        appendToExpression(literal);
    }

    function switchMode(mode) {
        currentMode = mode;

        modeButtons.forEach((button) => {
            const isActive = button.dataset.mode === mode;
            button.classList.toggle("active", isActive);
            button.setAttribute("aria-selected", isActive ? "true" : "false");
        });

        standardGrid.hidden = mode !== "standard";
        scientificGrid.hidden = mode !== "scientific";
        angleModeToggle.hidden = mode !== "scientific";
    }

    function toggleAngleMode() {
        angleMode = angleMode === "radians" ? "degrees" : "radians";
        angleModeToggle.textContent = angleMode === "radians" ? "RAD" : "DEG";
    }

    async function evaluateExpression() {
        if (currentExpression.trim() === "") {
            return;
        }

        clearError();

        const token = document
            .querySelector('meta[name="request-verification-token"]')
            .getAttribute("content");

        try {
            const response = await fetch("/Index?handler=Calculate", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "X-CSRF-TOKEN": token
                },
                body: JSON.stringify({
                    expression: currentExpression,
                    angleMode: angleMode
                })
            });

            const data = await response.json();

            if (data.success) {
                lastResult = data.result;
                render();
            } else {
                showError(data.error || "Invalid expression.");
            }
        } catch (networkError) {
            showError("Could not reach the server. Please try again.");
        }
    }

    function handleGridClick(event) {
        const button = event.target.closest("button[data-action]");
        if (!button) {
            return;
        }

        const action = button.dataset.action;
        const value = button.dataset.value;

        switch (action) {
            case "digit":
                appendToExpression(value);
                break;
            case "decimal":
                appendToExpression(".");
                break;
            case "operator":
                appendToExpression(` ${value} `);
                break;
            case "literal":
                insertLiteral(value);
                break;
            case "function":
                insertFunction(value);
                break;
            case "power":
                insertPower();
                break;
            case "factorial":
                insertFactorial();
                break;
            case "clear":
                clearAll();
                break;
            case "backspace":
                backspace();
                break;
            case "sign":
                toggleSign();
                break;
            case "percent":
                applyPercent();
                break;
            case "equals":
                evaluateExpression();
                break;
        }
    }

    standardGrid.addEventListener("click", handleGridClick);
    scientificGrid.addEventListener("click", handleGridClick);

    modeButtons.forEach((button) => {
        button.addEventListener("click", () => switchMode(button.dataset.mode));
    });

    angleModeToggle.addEventListener("click", toggleAngleMode);

    render();
})();