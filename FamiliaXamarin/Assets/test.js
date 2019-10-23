const scene = {
    init: '',
    update: '',
    finish: '',

    physics: {
        gravity: 1,
        velocity: 0
    }
}

let dimensionsString;
let movementX;
let movementY;
let speedX;
let speedY;
let isOver;
let isStarted;
let refreshIntervalId;
let direction = "Down";
let lastDirection = "Down";
let collectableItems = [];
let bigScore;
let currentScore;

$.keyframe.define([
    {
        name: 'home-move',
        '0%': {
            'margin-left': 50 + 'px',
            'margin-top': 0 + 'px'
        },
        '100%': {
            'margin-left': 50 + 'px',
            'margin-top': 200 + 'px'
        }
    },
    {
        name: 'bounce',
        '0%': {
            transform: 'translateY(0);'
        },
        '50%': {
            transform: 'translateY(-60px);'
        },
        '100%': {
            transform: 'translateY(0);'
        }
    }
]);

scene.init = function () {
    isStarted = false;
    isOver = false;
    bigScore = Number(JSHandler.getScore());
    currentScore = 0;
    sendMessage("SCORE " + score);
    $("#reset").css({ display: 'none' });
    $("#score").css({ display: 'none' });
    $("#itemsCollected").css({ display: 'none' });
    $(".player").css({ display: 'none' });
    $(".arrival").css({ top: '0px', left: '50px', position: 'absolute' });

    $('.arrival').playKeyframe({
        name: 'home-move',
        duration: '2s',
        timingFunction: 'ease-out',
        'fill-mode': 'forwards',
        complete: function () {
            $(".player").css({ top: '0px', left: '0px', position: 'absolute', display: 'block', animation: 'none' });
            collectableItems = appendCollectableItem(getRandomr(3, 10));
            isStarted = true;
            sendMessage("Animation is completed. Starting the game .... ");
            sendMessage("Loop started..");
            dimensionsString = JSHandler.getScreenDimension();
            scene.physics.velocity = 0;
            refreshIntervalId = setInterval(scene.update, 10);
        }
    });
    sendMessage("game loaded");
}

scene.update = function () {
    if (isOver === false && isStarted === true) {
        let values = JSHandler.getXYFromGyro();
        let currentX = Number(values.split("/")[0]);
        let currentY = Number(values.split("/")[1]);

        $(".player").parent().css({ position: 'relative' });
        $(".arrival").css({ position: 'absolute' });

        const player = {
            width: $(".player").outerWidth(),
            height: $(".player").outerHeight(),
            center: {
                x: $(".player").offset().left + ($(".player").outerWidth() / 2),
                y: $(".player").offset().top + ($(".player").outerHeight() / 2)
            },
            position: {
                left: $(".player").position().left,
                top: $(".player").position().top
            }
        };

        const arrivalPlace = {
            width: $(".arrival").outerWidth(),
            height: $(".arrival").outerHeight(),
            center: {
                x: $(".arrival").offset().left + ($(".arrival").outerWidth() / 2),
                y: $(".arrival").offset().top + ($(".arrival").outerHeight() / 2)
            }
        };

        movementY = player.position.top;
        movementX = player.position.left;

        lastDirection = direction;
        direction = getDirection(currentX, currentY);
        setVelocity();

        switch (direction) {
            case "Right":
                sendMessage("GO Right");
                movementX += scene.physics.velocity;
                break;
            case "Left":
                sendMessage("GO Left");
                movementX -= scene.physics.velocity;
                break;
            case "Down":
                sendMessage("GO Down");
                movementY += scene.physics.velocity;
                break;
            case "Up":
                sendMessage("GO Up");
                movementY -= scene.physics.velocity;
                break;
            default:
                sendMessage("none");
        }


        if (isInArea(player) === true) {
            $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
        }
        else {
            scene.physics.velocity = 0;
        }

        collect(player);
        
        if (checkForFinish(player, arrivalPlace) === true) {
            isOver = true;
        }

    } else {
        scene.finish();
    }
};

scene.finish = function () {
    sendMessage("finish called");
    displaySceneForFinish();
    JSHandler.saveScore(bigScore + "");
    clearInterval(refreshIntervalId);
}

function displaySceneForFinish() {
    $("#reset").css({ display: 'block' });
    $("#score").text("Scor: " + bigScore).css({ display: 'block' });
    $("#itemsCollected").text("Obiecte colectate: " + currentScore).css({ display: 'block' });
    displayPlayerForFinish();
}

