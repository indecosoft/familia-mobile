class DayTime {
    name;
    activities = [];
    constructor(name, activities) {
        this.name = name;
        this.copyActivities(activities)
    }

    copyActivities(list) {
        list.forEach(element => {
            this.activities.push(element);
        });
    }

    setActivities(activities) {
        this.copyActivities(activities)
    }
}