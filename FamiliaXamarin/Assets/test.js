const scene = {
    init: '',
    update: '',
    finish: ''
}

let dimensionsString;
let movementX;
let movementY;
let speedX;
let speedY;
let isOver;
let isStarted;
let refreshIntervalId;

$.keyframe.define([
    {
        name: 'ball-move',
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

scene.init = function() {
    isStarted = false;
    isOver = false;

    $("#reset").css({ display: 'none' });
    $(".player").css({ display: 'none' });
    $(".arrival").css({ top: '0px', left: '50px', position: 'absolute' });

    $('.arrival').playKeyframe({
        name: 'ball-move',
        duration: '2s',
        timingFunction: 'ease-out',
        'fill-mode': 'forwards',
        complete: function () {
            $(".player").css({ top: '0px', left: '0px', position: 'absolute', display: 'block', animation: 'none' });
            isStarted = true;
            sendMessage("Animation is completed. Starting the game .... ");
            sendMessage("Loop started..");
            dimensionsString = JSHandler.getScreenDimension();
            movementX = 0;
            movementY = 0;
            speedX = 0;
            speedY = 0;
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
            position: {
                left: $(".player").position().left,
                top: $(".player").position().top
            }
        }

        if (currentX !== 0 && currentY !== 0) {

            if (currentX > 0.3 || currentX < -0.3) {
                movementX = (speedX * currentX) + player.position.left;
            }
            if (currentY < -0.3 || currentY > 0.3) {
                movementY = (speedY * currentY) + player.position.top;
            }
            if (movementY <= Number($(window).height()) - player.height && movementY >= 0 &&
                movementX <= Number($(window).width()) - player.width && movementX >= 0) {
                $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
            }

            sendMessage("moveX " + movementX + " speedX: " + (speedX * currentX));
            sendMessage("moveY " + movementY + " speedY: " + (speedY * currentY));
            sendMessage("Player position top: " + player.position.top + " left: " + player.position.left);

            if (speedX < 10) {
                speedX++;
            }
            if (speedY < 10) {
                speedY++;
            }
        } else {
            speedX = 0;
            speedY = 0;
            sendMessage("not moving");
        }
        if (checkForFinish() === true) {
            isOver = true;
        }
    } else {
        scene.finish();
    }
};

scene.finish = function() {
    sendMessage("finish called");
    $("#reset").css({ display: 'block' });
    displayPlayerForFinish();
    sendMessage("Interval id: " + refreshIntervalId);
    clearInterval(refreshIntervalId);
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

function checkForFinish() {

    const player = {
        width: $(".player").outerWidth(),
        height: $(".player").outerHeight(),
        center: {
            x: $(".player").offset().left + ($(".player").outerWidth() / 2),
            y: $(".player").offset().top + ($(".player").outerHeight() / 2)
        }
    }

    const arrivalPlace = {
        width: $(".arrival").outerWidth(),
        height: $(".arrival").outerHeight(),
        center: {
            x: $(".arrival").offset().left + ($(".arrival").outerWidth() / 2),
            y: $(".arrival").offset().top + ($(".arrival").outerHeight() / 2)
        }
    };

    sendMessage("Center Player XY: " + player.center.x + " / " + player.center.x);
    sendMessage("Center Arrival XY: " + arrivalPlace.center.x + " / " + arrivalPlace.center.y);

    if (Math.floor(player.center.x) < Math.floor(arrivalPlace.center.x + (arrivalPlace.width / 2)) &&
        Math.floor(player.center.x) > Math.floor(arrivalPlace.center.x - (arrivalPlace.width / 2)) &&
        Math.floor(player.center.y < Math.floor(arrivalPlace.center.y + (arrivalPlace.height / 2))) &&
        Math.floor(player.center.y) > Math.floor(arrivalPlace.center.y - (arrivalPlace.height / 2))) {
        sendMessage("Player floor: " + Math.floor(player.center.x) + " / " + Math.floor(player.center.y));
        sendMessage("Arrival X: " + Math.floor(arrivalPlace.center.x) + " / " + Math.floor(arrivalPlace.center.x - arrivalPlace.width));
        sendMessage("Arrivale Y: " + Math.floor(arrivalPlace.center.y) + " / " + Math.floor(arrivalPlace.center.y - arrivalPlace.height));
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

