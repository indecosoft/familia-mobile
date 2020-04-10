const scene = {
    init: '',
    finish: ''
}

const ui = new UI();
let storedData = new Data();
let data;
let journal;
let uiList;
let activities = [];

scene.init = function() {
    console.log('init');
    data = storedData.getDataForCurrentTimeOfTheDay(new Date().getHours()); //could be null
    ui.init();
    uiList = [];
}

function start() {
    console.log("start");
    ui.runStartAnimations().then(() => ui.generateActivities(data.activities));
}


function onItemClicked(element) {
    console.log(element);
    ui.toggleSelectedElement(element);
    ui.displayNextButton(activities.length > 0)
}

function isHere(element, array) {
    for (let i = 0; i < array.length; i++) {
        if (element == array[i]) {
            return true;
        }
    }
    return false;
}

function next(page) {
    console.log(page);
    switch (page) {
        case 1:
            saveActivities();
            ui.displayThirdPage();
            break;
        case 2:
            break;
    }
}

function saveActivities() {
    let myActivities = getActivities();
    journal = {
        activities: myActivities
    };
    console.log(journal);
    ui.hideSecondPage();
}

function getActivities() {
    let myActivities = [];
    for (let i = 0; i < activities.length; i++) {
        for (let j = 0; j < uiList.length; j++) {
            if (activities[i] == uiList[j].id) {
                let activity = uiList[j].activity;
                activity.objects = [];
                activity.places = [];
                myActivities.push(uiList[j].activity);
            }
        }
    }
    return myActivities;
}


$(document).ready(function() {
    scene.init();
});