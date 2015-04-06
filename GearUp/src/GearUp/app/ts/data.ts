/// <reference path="app.ts" />


App.BuildObject = Ember.Object.extend({

	save: function () {
		App.Track.track("SaveBuild", { Build: this.id });

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
		App.Track.track("DeleteBuild", { Build: this.id });

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/DeleteBuild',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((res) => {
			//fixme todo does deleting a build remove it from lists???
			if (App.Data.builds[this.id]) {
				delete App.Data.builds[this.id];
			}
			return res;
		});
	},

	deleteImageFromBuild: function (guid) {
		var opts = { Build: this.id, Image: guid };
		var data = JSON.stringify(opts);
		App.Track.track("DeleteImage", opts);

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
		App.Track.track("AddImageToBuild", { Build: thisbuild.id });

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
		App.Track.track("SaveList", { List: this.id });
		App.Data.updateCacheList(this);
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/SaveList',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	},

	addBuildToList: function (bid) {
		var opts = { build: bid, list: this.id };
		var data = JSON.stringify(opts);
		App.Track.track("AddBuildToList", opts);

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
		var opts = { 'build': bid, 'list': this.id };
		var data = JSON.stringify(opts);
		App.Track.track("RemoveBuildFromList", opts);

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




	deleteList: function () {
		var data = JSON.stringify(this);
		console.log('delete list');
		console.log(this);
		App.Track.track("DeleteList", { List: this.id });
		App.Data.removeFromListCache(this);
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

	// builds we have loaded
	builds: any;

	// users' lists of builds (list of build lists)
	userlists: any;

	// a build list--- a list of builds
	buildlists: any;

	constructor() {
		this.builds = {};
		this.userlists = {};
		this.buildlists = {};
	}


	removeFromListCache(list) {
		// remove this from list cache
		if (App.Data.buildlists[list.id]) {
			delete App.Data.buildlists[list.id];
		}

		// remove this from user lists
		for (var name in App.Data.userlists) {
			var ul = App.Data.userlists[name];
			var newList = ul.get('lists').reject((li) => {
				return li.id === list.id;
			});
			console.log(ul);
			ul.set('lists', newList);
			console.log(ul);
		}

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


	fillListBuilds(l) {
		//console.log('getList');
		var origbuilds = l.get('builds').copy();
				
		// set builds to the ones we haven't already cached--so we can load them
		var notCachedBuilds = l.get('builds').filter((el) => { return !App.Data.builds[el.id]; })
		//console.log('filter');
		//console.log(notCachedBuilds);
		//console.log(App.Data.builds);
		//console.log(filtered);
		//l.set('builds', filtered);



		// if we need to load some builds in this list, ask for them in bulk
		if (notCachedBuilds.length > 0) {

			l.set('builds', notCachedBuilds);
			var postdata = JSON.stringify(l);
			Ember.$.ajax({
				type: 'POST',
				url: '/api/Build',
				contentType: 'application/json',
				data: postdata,
				dataType: 'json'
			}).then((data) => {
				var barray = Ember.A();
				//console.log('postbuild');
				//console.log(data);
				data.forEach((elem) => {
					if (!App.Data.builds[elem.id]) {
						var item = App.BuildObject.create(elem);
						App.Data.builds[elem.id] = item;
					}

				});

				//console.log('cache');
				//console.log(App.Data.builds);
				//console.log(origbuilds);

				origbuilds.forEach((guid) => {
					barray.pushObject(App.Data.builds[guid]);
				});

				//console.log('set builds');
				//console.log(barray);

				l.set('builds', barray);
			},(xhr) => {
					console.log(xhr);
					App.Track.track("GetListError", { Message: 'Error getting user build list: ' + xhr.responseJSON });
				});

			// set the list to empty until we load everything
			l.set('builds', Ember.A());



		} else {
			l.set('builds', Ember.A());
			origbuilds.forEach((b) => {
				l.get('builds').pushObject(App.Data.builds[b]);
			});
			//console.log('builds');
			//console.log(l.get('builds'));
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
					var newList = Ember.A();
					console.log('getUserList');
					console.log(res);
					res.forEach((elem) => {
						if (!App.Data.buildlists[elem.id]) {
							App.Data.buildlists[elem.id] = App.BuildListObject.create(elem);
						}
						this.fillListBuilds(App.Data.buildlists[elem.id]);
						newList.pushObject(App.Data.buildlists[elem.id]);
					});
					l = App.UserListsObject.create({ lists: newList });
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
				var l = App.BuildListObject.create(res);
				this.fillListBuilds(l);
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

