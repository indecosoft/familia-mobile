class UI {

    constructor() {}

    init() {

        $(".text-title").text('Începeți jurnalul de activități!');
        $("#firstPage").css({ display: 'block' });
        $("#secondPage").css({ display: 'none' });
        $("#thirdPage").css({ display: 'none' });
        $("#fourthPage").css({ display: 'none' });
        $("#fifthPage").css({ display: 'none' });

        //recreate first & second page from scratch
    }

    runStartAnimations() {
        return new Promise((res, rej) => {
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
                            $(".text-title").text('Selectează activități pe care le faci ' + data.name + '.').playKeyframe({
                                name: 'showBox',
                                duration: '2.5s',
                                complete: () => {
                                    $(".text-title").playKeyframe({
                                        name: 'moveUp',
                                        duration: '0.5s',
                                        complete: () => {
                                            $(".container-body").css({ display: "block" });
                                            res("done");
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

    generateElements(origList, otherData) {
        let list = [];
        if (origList.length < 6) {
            let set = this.getMoreElements(origList, otherData);
            list = [...set];
        } else {
            list = [...origList]
        }
        this.generateSelectableElements(list);
    }

    generateSelectableElements(list) {
        let x;
        let y;
        this.shuffleList(list);
        for (let i = 1; i <= list.length; i++) {
            switch (i) {
                case 1:
                    x = 7;
                    y = 30;
                    break;
                case 2:
                    y = 55;
                    break;
                case 3:
                    x = 37;
                    y = 30;
                    break;
                case 4:
                    y = 55;
                    break;
                case 5:
                    x = 67;
                    y = 30;
                    break;
                case 6:
                    y = 55;
                    break;
            }
            let item = $('<div id="item' + i + '" class="selectable-element" onclick=(onItemClicked("item' + i + '"))>' +
                '<div class="text-selectable-element">' + list[i - 1].name + '</div>' +
                '</div>');


            //for image
            // let item = $('<div id="item' + i + '" class="selectable-element-test" onclick=(onItemClicked("item' + i + '"))>' +
            //     '      <img style="width: 100%; height: 100%" src="images/Apus.png" />' +
            //     '</div>');

            item.css({
                left: x + '%',
                top: y + '%',
                display: 'block',
                position: 'absolute'
            });
            $(".container-body").append(item);
            uiList.push({
                id: 'item' + i,
                text: list[i - 1].name,
                jqueryElement: item,
                element: list[i - 1]
            });
        }
    }

    shuffleList(array) {
        for (let i = array.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [array[i], array[j]] = [array[j], array[i]];
        }
    }

    getMoreElements(list, otherData) {
        let set = new Set([...list]);
        let set2 = new Set();

        otherData.forEach((obj) => {
            set2.add(obj);
        });

        set2.forEach(e => {
            if (set.size < 6) {
                set.add(e);
            }
        });
        return set;
    }

    toggleSelectedElement(element) {
        if (isHere(element, list)) {
            $('#' + element).css({
                'background-color': 'rgb(58, 58, 61)'
            });
            $('#' + element).children(".text-selectable-element").css({
                'color': 'rgb(222, 222, 227)'
            });

            list = list.filter(function(el) { return el != element; });
        } else {
            //select it
            list.push(element);
            $('#' + element).css({
                'background-color': 'rgb(0, 250, 250)'
            });
            $('#' + element).children(".text-selectable-element").css({
                'color': 'rgb(7, 7, 7)'
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
        $("#secondPage").empty();
        $(".container-body").empty();
    }

    displayThirdPage(activityName) {
        return new Promise((res, rej) => {

            $(".text-primary").text('"' + activityName + '"');
            $(".text-title").text('Selectați cu ce vă desfășurați activitatea ');
            $(".container-body").css({ display: "block" });
            res('done');

            $(".container-body").append('<div class="next" onclick="next(2)">' +
                ' <div class="text-selectable-element" style="margin-top: 10%"> Next </div> </div>')
            $(".next").css({ display: 'none' });
            $("#thirdPage").css({ display: 'block' });
        })
    }

    displayFourthPage(activityName) {
        return new Promise((res, rej) => {
            $(".text-primary").text('"' + activityName + '"')
            $(".text-title").text('Selectați unde vă desfășurați activitatea');
            $(".container-body").css({ display: "block" });

            res('done');
            $(".container-body").append('<div class="next" onclick="next(3)">' +
                ' <div class="text-selectable-element" style="margin-top: 10%"> Next </div> </div>')
            $(".next").css({ display: 'none' });
            $("#fourthPage").css({ display: 'block' });
        })
    }

    displayFifthPage(textActivities, textObjects, textPlaces) {

        $("#textActivities").text(textActivities);
        $("#textObjects").text(textObjects);
        $("#textPlaces").text(textPlaces);

        $("#fifthPage").css({ display: 'block' });
    }

    clearContainerBody() {
        $(".container-body").empty();
    }

    clearThirdPage() {
        $("#thirdPage").empty();
    }

    clearFourthPage() {
        $("#fourthPage").empty();
    }

    clearFifthPage() {
        $("#fifthPage").empty();

    }

}