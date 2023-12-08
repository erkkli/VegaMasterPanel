$(function () {
    $(document).on('CloseModal', function () {
        $("#ModalClose").trigger("click");
    });
});

$(document).ready(function () {

    $(document).on("click", '.close', function (e) {

        var Control = $("#iframeidx").contents().find("#Control").val();
        if (Control == 1) {
            var resultControl = $("#iframeidx").contents().find("#resultControl").val();
            $("#dialog").dialog({
                title: "Process Confirm",
                buttons: {
                    "Save & Close": function () {                                                
                        $("#iframeidx").contents().find("#FormSend").trigger("click");
                        resultControl = $("#iframeidx").contents().find("#resultControl").val();
                        $("#dialog").dialog("close");

                        if (resultControl == 1) {
                            window.setTimeout(function () {
                                parent.$(parent.document).trigger('CloseModal');
                                //$('#iframeidx').location.reload(true);
                            }, 1000);
                        }
                    },
                    "Cancel": function () {
                        $(this).dialog("close");                            
                        parent.$(parent.document).trigger('CloseModal');
                        //$('#iframeidx').location.reload(true);                            
                    }
                }
            });
            $("#dialog").text("Değişiklik Yaptınız Kaydetmek İstiyor musunuz?");
            $("#dialog").dialog("open");
        } else {
            $('#iframeidx').attr('src', '');
            parent.$(parent.document).trigger('CloseModal');
            $('#iframeidx').location.reload(true);
        }
                      
    });

    $(document).on("click", '.modalPage', function (e) {

        var height = $(window).height() - 100;
        $(".modal-body").css("height", height);

        //$('body').css('overflow', 'hidden');

        var iframeSrc = $(this).attr("href");
        $('#iframeidx').attr("src", iframeSrc);

        $('.modal').modal({
            show: true,
            backdrop: 'static',
            keyboard: false,
            close: function (event, ui) {
                $('#iframeidx').attr('src', '');
                $('#iframeidx').location.reload(true);
            }
        });
        $('.ModelOut').modal('hide');
        $('#iframeidx').load(function () {
            $('.loading').hide();
        });
        e.preventDefault();
        return false;
    });
  
});