class Data {

    objects = [
        new Object("O1"),
        new Object("O2"),
        new Object("O3"),
        new Object("O4"),
        new Object("O5"),
        new Object("O6"),
        new Object("O7"),
        new Object("O8"),
    ];

    places = [
        new Place("P1"),
        new Place("P2"),
        new Place("P3"),
    ];

    activities = [
        new Activity("Ma spal pe fata", [this.objects[0], this.objects[2], this.objects[3]], [this.places[0], this.places[1]]),
        new Activity("Mananc", [this.objects[1], this.objects[2]], [this.places[0]]),
        new Activity("Spal vase", [this.objects[1], this.objects[2], this.objects[3]], [this.places[1], this.places[2]]),
        new Activity("Ma culc", [this.objects[4], this.objects[6], this.objects[3]], [this.places[1], this.places[2]]),
        new Activity("Deschid geamul", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),
        new Activity("Ma spal pe dinti", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),
    ];

    dayTimes = [
        new DayTime("dimineață", [this.activities[0], this.activities[1]]),
        new DayTime("la amiază", [this.activities[2], this.activities[0], this.activities[1], this.activities[3], this.activities[4], this.activities[5]])
    ]

    getDataForCurrentTimeOfTheDay(hour) {
        if (hour >= 4 && hour < 12) {
            return this.dayTimes[0];
        }

        if (hour >= 12) {
            return this.dayTimes[1];
        }

        return null;
    }

}