

/// <reference path="app.ts" />

App.SearchRoute = Ember.Route.extend({
	model: function (params) {
		return App.Data.searchIndex(params.q).then((res) => {
			var ids = [];
			res.value.forEach((elem) => {
				ids.push(elem.id);
			});
			var obj = {"builds": ids, "query": params.q};
			var l = App.BuildListObject.create(obj);
			App.Data.fillListBuilds(l);
			console.log(l);
			return l;
		});
	},
	actions: {
		
	}
});


App.SearchController = Ember.ObjectController.extend({

	searchQuery: ''
});

