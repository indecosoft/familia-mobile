
function getFromAndroid() {
    var myVar = JSHandler.getFromAndroid();
    alert(myVar);
}

function sendMessage(message) {
    JSHandler.receiveMessageFromJS(message);
}

//SET SCENE 

var dimensionsString = JSHandler.getScreenDimension();
var screen = {
    width: Number(dimensionsString.split("/")[0]),
    height: Number(dimensionsString.split("/")[1])
};

var movementX = 0;
var movementY = 0;
var speedX = 0;
var speedY = 0;

var updateScreen = function () {

    var values = JSHandler.getXYFromGyro();
    var currentX = Number(values.split("/")[0]);
    var currentY = Number(values.split("/")[1]);



    $(".player").parent().css({ position: 'relative' });
//    $(".player").css({ position: 'relative' });



    if (currentX !== 0 && currentY !== 0) {

        if (currentX > 0.3 || currentX < -0.3) {
            movementX = (speedX * currentX) + $(".player").position().left;
        }

        if (currentY < -0.3 || currentY > 0.3) {
            movementY = (speedY * currentY)+ $(".player").position().top;
        }
 
        if (movementY <= Number($(window).height()) -80 && movementY >= 0 && movementX <= Number($(window).width()) - 80 && movementX >= 0) {
            $(".player").css({ left: movementX, top: movementY, position: 'absolute' });
        }

        var offset = $(".player").offset();
        var playerWidth = $(".player").outerWidth();
        var playerHeight = $(".player").outerHeight();
        var playerCenterX = offset.left + playerWidth / 2;
        var playerCenterY = offset.top + playerHeight / 2;
        sendMessage("Center Player XY: " + playerCenterX + " / " + playerCenterY);


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
    
    //    alert("from android: " + dimensionsString + " from window: " + $(window).width() + "/" + $(window).height() + " player dimension" + $(".player").width() + "/" + $(".player").height());
};

setInterval(updateScreen, 10);

