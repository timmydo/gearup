/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="fileupload.ts" />

interface Window { App: any }

declare var App: any;
declare var Holder: any;

window.App = Ember.Application.create({
	LOG_TRANSITIONS: true,
    //authToken: localStorage['authToken'],
	User: '',
    error: null
});

App.Router.map(function () {
	this.route('index', { path: '/' });
	this.route('about', { path: '/about' });
	this.route('login', { path: '/login' });
	this.route('register', { path: '/register' });
	this.route('build', { path: '/builds/:bid' });
});

App.ApplicationRoute = Ember.Route.extend({
	actions: {
		setError: function (e) {
            this.get('controller').set('errorMessage', e);
        }
    }
});

App.ApplicationController = Ember.Controller.extend({
	appName: 'Gear up',

	errorMessage: '',

	userLoggedIn: function () {
		return window['UserIdentityName'] !== '';
	}.property('window.UserIdentityName'),

	userLoginName: function () {
		return 'Unknown User';
	}.property('window.UserIdentityName'),

	actions: {
		setError: function (e) {
            this.set('errorMessage', e);
        },
        dismissError: function () {
            this.set('errorMessage', null);
        }
    }
});



module Gear {

	export class UUID {
		static v4() {
			var d = Date.now();
			var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				var r = (d + Math.random() * 16) % 16 | 0;
				d = Math.floor(d / 16);
				return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
			});
			return uuid;
		}
	}


}


