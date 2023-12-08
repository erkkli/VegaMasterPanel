/* globals $,jQuery */
$(document).on({
    ajaxStart: function () { showProgressBar(); },
    //ajaxStop: function () { hideProgressBar(); },
    ajaxError: function (event, request, settings) {
        araadHandleAjaxError(request, event, settings);
    },
    ajaxComplete: function () { hideProgressBar(); }
});
function araadHandleAjaxError(request, event, settings) {
    var modal =
        '<div id="araadAjaxErrorModal" class="modal fade" role="dialog">' +
        '<div class="modal-dialog">' +
        '<!-- Modal content-->' +
        '<div class="modal-content">' +
        '<div class="modal-header alert alert-danger">' +
        '<button type="button" class="close" data-dismiss="modal">&times;</button>' +
        '<h4 class="modal-title"></h4>' +
        '</div>' +
        '<div class="modal-body"></div>' +
        '<div class="modal-footer"></div>' +
        '</div>' +
        '</div>';
    var modalBaslik = 'İşlem Yapılırken hata oluştu.';
    var hataMesaji = '';
    var $modal = $(modal);
    var footer = $modal.find('div.modal-footer');
    var jsonResponse = null;

    try {
        jsonResponse = (JSON.parse(request.responseText));
    } catch (e) { };

    if (jsonResponse) {
        hataMesaji = jsonResponse.HataMesaji;
        modalBaslik = jsonResponse.HataBasligi;

        for (var i = 0; i < jsonResponse.Buttons.length; i++) {
            var newElement = $(jsonResponse.Buttons[i]);
            newElement.appendTo(footer);
        }
    }
    else {
        hataMesaji = 'Hata :' + request.statusText + "<br/>Hata Kodu :" + request.status;
        var newElement = $('<button type="button" class="btn btn-default" data-dismiss="modal">Tamam</button>');
        newElement.appendTo(footer);
    }

    $modal.find('h4.modal-title').html(modalBaslik);
    $modal.find('div.modal-body').html(hataMesaji);

    $modal.modal({ backdrop: 'static' });
    $modal.modal('show');

    $($modal).on('hide.bs.modal', function (e) {
        hideProgressBar();
    })

    console.info("---------Log Hata Başlangıç---------");
    console.info("request", request);
    console.info("event", event);
    console.info("settings", settings);
    console.info("---------Log Hata Bitiş---------");
}

function toaster(title, message, isSuccess) {

    $.smallBox({
        title: title,
        content: message,
        color: isSuccess == true ? "green" : "red",
        timeout: 5000,
        icon: isSuccess == true ? "fa fa-check" : "fa fa-close"
    });
}

//loading panel modal"ini açar
function showProgressBar() {
    $("#modalPleaseWait").modal("show");

}
//loading panel modal"ini kapar
function hideProgressBar() {
    $("#modalPleaseWait").modal("hide");
}

$(function () {
    nullableCheckBox();
    registerCustomControls();
    callDatePicker();
    araadFileUpload();
    callSolPartialToolTips();
    gridRowDoubleClickInit();
    modalLinkClickInit();
    ajaxPanelInit();
    CopyToClipBoardInit();
    ImagesSliderInit();
    PhoneNumberInputInit();
});

function ImagesSliderInit() {
    $(document)
        .on("click", ".images-slider-container", function () {
            var target = event.target;
            if (!$(target).is('img,.images-slider-btn')) {
                return;
            }
            var imagesList = [];
            var continer = $(target).closest('.images-slider-container');
            continer.find("img").each(function (i, item) {
                var url = $(item).attr("src");
                var text = $(item).attr("title");
                imagesList.push({ 'text': text, 'url': url });
            })

            continer.find("a").each(function (i, item) {
                var url = $(item).attr("href");
                var text = $(item).html();
                if (url && url.indexOf('/Common/Download') > -1) {
                    imagesList.push({ 'text': text, 'url': url });
                }
            })

            var sliderModal = $('#slideModal');
            if (sliderModal.length == 0) {
                var sliderHtml =
                    '<div class="modal fade" id="slideModal" tabindex="-1" role="dialog" aria-labelledby="modalWarningLabel" aria-hidden="true">' +
                    '<div class="modal-dialog modal-lg">' +
                    '<div id="myCarousel" class="carousel slide" data-ride="carousel" data-interval="0">' +
                    '   <ol class="carousel-indicators"></ol>' +
                    '   <div class="carousel-inner"></div>' +
                    '   <a class="carousel-control left" href="#myCarousel" data-slide="prev"><span class="glyphicon glyphicon-chevron-left"></span></a>' +
                    '   <a class="carousel-control right" href="#myCarousel" data-slide="next"><span class="glyphicon glyphicon-chevron-right"></span></a></div></div>' +
                    '</div>';
                sliderModal = $(sliderHtml);
                $('body').append(sliderModal);
            }

            $('#myCarousel .carousel-inner').empty();
            $('#myCarousel .carousel-indicators').empty();

            for (var i = 0; i < imagesList.length; i++) {
                var item = $('<div class="item"><img   loading="lazy" src="' + imagesList[i].url + '"/>   <div  class="carousel-caption"> <h3>' + imagesList[i].text + '</h3></div>  </div>');
                var indicator = $('<li data-target="#myCarousel" data-slide-to="' + i + '"></li>');
                if (event.target.src && event.target.src.indexOf(imagesList[i].url) > -1) {
                    item.addClass('active');
                    indicator.addClass('active');
                }
                $('#myCarousel .carousel-inner').append(item);
                $('#myCarousel .carousel-indicators').append(indicator);
            }
            if (!event.target.src) {
                $('#myCarousel .carousel-inner>.item:first').addClass('active');
                $('#myCarousel .carousel-indicators>li:first').addClass('active');
            }

            sliderModal.modal("show");
        })
}

function removeValidations() {
    //Removes validation from input-fields
    $('.input-validation-error').addClass('input-validation-valid');
    $('.input-validation-error').removeClass('input-validation-error');
    $('.editorforError').removeClass('editorforError');
    //Removes validation message after input-fields
    $('.field-validation-error').addClass('field-validation-valid');
    $('.field-validation-error').removeClass('field-validation-error');
    //Removes validation summary 
    $('.validation-summary-errors').addClass('validation-summary-valid');
    $('.validation-summary-errors').removeClass('validation-summary-errors');
}

function CopyToClipBoardInit() {
    $("a.araad-kopyala").on("click", (function () {
        var targetId = $(this).data("targetid");
        event.preventDefault();
        CopyToClipBoard(targetId);
    }));
}

function ajaxPanelInit() {
    $("body").on("click", "a[data-ajaxurl]", function (event) {
        var $this = $(this);
        if (!$this.hasClass('ajax-dataloaded')) {

            var url = $this.data("ajaxurl");
            var containerselector = $this.data("ajaxcontainer");

            $.ajax({
                global: false,
                url: url,
                success: function (data) {
                    $(containerselector).html(data);
                    $this.addClass('ajax-dataloaded');
                },
                error: function (request) {
                    var mesaj = "Bilinmeyen Hata.";

                    if (request && request.responseJSON) {
                        mesaj = request.responseJSON.HataMesaji;
                    }
                    else {
                        mesaj = "Hata Mesajı:" + request.statusText + " Hata Kodu: " + request.status;
                    }

                    var content = '<div class="alert alert-danger">' + mesaj + '</div>';
                    $(containerselector).html(content);
                    $this.addClass('ajax-dataloaded');
                }
            });
        }
    });
}

function AraadSessionTimeout() {

    var logoutUrl = '/Account/Logout';
    var extendMethodUrl = '/Account/ExtendSession';
    var _timeLeft;
    this._popupTimer;
    this._countDownTimer;
    var $modal = null;

    function getModal() {
        if ($modal == null) {
            $modal = $('#divPopupTimeOut').modal({ backdrop: 'static' });
        }

        return $modal;
    }

    this.Init = function () {
        if (araadUserAuthorized) {
            schedulePopup();
        }
    };

    var schedulePopup = function () {
        hidePopup();
        stopTimers();
        _popupTimer = window.setTimeout(showPopup, araadPopupShowDelay);
    }

    this.KeepAlive = function () {
        console.debug("oturum uzatma çağrılıyor.");
        console.debug(extendMethodUrl);
        hidePopup();
        stopTimers();

        $.ajax({
            type: "GET",
            url: extendMethodUrl,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                if (data && data.IsSuccess) {
                    console.debug("oturum uzatıldı.");
                    schedulePopup();
                }
                else {
                    alert("Oturum sonlandırılmış. Lütfen tekrar giriş yapınız.");
                }
            },
            error: function () {
                alert("Oturum uzatılmadı.");
            }
        });
    };

    this.stopTimers = function () {
        window.clearTimeout(this._popupTimer);
        window.clearTimeout(this._countDownTimer);
    };

    var updateCountDown = function () {
        var min = Math.floor(_timeLeft / 60);
        var sec = _timeLeft % 60;
        if (sec < 10)
            sec = "0" + sec;

        getModal().find('#CountDownHolder').html(min + ":" + sec);
        if (_timeLeft > 0) {
            _timeLeft--;
            _countDownTimer = window.setTimeout(updateCountDown, 1000);
        } else {

            console.debug("logout çağrılıyor.");

            document.location = logoutUrl;
        }
    };

    var showPopup = function () {
        getModal().modal('show');
        _timeLeft = 120;
        updateCountDown();
    };

    var schedulePopup = function () {
        stopTimers();
        _popupTimer = window.setTimeout(showPopup, araadPopupShowDelay);
    };

    var hidePopup = function () {
        getModal().modal('hide');
        $modal = null;
    };

    Init();

    return this;
};

