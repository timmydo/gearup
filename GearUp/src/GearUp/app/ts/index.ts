﻿/// <reference path="app.ts" />


App.IndexRoute = Ember.Route.extend({
	setupController: function (controller) {
		// `controller` is the instance of ApplicationController
		controller.set('title', "Hello world!");
	}
});

App.IndexController = Ember.Controller.extend({
});
