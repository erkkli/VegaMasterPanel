//UAVT POC Dropdown Cascade
//Arda Aladağ

$(function () {

    $('.cascadeDdl').change(function () {
        var jsonStr = $(this).val();        

        var source = $(this).attr('name');
        var text = $(this).children(":selected").text();

        var semaValue = $('#UAVTSema').val();
        var targetDdlId = $(this).data("targetddlid");
        var targetMethod = $(this).data("targetmethod");
        var targetValue = $(this).data("targetvalue");
        var targetText = $(this).data("targettext");
        var targetResetList = $(this).data("targetresetlist");

        targetCurrentJsonStr = $(this).data("targetcurrentjsonstr");
        sayfaAcildigindaVeriSetlenecekMi = $(this).data("veriseciligelecekmi");

        

        var targetDdl = $('#' + targetDdlId);

        var targetResetArray = new Array();
        if (targetResetList !== undefined && targetResetList != "") {
            targetResetArray = targetResetList.split(",");

            //Empty target DDL's
            for (i in targetResetArray) {
                var targetObj = $('#' + targetResetArray[i]);
                targetObj.empty().append('<option value="">--Seçiniz--</option>');
            }
        }

        var obj;

        //ddlIl,ddlIlce,ddlBucakKoy,ddlMahalle,ddlCsbm,ddlBina,ddlBagimsizBolum
        if (source != 'UAVTSema')
        {
            //Boş değer seçilmişse
            if (jsonStr == "") {            
                return;
            }

            obj = JSON.parse(jsonStr);
            var kod = obj.KOD;

            //Boş değer seçilmişse
            if (kod == "") {
                return;
            }            
        }
        else
        {
            //kullanılmıyor
            var kod = 0;
            obj = semaValue;

            //Şema Boş değer seçilmişse
            if (semaValue == "0") {
                return;
            }
        }

        //Source ddl için hidden field alanları set edilir
        SetHiddenFields(source, obj);

        //Controller metodu çağır
        if (targetMethod !== undefined && targetMethod.length > 0 && targetDdl.length) {

            $.ajax({
                type: "POST",
                url: targetMethod,
                data: "{'kod': " + kod + ", 'sema': " + semaValue + "}",
                contentType: "application/json; charset=utf-8",
                dataType: "json"

            }).done(function (data) {
               
                //Başarılı Cevap Durumunda drop down seçeneklerini ekle
                if (data != null) {
                   
                    targetDdl.empty().append('<option value="">--Seçiniz--</option>');

                    $.each(data.liste, function (index, data) {

                        var text = data[targetText];                        

                        //Target text null ise '-' göster
                        if (text == null || text == "") {
                            text = "-";
                        }

                        //Bağımsız Bölümlerin Adres No Bilgileri Gösterilir
                        if (targetDdlId == 'ddlBagimsizBolum' && data["ADRESNO"]) {
                            text = text + " (" + data["ADRESNO"] + ")";
                        }

                       

                        if (sayfaAcildigindaVeriSetlenecekMi == "True" && data.KOD == targetCurrentJsonStr.KOD) {
                            targetDdl.append("<option selected value='" + data[targetValue] + "'>" + text + "</option>");

                        } else {
                            targetDdl.append("<option value='" + data[targetValue] + "'>" + text + "</option>");
                        }
                    });

                    if (sayfaAcildigindaVeriSetlenecekMi != "True") {
                        if (data.liste.length == 1) {
                            //Tek seçenek varsa ilk eleman seçilir ve change event tetiklenir
                            targetDdl.prop('selectedIndex', 1).change();
                        }
                        else {
                            if (source == "UAVTSema") {
                                SetDdlValue('IlKodu', 'ddlIl', false);
                            }

                            if (source == "ddlIl") {
                                SetDdlValue('IlceKodu', 'ddlIlce', false);
                            }
                        }
                    }

                    targetDdl.change();
                }
             
            }).fail(function (response) {
                if (response.status != 0) {
                    alert(response.status + " " + response.statusText);
                }
                });

        }
    });    
});

$(document).ready(function () {
    var aktif = false;

    if ($('#SemaSecimiAktif').val() == 'true') {
        aktif = true;
    }

    SetDdlValue('AktifSema', 'UAVTSema', aktif);
});

