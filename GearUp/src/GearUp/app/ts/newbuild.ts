/// <reference path="app.ts" />

var testbuild = new Gear.Build();
testbuild.parts = [new Gear.Part()];


App.NewbuildRoute = Ember.Route.extend({
	model: function () {
		console.log(testbuild);
		return testbuild;
	}
});

App.NewbuildController = Ember.ObjectController.extend({
});


