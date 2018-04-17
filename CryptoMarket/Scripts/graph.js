$(document).ready(function() {
    var chartData = [];
    var newPanel;
    var stockPanel;
    $.each(jsonArrayRaw, function(i, val) {
        chartData[i] = ({
            date: val.Period,
            open: parseFloat(val.Open),
            close: parseFloat(val.Close),
            high: parseFloat(val.High),
            low: parseFloat(val.Low),
            volume: parseFloat(val.Volume)
        });
    });

    //AmCharts.theme = AmCharts.themes.dark;

    //AmCharts.ready(function() {
    //    chart = new AmCharts.AmStockChart();
    //    chart.pathToImages = "https://www.amcharts.com/lib/3/images/";

    //    var categoryAxesSettings = new AmCharts.CategoryAxesSettings();
    //    categoryAxesSettings.minPeriod = "30mm";
    //    categoryAxesSettings.groupToPeriods = ["30mm", "hh", "3hh", "6hh", "12hh", "DD"];
    //    chart.categoryAxesSettings = categoryAxesSettings;

    //    chart.dataDateFormat = "YYYY-MM-DD JJ:NN";

    //    chart.balloon.horizontalPadding = 13;

    //    // DATASET //////////////////////////////////////////
    //    var dataSet = new AmCharts.DataSet();
    //    dataSet.fieldMappings = [
    //        {
    //            fromField: "open",
    //            toField: "open"
    //        }, {
    //            fromField: "close",
    //            toField: "close"
    //        }, {
    //            fromField: "high",
    //            toField: "high"
    //        }, {
    //            fromField: "low",
    //            toField: "low"
    //        }, {
    //            fromField: "volume",
    //            toField: "volume"
    //        }, {
    //            fromField: "value",
    //            toField: "value"
    //        }
    //    ];
    //    dataSet.color = "#4C4F53";
    //    dataSet.dataProvider = chartData;
    //    dataSet.categoryField = "date";

    //    chart.dataSets = [dataSet];

    //    // PANELS ///////////////////////////////////////////                                                  
    //    stockPanel = new AmCharts.StockPanel();
    //    stockPanel.title = "Value";

    //    // graph of first stock panel
    //    var graph = new AmCharts.StockGraph();
    //    graph.type = "candlestick";
    //    graph.openField = "open";
    //    graph.closeField = "close";
    //    graph.highField = "high";
    //    graph.lowField = "low";
    //    graph.valueField = "close";
    //    graph.lineColor = "#33c553";
    //    graph.fillColors = "#33c553";
    //    graph.negativeLineColor = "#db4c3c";
    //    graph.negativeFillColors = "#db4c3c";
    //    graph.fillAlphas = 1;
    //    graph.balloonText = "open:<b>[[open]]</b><br>close:<b>[[close]]</b><br>low:<b>[[low]]</b><br>high:<b>[[high]]</b>";
    //    graph.useDataSetColors = false;
    //    stockPanel.addStockGraph(graph);
    //    chart.panels = [stockPanel];


    //    // OTHER SETTINGS ////////////////////////////////////
    //    var sbsettings = new AmCharts.ChartScrollbarSettings();
    //    sbsettings.graph = graph;
    //    sbsettings.graphType = "line";
    //    sbsettings.usePeriod = "WW";
    //    chart.chartScrollbarSettings = sbsettings;

    //    // Enable pan events
    //    var panelsSettings = new AmCharts.PanelsSettings();
    //    panelsSettings.panEventsEnabled = true;
    //    chart.panelsSettings = panelsSettings;

    //    // CURSOR
    //    var cursorSettings = new AmCharts.ChartCursorSettings();
    //    cursorSettings.fullWidth = true;
    //    cursorSettings.cursorAlpha = 0.1;
    //    cursorSettings.valueBalloonsEnabled = true;
    //    cursorSettings.valueLineBalloonEnabled = true;
    //    cursorSettings.valueLineEnabled = true;
    //    cursorSettings.valueLineAlpha = 0.9;
    //    chart.chartCursorSettings = cursorSettings;

  

    //    chart.write('chart-block');

    //    newPanel = new AmCharts.StockPanel();
    //    newPanel.allowTurningOff = false;
    //    newPanel.title = "Volume";
    //    newPanel.showCategoryAxis = false;

    //    var graph2 = new AmCharts.StockGraph();
    //    graph2.valueField = "volume";
    //    graph2.fillAlphas = 0.6;
    //    graph2.type = "column";
    //    newPanel.addStockGraph(graph2);

    //    var legend = new AmCharts.StockLegend();
    //    legend.markerType = "none";
    //    legend.markerSize = 0;
    //    newPanel.stockLegend = legend;

    //    chart.addPanelAt(newPanel, 1);
    //    chart.validateNow();
    //});
});