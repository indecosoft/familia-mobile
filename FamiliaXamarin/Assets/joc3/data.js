class Data {

    objects = [
        new Object("O1"),
        new Object("O2"),
        new Object("O3"),
        new Object("O4"),
        new Object("O5"),
        new Object("O6"),
        new Object("O7"),
        new Object("O8")
    ];

    places = [
        new Place("P1"),
        new Place("P2"),
        new Place("P3"),
        new Place("P4"),
        new Place("P5"),
        new Place("P6"),
        new Place("P7"),
        new Place("P8")
    ];

    activities = [
        new Activity("A1", [this.objects[0], this.objects[2], this.objects[3]], [this.places[0], this.places[1], this.places[2], this.places[3], this.places[4]]),
        new Activity("A2", [this.objects[1], this.objects[2]], [this.places[5]]),
        new Activity("A3", [this.objects[1], this.objects[2], this.objects[3]], [this.places[6]]),
        new Activity("A4", [this.objects[4], this.objects[6], this.objects[3]], [this.places[7], this.places[2], this.places[5], this.places[6]]),
        new Activity("A5", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),
        new Activity("A6", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]])
    ];

    dayTimes = [
        new DayTime("dimineață", [this.activities[0], this.activities[1], this.activities[2]]),
        new DayTime("la amiază", [this.activities[2], this.activities[5], this.activities[4]]),
        new DayTime("seara", [this.activities[3]])
    ]

    getDataForCurrentTimeOfTheDay(hour) {
        if (hour >= 4 && hour < 12) {
            return this.dayTimes[0];
        }

        if (hour >= 12 && hour < 18) {
            return this.dayTimes[1];
        }

        if (hour >= 18) {
            return this.dayTimes[2];
        }

        return null;
    }

}