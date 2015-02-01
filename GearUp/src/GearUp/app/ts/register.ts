/// <reference path="app.ts" />


App.RegisterRoute = Ember.Route.extend({
	setupController: function (controller) {
		// `controller` is the instance of ApplicationController
		controller.set('title', "Hello world!");
	}
});

App.RegisterController = Ember.Controller.extend({
});
