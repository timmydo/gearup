/// <reference path="app.ts" />


var testbuild = new Gear.Build();
testbuild.parts = [new Gear.Build()];


App.BuildRoute = Ember.Route.extend({
	model: function () {
		console.log(testbuild);
		return testbuild;
	}
});

App.BuildController = Ember.ObjectController.extend({
	createdTime: function () {
		return moment(this.get('model.created')).format('ll');
	}.property('model.created')


});


App.BuildView = Ember.View.extend({
    didInsertElement: function () {
		console.log('run holder');
        Ember.run.next(null, function () {
            Holder.run();
        })
	}
});