function callSolPartialToolTips() {
    $(".leftPartial .row div:not(:first-child):not(.notinclude)").each(function () {
        var titleText = $(this).text();
        $(this).append("<span class=\"tooltipHidden\">" + titleText + "</span>");
    });
}

function gridRowDoubleClickInit() {

    $('body').on('dblclick', 'table.grid-table>tbody>tr,table.table>tbody>tr', function (event) {

        var link = $(this).find('a.araad-dbclick-button').get(0);

        if (link == null) {
            link = $(this).find('td:first a:first').get(0);
        }

        if (link != null) {
            link.click();
        }
    });
}

function modalLinkClickInit() {
    $('body').on('click', 'a.modalGoster', function (event) {
        event.preventDefault();
        callModalPage($(this).attr('href'), $(this).data('modalbaslik'), null, true);
    });
}

$(document).on("submit", "form", function (e) {
    if ($(this).valid()) {
        $('#modalOnay:visible').modal('hide');

        if (!($(this).is('.dontShowProgressBar') || $(document.activeElement).is('.dontShowProgressBar'))) {
            showProgressBar();
        }
    } else {
        hideProgressBar();
    }
});

$.ajaxSetup({
    headers: { "CurrentUrl": document.URL }
});

function validateDescriptionFields(callerDomElementId, targetDomElementId, errorMessage, formToValidateId) {
    var targetId = "#" + targetDomElementId;
    var callerId = "#" + callerDomElementId;
    var targetForm = "#" + formToValidateId;
    var target = $(targetId);
    if ($(callerId + " option:selected").text() == "Diğer") {

        target.removeAttr("readonly")
            .attr("data-val", true)
            .attr("data-val-required", errorMessage + "açıklama alanı boş bırakılamaz")
            .addClass("input-validation-error").addClass("editorforError").removeClass("valid");

    } else {
        target.attr("readonly", "readonly")
            .removeAttr("data-val data-val-required")
            .val("")
            .removeClass("input-validation-error").removeClass("editorforError").addClass("valid");
    }

    $(targetForm).removeData("validator unobtrusiveValidation");
    $.validator.unobtrusive.parse(targetForm);
}

$(document).on("click", "#genericModalCloseAndRemove", function () {
    $("#genericModal").modal("hide");
});

$(document).on("hidden.bs.modal", "#genericModal", function () {
    hideProgressBar();
    $("#genericModal").remove();
});

//text inputu Verilen listeye göre combobox olarak çalışmasını sağlar
function autocomplete(inp, arr) {
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    var currentFocus;
    /*execute a function when someone writes in the text field:*/
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        /*close any already open lists of autocompleted values*/
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        /*create a DIV element that will contain the items (values):*/
        a = document.createElement("DIV");
        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items");
        a.setAttribute("style", "position: absolute; z-index:1; background-color:white; height:100px; width:100%; overflow-y: auto;");
        /*append the DIV element as a child of the autocomplete container:*/
        this.parentNode.appendChild(a);
        /*for each item in the array...*/
        for (i = 0; i < arr.length; i++) {
            /*check if the item starts with the same letters as the text field value:*/
            if (arr[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                /*create a DIV element for each matching element:*/
                b = document.createElement("DIV");
                /*make the matching letters bold:*/
                b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
                b.innerHTML += arr[i].substr(val.length);
                /*insert a input field that will hold the current array item's value:*/
                b.innerHTML += "<input type='hidden' value='" + arr[i] + "'>";
                /*execute a function when someone clicks on the item value (DIV element):*/
                b.addEventListener("click", function (e) {
                    /*insert the value for the autocomplete text field:*/
                    inp.value = this.getElementsByTagName("input")[0].value;
                    /*close the list of autocompleted values,
                    (or any other open lists of autocompleted values:*/
                    closeAllLists();
                });
                a.appendChild(b);
            }
        }
    });
    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            /*If the arrow DOWN key is pressed,
            increase the currentFocus variable:*/
            currentFocus++;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 38) { //up
            /*If the arrow UP key is pressed,
            decrease the currentFocus variable:*/
            currentFocus--;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 13) {
            /*If the ENTER key is pressed, prevent the form from being submitted,*/
            e.preventDefault();
            if (currentFocus > -1) {
                /*and simulate a click on the "active" item:*/
                if (x) x[currentFocus].click();
            }
        }
    });
    function addActive(x) {
        /*a function to classify an item as "active":*/
        if (!x) return false;
        /*start by removing the "active" class on all items:*/
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        /*add class "autocomplete-active":*/
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        /*a function to remove the "active" class from all autocomplete items:*/
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        /*close all autocomplete lists in the document,
        except the one passed as an argument:*/
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }
    /*execute a function when someone clicks in the document:*/
    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}


function callModalPage(url, headerText, ModalId, async) {
    if (async) {
        async = true;
    }
    else {
        async = false;
    }

    if (!ModalId) {
        ModalId = "genericModal";
    }
    var modalVisible = $("#" + ModalId).is(":visible");
  
    if (!modalVisible) {
        $.ajax({
            async: async,
            global: false,
            type: "GET",
            url: "/SefimPanelVeriGonderimi/_Modal",
            data: { ModalId: ModalId },
            success: function (data) {
                $.ajax({
                    async: async,
                    global: false,
                    type: "GET",
                    url: url,
                    success: function (data2) {
                        $("body").append(data);
                        $("#" + ModalId).modal("show");
                        $("#" + ModalId + "HeaderText").append(headerText);
                        $("#" + ModalId + "Body").append(data2);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        araadHandleAjaxError(xhr, ajaxOptions, thrownError);
                    }
                });

                registerCustomControls();
                ////callDatePicker();
                //araadFileUpload();
                $("form").removeData("validator");
                $("form").removeData("unobtrusiveValidation");
                //$.validator.unobtrusive.parse("form");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                araadHandleAjaxError(xhr, ajaxOptions, thrownError);
            }
        });
    }
    else {
        $.ajax({
            async: async,
            global: false,
            type: "GET",
            url: url,
            success: function (data2) {
                $("#" + ModalId + "HeaderText").empty();
                $("#" + ModalId + "Body").empty();
                $("#" + ModalId + "HeaderText").append(headerText);
                $("#" + ModalId + "Body").append(data2);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                araadHandleAjaxError(xhr, ajaxOptions, thrownError);
            }
        });

        registerCustomControls();
        ////callDatePicker();
        araadFileUpload();
        $("form").removeData("validator");
        $("form").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse("form");
    }
}

function callModalPageIframe(url, headerText, ModalId, Height) {

    //url parametreleri parse işlemi.
    var parser = document.createElement('a');
    parser.href = url;

    if (!ModalId) {
        ModalId = "genericModal";
    }

    //url de yüksekli parametresi gelmişse onu kullan
    if (!parser.Height) {
        Height = 600;
    }
    else {
        Height = int.parse(parser.Height.replace('px', ''), 10);
    }

    var modalVisible = $("#" + ModalId).is(":visible");

    if (!modalVisible) {
        $.ajax({
            async: false,
            global: false,
            type: "GET",
            url: "/SefimPanelVeriGonderimi/_Modal",
            data: { ModalId: ModalId },
            success: function (data) {
                $("body").append(data);
                $("#" + ModalId).modal("show");

                $("#" + ModalId + "HeaderText").append(headerText);
                var $body = $("#" + ModalId + "Body");

                var data2 = "<iframe src=" + url + " height=\"" + (Height - 70) + "\" width=\"100%\" style=\"border-style:none\"></iframe>";
                $body.append(data2);

                nullableCheckBox();
                registerCustomControls();
                callDatePicker();
                araadFileUpload();
                $("form").removeData("validator");
                $("form").removeData("unobtrusiveValidation");
                $.validator.unobtrusive.parse("form");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                $("#" + ModalId + "Body")
                    .empty()
                    .append("Sayfa Yüklenemedi")
                    .Height(Height);
            }
        });
    }
    else {
        $("#" + ModalId + "HeaderText").empty();
        $("#" + ModalId + "HeaderText").append(headerText);

        var data2 = "<iframe src=" + url + " height=\"" + (Height - 70) + "\" width=\"100%\" style=\"border-style:none\"></iframe>";

        $("#" + ModalId + "Body")
            .empty()
            .append(data2)
            .Height(Height);

        nullableCheckBox();
        registerCustomControls();
        callDatePicker();
        araadFileUpload();
        $("form").removeData("validator");
        $("form").removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse("form");
    }
}

