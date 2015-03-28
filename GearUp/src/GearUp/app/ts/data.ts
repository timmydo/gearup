/// <reference path="app.ts" />


class MyAppData {

	getBuild(bid) {
		return Ember.$.getJSON('/api/build/' + bid);
	}

	saveBuild(data) {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/SaveBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'text',
			success: (data, status) => {
				console.log(status);
				console.log(data);
			}
		});
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

