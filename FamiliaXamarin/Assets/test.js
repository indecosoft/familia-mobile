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
let direction;
let lastDirection;
let complexDirection;

$.keyframe.define([
    {
        name: 'home-move',
        '0%': {
            'margin-left': 50 + 'px',
            'margin-top': 50 + 'px'
        },
        '100%': {
            'margin-left': 100 + 'px',
            'margin-top': 150 + 'px'
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
    $("#reset").css({ display: 'none' });
    $("#score").css({ display: 'none' });
    $("#itemsCollected").css({ display: 'none' });
    $(".player").css({ display: 'none' });
    $(".arrival").css({ top: '0px', left: '50px', position: 'absolute' });

    $('.arrival').playKeyframe({
        name: 'home-move',
        duration: '1s',
        timingFunction: 'ease-out',
        'fill-mode': 'forwards',
        complete: function () {
            $(".player").css({ top: '0px', left: '0px', position: 'absolute', display: 'block', animation: 'none' });
            isStarted = true;
            sendMessage("Animation is completed. Starting the game .... ");
            sendMessage("Loop started..");
            direction = "Down";
            lastDirection = "Down";
            complexDirection = "Down";
            dimensionsString = JSHandler.getScreenDimension();
            scene.physics.velocity = 0;
            refreshIntervalId = setInterval(scene.update, 5);
        }
    });
    sendMessage("game loaded");
}

scene.update = function () {
    if (isOver === false && isStarted === true) {
        let values = JSHandler.getXYFromSensor();
        //landscape orientation
        let currentX = Number(values.split("/")[1]); //Number(values.split("/")[0]);  // portrait orientation
        let currentY = Number(values.split("/")[0]); //Number(values.split("/")[1]);
        let rotationOZ = Number(values.split("/")[2]);

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

        //landscape
        let dirY = Math.ceil(currentX *  1.5); // Math.ceil(currentX * 1.5); //portrait
        let dirX = Math.ceil(currentY *  1.5); // Math.ceil(currentY * (-1) * 1.5);
        movementX += dirX;
        movementY += dirY;

        if (isInArea(player) === true) {
            $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
        }
        else {
            scene.physics.velocity = 0;
        }

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
    clearInterval(refreshIntervalId);
}

function displaySceneForFinish() {
    $("#reset").css({ display: 'block' });
    displayPlayerForFinish();
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

function playAgain() {
    sendMessage("reloading game...");
    scene.init();
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