function callDatePicker() {

    ////$(".datepicker").datepicker({
    ////    showButtonPanel: false,
    ////    changeMonth: true,
    ////    changeYear: true,
    ////    onSelect: function (dateText, inst) {
    ////        $(this).attr("value", dateText);
    ////        $(this).valid();
    ////    }
    ////});
}
function disableFutureDates(maxDate) {
    var today;
    if (maxDate == "undefined" || maxDate == "" || maxDate == null) {
        today = new Date();
    }
    else {
        today = maxDate;
    }
    var start = new Date();
    var end = today;

    const parts_start = start.toString().split(/[- :]/);
    const wanted_start = parts_start[2] + '/' + parts_start[1] + '/' + parts_start[3];

    const parts_end = end.toString().split(/[- :]/);
    const wanted_end = parts_end[2] + '/' + parts_end[1] + '/' + parts_end[3];

    var diff = new Date(new Date(wanted_end) - new Date(wanted_start));
    var days = diff / 1000 / 60 / 60 / 24;
    var myEndDate = "+" + Math.round(days).toString() + "d";
    $(".datepicker").datepicker({
        endDate: myEndDate
    });
}
function registerCustomControls() {

    $(".decimal_input").attr({
        "data-html": "true",
        "data-toggle": "tooltip",
        "title": "1,11 <br/> 1,1 <br/> Şeklinde giriniz",
        "trigger": "manual"
    });

    //Koordinat tooltip ayarlama
    $(".koordinat_input").attr({
        "data-html": "true",
        "data-toggle": "tooltip",
        "title": "1,111111 <br/> 1,111111 <br/> Şeklinde giriniz",
        "trigger": "manual"
    });

    //Telefon Numarası tooltip ayarlama
    $(".phonenumber_input").attr({
        "data-html": "true",
        "data-toggle": "tooltip",
        "title": "(545) 455-45-45",
        "trigger": "manual",
        "type": "tel"
    });

    //Eposta tooltip ayarlama
    $(".eposta_input").attr({
        "data-html": "true",
        "data-toggle": "tooltip",
        "title": "ornek@csb.gov.tr <br/> ornek@csb.gov.tr <br/> Şeklinde giriniz",
        "trigger": "manual",
        "type": "email"
    });

    //IBAN tooltip ayarlama
    $(".iban_input").attr({
        "data-html": "true",
        "data-toggle": "tooltip",
        "title": "IBAN \"TR\" dahil 26 karakter uzunluğunda olmalıdır",
        "trigger": "manual"
    });

    //TC Kimlik No tooltip ayarlama
    $(".tckimlik_input").attr({
        "data-html": "true",
        "data-toggle": "tooltip",
        "title": "T.C. Kimlik No 11 karakter uzunluğunda olmalıdır",
        "trigger": "manual"
    });
}

$(function () {
    $("[data-toggle=\"tooltip\"]").tooltip();
});

//  tckimlik_input Kontrolü
$(document).on("keypress", ".tckimlik_input", function () {
    return isPositiveNumber(event, this);
});

//TC Kimlik Numarası tooltip gösterme
$(document).on("mouseover", ".tckimlik_input", function () {
    $(this).tooltip().tooltip("show");
});

//TC Kimlik Numarası tooltip kapatma
$(document).on("mouseout", ".tckimlik_input", function () {
    $(this).tooltip().tooltip("destroy");
});

//IBAN  Kontrolü
$(document).on("keypress", ".iban_input", function () {
    return isIBAN(event, this);
});

//IBAN tooltip gösterme
$(document).on("mouseover", ".iban_input", function () {
    $(this).tooltip().tooltip("show");
});

//IBAN tooltip kapatma
$(document).on("mouseout", ".iban_input", function () {
    $(this).tooltip().tooltip("destroy");
});

//Eposta tooltip gösterme
$(document).on("mouseover", ".eposta_input", function () {
    $(this).tooltip().tooltip("show");
});

//Eposta tooltip kapatma
$(document).on("mouseout", ".eposta_input", function () {
    $(this).tooltip().tooltip("destroy");
});

//Telefon numarası UnMask
function formatPhoneNumber(number) {
    let cleaned = ('' + number).replace(/\D/g, '');
    let match = cleaned.match(/^(1|)?(\d{3})(\d{3})(\d{4})$/);
    if (match) {
        let intlCode = (match[1] ? '+1 ' : '')
        var mask = [intlCode, '(', match[2], ') ', match[3], '-', match[4]].join('')
        return mask
    }

    return number;
}

//Telefon numarası için format
function PhoneNumberInputInit() {
    $(".phonenumber_input").each(function () {
        this.value = formatPhoneNumber(this.value);
    });

    //Telefon numarası için format
    $(".phonenumber_input").keypress(function (e) {
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
            return false;
        }
        var curchr = this.value.length;
        var curval = $(this).val();
        if (curchr == 3 && curval.indexOf("(") <= -1) {
            $(this).val("(" + curval + ")" + "-");
        } else if (curchr == 4 && curval.indexOf("(") > -1) {
            $(this).val(curval + ")-");
        } else if (curchr == 5 && curval.indexOf(")") > -1) {
            $(this).val(curval + "-");
        } else if (curchr == 9) {
            $(this).val(curval + "-");
            $(this).attr('maxlength', '14');
        }
    });
};

//Telefon Numarası tooltip gösterme
$(document).on("mouseover", ".phonenumber_input", function () {
    $(this).tooltip().tooltip("show");
});

//Telefon Numarası tooltip kapatma
$(document).on("mouseout", ".phonenumber_input", function () {
    $(this).tooltip().tooltip("destroy");
});

$(document).on("focusout", ".interval_input", function (event) {
    return isInterval(event, this);
});

//Decimal TextBox Rakam Kontrolü
$(document).on("keypress", ".decimal_input", function (event) {
    return isDecimal(event, this);
});

//Decimal tooltip gösterme
$(document).on("mouseover", ".decimal_input", function () {
    $(this).tooltip().tooltip("show");
});

//Decimal tooltip kapatma
$(document).on("mouseout", ".decimal_input", function () {
    $(this).tooltip().tooltip("destroy");
});

//Koordinat TextBox Rakam Kontrolü
$(document).on("keypress", ".koordinat_input", function (event) {
    return isDecimal(event, this);
});

//Koordinat tooltip gösterme
$(document).on("mouseover", ".koordinat_input", function () {
    $(this).tooltip().tooltip("show");
});

//Koordinat tooltip kapatma
$(document).on("mouseout", ".koordinat_input", function () {
    $(this).tooltip().tooltip("destroy");
});

//Onaylı İşlem
$(document).on('click', '[data-confirm]', function (e) {
    e.preventDefault();

    var $this = $(this);
    var confirmModel = $this.data('confirm');
    var ilkKisim = '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
        '<div class="modal-dialog modal-md">' +
        '<div class="modal-content">' +
        '<div class="modal-header panel-heading">' +
        '<button type="button" class="close" data-dismiss="modal" aria-hidden="true" title="Kapat">&times;</button>' +
        '<h4 class="modal-title"></h4>' +
        '</div>' +
        '<div class="modal-body"></div>' +
        '<div class="modal-footer">';
    var vazgec = '<button type="button" class="btn btn-default btn-sm" data-dismiss="modal">Vazgeç</button>';
    var ikinciKisim = '<button type="button" class="btn confirm" data-dismiss="modal"></button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';
    var confirmText = ilkKisim;
    if (!confirmModel.VazgecButonuGosterme) {
        confirmText += vazgec;

    }
    confirmText += ikinciKisim;
    var modalConfirm = $(confirmText);
    //$(
    //'<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
    //'<div class="modal-dialog modal-md">' +
    //'<div class="modal-content">' +
    //'<div class="modal-header panel-heading">' +
    //'<button type="button" class="close" data-dismiss="modal" aria-hidden="true" title="Kapat">&times;</button>' +
    //'<h4 class="modal-title"></h4>' +
    //'</div>' +
    //'<div class="modal-body"></div>' +
    //'<div class="modal-footer">' +
    //'<button type="button" class="btn btn-default btn-sm" data-dismiss="modal">Vazgeç</button>' +
    //'<button type="button" class="btn confirm" data-dismiss="modal"></button>' +
    //'</div>' +
    //'</div>' +
    //'</div>' +
    //'</div>');

    modalConfirm.find('div.modal-content').addClass('panel-' + confirmModel.ModalKritiklik);
    modalConfirm.find('h4.modal-title').html(confirmModel.Baslik);
    modalConfirm.find('div.modal-body').html(confirmModel.Mesaj);

    modalConfirm.find('div.modal-footer>.confirm')
        .addClass('btn-' + confirmModel.ModalKritiklik)
        .html(confirmModel.OnayButonMetni)
        .on('click', function () {
            if (confirmModel.UseAjax) {
                $.ajax({
                    async: false,
                    global: false,
                    type: 'GET',
                    url: $this.attr('href'),
                    success: function () {
                        modalConfirm.modal('hide');
                        window.location.href = window.location.href;
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        modalConfirm.modal('hide');
                        araadHandleAjaxError(xhr, ajaxOptions, thrownError);
                    }
                });
            } else {
                showProgressBar();
                window.location.href = $this.attr('href');
            }
        });

    $(document).on('hidden.bs.modal', modalConfirm, function () {
        $(this).remove();
    });

    $('body').append(modalConfirm);

    modalConfirm.modal('show');
});

