/**
 * Select2 wrapper.
 * @date: 2020 / 08 / 16
 * @version: 2.0.0
 * @author BarrY
 */
;
(function($) {
  'use strict';

  $.TUSelect2 = {
    /**
     *
     *
     * @var Object _baseConfig
     */
    _baseConfig: {
      width: '100%',
      language: 'zh-TW',
      placeholder: '請選擇',
      minimumResultsForSearch: Infinity,
      maximumSelectionLength: 2,
      minimumInputLength: 0,
      tags: false,
      ajax: null,
      templateResult: undefined,
      allowClear: true,
      selectOnClose: false,
      matchStart: null,
      theme: 'bt-line'
    },

    /**
     *
     *
     * @var jQuery pageCollection
     */
    pageCollection: $(),

    /**
     * Initialization of Masked input wrapper.
     *
     * @param String selector (optional)
     * @param Object config (optional)
     *
     * @return jQuery pageCollection - collection of initialized items.
     */

    init: function(selector, config) {

      this.collection = selector && $(selector).length ? $(selector) : $();
      if (!$(selector).length) return;

      this.config = config && $.isPlainObject(config) ?
        $.extend({}, this._baseConfig, config) : this._baseConfig;

      this.searchInputPlaceholder();

      this.config.itemSelector = selector;

      this.initSelect();

      return this.pageCollection;

    },

    initSelect: function() {
      //Variables
      var $self = this,
        config = $self.config,
        collection = $self.pageCollection;


      //Actions
      this.collection.each(function(i, el) {
        //Variables
        var $this = $(el),
          width = $this.data('width'),
          placeholder = $this.data('placeholder'),
          search = false || Boolean($this.data('search')),
          theme = $this.data('theme'),
          max = $this.data('max');


        $this.select2({
          minimumInputLength: config['minimumInputLength'],
          minimumResultsForSearch: search ? 1 : config['minimumResultsForSearch'],
          maximumSelectionLength: max ? max : config['maximumSelectionLength'],
          width: width ? width : config['width'],
          placeholder: placeholder ? placeholder : config['placeholder'],
          searchInputPlaceholder: '請輸入關鍵字搜尋 / Key Word ',
          data: config['data'],
          ajax: config['ajax'],
          language: 'zh-TW',
          templateResult: config['templateResult'],
          allowClear: config['allowClear'],
          matchStart: config['matchStart'],
          selectOnClose: config['selectOnClose'],
          theme: theme ? theme : config['theme'],

        });



        // if (setControlClasses) {
        //   $this.next().find('.chosen-single div').addClass(setControlClasses);
        // }

        // if (setOpenIcon) {
        //   $this.next().find('.chosen-single div b').append('<i class="' + setOpenIcon + '"></i>');

        //   if (setCloseIcon) {
        //     $this.next().find('.chosen-single div b').append('<i class="' + setCloseIcon + '"></i>');
        //   }
        // }

        // Actions
        collection = collection.add($this);
      });
    },

    searchInputPlaceholder: function() {
      var Defaults = $.fn.select2.amd.require('select2/defaults');

      $.extend(Defaults.defaults, {
        searchInputPlaceholder: ''
      });

      var SearchDropdown = $.fn.select2.amd.require('select2/dropdown/search');

      var _renderSearchDropdown = SearchDropdown.prototype.render;

      SearchDropdown.prototype.render = function(decorated) {

        // invoke parent method
        var $rendered = _renderSearchDropdown.apply(this, Array.prototype.slice.apply(arguments));

        this.$search.attr('placeholder', this.options.get('searchInputPlaceholder'));

        return $rendered;
      };
    },
  };

})(jQuery);