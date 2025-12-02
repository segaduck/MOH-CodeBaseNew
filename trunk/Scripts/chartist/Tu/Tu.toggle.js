;
(function($) {
  'use strict';
  $.TuCore.charts.Toggle = {
    /**
     * Base configuration.
     *
     * @var Object _baseConfig
     */
    _baseConfig: {

    },
    /**
     *
     *
     * @var jQuery pageCollection
     */
    pageCollection: $(),
    /**
     * Initializtion of header.
     *
     * @param jQuery element
     *
     * @return jQuery
     */
    init: function(selector, config) {
      this.collection = selector && $(selector).length ? $(selector) : $();
      if (!$(selector).length) return;

      this.config = config && $.isPlainObject(config) ?
        $.extend({}, this._baseConfig, config) : this._baseConfig;

      this.config.itemSelector = selector;
      this.initToggle();
      return this.pageCollection;

    },

    initToggle: function() {
      
      var $self = this,
        config = $self.config,
        collection = $self.pageCollection;


      this.collection.each(function(i, el) {
        var $this = $(el),
          action = $this.data('toggle', 'action'),
          state = $this.data('toggle-state'),
          target = $this.data('toggle-target'),
          time = $this.data('toggle-target-durs'),
          Class = $this.data('toggle-target-class');


        $this.on('click', function(e) {
          if (time && action) {

            e.preventDefault();

            $this.addClass(state);
            $(target)
              .attr('data-custom-toggle', 'on')
              .addClass(Class);

            setTimeout(function() {
              $this.removeClass(state);
              $(target)
                .attr('data-custom-toggle', 'off')
                .removeClass(Class);
            }, time);

          } else if (action) {

            e.preventDefault();

            if ($(target).attr('data-custom-toggle') === 'on') {
              $this.removeClass(state);
              $(target)
                .attr('data-custom-toggle', 'off')
                .removeClass(Class);
            } else {
              $this.addClass(state);
              $(target)
                .attr('data-custom-toggle', 'on')
                .addClass(Class);
            }
          }
        });
      });

      return collection;
    },
  };
})
(jQuery);