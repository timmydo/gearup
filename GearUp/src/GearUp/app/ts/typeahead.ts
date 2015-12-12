/// <reference path="app.ts" />


App.TypeAheadComponent = Ember.TextField.extend({
  classNames: ['type-ahead', 'form-control'],
  didInsertElement: function() {
    this._super();
    var elem = $(this.element);
    var datasource = {
      name: this.$().attr('id') || "typeahead",
      limit: this.get("limit") || 15,
      source: (q, sr, asr) => {
        this.sendAction('action', q, sr, asr);
      },
      async: true,
      displayKey: this.get("display"),
      valueKey: this.get("display") || this.get("valueKey")
    };
    
    console.log(datasource);
    this.typeahead = elem.typeahead({
      minLength: 3,
      highlight: true
    },
    datasource);

    this.typeahead.on("typeahead:selected", (event, item) => {
      this.set("selection", item.emberObject);
    });

    this.typeahead.on("typeahead:autocompleted", (event, item) => {
      this.set("selection", item.emberObject);
    });

    if (this.get("selection")) {
      this.typeahead.val(this.get("selection.name"));
    }

  },


  selectionObserver: function() {
    if (Ember.isEmpty(this.get('selection'))) {
      return this.typeahead.val('');
    }
    return this.typeahead.val(this.get("selection").get(this.get("name")));
  }.observes("selection")

});
  

