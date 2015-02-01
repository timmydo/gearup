/// <reference path="../../typings/tsd.d.ts" />

interface Window { App: any }
window.App = Ember.Application.create({
	LOG_TRANSITIONS: true
});

var startup = function (App) {

	App.ApplicationRoute = Ember.Route.extend({
		setupController: function (controller) {
			// `controller` is the instance of ApplicationController
			controller.set('title', "Hello world!");
		}
	});

	App.ApplicationController = Ember.Controller.extend({
		appName: 'My First Example'
	});
};

startup(window.App);



