(function () {
    "use strict";

    const list = document.getElementById("history-list");
    if (!list) {
        return; // Empty state, no list rendered
    }

    function getAntiforgeryToken() {
        return document
            .querySelector('meta[name="request-verification-token"]')
            .getAttribute("content");
    }

    async function postJson(url, body) {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-CSRF-TOKEN": getAntiforgeryToken()
            },
            body: JSON.stringify(body)
        });
        return response.json();
    }

    async function toggleFavorite(entryElement, id) {
        const data = await postJson("/History?handler=ToggleFavorite", { id });
        if (data.success) {
            const button = entryElement.querySelector('[data-action="favorite"]');
            button.textContent = data.isFavorite ? "★" : "☆";
            button.classList.toggle("active", data.isFavorite);
        }
    }

    async function deleteEntry(entryElement, id) {
        const data = await postJson("/History?handler=Delete", { id });
        if (data.success) {
            entryElement.remove();
        }
    }

    function copyExpression(entryElement) {
        const expression = entryElement.querySelector(".history-expression").textContent;
        navigator.clipboard.writeText(expression).catch(() => {
            // Clipboard API can fail silently on unsupported/insecure contexts;
            // this is a non-critical convenience feature, so we just no-op.
        });
    }

    list.addEventListener("click", (event) => {
        const button = event.target.closest("button[data-action]");
        if (!button) {
            return;
        }

        const entryElement = button.closest(".history-entry");
        const id = parseInt(entryElement.dataset.id, 10);
        const action = button.dataset.action;

        switch (action) {
            case "favorite":
                toggleFavorite(entryElement, id);
                break;
            case "copy":
                copyExpression(entryElement);
                break;
            case "delete":
                deleteEntry(entryElement, id);
                break;
        }
    });
})();