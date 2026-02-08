if (window.matchMedia) {
    const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
    darkModeQuery.addEventListener("change", updateTheme);
    updateTheme(darkModeQuery);
}

function updateTheme(darkModeQuery) {
    var pageDataset = document.querySelector("html").dataset;
    if (darkModeQuery && darkModeQuery.matches) {
        pageDataset.bsTheme = "dark";
    } else {
        pageDataset.bsTheme = "light";
    }
}