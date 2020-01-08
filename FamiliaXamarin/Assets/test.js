const scene = {
    init: '',
    update: '',
    finish: '',

    physics: {
        gravity: 1.5,
        velocity: 0
    }
}

let movementX;
let movementY;
let isOver;
let isStarted;
let refreshIntervalId;
let durationAnimation = 3;
let score;
let isPlayingAgain = false;
let fail = false;
let level = 1;

$.keyframe.define([
    {
        name: 'home-move',
        '0%': {
            'margin-left': 50 + 'px',
            'margin-top': 50 + 'px'
        },
        '100%': {
            'margin-left': 250 + 'px',
            'margin-top': 250 + 'px'
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

    score = Number(JSHandler.getScore());
    // the game will be loaded with the right planet depends on score

    //---level 2
    if (level >= 2) {
        $(".wrong-place1").css({ top: '40px', left: '500px', display: 'block', 'background-color': '#DA416B' });
        $(".arrival").css({ 'background-color': '#339D82' });
        $(".ball").css({ 'background-image': 'url("planet 5.png")' });
    }
    //---level 3

    if (level >= 3) {
        $(".wrong-place2").css({ top: '200px', left: '100px', display: 'block' });

    }


    $(".box_scor").css({ display: 'none', height: Number($(window).height()) + 10 });
    $("#itemsCollected").css({ display: 'none' });
    $(".player").css({ display: 'none' });
    $(".arrival").css({ top: '0px', left: '50px', position: 'absolute' });

    $('.arrival').playKeyframe({
        name: 'home-move',
        duration: durationAnimation + 's',
        timingFunction: 'ease-out',
        'fill-mode': 'forwards',
        complete: function () {
            $(".player").css({ top: '0px', left: '0px', position: 'absolute', display: 'block', animation: 'none' });
            isStarted = true;
            sendMessage("Animation is completed. Starting the game .... ");
            sendMessage("Loop started..");
            refreshIntervalId = setInterval(scene.update, 5);
            $("#hintHowToPlay").css({ display: 'none' });
            durationAnimation = 1;
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
        let dirY = Math.ceil(currentX * scene.physics.gravity); // Math.ceil(currentX * 1.5); //portrait
        let dirX = Math.ceil(currentY * scene.physics.gravity); // Math.ceil(currentY * (-1) * 1.5);
        movementX += dirX;
        movementY += dirY;

        if (isInArea(player) === true) {
            $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
        }

        if (checkForFinish(player, arrivalPlace) === true) {
            $(".player").css({ left: arrivalPlace.center.x, top: arrivalPlace.center.y, position: 'absolute' });
            $("#nextLevel").css({ display: 'block' });
            isOver = true;
            fail = false;
        }

        if (level >= 2) {
            level2(player);
        }

        if (level >= 3) {
            level3(player);
        }

    } else {

        if (!isPlayingAgain) {
            if (!fail) {
                score++;
            }
        } 
        else
        if (!fail ) {
             score++;
        }
        

        scene.finish();
    }
};



scene.finish = function () {
    sendMessage("finish called");
    displaySceneForFinish();
    clearInterval(refreshIntervalId);
}

function level2(player) {
    const wrongPlace1 = {
        width: $(".wrong-place1").outerWidth(),
        height: $(".wrong-place1").outerHeight(),
        center: {
            x: $(".wrong-place1").offset().left + ($(".wrong-place1").outerWidth() / 2),
            y: $(".wrong-place1").offset().top + ($(".wrong-place1").outerHeight() / 2)
        }
    };
    if (checkForFinish(player, wrongPlace1) === true) {
        fail = true;
        $(".player").css({ left: wrongPlace1.center.x, top: wrongPlace1.center.y, position: 'absolute' });
        $("#nextLevel").css({display: 'none'});
        isOver = true;
    }
}


function level3(player) {
    const wrongPlace2 = {
        width: $(".wrong-place2").outerWidth(),
        height: $(".wrong-place2").outerHeight(),
        center: {
            x: $(".wrong-place2").offset().left + ($(".wrong-place2").outerWidth() / 2),
            y: $(".wrong-place2").offset().top + ($(".wrong-place2").outerHeight() / 2)
        }
    };
    if (checkForFinish(player, wrongPlace2) === true) {
        fail = true;
        $(".player").css({ left: wrongPlace2.center.x, top: wrongPlace2.center.y, position: 'absolute' });
        $("#nextLevel").css({ display: 'none' });
        isOver = true;
    }
}

function displaySceneForFinish() {
    $("#score").text(score);
    JSHandler.saveScore(score);
    $(".box_scor").css({ display: 'block' });
    displayPlayerForFinish();
}

function isInArea(player) {
    sendMessage("Window dimension: height " + Number($(window).height()) + " width " + Number($(window).width())  );

    if (movementY <= Number($(window).height()) - player.height && movementY >= 0 &&
        movementX <= Number($(window).width()) - player.width && movementX >= 0) {
        return true;
    }
    return false;
}

function displayPlayerForFinish() {
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
    isPlayingAgain = true;
    sendMessage("reloading game...");
    scene.init();
}

function nextLevel() {
    isPlayingAgain = false;
    level++;
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
    $("#hintHowToPlay").css({ display: 'block' });
   
    scene.init();
});

