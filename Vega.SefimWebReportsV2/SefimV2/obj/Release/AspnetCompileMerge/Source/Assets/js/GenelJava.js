
var LoadingOptionTag = "<option value='' selected disabled>Yükleniyor...</option>";
var MaxFileSize = 100;
var TotalFileSize = 0;
var fd = null;

$(document).ready(function () {
    DocumentReadyEvents();
    AJaxListeControl();
    KeyPressEvent();
    FormKurulum();
    FormAyar();
    FormSearchEvents();
    PopupLinkSetUp();
    LeftMenuHeight();
});

$(window).load(function () {
    WindowLoadEvents();
    FormAyar();
    LeftMenuHeight();
});

$(window).resize(function () {
    FormAyar();
    LeftMenuHeight();
});

//$.validator.methods.email = function (value, element) {
//    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
//    return this.optional(element) || re.test(value);
//}

//$.extend($.validator.messages, {
//    required: "Bu Alan Zorunludur.",
//    multiple: "Lütfen Seçiniz.",
//    remote: "Please fix this field.",
//    email: "Lütfen geçerli bir e-posta adresi giriniz.",
//    url: "Lütfen geçerli bir web adresi giriniz.",
//    date: "Lütfen geçerli bir tarih giriniz.",
//    dateISO: "Lütfen tarih ISO formatında giriniz.",
//    number: "Lütfen geçerli bir sayı giriniz.",
//    digits: "Please enter only digits.",
//    creditcard: "Lütfen geçerli bir kart numarası giriniz.",
//    equalTo: "Girdiğiniz bilgiler eşleşmiyor.",
//    accept: "Sadece izin verilen dosya biçimlerini yükleyebilirsiniz.",
//    maxlength: $.validator.format("En Fazla {0} karakter uzunluğunda olmalıdır."),
//    minlength: $.validator.format("En Az {0} karakter uzunluğunda olmalıdır."),
//    rangelength: $.validator.format("Girdiğiniz bilgiler {0} il {1} karakter uzunluğunda olmalıdır."),
//    range: $.validator.format("{0} ile {1} arasında bir değer giriniz."),
//    max: $.validator.format("Maximum {0} Girebilirsiniz."),
//    min: $.validator.format("Minimum {0} Girebilirsiniz.")
//});


//$.validator.setDefaults({
//    errorPlacement: function (error, element) {
//        element.closest(".form-group").append(error);
//    }
//});


function PopupLinkSetUp() {
    if (typeof popup != "undefined") {
        if (popup == true) {
            $("a").each(function () {
                var url = $(this).attr("href");
                if (url != null && url != "" && url.indexOf("#") < 0) { if (url.indexOf("popup") < 0) { $(this).attr("href", url + "/popup"); } }
            });
        }
    }
}


var ListSort = "";
var ListSortType = "";

function SearhFormData() {
    var FormData = "";
    $("form#form-search input").each(function () { FormData = FormData + "[#]" + $(this).attr("name") + "[=]" + $(this).val(); });
    if ($("form#form-search select").length > 0) { $("form#form-search select").each(function () { FormData = FormData + "[#]" + $(this).attr("name") + "[=]" + $(this).val(); }); }
    if ($("form#form-search textarea").length > 0) { $("form#form-search textarea").each(function () { FormData = FormData + "[#]" + $(this).attr("name") + "[=]" + $(this).val(); }); }
    return FormData;
}

function LeftMenuHeight() {
    var WH = parseInt($("body").outerHeight());
    $(".left_col").css({ "min-height": "" + WH + "px" });
}

function ListSortEvent(tip, obj) {

    if (ListSortType == "") { ListSortType = "asc"; } else {
        if (ListSort == tip) {
            if (ListSortType == "asc") { ListSortType = "desc"; }
            else if (ListSortType == "desc") { ListSortType = "asc"; }
        } else { ListSortType = "asc"; }
    }
    ListSort = tip;
    $("form#form-search input[name='sort']").val(ListSort);
    $("form#form-search input[name='sorttype']").val(ListSortType);
    $("form#form-search input[name='page']").val("1");
    Liste();
}
function ListPage(p) {
    var pbox = $("form#form-search input[name='page']");
    if (pbox.length > 0) { pbox.val(p); }
    if (typeof Liste !== "undefined") { Liste(); }
}

