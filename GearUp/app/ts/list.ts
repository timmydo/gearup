﻿/// <reference path="app.ts" />

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
					this.growl.success('Saved list');
					}, (xhr) => {
						console.log(xhr);
						this.growl.error('Error saving build: ' + xhr.responseText);
					});
			}
		}

	}
});

App.ListController = Ember.ObjectController.extend({
	canEditList: function () {
		return this.get('model.Creator') === window['UserIdentityKey'];
	}.property('model'),
	editTitle: false,
	savedTitle: '',
	savedDescription: '',
	startLoadBuildList: false,

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
				this.growl.error('Error removing item: ' + xhr.responseText);
			});
		},
		tryDeleteList: function () {
			this.set('tryDelete', !this.get('tryDelete'));
		},
		deleteList: function () {
			this.set('tryDelete', false); // try preventing doubleclick
			var model = this.get('model');

			model.deleteList().then((data) => {
				//console.log(status);
				this.growl.success('List deleted');
				this.transitionToRoute('userlists', model.Creator);
			},(xhr) => {
				console.log(xhr);
				this.growl.error('Error deleting list: ' + xhr.responseText);
			});
			
		},
		startEditTitle: function () {
			if (this.get('canEditList')) {
				this.savedTitle = this.get('Title');
				this.savedDescription = this.get('Description');
				this.set('editTitle', true);
			}
		},
		discardTitle: function () {
			this.set('editTitle', false);
			this.set('Title', this.savedTitle);
		},
		saveTitle: function () {
			this.set('editTitle', false);
			this.send('saveList');
		},

	}

});

