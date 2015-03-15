/// <reference path="app.ts" />

App.ListRoute = Ember.Route.extend({
	model: function (params) {
		return Ember.$.getJSON('/api/list/' + params.bid);
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
				Ember.$.ajax({
					type: 'POST',
					url: '/api/SaveList',
					contentType: 'application/json',
					data: data,
					dataType: 'text',
					success: (data, status) => {
						console.log(status);
						console.log(data);
					},
					error: (xhr, status, err) => {
						console.log(xhr);
						console.log(status);
						console.log(err);
						this.send('setError', 'Error saving build: ' + xhr.responseText);
					}
				});
			}
		}

	}
});

App.ListController = Ember.ObjectController.extend({
	canEditList: function () {
		return this.get('model.creator') === window['UserIdentityKey'];
	}.property('model.creator'),
	editTitle: false,
	savedTitle: '',

	actions: {
		tryDeleteList: function () {
			this.set('tryDelete', !this.get('tryDelete'));
		},
		deleteList: function () {
			this.set('tryDelete', false); // try preventing doubleclick
			var model = this.get('model');
			var data = JSON.stringify(model);
			console.log("Delete List " + data);
			if (data) {
				Ember.$.ajax({
					type: 'POST',
					url: '/api/DeleteList',
					contentType: 'application/json',
					data: data,
					dataType: 'text',
					success: (data, status) => {
						console.log(status);
						console.log(data);
						this.transitionToRoute('userlists', model.creator);
					},
					error: (xhr, status, err) => {
						console.log(xhr);
						console.log(status);
						console.log(err);
						this.send('setError', 'Error deleting list: ' + xhr.responseText);
					}
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

