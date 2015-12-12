/// <reference path="../../typings/tsd.d.ts" />

interface Window { App: any }

declare var App: any;
declare var Holder: any;
declare var appInsights: any;

window.App = Ember.Application.create({
	LOG_TRANSITIONS: true,
    //authToken: localStorage['authToken'],
	User: '',
    error: null
});

App.Router.map(function () {
	this.route('index', { path: '/' });
	this.route('login', { path: '/login' });
	this.route('register', { path: '/register' });
	this.route('build', { path: '/builds/:bid' });
	this.route('userbuilds', { path: '/userbuilds/:bid' });
	this.route('list', { path: '/lists/:bid' });
	this.route('userlists', { path: '/userlists/:bid' });
	this.route('search', { path: '/search/:q' });
});

App.ApplicationRoute = Ember.Route.extend({
	actions: {
		setError: function (e) {
            this.get('controller').set('errorMessage', e);
        },
		setInfo: function (e) {
            this.get('controller').set('infoMessage', e);
        }
    }
});

App.ApplicationController = Ember.Controller.extend({
	appName: 'Gear up',

	errorMessage: '',
	infoMessage: '',

	userLoggedIn: function () {
		return window['UserIdentityName'] !== '';
	}.property('window.UserIdentityName'),

	userLoginName: function () {
		return window['UserIdentityName'] || 'Unknown User';
	}.property('window.UserIdentityName'),

	userLoginKey: function () {
		return window['UserIdentityKey'] || '';
	}.property('window.UserIdentityKey'),

	latestSuggestionTag: '',

	suggest: function() {
		var q = this.get('searchQuery');
		if (q && q.length >= 3 && q.length < 25) {
			var tag = Gear.UUID.v4();
			this.set('latestSuggestionTag', tag);
			App.Data.searchSuggest(q).then((result) => {
				// ensure that if a later request happened after this,
				// we wait for it instead of updating now
				if (this.get('latestSuggestionTag') === tag) {
					console.log(result);
				}
			})
		}
	}.observes('searchQuery'),


	actions: {
		suggest: function(q, syncRes, asyncRes) {
			App.Data.searchSuggest(q).then((x) => {
				var buildlist = (x.value||[]);
				if (asyncRes) {
					asyncRes(buildlist);
				}
			});
		},
		setError: function (e) {
            this.set('errorMessage', e);
        },
        dismissError: function () {
            this.set('errorMessage', null);
        },
		setInfo: function (e) {
            this.set('infoMessage', e);
        },
        dismissInfo: function () {
            this.set('infoMessage', null);
        },
		search: function() {
			console.log(this.get('searchQuery'));
			this.transitionToRoute('search', this.get('searchQuery'));
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


