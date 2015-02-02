/// <reference path="app.ts" />

App.BuildRoute = Ember.Route.extend({
	setupController: function (controller) {
		// `controller` is the instance of ApplicationController
		controller.set('title', "Hello world!");
	}
});

App.BuildController = Ember.Controller.extend({
});


