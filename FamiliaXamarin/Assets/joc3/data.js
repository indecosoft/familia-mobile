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
        // test
        new Object("pieptan"), //8
        new Object("sacosa"), //9
        new Object("paine"), //10
        new Object("aragaz oprit"), //11
        new Object("aragaz pornit"), //12
        new Object("prosop"), //13
        new Object("sapun"), //14
        new Object("periuta de dinti"), //15
        new Object("tacamuri"), //16
        new Object("telecomanda"), //17
        new Object("telefon"), //18
        new Object("bani"), //19
        new Object("ochelari") //20
    ];

    places = [
        new Place("P1"),
        new Place("P2"),
        new Place("P3"),
        new Place("P4"),
        new Place("P5"),
        new Place("P6"),
        new Place("P7"),
        new Place("P8"),
        //test
        new Place("baie"), //8
        new Place("bucatarie"), //9
        new Place("sufragerie"), //10
        new Place("hol"), //11
        new Place("stomatologie"), //12
        new Place("magazin") //13

    ];

    activities = [
        new Activity("A1", [this.objects[0], this.objects[2], this.objects[3]], [this.places[0], this.places[1], this.places[2], this.places[3], this.places[4]]),
        new Activity("A2", [this.objects[1], this.objects[2]], [this.places[5]]),
        new Activity("A3", [this.objects[1], this.objects[2], this.objects[3]], [this.places[6]]),
        new Activity("A4", [this.objects[4], this.objects[6], this.objects[3]], [this.places[7], this.places[2], this.places[5], this.places[6]]),
        new Activity("A5", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),
        new Activity("A6", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),
        new Activity("mic dejun", [this.objects[10], this.objects[11], this.objects[12], this.objects[16]], [this.places[9]]),
        new Activity("spalat fata", [this.objects[13], this.objects[14]], [this.places[8]]),
        new Activity("spalat dinti", [this.objects[15]], [this.places[8]]),
        new Activity("cumparaturi", [this.objects[9], this.objects[19]], [this.places[13]]),
    ];

    dayTimes = [
        // new DayTime("dimineață", [this.activities[0], this.activities[1], this.activities[2]]),
        new DayTime("dimineață", [this.activities[6], this.activities[7], this.activities[8], this.activities[9]]),
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