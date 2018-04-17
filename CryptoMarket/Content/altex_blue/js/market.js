
function sellCalculate() {
    var total = parseFloat((parseFloat($("#SellAmount").val()) * parseFloat($("#SellPrice").val())).toFixed(8));
    $("#SellTotal").val(total.toFixed(8));
    var feePercent = parseFloat($("#SellFee").val()) / 100;
    var fee = parseFloat((total * feePercent).toFixed(8));
    $("#SellFeeField").val(fee.toFixed(8));
    var netTotal = (total + fee).toFixed(8);
    $("#SellNetTotal").val(netTotal);
}

function buyCalculate() {
    var total = parseFloat((parseFloat($("#BuyAmount").val()) * parseFloat($("#BuyPrice").val())).toFixed(8));
    $("#BuyTotal").val(total.toFixed(8));
    var feePercent = parseFloat($("#BuyFee").val()) / 100;
    var fee = parseFloat((total * feePercent).toFixed(8));
    $("#BuyFeeField").val(fee.toFixed(8));
    var netTotal = (total + fee).toFixed(8);
    $("#BuyNetTotal").val(netTotal);
}

function RenderSellBuyTables(data) {
    $("#tableSell").empty();
    $(data.SellOrders).each(function (i, v) {
        $("#tableSell").append("<tr class='sellorder' data-price='" + parseFloat(v.Price).toFixed(8).toLocaleString() + "' data-amount='" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "'>" +
            "<td>" + parseFloat(v.Price).toFixed(8).toLocaleString() + "</td>" +
            "<td>" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "</td>" +
            "<td>" + parseFloat(v.Total).toFixed(8).toLocaleString() + "</td>" +
            "</tr>");
    });

    $("#tableBuy").empty();
    $(data.BuyOrders).each(function (i, v) {
        $("#tableBuy").append("<tr class='buyorder' data-price='" + parseFloat(v.Price).toFixed(8).toLocaleString() + "' data-amount='" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "'>" +
            "<td>" + parseFloat(v.Price).toFixed(8).toLocaleString() + "</td>" +
            "<td class='buyorderprice'>" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "</td>" +
            "<td>" + parseFloat(v.Total).toFixed(8).toLocaleString() + "</td>" +
            "</tr>");
    });

    bindSellBuy();
}

function bindSellBuy() {
    $(".sellorder").click(function () {

        $("#BuyPrice").val($(this).data("price"));
        $("#BuyAmount").val($(this).data("amount"));

        buyingAmount = parseFloat($(this).data("amount"));
        buyPricePerOne = parseFloat($(this).data("price"));

        buyCalculate();
    });

    $(".buyorder").click(function () {
        $("#SellPrice").val($(this).data("price"));
        $("#SellAmount").val($(this).data("amount"));

        sellingAmount = parseFloat($(this).data("amount"));
        pricePerOne = parseFloat($(this).data("price"));

        sellCalculate();
    });
}

