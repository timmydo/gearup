/// <reference path="app.ts" />

App.LoginCreds = Ember.Object.extend({
    username: null,
    password: null,
    remember: false,

    json: function () {
        return {
            username: this.get('username') || '',
            password: this.get('password') || '',
            remember: !!this.get('remember')
        }
    }
});

App.LoginRoute = Ember.Route.extend({
	model: function () {
        // let our login template fill in the properties of a creds object
        return App.LoginCreds.create({});
    },

    events: {
		google: function () {
			window.location.href = "/login?authtype=Google";
		},
    }
});

App.LoginController = Ember.ObjectController.extend({
});
