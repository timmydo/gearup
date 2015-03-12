/// <reference path="app.ts" />

App.UserbuildsRoute = Ember.Route.extend({
	model: function (params) {
		var bid = params.bid;
		if (!bid) {
			bid = window['UserIdentityKey'] || '';
		}
		return Ember.$.getJSON('/api/UserBuilds/' + bid);
	}
});

App.UserbuildsController = Ember.ArrayController.extend({



});




