/// <reference path="app.ts" />




App.Router.map(function () {
	this.route('index', { path: '/' });
	this.route('about', { path: '/about' });
	this.route('login', { path: '/login' });
	this.route('register', { path: '/register' });
	this.route('build', { path: '/builds/:bid' });
});

App.ApplicationRoute = (<any>Ember.Route).extend(App.Ajax, {
	setupController: function (controller) {
	},
	events: {
		//fixme
        logout: function () {
            this.GET('/auth/logout').then(function (json) {
                if (json != null && json.error != null) {
                    App.set('error', json.error.message);
                }
            });
            // even if we error out, we can still clear our own record
            App.set('authToken', null);
            delete localStorage['authToken'];
            this.transitionTo('login');
        },

        dismissError: function () {
            App.set('error', null);
        }
    }
});

App.ApplicationController = Ember.Controller.extend({
	appName: 'Gear up'
});





