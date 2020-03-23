const scene = {
    init: '',
    update: '',
    finish: '',
    refreshIntervalId: ''
}

let children = [];
const idChild = "#child";
const idDashed = "#dashed";

let dashedElements = [];

let solution = [];

scene.init = function () {
    clean();

    $(".box_scor").css({ display: 'none', height: Number($(window).height()) + 10 });

    $(".parent").css({ top: 30, left: 30, position: 'absolute', display: 'block' });

    generateDashedElements(3);
    generateBorderedElements(5);
    createSolution();
    // scene.refreshIntervalId = setInterval(scene.update, 5);
}

scene.update = function () { }

scene.finish = function () {
    // clearInterval(scene.refreshIntervalId);
}

function clean() {
    for (let i = 0; i < children.length; i++) {
        children[i].jqueryElement.remove();
    }

    for (let i = 0; i < dashedElements.length; i++) {
        dashedElements[i].jqueryElement.remove();
    }

    solution = [];


    $(".round-green").css({
        top: 80,
        left: 650,
        position: 'absolute',
        display: 'none'
    });
    $(".round-grey").css({
        top: 120,
        left: 700,
        position: 'absolute',
        display: 'none'
    });
}

function createSolution() {
    let randomNumbers = getArrayWithRandomNumbers(dashedElements.length, children);
    solution = [...new Set(randomNumbers)];
    if (solution.length != dashedElements.length) {
        for (let i = 1; i <= children.length; i++) {
            if (solution.length != dashedElements.length) {
                if (!isHere(i, solution)) {
                    solution.push(i);
                }
            }
        }
    }
    console.log('Solution ', solution);
}

function generateDashedElements(nrOfItems) {
    dashedElements = [];
    let centerX = 100;
    let centerY = 80;
    for (let i = 1; i <= nrOfItems; i++) {
        let item = $('<div id="dashed' + i + '"class="dashed"></div>');
        centerX += 100;

        item.css({
            left: centerX,
            top: centerY,
            display: 'block',
            position: 'absolute'
        })
            .droppable({
                drop: function (event, ui) {
                    ui.helper.data('dropped', true);
                    onDropInside($(this), ui.draggable)
                }
            });

        $(".container").append(item);

        let obj = {
            id: idDashed + i,
            jqueryElement: item,
            occupied: {
                isBusy: false,
                element: ''
            }
        }

        dashedElements.push(obj);
    }
}

