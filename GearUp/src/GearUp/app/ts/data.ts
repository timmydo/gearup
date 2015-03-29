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
	},

	deleteBuild: function () {
		var data = JSON.stringify(this);
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/DeleteBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((res) => {
			//fixme todo does deleting a build remove it from lists???
			var idx = App.Data.builds.indexOf(this);
			if (idx >= 0) {
				App.Data.builds.splice(idx, 1);
			}
			return res;
		});
	},

	deleteImageFromBuild: function (guid) {
		var data = JSON.stringify({ Build: this.id, Image: guid });
		var thisbuild = this;
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/DeleteImageFromBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((res) => {
			var obj = null;
			for (var i = thisbuild.images.length - 1; i >= 0; i--) {
				if (thisbuild.images[i].guid === guid) {
					obj = thisbuild.images[i];
					break;
				}
			}
			if (obj) {
				thisbuild.images.removeObject(obj);
			}

			return res;
		});
	},

	addImageToBuild: function (file, guid, progressFunc) {
		var thisbuild = this;
		return new Ember.RSVP.Promise(function (resolve, reject) {
			var xhr = new XMLHttpRequest();
			xhr.open('POST', '/api/UploadImage?buildid=' + thisbuild.id, true);
			xhr.onload = function (e) {
				if (this.status == 200) {
					var parsed = JSON.parse(this.response);
					thisbuild.images.pushObject(parsed);
					resolve(this.response);
				} else {
					reject(this.response);
				}
			};
			xhr.onerror = function (e) {
				reject(e.error);
			};
			xhr.upload.onprogress = function (e) {
				if (e.lengthComputable) {
					var value = (e.loaded / e.total) * 100;

					if (progressFunc) {
						progressFunc(guid, value);
					}
				}
			};

			xhr.setRequestHeader('Content-Type', file.type);
			xhr.send(file);
		});

	}


});

App.UserListsObject = Ember.Object.extend({});
App.BuildListObject = Ember.Object.extend({
	save: function () {
		var data = JSON.stringify(this);

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/SaveList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	},

	addBuildToList: function (bid) {
		var data = JSON.stringify({ build: bid, list: this.id });
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/AddBuildToList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((success) => {
			this.builds.pushObject(bid);
		});
	},

	removeBuildFromList: function (bid) {
		var data = JSON.stringify({ 'build': bid, 'list': this.id });

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/RemoveBuildFromList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((success) => {
			this.builds.removeObject(bid);
			return success;
		});
	},



	getBuilds: function () {
		var data = JSON.stringify(this);
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/Build',
			contentType: 'application/json',
			data: data,
			dataType: 'json'
		});
	},

	deleteList: function () {
		var data = JSON.stringify(this);

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/DeleteList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}




});

function promiseFor(val) {
	return new Ember.RSVP.Promise(function (res) {
		return res(val);
	});
}

class MyAppData {

	builds: any;
	userlists: any;
	buildlists: any;

	constructor() {
		this.builds = {};
		this.userlists = {};
		this.buildlists = {};
	}

	getBuild(bid) {
		var b = this.builds[bid];
		if (!b) {
			return Ember.$.getJSON('/api/build/' + bid).then((res) => {
				b = App.BuildObject.create(res);
				console.log('get build ' + bid);
				console.log(b);
				this.builds[bid] = b;
				return b;
			});
		} else {
			return promiseFor(b);
		}
	}


	getUserList(userKey) {
		var l = this.userlists[userKey];
		if (!l) {
			return Ember.$.ajax({
				type: 'GET',
				url: '/api/UserLists/' + userKey,
				dataType: 'json',
			}).then((res) => {
				l = App.UserListsObject.create({ lists: res });
				console.log('get list ' + userKey);
				console.log(l);
				this.userlists[userKey] = l;
				return l;
			});
		} else {
			console.log('cached list');
			console.log(l);
			return promiseFor(l);
		}
	}

	getList(lid) {
		var l = this.buildlists[lid];
		if (!l) {
			return Ember.$.getJSON('/api/list/' + lid).then((res) => {
				l = App.BuildListObject.create(res);
				console.log('get list ' + lid);
				console.log(l);
				this.buildlists[lid] = l;
				return l;
			});
		} else {
			return promiseFor(l);
		}
		return
	}







	getUserBuilds(bid) {
		return Ember.$.getJSON('/api/UserBuilds/' + bid);
	}



}


App.Data = new MyAppData();

