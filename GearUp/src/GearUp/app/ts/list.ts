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
				App.Data.saveList(data).then((data, status) => {
						console.log(status);
						console.log(data);
					}, (xhr, status, err) => {
						console.log(xhr);
						console.log(status);
						console.log(err);
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
	startLoadBuildList: false,
	buildList: function (key, value, previousValue) {
		var list = [];
		var firstCall = true;
		var model = this.get('model');
		var data = JSON.stringify(model);

		//setter
		if (arguments.length > 1) {
			list = value;
		} else {
			if (!this.get('startLoadBuildList')) {
				this.set('startLoadBuildList', true);
				App.Data.getBuildsFromList(data).then((data, status) => {
					console.log(status);
					console.log(data);
					this.set('buildList', data);
				},(xhr, status, err) => {
					console.log(xhr);
					console.log(status);
					console.log(err);
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
			var data = JSON.stringify({'build': bid, 'list': model.id});
			console.log("Remove build from list" + data);
			if (data) {
				App.Data.removeBuildFromList(data).then((data, status) => {
					console.log(status);
					console.log(data);
					this.set('startLoadBuildList', false);
					this.send('invalidateModel');
				},(xhr, status, err) => {
					console.log(xhr);
					console.log(status);
					console.log(err);
					this.send('setError', 'Error removing item: ' + xhr.responseText);
				});
			}
		},
		tryDeleteList: function () {
			this.set('tryDelete', !this.get('tryDelete'));
		},
		deleteList: function () {
			this.set('tryDelete', false); // try preventing doubleclick
			var model = this.get('model');
			var data = JSON.stringify(model);
			console.log("Delete List " + data);
			if (data) {
				App.Data.deleteList(data).then((data, status) => {
					console.log(status);
					console.log(data);
					this.transitionToRoute('userlists', model.creator);
				},(xhr, status, err) => {
					console.log(xhr);
					console.log(status);
					console.log(err);
					this.send('setError', 'Error deleting list: ' + xhr.responseText);
				});
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

	}

});

