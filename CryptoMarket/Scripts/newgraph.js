$(document)
    .ready(function() {
        var chartData = [];
        $.each(window.jsonArrayRaw,
            function(i, val) {
                chartData[i] = ({
                    date: val.Period,
                    open: parseFloat(val.Open),
                    close: parseFloat(val.Close),
                    high: parseFloat(val.High),
                    low: parseFloat(val.Low),
                    volume: parseFloat(val.Volume)
                });
            });


        var chart = AmCharts.makeChart("chart_div",
        {
            "type": "stock",
            "theme": "dark",

            "dataSets": [
                {
                    "fieldMappings": [
                        {
                            "fromField": "open",
                            "toField": "open"
                        }, {
                            "fromField": "close",
                            "toField": "close"
                        }, {
                            "fromField": "high",
                            "toField": "high"
                        }, {
                            "fromField": "low",
                            "toField": "low"
                        }, {
                            "fromField": "volume",
                            "toField": "volume"
                        }, {
                            "fromField": "value",
                            "toField": "value"
                        }
                    ],
                    "color": "#7f8da9",
                    "dataProvider": chartData,
                    "categoryField": "date"
                }
            ],


            "panels": [
                {
                    "title": "Value",
                    "showCategoryAxis": false,
                    "percentHeight": 70,
                    "valueAxes": [
                        {
                            "id": "v1",
                            "dashLength": 5
                        }
                    ],

                    "categoryAxis": {
                        "dashLength": 5
                    },

                    "stockGraphs": [
                        {
                            "type": "candlestick",
                            "id": "g1",
                            "openField": "open",
                            "closeField": "close",
                            "highField": "high",
                            "lowField": "low",
                            "valueField": "close",
                            "lineColor": "#7f8da9",
                            "fillColors": "#7f8da9",
                            "negativeLineColor": "#db4c3c",
                            "negativeFillColors": "#db4c3c",
                            "fillAlphas": 1,
                            "useDataSetColors": false,
                            "comparable": true,
                            "compareField": "value",
                            "showBalloon": true,
                            "balloonText":
                                "Open:<b>[[open]]</b><br>Close:<b>[[close]]</b><br>Low:<b>[[low]]</b><br>High:<b>[[high]]</b>",
                            "proCandlesticks": true
                        }
                    ],

                    "stockLegend": {
                        "valueTextRegular": undefined,
                        "periodValueTextComparing": "[[percents.value.close]]%"
                    }
                },
                {
                    "title": "Volume",
                    "percentHeight": 30,
                    "marginTop": 1,
                    "showCategoryAxis": true,
                    "valueAxes": [
                        {
                            "dashLength": 5
                        }
                    ],

                    "categoryAxis": {
                        "dashLength": 5
                    },

                    "stockGraphs": [
                        {
                            "valueField": "volume",
                            "type": "column",
                            "showBalloon": false,
                            "fillAlphas": 1
                        }
                    ],

                    "stockLegend": {
                        "markerType": "none",
                        "markerSize": 0,
                        "labelText": "",
                        "periodValueTextRegular": "[[value.close]]"
                    }
                }
            ],

            "chartScrollbarSettings": {
                "graph": "g1",
                "graphType": "line",
                "usePeriod": "WW"
            },

            "chartCursorSettings": {
                "valueLineBalloonEnabled": true,
                "valueLineEnabled": true
            },

            "periodSelector": {
                "position": "bottom",
                "periods": [
                    {
                        "period": "DD",
                        "count": 10,
                        "label": "10 days"
                    }, {
                        "period": "MM",
                        selected: true,
                        "count": 1,
                        "label": "1 month"
                    }, {
                        "period": "YYYY",
                        "count": 1,
                        "label": "1 year"
                    }, {
                        "period": "YTD",
                        "label": "YTD"
                    }, {
                        "period": "MAX",
                        "label": "MAX"
                    }
                ]
            },
            "export": {
                "enabled": true
            }
        });
    });
