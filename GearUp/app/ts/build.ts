/// <reference path="app.ts" />

App.BuildRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.getBuild(params.bid);
	},
	actions: {
		saveBuild: function () {
			var model = this.modelFor(this.routeName);
			var data = JSON.stringify(model);
			if (data) {
				model.save().then(() => {
					this.growl.success('Saved changes');
				}, (xhr) => {
					this.growl.error('Error saving build: ' + xhr.responseText);
				});
			}
		}
	}
});

App.BuildController = Ember.ObjectController.extend({
	needs: ["Userbuilds"],

	tempModified: false,
	tempUnit: function () {
		var cel = this.get('IsCelsius');
		if (cel) {
			return 'C';
		} else {
			return 'F';
		}
	}.property('IsCelsius'),

	createdTime: function () {
		return moment(this.get('model.Created')).format('ll');
	}.property('model.Created'),
	selectedImage: function () {
		var i = this.get('model.Images');
		if (i && i.length > 0) {
			return i[0].Id;
		}
	}.property('model.images'),
	selectedImageCaption: function () {
		var images = this.get('model.Images');
		var guid = this.get('selectedImage');
		for (var i = 0; i < images.length; i++) {
			if (images[i].Id === guid) {
				return images[i].Title;
			}
		}
		return "No caption found";
	}.property('selectedImage', 'editCaption'),

	userLoginKey: function () {
		return window['UserIdentityKey'] || '';
	}.property('window.UserIdentityKey'),

	canEditBuild: function () {
		return this.get('model.Creator') === window['UserIdentityKey'];
	}.property('model.Creator'),

	editTitle: false,
	editDescription: false,
	editCaption: false,
	savedTitle: '',
	savedDescription: '',

	progressBars: [],

	selectedParts: [],

	actions: {
		addBuildToList: function (listId) {
			var build = this.get('model');
			
			App.Data.getList(listId).then((list) => {
				list.addBuildToList(build.Id).then(() => {
					this.growl.success('Build added to list');
				}, (xhr) => {
					this.growl.error('Error adding build to list: ' + xhr.responseText);
				});
			});
		},
		deletePart: function (part) {
			var parts = this.get('Parts');
			parts.removeObject(part);
			this.send('saveBuild');
		},
		deleteSelectedImage: function () {
			var image = this.get('selectedImage');
			var b = this.get('model');
			this.set('editCaption', false);
			//b.images.removeObject(image);
			console.log(b);
			console.log(image);
			b.deleteImageFromBuild(image).then((x) => {
				var i = this.get('model.Images');
				if (i && i.length > 0) {
					this.set('selectedImage', i[0].Id);
				}
				this.growl.success('Image deleted');

			},(xhr) => {
					this.growl.error('Error adding build to list: ' + xhr.responseText);
				});
		},

		addPart: function () {

			if (this.get('canEditBuild')) {
				this.get('Parts').pushObject({ Title: '' });
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
				this.transitionToRoute('userbuilds', model.Creator);
			}, (xhr) => {
				console.log(xhr);
				this.growl.error('Error deleting build: ' + xhr.responseText);
			});
		},
		startEditTitle: function () {
			if (this.get('canEditBuild')) {
				this.savedTitle = this.get('Title');
				this.set('editTitle', true);
			}
		},
		startEditImageCaption: function () {
			if (this.get('canEditBuild')) {
				this.imageCaption = this.get('selectedImageCaption');
				this.set('editCaption', true);
			}
		},
		startEditDescription: function () {
			if (this.get('canEditBuild')) {
				this.savedDescription = this.get('Description');
				this.set('editDescription', true);
			}
		},
		discardDescription: function () {
			this.set('editDescription', false);
			this.set('Title', this.savedDescription);
		},
		saveImageCaption: function () {
			this.set('editCaption', false);
			var c = this.get('imageCaption');
			var si = this.get('selectedImage');
			var images = this.get('Images');
			console.log(c);
			console.log(si);
			console.log(images);
			for (var i = 0; i < images.length; i++) {
				if (images[i].Id === si) {
					Ember.set(images[i], 'Title', c);
					break;
				}
			}
			this.send('saveBuild');
		},
		changeCelsius: function () {
			this.set('tempModified', true);
			this.set('IsCelsius', !this.get("IsCelsius"));
		},
		saveTemperature: function () {
			this.set('tempModified', false);
			this.send('saveBuild');
		},
		saveDescription: function () {
			this.set('editDescription', false);
			this.send('saveBuild');
		},
		discardImageCaption: function () {
			this.set('editCaption', false);
			this.set('imageCaption', '');
		},
		discardTitle: function () {
			this.set('editTitle', false);
			this.set('Title', this.savedTitle);
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
				
				var build = this.get('model');
				var guid = Math.random().toString(36);

				var supportedFileTypes = {
					'image/png': true,
					'image/jpeg': true,
					'image/gif': true,
				};

				if (!supportedFileTypes[file.type]) {
					this.growl.error('Filetype not supported: ' + file.type);
					return;
				}

				this.get('progressBars').pushObject({ guid: guid, name: file.name, progress: 0 });


				build.addImageToBuild(file, guid, progressFunc).then((success) => {
					//console.log("Removing upload " + guid);
					this.get('progressBars').setObjects(this.progressBars.filter(function (x) { return x.guid !== guid; }));
					this.growl.success('Uploaded file');
				},(failure) => {
						console.log('upload fail');
						console.log(failure);
						this.growl.error('Error uploading file: ' + failure);
						this.get('progressBars').setObjects(this.progressBars.filter(function (x) { return x.guid !== guid; }));
					});

			}
		}

	}

});

