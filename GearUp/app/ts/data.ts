/// <reference path="app.ts" />

App.getJSON = function(url) {
	var promise = new Ember.RSVP.Promise((resolve, reject) => {
		var client = new XMLHttpRequest();
		client.open("GET", url);
		client.onreadystatechange = handler;
		var responseTypeAware = true;
		try {
			client.responseType = "json";
		} catch (e) {
			responseTypeAware = false;
		}
		
		client.setRequestHeader("Accept", "application/json");
		client.send();

		function handler() {
			if (this.readyState === this.DONE) {
				if (this.status === 200) {
					try {
						
						if (responseTypeAware && client.responseType == 'json') {
							resolve(this.response);
						} else {
							var parsed = JSON.parse(this.response);
							resolve(parsed);
						}
					} catch (e) {
						reject('Error parsing:' + this.response + ' error: ' + e);
					}
				}
				else { reject(this); }
			}
		};
	});

	return promise;
};

App.BuildObject = Ember.Object.extend({

	save: function() {
		App.Track.track("SaveBuild", { Build: this.Id });

		var data = JSON.stringify(this);
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/build/save',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	},

	deleteBuild: function() {
		var data = JSON.stringify(this);
		App.Track.track("DeleteBuild", { Build: this.Id });

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/build/delete',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((res) => {
			//fixme todo does deleting a build remove it from lists???
			App.Data.removeBuildFromCache(this.Id);

			return res;
		});
	},

	deleteImageFromBuild: function(guid) {
		var opts = { Build: this.id, Image: guid };
		var data = JSON.stringify(opts);
		App.Track.track("DeleteImage", opts);

		var thisbuild = this;
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/build/delete-image',
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

	addImageToBuild: function(file, guid, progressFunc) {
		var thisbuild = this;
		App.Track.track("AddImageToBuild", { Build: thisbuild.Id });

		return new Ember.RSVP.Promise((resolve, reject) => {
			var xhr = new XMLHttpRequest();
			xhr.open('POST', '/api/UploadImage?buildid=' + thisbuild.Id, true);
			xhr.onload = function(e) {
				if (this.status == 200) {
					var parsed = JSON.parse(this.response);
					thisbuild.images.pushObject(parsed);
					resolve(this.response);
				} else {
					reject(this.response);
				}
			};
			xhr.onerror = function(e) {
				reject('error ' + xhr.status + ' - ' + xhr.statusText);
			};
			xhr.upload.onprogress = function(e) {
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
	save: function() {
		var data = JSON.stringify(this);
		App.Track.track("SaveList", { List: this.Id });
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/list/save',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	},

	addBuildToList: function(bid) {
		var opts = { build: bid, list: this.Id };
		var data = JSON.stringify(opts);
		App.Track.track("AddBuildToList", opts);

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/list/add',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((success) => {
			var b = App.Data.builds[bid];
			if (!b) {
				console.log('Add Build To List, could not find build ' + bid);
				App.Track.track("RemoveBuildFromListError", { Build: bid });

			} else {
				this.builds.pushObject(b);
			}
		});
	},

	removeBuildFromList: function(bid) {
		var opts = { 'build': bid, 'list': this.Id };
		var data = JSON.stringify(opts);
		App.Track.track("RemoveBuildFromList", opts);

		return Ember.$.ajax({
			type: 'POST',
			url: '/api/list/remove',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		}).then((success) => {
			this.builds.removeObjects(this.builds.filter((b) => { return b.Id === bid; }));
			return success;
		});
	},




	deleteList: function() {
		var data = JSON.stringify(this);
		console.log('delete list');
		console.log(this);
		App.Track.track("DeleteList", { List: this.Id });
		App.Data.removeListFromCache(this.Id);
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/list/delete',
			contentType: 'application/json',
			data: data,
			dataType: 'text'
		});
	}




});

function promiseFor(val) {
	return new Ember.RSVP.Promise((res) => {
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

	recentbuilds: any;

	constructor() {
		this.builds = {};
		this.userlists = {};
		this.userbuilds = {};
		this.buildlists = {};
		this.recentbuilds = null;
	}


	removeBuildFromCache(bid) {
		if (App.Data.builds[bid]) {
			delete App.Data.builds[bid];
		}

		for (var key in this.userbuilds) {
			var list = this.userbuilds[key];
			list.removeObjects(list.filter((item) => {
				return item.Id === bid;
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
			ul.get('Lists').removeObjects(ul.get('Lists').filter((li) => {
				return li.Id === lid;
			}));
		}

	}

	createBuild() {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/build/create',
			contentType: 'application/json',
		});
	}

	createList() {
		return Ember.$.ajax({
			type: 'POST',
			url: '/api/list/create',
			contentType: 'application/json',
		});
	}

	getBuild(bid) {
		console.log('Get Build: ' + bid);
		var b = this.builds[bid];
		if (!b) {
			return App.getJSON('/api/build/get/' + bid).then((res) => {
				b = App.BuildObject.create(res);
				console.log(b);
				this.builds[bid] = b;
				var ubl = this.userbuilds[b.get('Creator')];
				if (ubl) {
					if (!ubl.get('Builds').findBy('Id', b.Id)) {
						console.log('Adding new build ' + b.Id + ' to user build list ' + b.get('Creator'));
						ubl.get('Builds').pushObject(b);
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
		var origbuilds = l.get('Builds').slice(0);
		
		// set builds to the ones we haven't already cached--so we can load them
		var notCachedBuilds = origbuilds.filter((el) => { return !App.Data.builds[el.Id]; })
		
		// if we need to load some builds in this list, ask for them in bulk
		if (notCachedBuilds.length > 0) {

			l.set('Builds', notCachedBuilds);
			var postdata = JSON.stringify(notCachedBuilds);
			Ember.$.ajax({
				type: 'POST',
				url: '/api/build/get',
				contentType: 'application/json',
				data: postdata,
				dataType: 'json'
			}).then((data) => {
				var barray = Ember.A();
				data.forEach((elem) => {
					if (!App.Data.builds[elem.Id]) {
						var item = App.BuildObject.create(elem);
						App.Data.builds[elem.Id] = item;
					}

				});

				origbuilds.forEach((guid) => {
					barray.pushObject(App.Data.builds[guid]);
				});

				l.set('Builds', barray);
			}, (xhr) => {
					console.log(xhr);
					App.Track.track("GetListError", { Message: 'Error getting user build list: ' + xhr.responseJSON });
				});

			// set the list to empty until we load everything
			l.set('Builds', Ember.A());
		} else {
			l.set('Builds', Ember.A());
			origbuilds.forEach((b) => {
				l.get('Builds').pushObject(App.Data.builds[b]);
			});
		}
	}

	// a list has a List<string> of BuildLists, after calling this, it will
	// be a List<BuildList>
	fillListLists(l) {
		console.log("Fill List Lists");
		console.log(l);
		var origList = l.get('Lists').slice(0);
		
		// set builds to the ones we haven't already cached--so we can load them
		var notCached = origList.filter((el) => { return !App.Data.buildlists[el.Id]; })
		
		// if we need to load some builds in this list, ask for them in bulk
		if (notCached.length > 0) {

			l.set('Lists', notCached);
			var postdata = JSON.stringify(notCached);
			Ember.$.ajax({
				type: 'POST',
				url: '/api/list/get',
				contentType: 'application/json',
				data: postdata,
				dataType: 'json'
			}).then((data) => {
				var arr = Ember.A();
				data.forEach((elem) => {
					if (!App.Data.buildlists[elem.Id]) {
						var item = App.BuildListObject.create(elem);
						this.fillListBuilds(item);
						App.Data.buildlists[elem.Id] = item;
					}
				});

				origList.forEach((guid) => {
					arr.pushObject(App.Data.buildlists[guid]);
				});

				l.set('Lists', arr);
			}, (xhr) => {
				console.log(xhr);
				App.Track.track("GetListError", { Message: 'Error getting user build list: ' + xhr.responseJSON });
			});

			// set the list to empty until we load everything
			l.set('Lists', Ember.A());
		} else {
			l.set('Lists', Ember.A());
			origList.forEach((b) => {
				l.get('Lists').pushObject(App.Data.buildlists[b]);
			});
		}
	}

	getUserList(userKey) {
		console.log('Get User List: ' + userKey);
		var l = this.userlists[userKey];
		if (!l) {
			return new Ember.RSVP.Promise((resolve, reject) => {
				Ember.$.ajax({
					type: 'GET',
					url: '/api/user/lists/' + userKey,
					dataType: 'json',
				}).then((res) => {
					l = App.UserListsObject.create({ Lists: res });
					this.fillListLists(l);

					this.userlists[userKey] = l;
					resolve(l);
				}).fail((err) => {
					reject(err);
				});
			})
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
			return App.getJSON('/api/list/get/' + lid).then((res) => {
				var l = App.BuildListObject.create(res);
				this.fillListBuilds(l);
				this.buildlists[lid] = l;

				var ubl = this.userlists[l.get('Creator')];
				if (ubl) {
					if (!ubl.get('Lists').findBy('Id', l.Id)) {
						console.log('Adding new list ' + l.Id + ' to user build list ' + l.get('Creator'));
						ubl.get('Lists').pushObject(l);
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




	getRecentBuilds() {
		console.log('Get Recent Builds:');
		if (this.recentbuilds) {
			console.log('cached recent builds');
			return promiseFor(this.recentbuilds);
		} else {
			return App.getJSON('/api/build/recent').then((builds) => {
				var bl = App.BuildListObject.create({ Builds: Ember.A(builds) });
				console.log(bl);
				this.fillListBuilds(bl);
				this.recentbuilds = bl;

				return bl;
			}, (fail) => {
				var bl = App.BuildListObject.create({ Builds: Ember.A([]) });
				console.log('Failure getting recent builds: ' + fail);
				this.fillListBuilds(bl);
				this.recentbuilds = bl;
				App.Track.track("GetRecentError", { Message: 'Error getting recent list: ' + fail });
			});
		}
	}


	getUserBuilds(bid) {
		console.log('Get User Builds: ' + bid);
		if (this.userbuilds[bid]) {
			console.log('cached list');
			return promiseFor(this.userbuilds[bid]);
		} else {
			return App.getJSON('/api/user/builds/' + bid).then((builds) => {
				var bl = App.BuildListObject.create({ Builds: Ember.A(builds) });
				console.log(bl);
				this.fillListBuilds(bl);
				this.userbuilds[bid] = bl;

				return bl;
			});
		}
	}

	searchIndex(text) {
		var prom = new Ember.RSVP.Promise((resolve, reject) => {
			// fixme rename mainindexname 
			Ember.$.ajax({
				url: App.SearchEndpoint+'/indexes/' + App.SearchIndexName + '/docs?api-version=2015-02-28&search='+text,
          		type: 'GET',
          		dataType: 'json',
          		success: function(x) { resolve(x); },
          		error: function(x) { reject(x); },
          		beforeSend: function(xhr) {xhr.setRequestHeader('api-key', App.SearchQueryKey);}
			});
		});
		
		return prom;
	}

	searchSuggest(text) {
		console.log('Suggest: ' + text);
		var prom = new Ember.RSVP.Promise((resolve, reject) => {
			// fixme rename mainindexname 
			Ember.$.ajax({
				url: App.SearchEndpoint+'/indexes/' + App.SearchIndexName + '/docs/suggest?api-version=2015-02-28&suggesterName=default&fuzzy=true&search='+text,
          		type: 'GET',
          		dataType: 'json',
          		success: function(x) { resolve(x); },
          		error: function(x) { reject(x); },
          		beforeSend: function(xhr) {xhr.setRequestHeader('api-key', App.SearchQueryKey);}
			});
		});
		
		return prom;
	}


}


App.Data = new MyAppData();

