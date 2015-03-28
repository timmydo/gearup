/// <reference path="app.ts" />


App.BuildObject = Ember.Object.extend({

	save: function () {
		var data = JSON.stringify(this);
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/SaveBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}
});

App.ListObject = Ember.Object.extend({});


class MyAppData {

	builds : any;
	lists: any;

	constructor() {
		this.builds = {};
		this.lists = {};
	}

	getBuild(bid) {
		var b = this.builds[bid];
		if (!b) {
			console.log('get build ' + bid);
			return Ember.$.getJSON('/api/build/' + bid).then((res) => {
				console.log('get build ' + bid);
				console.log(res);
				b = App.BuildObject.create(res);
				this.builds[bid] = b;
				return b;
			});
		} else {
			return b;
		}
	}

	saveBuild(data) {
		
	}

	getUserList(userKey) {
		return Ember.$.ajax({
			type: 'GET',
			url: '/api/UserLists/' + userKey,
			dataType: 'json',
		});
	}

	addBuildToList(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/AddBuildToList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}

	deleteBuild(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/DeleteBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}

	getList(lid) {
		return Ember.$.getJSON('/api/list/' + lid);
	}

	saveList(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/SaveList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}

	getBuildsFromList(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/Build',
			contentType: 'application/json',
			data: data,
			dataType: 'json'
		});
	}

	removeBuildFromList(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/RemoveBuildFromList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}

	deleteList(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/DeleteList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}

	getUserBuilds(bid) {
		return Ember.$.getJSON('/api/UserBuilds/' + bid);
	}

	getUserLists(bid) {
		return Ember.$.getJSON('/api/UserLists/' + bid);
	}

}


App.Data = new MyAppData();