function showModal(options) {

    var defaults = {
        ModalKritiklik: "warning",
        Baslik: "Uyarı",
        Mesaj: null,
        OnayButonMetni: "Tamam",
        Size: "modal-md"
    };

    var settings = $.extend({}, defaults, options);

    var modalConfirm = $(
        '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
        '<div class="modal-dialog ' + settings.Size + '">' +
        '<div class="modal-content">' +
        '<div class="modal-header panel-heading">' +
        '<button type="button" class="close" data-dismiss="modal" aria-hidden="true" title="Kapat">&times;</button>' +
        '<h4 class="modal-title"></h4>' +
        '</div>' +
        '<div class="modal-body"></div>' +
        '<div class="modal-footer">' +
        '<button type="button" class="btn btn-default btn-sm" data-dismiss="modal">Vazgeç</button>' +
        '<button type="button" class="btn confirm"></button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>');

    modalConfirm.find('div.modal-content').addClass('panel-' + settings.ModalKritiklik);
    modalConfirm.find('h4.modal-title').html(settings.Baslik);
    modalConfirm.find('div.modal-body').html(settings.Mesaj);

    modalConfirm.find('div.modal-footer>.confirm')
        .addClass('btn-' + settings.ModalKritiklik)
        .html(settings.OnayButonMetni)
        .on('click', function () {
            modalConfirm.modal('hide');
        }
        );
    $(document).on('hidden.bs.modal', modalConfirm, function () {
        $(this).remove();
    });

    $('body').append(modalConfirm);

    modalConfirm.modal('show');
};

function uyariShowModal(options) {

    var defaults = {
        ModalKritiklik: "warning",
        Baslik: "Uyarı",
        Mesaj: null,
        OnayButonMetni: "Tamam"
    };

    var settings = $.extend({}, defaults, options);

    var modalConfirm = $(
        '<div class="modal fade" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">' +
        '<div class="modal-dialog modal-md">' +
        '<div class="modal-content">' +
        '<div class="modal-header panel-heading">' +
        '<button type="button" class="close" data-dismiss="modal" aria-hidden="true" title="Kapat">&times;</button>' +
        '<h4 class="modal-title"></h4>' +
        '</div>' +
        '<div class="modal-body"></div>' +
        '<div class="modal-footer">' +

        '</div>' +
        '</div>' +
        '</div>' +
        '</div>');

    modalConfirm.find('div.modal-content').addClass('panel-' + settings.ModalKritiklik);
    modalConfirm.find('h4.modal-title').html(settings.Baslik);
    modalConfirm.find('div.modal-body').html(settings.Mesaj);

    modalConfirm.find('div.modal-footer>.confirm')
        .addClass('btn-' + settings.ModalKritiklik)
        .html(settings.OnayButonMetni)
        .on('click', function () {
            modalConfirm.modal('hide');
        }
        );
    $(document).on('hidden.bs.modal', modalConfirm, function () {
        $(this).remove();
    });

    $('body').append(modalConfirm);

    modalConfirm.modal('show');
};

//Onaylı Silme İşlemi
$(document).on("click", ".deleteConfirmDialog", function () {

    var $this = $(this);
    event.preventDefault();

    var mesaj = $this.data("mesaj") || 'Seçmiş Olduğunuz Kayıt Silinecektir';
    var baslik = $this.data("baslik") || 'Kayıt Silme';
    var url = $this.data("url") || $this.attr('href');
    var ajaxPost = $this.data("ajaxpost");
    var rowId = $this.data("rowid");
    var bodyId = $this.data("bodyid");

    var modalDelete =
        "<div class=\"modal fade\" id=\"modalDelete\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"modalDeleteLabel\" aria-hidden=\"true\" data-backdrop=\"static\" data-keyboard=\"false\">" +
        "<div class=\"modal-dialog modal-md\">" +
        "<div class=\"modal-content panel-danger\">" +
        "<div class=\"modal-header panel-heading\">" +
        "<button type=\"button\" class=\"close\" data-dismiss=\"modal\" aria-hidden=\"true\" title=\"Kapat\">&times;</button>" +
        "<h4 class=\"modal-title\">" + baslik + "</h4>" +
        "</div>" +
        "<div class=\"modal-body\">" + mesaj + "</div>" +
        "<div class=\"modal-footer\">" +
        "<button type=\"button\" class=\"btn btn-default btn-sm\" data-dismiss=\"modal\">Vazgeç</button>" +
        "<a id=\"hrefDeleteConfirm\" class=\"btn btn-danger btn-sm\">Sil</a>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "</div>";
    if (ajaxPost == 1) {

        $("body").append(modalDelete);

        $(document).on("hidden.bs.modal", "#modalDelete", function () {
            $("#modalDelete").remove();
        });

        $("#modalDelete").modal("show");

        $("#hrefDeleteConfirm").on("click", function () {
            $.ajax({
                async: false,
                global: false,
                type: "GET",
                url: url,
                success: function (data) {
                    $("#" + bodyId).find("#" + rowId).remove();
                    $("#modalDelete").modal("hide");
                    if (data != null && data.length > 0) {
                        window.location.href = data;
                    } else {
                        window.location.href = window.location.href;
                    }

                },
                error: function (xhr, ajaxOptions, thrownError) {
                    $("#modalDelete").modal("hide");
                    araadHandleAjaxError(xhr, ajaxOptions, thrownError);
                }
            });
        });
    }
    else {
        if (url) {

            $("body").append(modalDelete);

            $(document).on("hidden.bs.modal", "#modalDelete", function () {
                $("#modalDelete").remove();
            });

            $("#hrefDeleteConfirm").attr("href", url);
            $("#modalDelete").modal("show");
        }
    }
});

//  integer Kontrolü
$(document).on("keypress", ".integer_input", function (event) {
    return isInteger(event, this);
});


$(document).on("keydown", ".decimal_money_format_input", function (e) {
    if (this.selectionStart || this.selectionStart == 0) {
        // selectionStart won't work in IE < 9

        var key = e.which;
        var prevDefault = true;

        var thouSep = ",";  // your seperator for thousands
        var deciSep = ".";  // your seperator for decimals
        var deciNumber = 2; // how many numbers after the comma

        var thouReg = new RegExp(thouSep, "g");
        var deciReg = new RegExp(deciSep, "g");

        function spaceCaretPos(val, cPos) {
            /// get the right caret position without the spaces

            if (cPos > 0 && val.substring((cPos - 1), cPos) == thouSep)
                cPos = cPos - 1;

            if (val.substring(0, cPos).indexOf(thouSep) >= 0) {
                cPos = cPos - val.substring(0, cPos).match(thouReg).length;
            }

            return cPos;
        }

        function spaceFormat(val, pos) {
            /// add spaces for thousands

            if (val.indexOf(deciSep) >= 0) {
                var comPos = val.indexOf(deciSep);
                var int = val.substring(0, comPos);
                var dec = val.substring(comPos);
            } else {
                var int = val;
                var dec = "";
            }
            var ret = [val, pos];

            if (int.length > 3) {

                var newInt = "";
                var spaceIndex = int.length;

                while (spaceIndex > 3) {
                    spaceIndex = spaceIndex - 3;
                    newInt = thouSep + int.substring(spaceIndex, spaceIndex + 3) + newInt;
                    if (pos > spaceIndex) pos++;
                }
                ret = [int.substring(0, spaceIndex) + newInt + dec, pos];
            }
            return ret;
        }

        $(this).on('keyup', function (ev) {

            if (ev.which == 8) {
                // reformat the thousands after backspace keyup

                var value = this.value;
                var caretPos = this.selectionStart;

                caretPos = spaceCaretPos(value, caretPos);
                value = value.replace(thouReg, '');

                var newValues = spaceFormat(value, caretPos);
                this.value = newValues[0];
                this.selectionStart = newValues[1];
                this.selectionEnd = newValues[1];
            }
        });

        if ((e.ctrlKey && (key == 65 || key == 67 || key == 86 || key == 88 || key == 89 || key == 90)) ||
            (e.shiftKey && key == 9)) // You don't want to disable your shortcuts!
            prevDefault = false;

        if ((key < 37 || key > 40) && key != 8 && key != 9 && prevDefault) {
            e.preventDefault();

            if (!e.altKey && !e.shiftKey && !e.ctrlKey) {

                var value = this.value;
                if ((key > 95 && key < 106) || (key > 47 && key < 58) ||
                    (deciNumber > 0 && (key == 110 || key == 188 || key == 190))) {

                    var keys = { // reformat the keyCode
                        48: 0, 49: 1, 50: 2, 51: 3, 52: 4, 53: 5, 54: 6, 55: 7, 56: 8, 57: 9,
                        96: 0, 97: 1, 98: 2, 99: 3, 100: 4, 101: 5, 102: 6, 103: 7, 104: 8, 105: 9,
                        110: deciSep, 188: deciSep, 190: deciSep
                    };

                    var caretPos = this.selectionStart;
                    var caretEnd = this.selectionEnd;

                    if (caretPos != caretEnd) // remove selected text
                        value = value.substring(0, caretPos) + value.substring(caretEnd);

                    caretPos = spaceCaretPos(value, caretPos);

                    value = value.replace(thouReg, '');

                    var before = value.substring(0, caretPos);
                    var after = value.substring(caretPos);
                    var newPos = caretPos + 1;

                    if (keys[key] == deciSep && value.indexOf(deciSep) >= 0) {
                        if (before.indexOf(deciSep) >= 0) newPos--;
                        before = before.replace(deciReg, '');
                        after = after.replace(deciReg, '');
                    }
                    var newValue = before + keys[key] + after;

                    if (newValue.substring(0, 1) == deciSep) {
                        newValue = "0" + newValue;
                        newPos++;
                    }

                    while (newValue.length > 1 && newValue.substring(0, 1) == "0" && newValue.substring(1, 2) != deciSep) {
                        newValue = newValue.substring(1);
                        newPos--;
                    }

                    if (newValue.indexOf(deciSep) >= 0) {
                        var newLength = newValue.indexOf(deciSep) + deciNumber + 1;
                        if (newValue.length > newLength) {
                            newValue = newValue.substring(0, newLength);
                        }
                    }

                    newValues = spaceFormat(newValue, newPos);

                    this.value = newValues[0];
                    this.selectionStart = newValues[1];
                    this.selectionEnd = newValues[1];
                }
            }
        }

        $(this).on('blur', function (e) {

            if (deciNumber > 0) {
                var value = this.value;

                var noDec = "";
                for (var i = 0; i < deciNumber; i++) noDec += "0";

                if (value == "0" + deciSep + noDec) {
                    this.value = ""; //<-- put your default value here
                } else if (value.length > 0) {
                    if (value.indexOf(deciSep) >= 0) {
                        var newLength = value.indexOf(deciSep) + deciNumber + 1;
                        if (value.length < newLength) {
                            while (value.length < newLength) value = value + "0";
                            this.value = value.substring(0, newLength);
                        }
                    }
                    else this.value = value + deciSep + noDec;
                }
            }
        });
    }
});

