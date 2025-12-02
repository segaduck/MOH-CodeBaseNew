"use strict"; // Class definition

var JsTuScrolltop = function JsTuScrolltop(element, options) {
  ////////////////////////////
  // ** Private variables  ** //
  ////////////////////////////
  var that = this;
  var body = document.getElementsByTagName("BODY")[0];

  if (typeof element === "undefined" || element === null) {
    return;
  } // Default options


  var defaultOptions = {
    offset: 400,
    speed: 250
  }; 
  ////////////////////////////
  // ** Private methods  ** //
  ////////////////////////////

  var _construct = function _construct() {
    if (JsUtils.data(element).has('scrolltop')) {
      that = JsUtils.data(element).get('scrolltop');
    } else {
      _init();
    }
  };

  var _init = function _init() {
    // Variables
    that.options = JsUtils.deepExtend({}, defaultOptions, options);
    that.uid = JsUtils.getUniqueId('scrolltop');
    that.element = element; // Set initialized
    that.element.setAttribute('data-tu-scrolltop', 'true'); // Event Handlers
    _handlers();
    JsUtils.data(that.element).set('scrolltop', that);
  };

  var _handlers = function _handlers() {
    var timer;
    window.addEventListener('scroll', JsUtils.throttle(function() {
      _scroll();
    }, 200));
    
    JsUtils.addEvent(that.element, 'click', function(event) {
      event.preventDefault();
      _go();
    });
  };

  var _scroll = function _scroll() {
    var offset = parseInt(_getOption('offset'));
    var pos = JsUtils.getScrollTop(); // current vertical position

    if (pos > offset) {
      if (body.hasAttribute('data-tu-scrolltop') === false) {
        body.setAttribute('data-tu-scrolltop', 'on');
      }
    } else {
      if (body.hasAttribute('data-tu-scrolltop') === true) {
        body.removeAttribute('data-tu-scrolltop');
      }
    }
  };

  var _go = function _go() {
    var speed = parseInt(_getOption('speed'));
    JsUtils.scrollTop(0, speed);
  };

  var _getOption = function _getOption(name) {
    if (that.element.hasAttribute('data-tu-scrolltop-' + name) === true) {
      var attr = that.element.getAttribute('data-tu-scrolltop-' + name);
      var value = JsUtils.getResponsiveValue(attr);

      if (value !== null && String(value) === 'true') {
        value = true;
      } else if (value !== null && String(value) === 'false') {
        value = false;
      }

      return value;
    } else {
      var optionName = JsUtils.snakeToCamel(name);

      if (that.options[optionName]) {
        return JsUtils.getResponsiveValue(that.options[optionName]);
      } else {
        return null;
      }
    }
  };

  var _destroy = function _destroy() {
    JsUtils.data(that.element).remove('scrolltop');
  };

  _construct();
  ///////////////////////
  // ** Public API  ** //
  ///////////////////////

  that.go = function() {
    return _go();
  };

  that.getElement = function() {
    return that.element;
  };

  that.destroy = function() {
    return _destroy();
  };
}; 
// Static


JsTuScrolltop.getInstance = function(element) {
  if (element && JsUtils.data(element).has('scrolltop')) {
    return JsUtils.data(element).get('scrolltop');
  } else {
    return null;
  }
}; 
// Create


JsTuScrolltop.createInstances = function() {
  var selector = arguments.length > 0 && arguments[0] !== undefined ? arguments[0] : '[data-tu-scrolltop="true"]';
  var body = document.getElementsByTagName("BODY")[0]; // Initialize Menus

  var elements = body.querySelectorAll(selector);
  var scrolltop;

  if (elements && elements.length > 0) {
    for (var i = 0, len = elements.length; i < len; i++) {
      scrolltop = new JsTuScrolltop(elements[i]);
    }
  }
}; 
// Global


JsTuScrolltop.init = function() {
  JsTuScrolltop.createInstances();
}; 
// On document ready


if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', JsTuScrolltop.init);
} else {
  JsTuScrolltop.init();
} 
// Webpack support


if (typeof module !== 'undefined' && typeof module.exports !== 'undefined') {
  module.exports = JsTuScrolltop;
}