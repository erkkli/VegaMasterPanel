
function Init_Upload() {
    $('#FormUpload input[name=UploadedFile]').change(function (evt) { singleFileSelected(evt); });
    $("#FormUpload button[id=Cancel_btn]").click(function () {
        DeleteFile();
    });
    $('#FormUpload button[id=Submit_btn]').click(function () {
        UploadFile();
    });
    //$.blockUI.defaults.overlayCSS = {
    //    backgroundColor: '#000',
    //    opacity: 0.6
    //};
    //$.blockUI.defaults.css = {
    //    padding: 0,
    //    margin: 5,
    //    width: '50%',
    //    top: '30%',
    //    left: '25%',
    //    color: '#000',
    //    border: '3px solid #aaa',
    //    backgroundColor: '#fff'
    //};
    //$.blockUI({ message: $('#createView') });
}
function singleFileSelected(evt) {
    //var selectedFile = evt.target.files can use this  or select input file element and access it's files object
    var selectedFile = ($("#UploadedFile"))[0].files[0];//FileControl.files[0];
    if (selectedFile) {
        var FileSize = 0;
        var imageType = /image.*/;
        if (selectedFile.size > 1048576) {
            FileSize = Math.round(selectedFile.size * 100 / 1048576) / 100 + " MB";
        }
        else if (selectedFile.size > 1024) {
            FileSize = Math.round(selectedFile.size * 100 / 1024) / 100 + " KB";
        }
        else {
            FileSize = selectedFile.size + " Bytes";
        }

        if (selectedFile.type.match(imageType)) {
            var reader = new FileReader();
            reader.onload = function (e) {

                $("#Imagecontainer").empty();
                var dataURL = reader.result;
                var img = new Image()
                img.src = dataURL;
                img.className = "thumb";
                $("#Imagecontainer").append(img);
            };
            reader.readAsDataURL(selectedFile);
        }
        $("#FileName").html("Name : <span id='spanFileName'>" + selectedFile.name + "</span>");
        $("#FileType").text("type : " + selectedFile.type);
        $("#FileSize").text("Size : " + FileSize);
    }
}
function UploadFile() {
    var form = $('#UploadedFile')[0].files[0];
    var dataString = new FormData();
    dataString.append("upload", form);
    dataString.append("StokKodu", $("#selectStokKodu").find(":selected").attr("data-text"));

    $.ajax({
        url: '/Product/UploadProductImage',  //Server script to process data
        type: 'POST',
        xhr: function () {  // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // Check if upload property exists
                //myXhr.upload.onprogress = progressHandlingFunction
                myXhr.upload.addEventListener('progress', progressHandlingFunction, false); // For handling the progress of the upload
            }
            return myXhr;
        },
        //Ajax events
        success: successHandler_Upload,
        error: errorHandler,
        complete: completeHandler,
        // Form data
        data: dataString,
        //Options to tell jQuery not to process data or worry about content-type.
        cache: false,
        contentType: false,
        processData: false,
        dataType: 'json'
    });

}
function DeleteFile() {
    var form = $('#FormUpload')[0];
    var dataString = new FormData(form);
    $.ajax({
        url: '/Product/DeleteProductImage',  //Server script to process data
        type: 'POST',
        xhr: function () {  // Custom XMLHttpRequest
            var myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // Check if upload property exists
                //myXhr.upload.onprogress = progressHandlingFunction
                myXhr.upload.addEventListener('progress', progressHandlingFunction, false); // For handling the progress of the upload
            }
            return myXhr;
        },
        //Ajax events
        success: successHandler_Delete,
        error: errorHandler,
        complete: completeHandler,
        // Form data
        data: dataString,
        //Options to tell jQuery not to process data or worry about content-type.
        cache: false,
        contentType: false,
        processData: false
    });
}
function successHandler_Delete(data) {
    if (data.statusCode === 200) {
        var _src = $("#imgUrun").attr("src");
        $("#imgUrun").attr("data-src", _src);
        alert(data.status);
        ClearUpload();
    }
    else {
        alert(data.status);
    }
}
function ClearUpload() {
    $("#Imagecontainer").html(" ");
    $("#FileName").html(" ");
    $("#FileType").html(" ");
    $("#FileSize").html(" ");
    $("#FileProgress").addClass("hide");
}
function successHandler_Upload(data) {
    if (data.statusCode === 200) {
        $("#imgUrun").attr("src", data.NewRow).attr("data-src", data.NewRow);
        alert(data.status);
    }
    else {
        alert(data.status);
    }
}

