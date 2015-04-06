/// <reference path="app.ts" />

App.UserlistsRoute = Ember.Route.extend({
	model: function (params) {
		var bid = params.bid;
		if (!bid) {
			bid = window['UserIdentityKey'] || '';
		}
		return App.Data.getUserList(bid);
	}
});

App.UserlistsController = Ember.ObjectController.extend({

	actions: {
		createList: function () {
			this.transitionToRoute('list', Gear.UUID.v4());
			// refresh the cache
			if (window['UserIdentityKey']) {
				App.Data.getUserList(window['UserIdentityKey']);
			}
		}
	}

});



