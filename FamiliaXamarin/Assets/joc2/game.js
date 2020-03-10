const scene = {
    init: '',
    update: '',
    finish: '',
    refreshIntervalId: ''
}


scene.init = function () {

    sendMessage("init");

   
    $(".parent").css({ top: 30, left: 30, position: 'absolute', display: 'block' });

    generateDashedElements(4);
    generateBorderedElements(7);


    $(".round-green").css({ top: 80, left: 750, position:'absolute', display: 'block' });
    $(".round-grey").css({ top: 120, left: 800, position: 'absolute', display: 'block' });
    scene.refreshIntervalId = setInterval(scene.update, 5);
}


scene.update = function () {
    sendMessage("update");
}


scene.finish = function () {

    sendMessage("finish");
    clearInterval(scene.refreshIntervalId);
}


function generateDashedElements(nrOfItems) {
    let centerX = 100;
    let centerY = 80;
    for (let i = 1; i < nrOfItems; i++) {
        let item = $('<div class="dashed"></div>');
        sendMessage(" x and y and i " + centerX + ", " + centerY + ", " + i);
        centerX += 100;
        item.css({ left: centerX, top: centerY, display: 'block', position: 'absolute' });
        $("body").append(item);
    }
}

function generateBorderedElements(nrOfItems) {
    let centerX = 100;
    let centerY = 300;
    for (let i = 1; i < nrOfItems; i++) {
        let item = $('<div class="child"></div>');
        sendMessage(" x and y and i " + centerX + ", " + centerY + ", " + i);
        centerX += 100;
        item.css({ left: centerX, top: centerY, display: 'block', position: 'absolute', 'background-image': 'url("images/pui' + i + '.png")' })
            .draggable();
        $("body").append(item);
    }
}

function getFromAndroid() {
    alert(AndroidJSHandler.getFromAndroid());
}

function sendMessage(message) {
    AndroidJSHandler.receiveMessageFromJS(message);
}

$(document).ready(function () {
    sendMessage("joc 2 document ready");
    getFromAndroid();
    scene.init();
});

