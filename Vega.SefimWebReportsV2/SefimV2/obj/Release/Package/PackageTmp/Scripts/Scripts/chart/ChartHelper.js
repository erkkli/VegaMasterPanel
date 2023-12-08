/*
	ChartHelper
	by Arda Aladağ
*/

this.pieceLabelOption = {
    // render 'label', 'value', 'percentage' or custom function, default is 'percentage'
    render: 'percentage',

    // precision for percentage, default is 0
    precision: 2,

    // identifies whether or not labels of value 0 are displayed, default is false
    showZero: true,

    // font size, default is defaultFontSize
    fontSize: 12,

    // font color, can be color array for each data, default is defaultFontColor
    fontColor: '#fff',

    // font style, default is defaultFontStyle
    fontStyle: 'normal',

    // font family, default is defaultFontFamily
    fontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",

    // draw label in arc, default is false
    arc: false,

    // position to draw label, available value is 'default', 'border' and 'outside'
    // default is 'default'
    position: 'default',

    // draw label even it's overlap, default is false
    overlap: true
}

this.currencyFormatter = new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    minimumFractionDigits: 2,
});
currencyFormatter.style = 'currency';

this.decimalFormatter = new Intl.NumberFormat('tr-TR', {
    style: 'decimal',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
});
decimalFormatter.style = 'decimal';

this.percentFormatter = new Intl.NumberFormat('tr-TR', {
    style: 'percent',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
});
percentFormatter.style = 'percent';

this.barThickness = 30;

window.chartColors = {
    red: 'rgb(255, 99, 132)',
    blue: 'rgb(54, 162, 235)',
    orange: 'rgb(255, 159, 64)',
    green: 'rgb(75, 192, 192)',
    yellow: 'rgb(255, 205, 86)',
    purple: 'rgb(153, 102, 255)',
    grey: 'rgb(201, 203, 207)'
};

this.colorNames = Object.keys(window.chartColors);

this.getRandomColor = function() {
    var letters = '0123456789ABCDEF'.split('');
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

this.GetBarChart = function (canvasId, title, xLabel, yLabel, chartDataJson, formatter)
{
    var ctx = $("#" + canvasId);

    var myChart = new Chart(ctx, {
        type: 'bar',//line, radar, bar, doughnut, pie, area, scatter, polarArea, bubble
        data: JSON.parse(chartDataJson),
        options: {
            scales: {
                yAxes: [{
                    display: true,
                    scaleLabel: {
                        display: false,
                        labelString: yLabel
                    },
                    ticks: {
                        beginAtZero: true
                    }
                }],
                xAxes: [{
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: xLabel
                    },
                    barThickness: barThickness
                }]
            },
            legend: {
                position: 'top',
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: false,
                text: title
            },
            plugins: {
                datalabels: {
                    //color: 'white',
                    //display: function (context) {
                    //    return context.dataset.data[context.dataIndex] > 15;
                    //},
                    font: {
                        weight: 'bold',
                        size: '10'
                    },
                    anchor: 'end',
                    align: 'end',
                    offset: 0,
                    formatter: function (value, context) {
                        if (formatter.style == 'percent') {
                            value = value / 100;
                        }
                        return formatter.format(value);
                    }
                }
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        if (formatter.style == 'percent') {
                            tooltipItem.yLabel = tooltipItem.yLabel / 100;
                        }
                        return formatter.format(tooltipItem.yLabel);
                    }
                }
            }
        }
    });

    //Update Background Color & Border Color
    //myChart.data.labels.push(label);
    var index = 0;
    myChart.data.datasets.forEach((dataset) => {

        var colorName = colorNames[index++ % colorNames.length];
        var dsColor = window.chartColors[colorName];

        dataset.backgroundColor = Chart.helpers.color(dsColor).alpha(0.5).rgbString();
        dataset.borderColor = dsColor;

        //dataset.backgroundColor = [getRandomColor(), getRandomColor()];
        //dataset.backgroundColor = 'rgb(255, 99, 132)';
        //dataset.borderColor = 'rgb(255, 99, 132)';
    });
    myChart.update();

    return myChart;
}

