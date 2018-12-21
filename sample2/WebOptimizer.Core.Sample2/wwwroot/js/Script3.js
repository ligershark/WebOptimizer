(function (innerhtml) {
    var content = document.getElementById("content"),
        div = document.createElement("div");

    div.innerHTML = innerhtml;
    content.appendChild(div);
})('wwwroot/js/Script3.js loaded');