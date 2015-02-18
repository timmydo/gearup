/// <reference path="app.ts" />

declare var DropletView: any;
declare var DropletController: any;


App.BuildRoute = Ember.Route.extend({
	model: function (params) {
		return Ember.$.getJSON('/api/build/' + params.bid);
	},
	actions: {
		invalidateModel: function () {
			Ember.Logger.log('Route is now refreshing...');
			this.refresh();
		}
	}
});

App.BuildController = Ember.ObjectController.extend({
	createdTime: function () {
		return moment(this.get('model.created')).format('ll');
	}.property('model.created'),
	selectedImage: function () {
		var i = this.get('model.images');
		if (i) {
			return i[0].guid;
		}
	}.property('model.images'),

	actions: {
		selectImage: function (guid) {
			this.set('selectedImage', guid);
		},
		uploadFile: function (evt, bid) {
			var files = evt.files;
			for (var i = 0; i < files.length; i++) {
				var file = files[i];
				var upload = new FileUpload(file, bid);
				upload.start().then((success) => {
					this.send('invalidateModel');
				}, (failure) => {
						console.log('upload fail');
						console.log(failure);
					});
			}
		}

	}

});


Ember.Handlebars.registerBoundHelper('buildThumbnail', function (value) {
	return new Ember.Handlebars.SafeString('<img class="build-image-thumbnail" src="' + App.ImageEndpoint + '/' + value + '" />');
});
Ember.Handlebars.registerBoundHelper('buildImage', function (value) {
	return new Ember.Handlebars.SafeString('<img class="build-image-main" src="' + App.ImageEndpoint + '/' + value + '" />');
});


