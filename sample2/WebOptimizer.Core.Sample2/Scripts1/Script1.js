(function (innerhtml) {
    var content = document.getElementById("content"),
        div = document.createElement("div");

    div.innerHTML = innerhtml;
    content.appendChild(div);
})('/Scripts1/Script1.js loaded');