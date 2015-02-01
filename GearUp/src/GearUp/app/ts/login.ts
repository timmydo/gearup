/// <reference path="app.ts" />


App.LoginRoute = Ember.Route.extend({
	setupController: function (controller) {
		// `controller` is the instance of ApplicationController
		controller.set('title', "Hello world!");
	}
});

App.LoginController = Ember.Controller.extend({
});