function ListEveryPage(ep) {
    var epbox = $("form#form-search input[name='everypage']");
    if (epbox.length > 0) { epbox.val(ep); $("form#form-search input[name='page']").val("1"); }
    if (typeof Liste !== "undefined") { Liste(); }
}

function FormSearchEvents() {

    $(".form-reset-button").click(function () {
        var durum = $(this).closest(".form-search-container").attr("class").indexOf("opened") > -1;
        if (durum == true) {
            $(this).closest(".form-search-container").removeClass("opened");
            $(this).closest(".form-search-container").find(".form-search-button").removeClass("btn-primary").addClass("btn-default");

            $("form#form-search button").addClass("btn-sm");
            $("form#form-search input[type='text']").each(function () { $(this).val(""); });
            $("form#form-search input[type='tel']").each(function () { $(this).val(""); });
            $("form#form-search input[type='email']").each(function () { $(this).val(""); });
            $("form#form-search input[type='number']").each(function () { $(this).val(""); });
            $("form#form-search input[type='date']").each(function () { $(this).val(""); });
            $("form#form-search select").each(function () { $(this).val(""); });
            $("form#form-search textarea").each(function () { $(this).val(""); });
            $("form#form-search input[type='checkbox']").each(function () { $(this).removeAttr("checked"); });

            Liste();
        }
    });

    $(document).keydown(function (e) {
        if ((e.ctrlKey || e.metaKey) && e.keyCode === 70) {
            $(".form-search-container").addClass("opened");
            $(".form-search-button").removeClass("btn-default").addClass("btn-primary");
            $("form#form-search button").removeClass("btn-sm");
        }
    });

    $(".form-search-button").click(function () {
        var durum = $(this).closest(".form-search-container").attr("class").indexOf("opened") > -1;
        if (durum == false) {
            $(this).closest(".form-search-container").addClass("opened");
            $(this).removeClass("btn-default").addClass("btn-primary");
            $("form#form-search button").removeClass("btn-sm");
        } else {
            $("form#form-search input[name='page']").val("1");
            Liste();
        }
    });

}



function TagBul(txt, s) {
    var StartTag = "<" + s + ">";
    var EndTag = "</" + s + ">";
    var sonuc = "";
    if (txt.indexOf(StartTag) > -1 && txt.indexOf(EndTag) > -1) {
        var StartTagIndex = txt.indexOf(StartTag) + StartTag.length;
        var EndTagIndex = txt.indexOf(EndTag);
        sonuc = txt.substring(StartTagIndex, EndTagIndex);
    }
    return sonuc;
}

//Select Panels
function SelectPanelsSetUp() {
    $(document).ready(function () { SelectPanelsReady(); });
    $(".onChangePanels select").change(function () { SelectPanelsChangeEvent($(this)); });
}

function SelectPanelsReady() {
    $(".onChangePanels select").each(function () {
        SelectPanelsChangeEvent($(this));
    });
}
function SelectPanelsChangeEvent(obj) {
    var tip = $(obj).attr("name");
    var val = $(obj).val();
    $(".onChangePanel." + tip + "").stop().hide();
    if (val != null && val != "") {
        $(".onChangePanel." + tip + ".pan-" + val + "").stop().fadeIn();
    }
}



function isNumeric(sText) { var ValidChars = "0123456789.,"; var IsNumber = true; var Char; for (i = 0; i < sText.length && IsNumber == true; i++) { Char = sText.charAt(i); if (ValidChars.indexOf(Char) == -1) { IsNumber = false; } } return IsNumber; }


