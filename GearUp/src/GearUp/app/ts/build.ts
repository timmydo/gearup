/// <reference path="app.ts" />

App.BuildRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.getBuild(params.bid);
	},
	actions: {
		invalidateModel: function () {
			Ember.Logger.log('Route is now refreshing...');
			this.refresh();
		},
		saveBuild: function () {
			var model = this.modelFor(this.routeName);
			var data = JSON.stringify(model);
			if (data) {
				model.save().then(() => {
					this.send('setInfo', 'Saved changes');
				}, (xhr) => {
					this.send('setError', 'Error saving build: ' + xhr.responseText);
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

	userLoginKey: function () {
		return window['UserIdentityKey'] || '';
	}.property('window.UserIdentityKey'),

	canEditBuild: function () {
		return this.get('model.creator') === window['UserIdentityKey'];
	}.property('model.creator'),

	userBuildList: function (key, value, previousValue) {
		var userKey = this.get('userLoginKey');

		if (arguments.length > 1) {
			//setter
			return value;
		}

		if (userKey && !value) {
			App.Data.getUserList(userKey).then((data) => {
				this.set('userBuildList', data);
				return data;
			},(xhr) => {
					console.log(xhr);
					this.send('setError', 'Error getting user build list: ' + xhr.responseJSON);
				});
		} else {
			console.log('Not loading userbuild list');
			console.log(userKey);
			console.log(value);
		}

		return value || [];
	}.property('userLoginKey'),


	editTitle: false,
	savedTitle: '',

	progressBars: [],

	selectedParts: [],

	actions: {
		addBuildToList: function (listId) {
			var build = this.get('model');
			
			App.Data.getList(listId).then((list) => {
				list.addBuildToList(build.id).then(() => {
					this.send('setInfo', 'Build added to list');
				}, (xhr) => {
					this.send('setError', 'Error adding build to list: ' + xhr.responseText);
				});
			});
		},
		deletePart: function (part) {
			var parts = this.get('parts');
			parts.removeObject(part);
			this.send('saveBuild');
		},
		addPart: function () {
			if (this.get('canEditBuild')) {
				this.set('parts', this.get('parts').concat({ url: '', title: 'New part', price: '' }));
			}
		},
		tryDeleteBuild: function () {
			this.set('tryDelete', !this.get('tryDelete'));
		},
		deleteBuild: function () {
			this.set('tryDelete', false); // try preventing doubleclick
			var model = this.get('model');
			model.deleteBuild().then((data) => {
				console.log(data);
				this.transitionToRoute('userbuilds', model.creator);
			}, (xhr) => {
				console.log(xhr);
				this.send('setError', 'Error deleting build: ' + xhr.responseText);
			});
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