var delay = (function () {
    var timer = 0;
    return function (callback, ms) {
        clearTimeout(timer);
        timer = setTimeout(callback, ms);
    };
})();

//Jquery hatalı verilerin olduğu textbox rengini değiştirme
jQuery.validator.setDefaults({
    highlight: function (element, errorClass, validClass) {
        if (element.type === "radio") {
            this.findByName(element.name).addClass(errorClass).removeClass(validClass);
        }
        else if (element.type === "text" || element.type === "textarea") {
            if ($(element).attr("readonly") == "readonly") // file input ise
            {
                var div_file_name = $(element).parent();
                if (div_file_name) {
                    var style_file_name = div_file_name.attr("class");
                    if (style_file_name == "file_name") {
                        var div_fileUploadStyle = div_file_name.parent();
                        if (div_fileUploadStyle) {
                            var style_fileUploadStyle = div_fileUploadStyle.attr("class");
                            if (style_fileUploadStyle == "fileUploadStyle") {
                                //div_fileUploadStyle.removeClass(validClass);
                                $(element).addClass(errorClass).removeClass(validClass);
                                div_fileUploadStyle.addClass("editorforError");
                            }
                        }
                    }
                }
            }
            else // geri kalan type text olanlar
            {
                $(element).addClass(errorClass).removeClass(validClass);
                $(element).closest(".form-control").addClass("editorforError");
            }

        }
        else {
            $(element).addClass(errorClass).removeClass(validClass);
            $(element).closest(".form-control").addClass("editorforError");
        }

    },
    unhighlight: function (element, errorClass, validClass) {
        if (element.type === "radio") {
            this.findByName(element.name).removeClass(errorClass).addClass(validClass);
        }
        else if (element.type === "file") {
            this.findByName($(element).attr("id") + ".Adi").removeClass(errorClass).addClass(validClass);
            $(element).parent().parent().removeClass("editorforError");
        }
        else {
            $(element).removeClass(errorClass).addClass(validClass);
            $(element).closest(".form-control").removeClass("editorforError");
        }
    }
});

$.validator.methods.number = function (value, element) {
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:[,]\d+)?$/.test(value);
}

// https://jqueryvalidation.org/range-method/
//sayıları range ile kontrol ederken türkçe gelen binler ayracı ve ondalık ayracı karakterlerini değiştirilerek işlem yapılmalıdır.
// Örnek: 100.234,56 --> 100234.56'a dönüştür
$.validator.methods.range = function (value, element, param) {
    var isNumber = $(element).rules().number;
    if (isNumber) {
        value = (value + '').replaceAll('.', '');
        value = value.replaceAll(',', '.');
        value = parseFloat(value);

        return this.optional(element) || (value >= param[0] && value <= param[1]);
    }

    return this.optional(element) || (value >= param[0] && value <= param[1]);
}

