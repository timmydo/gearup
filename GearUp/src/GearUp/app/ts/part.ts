/// <reference path="app.ts" />

App.PartController = Ember.ObjectController.extend({
	editing: function () {
		return this.get('model.title') === '';
	}.property('title'),

	actions: {
		editPart: function () {
			this.setProperties({
				editing: true,
				newTitle: this.get('title'),
				newUrl: this.get('url'),
				newPrice: this.get('price')
			});
		},
		savePart: function () {
			this.setProperties({
				editing: false,
				title: this.get('newTitle'),
				url: this.get('newUrl'),
				price: this.get('newPrice')
			});

			this.send('saveBuild');
		},
		discardPart: function () {
			this.set('editing', false);
		},

	}
});

