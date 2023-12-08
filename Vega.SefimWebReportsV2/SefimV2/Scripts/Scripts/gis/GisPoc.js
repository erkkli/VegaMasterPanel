//Arda Aladağ

function Clear() {
    $('#adaParselListesi').empty();
    $('#mulkiyetListesi').empty();
    $('#malikListesi').empty();
    $('#kisiBilgileri').empty();
}

function KisiBilgisiGetir(tckn) {    

    if (tckn == null || tckn.length == 0) {
        alert('Kişi bilgisi bulunamadı!');
        return;
    }

    var targetDiv = $('#kisiBilgileri');
    targetDiv.empty();

    $.ajax({
        type: "POST",
        url: "/GIS/KisiBilgisiGetir",
        data: "{'tckn':" + tckn + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json"

    }).done(function (data) {
        //Başarılı Cevap Durumu
        if (data != null && data.kisi != null) {

            targetDiv.append('<table width="320" border="1">');

            var tckn = data.kisi['TcKimlikNo'];
            var adi = data.kisi['Adi'];
            var soyadi = data.kisi['Soyadi'];
            var babaAdi = data.kisi['BabaAdi'];
            var dogumYeri = data.kisi['DogumYeri'];
            var dogumTarihi = data.kisi['DogumTarihi'];
            var yasamaDurumu = data.kisi['YasamaDurumuAciklama'];
            var cinsiyet = data.kisi['CinsiyetAciklama'];

            var date = new Date(parseInt(dogumTarihi.substr(6)));
            var formattedDate = date.toLocaleDateString()

            var text = "<b>TCKN:</b> " + tckn + ", <b>Adı:</b> " + adi + ", <b>Soyadı:</b> " + soyadi + ", <b>Baba Adı:</b> " + babaAdi + ", <b>Doğum Yeri:</b> " + dogumYeri + ", <b>Doğum Tarihi:</b> " + formattedDate + ", <b>Cinsiyet:</b> " + cinsiyet + ", <b>Durumu:</b> " + yasamaDurumu;
            //var button = "<td><a href=\"javascript:KisiBilgisiGetir(" + tckn + ");\" class=\"button\">Kişi Bilgisi Getir</a></td>";
            var button = "";
            targetDiv.append('<tr><td colspan="2" rowspan="1">' + text + '</td>' + button + '</tr>');
            targetDiv.append('</table>');
        }
        else {
            alert('Seçilen malik için bilgi bulunamadı!');
        }
    }).fail(function (response) {
        if (response.status != 0) {
            alert(response.status + " " + response.statusText);
        }
    });
}

function MalikListesiGetir(adaNo, parselNo) {

    if (adaNo.length == 0 || parselNo.length == 0) {
        alert('Malik bilgisi bulunamadı!');
        return;
    }

    var targetDiv = $('#malikListesi');
    targetDiv.empty();
    $('#kisiBilgileri').empty();

    $.ajax({
        type: "POST",
        url: "/GIS/MalikListesiGetir",
        data: "{'il':" + adaNo + ", 'ilce' : " + parselNo + ", 'ada' : " + adaNo + ", 'parsel' : " + parselNo + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json"

    }).done(function (data) {
        //Başarılı Cevap Durumu
        if (data != null && data.liste != null && data.liste.length > 0) {
            
            targetDiv.append('<table width="320" border="1">');

            $.each(data.liste, function (index, data) {
                var tckn = data['Tckn'];
                var adi = data['Adi'];
                var soyadi = data['Soyadi'];                

                var text = "<b>TCKN:</b> " + tckn + ", <b>Adı:</b> " + adi + ", <b>Soyadı:</b> " + soyadi;
                var button = "<td><a href=\"javascript:KisiBilgisiGetir(" + tckn + ");\" class=\"button\">Kişi Bilgisi Getir</a></td>";
                targetDiv.append('<tr><td colspan="2" rowspan="1">' + text + '</td>' + button + '</tr>');
            });

            targetDiv.append('</table>');
        }
        else {
            alert('Seçilen yapı için malik bilgisi bulunamadı!');
        }
    }).fail(function (response) {
        if (response.status != 0) {
            alert(response.status + " " + response.statusText);
        }
    });
}

function YapiListesiGetir(adaNo, parselNo, tapuMahalleRef) {

    if (adaNo.length == 0 || parselNo.length == 0 || tapuMahalleRef.length == 0) {
        alert('Ada/Parsel bilgisi bulunamadı!');
        return;
    }

    var targetDiv = $('#mulkiyetListesi');
    targetDiv.empty();
    $('#malikListesi').empty();
    $('#kisiBilgileri').empty();

    $.ajax({
        type: "POST",
        url: "/GIS/GetMulkiyetByAdaParsel",
        data: "{'ada':" + adaNo + ", 'parsel' : " + parselNo + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "json"

    }).done(function (data) {
        //Başarılı Cevap Durumu
        if (data != null && data.liste != null && data.liste.length > 0) {

            targetDiv.append('<table width="320" border="1">');

            $.each(data.liste, function (index, data) {
                var mulkiyetId = data['Mulkiyet_Id'];
                var mulkiyetAda = data['Mulkiyet_Ada'];
                var mulkiyetParsel = data['Mulkiyet_Parsel'];
                var mulkiyetAlan = data['Mulkiyet_Alan'];
                var yapiAciklama = data['Yapi_Aciklama'];
                var yapiBinaAdi = data['Yapi_BinaAdi'];
                var yapiBinaDisKapiNo = data['Yapi_BinaUavtDisKapiNo'];
                var yapiAda = data['Yapi_UavtAda'];
                var yapiParsel = data['Yapi_UavtParsel'];
                var yknNo = data['Yapi_YknNo'];

                var text = "<b>Mülkiyet Id:</b> " + mulkiyetId + ", <b>Açıklama:</b> " + yapiAciklama + ", <b>Bina Adı:</b> " + yapiBinaAdi + ", <b>Bina Dış Kapı No:</b> " + yapiBinaDisKapiNo + ", <b>YKN:</b> " + yknNo;
                var button = "<td><a href=\"javascript:MalikListesiGetir(" + yapiAda + "," + yapiParsel + ");\" class=\"button\">Malik Bilgileri Getir</a></td>";
                targetDiv.append('<tr><td colspan="2" rowspan="1">' + text + '</td>' + button + '</tr>');
            });

            targetDiv.append('</table>');
        }
        else {
            alert('Seçilen parsel için mülkiyet/yapı kaydı bulunamadı!');
        }
    }).fail(function (response) {
        if (response.status != 0) {
            alert(response.status + " " + response.statusText);
        }
    });
}