function FormAyar() {

    //$("input[mask='tel']").mask("(000) 000 00 00", { placeholder: "(---) --- -- --" }).attr({ "maxlength": "15" });
    //$("input[mask='kartno']").mask("0000 - 0000 - 0000 - 0000", { placeholder: "0000 - 0000 - 0000 - 0000" });
    //$("input[mask='stk']").mask("00 / 0000", { placeholder: "AA / YYYY" });
    //$("input[mask='ccv']").mask("0000", { placeholder: "000" });
    //$("input[mask='date']").mask("00.00.0000", { placeholder: "gg.aa.yyyy" });
    //$("input[mask='datetime']").mask("00.00.0000 00:00", { placeholder: "01/01/1999 12:00" });
    //$("input[mask='tc']").mask("000 000 000 00").attr({ "maxlength": "14" });
    //$("input[mask='number']").mask("000000000000");
    //$("input[mask='decimal']").mask("#.##0,00", { reverse: true, placeholder: "0,00" });
    //$("input[mask='iban']").mask("AA00 - 0000 0000 0000 0000 00", { placeholder: "TR00 - 0000 - 0000 - 0000 - 0000 - 00" });
    //$("input[mask='hesapno']").mask("000 000 000 000 000 000");
    //$("input[mask='vergino']").mask("000-000-000-000");

    $(".select2_single").select2({
        placeholder: "Lütfen Seçiniz",
        allowClear: true
    });

    $(".select2_multiple").each(function () {
        var max = $(this).attr("data-maxselect");
        if (typeof max === typeof undefined || max === false) { max = 10; }
        $(this).select2({
            maximumSelectionLength: max,
            placeholder: "Lütfen Seçiniz"
        });
    });





    // select <> input data transfer
    $("select.selectInputTransfer").addClass("firstload");
    $("select.selectInputTransfer").click(function () { $(this).removeClass("firstload"); });
    $("select.selectInputTransfer").change(function () {
        var id = $(this).attr('id');
        if (typeof id !== typeof undefined && id !== false) {
            var input = $("input[name='" + id + "']");
            var select = $("select#" + id + "");
            if (input.length > 0 && select.length > 0) {
                if (select.attr("class").indexOf("firstload") < 0) { input.val(select.val()); } else {
                    var val = input.val();
                    if (val !== "" && val !== null && select.find("option").length > 0) {
                        var valueExist = false;
                        select.find("option").each(function () {
                            var thisOptionVal = $(this).attr("value");
                            if (thisOptionVal === val) { select.val(val); valueExist = true; }
                        });
                        if (valueExist === false) { input.val(""); }
                    }
                }
            }
        }
    });



    //select popup link
    var SelectModalItems = $("a[data-popup='true']");
    if (SelectModalItems.length > 0) {
        SelectModalItems.click(function () {
            var item = $(this);
            var link = $(this).attr('href');
            if (typeof link !== typeof undefined && link !== false) {
                if (link !== null && link !== "") {
                    $("#pagemodal .modal-body").html("<iframe src='" + link + "'><iframe>");
                    $("#pagemodal").modal("show");
                    $('#pagemodal').on('hidden.bs.modal', function () {
                        var select = item.closest("div.form-group").find("select");
                        if (select.length > 0) {
                            var id = select.attr('id');
                            if (typeof id !== typeof undefined && id !== false) { SelectDataFill(id); }
                        }


                    });
                }
            }
            return false;
        });
    }




    // select data fill
    var selectDataFillItems = $("select.selectDataFill");
    if (selectDataFillItems.length > 0) {
        selectDataFillItems.each(function () {
            var id = $(this).attr('id');
            if (typeof id !== typeof undefined && id !== false) { SelectDataFill(id); }
        });

    }


    $("form button.yon").click(function () {
        $(this).closest("form").find("input[name='yon']").val($(this).val());
    });
    FormTabsClickEvent();
    BoxButtonSet();
    SelectPanelsSetUp();
    IslemButton();
    KarakterSay();
    IfFilledRequired();
    PasswordIfFilledRequired();
}

function SelectDataFill(id) {
    $("select#" + id + "").html("<option value>Yükleniyor...</option>").attr("disabled", "disabled");
    $.ajax({
        url: "",
        type: "post",
        data: { "islem": "" + id + "list" },
        success: function (sonuc) {
            $("select#" + id + "").html(sonuc);
            if (sonuc.indexOf("data=\"bos\"") > -1) { $("input[name='" + id + "']").val(""); } else { $("select#" + id + "").removeAttr("disabled"); }
            $("select#" + id + "").addClass("firstload").change();
        }
    });
}

