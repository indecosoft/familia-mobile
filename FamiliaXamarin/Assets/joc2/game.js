const scene = {
    init: '',
    update: '',
    finish: '',
    refreshIntervalId: ''
}

let childs = [];
const idChild = "#child";

let dashedElements = [];


scene.init = function () {

    sendMessage("init");

    $(".parent").css({ top: 30, left: 30, position: 'absolute', display: 'block' });

    generateDashedElements(4);
    generateBorderedElements(4);

    for (let i = 0; i < childs.length; i++) {
        sendMessage("id " + childs[i].id + ", oldPosition " + childs[i].oldPosition.x + ", " + childs[i].oldPosition.y);
    }


    $(".round-green").css({ top: 80, left: 750, position: 'absolute', display: 'block' });
    $(".round-grey").css({ top: 120, left: 800, position: 'absolute', display: 'block' });
    scene.refreshIntervalId = setInterval(scene.update, 5);
}


scene.update = function () {

    for (let i = 0; i < childs.length; i++) {

        player = {
            width: $(idChild + childs[i].id).outerWidth(),
            height: $(idChild + childs[i].id).outerHeight(),
            center: {
                x: $(idChild + childs[i].id).offset().left + ($(idChild + childs[i].id).outerWidth() / 2),
                y: $(idChild + childs[i].id).offset().top + ($(idChild + childs[i].id).outerHeight() / 2)
            }
        };

        if (childs[i].oldPosition.x != player.center.x || childs[i].oldPosition.y != player.center.y) {
            // here you have the player that you moved from his orginal position
            // TODO check here if the player is inside of one of dashed divs, you can find them in dashedElements list

        }
    }

}


scene.finish = function () {

    sendMessage("finish");
    clearInterval(scene.refreshIntervalId);
}


function generateDashedElements(nrOfItems) {
    dashedElements = [];
    let centerX = 100;
    let centerY = 80;
    for (let i = 1; i <= nrOfItems; i++) {
        let item = $('<div class="dashed"></div>');
        sendMessage(" x and y and i " + centerX + ", " + centerY + ", " + i);
        centerX += 100;

        item.css({ left: centerX, top: centerY, display: 'block', position: 'absolute' });
        $("body").append(item);

        let obj = {
            id: i,
            jqueryElement: item
        }

        dashedElements.push(obj);
    }
}

function generateBorderedElements(nrOfItems) {
    childs = [];
    let centerX = 100;
    let centerY = 300;
    for (let i = 1; i <= nrOfItems; i++) {
        let item = $('<div id="child' + i + '" class="child"></div>');
        centerX += 100;

        item.css({ left: centerX, top: centerY, display: 'block', position: 'absolute', 'background-image': 'url("images/pui' + i + '.png")' })
            .draggable();
        $("body").append(item);

        let obj = {
            id: i,
            jqueryElement: item,
            oldPosition: {
                    x: $(idChild + i).offset().left + ($(idChild + i).outerWidth() / 2),
                    y: $(idChild + i).offset().top + ($(idChild + i).outerHeight() / 2)
            },
            currentPosition: {
                x: $(idChild + i).offset().left + ($(idChild + i).outerWidth() / 2),
                y: $(idChild + i).offset().top + ($(idChild + i).outerHeight() / 2)
            }
        }

        childs.push(obj);

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

