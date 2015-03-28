/// <reference path="app.ts" />

App.UserlistsRoute = Ember.Route.extend({
	model: function (params) {
		var bid = params.bid;
		if (!bid) {
			bid = window['UserIdentityKey'] || '';
		}
		return App.Data.getUserLists(bid);
	}
});

App.UserlistsController = Ember.ArrayController.extend({

	actions: {
		createList: function () {
			this.transitionToRoute('list', Gear.UUID.v4());
		}
	}

});



