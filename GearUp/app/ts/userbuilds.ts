/// <reference path="app.ts" />

App.UserbuildsRoute = Ember.Route.extend({
	model: function (params) {
		var bid = params.bid;
		if (!bid) {
			bid = window['UserIdentityKey'] || '';
		}
		return App.Data.getUserBuilds(bid);
	}
});

App.UserbuildsController = Ember.ObjectController.extend({

	userLoginKey: function () {
		return window['UserIdentityKey'] || '';
	}.property('window.UserIdentityKey'),


	//fixme async property
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
					this.growl.error('Error getting user build list: ' + xhr.responseJSON);
				});
		} else {
			console.log('Not loading userbuild list');
			console.log(userKey);
			console.log(value);
		}

		return value || [];
	}.property('userLoginKey'),




	actions: {
		createBuild: function () {
			App.Data.createBuild().then((bid) => {
				this.transitionToRoute('build', bid.Id);
			});
		},

		addBuildToList: function (thelist, thebuild) {
			thelist.addBuildToList(thebuild).then(() => {
				this.growl.success('Build added to list');
			}, (xhr) => {
				this.growl.error('Error adding build to list: ' + xhr.responseText);
			});
		},
	}

});



