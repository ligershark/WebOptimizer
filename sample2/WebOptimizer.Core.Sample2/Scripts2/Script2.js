(function (innerhtml) {
    var content = document.getElementById("content"),
        div = document.createElement("div");

    div.innerHTML = innerhtml;
    content.appendChild(div);
})('/Scripts2/Script2.js loaded');