this.GetPieChart = function(canvasId, title, chartDataJson, formatter) {
    var ctx = $("#" + canvasId);

    var myChart = new Chart(ctx, {
        type: 'pie',//line, radar, bar, doughnut, pie, area, scatter, polarArea, bubble
        data: JSON.parse(chartDataJson),
        options: {
            legend: {
                position: 'top',
            },
            responsive: true,
            maintainAspectRatio: false,
            title: {
                display: true,
                text: title
            },
            pieceLabel: pieceLabelOption,
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        var indice = tooltipItem.index;
                        return data.labels[indice] + ': ' + formatter.format(data.datasets[0].data[indice]) + '';
                    }
                }
            }
        }
    });

    //Update Background Color & Border Color
    //myChart.data.labels.push(label);
    myChart.data.datasets.forEach((dataset) => {

        //var colorName = colorNames[index++ % colorNames.length];
        //var dsColor = window.chartColors[colorName];

        //dataset.backgroundColor = [Chart.helpers.color(dsColor).alpha(0.5).rgbString()];
        //dataset.borderColor = dsColor;

        dataset.backgroundColor = [window.chartColors['red'],
                                    window.chartColors['green'],
                                    window.chartColors['orange'],
                                    window.chartColors['yellow'],
                                    window.chartColors['blue']];

        //dataset.backgroundColor = [getRandomColor(), getRandomColor()];
        //dataset.backgroundColor = 'rgb(255, 99, 132)';
        //dataset.borderColor = 'rgb(255, 99, 132)';
    });
    myChart.update();
}

///Genel

function GetAlanSayilariChart(chartDataJson, canvasId) {

    GetPieChart(canvasId, 'Sayı Dağılımı', chartDataJson, decimalFormatter);    
}

function GetAlanNufusChart(chartDataJson, canvasId) {

    GetPieChart(canvasId, 'Nüfus Dağılımı', chartDataJson, decimalFormatter);    
}

function GetToplamAlanChart(chartDataJson, canvasId) {

    GetPieChart(canvasId, 'Toplam Alan(Ha) Dağılımı', chartDataJson, decimalFormatter);
}

function GetToplamBBChart(chartDataJson, canvasId) {

    GetPieChart(canvasId, 'Bağımsız Birimlerin Dağılımı', chartDataJson, decimalFormatter);    
}

function GetOdenenDagilimiChart(chartDataJson, canvasId) {

    GetPieChart(canvasId, 'Ödenen Miktarın Dağılımı', chartDataJson, currencyFormatter);
}

function GetYikimYuzdesiChart(chartDataJson, canvasId) {

    GetPieChart(canvasId, 'Riskli Yapı Tespiti Yapılan Binaların Yıkım Yüzdesi', chartDataJson, decimalFormatter);
}

/////////////////////////////////////////////////////////////////////////

///Riskli Yapı

function GetRiskliYapiYikimYuzdesiChart(chartDataJson, canvasId) {
    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Yıkım Yüzdeleri', 'Yıl', 'Sayı', chartDataJson, percentFormatter);
}

function GetRiskliYapiBBSayisiChart(chartDataJson, canvasId) {
    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Bağımsız Bölüm Sayıları', 'Yıl', 'Sayı', chartDataJson, decimalFormatter);
}

function GetRiskliYapiBinaSayisiChart(chartDataJson, canvasId) {
    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Bina Sayıları', 'Yıl', 'Miktar', chartDataJson, decimalFormatter);
}

/////////////////////////////////////////////////////////////////////////

///Finans

function GetToplamOdenenChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Ödenen Miktarlar', 'Yıl', 'Sayı', chartDataJson, currencyFormatter);
}

function GetKiraYardimiSayisiChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Kira Yardımı Sayıları', 'Yıl', 'Sayı', chartDataJson, decimalFormatter);
}

function GetFaizYardimiSayisiChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Faiz Yardımı Sayıları', 'Yıl', 'Miktar', chartDataJson, decimalFormatter);
}

/////////////////////////////////////////////////////////////////////////

///Kentsel Dönüşüm Alanları

function GetBagimsizBolumSayisiChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Bağımsız Bölüm Sayıları', 'Yıl', 'Sayı', chartDataJson, decimalFormatter);    
}

function GetIlanEdilmisChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında İlan Edilmiş Alan Sayıları', 'Yıl', 'Sayı', chartDataJson, decimalFormatter);    
}

function GetBinaSayisiChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Bina Sayıları', 'Yıl', 'Sayı', chartDataJson, decimalFormatter);
}

/////////////////////////////////////////////////////////////////////////

///Rezerv Yapı Alan

function GetSayilarChart(chartDataJson, canvasId) {

    GetBarChart(canvasId, 'Türkiye - Yıl Bazında Sayılar', 'Yıl', 'Sayı', chartDataJson, decimalFormatter);    
}

/////////////////////////////////////////////////////////////////////////

this.options = {
    useEasing: true,
    useGrouping: true,
    separator: '.',
    decimal: ',',
};
//Count Up Animation
function CountUpAnimation(value, headerId)
{
    var numAnim = new CountUp(headerId, 0, value, 0, 3, options);
    if (!numAnim.error) {
        numAnim.start();
    } else {
        console.error(numAnim.error);
    }
}