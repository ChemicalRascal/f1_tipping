// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

 if ('serviceWorker' in navigator) {
    window.addEventListener("load", async () => {
        await navigator.serviceWorker.register("/js/serviceWorker.js", { scope: "/" });
        window.dispatchEvent(new Event("serviceWorkerLoaded"));
    });
}