//本類別需要引用 jquery-x.x.x.js 與 sessionstorage.js
$.fn.extend({
    treeMenu: function(config) {
        var _nodeClicked;
        var openedClass = "glyphicon-minus-sign";
        var closedClass = "glyphicon-plus-sign";
      
        if (config !== undefined && config != null) {
            if (config.openedClass) openedClass = config.openedClass;
            if (config.closedClass) closedClass = config.closedClass;
        }
        else {
            config = {};
        };

        //若陣列不支援 includes() 方法時
        if (![].includes) {
            Array.prototype.includes = function(searchElement /*, fromIndex*/ ) {
                'use strict';
                var O = Object(this);
                var len = parseInt(O.length) || 0;
                if (len === 0) {
                    return false;
                }
                var n = parseInt(arguments[1]) || 0;
                var k;
                if (n >= 0) {
                    k = n;
                } else {
                    k = len + n;
                    if (k < 0) {k = 0;}
                }
                var currentElement;
                while (k < len) {
                    currentElement = O[k];
                    if (searchElement === currentElement ||
                        (searchElement !== searchElement && currentElement !== currentElement)) {
                    return true;
                    }
                    k++;
                }
                return false;
            };
        }

        //若陣列不支援 filter() 方法時
        if (![].filter) {
            Array.prototype.filter = function(func, thisArg) {
                'use strict';
                if ( ! ((typeof func === 'Function') && this) )
                    throw new TypeError();
    
                var len = this.length >>> 0,
                    res = new Array(len), // 預先配置陣列
                    c = 0, i = -1;
                if (thisArg === undefined)
                    while (++i !== len)
                    // 確認物件的鍵值i是否有被設置
                    if (i in this)
                        if (func(t[i], i, t))
                        res[c++] = t[i];
                else
                    while (++i !== len)
                    // 確認物件的鍵值i是否有被設置
                    if (i in this)
                        if (func.call(thisArg, t[i], i, t))
                        res[c++] = t[i];
    
                res.length = c; // 將陣列縮至適當大小
                return res;
            };
        }
      
        var tree = $(this);
        tree.addClass("tree");

        tree.find("li").has("ul").each(function() {
            var branch = $(this);
            branch.prepend(["<i class=\"indicator glyphicon ", closedClass, "\" style=\"color:#555555;\"></i>"].join(""));
            branch.addClass("branch");
            branch.on("click", function(e) {
                if (this == e.target) {
                    var elm = $(this);
                    var icon = elm.children("i:first");
                    icon.toggleClass(openedClass + " " + closedClass);
                    elm.children().children().toggle();
                }
            });
            branch.children().children().toggle();
        });

        //取得子層選單展開狀態值
        getExpandState = function(obj) {
            if (obj.prop("_exp")) return obj.prop("_exp");
            else {
                var ret = "0";
                obj.prop("_exp", ret);
                return ret;
            }
        }

        //「在分層選單節點標題的前置圖示」點擊事件處理
        tree.find(".branch .indicator").each(function() {
            $(this).on("click", function() {
                var _exp = "0";
                var p = $(this).closest("li");
                var h = p.find("a:first");
                if (h.length == 1) {
                    _exp = getExpandState(h);
                    _exp = (_exp == "1") ? "0" : "1";
                    h.prop("_exp", _exp);
                }
                p.click();
                raiseExpandEvent(h, "1", _exp);
            });
        });

        //取得在「視窗開啟者」內的方法物件
        findFunc = function(funcName) {
            var ret = window[funcName];
            if (typeof ret !== "function") ret = undefined;
            return ret;
        }

        getNodePathArray = function(obj) {
            var m = [];
            var p = obj.closest("i .indicator");
            return m;
        }

        //建立要傳回給 callback 方法的 JSNO 參數
        createCallbackArgs = function(obj, nodeType) {
            var _exp = getExpandState(obj);
            var ret = {
                "node": { "type": nodeType, "id": obj.attr("id"), "expand": _exp },
                "text": obj.text(),
                "url": obj.attr("href")
            };
            return ret; 
        }

        //取得本機端瀏覽器 local storage 物件
        getLocalStorage = function(objLS) {
            return (objLS) ? objLS : (($.LS) ? $.LS : window.LS);
        }

        //移除已展開的選單節點 id
        removeExpandKey = function(key, objLS) {
            try {
                var ls = getLocalStorage(objLS);
                if (ls) {
                    var m = readExpandKey(ls);
                    if (m && m.id) {
                        if (m.id.includes(key)) {
                            //移除陣列值
                            m.id = m.id.filter(function(item) { return item !== key; });
                            ls.set("tree", JSON.stringify(m));
                        }
                    }
                }
            }
            catch (ex) {
                alert(ex.message);
            }
        }

        //取出已展開的選單節點 id
        readExpandKey = function(objLS) {
            var ls = getLocalStorage(objLS);
            if (!ls) return undefined;
            else {
                var m = ls.get("tree");
                return (m) ? JSON.parse(m) : { "cid": "", "id": [] };
            }
        }

        //暫存已展開的選單節點 id
        saveExpandKey = function(key, objLS) {
            var ls = getLocalStorage(objLS);
            if (ls) {
                var m = readExpandKey(ls);
                if (m && m.id) {
                    if (!m.id.includes(key)) {
                        m.id.push(key);
                        ls.set("tree", JSON.stringify(m));
                    }
                }
            }
        }

        //取出最後一次點擊的選單節點 id
        readClickedKey = function(objLS) {
            var ls = getLocalStorage(objLS);
            if (!ls) return "";
            else {
                var m = readExpandKey(ls);
                return (m && m.cid) ? m.cid : "";
            }
        }

        //暫存最後一次點擊的選單節點 id
        saveClickedKey = function(key, objLS) {
            var ls = getLocalStorage(objLS);
            if (ls) {
                var m = readExpandKey(ls);
                if (m) {
                    m.cid = key;
                    ls.set("tree", JSON.stringify(m));
                }
            }
        }

        //清除所存暫存的選單節點 id
        clearSavedKeys = function(objLS) {
            var ls = getLocalStorage(objLS);
            if (ls) ls.clear();
        }

        //引發 onExpand 或是 onCollapse 事件
        raiseExpandEvent = function(obj, nodeType, expandState) {
            try {
                var args = createCallbackArgs(obj, nodeType);
                var key = obj.attr("id");
                switch (expandState) {
                    case "0":
                        removeExpandKey(key);
                        if (config.onCollapse) config.onCollapse.call(window, args, obj);
                        break;
                    case "1":
                        saveExpandKey(key);
                        if (config.onExpand) config.onExpand.call(window, args, obj);
                        break;
                }
            }
            catch (ex) {
                alert(ex.message);
            }
        }

        findAnchor = function(nodeId) {
            var path = ["li a[id=\"", nodeId, "\"]:first"].join("");
            return tree.find(path);
        }

        //展開分層節點內容（nodeId 為選單節點的 HTML id）
        expand = function(nodeId, isRecursive) {
            try {
                var elm = (typeof nodeId === "object") ? nodeId : findAnchor(nodeId);
                if (elm.length > 0) {
                    //判斷是否為分層節點
                    var elmUl = elm.next("ul");
                    if (elmUl.length > 0) {
                        var _exp = getExpandState(elm);
                        if (_exp == "0") {
                            var elmli = elm.closest("li");
                            if (elmli.length > 0) {
                                elm.prop("_exp", "1");
                                elmli.click();
                                raiseExpandEvent(elm, "1", "1");
                                //逐一展開父層選單
                                isRecursive = (!isRecursive) ? false : (isRecursive == true);
                                if (isRecursive == true) {
                                    var p = elm.parent(".branch").parent("ul").parent(".branch").find("a:first");
                                    expand(p, isRecursive);
                                }
                            }
                        }
                    }
                }
            }
            catch (ex) {
                alert(ex.message);
            }
        }

        //展開分層節點內容（nodeId 為選單節點的 HTML id）
        collapse = function(nodeId, isRecursive) {
            try {
                var elm = (typeof nodeId === "object") ? nodeId : tree.find(["li a[id=\"", nodeId, "\"]:first"].join(""));
                if (elm.length > 0) {
                    //判斷是否為分層節點
                    var elmUl = elm.next("ul");
                    if (elmUl.length > 0) {
                        var _exp = getExpandState(elm);
                        if (_exp == "1") {
                            var elmli = elm.closest("li");
                            if (elmli.length > 0) {
                                elm.prop("_exp", "0");
                                elmli.click();
                                raiseExpandEvent(elm, "1", "0");
                                //逐一收合父層選單
                                isRecursive = (!isRecursive) ? false : (isRecursive == true);
                                if (isRecursive == true) {
                                    var p = elm.parent(".branch").parent("ul").parent(".branch").find("a:first");
                                    collapse(p, isRecursive);
                                }
                            }
                        }
                    }
                }
            }
            catch (ex) {
                alert(ex.message);
            }
        }

        //引發 onXXXXClick 事件
        raiseNodeClickEvent = function(obj, nodeType) {
            try {
                var ret = true;
                var args = createCallbackArgs(obj, nodeType);
                switch (nodeType) {
                    case "1":
                        ret = (config.onBranchClick) ? config.onBranchClick.call(window, args, obj) : true;
                        if (config.onNodePathArray) config.onNodePathArray(window, getNodePathArray(obj));
                        break;
                    case "2":
                         saveClickedKey(obj.attr("id"));
                        if (_nodeClicked) _nodeClicked.removeClass("nodeClicked");
                        _nodeClicked = obj;
                        obj.addClass("nodeClicked");
                        ret = (config.onLeafClick) ? config.onLeafClick.call(window, args, obj) : true;
                        if (config.onNodePathArray) config.onNodePathArray(window, getNodePathArray(obj));
                        break;
                }
                ret = (config.onNodeClick) ? config.onNodeClick.call(window, args, obj) : ret;
                return ret;
            }
            catch (ex) {
                alert(ex.message);
            }
        }

        //「分層選單節點標題」點擊事件處理
        tree.find("li.branch>i.indicator+a").each(function() {
            $(this).on("click", function(e) {
                try {
                    var elm = $(this);
                    var i = getExpandState(elm);
                    if (i == "1") {
                        //收合
                        elm.prop("_exp", "0");
                        elm.closest("li.branch").click();
                        raiseExpandEvent(elm, "1", "0");
                    }
                    else {
                        //展開
                        elm.prop("_exp", "1");
                        var lnk = raiseNodeClickEvent(elm, "1");
                        if (lnk === undefined || lnk == true) elm.closest("li.branch").click();
                        raiseExpandEvent(elm, "1", "1");
                    }
                }
                catch (ex) {
                    alert(ex.message);
                }
                finally {
                    e.preventDefault();
                }
            });
        });

        //「在分層選單內的葉節點標題」點擊事件處理
        tree.find(".branch>ul>li>i.leaf-marker+a").each(function() {
            $(this).on("click", function(e) {
                try {
                    var elm = $(this);
                    var lnk = raiseNodeClickEvent(elm, "2");
                    if (lnk === undefined || lnk == true) {
                        var url = elm.attr("href");
                        if (url && url.length > 0) elm.closest("li").click();
                        else e.preventDefault();
                    }
                    else e.preventDefault();
                }
                catch (ex) {
                    e.preventDefault();
                }
            });
        });

        //（公開方法）顯示最近展開的分層選單
        this.showLastExpanded = function() {
            var m = readExpandKey();
            if (m && m.id) {
                $.each(m.id, function(idx, value) {
                    expand(value);
                });
            }
        }

        //（公開方法）顯示最近點擊的選單節點
        this.showLastClicked = function() {
            var key = readClickedKey();
            var elm = findAnchor(key);
            if (elm) elm.addClass("nodeClicked");
        }

        //顯示樹型結構選單
        this.showMenu = function() {
            tree.show();
            this.showLastExpanded();
            this.showLastClicked();
        }
    }
});