function generateBorderedElements(nrOfItems) {
    children = [];
    let centerX = 100;
    let centerY = 300;
    for (let i = 1; i <= nrOfItems; i++) {
        let item = $('<div id="child' + i + '" class="child"></div>');
        centerX += 100;

        item.css({
            left: centerX,
            top: centerY,
            display: 'block',
            position: 'absolute',
            'background-image': 'url("images/pui' + i + '.png")'
        })
            .draggable({
                revert: false,
                start: function (event, ui) {
                    ui.helper.data('dropped', false);
                },
                stop: function (event, ui) {
                    if (ui.helper.data('dropped') == false) {
                        onDropOutside(ui);
                    }
                }
            }).droppable;

        $(".container").append(item);

        let obj = {
            id: idChild + i,
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

        children.push(obj);
    }
}

function getArrayWithRandomNumbers(noOfItems, list) {
    let array = [];
    for (let i = 0; i < noOfItems; i++) {
        array.push(Math.floor(Math.random() * list.length) + 1);
    }
    return array;
}

function isHere(number, list) {
    for (let i = 0; i < list.length; i++) {
        if (number == list[i]) {
            return true;
        }
    }
    return false;
}


function getPosition(uiElement, list) {
    let id = -1;
    for (let i = 0; i < list.length; i++) {
        if (list[i].id === "#" + uiElement.attr('id')) {
            id = i;
        }
    }
    return id;
}

function onDropInside(dashedElement, uiElement) {

    const child = {
        width: uiElement.outerWidth(),
        height: uiElement.outerHeight(),
        center: {
            x: uiElement.offset().left + (uiElement.outerWidth() / 2),
            y: uiElement.offset().top + (uiElement.outerHeight() / 2)
        }
    };

    const dashed = {
        width: dashedElement.outerWidth(),
        height: dashedElement.outerHeight(),
        center: {
            x: dashedElement.offset().left + (dashedElement.outerWidth() / 2),
            y: dashedElement.offset().top + (dashedElement.outerHeight() / 2)
        }
    };

    let isMovedBetweenDashedElements = false;
    const idUiElement = getPosition(uiElement, children);
    if (children[idUiElement].currentPosition.x != children[idUiElement].oldPosition.x &&
        children[idUiElement].currentPosition.y != children[idUiElement].oldPosition.y) {
        isMovedBetweenDashedElements = true;
    }

    const idElement = getPosition(dashedElement, dashedElements);
    setChildToDashedPosition(uiElement, dashed, child, idElement);
    removeStinkyItem(isMovedBetweenDashedElements, idElement, uiElement);

    let details = getDetails();
    if (details.allOccupied) {
        if (details.isWinner) {
            console.log('Winner');
            $(".box_scor").css({ display: 'block' });
        }
        displayInformation(details);
    }

}

function displayInformation(details) {
    $(".round-green").css({
        top: 80,
        left: 750,
        position: 'absolute',
        display: 'block'
    });
    $(".round-grey").css({
        top: 120,
        left: 800,
        position: 'absolute',
        display: 'block'
    });

    $("#textCorrect").text(details.correctPosition);
    $("#textIncorrect").text(details.incorrectPosition);
}

function getDetails() {
    let isWinnner = true;
    let list = [];
    let correctPosition = 0;
    let incorrectPosition = 0;
    let allOccupied = areAllItemsOccupied(list);

    if (allOccupied) {
        console.log('your list', list);
        for (let i = 0; i < solution.length; i++) {
            if (solution[i] != list[i]) {
                isWinnner = false;
                if (isHere(list[i], solution)) {
                    incorrectPosition++;
                }
            } else {
                correctPosition++;
            }
        }
    } else {
        isWinnner = false;
    }

    return {
        allOccupied: allOccupied,
        isWinner: isWinnner,
        correctPosition: correctPosition,
        incorrectPosition: incorrectPosition
    };
}

function areAllItemsOccupied(list) {
    for (let i = 0; i < dashedElements.length; i++) {
        if (dashedElements[i].occupied.element == '') {
            return false;
        }
        let id = dashedElements[i].occupied.element.attr('id')
        list.push(Number(id.substr(id.length - 1)));
    }
    return true;
}

function removeStinkyItem(isMovedBetweenDashedElements, idElement, uiElement) {
    if (isMovedBetweenDashedElements == true) {
        for (let i = 0; i < dashedElements.length; i++) {
            if (i != idElement) {
                if (dashedElements[i].occupied.element == uiElement) {
                    dashedElements[i].occupied.element = '';
                }
            }
        }
    }
}

function setChildToDashedPosition(uiElement, dashed, child, idElement) {
    const error = 7;
    uiElement.css({
        left: dashed.center.x - (child.width / 2) - error,
        top: dashed.center.y - (child.height / 2) - error,
        position: 'absolute'
    });
    updateCurrentPostion(uiElement, child.center.x, child.center.y);
    if (dashedElements[idElement].occupied.element != uiElement) {
        try {
            setElementToOldPosition(dashedElements[idElement].occupied.element,
                getPosition(dashedElements[idElement].occupied.element, children));
        } catch (e) { }
        dashedElements[idElement].occupied.element = uiElement;
    }
}

function onDropOutside(ui) {
    const idElement = getPosition(ui.helper, children);
    if (idElement != -1) {
        updateCurrentPostion(ui.helper,
            children[idElement].oldPosition.x,
            children[idElement].oldPosition.y);

        setElementToOldPosition(ui.helper, idElement);

        for (let i = 0; i < dashedElements.length; i++) {
            try {
                let idx = getPosition(dashedElements[i].occupied.element, children);
                if (children[idx].oldPosition.x == children[idx].currentPosition.x &&
                    children[idx].oldPosition.y == children[idx].currentPosition.y) {
                    dashedElements[i].occupied.element = '';
                }
            } catch (e) { }
        }
    }
}

function setElementToOldPosition(ui, idElement) {
    const child = {
        width: ui.outerWidth(),
        height: ui.outerHeight(),
        center: {
            x: children[idElement].oldPosition.x,
            y: children[idElement].oldPosition.y
        }
    };
    const error = 7;
    ui.css({
        left: child.center.x - (child.width / 2) - error,
        top: child.center.y - (child.height / 2) - error,
        position: 'absolute'
    });
}

function updateCurrentPostion(uiElement, x, y) {
    for (let i = 0; i < children.length; i++) {
        if (children[i].id == "#" + uiElement.attr('id')) {
            children[i].currentPosition.x = x;
            children[i].currentPosition.y = y;
        }
    }
}


function playAgain() {
    console.log('play again');
    scene.init();
}

/*
function getFromAndroid() {
    alert(AndroidJSHandler.getFromAndroid());
}

function sendMessage(message) {
    AndroidJSHandler.receiveMessageFromJS(message);
}
*/
$(document).ready(function () {
    scene.init();
});