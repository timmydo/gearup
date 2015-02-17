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

	actions: {
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

Ember.Handlebars.registerHelper('buildImage', function (name, env) {
	var guid = Ember.get(env.data.view.content, name);
	return new Ember.Handlebars.SafeString('<img class="build-image-thumbnail" src="' + App.ImageEndpoint + '/' + guid + '" />');
});



