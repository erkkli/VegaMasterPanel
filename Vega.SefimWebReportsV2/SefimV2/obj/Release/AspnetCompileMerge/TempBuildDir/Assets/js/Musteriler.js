var DatatableHtmlTableGrup = function () {
    //== Private functions

    // demo initializer
    var demo = function () {

        var datatable = $('#musteri_grup_datatable').mDatatable({
            data: {
                saveState: { cookie: false },
            },
            search: {
                input: $('#generalSearchGrup'),
            },
            columns: [
                {
                    field: 'Deposit Paid',
                    type: 'number',
                },
                {
                    field: 'Order Date',
                    type: 'date',
                    format: 'YYYY-MM-DD',
                }, {
                    field: 'Status',
                    title: 'Status',
                    // callback function support for column rendering
                    template: function (row) {
                        var status = {
                            1: { 'title': 'Pending', 'class': 'm-badge--brand' },
                            2: { 'title': 'Delivered', 'class': ' m-badge--metal' },
                            3: { 'title': 'Canceled', 'class': ' m-badge--primary' },
                            4: { 'title': 'Success', 'class': ' m-badge--success' },
                            5: { 'title': 'Info', 'class': ' m-badge--info' },
                            6: { 'title': 'Danger', 'class': ' m-badge--danger' },
                            7: { 'title': 'Warning', 'class': ' m-badge--warning' },
                        };
                        return '<span class="m-badge ' + status[row.Status].class + ' m-badge--wide">' + status[row.Status].title + '</span>';
                    },
                }, {
                    field: 'Type',
                    title: 'Type',
                    // callback function support for column rendering
                    template: function (row) {
                        var status = {
                            1: { 'title': 'Online', 'state': 'danger' },
                            2: { 'title': 'Retail', 'state': 'primary' },
                            3: { 'title': 'Direct', 'state': 'accent' },
                        };
                        return '<span class="m-badge m-badge--' + status[row.Type].state + ' m-badge--dot"></span>&nbsp;<span class="m--font-bold m--font-' +
                            status[row.Type].state + '">' +
                            status[row.Type].title + '</span>';
                    },
                },
            ],
        });

        $('#m_form_status').on('change', function () {
            datatable.search($(this).val().toLowerCase(), 'Status');
        });

        $('#m_form_type').on('change', function () {
            datatable.search($(this).val().toLowerCase(), 'Type');
        });

        $('#m_form_status, #m_form_type').selectpicker();

    };

    return {
        //== Public functions
        init: function () {
            // init dmeo
            demo();
        },
    };
}();




var BootstrapSelect = function () {

    //== Private functions
    var demos = function (id) {
        // minimum setup
        $('#' + id).selectpicker();
    }

    return {
        // public functions
        init: function (id) {
            demos(id);
        }
    };
}();


jQuery(document).ready(function () {
    DatatableHtmlTableGrup.init();
    BootstrapSelect.init("selectfilter_insert");
    BootstrapSelect.init("harcama");
});



function openModalinsert() {

    document.getElementById("insertModal").click();

}


function detayModalOpenInsertMessage(message) {
    if (message != "") {
        document.getElementById("manualModalMessage").click();
        document.getElementById("detay_modal_message").innerHTML = message;
        document.getElementById("detay_modal_message_title").innerHTML = "Veri ekleme!";

    }



}


function openModalUpdate(modalid) {

    document.getElementById("updateModal").click();
    initData(modalid);
}

function initData(modalid) {

    var parentModal = document.getElementById("m_modal_update");
    var parentData = document.getElementById("row_update_" + modalid);

    parentModal.querySelector("#idedit").value = parentData.querySelector("#idedit").value;
    parentModal.querySelector("#grupismi").value = parentData.querySelector("#groupname").value;


    //parentModal.querySelector("#durum").value = parentData.querySelector("#status").value;
    var ch = parentData.querySelector("#status").value == "True" ? true : false;
    $(parentModal.querySelector("#durum")).prop("checked", ch);
    //$(parentModal.querySelector("#durum")).val("checked", ch).trigger('change');


    // parentModal.querySelector("#harcama").value = parentData.querySelector("#criter").value;
    // $(parentModal.querySelector("#harcama")).val(parentData.querySelector("#criter").value);
    // $(parentModal.querySelector("#harcama")).val(parentData.querySelector("#criter").value).attr("selected", "selected");
    $(parentModal.querySelector("#harcama")).val(parentData.querySelector("#criter").value).attr("selected", "selected");
    $(parentModal.querySelector("#harcama")).val(parentData.querySelector("#criter").value).trigger('change');
    $(parentModal.querySelector("#harcama")).selectpicker('refresh')
}



function detayModalOpenUpdateMessage(message) {
    if (message != "") {
        document.getElementById("manualModalMessage").click();
        document.getElementById("detay_modal_message").innerHTML = message;
        document.getElementById("detay_modal_message_title").innerHTML = "Veri güncelleme!";

    }



}


function openModalDelete(musteriid) {

    document.getElementById("deletedid").value = musteriid;
    document.getElementById("manualModalSil").click();


}

function openModalDeleteSuccess() {

    document.getElementById("deletedform").submit();

}

function openModalQr(modalid) {

    document.getElementById("qrpartialcontainer").innerHTML = "";

    $.ajax({
        type: "POST",
        async: true,
        url: '/settings/musterigrubuQrPartial',
        data: { groupid: modalid },
        success: function (partialView) {
            var maincont = document.getElementById("qrpartialcontainer");
            //$(maincont).fadeOut();
            maincont.innerHTML = "";
            $(maincont).append(partialView);


        }
    });
}