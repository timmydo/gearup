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

App.UserbuildsController = Ember.ArrayController.extend({

	actions: {
		createBuild: function () {
			this.transitionToRoute('build', Gear.UUID.v4());
		}
	}

});