//jquery validation kontrolü
jQuery(function ($) {
    $.validator.addMethod(
        "date",
        function (value, element) {
            if (this.optional(element)) {
                return true;
            }

            var ok = true;
            try {

                $.datepicker.parseDate("dd/mm/yy", value);
            }
            catch (err) {
                ok = false;
            }
            return ok;
        });
    $.validator.addMethod(
        "regtckimlik_input",
        function (value, element) {
            var no = value.split("");
            var i, total1 = 0, total2 = 0, total3 = parseInt(no[0]);
            for (i = 0; i < 10; i++) {
                total1 = total1 + parseInt(no[i]);
            }

            for (i = 1; i < 9; i = i + 2) {
                total2 = total2 + parseInt(no[i]);
                total3 = total3 + parseInt(no[i + 1]);
            }

            return this.optional(element) || !(!/^[1-9][0-9]{10}$/.test(value) || (total1 % 10 != no[10]) || (total3 * 7 - total2) % 10 != no[9]);
        },
        "Geçersiz TC Kimlik No"
    );

    $.validator.addMethod(
        "regexeposta_input",
        function (value, element) {
            return this.optional(element) || !(!/^([a-z\d!#$%&"*+\-\/=?^_`{|}~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+(\.[a-z\d!#$%&"*+\-\/=?^_`{|}~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]+)*|"((([ \t]*\r\n)?[ \t]+)?([\x01-\x08\x0b\x0c\x0e-\x1f\x7f\x21\x23-\x5b\x5d-\x7e\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|\\[\x01-\x09\x0b\x0c\x0d-\x7f\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))*(([ \t]*\r\n)?[ \t]+)?")@(([a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|[a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF][a-z\d\-._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*[a-z\d\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])\.)+([a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]|[a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF][a-z\d\-._~\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]*[a-z\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])\.?$/i.test(value));
        },
        "Eposta adresi geçerli değil"
    );
    $.validator.addMethod(
        "regexiban_input",
        function (value, element) {
            return this.optional(element) || !(!/^\d{24}$/.test(value));
        },
        "IBAN 26 haneli olmalıdır"
    );
    $.validator.addMethod(
        "iban_ziraat_input",
        function (value, element) {
            if (value.length == 24) {
                var isZiraat = false;
                var bankaKodu = value.slice(2, 7);
                if (bankaKodu == "00010") {
                    isZiraat = true;
                }
                return this.optional(element) || (isZiraat);
            }
            else {
                return true;
            }
        },
        "Ziraat Bankasına ait bir IBAN girmelisiniz"
    );
});

jQuery.validator.addClassRules("iban_ziraat", {
    iban_ziraat_input: true
});

jQuery.validator.addClassRules("iban_input", {
    regexiban_input: true
});

jQuery.validator.addClassRules("tckimlik_input", {
    regtckimlik_input: false
});

jQuery.validator.addClassRules("datepicker", {
    date: true
});

jQuery.validator.addClassRules("eposta_input", {
    regexeposta_input: true
});

//girilen input rakam mı harf mi kontrolü
function isDecimal(evt, element) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (
        //(charCode != 45 || $(element).val().indexOf("-") != -1) &&      // “-” CHECK MINUS, AND ONLY ONE.
        (charCode != 44 || $(element).val().indexOf(",") != -1) &&      // “,” CHECK COMMA, AND ONLY ONE.
        //(charCode != 46) &&
        (charCode < 48 || charCode > 57)) {
        $(element).addClass("editorforError");
        return false;
    }
    $(element).removeClass("editorforError");
    return true;
}

function isInterval(evt, element) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    var control = $(element).val().replace(',', '.');
    if (parseFloat("0") >= control || control >= parseFloat("1")) {
        $(element).addClass("editorforError");
        return false;
    }
    $(element).removeClass("editorforError");
    return true;
}

function isInteger(evt, element) {
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (
        //(charCode != 45 || $(element).val().indexOf("-") != -1) &&      // “-” CHECK MINUS, AND ONLY ONE.
        //(charCode != 44 || $(element).val().indexOf(",") != -1) &&      // “,” CHECK COMMA, AND ONLY ONE.
        //(charCode != 46) &&
        (charCode < 48 || charCode > 57)) {
        $(element).addClass("editorforError");
        return false;
    }
    $(element).removeClass("editorforError");
    return true;
}

function isIBAN(evt, element) {
    //24 karakteri asınca giris engellenir.
    if ($(element).val().length >= 24)
        event.preventDefault();

    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (
        //(charCode != 44 || $(element).val().indexOf(",") != -1) &&      // “,” CHECK COMMA, AND ONLY ONE.
        //(charCode != 46) &&
        (charCode < 48 || charCode > 57)) {
        $(element).addClass("editorforError");
        return false;
    }
    $(element).removeClass("editorforError");
    return true;
}

function isPositiveNumber(evt, element) {

    //11 karakteri asınca giris engellenir.
    if ($(element).val().length >= 11)
        event.preventDefault();

    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (
        //(charCode != 44 || $(element).val().indexOf(",") != -1) &&      // “,” CHECK COMMA, AND ONLY ONE.
        //(charCode != 46) &&
        (charCode < 48 || charCode > 57)) {
        $(element).addClass("editorforError");
        return false;
    }
    $(element).removeClass("editorforError");
    return true;
}

function convertJsonDate(value) {
    if (value) {
        var date = new Date(parseInt(value.substr(6)));
        var formatted = ("0" + date.getDate()).slice(-2) + "/" + ("0" + (date.getMonth() + 1)).slice(-2) + "/" + date.getFullYear();
        return formatted;
    }
    else {
        return "";
    }
}

/*iki tarih arasındaki gün farkını hesaplar.*/
var MS_One_Day = 1000 * 60 * 60 * 24;
function getDiffBetweenDays(startDate, endDate) {

    var startDate2 = startDate.split('/');
    var endDate2 = endDate.split('/');
    var dateStart = new Date(parseInt(startDate2[2], 10), parseInt(startDate2[1], 10), parseInt(startDate2[0], 10));
    var dateEnd = new Date(parseInt(endDate2[2], 10), parseInt(endDate2[1], 10), parseInt(endDate2[0], 10));
    return Math.floor((dateStart - dateEnd) / MS_One_Day);
}

function nullableCheckBox() {
    function output(state, value) {
        var hiddenValueId;
        if (value == "Evet") {
            $(this).attr("value", "true");
            $(this).next("p").text(value);
            hiddenValueId = $(this).attr("id");
            $("#" + hiddenValueId + "_hiddenValue").remove();
        }
        else if (value == "Hayır") {
            $(this).attr("value", "true");
            $(this).append("<input type=\"hidden\" id=\"" + $(this).attr("id") + "_hiddenValue\"  value=\"false\" name=\"" + $(this).attr("name") + "\" />");
            $(this).next("p").text(value);
        }
        else if (value == "Belirsiz") {
            $(this).attr("value", "null");
            $(this).next("p").text(value);
            hiddenValueId = $(this).attr("id");
            $("#" + hiddenValueId + "_hiddenValue").remove();
        }
        $("#tristate-value").text(value);
    }
}

function fillAutoCompleteFor(valueTagId, keyTagId, methodName, topValue) {
    /// <param name="methodName" type="string">
    /// @General.AutoCompleteTipi değerlerinden biri.
    /// </param>

    if (!topValue) {
        topValue = 3
    }

    autoCompleteClearButton(valueTagId, keyTagId);
    var selectedValue = $("#" + keyTagId).val();

    if (selectedValue != '') {
        $.ajax({
            global: false,
            async: false,
            url: "/AutoComplete/Result",
            dataType: "json",
            data: {
                methodName: methodName,
                key: selectedValue
            },
            success: function (data) {
                $("#" + valueTagId).before("<label id='lbl_autocomplete_selectedItem_" + valueTagId + "'>" + data + "</label>");
            }
        });
    }
    $("#" + valueTagId).autocomplete({
        source: function (request, response) {
            $.ajax({
                global: false,
                async: false,
                url: "/AutoComplete/Main",
                datatype: "json",
                clearButton: true,
                minLength: topValue,
                data: {
                    methodName: methodName,
                    text: request.term,
                    topValue: topValue
                },
                success: function (data) {
                    response($.map(data, function (val, item) {
                        return {
                            label: val.Value,
                            value: val.Value,
                            customerId: val.Key
                        }
                    }))
                }
            })
        },
        select: function (event, ui) {
            $("#" + keyTagId).val("");
            $("#" + keyTagId).val(ui.item.customerId);
            $("#lbl_autocomplete_selectedItem_" + valueTagId).remove();
            $("#" + valueTagId).before("<label id='lbl_autocomplete_selectedItem_" + valueTagId + "'>" + ui.item.value + "</label>");
            ui.item.value = "";
        }
    });
}

function autoCompleteClearButton(valueTagId, keyTagId) {
    $.widget("ui.autocomplete", $.ui.autocomplete, {
        options: {
            clearButton: true,
            clearButtonHtml: '&times;',
            clearButtonPosition: {
                my: "right center",
                at: "right center"
            }
        },

        _create: function () {

            var self = this;
            self._super();

            if (self.options.clearButton) {
                self._createClearButton();
            }

        },

        _createClearButton: function () {
            var self = this;
            self.clearElement = $("<span>")
                .attr("tabindex", "-1")
                .addClass("ui-autocomplete-clear")
                .html(self.options.clearButtonHtml)
                .appendTo(self.element.parent());

            if (self.options.clearButtonPosition !== false && typeof self.options.clearButtonPosition === 'object') {
                if (typeof self.options.clearButtonPosition.of === 'undefined') {
                    self.options.clearButtonPosition.of = self.element;
                }
                self.clearElement.position(self.options.clearButtonPosition);
            }

            self._on(self.clearElement, {
                click: function () {
                    $("#lbl_autocomplete_selectedItem_" + valueTagId).remove();
                    $("#" + keyTagId).val("");
                    self.element.val('').focus();
                    self._hideClearButton();
                }
            });

            self.element.addClass('ui-autocomplete-input-has-clear');

            self._on(self.element, {
                input: function () {
                    if (self.element.val() !== "") {
                        self._showClearButton();
                    } else {
                        self._hideClearButton();
                    }
                }
            });

            self._on(self.menu.element, {
                menuselect: function () {
                    self._showClearButton();
                }
            });

            // show clearElement if input has some content on initialization
            if (self.element.val() !== "") {
                self._showClearButton();
            } else {
                self._hideClearButton();
            }

        },

        _showClearButton: function () {
            this.clearElement.css({ 'display': 'inline-block' });
        },

        _hideClearButton: function () {
            this.clearElement.css({ 'display': 'none' });
        }

    });
}

function araadFileUpload() {

    ////$(".fileUploadClass").fileupload({
    ////    add: function (e, data) {
    ////        var uploadErrors = [];
    ////        var attr = $$("#" + e.target.id).attr("imageFileTypes");
    ////        var isPdf = $$("#" + e.target.id).attr("ispdf");
    ////        var isExcel = $$("#" + e.target.id).attr("isexcel");
    ////        var control = false;

    ////        if (isPdf == "true") {
    ////            if (data.originalFiles[0].type == "application/pdf") {
    ////                control = true;
    ////            }
    ////            if (!control) {
    ////                uploadErrors.push("Seçilen dosya tipi pdf olmalıdır");
    ////            }
    ////        } else if (isExcel == "true") {
    ////            if (data.originalFiles[0].type == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
    ////                control = true;
    ////            }
    ////            if (!control) {
    ////                uploadErrors.push("Seçilen dosya tipi excel olmalıdır");
    ////            }
    ////        } else {
    ////            if (typeof attr !== typeof undefined && attr !== false) {
    ////                var acceptFileTypes = $$("#" + e.target.id).attr("imageFileTypes").split(",");
    ////                for (var i = 0; i < acceptFileTypes.length; i++) {
    ////                    if (acceptFileTypes[i] == data.originalFiles[0].type.replace("image/", "")) {
    ////                        control = true;
    ////                    }
    ////                }
    ////                if (!control) {
    ////                    uploadErrors.push("Seçilen dosya izin verilen tipte değil. Geçerli dosya tipleri :" + attr);
    ////                }
    ////            }
    ////        }
    ////        if (data.originalFiles[0].size > $$("#" + e.target.id).attr("filesize")) {
    ////            uploadErrors.push("Seçilen dosyanın boyutu istenilenden daha büyük. Dosya boyutu en fazla " + $$("#" + e.target.id).attr("filesize") / (1024 * 1024) + " MB olabilir.");
    ////        }
    ////        if (uploadErrors.length > 0) {
    ////            alert(uploadErrors.join("\n"));
    ////        } else {
    ////            data.submit();
    ////        }
    ////    },
    ////    global: false,
    ////    dataType: "json",
    ////    disableImageMetaDataLoad: true,
    ////    url: "/Common/UploadFiles",
    ////    autoUpload: true,
    ////    done: function (e, data) {
    ////        var bindingName = $$("#" + e.target.id).attr("bindingname");
    ////        clearSelectedFileValuesFileUpload(e.target.id);
    ////        $$("#" + e.target.id + "_Adi").val(data.result.name);
    ////        $$("#" + e.target.id + "_Adi").attr("value", data.result.name);
    ////        $$("#" + e.target.id + "_FileName").append("<input type=\"hidden\" id=\"" + e.target.id + "_FizikselAdres\" name=\"" + bindingName + ".FizikselAdres\" value=\"" + data.result.guidName + "\"/>" +
    ////            "<input type=\"hidden\" id=\"" + e.target.id + "_DokumanBuyuklugu\" name=\"" + bindingName + ".DokumanBuyuklugu\" value=\"" + data.result.length + "\"/>" +
    ////            "<input type=\"hidden\" id=\"" + e.target.id + "_DokumanTipi\" name=\"" + bindingName + ".DokumanTipi\" value=\"" + data.result.type + "\"/>" +
    ////            "<input type=\"hidden\" id=\"" + e.target.id + "_Id\" name=\"" + bindingName + ".Id\" value=\"0\"/>");
    ////    },
    ////    error: function (xhr, ajaxOptions, thrownError) {
    ////        araadHandleAjaxError(xhr, ajaxOptions, thrownError);
    ////    },
    ////    fail: function (e, data) {
    ////        clearSelectedFileUpload(e.target.id)
    ////    }
    ////}).on("fileuploadprogressall", function (e, data) {
    ////    var progress = parseInt(data.loaded / data.total * 100, 10);
    ////    $$("#" + e.target.id + "_Progress").css("width", progress + "%");
    ////});
}

function clearSelectedFileValuesFileUpload(targetId) {
    $$("#" + targetId + "_Adi").val("");
    $$("#" + targetId + "_Adi").attr("value", "");
    $$("#" + targetId + "_FizikselAdres").remove();
    $$("#" + targetId + "_DokumanBuyuklugu").remove();
    $$("#" + targetId + "_DokumanTipi").remove();
    $$("#" + targetId + "_Id").remove();
}

function clearSelectedFileUpload(targetId) {

    $$("#" + targetId + "_Download").remove();
    $$("#" + targetId + "_FileUploadArea").css("display", "block");
    $$("#" + targetId + "_ProgressBarArea").css("display", "block");
    $$("#" + targetId + "_Adi").val("");
    $$("#" + targetId + "_Adi").attr("value", "");
    $$("#" + targetId + "_FizikselAdres").remove();
    $$("#" + targetId + "_DokumanBuyuklugu").remove();
    $$("#" + targetId + "_DokumanTipi").remove();
    $$("#" + targetId + "_Id").remove();
    $$("#" + targetId + "_Progress").css("width", "0%").css("aria-valuenow", "0");
}

function CreateRow(bodyId, rowId) {
    var newRow = document.createElement("TR");
    newRow.setAttribute("id", rowId);
    document.getElementById(bodyId).appendChild(newRow);
}
function CreateColumnAndText(bodyId, rowId, innerText, listName, columnName, dropDownValue, innerTextExtension) {
    var newColumn = document.createElement("TD");
    if (dropDownValue == 0)
        innerText = "";
    var totalText = "";
    if (innerTextExtension == "undefined" || innerTextExtension == "" || innerTextExtension == null) {
        totalText = innerText;
    }
    else {
        totalText = innerText + " " + innerTextExtension;
    }
    var innerTextNode = document.createTextNode(totalText);
    newColumn.appendChild(innerTextNode);

    if (listName) {
        var inputElement = document.createElement("input");
        inputElement.type = "hidden";


        //var inputType = document.createAttribute("type");
        //inputType.value = "hidden";
        //inputElement.setAttributeNode(inputType);



        var inputName = document.createAttribute("name");
        inputName.value = listName + "[" + rowId + "]." + columnName;
        inputElement.setAttributeNode(inputName);

        var inputValue = document.createAttribute("value");
        if (typeof dropDownValue === "undefined" || dropDownValue === null) {
            inputValue.value = innerText;
        }
        else {
            inputValue.value = dropDownValue;
        }

        inputElement.setAttributeNode(inputValue);

        newColumn.appendChild(inputElement);
    }

    $("#" + bodyId).find("#" + rowId).append(newColumn);
}
function AppendToLastColumn(bodyId, rowId, innerText, listName, columnName) {
    var inputElement = document.createElement("input");
    inputElement.type = "hidden";
    //var inputType = document.createAttribute("type");
    //inputType.value = "hidden";
    //inputElement.setAttributeNode(inputType);

    var inputName = document.createAttribute("name");
    inputName.value = listName + "[" + rowId + "]." + columnName;
    inputElement.setAttributeNode(inputName);

    var inputValue = document.createAttribute("value");
    inputValue.value = innerText;
    inputElement.setAttributeNode(inputValue);

    $("#" + bodyId).find("#" + rowId + " td:last").append(inputElement);
}
function CreateNoSearchResult(bodyId, columCount) {
    CreateRow(bodyId, "emptyRow");
    var newColumn = document.createElement("TD");
    var innerTextNode = document.createTextNode("Sonuç Bulunamadı");
    newColumn.appendChild(innerTextNode);
    newColumn.colSpan = columCount;
    $("#" + bodyId).find("#" + "emptyRow").append(newColumn);
}
function deleteRow(rowId, bodyId, empytListBodyId) {
    $("#" + bodyId).find("#" + rowId).remove();
    var rowCount = 0;
    var inputName;
    $("#" + bodyId + "> tr").each(function () {
        $(this).attr("id", rowCount);
        $(this).find("td input:button").attr("onclick", "deleteRow(" + rowCount + ",\"" + bodyId + "\")");
        $(this).find("td input:hidden").each(function () {
            inputName = $(this).attr("name");
            $(this).attr("name", inputName.replace(/\[{1}[^\]]+\]{1}/ig, "[" + rowCount + "]"));
        });
        $(this).find("td input:text").each(function () {
            inputName = $(this).attr("name");
            $(this).attr("name", inputName.replace(/\[{1}[^\]]+\]{1}/ig, "[" + rowCount + "]"));
        });
        rowCount++;
    });
    if (empytListBodyId) {
        $("#" + empytListBodyId).empty();
    }
}
function scrollToId(elementId) {
    var anchor = document.getElementById(elementId);
    anchor.scrollIntoView(true);
}
function removeParam(key, sourceURL) {
    var rtn = sourceURL.split("?")[0],
        param,
        params_arr = [],
        queryString = (sourceURL.indexOf("?") !== -1) ? sourceURL.split("?")[1] : "";
    if (queryString !== "") {
        params_arr = queryString.split("&");
        for (var i = params_arr.length - 1; i >= 0; i -= 1) {
            param = params_arr[i].split("=")[0];
            if (param === key) {
                params_arr.splice(i, 1);
            }
        }
        rtn = rtn + "?" + params_arr.join("&");
    }
    return rtn;
}
function getParameterByName(name, url) {
    if (!url) {
        url = window.location.href;
    }
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return "";
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}
function setGetParameter(paramName, paramValue) {
    var url = window.location.href;
    //var hash = location.hash;
    //url = url.replace(hash, "");

    //var gridPage = getParameterByName("grid-page");
    //if (gridPage!=null)
    //    gridPage = "&grid-page=" + gridPage;

    //url = url.replace(gridPage, "");
    url = removeParam("grid-page", url);
    url = removeParam("grid-filter", url);
    url = removeParam("grid-column", url);
    url = removeParam("grid-dir", url);

    if (url.indexOf(paramName + "=") >= 0) {
        var prefix = url.substring(0, url.indexOf(paramName));
        var suffix = url.substring(url.indexOf(paramName));
        suffix = suffix.substring(suffix.indexOf("=") + 1);
        suffix = (suffix.indexOf("&") >= 0) ? suffix.substring(suffix.indexOf("&")) : "";
        url = prefix + paramName + "=" + paramValue + suffix;
    }
    else {
        if (url.indexOf("?") < 0)
            url += "?" + paramName + "=" + paramValue;
        else
            url += "&" + paramName + "=" + paramValue;
    }
    window.location.href = url;


}
function setUpTabs(tabId, tabPrefix, varsayilanSekme) {

    var sekme = getParameterByName("sekme");

    if (sekme == null || sekme == undefined) {
        sekme = "1";
        if (varsayilanSekme != null || varsayilanSekme != undefined) {
            sekme = varsayilanSekme + "";
        }
    }

    //var aktifTabIndex = sekme.replace(tabPrefix, "") - 1;
    var aktifTabIndex = $("div.tab-pane").index($("#" + tabPrefix + sekme));

    $("#" + tabId).tabs({ active: aktifTabIndex });
    $("#" + tabPrefix + sekme).addClass("active in");

    $(function () {
        var $tabs = $("#" + tabId).tabs({
            activate: function (e, ui) {
                var thistab = ui.newPanel.attr("id");
                setGetParameter("sekme", thistab.replace(tabPrefix, ""));
            }
        });
    });

}

