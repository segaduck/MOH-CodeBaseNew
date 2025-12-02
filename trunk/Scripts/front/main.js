// 瀏覽器版本偵測
function detectIE() {
    var ua = window.navigator.userAgent;

    var msie = ua.indexOf('MSIE ');
    if (msie > 0) {
        // IE 10 or older => return version number
        return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
    }

    var trident = ua.indexOf('Trident/');
    if (trident > 0) {
        // IE 11 => return version number
        var rv = ua.indexOf('rv:');
        return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
    }

    var edge = ua.indexOf('Edge/');
    if (edge > 0) {
        // Edge (IE 12+) => return version number
        return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
    }

    // other browser
    return false;
}

$(function() {
    var st = 0;
    var top = 0;
    var header = $('#wel_header');

    $(window).scroll(function() {
        var st = $(this).scrollTop();
        var h = header.height();

        if (st >= h * 2) {
            // wel_header Fixed 盯住
            header.addClass('sticky-top');

            // 判斷往下滾動還是往上
            if (top < st) {
                // 向下
                header
                    .removeClass('nav-shadow-show')
                    .addClass('move-up');

            } else if (top > st) {
                // 向上
                header
                    .addClass('nav-shadow-show')
                    .removeClass('move-up');
            }

            setTimeout(function() {
                top = st
            }, 500);

        } else {
            header.removeClass('sticky-top');
            header.removeClass('move-up');
        }

    });
});


var wel_medical = (function(window, jQuery) {
    if (!window.jQuery) {
        throw new Error("wel_medical requires jQuery")
    }
    var $ = window.jQuery;
    var _this = this;

    return {
        init: function() {
            // 若是IE，則加上class
            if (detectIE()) {
                // alert()
                $("html").addClass('ie ie' + detectIE());
            }

            $(document).ready(function(e) {
                // Go Top 
                $.BackToGo.init('#js-go-to');

                $("#index_news").owlCarousel({
                    stagePadding: 24,
                    margin: 8,
                    loop: true,
                    merge: true,
                    dots: false,
                    lazyLoad: true,
                    nav: true,
                    navText: ['<i class="fas fa-chevron-left"></i>', '<i class="fas fa-chevron-right"></i>'],
                    responsiveClass: true,
                    responsive: {
                        0: {
                            items: 2,
                            stagePadding: 4,
                        },
                        768: {
                            items: 3,
                            stagePadding: 8,

                        },
                        992: {
                            items: 4,
                        },
                    },
                });

                $("#index_links_fixed").owlCarousel({
                    stagePadding: 24,
                    margin: 8,
                    loop: false,
                    rewindNav: false,
                    merge: true,
                    dots: false,
                    lazyLoad: true,
                    nav: false,
                    navText: ['<i class="fas fa-chevron-left"></i>', '<i class="fas fa-chevron-right"></i>'],
                    responsiveClass: true,
                    responsive: {
                        0: {
                            items: 2,
                            stagePadding: 8,
                        },
                        768: {
                            items: 3,

                        },
                        992: {
                            items: 4,
                        },
                    },
                });

                $("#index_links").owlCarousel({
                    stagePadding: 24,
                    margin: 8,
                    loop: true,
                    merge: true,
                    dots: false,
                    lazyLoad: true,
                    nav: true,
                    navText: ['<i class="fas fa-chevron-left"></i>', '<i class="fas fa-chevron-right"></i>'],
                    responsiveClass: true,
                    responsive: {
                        0: {
                            items: 2,
                            stagePadding: 8,
                        },
                        768: {
                            items: 3,

                        },
                        992: {
                            items: 4,
                        },
                    },
                });


            });

            $(window).on('load', function(e) {



            });


        },

        AcToggle: function(element) {
            $(element).next().slideToggle(150);
        },

    }

}(window, jQuery));