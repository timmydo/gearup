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
			App.Data.createList().then((lid) => {
				this.transitionToRoute('list', lid.Id);
			});
		}

	}

});