$(document)
    .ready(function() {
        $("#BuyOrderType")
            .change(function(e) {
                switch ($(this).val()) {
                    // Limit
                case "0":
                    $("#BuyPrice").prop("disabled", false);
                    $("#BuyActionPrice").prop("disabled", true);
                    break;

                // Market
                case "1":
                    $("#BuyPrice").prop("disabled", true);
                    $("#BuyActionPrice").prop("disabled", true);
                    break;

                // Stop loss
                case "2":
                    $("#BuyPrice").prop("disabled", true);
                    $("#BuyActionPrice").prop("disabled", false);
                    break;

                // Take Profit
                case "3":
                    $("#BuyPrice").prop("disabled", true);
                    $("#BuyActionPrice").prop("disabled", false);
                    break;

                // Stop Loss Limit
                case "4":
                    $("#BuyPrice").prop("disabled", false);
                    $("#BuyActionPrice").prop("disabled", false);
                    break;

                // Take Profit Limit
                case "5":
                    $("#BuyPrice").prop("disabled", false);
                    $("#BuyActionPrice").prop("disabled", false);
                    break;
                }
            });

        $("#SellOrderType")
            .change(function(e) {
                switch ($(this).val()) {
                    // Limit
                case "0":
                    $("#SellPrice").prop("disabled", false);
                    $("#SellActionPrice").prop("disabled", true);
                    break;

                // Market
                case "1":
                    $("#SellPrice").prop("disabled", true);
                    $("#SellActionPrice").prop("disabled", true);
                    break;

                // Stop loss
                case "2":
                    $("#SellPrice").prop("disabled", true);
                    $("#SellActionPrice").prop("disabled", false);
                    break;

                // Take Profit
                case "3":
                    $("#SellPrice").prop("disabled", true);
                    $("#SellActionPrice").prop("disabled", false);
                    break;

                // Stop Loss Limit
                case "4":
                    $("#SellPrice").prop("disabled", false);
                    $("#SellActionPrice").prop("disabled", false);
                    break;

                // Take Profit Limit
                case "5":
                    $("#SellPrice").prop("disabled", false);
                    $("#SellActionPrice").prop("disabled", false);
                    break;
                }
            });


        $("#SellAmount")
            .keyup(function() {
                sellCalculate();
            });
        $("#SellPrice")
            .keyup(function() {
                sellCalculate();
            });

        $("#BuyAmount")
            .keyup(function() {
                buyCalculate();
            });
        $("#BuyPrice")
            .keyup(function() {
                buyCalculate();
            });


        $("#BuyAmount")
            .keyup(function() {
                buyingAmount = parseFloat($(this).val());
                buyCalculate();
            });
        $("#BuyPricePerOne")
            .keyup(function() {
                buyPricePerOne = parseFloat($(this).val());
                buyCalculate();
            });
        $("#BuyAmount").val("1.00000000");
        $("#BuyPricePerOne").val(window.latestPrice);

        buyPricePerOne = parseFloat($("#BuyPricePerOne").val());
        buyingAmount = parseFloat($("#BuyAmount").val());

        buyCalculate();

        var marketrealtime = $.connection.marketrealtime;
        $.connection.hub.start()
            .done(function() {
                marketrealtime.server.marketHello(marketId);
            });

        $.connection.hub.stateChanged(function(change) {
            if (change.newState === $.signalR.connectionState.reconnecting) {
                console.log("Web Socket is reconnecting!");
            } else if (change.newState === $.signalR.connectionState.connected) {
                console.log("Web Socket is connected!");
            }
        });

        GetUserActiveOrders(marketId);

        marketrealtime.client.orderClosed = function(msg) {
            GetUserActiveOrders(marketId);
            $.notiny({ text: 'Order state changed! ' + msg, position: 'right-top' });
        };

        marketrealtime.client.updateBuySell = function(orders, history, depthGraph) {
            RenderSellBuyTables(orders);
            RenderOrdersHistory(history);
            GetUserActiveOrders(marketId);
        };

        marketrealtime.client.priceChanged = function(marketId, priceChangePercent, price, grow) {
            var elem = $("#pricechange-" + marketId);
            elem.html(priceChangePercent);

            if (grow) {
                elem.addClass("valid");
                setTimeout(function() {
                        elem.removeClass("valid");
                    },
                    2000);
            } else {
                elem.addClass("invalid");
                setTimeout(function() {
                        elem.removeClass("invalid");
                    },
                    2000);
            }

            var elemPrice = $("#latest-" + marketId);
            elemPrice.html(price);

            if (grow) {
                elemPrice.addClass("valid");
                setTimeout(function() {
                        elemPrice.removeClass("valid");
                    },
                    2000);
            } else {
                elemPrice.addClass("invalid");
                setTimeout(function() {
                        elemPrice.removeClass("invalid");
                    },
                    2000);
            }
        };

        marketrealtime.client.balanceUpdate = function(coinId, newBalance, coinName, value, type) {
            switch (type) {
                // deposit
            case 0:
                var text = coinName + " balance updated. Deposited:" + value + ". Current total:" + newBalance;

                $.notiny({ text: 'Balance Update ' + text, position: 'right-top' });

                break;
            // Withdraw
            case 1:
                var text2 = coinName + " balance updated. Withdrawed:" + value + ". Current total:" + newBalance;
                $.notiny({ text: 'Balance Update ' + text2, position: 'right-top' });
                break;
            }

            $("span[id^='" + coinId + "']").html(newBalance);
        }

        marketrealtime.client.chatMessage = function(username, message, timeString) {
            var template =
                '<li class="clearfix"><div class="chat-avatar"><i>' +
                    timeString +
                    '</i></div><div class="conversation-text"><div class="ctext-wrap"><i>' +
                    username +
                    '</i><p>' +
                    message +
                    '</p></div></div></li>';
            $("#chatbox").append(template);
        }

        $("#chatbox_send")
            .click(function() {
                var text = $("#chatbox_field").val();
                $("#chatbox_field").val("");
                $.post('chatmessage',
                    { message: text },
                    function(result) {});
            });

        $("#SellAmount")
            .keyup(function() {
                sellingAmount = parseFloat($(this).val());
                sellCalculate();
            });
        $("#SellPrice")
            .keyup(function() {
                pricePerOne = parseFloat($(this).val());
                sellCalculate();
            });

        $("#SellAmount").val(sellingAmount.toFixed(8));
        $("#SellPrice").val(pricePerOne.toFixed(8));

        pricePerOne = parseFloat($("#SellPrice").val());
        sellingAmount = parseFloat($("#SellAmount").val());

        sellCalculate();

        var ctb = window.coinTo + "-balance-sell";

        $("#" + ctb)
            .click(function() {
                var amount = parseFloat($(this).html());
                sellingAmount = amount;
                $("#SellAmount").val(amount);
                sellCalculate();
            });

        var cts = window.coinFrom + "-balance-buy";

        $("#" + cts)
            .click(function() {
                var amount = parseFloat($(this).html());
                buyingAmount = amount;
                $("#BuyAmount").val(amount);
                buyCalculate();
            });

        bindSellBuy();

        $("#BuyAmount")
            .keyup(function() {
                buyingAmount = parseFloat($(this).val());
                buyCalculate();
            });
        $("#BuyPrice")
            .keyup(function() {
                buyPricePerOne = parseFloat($(this).val());
                buyCalculate();
            });
        $("#BuyAmount").val("1.00000000");
        $("#BuyPrice").val(window.latestPrice);

        buyPricePerOne = parseFloat($("#BuyPricePerOne").val());
        buyingAmount = parseFloat($("#BuyAmount").val());

        buyCalculate();

        //Random highlighting by timer
        var timerId = setTimeout(function tick() {
            HighLightRandom();
            timerId = setTimeout(tick, 2000);
        }, 2000);

    });