function SetHiddenFields(source, jsonObj) {

    if (source == "UAVTSema") {
        $('#AktifSema').val(jsonObj);
    }
    else if (source == "ddlIl") {
        $('#IlKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#IlAdiUAVTMulkiyetYapi').val(jsonObj.AD);
    }
    else if (source == "ddlIlce") {
        $('#IlceKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#IlceAdiUAVTMulkiyetYapi').val(jsonObj.AD);
    }
    else if (source == "ddlBucak") {
        $('#BucakKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#BucakAdiUAVTMulkiyetYapi').val(jsonObj.AD);
    }
    else if (source == "ddlKoy") {
        $('#KoyKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#KoyAdiUAVTMulkiyetYapi').val(jsonObj.AD);
    }
    else if (source == "ddlBucakKoy") {
        $('#BucakKoduUAVTMulkiyetYapi').val(jsonObj.BUCAK_KOD);
        $('#BucakAdiUAVTMulkiyetYapi').val(jsonObj.BUCAK_AD);
        $('#KoyKoduUAVTMulkiyetYapi').val(jsonObj.KOY_KOD);
        $('#KoyAdiUAVTMulkiyetYapi').val(jsonObj.KOY_AD);
    }
    else if (source == "ddlMahalle") {
        $('#MahalleKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#MahalleAdiUAVTMulkiyetYapi').val(jsonObj.AD);
    }
    else if (source == "ddlCsbm") {
        $('#CsbmKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#CsbmAdiUAVTMulkiyetYapi').val(jsonObj.AD);
        $('#CsbmAdTipUAVTMulkiyetYapi').val(jsonObj.AD_TIP);
        $('#CsbmTipKoduUAVTMulkiyetYapi').val(jsonObj.TIP);
    }
    else if (source == "ddlBina") {
        $('#BinaKoduUAVTMulkiyetYapi').val(jsonObj.KOD);
        $('#BinaDisKapiNoUAVTMulkiyetYapi').val(jsonObj.DISKAPINO);
        $('#BinaNitelikKoduUAVTMulkiyetYapi').val(jsonObj.NITELIK);
        $('#BinaNitelikAciklamaUAVTMulkiyetYapi').val(jsonObj.NITELIK_ACIKLAMA);
        $('#SiteAdiUAVTMulkiyetYapi').val(jsonObj.SITEADI);
        $('#BlokAdiUAVTMulkiyetYapi').val(jsonObj.BLOKADI);
        $('#PaftaUAVTMulkiyetYapi').val(jsonObj.PAFTA);
        $('#AdaUAVTMulkiyetYapi').val(jsonObj.ADA);
        $('#ParselUAVTMulkiyetYapi').val(jsonObj.PARSEL);

        //var tamAdresStr = "İl: " + $('#IlAdiUAVTMulkiyetYapi').val();
        //tamAdresStr += ", İlçe: " + $('#IlceAdiUAVTMulkiyetYapi').val();
        //tamAdresStr += ", Bucak: " + $('#BucakAdiUAVTMulkiyetYapi').val();
        //tamAdresStr += ", Köy: " + $('#KoyAdiUAVTMulkiyetYapi').val();
        //tamAdresStr += ", Mahalle: " + $('#MahalleAdiUAVTMulkiyetYapi').val();
        //tamAdresStr += ", C/S/B/M: " + $('#CsbmAdTipUAVTMulkiyetYapi').val();
        //tamAdresStr += ", Bina UAVT Dış Kapı No: " + $('#BinaDisKapiNoUAVTMulkiyetYapi').val();

        var tamAdresStr = $('#MahalleAdiUAVTMulkiyetYapi').val() + " Mahallesi, ";
        tamAdresStr += $('#CsbmAdTipUAVTMulkiyetYapi').val() + ", ";
        tamAdresStr += "No: " + $('#BinaDisKapiNoUAVTMulkiyetYapi').val() + ", ";
        tamAdresStr += $('#IlceAdiUAVTMulkiyetYapi').val() + "/" + $('#IlAdiUAVTMulkiyetYapi').val();

        $('#TamAdresStrUAVTMulkiyetYapi').val(tamAdresStr);
    }
    else if (source == "ddlBagimsizBolum") {
        $('#BagimsizBolumKoduUAVTMulkiyetYapi').val(jsonObj.ADRESNO);
        $('#BagimsizBolumMulkiyetKullanimTipiUAVTMulkiyetYapi').val(jsonObj.KullanimTipi);
        $('#BagimsizBolumIcKapiNoUAVTMulkiyetYapi').val(jsonObj.ICKAPINO);
    }
}

function SetDdlValue(sourceHidden, ddlName, enabled) {
    var hiddenValue = $('#' + sourceHidden).val();

    if (hiddenValue !== undefined && hiddenValue.length > 0) {

        if (ddlName == 'UAVTSema') {
            $('#UAVTSema' + ' option').each(function (index, element) {
                if (index > 0) {
                    if (element.value == hiddenValue) {
                        var ddl = $('#UAVTSema');
                        ddl.prop('selectedIndex', index);
                        ddl.prop("disabled", !enabled);
                        ddl.change();

                        return false;//Break each loop
                    }
                }
            });
        }
        else {
            $('#'+ddlName+' option').each(function (index, element) {
                if (index > 0) {
                    var jsonObj = JSON.parse(element.value);
                    var kod = jsonObj.KOD;

                    if (kod == hiddenValue) {
                        var ddl = $('#' + ddlName);
                        ddl.prop('selectedIndex', index);
                        ddl.prop("disabled", !enabled);
                        ddl.change();

                        return false;//Break each loop
                    }
                }
            });
        }
    }
}