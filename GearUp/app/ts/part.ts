/// <reference path="app.ts" />

App.PartController = Ember.ObjectController.extend({
	editing: function () {
		return this.get('model.title') === '';
	}.property('title'),

	actions: {
		editPart: function () {
			this.setProperties({
				editing: true,
				newTitle: this.get('Title'),
				newUrl: this.get('Url'),
				newPrice: this.get('Price')
			});
		},
		savePart: function () {
			this.setProperties({
				editing: false,
				Title: this.get('newTitle'),
				Url: this.get('newUrl'),
				Price: this.get('newPrice')
			});

			this.send('saveBuild');
		},
		discardPart: function () {
			this.set('editing', false);
		},

	}
});