//使用範例（HTML）
//<link rel="stylesheet" type="text/css" href="~/Scripts/treemenu/treemenu.css" />
//<script type="text/javascript" src="~/Scripts/treemenu/sessionstorage.js"></script>
//<script type="text/javascript" src="~/Scripts/treemenu/treemenu.js"></script>

//<ul id="mainMenu" style="display:none;">
//  <li>
//    <a href="#">基本資料</a>
//    <ul>
//      <li>
//        <a href="#">職類</a>
//        <ul>
//          <li>
//            <a href="/A1/C101M">職類分類維護作業</a>
//            <ul>
//              <li>
//                <a href="/A1/C111M">術科工具職類維護</a>
//                <ul>
//                  <li>
//                    <a href="">連結一</a>
//                    <ul>
//                      <li><a href="">台北</a></li>
//                      <li><a href="">台中</a></li>
//                      <li><a href="">台南</a></li>
//                    </ul>
//                  </li>
//                  <li><a href="">連結二</a></li>
//                  <li><a href="">連結三</a></li>
//                  <li><a href="">連結四</a></li>
//                </ul>
//              </li>
//              <li><a href="/A1/C113M">術科工具種類類維護</a></li>
//              <li><a href="/A1/C110M">職類監評人員需求基準數設定</a></li>
//            </ul>
//          </li>
//        </ul>
//      </li>
//    </ul>
//  </li>
//</ul>

//使用範例（Javascript）
//function markupTreeMenu() {
//    try {
//        var cfg = {
//            "onNodeClick": function(args) {
//                alert("Node=" + JSON.stringify(args));
//            },
//            "onBranchClick": function(args) {
//                alert("Branch=" + JSON.stringify(args));
//            },
//            "onLeafClick": function(args) {
//                alert("Leaf=" + JSON.stringify(args));
//            }
//        }
//        var elm = $("ul#mainMenu");
//        if (elm) {
//            elm.treeMenu(cfg);
//            elm.showMenu();
//        };
//    }
//    catch (ex) {
//        blockAlert(ex.message);
//    }
//}
//
//$(document).ready(function() {
//    markupTreeMenu();
//});