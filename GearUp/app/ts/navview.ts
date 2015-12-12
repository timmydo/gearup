/// <reference path="app.ts" />


App.NavView = Ember.View.extend({

	tagName: 'li',
	classNameBindings: 'active'.w(),

	didInsertElement: function () {
		this._super();
		this.get('parentView').on('click',() => {
			this.notifyPropertyChange('active');
		});
	},

	active: function () {
		return this.get('childViews.firstObject.active');
	}.property()
});