function PasswordIfFilledRequired() {
    $(document).ready(function () {
        if ($("input.password-if-filled-required").length > 0) {
            PasswordIfFilledRequiredSet();
            $("input.password-if-filled-required").keyup(function () { PasswordIfFilledRequiredSet(); });
        }
    });
}

function PasswordIfFilledRequiredSet() {
    var l = $("input#password").val().length;
    if (l > 0) {
        $("input#password").attr("required", "required");
        if ($("input#password2").length > 0) { $("input#password2").attr("required", "required"); }
    } else {
        var attr = $("input#password").attr('required'); if (typeof attr !== typeof undefined && attr !== false) { $("input#password").removeAttr("required"); }
        if ($("input#password2").length > 0) { var attr2 = $("input#password2").attr('required'); if (typeof attr2 !== typeof undefined && attr2 !== false) { $("input#password2").removeAttr("required"); } }
    }

}

function IfFilledRequired() {
    $(document).ready(function () {
        $("input.if-filled-required").each(function () {
            IfFilledRequiredSet($(this));
        });
    });
    $("input.if-filled-required").keyup(function () { IfFilledRequiredSet($(this)); });
}

function IfFilledRequiredSet(obj) {
    if ($(obj).val().length > 0) {
        $(obj).attr("required", "required");
    } else {
        var attr = $(obj).attr('required'); if (typeof attr !== typeof undefined && attr !== false) { $(obj).removeAttr("required"); }
    }
}


function KarakterSay() {
    $(".karaktersay").each(function () { KarakterSayIslem($(this)); });
    $(".karaktersay").keyup(function () { KarakterSayIslem($(this)); });
}

function KarakterSayIslem(obj) {
    var l = $(obj).val().length;
    var label = $(obj).closest(".form-group").find("label");
    if (l > 0) {
        if (label.find("span.karaktersaylabel").length < 1) {
            label.append("<span class='karaktersaylabel' style='font-weight:normal'><span class='ml-20'>Karakter Sayısı : </span><span class='say'></span></span>");
        }
    } else {
        if (label.find("span.karaktersaylabel").length > 0) { label.find("span.karaktersaylabel").remove(); }
    }
    label.find("span.say").html(l);
}

function IslemButton() {
    if ($("form button.islembutton").length > 0) {
        $("form button.islembutton").click(function () {
            var islem = $(this).attr("value");
            $("form input[name='islem']").val(islem);
        });
    }
}

function FormTabsClickEvent() {
    $("ul.nav.bar_tabs li a").click(function () {
        setTimeout(function () { BoxButtonSet(); }, 200);
    });
}

function BoxButtonSet() {
    if ($(".boxbutton").length > 0) {
        var ButtonW = parseInt($(".boxbutton").outerWidth());
        var ButtonML = parseInt($(".boxbutton").css("margin-left"));
        var ContainerW = parseInt($(".boxbutton").closest("div").outerWidth());
        var ContainerPL = parseInt($(".boxbutton").closest("div").css("padding-left"));
        var ContainerPR = parseInt($(".boxbutton").closest("div").css("padding-right"));
        ContainerW = ContainerW - ContainerPL - ContainerPR;
        var BoxW = ContainerW - ButtonW - ButtonML;
        $(".boxbutton").closest("div").find(".form-control").css({ "width": "" + BoxW + "px" });
    }
}


function DeleteFunctionCheck() {
    if (typeof DeleteFunction !== "undefined") { DeleteFunction(); }
}

function BoxButonKurulum() {

    if ($(".boxbutton").length > 0) {
        var btn = $(".boxbutton");
        var btnmenu = btn.closest("div").find(".boxbutton-menu");

        if (btnmenu.length > 0) {
            btnmenu.closest(".form-group").mouseleave(function () {
                btnmenu.fadeOut();
                $(this).find(".boxbutton").removeClass("opened");
            });
        }

        btn.click(function () {
            if (btnmenu.length > 0) {
                var cont = $(this).closest("div").find(".boxbutton-menu");
                if (cont.css("display") == "none") {
                    cont.animate({ height: "show" }, 150);
                    $(this).closest(".form-group").find(".boxbutton").addClass("opened");
                } else {
                    cont.animate({ height: "hide" }, 150);
                    $(this).closest(".form-group").find(".boxbutton").removeClass("opened");
                }
            }
        });
    }

}

