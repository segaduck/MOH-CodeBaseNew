/**
 * TuCore Jquery
 *
 * @ Author   BarrY
 * @ Version  1.2.2
 * @ Update   2022 / 03 / 22
 */
"use strict";
(function($) {

  $.TuCore = {

    init: function() {

      var sideMenuBar = $('#SideBar');

      $(document).ready(function() {

        if ($('#slider').length) {
          var init = function() {

            var slider = new rSlider({
              target: '#slider',
              values: [110.01, 110.02, 110.03, 110.04, 110.05, 110.06, 110.07, 110.08, 110.09, 110.10, 110.11, 110.12, 111.01, 111.02, 111.03, 111.04, 111.05, ],
              range: true,
              set: [111.01, 111.05],
              onChange: function(vals) {
                console.log(vals);
              }
            });
          };
          window.onload = init;
        }

        $('[data-toggle="tooltip"]').tooltip();
        $('[data-toggle="popover"]').popover();


        if ($('[data-toggle="action"]').length) {
          $.TuCore.charts.Toggle.init('[data-toggle="action"]');
        }

        // Set Background
        if ($('[data-bg-img-src]').length) {
          $.TuCore.helpers.bgImage($('[data-bg-img-src]'));
        };

        // chartist
        if ($('.js-area-chart').length)
          $.TuCore.charts.areaChart.init('.js-area-chart');

        if ($('.js-donut-chart').length)
          $.TuCore.charts.donutChart.init('.js-donut-chart');

        if ($('.js-bar-chart').length)
          $.TuCore.charts.barChart.init('.js-bar-chart');

        if ($('.js-pie-chart').length)
          $.TuCore.charts.pieChart.init('.js-pie-chart');

        // circles
        if ($('.js-pie').length)
          $.TuCore.charts.circles.init('.js-pie');

        if ($('.js-vr-progress-bar').length)
          $.TuCore.charts.progressideMenuBar.init('.js-vr-progress-bar');

        if ($('.js-counter').length) {
          $.TuCore.components.counter.init('.js-counter');
        };

        if ($('.js-go-to').length) {
          $.TuCore.components.GoToTop.init('.js-go-to')
        }
        // Side Bar Scroll
        if ($('.js-scrollbar').length) {
          $.TUScrollBar.init($('.js-scrollbar'));
        };

        if ($('.js-x-scrollbar').length) {
          $.TUScrollBar.init($('.js-x-scrollbar'), {
            axis: "x",
            advanced: {
              autoExpandHorizontalScroll: true,
              updateOnBrowserResize: true
            }
          })
        };
        $.TuCore.sideMenu.init('#SideBar');
      });

      $(window).on('load', function() {
        $.TuCore.helpers.detectIE();
        $.TuCore.components.clock.init('#current_time');

        if ($('#lottie_load').length) {
          var animation = bodymovin.loadAnimation({
            container: document.getElementById('lottie_load'),
            renderer: 'canvas',
            loop: false,
            autoplay: true,
            // path: "https://assets10.lottiefiles.com/packages/lf20_aawlikdj.json"
            path: 'https://assets4.lottiefiles.com/packages/lf20_j7eupod8.json'
          });
          animation.onComplete = function() {
            $('#loader').fadeOut();
          }
        }
      });
    },

    sideMenu: {
      init: function(selector) {

        this.collection = selector && $(selector).length ? $(selector) : $();
        if (!$(selector).length) return;

        var sideMenuBar = this.collection,
          wd = $(window),
          wdW = wd.width();
        var sideMenuBar_width = sideMenuBar.width();
        var mcnt = $('#MainContent');

        this.sideFixed();

        window.addEventListener('scroll', this.sideFixed);

        if (wdW <= 992) {
          sideMenuBar.addClass('min_size');
          $('.side-main-list-item[data-sub="true"]').removeClass('open');
        } else {
          sideMenuBar.removeClass('min_size');
          $('.side-main-list-item.active').addClass('open')
        }

        $.TuCore.helpers.resize(wd, function() {
          var nwW = wd.innerWidth();
          if (nwW <= 992) {
            sideMenuBar.addClass('min_size');
            $('.side-main-list-item[data-sub="true"]').removeClass('open');
          } else {
            sideMenuBar.removeClass('min_size');
            $('.side-main-list-item.active').addClass('open')
          }
          // $.TuCore.helpers.doResize();
        });

        (function sideMenuClick() {

          // Sub Menu Click
          $('.side-main-list-item[data-sub="true"]').on("click", function(event) {
            var _this = $(this);
            if (sideMenuBar.hasClass('min_size') || sideMenuBar.hasClass('click_min_size') || sideMenuBar.hasClass('side-bar-menu-with-icon')) {
              if (_this.hasClass('open')) {
                _this.removeClass("open");
                event.stopPropagation();
              } else {
                _this.siblings().removeClass("open");
                _this.addClass("open");
              }
            } else {
              if (_this.hasClass('open')) {
                _this.removeClass("open");
                event.stopPropagation();
              } else {
                _this.addClass("open");
              }
            }
          });

          $('.side-sub-menu-item[data-sub="true"]').on("click", function(event) {
            var _this = $(this);
            _this.toggleClass("open");
            event.stopPropagation();
          });
          // Menu Burger Click
          $('#BurGerMenu').on('click', function() {
            if (sideMenuBar.hasClass('click_min_size') || sideMenuBar.hasClass('min_size')) {
              sideMenuBar.removeClass('click_min_size min_size');
            } else {
              sideMenuBar.addClass('click_min_size min_size');
            }
          });
        })();
      },

      sideMenuMaps: function() {
        var sideBar = $('#SideBar');
        //  尋找麵包屑以及取功能的文字
        var breadcrumb = $('#pageBreadcrumb').find('li');
        var li_1 = breadcrumb.eq(1).text().trim();
        var li_2 = breadcrumb.eq(2).text().trim();
        //  Side 選單
        var l1_item = sideBar.find('[data-nav1^="' + li_1 + '"]');
        var sub_menu = l1_item.parent('li.backend-sidebar-item').next('ul.sub-menu');
        if (breadcrumb.length > 1) {
          l1_item.parent('li.backend-sidebar-item').addClass('active open');
          sub_menu.find('[data-nav2^="' + li_2 + '"]').parent().addClass('active');
        } else {
          $('ul.backend-sidebar-menu > li:first').addClass('active open');
        }
      },

      sideFixed: function() {
        var sideBar = $('#SideBar');
        var HeaderHeight = $('#Header').height();
        var scrollTop = $(window).scrollTop();
        if (scrollTop >= HeaderHeight) {
          sideBar.addClass('side-bar-menu-fixed');
        } else {
          sideBar.removeClass('side-bar-menu-fixed');
        }
        $.TuCore.helpers.doResize();
      }
    },

    helpers: {

      resize: function(element, callback) {
        var delay = 300;
        var controlTime = 0;

        $(window, element).resize(function() {
          var nowTime = new Date().getTime();
          if (controlTime) {
            setTimeout(function() {
              if (nowTime - controlTime > delay) {
                if (typeof callback == 'function') {
                  controlTime = callback();
                }
              }
            }, delay);
          } else {
            setTimeout(function() {
              if (typeof callback == 'function') {
                controlTime = callback();
              }
            }, delay);
            controlTime++;
          }
        })
      },

      doResize: function() {
        setTimeout(function() {
          if (document.createEvent) {
            var event = document.createEvent("HTMLEvents");
            event.initEvent("resize", true, true);
            window.dispatchEvent(event);
          } else if (document.createEventObject) {
            window.fireEvent("onresize");
          }
        }, 500);
      },

      Math: {
        /*
         * Sum 
         * $.TuCore.helpers.sum(arr);
         */
        sum: function(arr) {
          var sum = 0;
          for (var i = 0; i < arr.length; i++) {
            sum += arr[i];
          };
          return sum;
        },
        /*
         * countPercentage 
         * $.TuCore.helpers.countPercentage(arr);
         */
        countPercentage: function(countArray) {
          var num = eval(countArray.join('+'));
          var resultArray = [];
          for (var i = 0; i < countArray.length; i++) {
            var val = Math.floor((countArray[i] / num) * 100) + "%";
            resultArray.push(val);
          }
          return resultArray;
        },
      },
      // 背景替代圖片 RWD
      bgImage: function(collection) {
        if (!collection || !collection.length) return;
        return collection.each(function(i, el) {
          var $el = $(el),
            bgImageSrc = $el.data('bg-img-src');

          if (bgImageSrc) $el.css('background-image', 'url(' + bgImageSrc + ')');
        });
      },
      /*
       * 吐司彈跳訊息 
       * $.TuCore.helpers.toast(' 提示訊息 ', 'type' ,'1500');
       */
      toast: function(txt, type, duration) {
        var duration = isNaN(duration) ? 3000 : duration;
        var toast = document.createElement("div");
        toast.id = "toast";
        var toast_id = document.getElementById("toast");

        if (toast_id) toast_id.remove();

        var successIcon =
          '<path d="M22 12A10 10 0 1 1 12 2a10 10 0 0 1 10 10Z" opacity=".4"/>' +
          '<path d="M11 16a1 1 0 0 1-1-1l-3-2a1 1 0 0 1 1-2l3 3 5-5a1 1 0 0 1 1 1l-6 5a1 1 0 0 1 0 1Z"/>';
        var errorIcon =
          '<path d="M15 2H9a3 3 0 0 0-2 1L3 7a3 3 0 0 0-1 2v6a3 3 0 0 0 1 2l4 4a3 3 0 0 0 2 1h6a3 3 0 0 0 2-1l4-4a3 3 0 0 0 1-2V9a3 3 0 0 0-1-2l-4-4a3 3 0 0 0-2-1Z" opacity=".4"/>' +
          '<path d="m13 12 3-3a1 1 0 0 0-1-1l-3 3-3-3a1 1 0 0 0-1 1l3 3-3 3a1 1 0 0 0 0 1 1 1 0 0 0 1 0l3-3 3 3a1 1 0 0 0 1 0 1 1 0 0 0 0-1Z"/>';
        var infoIcon =
          '<path d="M2 13V7a5 5 0 0 1 5-5h10a5 5 0 0 1 5 5v7a5 5 0 0 1-5 5h-1a1 1 0 0 0-1 0l-2 2a1 1 0 0 1-2 0l-2-2H7a5 5 0 0 1-5-5Z" opacity=".4"/>' +
          '<path d="M15 11a1 1 0 0 1 1-1 1 1 0 0 1 1 1 1 1 0 0 1-1 1 1 1 0 0 1-1-1Zm-4 0a1 1 0 0 1 1-1 1 1 0 0 1 1 1 1 1 0 0 1-1 1 1 1 0 0 1-1-1Zm-4 0a1 1 0 0 1 1-1 1 1 0 0 1 1 1 1 1 0 0 1-1 1 1 1 0 0 1-1-1Z"/>';
        var warnIcon =
          '<path d="M11 2a2 2 0 0 1 2 0l2 2a2 2 0 0 0 1 0h2a2 2 0 0 1 2 2v2a2 2 0 0 0 0 1l2 2a2 2 0 0 1 0 2l-2 2a2 2 0 0 0 0 1v2a2 2 0 0 1-2 2h-2a2 2 0 0 0-1 0l-2 2a2 2 0 0 1-2 0l-2-2a2 2 0 0 0-1 0H6a2 2 0 0 1-2-2v-2a2 2 0 0 0 0-1l-2-2a2 2 0 0 1 0-2l2-2a2 2 0 0 0 0-1V6a2 2 0 0 1 2-2h2a2 2 0 0 0 1 0Z" opacity=".4"/>' +
          '<path d="M11 16a1 1 0 0 1 1-1 1 1 0 0 1 1 1 1 1 0 0 1-1 1 1 1 0 0 1-1-1Zm0-3V8a1 1 0 0 1 1-1 1 1 0 0 1 1 1v5a1 1 0 0 1-1 1 1 1 0 0 1-1-1Z"/>'
        var helpIcon =
          '<path d="M17 18h-4l-4 3a1 1 0 0 1-2 0v-3a5 5 0 0 1-5-5V7a5 5 0 0 1 5-5h10a5 5 0 0 1 5 5v6a5 5 0 0 1-5 5Z" opacity=".4"/>' +
          '<path d="M11 14a1 1 0 0 1 1-1 1 1 0 0 1 1 1 1 1 0 0 1-1 1 1 1 0 0 1-1-1Zm0-3a2 2 0 0 1 1-2h1a1 1 0 0 0-1-1 1 1 0 0 0-1 1 1 1 0 0 1-1 0 2 2 0 0 1 2-3 2 2 0 0 1 2 3 2 2 0 0 1-1 1v1a1 1 0 0 1-1 1 1 1 0 0 1-1-1Z"/>';
        var load =
          '<path d="M20 5a15 15 0 1 0 0 30 15 15 0 0 0 0-30zm0 27a12 12 0 1 1 0-24 12 12 0 0 1 0 24z" opacity=".24"/>' +
          '<path d="m26 10 2-3-8-2v3l6 2z">' +
          '  <animateTransform attributeName="transform" attributeType="xml" dur="0.8s" from="0 20 20" repeatCount="indefinite" to="360 20 20" type="rotate"/>' +
          '</path>';
        var text = '<font>' + txt + '</font>';

        document.body.appendChild(toast);

        switch (type) {
          case 'success':
            toast.className = 'tu-toast float-toast toast--' + type + ' fadeIn';
            toast.innerHTML = '<svg viewBox="0 0 24 24"><g fill="currentColor">' + successIcon + '</g></svg>' + text;
            break;

          case 'error':
            toast.className = 'tu-toast float-toast toast--' + type + ' fadeIn';
            toast.innerHTML = '<svg viewBox="0 0 24 24"><g fill="currentColor">' + errorIcon + '</g></svg>' + text;
            break;

          case 'info':
            toast.className = 'tu-toast float-toast toast--' + type + ' fadeIn';
            toast.innerHTML = '<svg viewBox="0 0 24 24"><g fill="currentColor">' + infoIcon + '</g></svg>' + text;
            break;

          case 'warning':
            toast.className = 'tu-toast float-toast toast--' + type + ' fadeIn';
            toast.innerHTML = '<svg viewBox="0 0 24 24"><g fill="currentColor">' + warnIcon + '</g></svg>' + text;
            break;

          case 'help':
            toast.className = 'tu-toast float-toast toast--' + type + ' fadeIn';
            toast.innerHTML = '<svg viewBox="0 0 24 24"><g fill="currentColor">' + helpIcon + '</g></svg>' + text;
            break;

          case 'default':
            toast.className = 'tu-toast float-toast toast--' + type + ' fadeIn';
            toast.innerHTML = '<svg xml:space="preserve" viewBox="0 0 40 40"><g fill="currentColor">' + load + '</g></svg>' + text;
            break;
        };

        setTimeout(function() {
          toast.classList.remove("fadeIn");
          toast.classList.add("fadeOut", "t-invisible");
        }, duration);
      },

      // 網頁字體放大 -- 變更 Html FontSize(Rem)
      cFontSize: function(collection, size) {
        if (!collection || !collection.length) return;
        var html = document.getElementsByTagName('html');
        var elem = html[0];
        // var defaultFontSize = window.getComputedStyle(elem, null).getPropertyValue('font-size');
        var upClasses = ["js-font-scale-up-1st", "js-font-scale-up-2nd", "js-font-scale-up-3rd", "js-font-scale-up-4th"];
        var downClasses = ["js-font-scale-down-1st", "js-font-scale-down-2nd", "js-font-scale-down-3rd", "js-font-scale-down-4th"];

        var upClassesLen = upClasses.length - 1 - 2;
        var downClassesLen = upClasses.length - 1 - 2;

        var elemClasses = elem.classList;
        var elClassNames = elemClasses.toString();

        function checkContains(target, str, separator) {
          return separator ?
            (separator + target + separator).indexOf(separator + str + separator) > -1 : //需要判断分隔符
            target.indexOf(str) > -1; //不需判断分隔符
        }


        // var dddd = document.getElementById("fontBtns").onclick = setCookie;


        switch (size) {
          case 'up':
            for (var i = 0; i < upClassesLen; i++) {
              if (!checkContains(elClassNames, 'js-font-scale')) {
                elem.classList.add(upClasses[0]);
                $.TuCore.helpers.toast('字體放大成功！', 'success', 1500);

                document.getElementById('fontSizeClass').value = upClasses[0];
                Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);
                return;
              } else if (elem.classList.contains(upClasses[i])) {
                elem.classList.remove(upClasses[i]);
                i++;
                elem.classList.add(upClasses[i]);
                $.TuCore.helpers.toast('字體放大成功！', 'success', 1500);

                document.getElementById('fontSizeClass').value = upClasses[i];
                Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);

                return;
              } else if (elem.classList.contains(upClasses[i + 1])) {
                $.TuCore.helpers.toast('字體已經放到最大！', 'warning', 1500);

                document.getElementById('fontSizeClass').value = upClasses[i + 1];
                Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);
                return;
              } else if (elem.classList.contains(downClasses[i])) {
                elem.classList.remove(downClasses[i]);
                $.TuCore.helpers.toast('已恢復預設大小！', 'success', 1500);

                document.getElementById('fontSizeClass').value = ''

                return;
              } else if (elem.classList.contains(downClasses[i + 1])) {
                i++;
                elem.classList.remove(downClasses[i]);
                i--;
                elem.classList.add(downClasses[i]);
                $.TuCore.helpers.toast('字體放大成功！', 'success', 1500);

                document.getElementById('fontSizeClass').value = downClasses[i];
                Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);
                return;
              }
            }
            break;
          case 'down':
            for (var i = 0; i < downClassesLen; i++) {
              if (!checkContains(elClassNames, 'js-font-scale')) {
                elem.classList.add(downClasses[0]);
                $.TuCore.helpers.toast('字體縮小成功！', 'success', 1500);

                document.getElementById('fontSizeClass').value = downClasses[0];
                Tu.saveToStorage("fontSizeClass");

                return;
              } else if (elem.classList.contains(downClasses[i])) {
                elem.classList.remove(downClasses[i]);
                i++;
                elem.classList.add(downClasses[i]);
                $.TuCore.helpers.toast('字體縮小成功！', 'success', 1500);

                document.getElementById('fontSizeClass').value = downClasses[i];
                Tu.saveToStorage("fontSizeClass");


                return;
              } else if (elem.classList.contains(downClasses[i + 1])) {
                $.TuCore.helpers.toast('字體已經縮到最小！', 'warning', 1500);

                document.getElementById('fontSizeClass').value = downClasses[i + 1];
                Tu.saveToStorage("fontSizeClass");
                Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);
                return;
              } else if (elem.classList.contains(upClasses[i])) {
                elem.classList.remove(upClasses[i]);
                $.TuCore.helpers.toast('已恢復預設大小！', 'success', 1500);

                document.getElementById('fontSizeClass').value = '';

                return;
              } else if (elem.classList.contains(upClasses[i + 1])) {
                i++;
                elem.classList.remove(upClasses[i]);
                i--;
                elem.classList.add(upClasses[i]);
                $.TuCore.helpers.toast('字體放大成功！', 'success', 1500);

                document.getElementById('fontSizeClass').value = upClasses[i];
                Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);
                return;
              }
              return;
            }
            break;
          case 'default':
            if (!checkContains(elClassNames, 'js-font-scale')) {
              $.TuCore.helpers.toast('目前已是預設大小！', 'warning', 1500);
              document.getElementById('fontSizeClass').value = '';

              return;
            } else {
              elem.classList.remove(
                upClasses[0], upClasses[1], upClasses[2], upClasses[3],
                downClasses[0], downClasses[1], downClasses[2], downClasses[3]
              );
              $.TuCore.helpers.toast('已恢復預設大小！', 'success', 1500);
              document.getElementById('fontSizeClass').value = '';
            }
            break;
            åå
        };


        Tu.saveToStorage("fontSizeClass", document.getElementById('fontSizeClass').value);
        return this.collection;
      },

      // 判斷 瀏覽器 是否為 IE 以及其版本 
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
        return false;
      },
    },

    components: {
      // 取得日期以及時間
      clock: {
        init: function(selector) {
          this.collection = selector && $(selector).length ? $(selector) : $();
          if (!$(selector).length) return;

          this.updateClock();

          return this.pageCollection;
        },
        updateClock: function() {
          var $self = this,
            collection = $self.pageCollection;
          this.collection.each(function(i, el) {
            var $this = $(el),
              _this = $this[i];

            setInterval(function() {
              Number.prototype.pad = function(n) {
                for (var r = this.toString(); r.length < n; r = 0 + r);
                return r;
              };
              var now = new Date();
              var milli = now.getMilliseconds(),
                sec = now.getSeconds(),
                min = now.getMinutes(),
                hou = now.getHours(),
                mo = now.getMonth(),
                date = now.getDate(),
                day = now.getDay(),
                yr = now.getFullYear();
              var months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
              var days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
              var months_ch = ["1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"];

              var tags = ["mon", "mon_ch", "date", "day", "year", "hour", "min", "sec"],
                corr = [months[mo], months_ch[mo], date, days[day], yr, hou.pad(2), min.pad(2), sec.pad(2)];

              for (var i = 0; i < tags.length; i++) {

                document.getElementById(tags[i]).textContent = corr[i];
              }
            }, 1000);

            setTimeout(function() {
              _this.className += "current-time fadeIn animated";
            }, 1000);
            return this.collection;
          });
        },
      },
      // 圖片預先載入
      lazyLoad: function(collection) {
        if (!collection || !collection.length) return;
        var images = document.querySelectorAll('.js-img-lazy');
        var loadImage = function(img) {
          var src = img.getAttribute('data-src');
          var bgImg = /(^|\s)js-img-lazy-bg(\s|$)/;
          if (bgImg.test(img.className)) {
            img.style.backgroundImage = "url('" + src + "')";
          } else {
            img.src = src;
          }
          img.classList.remove('js-img-lazy');
        };
        var options = {
          root: null,
          rootMargin: '0px',
          threshold: [0]
        };
        var callback = function(entries, observer) {
          entries.forEach(function(entry) {
            if (!entry.isIntersecting)
              return;
            loadImage(entry.target);
            observer.unobserve(entry.target);
          });
        };
        var observer = new IntersectionObserver(callback, options);
        images.forEach(function(el) {
          observer.observe(el);
        });
      },

      scrollBar: function(collection) {
        if (!collection || !collection.length) return;

        return collection.each(function(i, el) {
          var _this = $(this);
          var _document = $(document);
          var $el = $(el);
          var sroll = $el.data('scroll');

          if (sroll) {
            /// 滾動兼容性 ///
            var u = navigator.userAgent;
            var isAndroid = u.indexOf('Android') > -1 || u.indexOf('Adr') > -1;
            var isUc = u.indexOf('UCBrowser') > -1;
            var isiOS = !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/);

            if (isAndroid && isUc) {
              _this.on('touchstart', function() {
                _document.on('touchmove', function(e) {
                  e.preventDefault();
                });
                _document.on('touchend', function() {
                  _document.unbind();
                });
              });
            }


            _this.find('li').on('click', function() {
              var _thisW = _this.width();
              var thisWidth = $(this).width();
              var moveLeft = this.offsetLeft;
              // console.log(thisWidth);
              // console.log(moveLeft);
              if (_thisW < moveLeft + thisWidth) {
                _this[i].scrollTo({
                  left: moveLeft,
                  behavior: "smooth"
                });
              } else {
                _this[i].scrollTo({
                  left: 0,
                  behavior: "smooth"
                });
              }
            });

            var listener = (function() {
              /// 鼠標拖曳行為 ///
              var slider = _this[i];
              var isDown = false;
              var startX = '';
              var scrollLeft = '';

              slider.addEventListener('mousedown', function(e) {
                // 按下就將 flag 設為 true
                isDown = true;
                slider.classList.add('nav-grab');
                // 設立起始點
                startX = e.pageX - slider.offsetLeft;
                scrollLeft = slider.scrollLeft;
              });

              slider.addEventListener('mousemove', function(e) {
                // 滑鼠有被按下才會繼續執行
                if (!isDown)
                  return;
                // 阻止預設拖曳會選取的行為
                e.preventDefault();
                // 計算
                var x = e.pageX - slider.offsetLeft;
                var walk = (x - startX) * 3;
                slider.scrollLeft = scrollLeft - walk;
              });

              slider.addEventListener('mouseleave', function() {
                isDown = false;
                slider.classList.remove('nav-grab');
              });

              slider.addEventListener('mouseup', function() {
                isDown = false;
                slider.classList.remove('nav-grab');
              });
              slider.addEventListener("wheel", function(e) {
                slider.scrollLeft += e.deltaX;
                slider.scrollLeft += e.deltaY;
                e.preventDefault();
              });
            })();
          }
        });

      },
    },

    charts: {},

    settings: {},
  };

  $(function() {


    // window.paceOptions = {
    //   ajax: false,
    //   document: false,
    //   eventLag: false,
    //   elements: {
    //     selectors: ['.dashboard-card', '#lottie_load'],
    //   },
    //   restartOnRequestAfter: false,
    //   restartOnPushState: false
    // };

    $.TuCore.init();

    $.TuCore.components.lazyLoad($('.js-img-lazy'));

    $.TuCore.components.scrollBar($('[data-scroll="md-x"]'));

  });

})(jQuery);