function $$(selector, context) {
    return jQuery(selector.replace(/(\[|\])/g, "\\$1"), context);
}

$(".leftPartial .row div:not(.notinclude)").each(function () {
    var titleText = $(this).text();
    $(this).html("<span>" + titleText + "</span>");
});

//$(".tabContainer li").on("click", function () {
//tabText = $(this).text();
//$(".tabToggleButton").html("<span>" + tabText + "<span>" + "<i class="fa fa-sort-desc"></i>");

//});

$(".leftPartial .row div:not(.notinclude)").each(function () {
    var titleText = $(this).text();
    $(this).html("<span>" + titleText + "</span>");
});

//--------------------------

//Menu Search
$(document).ready(function () {
    var availableTags = [];
    $("#left-panel nav ul li a").each(function () {
        if ($(this).attr("href") != "#") {
            var parentText = $(this).text();
            var parentLink = $(this).attr("href");
            availableTags.push({
                label: parentText,
                the_link: parentLink
            });
        }

    });

    var accentMap = {
        "á": "a",
        "ö": "o",
        "Ş": "S",
        "ı": "I",
        "İ": "i"
    };

    var normalize = function (term) {
        var ret = "";
        for (var i = 0; i < term.length; i++) {
            ret += accentMap[term.charAt(i)] || term.charAt(i);
        }
        return ret;
    };

    //$(function () {
    //    $("#my-form").autocomplete({
    //        source: function (request, response) {
    //            var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
    //            response($.grep(availableTags, function (value) {
    //                value = value.label || value.value || value;
    //                return matcher.test(value) || matcher.test(normalize(value));
    //            }));
    //        },
    //        select: function (event, ui) {
    //            location.href = ui.item.the_link;
    //        },

    //    });

    //});

});

