/*
jQuery(window).load(function () {

    "use strict";
    // Page Preloader
    window.setTimeout(function () {
        App.unblockUI();
    }, 100);

    $(".tcheckers").click(function () {
        if ($(this).find("input[type='checkbox']").is(':checked')) {
            $(this).find("input[type='checkbox']").prop("checked", false);
        }
        else {
            $(this).find("input[type='checkbox']").prop("checked", true);
        }
    });
});
*/

//-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/-/ NOTİFİCATİON

function SuccessToast(message) { 
    ToastrDemo.showtoast(message, "success");
}

function DangerToast(message) {
    ToastrDemo.showtoast(message, "error");
}

function SuccessAlert(message) {       
    $("#notification,#notificationModal").show();
    $("#notification,#notificationModal").removeClass("alert alert-info");
    $("#notification,#notificationModal").removeClass("alert alert-danger");
    $("#notification,#notificationModal").removeClass("alert alert-warning");
    $("#notification,#notificationModal").addClass("alert alert-success");
    $("#notification,#notificationModal").html("<strong>Success ! </strong> " + message);
    $("#notification,#notificationModal").delay(5000).fadeOut(400);    
}

function SuccessAlertModal(message, id) {
    $("#"+id).show();
    $("#"+id).removeClass("alert alert-info");
    $("#"+id).removeClass("alert alert-danger");
    $("#"+id).removeClass("alert alert-warning");
    $("#"+id).addClass("alert alert-success");
    $("#"+id).html("<strong>Başarılı ! </strong> " + message);
    $("#"+id).delay(5000).fadeOut(400);
}
function WarningAlert(message) {
    $("#notification,#notificationModal").show();
    $("#notification,#notificationModal").removeClass("alert alert-info");
    $("#notification,#notificationModal").removeClass("alert alert-danger");
    $("#notification,#notificationModal").removeClass("alert alert-success");
    $("#notification,#notificationModal").addClass("alert alert-warning");
    $("#notification,#notificationModal").html("<strong>Warning ! </strong> " + message);
    $("#notification,#notificationModal").delay(5000).fadeOut(400);
}

function WarningAlertModal(message,id) {
    $("#"+id).show();
    $("#"+id).removeClass("alert alert-info");
    $("#"+id).removeClass("alert alert-danger");
    $("#"+id).removeClass("alert alert-success");
    $("#"+id).addClass("alert alert-warning");
    $("#"+id).html("<strong>Uyarı !</strong> " + message);
    $("#"+id).delay(5000).fadeOut(400);
}

function DangerAlert(message) {
    $("#notification,#notificationModal").show();
    $("#notification,#notificationModal").removeClass("alert alert-info");
    $("#notification,#notificationModal").removeClass("alert alert-warning");
    $("#notification,#notificationModal").removeClass("alert alert-success");
    $("#notification,#notificationModal").addClass("alert alert-danger");
    $("#notification,#notificationModal").html("<strong>Error ! </strong> " + message);
    $("#notification,#notificationModal").delay(5000).fadeOut(400);
}

function DangerAlertModel(message,id) {
    $("#"+id).show();
    $("#"+id).removeClass("alert alert-info");
    $("#"+id).removeClass("alert alert-warning");
    $("#"+id).removeClass("alert alert-success");
    $("#"+id).addClass("alert alert-danger");
    $("#"+id).html("<strong>Hata !</strong> " + message);
    $("#"+id).delay(5000).fadeOut(400);
}

function InfoAlert(message) {
    $("#notification,#notificationModal").show();
    $("#notification,#notificationModal").removeClass("alert alert-danger");
    $("#notification,#notificationModal").removeClass("alert alert-warning");
    $("#notification,#notificationModal").removeClass("alert alert-success");
    $("#notification,#notificationModal").addClass("alert alert-info");
    $("#notification,#notificationModal").html("<strong>Bilgi ! </strong> " + message);
}

function AlertMessage(message){
    $("#dialog-confirm p").html(message);
    $("#dialog-confirm").dialog({
        resizable: false,
        height: "auto",
        width: 400,
        modal: true,
        buttons: [{
            text: "Tamam",
            click: function () {
                $(this).dialog("close");
            }
        }]
    });
}

function AlertMessageOrder(message) {
    $("#dialog-confirm p").html(message);
    $("#dialog-confirm").dialog({
        resizable: false,
        height: 200,
        width: 400,
        modal: false,
        buttons: [{
            text: "Tamam",
            click: function () {
                $(this).dialog("close");
            }
        }]
    });
}


/*
function showtoastMessage() {
    console.log("aaaaaaa");
    var toastCount = 0;
    var $toastlast;
    var shortCutFunction = "success";
    var msg = "qqqqqqqqq";
    var title = "aaaaaa";
    var $showDuration = 1000;
    var $hideDuration = 1000;
    var $timeOut = 5000;
    var $extendedTimeOut = 1000;
    var $showEasing = "linear";
    var $hideEasing = "linear";
    var $showMethod = "show";
    var $hideMethod = "hide";
    var toastIndex = toastCount++;

    toastr.options = {
        closeButton: $('#closeButton').prop('checked'),
        debug: $('#debugInfo').prop('checked'),
        positionClass: $('#positionGroup input:checked').val() || 'toast-top-right',
        onclick: null
    };

    if ($('#addBehaviorOnToastClick').prop('checked')) {
        toastr.options.onclick = function () {
            alert('You can perform some custom action after a toast goes away');
        };
    }

    if ($showDuration.val().length) {
        toastr.options.showDuration = $showDuration.val();
    }

    if ($hideDuration.val().length) {
        toastr.options.hideDuration = $hideDuration.val();
    }

    if ($timeOut.val().length) {
        toastr.options.timeOut = $timeOut.val();
    }

    if ($extendedTimeOut.val().length) {
        toastr.options.extendedTimeOut = $extendedTimeOut.val();
    }

    if ($showEasing.val().length) {
        toastr.options.showEasing = $showEasing.val();
    }

    if ($hideEasing.val().length) {
        toastr.options.hideEasing = $hideEasing.val();
    }

    if ($showMethod.val().length) {
        toastr.options.showMethod = $showMethod.val();
    }

    if ($hideMethod.val().length) {
        toastr.options.hideMethod = $hideMethod.val();
    }

    if (!msg) {
        msg = getMessage();
    }

    $("#toastrOptions").text("Command: toastr[" + shortCutFunction + "](\"" + msg + (title ? "\", \"" + title : '') + "\")\n\ntoastr.options = " + JSON.stringify(toastr.options, null, 2));

    var $toast = toastr[shortCutFunction](msg, title); // Wire up an event handler to a button in the toast, if it exists
    $toastlast = $toast;
    if ($toast.find('#okBtn').length) {
        $toast.delegate('#okBtn', 'click', function () {
            alert('you clicked me. i was toast #' + toastIndex + '. goodbye!');
            $toast.remove();
        });
    }
    if ($toast.find('#surpriseBtn').length) {
        $toast.delegate('#surpriseBtn', 'click', function () {
            alert('Surprise! you clicked me. i was toast #' + toastIndex + '. You could perform an action here.');
        });
    }

    $('#clearlasttoast').click(function () {
        toastr.clear($toastlast);
    });
}
*/