/// <reference path="app.ts" />

App.AboutRoute = Ember.Route.extend({
	setupController: function (controller) {
		// `controller` is the instance of ApplicationController
		controller.set('title', "Hello world!");
	}
});

App.AboutController = Ember.Controller.extend({
});


