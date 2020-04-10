class Activity {
    name;
    objects = [];
    places = [];
    constructor(name, objects, places) {
        this.name = name;
        this.copy(objects, this.objects);
        this.copy(places, this.places)
    }

    copy(fromList, toList) {
        fromList.forEach(element => {
            toList.push(element);
        });
    }
}