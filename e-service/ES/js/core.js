// /**
//  * 
//  * Select2 Set Placeholder
//  * 
//  */
// (function($) {

//   var Defaults = $.fn.select2.amd.require('select2/defaults');

//   $.extend(Defaults.defaults, {
//     searchInputPlaceholder: ''
//   });

//   var SearchDropdown = $.fn.select2.amd.require('select2/dropdown/search');

//   var _renderSearchDropdown = SearchDropdown.prototype.render;

//   SearchDropdown.prototype.render = function(decorated) {

//     // invoke parent method
//     var $rendered = _renderSearchDropdown.apply(this, Array.prototype.slice.apply(arguments));

//     this.$search.attr('placeholder', this.options.get('searchInputPlaceholder'));

//     return $rendered;
//   };

// })(window.jQuery);

/**
 * 
 * Main
 * 
 */
var rMain = (function(window, jQuery) {
  if (!window.jQuery) {
    throw new Error("rMain requires jQuery")
  }
  var $ = window.jQuery;
  var _this = this;

  return {

    init: function() {
      /**
       * On Load
       */
      $(window).on('load', function() {
        // LOADING 
        $("#loader").fadeOut('slow');
        // IE DECIDE
        rMain.detectIE();
      });

      /**
       * ON READY
       */
      $(document).ready(function() {

        $('[data-toggle="popover"]').popover();
        $('[data-toggle="tooltip"]').tooltip();

        if ($('.js-select-2').length)
          $.TUSelect2.init('select.js-select-2');
        // // SELECT2
        // $('#payBasic_dirct , #payBasic_dirct_tr').select2({
        //   width: '100%',
        //   placeholder: '請選擇國家',
        //   searchInputPlaceholder: '請輸入關鍵字搜尋',
        //   language: {
        //     noResults: function() {
        //       return "找不到符合的資料 No Results Found.";
        //     }
        //   }
        // });

        // RT-SCROLL FIXED 
        $('#check_form').scrollToFixed({
          minWidth: 768,
          // marginTop: 20,

          limit: function() {
            var limit = $('#footer').offset().top - ($('#check_form').outerHeight() + $('#header').outerHeight());
            return limit;
          },
          zIndex: 999,
          fixed: function() {
            $(this).addClass('scroll-fixed')
          },
          unfixed: function() {
            $(this).removeClass('scroll-fixed')
          },
        });


      });

    },
    /**
     * Toast 吐司彈跳訊息
     */
    toast: function(txt, type, time) {
      var time = isNaN(time) ? 3E3 : time;
      var toast = document.createElement('div');
      var oldToast = document.getElementsByClassName('toast');

      if (oldToast.length != 0) {
        document.querySelectorAll(".toast")[0].remove()
      }

      switch (type) {
        case 'success':
          toast.className = 'toast toast--success fadeIn';
          toast.innerHTML = '<i class="tu-check-circle" aria-hidden="true"></i> <font>' + txt + '</font>';
          break;
        case 'error':
          toast.className = 'toast toast--error fadeIn';
          toast.innerHTML = '<i class="tu-x-circle" aria-hidden="true"></i> <font>' + txt + '</font>';
          break;
        case 'info':
          toast.className = 'toast toast--info fadeIn';
          toast.innerHTML = '<i class="tu-info" aria-hidden="true"></i> <font>' + txt + '</font>';
          break;
        case 'alerts':
          toast.className = 'toast toast--alerts fadeIn';
          toast.innerHTML = '<i class="tu-alert-circle" aria-hidden="true"></i> <font>' + txt + '</font>';
          break;
        case 'warning':
          toast.className = 'toast toast--warning fadeIn';
          toast.innerHTML = '<i class="tu-alert-triangle" aria-hidden="true"></i> <font>' + txt + '</font>';
          break;
        case 'help':
          toast.className = 'toast toast--help fadeIn';
          toast.innerHTML = '<i class="tu-help-circle" aria-hidden="true"></i> <font>' + txt + '</font>';
          break;
      };

      document.body.appendChild(toast);

      setTimeout(function() {
        toast.classList.remove('fadeIn');
        toast.classList.add('fadeOut');
        toast.remove();
      }, time, 1000);
    },
    /**
     *  網頁字體放大 ( 變更 Html FontSize(Rem) )
     */
    cFontSize: function(size) {
      var min = 16;
      var max = 24;
      var html = document.getElementsByTagName('html');

      switch (size) {
        case 'bigger':
          for (i = 0; i < html.length; i++) {
            var elem = html[0];
            var styles = window.getComputedStyle(elem, null).getPropertyValue('font-size');

            if (elem.style.fontSize) {
              var fontSize = parseFloat(styles);
            } else {
              var fontSize = 20;
            }

            if (fontSize != max) {
              fontSize += 1;
              elem.style.fontSize = fontSize + 'px';
              rMain.toast('字體已經放大一個級距！', 'success', 1500);
            } else {
              rMain.toast('字級已經放到最大！', 'info', 1500);
            }
          };
          break;
        case 'smaller':
          for (i = 0; i < html.length; i++) {
            var elem = html[0];
            var styles = window.getComputedStyle(elem, null).getPropertyValue('font-size');

            if (elem.style.fontSize) {
              var fontSize = parseFloat(styles);
            } else {
              var fontSize = 20;
            }

            if (fontSize != min) {
              fontSize -= 1;
              elem.style.fontSize = fontSize + 'px';
              rMain.toast('字體已經縮小一個級距！', 'success', 1500);
            } else {
              rMain.toast('字級已經縮到最小！', 'info', 1500);
            }
          };
          break;
        case 'unset':
          var elem = html[0];
          var fontSize = 20;
          elem.style.fontSize = fontSize + 'px';
          break;
      };

    },
    /**
     *  判斷 瀏覽器 是否為ＩＥ以及其版本 
     */
    detectIE: function() {

      var ua = window.navigator.userAgent;
      var msie = ua.indexOf('MSIE ');

      if (msie > 0) {
        // IE 10
        var ieV = parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
        document.querySelector('body').className += 'ie ie' + ieV + ' ';

        if (ieV < 9) {
          // IE 9 以下建議使用者升級瀏覽器
          confirm("您的 IE 版本過低，點選【確定】升級，如不升級您將不能正常瀏覽網頁！")
          location.href = "https://support.microsoft.com/zh-tw/help/17621/internet-explorer-downloads";
        }

      }

      var trident = ua.indexOf('Trident/');
      if (trident > 0) {
        // IE 11
        var rv = ua.indexOf('rv:');
        var ieV = parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
        document.querySelector('body').className += 'ie ie' + ieV + ' ';
      }

      var edge = ua.indexOf('Edge/');
      if (edge > 0) {
        // IE 12
        var edgeV = parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
        document.querySelector('body').className += 'edge edge' + edgeV + ' ';
      }

      // other browser
      return false;

    },
  }
}(window, jQuery));

rMain.init();