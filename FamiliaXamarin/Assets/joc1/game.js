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
let score = 0;
let isPlayingAgain = false;
let fail = false;
let level = 1;
let planetNumber;
let wrongPlacesArrayUI = [];
let arrivalPlace;
let player;

let stopHome = {
    x: 350,
    y: 250
}

const colors = [
    {
        planet: 1,
        color: '#DA416B'
    },
    {
        planet: 2,
        color: '#DD3A3F'
    },
    {
        planet: 3,
        color: '#FCD55E'
    },
    {
        planet: 4,
        color: '#458EC5'
    },
    {
        planet: 5,
        color: '#35932E'
    }
    ,
    {
        planet: 6,
        color: '#F39CA5'
    },
    {
        planet: 7,
        color: '#F2932A'
    },
    {
        planet: 8,
        color: '#72B7AE'
    }
];


$.keyframe.define([
    {
        name: 'home-move',
        '0%': {
            'margin-left': 50 + 'px',
            'margin-top': 50 + 'px'
        },
        '100%': {
            'margin-left': stopHome.x + 'px',
            'margin-top': stopHome.y + 'px'
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

    if (isPlayingAgain) {
        score = Number(AndroidJSHandler.getScore());
        level = 1;
    }

    for (let i = 0; i < wrongPlacesArrayUI.length; i++) {
        wrongPlacesArrayUI[i].remove();
    }

    planetNumber = Math.floor(Math.random() * 8) + 1;
    let arrivalColor = colors.filter((x) => { return x.planet == planetNumber; });

    $(".arrival").css({ 'background-color': "'" + arrivalColor[0].color + "'" });

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

           
            $(".ball").css({ 'background-image': 'url("images/planet' + planetNumber + '.png")' });
            $(".player").css({ top: '0px', left: '0px', position: 'absolute', display: 'block', animation: 'none' });

            let centerY = getRandomBtwn(40, 150);
            let centerX = getRandomBtwn(40, 400);

            $(".arrival").css({ top: centerY + 'px', left: centerX + 'px', position: 'absolute' });

            arrivalPlace = {
                width: $(".arrival").outerWidth(),
                height: $(".arrival").outerHeight(),
                center: {
                    x: $(".arrival").offset().left + ($(".arrival").outerWidth() / 2),
                    y: $(".arrival").offset().top + ($(".arrival").outerHeight() / 2)
                }
            };

            player = {
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

           
            if (level > 1) {
                if (score < 10) {
                    wrongPlacesArrayUI = generateWrongPlaces(score, player, arrivalPlace);
                } else {
                    wrongPlacesArrayUI = generateWrongPlaces(10, player, arrivalPlace);
                }
            }


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
        let values = AndroidJSHandler.getXYFromSensor();
        sendMessage("Values came from Android " + values);
        //landscape orientation
        let currentX = Number(values.split("/")[1]); //Number(values.split("/")[0]);  // portrait orientation
        let currentY = Number(values.split("/")[0]); //Number(values.split("/")[1]);
        let rotationOZ = Number(values.split("/")[2]);
        $(".player").parent().css({ position: 'relative' });
        $(".arrival").css({ position: 'absolute' });

        player = {
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


        movementY = player.position.top;
        movementX = player.position.left;

        //landscape
        let dirY = Math.ceil(currentX * scene.physics.gravity); // Math.ceil(currentX * 1.5); //portrait
        let dirX = Math.ceil(currentY * scene.physics.gravity); // Math.ceil(currentY * (-1) * 1.5);
        movementX += dirX;
        movementY += dirY;

        setPlayerToNewPosition(player);

        if (checkForFinish(player, arrivalPlace) === true) {
            $(".player").css({ left: arrivalPlace.center.x - (player.width / 2), top: arrivalPlace.center.y - (player.height / 2), position: 'absolute' });
            $("#nextLevel").css({ display: 'block' });
            isOver = true;
            fail = false;
        }

        if (level >= 2) {
            checkForFinishForWrongPlaces(player);
        }

    } else {

        if (!isPlayingAgain) {
            if (!fail) {
                score++;
            }
        }
        else
            if (!fail) {
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

function checkForFinishForWrongPlaces(player) {

    for (let idx = 0; idx < wrongPlacesArrayUI.length; idx++) {
        let item = {
            width: wrongPlacesArrayUI[idx].outerWidth(),
            height: wrongPlacesArrayUI[idx].outerHeight(),
            center: {
                x: wrongPlacesArrayUI[idx].offset().left + (wrongPlacesArrayUI[idx].outerWidth() / 2),
                y: wrongPlacesArrayUI[idx].offset().top + (wrongPlacesArrayUI[idx].outerHeight() / 2)
            }
        };

        if (checkForFinish(player, item) === true) {
            fail = true;
            $(".player").css({ left: item.center.x - (player.width / 2), top: item.center.y - (player.height / 2), position: 'absolute' });
            $("#nextLevel").css({ display: 'none' });
            isOver = true;
        }
    }
}

function displaySceneForFinish() {
    $("#score").text(score);
    AndroidJSHandler.saveScore(score);
    $(".box_scor").css({ display: 'block' });
    displayPlayerForFinish();
}

function isInArea(player) {
    sendMessage("Window dimension: height " + Number($(window).height()) + " width " + Number($(window).width()));

    if (movementY <= Number($(window).height()) - player.height && movementY >= 0 &&
        movementX <= Number($(window).width()) - player.width && movementX >= 0) {
        return true;
    }

    return false;
}

function isInAreaForX(player) {
    sendMessage("Window dimension: height " + Number($(window).height()) + " width " + Number($(window).width()));

    if (movementX <= Number($(window).width()) - player.width && movementX >= 0) {
        return true;
    }

    return false;
}


function isInAreaForY(player) {
    sendMessage("Window dimension: height " + Number($(window).height()) + " width " + Number($(window).width()));

    if (movementY <= Number($(window).height()) - player.height && movementY >= 0) {
        return true;
    }

    return false;
}


function setPlayerToNewPosition(player) {
    if (isInArea(player) === true) {
        $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
    } else {
        if (isInAreaForX(player) === true) {
            $(".player").css({ left: movementX, position: 'absolute' });
        } else {
            if (isInAreaForY(player) === true) {
                $(".player").css({ top: movementY, position: 'absolute' });
            }
        }
    }
}


function generateWrongPlaces(noOfItems, player, home) {
    let items = [];
    let count = 0;
    let wrongPlaces = colors.filter((x) => { return x.planet != planetNumber; });
    let wrongPlacesCopy = [...wrongPlaces];

    while (count < noOfItems) {

        let centerY = getRandomBtwn(80, Number($(window).height() - home.height - 80));
        let centerX = getRandomBtwn(80, Number($(window).width() - home.width - 80));
        

        if (checkCollision(centerX, centerY, player, home, items) == false) {
            sendMessage("no of items in copy " + wrongPlacesCopy.length);
            count++;
            if (wrongPlacesCopy.length != 0) {

                let randomIndex = Math.floor(Math.random() * wrongPlacesCopy.length);
                sendMessage("RANDOM WP H:" + centerY + " W:" + centerX + " color " + wrongPlacesCopy[randomIndex].color);

                let item = $('<div class="wrong-place"></div>');
                item.css({ left: centerX, top: centerY, 'background-color': "'" + wrongPlacesCopy[randomIndex].color + "'", position: 'absolute', display: 'block' });
                items.push(item);
                $("body").append(item);
               // wrongPlacesCopy = wrongPlacesCopy.filter((x) => { return x.planet != wrongPlacesCopy[randomIndex].planet }); 

            }
        }

    }

    return items;
}


function checkCollision(centerX, centerY, player, home, items) {
    if (checkCollisionWithItem(centerX, centerY, player, home)) {
        return true;
    }
    return false;
}


function getElementForColor(color, items) {
    for (let i = 0; i < items.length; i++) {
        if (items[i].css('background-color') === color) {
            return items[i];
        }
    }
    return null;
}


function checkCollisionWithItem(centerX, centerY, player, item) {
    const playerWidth = player.width + 40;
    const playerHeight = player.height + 40;

    if (centerX - playerWidth <= Math.floor(item.center.x + (item.width)) &&
        centerX + playerWidth >= Math.floor(item.center.x - (item.width)) &&
        centerY - playerHeight <= Math.floor(item.center.y + (item.height)) &&
        centerY + playerHeight >= Math.floor(item.center.y - (item.height))) {
        return true;
    } else {
        return false;
    }

}


function getRandomBtwn(min, max) {
    return Math.floor(Math.random() * (max - min)) + min;
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
    alert(AndroidJSHandler.getFromAndroid());
}

function sendMessage(message) {
    AndroidJSHandler.receiveMessageFromJS(message);
}

$(document).ready(function () {
    $("#hintHowToPlay").css({ display: 'block' });

    scene.init();
});

