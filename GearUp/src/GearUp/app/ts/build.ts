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
		if (i && i.length > 0) {
			return i[0].guid;
		}
	}.property('model.images'),


	canEditBuild: true,
	editTitle: false,
	savedTitle: '',

	progressBars: [],

	saveBuild: function () {
		var data = JSON.stringify(this.get('model'));
		console.log("Saving Build " + data);
		Ember.$.ajax({
			type: 'POST',
			url: '/api/SaveBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'json',
			success: (data, status) => {
				console.log(status);
				console.log(data);
			},
			error: (xhr, status, err) => {
				console.log(xhr);
				console.log(status);
				console.log(err);
				this.send('setError', 'Error saving build: ' + err);
			}
		});
	},

	actions: {
		addPart: function () {
			if (this.canEditBuild) {
				this.set('parts', this.get('parts').concat({url:'', title:'New part', price:''}));
			}
		},
		startEditTitle: function () {
			if (this.canEditBuild) {
				this.savedTitle = this.get('title');
				this.set('editTitle', true);
			}
		},
		discardTitle: function () {
			this.set('editTitle', false);
			this.set('title', this.savedTitle);
		},
		saveTitle: function () {
			this.set('editTitle', false);
			this.saveBuild();
		},
		selectImage: function (guid) {
			this.set('selectedImage', guid);
		},
		uploadFile: function (evt, bid) {
			var files = evt.files;
			for (var i = 0; i < files.length; i++) {
				var file = files[i];

				var progressFunc = (guid, val) => {
					console.log('progress ' + guid + ' ' + val);
					this.progressBars.forEach((item, idx) => {
						if (item.guid === guid) {
							this.progressBars[idx].progress = val;
						}
					});

					this.get('progressBars').setObjects(this.progressBars);

				}
				var upload = new FileUpload(file, bid, progressFunc);
				this.get('progressBars').pushObject({ guid: upload.guid, name: upload.file.name, progress: 0 });
				((upload) => {
					upload.start().then((success) => {
						console.log("Removing upload " + upload.guid);
						this.get('progressBars').setObjects(this.progressBars.filter(function (x) { return x.guid !== upload.guid; }));
						this.send('invalidateModel');
					},(failure) => {
							console.log('upload fail');
							console.log(failure);
							this.send('setError', 'Error uploading file: ' + failure);
							this.get('progressBars').setObjects(this.progressBars.filter(function (x) { return x.guid !== upload.guid; }));
						});
				})(upload);
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


