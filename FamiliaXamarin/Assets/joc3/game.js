const scene = {
    init: '',
    finish: ''
}

const ui = new UI();
let storedData = new Data();
let data;
let journal;
let uiList = [];
let list = [];

scene.init = function() {
    console.log('init');
    data = storedData.getDataForCurrentTimeOfTheDay(new Date().getHours());
    ui.init();
    uiList = [];
}

function start() {
    console.log("start", data);
    ui.runStartAnimations().then(() => ui.generateElements(data.activities, storedData.activities));
}

// this function will be used when is needed to load other data
function loadFromJSON() {
    var obj = JSON.parse(storage);
    console.log(obj);
    storedData.places = obj.places;
    storedData.objects = obj.objects;
    storedData.activities = obj.activities;
    storedData.dayTimes = obj.dayTimes;
}

function onItemClicked(element) {
    ui.toggleSelectedElement(element);
    ui.displayNextButton(list.length > 0)
}

function isHere(element, array) {
    for (let i = 0; i < array.length; i++) {
        if (element == array[i]) {
            return true;
        }
    }
    return false;
}
let indexObj = 0;

function next(page) {
    switch (page) {
        case 1:
            saveActivities();
            list = [];
            uiList = [];
            ui.showElement("#thirdPage");
            ui.runTranslateTextAnimation('..în continuare trebuie să selectați activitățile pe care le faceți ' + data.name + '..').then(() => {
                next(2);
            })
            break;
        case 2:
            if (list.length != 0) {
                addObjectsToJournal(list, indexObj - 1);
            }
            if (journal.selectedActivities.length > indexObj) {
                ui.clearContainerBody();
                ui.displayThirdPage(journal.selectedActivities[indexObj].name).then(() => {
                    list = [];
                    uiList = [];
                    ui.generateElements(journal.selectedActivities[indexObj].objects, storedData.objects);
                    indexObj++;
                });
            } else {
                console.log('done with objects');
                list = [];
                uiList = [];
                indexObj = 0;
                ui.clearThirdPage();
                ui.clearContainerBody();
                ui.showElement("#fourthPage");
                ui.hideElement(".text-primary");
                ui.runTranslateTextAnimation('.. în continuare trebuie să selectați unde vă desfășurați activitatea ..').then(() => {
                    next(3);
                })
            }
            break;
        case 3:
            if (list.length != 0) {
                addPlacesToJournal(list, indexObj - 1);
            }
            if (journal.selectedActivities.length > indexObj) {
                ui.clearContainerBody();
                ui.displayFourthPage(journal.selectedActivities[indexObj].name).then(() => {
                    list = [];
                    uiList = [];
                    ui.generateElements(journal.selectedActivities[indexObj].places, storedData.places);
                    indexObj++;
                });
            } else {
                console.log('done with places');
                list = [];
                uiList = [];
                indexObj = 0;
                ui.clearFourthPage();
                next(4);
            }
            break;
        case 4:
            console.log('final journal', journal);

            let countActivities = 0;
            let countObjects = 0;
            let countPlaces = 0;
            let totalObjects = 0;
            let totalPlaces = 0;

            for (let i = 0; i < journal.activities.length; i++) {
                for (let j = 0; j < journal.selectedActivities.length; j++) {
                    if (journal.activities[i].name == journal.selectedActivities[j].name) {
                        countActivities++;
                    }
                }
            }

            journal.selectedActivities.forEach(activity => {

                totalObjects += activity.objects.length;
                totalPlaces += activity.places.length;

                activity.objects.forEach(object => {
                    activity.selectedObjects.forEach(selectedObject => {
                        if (object.name == selectedObject.name) {
                            countObjects++;
                        }
                    });
                });
                activity.places.forEach(place => {
                    activity.selectedPlaces.forEach(selectedPlace => {
                        if (place.name == selectedPlace.name) {
                            countPlaces++;
                        }
                    });
                });
            });

            ui.displayFifthPage("Activitati: " + countActivities + "/" + journal.activities.length,
                "Obiecte: " + countObjects + "/" + totalObjects,
                "Locuri: " + countPlaces + "/" + totalPlaces);

            break;
    }
}

function saveActivities() {
    let myActivities = getActivities();
    journal = {
        activities: [...data.activities],
        selectedActivities: myActivities
    };
    ui.clearSecondPage();
}

function getActivities() {
    let myActivities = [];
    for (let i = 0; i < list.length; i++) {
        for (let j = 0; j < uiList.length; j++) {
            if (list[i] == uiList[j].id) {
                let activity = {...uiList[j].element };
                activity.selectedObjects = [];
                activity.selectedPlaces = [];
                myActivities.push(activity);
            }
        }
    }
    return myActivities;
}

function getSelectedItems(list) {
    let myObjects = [];
    for (let i = 0; i < list.length; i++) {
        for (let j = 0; j < uiList.length; j++) {
            if (list[i] == uiList[j].id) {
                myObjects.push({...uiList[j].element });
            }
        }
    }
    return myObjects;
}

function addObjectsToJournal(list, indexObj) {
    let selectedObjects = getSelectedItems(list);
    journal.selectedActivities[indexObj].selectedObjects = [...selectedObjects];
}

function addPlacesToJournal(list, indexObj) {
    let selectedPlaces = getSelectedItems(list);
    journal.selectedActivities[indexObj].selectedPlaces = [...selectedPlaces];
}


function playAgain() {
    ui.clearFifthPage();
    ui.recreateFirstPage();
    ui.createPages();
    scene.init();
}

$(document).ready(function() {
    scene.init();
});