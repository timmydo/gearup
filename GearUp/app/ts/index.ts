/// <reference path="app.ts" />


App.IndexRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.getRecentBuilds();
	},
	actions: {
		error: function(reason) {
			console.log(reason);
		}
	}
});

App.IndexController = Ember.Controller.extend({

	groupedBuilds: function () {
		var a = Ember.A();
		var row = Ember.A();
		var count = 0;
		var builds = this.get('model.Builds');
		if (builds) {
			builds.forEach((b) => {
				if (count >= 3) {
					count = 0;
					a.pushObject(row);
					row = Ember.A();
				}

				// only display builds that have an image
				if (b.Images.length > 0) {
					count++;
					row.pushObject(b);
				}
			});
		}

		if (row.length > 0) {
			a.pushObject(row);
		}

		return a;

	}.property('model.Builds')

});
