/// <reference path="app.ts" />

App.ListRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.getList(params.bid);
	},
	resetController: function (controller, isExiting, transition) {
		if (isExiting) {
			controller.set('startLoadBuildList', false);
		}
	},	
	actions: {
		invalidateModel: function () {
			Ember.Logger.log('Route is now refreshing...');
			this.refresh();
		},
		saveList: function () {
			var model = this.modelFor(this.routeName);
			var data = JSON.stringify(model);
			console.log("Saving List " + data);
			if (data) {
				model.save().then((data) => {
					console.log(data);
					this.send('setInfo', 'Saved list');
					}, (xhr) => {
						console.log(xhr);
						this.send('setError', 'Error saving build: ' + xhr.responseText);
					});
			}
		}

	}
});

App.ListController = Ember.ObjectController.extend({
	canEditList: function () {
		return this.get('model.creator') === window['UserIdentityKey'];
	}.property('model'),
	editTitle: false,
	savedTitle: '',
	savedDescription: '',
	startLoadBuildList: false,
	buildList: function (key, value, previousValue) {
		var list = [];
		var firstCall = true;
		var model = this.get('model');

		//setter
		if (arguments.length > 1) {
			list = value;
		} else {
			if (!this.get('startLoadBuildList')) {
				this.set('startLoadBuildList', true);
				model.getBuilds().then((data, status) => {
					console.log(data);
					this.set('buildList', data);
				},(xhr, status, err) => {
					console.log(xhr);
					this.send('setError', 'Error getting build list: ' + xhr.responseJSON);
				});
			}
		}

		//getter
		return list;

	}.property('model'),

	actions: {

		removeFromList: function (bid) {
			var model = this.get('model');
			model.removeBuildFromList(bid).then((data) => {
				console.log(status);
				console.log(data);
				//this.set('startLoadBuildList', false);
				//this.send('invalidateModel');
			},(xhr) => {
				console.log(xhr);
				console.log(status);
				this.send('setError', 'Error removing item: ' + xhr.responseText);
			});
		},
		tryDeleteList: function () {
			this.set('tryDelete', !this.get('tryDelete'));
		},
		deleteList: function () {
			this.set('tryDelete', false); // try preventing doubleclick
			var model = this.get('model');

			model.deleteList().then((data) => {
				console.log(status);
				this.send('setInfo', 'List deleted');
				this.transitionToRoute('userlists', model.creator);
			},(xhr) => {
				console.log(xhr);
				this.send('setError', 'Error deleting list: ' + xhr.responseText);
			});
			
		},
		startEditTitle: function () {
			if (this.get('canEditList')) {
				this.savedTitle = this.get('title');
				this.savedDescription = this.get('description');
				this.set('editTitle', true);
			}
		},
		discardTitle: function () {
			this.set('editTitle', false);
			this.set('title', this.savedTitle);
		},
		saveTitle: function () {
			this.set('editTitle', false);
			this.send('saveList');
		},

	}

});

