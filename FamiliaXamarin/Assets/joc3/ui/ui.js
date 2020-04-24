class UI {

    constructor() {}

    init() {
        $(".text-title").text('Începeți jurnalul de activități!');
        $("#firstPage").css({ display: 'block' });
        $("#secondPage").css({ display: 'none' });
        $("#thirdPage").css({ display: 'none' });
        $("#fourthPage").css({ display: 'none' });
        $("#fifthPage").css({ display: 'none' });
    }

    runStartAnimations() {
        return new Promise((res, rej) => {
            $("#firstPage").css({ display: 'none' });
            $("#secondPage").css({ display: 'block' });
            $(".text-title").text('Este ' + data.name + ' !').playKeyframe({
                name: 'showBox',
                duration: '2s',
                complete: function() {
                    $(".text-title").playKeyframe({
                        name: 'byeBox',
                        duration: '2s',
                        complete: function() {
                            $(".text-title").text('Trebuie să selectați activități pe care le faceți ' + data.name + '.')
                                .playKeyframe({
                                    name: 'showBox',
                                    duration: '5s',
                                    complete: () => {
                                        $(".text-title").playKeyframe({
                                            name: 'byeBox',
                                            duration: '3s',
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
                    y = 18;
                    break;
                case 2:
                    y = 47;
                    break;
                case 3:
                    x = 37;
                    y = 18;
                    break;
                case 4:
                    y = 47;
                    break;
                case 5:
                    x = 67;
                    y = 18;
                    break;
                case 6:
                    y = 47;
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

    clearSecondPage() {
        $("#secondPage").empty();
        $(".container-body").empty();
    }

    showElement(element) {
        $(element).css({ display: 'block' });
    }

    hideElement(element) {
        $(element).css({ display: 'none' });
    }

    runTranslateTextAnimation(text) {
        return new Promise((res, rej) => {
            $(".text-title").text(text)
                .playKeyframe({
                    name: 'showBox',
                    duration: '5s',
                    complete: function() {
                        $(".text-title").playKeyframe({
                            name: 'byeBox',
                            duration: '3s',
                            complete: () => {
                                res('done');
                            }
                        });
                    }
                });
        });
    }

    displayThirdPage(activityName) {
        return new Promise((res, rej) => {
            $(".text-primary").text(activityName).css({ display: 'block' });
            $(".container-body").css({ display: "block" });
            $(".container-body").append('<div class="next" onclick="next(2)">' +
                ' <div class="text-selectable-element" style="margin-top: 10%"> Continuă </div> </div>')
            $(".next").css({ display: 'none' });
            res('done');
        })
    }

    displayFourthPage(activityName) {
        return new Promise((res, rej) => {
            $(".text-primary").text(activityName).css({ display: 'block' });
            $(".text-title").text('Selectați unde vă desfășurați activitatea');
            $(".container-body").css({ display: "block" });
            $(".container-body").append('<div class="next" onclick="next(3)">' +
                ' <div class="text-selectable-element" style="margin-top: 10%"> Continuă </div> </div>')
            $(".next").css({ display: 'none' });
            res('done');
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

    createSecondPage() {
        $("#secondPage").append(
            '<div class="container-text-title">' +
            ' <div class="text-title">' +
            ' Se pare ca este dimineata!' +
            ' </div>' +
            '  </div>' +
            '<div class="container-body">' +
            '  <div class="next" onclick="next(1)">' +
            '<div class="text-selectable-element" style="margin-top: 10%">' +
            '   Continuă' +
            '</div>' +
            '</div>' +
            '</div>'
        )
    }


    createThirdPage() {
        $("#thirdPage").append(
            '<div class="container-text-title">' +
            '<div class="text-primary"></div>' +
            '<div class="text-title"></div>' +
            '</div>' +
            '<div class="container-body">' +
            '<div class="next" onclick="next(2)">' +
            '<div class="text-selectable-element" style="margin-top: 10%">' +
            'Continuă' +
            '</div>' +
            '</div>' +
            '</div>'
        )
    }

    createFourthPage() {
        $("#fourthPage").append(
            '<div class="container-text-title">' +
            '<div class="text-primary"></div>' +
            '<div class="text-title"></div>' +
            '</div>' +
            '<div class="container-body">' +
            '<div class="next" onclick="next(3)">' +
            '<div class="text-selectable-element" style="margin-top: 10%">' +
            'Continuă' +
            '</div>' +
            '</div>' +
            '</div>'
        )
    }

    createFifthPage() {
        $("#fifthPage").append(
            '<div class="container-text-title">' +
            '<div id="textActivities" class="text-primary"></div>' +
            '<div id="textObjects" class="text-primary"> </div>' +
            '<div id="textPlaces" class="text-primary"></div>' +
            '<button class="button-start" onclick="playAgain()"> Din nou </button>' +
            '</div>'
        )
    }

    recreateFirstPage() {
        $("#firstPage").empty();
        $("#firstPage").append(
            '<div class="container-text-title">' +
            '<div class="text-title">' +
            'Începeți jurnalul de activități!' +
            '</div>' +
            '<button class="button-start" onclick="start()">START</button>' +
            '</div>'
        )
    }

    createPages() {
        this.createSecondPage();
        this.createThirdPage();
        this.createFourthPage();
        this.createFifthPage();
    }

}