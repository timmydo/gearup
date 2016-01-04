

/// <reference path="app.ts" />

App.SearchRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.searchIndex(params.q).then((res) => {
			var barray = Ember.A();
			res.forEach((elem) => {
				if (!App.Data.builds[elem.Id]) {
					var item = App.BuildObject.create(elem);
					App.Data.builds[elem.Id] = item;
				}
				
				barray.pushObject(App.Data.builds[elem.Id]);
			});

			return { "Builds": barray, "Query": params.q };
		});
	},
	actions: {
		
	}
});


App.SearchController = Ember.ObjectController.extend({

	searchQuery: ''
});