/**
 * Utils Javascript
 *
 * @ Author   BarrY
 * @ Version  1.0.6
 * @ Update   2022 / 08 / 01
 */

(function(global) {

  // 命名建構
  var Tu = function(args) {
    var descriptors = Object.getOwnPropertyDescriptors(Tu);
    console.log(descriptors)
    return new Tu.Mth.init(args);
  };

  // Global_Extend 將 Obj 綁定
  function Global_Extend(target, source) {
    Utils.each(source, function(value, key) {
      target[key] = value;
      return this
    });
  };

  // 公用工具 針對 Tu.Mth 處理函式用
  // Use  -->  Tu.func() ...
  var Utils = {
    // ** Arrays Tools
    // 
    // 清除字符串兩端的空白字符
    // Target 目標字符串 
    trim: function(str) {
      // return (str || '').replace(/^(\s|\u00A0)+|(\s|\u00A0)+$/g, '');
      return str.replace(/^\s\s*/, '').replace(/\s\s*$/, '');
    },

    // 判定一個字符串是否包含另外一個字符串
    // Target 目標字符串 || Str 父字符串 || Separator 分隔符
    contains: function(target, str, separator) {
      return separator ?
        (separator + target + separator).indexOf(separator + str + separator) > -1 : //需要判斷分隔符
        target.indexOf(str) > -1; //不需判斷分隔符
    },

    // 判定目標字符串是否位於原字符串的開始之處
    // Target 目標字符串 || Str 父字符串 || Ignorecase 是否忽略大小寫
    startsWith: function(target, str, ignorecase) {
      var start_str = target.substr(0, str.length);
      return ignorecase ? start_str.toLowerCase() === str.toLowerCase() : //
        start_str === str;
    },

    // 判定目標字符串是否位於原字符串的末尾
    // Target 目標字符串 || Str 父字符串 || Ignorecase 是否忽略大小寫
    endWith: function(target, str, ignorecase) {
      var end_str = target.substring(target.length - str.length);
      return ignorecase ? end_str.toLowerCase() === str.toLowerCase() : //
        end_str === str;
    },

    // 將一個字符串重覆自身 N 次
    // Target 目標字符串 || Ignorecase 重覆次數
    repeat: function(target, n) {
      var ev = target;
      var total = "";
      while (n > 0) {
        if (n % 2 == 1) {
          total += ev;
        }
        if (n == 1) {
          break;
        }
        ev += ev;
        // >> 是右移位運算符，相當於將 n 除以 2 取其商,或說開 2 二次方
        n = n >> 1;
      }
      return total;
    },

    // 移除字符串中的 html 標簽。
    // Target 目標字符串 
    stripTags: function(target) {
      return String(target || "").replace(/<[^>]+>/g); //[^>] 匹配除>以外的任意字符
    },

    // 移除字符串中所有的 Script 標簽及內容
    // Target 目標字符串 
    stripScripts: function(target) {
      return String(target || "").replace(/<script[^>]*>([\S\s]*?)<\/script>/img); //[\S\s]*? 懶惰匹配任意字符串盡可能少
    },

    // 在字符串兩端添加雙引號，然後內部需要轉義的地方都要轉義，用於接裝 JSON的鍵名或模析系統中。
    // Target 目標字符串 
    quote: function(target) {
      //需要轉義的非法字符
      var escapeable = /["\\\x00-\x1f\x7f-\x9f]/g;
      var meta = {
        '\b': '\\b',
        '\t': '\\t',
        '\n': '\\n',
        '\f': '\\f',
        '\r': '\\r',
        '"': '\\"',
        '\\': '\\\\'
      };

      if (target.match(escapeable)) {
        return '"' + target.replace(escapeable, function(a) {
          var c = meta[a];
          if (typeof c === 'string') {
            return c;
          }
          return '\\u' + ('0000' + c.charCodeAt(0).toString(16)).slice(-4)
        }) + '"';
      }
      return '"' + target + '"';
    },

    // 將數字轉換為 每3位添加一個逗號, 123456 -> 123,456
    // Num 數字 
    numOfCom: function(num) {
      var num = "" + num;
      var reg = /(?!^)(?=(\d{3})+$)/g;
      return num.replace(reg, ',');
    },
    // 數組去重複
    // Arr 數字 Arrays [aaaa,aaaa,aaaa,b,c,d] --> [aaaa,b,c,d]
    unique: function(arr) {
      var obj = {};
      return arr.filter(ele => {
        if (!obj[ele]) {
          obj[ele] = true;
          return true;
        }
      })
    },
    // 字符串去重
    // Str 字串 Arrays [aa,aa,aa,aa] --> [a,a,a,a]
    uniq: function(str) {
      var obj = {},
        len = str.length;
      console.log(str.length)
      for (var i = 0; i < len; i++) {
        if (!obj[str[i]]) {
          str += str[i];
          obj[str[i]] = true;
        }
      }
      return str.replace(/(\w)\1+/g, '$1')
    },

    isArray: function(obj) {
      return Object.prototype.toString.call(obj) === '[object Array]';
    },

    isFunction: function(obj) {
      return Object.prototype.toString.call(obj) === '[object Function]';
    },

    each: function(obj, callback) {
      var length = obj.length;
      var isObj = (length === undefined) || this.isFunction(obj);

      if (isObj) {
        for (var name in obj) {
          if (callback.call(obj[name], obj[name], name) === false) {
            break;
          }
        }
      } else {
        for (var i = 0, value = obj[0]; i < length && callback.call(obj[i], value, i) !== false; value = obj[++i]) {}
      }
      return obj;
    },

    makeArray: function(arrayLike) {
      if (arrayLike.length != null) {
        return Array.prototype.slice.call(arrayLike, 0)
          .filter(function(ele) {
            return ele !== undefined;
          });
      }
      return [];
    },
  };

  Global_Extend(Tu, Utils);

  Tu.Global_Extend = Global_Extend;

  Tu.Props = {
    'for': 'htmlFor',
    'class': 'className',
    readonly: 'readOnly',
    maxlength: 'maxLength',
    cellspacing: 'cellSpacing',
    rowspan: 'rowSpan',
    colspan: 'colSpan',
    tabindex: 'tabIndex',
    usemap: 'useMap',
    frameborder: 'frameBorder'
  };

  // 原生 Js 物件操作簡寫 Tu.func()
  var Doms = {
    // Storage
    getStorageClass: function(gClass) {
      console.log(this)
      return document.getElementsByTagName('html')[0].classList.add(gClass);
    },
    // Storage
    saveToStorage: function(sId) {
      var fontSizeClassV = document.getElementById("" + sId + "").value;
      console.log(fontSizeClassV)
      return window.localStorage.setItem("fontSizeClass", fontSizeClassV);
    },
    // ID
    Gid: function(id) {
      return document.getElementById("" + id + "");
    },
    // Tag [0]
    Gtag: function(tag, parentNode) {
      var node = null; // 存放父節點
      var temps = [];
      if (parentNode != undefined) {
        node = parentNode;
      } else {
        node = document;
      }
      var tags = node.getElementsByTagName(tag);
      for (var i = 0; i < tags.length; i++) {
        temps.push(tags[i]);
      }
      return temps;
    },
    // Gclass 不能用 是工具函式
    Gclass: function(className, parentNode) {
      var node = null; //存放父節點
      var temps = [];
      console.log(className)
      if (parentNode != undefined) { //存在父節點時
        node = parentNode;
      } else { //不存在則默認document
        node = document;
      }
      var all = node.getElementsByTagName('*');
      for (var i = 0; i < all.length; i++) {
        //遍曆所有節點，判斷是否有包含className
        if ((new RegExp('(\\s|^)' + className + '(\\s|$)')).test(all[i].className)) {
          temps.push(all[i]);
          console.log(all)
        }
      }
      console.log(temps)
      return temps;
    },
    // QuerAll [0]
    QuerAl: function(selector) {
      return document.querySelectorAll("" + selector + "")
    },
    // Quer [0]
    Quer: function(selector) {
      return document.querySelector("" + selector + "")
    },
  };

  Global_Extend(Tu, Doms);

  // Mth 選擇器
  Tu.Mth = Tu.prototype = {

    getClass: function(className, parentNode) {
      var node = null; //存放父節點
      var temps = [];
      if (parentNode != undefined) { //存在父節點時
        node = parentNode;
      } else { //不存在則默認document
        node = document;
      }
      var all = node.getElementsByTagName('*');
      for (var i = 0; i < all.length; i++) {
        //遍曆所有節點，判斷是否有包含className
        if ((new RegExp('(\\s|^)' + className + '(\\s|$)')).test(all[i].className)) {
          temps.push(all[i]);
        }
      }
      return temps;
    },

    elemChildren: function(node) {
      var node = this[0];
      var temp = {
          'length': 0,
          'push': Array.prototype.push,
          'splice': Array.prototype.splice
        },
        child = node.childNodes; // 缓存所有子节点
      // 遍历子节点
      for (var i = 0; i < child.length; i++) {
        var childItem = child[i]; //缓存单个子节点
        // 通过nodeType的值筛选出元素节点
        if (childItem.nodeType === 1) {
          // 类数组的push添加元素的逻辑
          // temp[temp['length']] = childItem;
          // temp['length']++;
          temp.push(childItem);
        }
      }
      return temp[0];
      // return this.init(temp);
    },

    init: function(args, node) {
      var _self = this,
        doc = document;

      if (node && node[0]) {
        node = node[0];
      } else { // 否則，使用document
        node = document;
      }
      // 創建一個數組，用來保存獲取的節點或節點數組
      elements = [];
      // 當參數是一個字符串，說明是常規 css 選擇器，不是this,或者function
      if (typeof args == 'string') {
        // Css 模擬，就是跟 Css 後代選擇器一樣
        if (args.indexOf(' ') != -1) {
          // 把節點拆分開並保存在數組裏
          var elements = args.split(' ');
          // 存放臨時節點對象的數組，解決被覆蓋問題
          var childElements = [];
          var node = []; //用來存放父節點用的
          for (var i = 0; i < elements.length; i++) {
            // 如果默認沒有父節點，就指定document
            if (node.length == 0) node.push(document);
            switch (elements[i].charAt(0)) {
              // id
              case '#':
                // 先清空臨時節點數組
                childElements = [];
                childElements.push(Tu.Gid(elements[i].substring(1)));
                node = childElements; // 保存到父節點
                break;
                // 類
              case '.':
                childElements = [];
                // 遍曆父節點數組，匹配符合className的所有節點
                for (var j = 0; j < node.length; j++) {
                  var temps = Tu.Gclass(elements[i].substring(1), node[j]);
                  for (var k = 0; k < temps.length; k++) {
                    childElements.push(temps[k]);
                  }
                }
                node = childElements;
                break;
                //標簽
              default:
                childElements = [];
                for (var j = 0; j < node.length; j++) {
                  var temps = Tu.Gtag(elements[i], node[j]);
                  for (var k = 0; k < temps.length; k++) {
                    childElements.push(temps[k]);
                  }
                }
                node = childElements;

            }
          }
          elements = childElements;
        } else {
          // Find模擬,就是說只是單一的選擇器
          switch (args.charAt(0)) {
            case '#':
              elements.push(Tu.Gid(args.substring(1)));
              break;
            case '.':
              elements = Tu.Gclass(args.substring(1));
              break;
            default:
              elements = Tu.Gtag(args);
          }
        }
      } else if (typeof args == 'Object') {
        if (args != undefined) {

          elements[0] = args;
        }
      } else if (typeof args == 'function') {
        _self.ready(args);
      }
      console.log(elements.length);

      _self.length = elements.length;
      Tu.Global_Extend(_self, elements);
      return this;
    },

    // children: function(tagName) {
    //   var result = [];
    //   var curEle = this[0];
    //   var childList = Array.prototype.slice.call(curEle.childNodes);
    //   console.log(childList)
    //   tagName = tagName.toLowerCase();

    //   // Array.prototype.slice.call(children);
    //   for (var i = 0; i < childList.length; i += 1) {
    //     var item = childList[i];
    //     if (item.nodeType === 1) {
    //       if (typeof tagName !== 'undefined') {
    //         console.log(item)
    //         //=> 保证比较時的统一性，全部转化为小写进行比较
    //         if (item.tagName.toLowerCase() === tagName.toLowerCase()) {
    //           result.push(item);
    //         }
    //       } else {
    //         // console.log(item)
    //         //=> 没有传标签名，默认返回全部儿子辈子元素
    //         result.push(item);
    //       }
    //     }
    //   }
    //    // console.log(typeof this.init(result[0]), this[0])
    //   return this.init(result)
    // },
    children: function(selector, log) {
      // if (!el || !el.childNodes) {
      //   return null;
      // }
      var el = this[0];
      console.log(el)
      var result = [],
        i = 0,
        l = el.childNodes.length;

      for (var i; i < l; ++i) {
        if (el.childNodes[i].nodeType == 1 && TuUtil.matches(el.childNodes[i], selector, log)) {
          result.push(el.childNodes[i]);
        }
      }
      console.log(result)
      return result[0];
    },

    prev: function(curEle) {
      var curEle = this;
      // console.log(curEle.previousElementSibling);
      var prev = curEle[0].previousElementSibling;
      // return Tu.each(this, function(curEle) {
      //   var curEle = this

      //   console.log(curEle);
      //   var pre = curEle.previousSibling;

      //   while (pre && pre.nodeType !== 1) {    
      //     console.log(pre)
      //     pre = pre.previousSibling;  
      //     console.log(pre)
      //   }
      console.log(prev)
      return prev

      // });
      // return prev;
    },

    next: function(curEle) {
      var curEle = this[0];
      var next = curEle.nextElementSibling;
      return this.init(next);
    },


    sibling: function(el) {

    },

    hide: function() {
      Tu.each(this, function(element) {
        if (typeof element !== 'undefined') {
          element.style.display = 'none';
        }
      });
    },

    show: function(display) {
      Tu.each(this, function(element) {
        if (typeof element !== 'undefined') {
          element.style.display = (display ? display : 'block');
        }
      });
    },

    size: function() {
      return this.length;
    },

    isEmpty: function() {
      return this.length === 0;
    },

    each: function(callback) {
      return Tu.each(this, callback);
    },

    html: function(cur, html) {
      var cur = this[0];
      console.log(cur);
      // return this[0].innerHTML = html
      // return this[0].innerHTML = html;
      // return Tu.each(this, function(element, index) {
      //   if (element.nodeType === 1) {

      //     console.log(element);
      //     element.innerHTML = html;
      //   }
      // });
      if (html === undefined) {
        return this[0] && this[0].nodeType === 1 ?
          this[0].innerHTML : null;
      } else {
        return Tu.each(this, function(element, index) {
          console.log(element)
          if (element.nodeType === 1) {
            element.innerHTML = html;
          }
        });
      }

    },
    append: function(html) {
      console.log(this)
      return Tu.each(this, function(element) {
        console.log(element)
        element.insertAdjacentHTML('beforeend', html)
      });

    },

    remove: function() {
      return Tu.each(this, function(element) {
        element.parentNode.removeChild(element);
      });
    },
    attr: function(name, value) {
      name = Tu.props[name] || name;
      if (value !== undefined) {
        return Tu.each(this, function(element) {
          console.log(element.nodeType);
          if (element.nodeType !== 3 && element.nodeType !== 8) {
            element.setAttribute(name, value);
          }
        });

      } else {
        console.log(this);
        return this[0].getAttribute(name);

      }
    },

    hasAttr: function(name) {
      return this[0].getAttribute(name) ? true : false;
    },

    removeAttr: function(name) {
      this[0].removeAttribute(name);
    },

    val: function(value) {

      if (value === undefined) {
        return this[0] && this[0].nodeName === 'INPUT' ?
          this[0].value : null;
      } else {
        return Tu.each(this, function(element) {
          if (element.nodeName === 'INPUT') {
            element.value = value;
          }
        });
      }
    },



    isInViewport: function() {
      var rect = this[0].getBoundingClientRect();
      return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
      );
    },

    toArray: function(arr) {
      var ary = [];
      console.log(arr)
      for (var i = arr.length; i--; ary.unshift(arr[i]));
      console.log(arr)
      return ary;
    },
  };

  Tu.Mth.init.prototype = Tu.Mth;

  global.Tu = Tu;

})(this);


document.addEventListener('DOMContentLoaded', function(el) {

  if (window.localStorage) {
    var fontSizeClass = window.localStorage.getItem("fontSizeClass");
    console.log(fontSizeClass)
    if (fontSizeClass != "") {
      document.getElementById("fontSizeClass").value = fontSizeClass;
    }
  }

  var fontBtnList = document.getElementById('fontBtns').addEventListener('click', {
    handleEvent: function(event) {
      var target = event.target;
      event.stopImmediatePropagation();
      event.preventDefault();

      switch (target.id) {
        case "scale_down":
          $.TuCore.helpers.cFontSize('#scale_down', 'down');

          break;
        case "scale_default":
          $.TuCore.helpers.cFontSize('#scale_default', 'default');

          break;
        case "scale_up":
          $.TuCore.helpers.cFontSize('#scale_up', 'up');

          break;
      }
    },

  }, false);
});