function GetUserActiveOrders(marketId) {
    $.get('/market/GetUserActiveOrders', { marketId: marketId }, RenderUserActiveOrdersTable);
}

function RenderOrdersHistory(orders) {
    var prependData = "";
    $.each(orders, function (i, value) {
        var orderTypeName = value.OrderType === 0 ? "BUY" : "SELL";
        prependData += '<tr id="hisory-tr-' + value.Id + '">' +
            '<td style="vertical-align:middle;">' + dateFormat(value.DateClosed, "mm/dd/yyyy h:MM:ss TT") + '</td>' +
            '<td style="vertical-align:middle;">' + orderTypeName + '</td>' +
            '<td style="vertical-align:middle;">' + value.Price.toFixed(8) + '</td>' +
            '<td style="vertical-align:middle;">' + value.Amount.toFixed(8) + '</td></tr>';
    });
    $("#closed-orders-tbody").html(prependData);
}


function RenderUserActiveOrdersTable(data) {
    var myId = marketId + "myorders";
    $("#" + myId).show();
    var prependData = "";
    $.each(data, function (i, value) {
        var orderTypeName = value.OrderType === 0 ? "BUY" : "SELL";
        prependData += '<tr id="tr-' + value.Id + '">' +
            '<td style="vertical-align:middle;">' + dateFormat(parseJsonDate(value.DateCreated), "mm/dd/yyyy h:MM:ss TT") + '</td>' +
            '<td style="vertical-align:middle;">' + orderTypeName + '</td>' +
            '<td style="vertical-align:middle;">' + value.PartialOrderTotalLeft.toFixed(8) + '</td>' +
            '<td style="vertical-align:middle;">' + value.Price.toFixed(8) + '</td>' +
            '<td style="vertical-align:middle;">' + value.Total.toFixed(8) + '</td>' +
            //'<td style="vertical-align:middle;">' + value.FeeTotal.toFixed(8) + '</td>' +
            //'<td style="vertical-align:middle;">' + value.NetTotal.toFixed(8) + '</td>' +
            '<td style="vertical-align:middle;"><a href="javascript:void(0);" class="btn btn-sm btn-labeled btn-danger orderButton" data-orderId=' + value.Id + '> <span><i class="glyphicon glyphicon-remove"></i></span></a></td>' +
            '</tr>';
    });
    $("#myorders-tbody").html(prependData);

    $(".orderButton").click(function () {
        DeleteOrder($(this).attr("data-orderId"));
    });
}

