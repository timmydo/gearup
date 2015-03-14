/// <reference path="app.ts" />

App.BuildRoute = Ember.Route.extend({
	model: function (params) {
		return Ember.$.getJSON('/api/build/' + params.bid);
	},
	actions: {
		invalidateModel: function () {
			Ember.Logger.log('Route is now refreshing...');
			this.refresh();
		},
		saveBuild: function () {
			var model = this.modelFor(this.routeName);
			var data = JSON.stringify(model);
			console.log("Saving Build " + data);
			if (data) {
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
						this.send('setError', 'Error saving build: ' + xhr.responseJSON.Message);
					}
				});
			}
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


	canEditBuild: function () {
		return this.get('model.creator') === window['UserIdentityKey'];
	}.property('model.creator'),
	editTitle: false,
	savedTitle: '',

	progressBars: [],

	selectedParts: [],

	actions: {
		deletePart: function (part) {
			var parts = this.get('parts');
			parts.removeObject(part);
			this.send('saveBuild');
		},
		addPart: function () {
			if (this.get('canEditBuild')) {
				this.set('parts', this.get('parts').concat({url:'', title:'New part', price:''}));
			}
		},
		startEditTitle: function () {
			if (this.get('canEditBuild')) {
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
			this.send('saveBuild');
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
					this.get('progressBars').forEach((item, idx) => {
						if (item.guid === guid) {
							Ember.set(item, 'progress', val);
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

