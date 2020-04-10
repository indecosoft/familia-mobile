class UI {

    constructor() {}

    init() {
        $("#firstPage").css({ display: 'block' });
        $("#secondPage").css({ display: 'none' });
        $("#thirdPage").css({ display: 'none' });
    }

    runStartAnimations() {
        return new Promise((res, rejectirejonFunc) => {

            $("#firstPage").css({ display: 'none' });
            $("#secondPage").css({ display: 'block' });
            $(".text-title").text('Se pare că este ' + data.name + ' !').playKeyframe({
                name: 'showBox',
                duration: '2.5s',
                complete: function() {
                    $(".text-title").playKeyframe({
                        name: 'byeBox',
                        duration: '2s',
                        complete: function() {
                            $(".text-title").text('Tu ce activități faci ' + data.name + ' ?').playKeyframe({
                                name: 'showBox',
                                duration: '2.5s',
                                complete: () => {
                                    $(".text-title").playKeyframe({
                                        name: 'moveUp',
                                        duration: '0.5s',
                                        complete: () => {
                                            console.log("done");
                                            $(".container-body").css({ display: "block" });
                                            res("done");
                                            // ui.generateActivities(data.activities);
                                        }
                                    });
                                }
                            });
                        }
                    });
                }
            });

        });
    }

    generateActivities(activities) {
        console.log(activities.length, activities);
        let x;
        let y;
        for (let i = 1; i <= activities.length; i++) {

            switch (i) {
                case 1:
                    x = 7;
                    y = 25;
                    break;
                case 2:
                    y = 55;
                    break;
                case 3:
                    x = 37;
                    y = 25;
                    break;
                case 4:
                    y = 55;
                    break;
                case 5:
                    x = 67;
                    y = 25;
                    break;
                case 6:
                    y = 55;
                    break;
            }

            let item = $('<div id="item' + i + '" class="selectable-element" onclick=(onItemClicked("item' + i + '"))>' +
                '<div class="text-selectable-element">' + activities[i - 1].name + '</div>' +
                '</div>');

            item.css({
                left: x + '%',
                top: y + '%',
                display: 'block',
                position: 'absolute'
            })

            $(".container-body").append(item);
            uiList.push({
                id: 'item' + i,
                text: activities[i - 1].name,
                jqueryElement: item,
                activity: activities[i - 1]
            })
        }
    }

    toggleSelectedElement(element) {
        if (isHere(element, activities)) {
            $('#' + element).css({
                'background-color': 'grey'
            });
            activities = activities.filter(function(ele) { return ele != element; });
        } else {
            activities.push(element);
            $('#' + element).css({
                'background-color': 'green'
            });
        }
    }

    displayNextButton(condition) {
        if (condition) {
            $(".next").css({ display: 'block' });
        } else {
            $(".next").css({ display: 'none' });
        }
    }

    hideSecondPage() {
        $("#secondPage").css({ display: 'none' });
        $(".container-body").empty();
    }

    displayThirdPage() {
        $(".text-title").text('De ce obiecte ai nevoie ?').playKeyframe({
            name: 'moveUp',
            duration: '1s',
            complete: () => {
                console.log("done");
                $(".container-body").css({ display: "block" });
            }
        });
        $(".next").css({ display: 'none' });
        $("#thirdPage").css({ display: 'block' });
    }


}