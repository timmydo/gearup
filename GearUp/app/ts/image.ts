/// <reference path="app.ts" />

App.ImageController = Ember.ObjectController.extend({
	editing: false,

	actions: {
		editImage: function (Image) {
			this.setProperties({
				editing: true,
				newTitle: this.get('title'),
			});
		},
		saveImage: function (Image) {
			this.setProperties({
				editing: false,
				title: this.get('newTitle'),
			});

			this.send('saveBuild');
		},
		discardImage: function (Image) {
			this.set('editing', false);
		},

	}
});