function AdaParselListesiGetir(option) {

    var url = "";
    var data = "";

    //Seçili alanlara göre getir
    if(option == 1) {
        url = "/GIS/AdaParselListesiGetir";
        var ring = gisApi.getCoordinatesOfSelectedFeatures('EPSG:4326');
        //var ring = "[[[3236364.081180001,5006311.721996905],[3236531.287179375,5006524.312481823],[3236644.7483932357,5006428.766196467],[3236462.0161224915,5006209.009740147],[3236364.081180001,5006311.721996905]]]";

        if (ring === undefined || ring == null || ring.length == 0) {
            return;
        }

        ring = "{rings : " + ring + "}";
        data = { ringListJson: ring };
    }
    //Çizili bütün alanlara göre getir
    else if (option == 2) {
        url = "/GIS/AdaParselListesiGetirByGeojson";
        var exportGeojson = gisApi.exportGeojson(null, 'EPSG:4326');
        if (exportGeojson) {
            data = { geojson: exportGeojson };
        }
        else {
            alert('Çizili bir alan bulunamadı!');
            return;
        }
    }
    
    var targetDiv = $('#adaParselListesi');
    targetDiv.empty();
    $('#mulkiyetListesi').empty();
    $('#malikListesi').empty();
    $('#kisiBilgileri').empty();

    //Controller metodu çağır
    if (data) {
        $.ajax({
            type: "POST",
            url: url,
            data: data,
            //contentType: "application/json; charset=utf-8",
            //dataType: "json"

        }).done(function (data) {
            //Başarılı Cevap Durumunda drop down seçeneklerini ekle
            if (data != null && data.liste != null && data.liste.length > 0) {

                targetDiv.append('<table width="320" border="1">');

                $.each(data.liste, function (index, data) {
                    //var ada = data.Attributes['AdaNo'];
                    //var id = data.Attributes['Id'];
                    //var objectId = data.Attributes['ObjectId'];
                    //var parsel = data.Attributes['Parselno'];
                    //var tapuCinsAciklama = data.Attributes['TapuCinsAciklama'];
                    //var tapuKimlikNo = data.Attributes['TapuKimlikNo'];
                    //var tapuMahalleRef = data.Attributes['TapuMahalleRef'];
                    //var tapuZeminRef = data.Attributes['TapuZeminRef'];
                    //var tip = data.Attributes['Tip'];
                    //var tapuAlan = data.Attributes['TapuLan'];
                    //var kadastroAlan = data.Attributes['KadastroAlan'];
                    //var sekilAlan = data.Attributes['Shape.Area'];

                    var ada = data.AdaNo;
                    var parsel = data.ParselNo;
                    var tapuCinsAciklama = data.TapuCinsAciklama;
                    var tapuKimlikNo = data.TapuKimlikNo;
                    var tapuMahalleRef = data.TapuMahalleRef;
                    var tapuZeminRef = data.TapuZeminRef;
                    var tapuAlan = data.TapuALan;
                    var kadastroAlan = data.KadastroAlan;
                    
                    var text = ""
                        //+ "<b>Id:</b> " + id
                        + " <b>Ada:</b> " + ada
                        + ", <b>Parsel:</b> " + parsel
                        + ", <b>Tapu Mahalle Ref:</b> " + tapuMahalleRef
                        + ", <b>Tapu Zemin Ref:</b> " + tapuZeminRef
                        + ", <b>Aciklama:</b> " + tapuCinsAciklama
                        + ", <b>Tapu Kimlik No:</b> " + tapuKimlikNo;
                        + ", <b>Tapu Alan:</b> " + tapuAlan;
                        + ", <b>Kadastro Alan:</b> " + kadastroAlan;
                    var button = "<a href=\"javascript:YapiListesiGetir(" + ada + "," + parsel + "," + tapuMahalleRef + ");\" class=\"button\">Yapıları Getir</a>";
                    targetDiv.append('<tr><td colspan="2" rowspan="1">' + text + '</td><td>' + button + '</td></tr>');
                });

                targetDiv.append('</table>');
            }
            else {
                alert('Seçilen alan içerisinde ada/parsel kaydı bulunamadı!');
            }
        }).fail(function (response) {
            if (response.status != 0) {
                alert(response.status + " " + response.statusText);
            }
        });
    }
}

$(function () {

});