
var SeasonSelector = document.querySelector("select.season-selector");

const seasonMainUrl = "/api/PlayerSeason/";

SeasonSelector.onchange = (e) => PutSeasonId(e.target.value);

async function PutSeasonId(id) {
    var putUrl = seasonMainUrl + id;

    var fetchResult = await $.ajax({
        method: "PUT",
        url: putUrl,
    });

    if (fetchResult.requiresRefresh) {
        window.location.reload();
    }
}