$(".araadSolPartialHeader").on("click", function () {
    $("body").toggleClass("leftPartHide");
});

//Left Menu  Minified

$(document).on("click", ".minifyme", function () {
    if ($("body").hasClass("minified")) {
        $("body").removeClass("minified");
        document.cookie = "menuPosition=unminified; path=/";
    }
    else {
        $("body").addClass("minified");
        document.cookie = "menuPosition=minified; path=/";
    }
});


//$(function () {
//    var storage;
//    try {
//        storage = localStorage;
//        var hideMenuSession = localStorage.getItem("hideMenuSession");

//        $(".minifyme").click(function (hideMenuSession) {
//            if ($("body").hasClass("minified")) {
//                hideMenuSession = "";
//                localStorage.setItem("hideMenuSession", hideMenuSession);
//            }
//            else {
//                hideMenuSession = "minified";
//                localStorage.setItem("hideMenuSession", hideMenuSession);
//            }
//        });
//        $("body").addClass(hideMenuSession);

//    } catch (e) { }
//});

$(document).ready(function () {
    $("#left-panel nav > ul > li > a").each(function (index) {
        var $this = $(this);
        var words = $this.text().split(" ");
        var text = "";

        $.each(words, function (index) {
            text += this.substring(0, 1);
        });

        if (text.length == 1) {
            $this.prepend("<span class=\"menuIconSingle\">" + text + "</span>");
        }
        else if (text.length == 2) {
            $this.prepend("<span class=\"menuIconDouble\">" + text + "</span>");
        }
        else if (text.length == 3) {
            $this.prepend("<span class=\"menuIconTrio\">" + text + "</span>");
        }
    });
});

$(function () {
    $(".tabContainer ul li").each(function () {
        var $this = $(this);
        $this.attr("title", $this.text());
    });
});

//Double ScrollBar
//$(document).ready(function () {
//    $('.addDoubleScroll').doubleScroll();
//});

/*ekremerkekli
 * @name DoubleScroll
 * @desc displays scroll bar on top and on the bottom of the div
 * @requires jQuery
 *
 * @author Pawel Suwala - http://suwala.eu/
 * @author Antoine Vianey - http://www.astek.fr/
 * @version 0.5 (11-11-2015)
 *
 * Dual licensed under the MIT and GPL licenses:
 * https://www.opensource.org/licenses/mit-license.php
 * http://www.gnu.org/licenses/gpl.html
 * 
 * Usage:
 * https://github.com/avianey/jqDoubleScroll
 */
(function ($) {

    jQuery.fn.doubleScroll = function (userOptions) {

        // Default options
        var options = {
            contentElement: undefined, // Widest element, if not specified first child element will be used
            scrollCss: {
                'overflow-x': 'auto',
                'overflow-y': 'hidden'
            },
            contentCss: {
                'overflow-x': 'auto',
                'overflow-y': 'hidden'
            },
            onlyIfScroll: true, // top scrollbar is not shown if the bottom one is not present
            resetOnWindowResize: false, // recompute the top ScrollBar requirements when the window is resized
            timeToWaitForResize: 30 // wait for the last update event (usefull when browser fire resize event constantly during ressing)
        };

        $.extend(true, options, userOptions);

        // do not modify
        // internal stuff
        $.extend(options, {
            topScrollBarMarkup: '<div class="doubleScroll-scroll-wrapper" style="height: 20px;"><div class="doubleScroll-scroll" style="height: 20px;"></div></div>',
            topScrollBarWrapperSelector: '.doubleScroll-scroll-wrapper',
            topScrollBarInnerSelector: '.doubleScroll-scroll'
        });

        var _showScrollBar = function ($self, options) {

            if (options.onlyIfScroll && $self.get(0).scrollWidth <= $self.width()) {
                // content doesn't scroll
                // remove any existing occurrence...
                $self.prev(options.topScrollBarWrapperSelector).remove();
                return;
            }

            // add div that will act as an upper scroll only if not already added to the DOM
            var $topScrollBar = $self.prev(options.topScrollBarWrapperSelector);

            if ($topScrollBar.length == 0) {

                // creating the scrollbar
                // added before in the DOM
                $topScrollBar = $(options.topScrollBarMarkup);
                $self.before($topScrollBar);

                // apply the css
                $topScrollBar.css(options.scrollCss);
                $self.css(options.contentCss);

                // bind upper scroll to bottom scroll
                $topScrollBar.bind('scroll.doubleScroll', function () {
                    $self.scrollLeft($topScrollBar.scrollLeft());
                });

                // bind bottom scroll to upper scroll
                var selfScrollHandler = function () {
                    $topScrollBar.scrollLeft($self.scrollLeft());
                };
                $self.bind('scroll.doubleScroll', selfScrollHandler);
            }

            // find the content element (should be the widest one)	
            var $contentElement;

            if (options.contentElement !== undefined && $self.find(options.contentElement).length !== 0) {
                $contentElement = $self.find(options.contentElement);
            } else {
                $contentElement = $self.find('>:first-child');
            }

            // set the width of the wrappers
            $(options.topScrollBarInnerSelector, $topScrollBar).width($contentElement.outerWidth());
            $topScrollBar.width($self.width());
            $topScrollBar.scrollLeft($self.scrollLeft());

        }

        return this.each(function () {

            var $self = $(this);

            _showScrollBar($self, options);

            // bind the resize handler 
            // do it once
            if (options.resetOnWindowResize) {

                var id;
                var handler = function (e) {
                    _showScrollBar($self, options);
                };

                $(window).bind('resize.doubleScroll', function () {
                    // adding/removing/replacing the scrollbar might resize the window
                    // so the resizing flag will avoid the infinite loop here...
                    clearTimeout(id);
                    id = setTimeout(handler, options.timeToWaitForResize);
                });

            }

        });
    }

}(jQuery));

function exportToExcelFromGrid(tempGrid) {
    var _tempgrid = null;
    if ($(tempGrid).find('[hideExcel="true"]').length > 0) {
        _tempgrid = $(tempGrid).clone();
        $(tempGrid).find('[hideExcel="true"]').remove();
    }

    $(tempGrid).find('table').removeAttr('class');
    $(tempGrid).find('table td a').remove();
    $(tempGrid).find('a[href*="grid-column"]').each(function () {
        $(this).attr('href', "");
    });

    $(tempGrid).table2excel({
        name: "Excel Document Name",
        filename: $('.the-legend:first').text(),
    });

    if (_tempgrid != null) {
        $(tempGrid).html("").append(_tempgrid);
    }
}
//Double ScrollBar
function getQueryParams(name) {
    qs = location.search;

    var params = [];
    var tokens;
    var re = /[?&]?([^=]+)=([^&]*)/g;

    while (tokens = re.exec(qs)) {
        if (decodeURIComponent(tokens[1]) == name)
            params.push(decodeURIComponent(tokens[2]));
    }

    return params;
}

function exportToExcel(pagingEnabled, httpMethod, gridName) {

    if (pagingEnabled == 'true') {
        var currentUrl = window.location.pathname;
        var grid_filter = getQueryParams("grid-filter");

        var grid_filterParam = '';
        grid_filter.forEach(function (item) {
            grid_filterParam += "&grid-filter=" + item;
        });

        var data = $("form").serialize();

        if (data.indexOf('?') > 0) {
            data = data + "&";
        } else {
            data = data + "?";
        }

        data = data + "disableGridPaging=1" + grid_filterParam;

        if (httpMethod == 'POST') {
            $.ajax({
                type: "POST",
                data: data,
                url: currentUrl,
                success: function (data) {
                    var result = $(data).find('[data-gridname = \'' + gridName + '\' ]').find('.grid-wrap');
                    exportToExcelFromGrid(result);
                }
            });
        }
        else {
            var url = currentUrl + location.search;

            if (url.indexOf('?') > 0) {
                url = url + "&";
            } else {
                url = url + "?";
            }
            url = url + "disableGridPaging=1" + grid_filterParam;

            $.ajax({
                type: "GET",
                url: url,
                success: function (data) {
                    var result = $(data).find('[data-gridname = \'' + gridName + '\' ]').find('.grid-wrap');
                    exportToExcelFromGrid(result);
                }
            });
        }
    }
    else {

        var tempGrid = $('[data-gridname = \'' + gridName + '\' ]').find('.grid-wrap').clone();
        exportToExcelFromGrid(tempGrid);
    }
}

function CopyToClipBoard(elId) {
    var el = document.getElementById(elId);
    var body = document.body, range, sel;

    if (document.createRange && window.getSelection) {
        range = document.createRange();
        sel = window.getSelection();
        sel.removeAllRanges();
        try {
            range.selectNodeContents(el);
            sel.addRange(range);
        } catch (e) {
            range.selectNode(el);
            sel.addRange(range);
        }

        document.execCommand("copy");

    } else if (body.createTextRange) {
        range = body.createTextRange();
        range.moveToElementText(el);
        range.select();
        document.execCommand("copy");
    }

    if (window.getSelection) {
        window.getSelection().removeAllRanges();
    }
    else if (document.selection) {
        document.selection.empty();
    }
}


