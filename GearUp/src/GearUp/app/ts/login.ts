/// <reference path="app.ts" />

App.LoginCreds = Ember.Object.extend({
    username: null,
    password: null,
    remember: false,

    json: function () {
        return {
            username: this.get('username'),
            password: this.get('password'),
            remember: this.get('remember')
        }
    }
});

App.LoginRoute = (<any>Ember.Route).extend(App.Ajax, {
	model: function () {
        // let our login template fill in the properties of a creds object
        return App.LoginCreds.create({});
    },

    events: {
        login: function () {
            var model = this.modelFor('login'); // <App.LoginCreds>
            var self = this;
            self.POST('/api/login', model.json()).then(
                function (json) {
                    if (json == null) return; // shouldn't happen, but should still NPE protect
                    if (json.error != null) {
                        // useful for any ajax call: set the global error alert with our error message
                        App.set('error', json.error.message);
                    } else {
                        // setting this value will reveal our logout button
                        App.set('authToken', json.authToken);
                        if (model.get('remember')) {
                            localStorage['authToken'] = json.authToken;
                        } else {
                            // make sure a stale value isn't left behind
                            delete localStorage['authToken'];
                        }
                        // clear out any login error that was left over
                        App.set('error', null);
                        self.router.transitionTo('index');
                    }
                });
        }
    }
});

App.LoginController = Ember.ObjectController.extend({
});
