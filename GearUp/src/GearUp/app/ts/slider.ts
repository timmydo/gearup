/// <reference path="app.ts" />

App.BootstrapSliderComponent = Ember.TextField.extend({
	classNames: ['slider'],
	classNameBindings: ['type'],

	attributeBindings: ['type', 'min', 'max', 'value', 'handle', 'enabled'],

	updateValue: function () {
		//console.log('update value');
		//this.$().slider({
		//	min: this.get('min'),
		//	max: this.get('max'),
		//	value: this.get('value'),
		//	handle: this.get('handle'),
		//	tooltip: 'hide'
		//});
	}.observes('value'),

	didInsertElement: function () {
		var self = this;
		var opts = {
			min: this.get('min'),
			max: this.get('max'),
			value: this.get('value'),
			handle: this.get('handle'),
			enabled: !!this.get('enabled')
			//tooltip: 'hide'
		};
		console.log(opts);
		this.$().slider(opts);

		this.$().on('slide', function () {
			self.set('value', this.value);
		});

		this.$().on('slideStop', function () {
			self.set('value', this.value);
		});

	},

	willDestroyElement: function () {
		this.$().off('slide');
		this.$().off('slideStop');
	}

});