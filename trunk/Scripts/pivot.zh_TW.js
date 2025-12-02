(function () {
    var callWithJQuery;

    callWithJQuery = function (pivotModule) {
        if (typeof exports === "object" && typeof module === "object") {
            return pivotModule(require("jquery"));
        } else if (typeof define === "function" && define.amd) {
            return define(["jquery"], pivotModule);
        } else {
            return pivotModule(jQuery);
        }
    };

    callWithJQuery(function ($) {
        var c3r, d3r, frFmt, frFmtInt, frFmtPct, gcr, nf, r, tpl;
        nf = $.pivotUtilities.numberFormat;
        tpl = $.pivotUtilities.aggregatorTemplates;
        r = $.pivotUtilities.renderers;
        gcr = $.pivotUtilities.gchart_renderers;
        d3r = $.pivotUtilities.d3_renderers;
        c3r = $.pivotUtilities.c3_renderers;

        console.debug(gcr);

        frFmt = nf({
            thousandsSep: ",",
            decimalSep: "."
        });
        frFmtInt = nf({
            digitsAfterDecimal: 0,
            thousandsSep: ",",
            decimalSep: "."
        });
        frFmtPct = nf({
            digitsAfterDecimal: 2,
            scaler: 100,
            suffix: "%",
            thousandsSep: ",",
            decimalSep: "."
        });
        $.pivotUtilities.locales.zh_TW = {
            localeStrings: {
                renderError: "展示結果時出錯。",
                computeError: "計算結果時出錯。",
                uiRenderError: "展示界面時出錯。",
                selectAll: "選擇全部",
                selectNone: "全部不選",
                tooMany: "(因數據過多而無法列出)",
                filterResults: "篩選值",
                apply: "套用",
                cancel: "取消",
                totals: "合計",
                vs: "於",
                by: "分組於"
            },
            aggregators: {
                "加總": tpl.sum(frFmtInt)
            },
            vals: ["件數"],
            aggregatorName: "加總",
            /*
            aggregators: {
                "個數": tpl.count(frFmtInt),
                "非重複值的個數": tpl.countUnique(frFmtInt),
                "列出非重複值": tpl.listUnique(", "),
                "求和": tpl.sum(frFmt),
                "求和後取整": tpl.sum(frFmtInt),
                "平均值": tpl.average(frFmt),
                "中位數": tpl.median(frFmt),
                "方差": tpl["var"](1, frFmt),
                "樣本標準偏差": tpl.stdev(1, frFmt),
                "最小值": tpl.min(frFmt),
                "最大值": tpl.max(frFmt),
                "第一": tpl.first(frFmt),
                "最後": tpl.last(frFmt),
                "兩和之比": tpl.sumOverSum(frFmt),
                "二項分佈：置信度為80%時的區間上限": tpl.sumOverSumBound80(true, frFmt),
                "二項分佈：置信度為80%時的區間下限": tpl.sumOverSumBound80(false, frFmt),
                "和在總計中的比例": tpl.fractionOf(tpl.sum(), "total", frFmtPct),
                "和在行合計中的比例": tpl.fractionOf(tpl.sum(), "row", frFmtPct),
                "和在列合計中的比例": tpl.fractionOf(tpl.sum(), "col", frFmtPct),
                "個數在總計中的比例": tpl.fractionOf(tpl.count(), "total", frFmtPct),
                "個數在行合計中的比例": tpl.fractionOf(tpl.count(), "row", frFmtPct),
                "個數在列合計中的比例": tpl.fractionOf(tpl.count(), "col", frFmtPct)
            }, */
            renderers: {
                "表格": r["Table"],
                "表格內柱狀圖": r["Table Barchart"],
                "熱圖": r["Heatmap"],
                "行熱圖": r["Row Heatmap"],
                "列熱圖": r["Col Heatmap"]
            }
        };
        if (gcr) {
            $.pivotUtilities.locales.zh_TW.gchart_renderers = {
                "折線圖": gcr["Line Chart"],
                "柱狀圖": gcr["Bar Chart"],
                "堆棧柱狀圖": gcr["Stacked Bar Chart"],
                "面積圖": gcr["Area Chart"],
                "圓餅圖": gcr["Pie Chart"],
                "圓餅圖(3D)": gcr["Pie Chart 3D"],
                "甜甜圈": gcr["Donut Chart"]
            };
            $.pivotUtilities.locales.zh_TW.renderers = $.extend($.pivotUtilities.locales.zh_TW.renderers, $.pivotUtilities.locales.zh_TW.gchart_renderers);
            console.debug($.pivotUtilities.locales.zh_TW.renderers);
        }
        if (d3r) {
            $.pivotUtilities.locales.zh_TW.d3_renderers = {
                "樹圖": d3r["Treemap"]
            };
            $.pivotUtilities.locales.zh_TW.renderers = $.extend($.pivotUtilities.locales.zh_TW.renderers, $.pivotUtilities.locales.zh_TW.d3_renderers);
        }
        if (c3r) {
            $.pivotUtilities.locales.zh_TW.c3_renderers = {
                "折線圖": c3r["Line Chart"],
                "柱狀圖": c3r["Bar Chart"],
                "堆棧柱狀圖": c3r["Stacked Bar Chart"],
                "面積圖": c3r["Area Chart"],
                "散點圖": c3r["Scatter Chart"]
            };
            $.pivotUtilities.locales.zh_TW.renderers = $.extend($.pivotUtilities.locales.zh_TW.renderers, $.pivotUtilities.locales.zh_TW.c3_renderers);
        }
        return $.pivotUtilities.locales.zh_TW;
    });

}).call(this);

//# sourceMappingURL=pivot.zh_TW.js.map