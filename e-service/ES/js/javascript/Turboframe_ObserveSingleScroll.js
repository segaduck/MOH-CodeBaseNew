// 對象監聽器 (Just ID) --> 針對單一物件監聽 --> 小封裝 
// Element 監聽並且更動的變動對象(正常來說就 Target) || 
// PrevElement 在此函式裡面代表所監聽對象之同級上一個 ( IntersectionObserver 監聽是看觀察對象有沒有碰撞 ，算不出 scroll 所以改監聽上一個對象碰撞事件) ||
// ClassName || options [root: null || 
// Options [root: null , 
//          rootMargin: "20px 0 0 0" , 
//          threshold: [0, 1]]

var scrollObserver = function(element, prevElement, className, options) {
  var that = this;
  // element 不在存退出
  if (typeof element === "undefined" || element === null) {
    return;
  }
  element = document.getElementById(element);
  prevElement = document.getElementById(prevElement) || element;

  // 不支援 IntersectionObserver 時，改用 getBoundingClientRect 監聽 
  if (!("IntersectionObserver" in window) &&
    !("IntersectionObserverEntry" in window) &&
    !("intersectionRatio" in window.IntersectionObserverEntry.prototype)
  ) {
    var initialCords = element.getBoundingClientRect();
    document.addEventListener('scroll', function() {
      if (window.scrollY >= initialCords.top) {
        element.classList.add(className);
      } else {
        element.classList.remove(className);
      }
    });
  }
  //////////////////////////////
  // **   scrollObserver   ** //
  //////////////////////////////
  var defaultOptions = {
    root: null,
    rootMargin: '',
    threshold: ''
  };

  that.options = options || defaultOptions ;

  var observer = new IntersectionObserver(function(entries) {
    entries.forEach(function(entry) {
      if (entry.isIntersecting ) {
        element.classList.remove(className);
      } else if (entry.boundingClientRect.top <= 0) {
        element.classList.add(className);
      }
    });
  });
  return observer.observe(prevElement);
};
