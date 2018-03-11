(function (innerhtml) {
    var content = document.getElementById("content"),
        div = document.createElement("div");

    div.innerHTML = innerhtml;
    content.appendChild(div);
})('/EmbeddedResourcesScripts/EmbeddedScript1.js loaded');