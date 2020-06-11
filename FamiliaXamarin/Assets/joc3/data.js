class Data {

    objects = [
        //generic
        // new Object("O1"),
        // new Object("O2"),
        // new Object("O3"),
        // new Object("O4"),
        // new Object("O5"),
        // new Object("O6"),
        // new Object("O7"),
        // new Object("O8"),

        new Object("pieptăn"), //0
        new Object("sacoșă"), //1
        new Object("pâine"), //2
        new Object("aragaz oprit"), //3
        new Object("aragaz pornit"), //4
        new Object("prosop"), //5
        new Object("săpun"), //6
        new Object("periuță de dinți"), //7
        new Object("tacâmuri"), //8
        new Object("telecomandă"), //9
        new Object("telefon"), //10
        new Object("bani"), //11
        new Object("ochelari"), //12
        new Object("televizor"), //13
        new Object("radio") //14
    ];

    places = [
        //generic
        // new Place("P1"),
        // new Place("P2"),
        // new Place("P3"),
        // new Place("P4"),
        // new Place("P5"),
        // new Place("P6"),
        // new Place("P7"),
        // new Place("P8"),

        new Place("baie"), //0
        new Place("bucătărie"), //1
        new Place("sufragerie"), //2
        new Place("hol"), //3
        new Place("stomatologie"), //4
        new Place("magazin") //5

    ];

    activities = [
        //generic
        // new Activity("A1", [this.objects[0], this.objects[2], this.objects[3]], [this.places[0], this.places[1], this.places[2], this.places[3], this.places[4]]),
        // new Activity("A2", [this.objects[1], this.objects[2]], [this.places[5]]),
        // new Activity("A3", [this.objects[1], this.objects[2], this.objects[3]], [this.places[6]]),
        // new Activity("A4", [this.objects[4], this.objects[6], this.objects[3]], [this.places[7], this.places[2], this.places[5], this.places[6]]),
        // new Activity("A5", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),
        // new Activity("A6", [this.objects[5], this.objects[2], this.objects[7]], [this.places[1], this.places[2]]),

        //dimineata
        new Activity("mic dejun", [this.objects[2], this.objects[3], this.objects[4], this.objects[8]], [this.places[1]]),
        new Activity("spălat față", [this.objects[5]], [this.places[0]]),
        new Activity("spălat dinți", [this.objects[7]], [this.places[0]]),
        new Activity("cumpărături", [this.objects[1], this.objects[11]], [this.places[5]]),

        //la amiaza
        new Activity("prânz", [this.objects[2], this.objects[3], this.objects[4], this.objects[8]], [this.places[1]]),
        new Activity("ascultat muzică", [this.objects[14], this.objects[13]], [this.places[2]]),

        //seara
        new Activity("cina", [this.objects[2], this.objects[3], this.objects[4], this.objects[8]], [this.places[1]]),
        new Activity("privit TV", [this.objects[9], this.objects[13]], [this.places[2]])
    ];

    dayTimes = [
        new DayTime("dimineață", [this.activities[0], this.activities[1], this.activities[2], this.activities[3]]),
        new DayTime("mijlocul zilei", [this.activities[4], this.activities[5], this.activities[7]]),
        new DayTime("seară", [this.activities[6], this.activities[7]])
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