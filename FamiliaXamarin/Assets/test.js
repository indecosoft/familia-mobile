var dimensionsString;
var movementX;
var movementY;
var speedX;
var speedY;
var isOver;
var refreshIntervalId;

function initGame() {
    $("#reset").css({ display: 'none' });

    $.keyframe.define([
        {
            name: 'ball-move',
            '0%': {
                'margin-left': 50 + 'px',
                'margin-top': 0 + 'px'
            },
            '50%': {
                'margin-left': 50 + 'px',
                'margin-top': 100 + 'px'
            },
            '100%': {
                'margin-left': 50 + 'px',
                'margin-top': 200 + 'px'
            }
        }
    ]);

    $('.arrival').playKeyframe({
        name: 'ball-move',
        duration: '2s',
        timingFunction: 'ease-out',
        'fill-mode': 'forwards'
    });

    dimensionsString = JSHandler.getScreenDimension();
    movementX = 0;
    movementY = 0;
    speedX = 0;
    speedY = 0;
    isOver = false;
    refreshIntervalId = 'none';
    sendMessage("game loaded");
}

var loop = function () {
    if (isOver == false) {
        var values = JSHandler.getXYFromGyro();
        var currentX = Number(values.split("/")[0]);
        var currentY = Number(values.split("/")[1]);

        $(".player").parent().css({ position: 'relative' });
        $(".arrival").css({ top: '200px', left: '50px', position: 'absolute' });

        if (currentX !== 0 && currentY !== 0) {

            if (currentX > 0.3 || currentX < -0.3) {
                movementX = (speedX * currentX) + $(".player").position().left;
            }

            if (currentY < -0.3 || currentY > 0.3) {
                movementY = (speedY * currentY) + $(".player").position().top;
            }

            if (movementY <= Number($(window).height()) - 80 &&
                movementY >= 0 &&
                movementX <= Number($(window).width()) - 80 &&
                movementX >= 0) {
                $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
            }

            sendMessage("moveX " + movementX + " speedX: " + (speedX * currentX));
            sendMessage("moveY " + movementY + " speedY: " + (speedY * currentY));
            sendMessage("Player position top: " + $(".player").position().top + " left: " + $(".player").position().left);

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
        $("#reset").css({ display: 'block' });
        alert("YOU WON!");
        sendMessage("Interval id: " + refreshIntervalId);
        clearInterval(refreshIntervalId);
    }
};


function checkForFinish() {
    var offset = $(".player").offset();
    var playerWidth = $(".player").outerWidth();
    var playerHeight = $(".player").outerHeight();
    var playerCenterX = offset.left + playerWidth / 2;
    var playerCenterY = offset.top + playerHeight / 2;

    var offsetA = $(".arrival").offset();
    var arrivalWidth = $(".arrival").outerWidth();
    var arrivalHeight = $(".arrival").outerHeight();
    var arrivalCenterX = offsetA.left + arrivalWidth / 2;
    var arrivalCenterY = offsetA.top + arrivalHeight / 2;

    sendMessage("Center Player XY: " + playerCenterX + " / " + playerCenterY);
    sendMessage("Center Arrival XY: " + arrivalCenterX + " / " + arrivalCenterY);

    if (Math.floor(playerCenterX) === Math.floor(arrivalCenterX) &&
        Math.floor(playerCenterY) === Math.floor(arrivalCenterY)) {
        return true;
    } else {
        return false;
    }
}

function playAgain() {
    sendMessage("reloading game...");
    isOver = false;
    initGame();
    refreshIntervalId = setInterval(loop, 10);
}

function getFromAndroid() {
    var myVar = JSHandler.getFromAndroid();
    alert(myVar);
}

function sendMessage(message) {
    JSHandler.receiveMessageFromJS(message);
}

$(document).ready(function () {
    initGame();
    refreshIntervalId = setInterval(loop, 10);
});

