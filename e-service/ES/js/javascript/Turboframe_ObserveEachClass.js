// 對象監聽器(單個對象 例如每張卡片、每個表單之類) 針對多個對象物件監聽 --> 小封裝 
// Element 監聽並且更動的變動對象 || 
// Defaultclassname 給監聽對象設置起始樣式 || 
// Addclassname 當目標進入可是範圍後增加的樣式 || 
// Options [root: null , 
//          rootMargin: "20px 0 0 0" , 
//          threshold: [0, 1]]

var animationObserver = function(element, defaultClassName, addClassName, options) {
  var that = this;
  // element 不在存退出
  if (typeof element === "undefined" || element === null) {
    return;
  }
  // 不支援 IntersectionObserver 時，退出
  if (!("IntersectionObserver" in window) &&
    !("IntersectionObserverEntry" in window) &&
    !("intersectionRatio" in window.IntersectionObserverEntry.prototype)
  ) {
    return;
  }
  // var options = Object.assign({}, defaultOptions, options || {});
  /////////////////////////////////
  // **   animationObserver   ** //
  /////////////////////////////////
  var defaultOptions = {
    root: null,
    rootMargin: '',
    threshold: ''
  };
  that.options = options || defaultOptions;

  var elements = document.querySelectorAll(element);

  var observer = new IntersectionObserver(function(entries) {
    entries.forEach(function(entry) {
      if (entry.isIntersecting) {
        entry.target.classList.add(addClassName);

        // 停止觀察
        setTimeout(function() {
          entry.target.classList.remove(addClassName);
          entry.target.classList.remove(defaultClassName);
        }, 200);

        observer.unobserve(entry.target);
      }
    });
  });

  elements.forEach(function(el) {
    el.classList.add(defaultClassName);
    return observer.observe(el);
  });

};