function parseJsonDate(jsonDateString) {
    return new Date(parseInt(jsonDateString.replace('/Date(', '')));
}

function DeleteOrder(orderId) {
    $.get(window.deleteRoute, { orderId: orderId }, function (result) {
        if (result === true) {
            var tr = $("#tr-" + orderId);
            tr.fadeOut(500, function () {
                tr.remove();
            });
        } else if (result === false) {
            $.notiny({ text: 'Error delete Order. Maybe it was matched due request or deleted before.', position: 'right-top' });

        }
    });
}

function OrderCreateResult(data) {
    if (data.Success === false) {
        $.notiny({ text: 'Error '+ data.ErrorMessage, position: 'right-top' });
    } else {
        $.notiny({ text: 'Order successfully created', position: 'right-top' });
    }
    if (data.Status === 0) {

        GetUserActiveOrders(marketId);
    }
}



function RenderSellBuyTables(data) {
    $("#tableSell").empty();
    $(data.SellOrders).each(function (i, v) {
        $("#tableSell").append("<tr class='sellorder' data-price='" + parseFloat(v.Price).toFixed(8).toLocaleString() + "' data-amount='" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "'>" +
            "<td>" + parseFloat(v.Price).toFixed(8).toLocaleString() + "</td>" +
            "<td>" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "</td>" +
            "<td>" + parseFloat(v.Total).toFixed(8).toLocaleString() + "</td>" +
            "</tr>");
    });

    $("#tableBuy").empty();
    $(data.BuyOrders).each(function (i, v) {
        $("#tableBuy").append("<tr class='buyorder' data-price='" + parseFloat(v.Price).toFixed(8).toLocaleString() + "' data-amount='" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "'>" +
            "<td>" + parseFloat(v.Price).toFixed(8).toLocaleString() + "</td>" +
            "<td class='buyorderprice'>" + parseFloat(v.PartialOrderTotalLeft).toFixed(8).toLocaleString() + "</td>" +
            "<td>" + parseFloat(v.Total).toFixed(8).toLocaleString() + "</td>" +
            "</tr>");
    });

    bindSellBuy();
}

function bindSellBuy() {
    $(".sellorder").click(function () {

        $("#BuyPrice").val($(this).data("price"));
        $("#BuyAmount").val($(this).data("amount"));

        buyingAmount = parseFloat($(this).data("amount"));
        buyPricePerOne = parseFloat($(this).data("price"));

        buyCalculate();
    });

    $(".buyorder").click(function () {
        $("#SellPrice").val($(this).data("price"));
        $("#SellAmount").val($(this).data("amount"));

        sellingAmount = parseFloat($(this).data("amount"));
        pricePerOne = parseFloat($(this).data("price"));

        sellCalculate();
    });
}

//Highlights for market list
function HighLightRandom() {
    var tableRows = $('table.table-markets > tbody > tr');
    var rowIndex = Math.round(Math.random() * 5);
	
    if (Math.random() < 0.5) {
        HighLightChildrenElements(tableRows[rowIndex].id, "#FFB3B3");
		sleep(1000).then(() => {
			$('#lastPrice').html(Math.round(Math.random() * 1000000) / 1000000)
			HighLightElement("lastPriceContainer", "#FFB3B3");
		});
		
    }
    else {
        HighLightChildrenElements(tableRows[rowIndex].id, "#B5FFB3");
		sleep(1000).then(() => {
			$('#lastPrice').html(Math.round(Math.random() * 1000000) / 1000000)
			HighLightElement("lastPriceContainer", "#B5FFB3");
		});
		
    }
}
function HighLightElement(elementId, color) {
    $('#' + elementId).each(function () {
        $(this).animate({ backgroundColor: color }, 300);
    });

    setTimeout(function () {
        $('#' + elementId).each(function () {
            $(this).animate({ backgroundColor: "#FFFFFF" }, 300);
        });
    }, 1000);
}
function HighLightChildrenElements(elementId, color) {
    $('#' + elementId).each(function () {
        $(this).children().animate({ backgroundColor: color }, 300);
    });

    setTimeout(function () {
        $('#' + elementId).each(function () {
            $(this).children().animate({ backgroundColor: "#FFFFFF" }, 300);
        });
    }, 1000);
}
function sleep (time) {
  return new Promise((resolve) => setTimeout(resolve, time));
}