﻿/// <reference path="app.ts" />


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
			App.Data.removeBuildFromCache(this.id);
			
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
				var b = App.Data.builds[bid];
				if (!b) {
					console.log('Add Build To List, could not find build ' + bid);
					App.Track.track("RemoveBuildFromListError", {Build: bid});

				} else {
					this.builds.pushObject(b);
				}
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
				this.builds.removeObjects(this.builds.filter((b) => { return b.id === bid; }));
				return success;
		});
	},




	deleteList: function () {
		var data = JSON.stringify(this);
		console.log('delete list');
		console.log(this);
		App.Track.track("DeleteList", { List: this.id });
		App.Data.removeListFromCache(this.id);
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

	// a build list--- a list of builds (for a particular user)
	userbuilds: any;

	constructor() {
		this.builds = {};
		this.userlists = {};
		this.userbuilds = {};
		this.buildlists = {};
	}


	removeBuildFromCache(bid) {
		if (App.Data.builds[bid]) {
			delete App.Data.builds[bid];
		}

		for (var key in this.userbuilds) {
			var list = this.userbuilds[key];
			list.removeObjects(list.filter((item) => {
				return item.id === bid;
			}));
		}
	}

	removeListFromCache(lid) {
		// remove this from list cache
		if (App.Data.buildlists[lid]) {
			delete App.Data.buildlists[lid];
		}

		// remove this from user lists
		for (var name in App.Data.userlists) {
			var ul = App.Data.userlists[name];
			ul.get('lists').removeObjects(ul.get('lists').filter((li) => {
				return li.id === lid;
			}));
		}

	}


	getBuild(bid) {
		console.log('Get Build: ' + bid);
		var b = this.builds[bid];
		if (!b) {
			return Ember.$.getJSON('/api/build/' + bid).then((res) => {
				b = App.BuildObject.create(res);
				console.log(b);
				this.builds[bid] = b;
				var ubl = this.userbuilds[b.get('creator')];
				if (ubl) {
					if (!ubl.findBy('id', b.id)) {
						console.log('Adding new build ' + b.id + ' to user build list ' + b.get('creator'));
						ubl.pushObject(b);
					}
				}
				return b;
			});
		} else {
			console.log('..from cache');
			return promiseFor(b);
		}
	}

	// given a list of build ids, parameter: l, fetch the ones that aren't in the cache
	// then set the l.builds property to a ember array of actual build objects, using the cache,
	// so that build objects are reused and are updated together
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
		console.log('Get User List: ' + userKey);
		var l = this.userlists[userKey];
		if (!l) {
			return Ember.$.ajax({
				type: 'GET',
				url: '/api/UserLists/' + userKey,
				dataType: 'json',
			}).then((res) => {
					var newList = Ember.A();
					res.forEach((elem) => {
						if (!App.Data.buildlists[elem.id]) {
							App.Data.buildlists[elem.id] = App.BuildListObject.create(elem);
						}
						this.fillListBuilds(App.Data.buildlists[elem.id]);
						newList.pushObject(App.Data.buildlists[elem.id]);
					});
					l = App.UserListsObject.create({ lists: newList });
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
		console.log('Get List: ' + lid);
		var l = this.buildlists[lid];
		if (!l) {
			return Ember.$.getJSON('/api/list/' + lid).then((res) => {
				var l = App.BuildListObject.create(res);
				this.fillListBuilds(l);
				this.buildlists[lid] = l;

				var ubl = this.userlists[l.get('creator')];
				if (ubl) {
					if (!ubl.get('lists').findBy('id', l.id)) {
						console.log('Adding new list ' + l.id + ' to user build list ' + l.get('creator'));
						ubl.get('lists').pushObject(l);
					}
				}

				return l;
			});
		} else {
			console.log('cached list');
			console.log(l);
			return promiseFor(l);
		}
		return
	}







	getUserBuilds(bid) {
		console.log('Get User Builds: ' + bid);
		if (this.userbuilds[bid]) {
			console.log('cached list');
			return promiseFor(this.userbuilds[bid]);
		} else {
			return Ember.$.getJSON('/api/UserBuilds/' + bid).then((builds) => {
				var arr = Ember.A();
				builds.forEach((b) => {
					if (!this.builds[b.id]) {
						var bo = App.BuildObject.create(b);
						this.builds[b.id] = bo;
					}

					arr.pushObject(this.builds[b.id]);
				});
				this.userbuilds[bid] = arr;

				console.log(arr);
				return arr;
			});
		}
	}



}


App.Data = new MyAppData();