function getDirection(currentX, currentY) {
    if (currentX > 0.2) {
        direction = "Right";
    }
    if (currentX < -0.2) {
        direction = "Left";
    }
    if (currentY < -0.2) {
        direction = "Up";
    }
    if (currentY > 0.2) {
        direction = "Down";
    }

    return direction;
}

function setVelocity() {
    if (lastDirection !== direction) {
        scene.physics.velocity= 0;
    }
    else {
        scene.physics.velocity += scene.physics.gravity;
    }
}

function collect(player) {

    if ($(".brick").length > 0) {
        for (let i = 0; i < collectableItems.length; i++) {
            const brick = {
                width: collectableItems[i].outerWidth(),
                height: collectableItems[i].outerHeight(),
                center: {
                    x: collectableItems[i].offset().left + (collectableItems[i].outerWidth() / 2),
                    y: collectableItems[i].offset().top + (collectableItems[i].outerHeight() / 2)
                }
            };
         
            if (Math.floor(player.center.x) < Math.floor(brick.center.x + (brick.width / 2)) &&
                Math.floor(player.center.x) > Math.floor(brick.center.x - (brick.width / 2)) &&
                Math.floor(player.center.y < Math.floor(brick.center.y + (brick.height / 2))) &&
                Math.floor(player.center.y) > Math.floor(brick.center.y - (brick.height / 2))) {
                collectableItems[i].remove();
                currentScore++;
                bigScore++;
            }
        }
    }
    else {
        collectableItems = [];
    }
}

function isInArea(player) {
    if (movementY <= Number($(window).height()) - player.height && movementY >= 0 &&
        movementX <= Number($(window).width()) - player.width && movementX >= 0) {
        return true;
    }
    return false;
}

function displayPlayerForFinish() {
    const arrivalPlace = {
        width: $(".arrival").outerWidth(),
        height: $(".arrival").outerHeight(),
        center: {
            x: $(".arrival").offset().left,
            y: $(".arrival").offset().top
        }
    };
    $(".player").css({ left: arrivalPlace.center.x, top: arrivalPlace.center.y, position: 'absolute' });
    $('.player').playKeyframe({
        name: 'bounce',
        duration: '2s',
        complete: function () { }
    });
}

function checkForFinish(player, arrivalPlace) {
    if (Math.floor(player.center.x) < Math.floor(arrivalPlace.center.x + (arrivalPlace.width / 2)) &&
        Math.floor(player.center.x) > Math.floor(arrivalPlace.center.x - (arrivalPlace.width / 2)) &&
        Math.floor(player.center.y < Math.floor(arrivalPlace.center.y + (arrivalPlace.height / 2))) &&
        Math.floor(player.center.y) > Math.floor(arrivalPlace.center.y - (arrivalPlace.height / 2))) {
        return true;
    }
    return false;
}

function appendCollectableItem(noOfItems) {
    let items = [];
    let count = 0;
    while (count < noOfItems) {
        let height = getRandomr(50, Number($(window).height() - 80));
        let width = getRandomr(50, Number($(window).width() - 80));
        sendMessage("RANDOM H:" + height + " W:" + width);
        if (checkCollider(width, height) == false) {
            let item = $('<div class="brick"></div>');
            item.css({ left: width, top: height, position: 'absolute' });
            items.push(item);
            $("body").append(item);
            count++;
        }
    }

    return items;

    function checkCollider(width, height) {
        const arrivalPlace = {
            width: $(".arrival").outerWidth(),
            height: $(".arrival").outerHeight(),
            center: {
                x: $(".arrival").offset().left + ($(".arrival").outerWidth() / 2),
                y: $(".arrival").offset().top + ($(".arrival").outerHeight() / 2)
            }
        };

        if (width - 40 <= Math.floor(arrivalPlace.center.x + (arrivalPlace.width / 2)) &&
            width + 40 >= Math.floor(arrivalPlace.center.x - (arrivalPlace.width / 2)) &&
            height - 40 <= Math.floor(arrivalPlace.center.y + (arrivalPlace.height / 2)) &&
            height + 40 >= Math.floor(arrivalPlace.center.y - (arrivalPlace.height / 2))) {
            return true;
        } else {
            return false;
        }
    }
}

function playAgain() {
    sendMessage("reloading game...");
    collectableItems.forEach(element => element.remove());
    scene.init();
}

function getRandomr(min, max) {
    return Math.floor(Math.random() * (max - min)) + min;
}

function getFromAndroid() {
    alert(JSHandler.getFromAndroid());
}

function sendMessage(message) {
    JSHandler.receiveMessageFromJS(message);
}

$(document).ready(function () {
    scene.init();
});

