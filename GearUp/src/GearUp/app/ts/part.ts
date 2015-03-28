/// <reference path="app.ts" />

App.PartController = Ember.ObjectController.extend({
	editing: false,

	actions: {
		editPart: function (part) {
			this.setProperties({
				editing: true,
				newTitle: this.get('title'),
				newUrl: this.get('url'),
				newPrice: this.get('price')
			});
		},
		savePart: function (part) {
			this.setProperties({
				editing: false,
				title: this.get('newTitle'),
				url: this.get('newUrl'),
				price: this.get('newPrice')
			});

			this.send('saveBuild');
		},
		discardPart: function (part) {
			this.set('editing', false);
		},

	}
});

