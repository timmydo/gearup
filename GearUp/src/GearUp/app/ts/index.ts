/// <reference path="app.ts" />


App.IndexRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.getRecentBuilds();
	}
});

App.IndexController = Ember.Controller.extend({

	groupedBuilds: function () {
		var a = Ember.A();
		var row = Ember.A();
		var count = 0;
		var builds = this.get('model.builds');
		if (builds) {
			builds.forEach((b) => {
				if (count > 4) {
					count = 0;
					a.pushObject(row);
					row = Ember.A();
				}

				// only display builds that have an image
				if (b.images.length > 0) {
					count++;
					row.pushObject(b);
				}
			});
		}

		if (row.length > 0) {
			a.pushObject(row);
		}

		return a;

	}.property('model.builds')

});