function FormKurulum() {

    BoxButonKurulum();

    $(".ListDeleteButton").click(function () {

        var form = $(this).closest(".row").find(".liste");

        var len = 0;
        form.find("input[type='checkbox'][name='id']").each(function () {
            if ($(this).is(":checked") == true) { len = len + 1; }
        });
        if (len > 0) {

            form = $(this).closest(".row").find("form");
            $("#listedeletemodal").modal("show");
        } else {
            BalonAlert('alert', 'Bilgilendirme', 'Lütfen silmek istediğiniz satırların sol kısmındaki kutucuğu işaretleyiniz.');
        }
        return false;
    });







    if ($("textarea.ckeditor").length > 0) {
        $("textarea.ckeditor").each(function () {
            $(this).ckeditor({
                on: {
                    key: function (evt) { CKEDITOR.instances[$(this).attr("name")].updateElement(); },
                    change: function (evt) { CKEDITOR.instances[$(this).attr("name")].updateElement(); },
                    blur: function (evt) { CKEDITOR.instances[$(this).attr("name")].updateElement(); }
                }
            });
        });
    }

    if ($("textarea.ckeditorsmall").length > 0) {
        $("textarea.ckeditorsmall").ckeditor({
            height: "100px",
            toolbar: [
                { name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
                { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
                { name: 'links', items: ['Link', 'Unlink', 'Anchor'] }
            ],

            on: {
                key: function (evt) { CKEDITOR.instances[$(this).attr("name")].updateElement(); },
                change: function (evt) { CKEDITOR.instances[$(this).attr("name")].updateElement(); },
                blur: function (evt) { CKEDITOR.instances[$(this).attr("name")].updateElement(); }
            }
        });



    }




}




/*  78=N , 69=E , 76=L */
function KeyPressEvent() {
    $(document).keyup(function (evnt) {

        if (evnt.keyCode == 78 && evnt.shiftKey == true) {
            if ($('a.big-add-button').length > 0) {
                window.location.href = $('a.big-add-button').attr("href");
            }
        }

        if (evnt.keyCode == 69 && evnt.shiftKey == true) {
            if ($('a.big-add-button').length > 0) {
                window.location.href = $('a.big-add-button').attr("href");
            }
        }

        if (evnt.keyCode == 76 && evnt.shiftKey == true) {
            if ($('a.big-list-button').length > 0) {
                window.location.href = $('a.big-list-button').attr("href");
            }
        }
    });
}

$.fn.hasAttr = function (name) {
    return this.attr(name) !== undefined;
};

function AJaxListeControl() {
    if ($("div.ajaxliste").length > 0) {
        AjaxListe();
    }
}

function AjaxListe(pagesize, page) {
    if (pagesize == null || pagesize == "") { pagesize = "10"; }
    if (page == null || page == "") { page = "1"; }

    var url = $("div.ajaxliste").attr("data-url");
    $.ajax({
        url: url,
        type: "post",
        data: { islem: "liste", page: "" + page + "", pagesize: "" + pagesize + "" },
        success: function (sonuc) {
            $(".ajaxliste").html(sonuc);
            $(".contentloading").fadeOut();
        }
    });
}

function WindowLoadEvents() {
    $(".showafterload").delay(100).animate({ opacity: "1" }, 500);
}

function DocumentReadyEvents() {
    $("a.nolink").click(function () { return false; });
}


function BalonAlert(type, title, text) {
    //if($(".ui-pnotify").length>0){$(".ui-pnotify").remove();}
    new PNotify({
        delay: 5000,
        title: title,
        type: type,
        text: text,
        nonblock: {
            nonblock: false
        },
        addclass: 'Notice',
        styling: 'bootstrap3',
        hide: true,
        before_close: function (PNotify) {
            PNotify.update({
                title: PNotify.options.title + "",
                before_close: null
            });

            PNotify.queueRemove();
            return false;
        }
    });
}


function DetailedErrorModal() {
    $(".modal#detailederrormodal").modal("show");
}


function LogPageOpen(url) {
    $("#pagemodal .modal-body").html("<iframe src='" + url + "'><iframe>");
    $("#pagemodal").modal("show");
}

function CopyToClipboard(text) {
    $("body").append("<input id='clipboardbox' value='" + text + "' />");
    $("body").find("input#clipboardbox").focus().select();
    document.execCommand("copy");
    $("body").find("input#clipboardbox").remove();
    BalonAlert('success', 'Bilgi', 'Panoya Kopyalandı.')
}


function KodKontrol(kod) {
    if (kod != null && kod != "") {
        kod = kod.toLowerCase();
        var Silinecek = "!,',^,%,&,/,(,),=,?,_,>,<,£,#,$,½,{,[,],},\\,|,~,¨,@,æ,ß,€,´,`,*,+";
        var SB = Silinecek.split(',');
        for (var i = 0; i < SB.length; i++) {
            kod = replaceAll(kod, SB[i], '');
        }

        kod = replaceAll(kod, 'ü', 'u');
        kod = replaceAll(kod, 'ğ', 'g');
        kod = replaceAll(kod, 'ı', 'i');
        kod = replaceAll(kod, 'ş', 's');
        kod = replaceAll(kod, 'ç', 'c');
        kod = replaceAll(kod, 'ö', 'o');
        kod = replaceAll(kod, ',,,', '');
        kod = replaceAll(kod, ',,', '');
        kod = replaceAll(kod, ',', '-');
        kod = replaceAll(kod, ' ', '-');
        kod = replaceAll(kod, '---', '-');
        kod = replaceAll(kod, '---', '-');
        kod = replaceAll(kod, '--', '-');

    } else { kod = ""; }
    return kod;
}

function escapeRegExp(str) { return str.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1"); }
function replaceAll(str, find, replace) { return str.replace(new RegExp(escapeRegExp(find), 'g'), replace); }
function isnumeric(sText) { var ValidChars = "0123456789.,"; var IsNumber = true; var Char; for (i = 0; i < sText.length && IsNumber == true; i++) { Char = sText.charAt(i); if (ValidChars.indexOf(Char) == -1) { IsNumber = false; } } return IsNumber; }
function Left(str, n) { if (n <= 0) return ""; else if (n > String(str).length) return str; else return String(str).substring(0, n); }
function Right(str, n) { if (n <= 0) return ""; else if (n > String(str).length) return str; else { var iLen = String(str).length; return String(str).substring(iLen, iLen - n); } }
function FormatNumber(nStr) { nStr += ''; c = nStr.split('.'); nStr = c.join(''); x = nStr.split(','); x1 = x[0]; x2 = x.length > 1 ? ',' + x[1] : ''; var rgx = /(\d+)(\d{3})/; while (rgx.test(x1)) { x1 = x1.replace(rgx, '$1' + '.' + '$2'); } return x1 + x2.substring(0, 3); }
function validatedate(gelen) { var dateformat = /^(0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])[\/\-]\d{4}$/; if (gelen.match(dateformat)) { var opera1 = gelen.split('/'); var opera2 = gelen.split('-'); lopera1 = opera1.length; lopera2 = opera2.length; if (lopera1 > 1) { var pdate = gelen.split('/'); } else if (lopera2 > 1) { var pdate = gelen.split('-'); } var mm = parseInt(pdate[0]); var dd = parseInt(pdate[1]); var yy = parseInt(pdate[2]); var ListofDays = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]; if (mm == 1 || mm > 2) { if (dd > ListofDays[mm - 1]) { return false; } } if (mm == 2) { var lyear = false; if ((!(yy % 4) && yy % 100) || !(yy % 400)) { lyear = true; } if ((lyear == false) && (dd >= 29)) { return false; } if ((lyear == true) && (dd > 29)) { return false; } } } else { return false; } }
function myTrim(x) { return x.replace(/^\s+|\s+$/gm, ''); }
