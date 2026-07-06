(function () {
    "use strict";

    const expressionDisplay = document.getElementById("display-expression");
    const resultDisplay = document.getElementById("display-result");
    const errorBanner = document.getElementById("calculator-error");
    const standardGrid = document.getElementById("standard-grid");

    let currentExpression = "";
    let lastResult = null;

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
        // Wrap the whole current expression in a unary minus using parentheses,
        // relying on the parser's correct handling of unary minus rather than
        // any string-splicing heuristics.
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
                body: JSON.stringify({ expression: currentExpression })
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

    standardGrid.addEventListener("click", (event) => {
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
    });

    render();
})();