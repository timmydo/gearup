/// <reference path="../../typings/tsd.d.ts" />
window.App = Ember.Application.create({
    LOG_TRANSITIONS: true,
    //authToken: localStorage['authToken'],
    User: '',
    error: null
});
App.Router.map(function () {
    this.route('index', { path: '/' });
    this.route('login', { path: '/login' });
    this.route('register', { path: '/register' });
    this.route('build', { path: '/builds/:bid' });
    this.route('userbuilds', { path: '/userbuilds/:bid' });
    this.route('list', { path: '/lists/:bid' });
    this.route('userlists', { path: '/userlists/:bid' });
    this.route('search', { path: '/search/:q' });
});
App.ApplicationRoute = Ember.Route.extend({
    actions: {
        setError: function (e) {
            this.get('controller').set('errorMessage', e);
        },
        setInfo: function (e) {
            this.get('controller').set('infoMessage', e);
        }
    }
});
App.ApplicationController = Ember.Controller.extend({
    appName: 'Gear up',
    errorMessage: '',
    infoMessage: '',
    userLoggedIn: function () {
        return window['UserIdentityName'] !== '';
    }.property('window.UserIdentityName'),
    userLoginName: function () {
        return window['UserIdentityName'] || 'Unknown User';
    }.property('window.UserIdentityName'),
    userLoginKey: function () {
        return window['UserIdentityKey'] || '';
    }.property('window.UserIdentityKey'),
    latestSuggestionTag: '',
    suggest: function () {
        var _this = this;
        var q = this.get('searchQuery');
        if (q && q.length >= 3 && q.length < 25) {
            var tag = Gear.UUID.v4();
            this.set('latestSuggestionTag', tag);
            App.Data.searchSuggest(q).then(function (result) {
                // ensure that if a later request happened after this,
                // we wait for it instead of updating now
                if (_this.get('latestSuggestionTag') === tag) {
                    console.log(result);
                }
            });
        }
    }.observes('searchQuery'),
    actions: {
        suggest: function (q, syncRes, asyncRes) {
            App.Data.searchSuggest(q).then(function (x) {
                var buildlist = (x.value || []);
                if (asyncRes) {
                    asyncRes(buildlist);
                }
            });
        },
        setError: function (e) {
            this.set('errorMessage', e);
        },
        dismissError: function () {
            this.set('errorMessage', null);
        },
        setInfo: function (e) {
            this.set('infoMessage', e);
        },
        dismissInfo: function () {
            this.set('infoMessage', null);
        },
        search: function () {
            console.log(this.get('searchQuery'));
            this.transitionToRoute('search', this.get('searchQuery'));
        }
    }
});
var Gear;
(function (Gear) {
    var UUID = (function () {
        function UUID() {
        }
        UUID.v4 = function () {
            var d = Date.now();
            var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = (d + Math.random() * 16) % 16 | 0;
                d = Math.floor(d / 16);
                return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
            return uuid;
        };
        return UUID;
    })();
    Gear.UUID = UUID;
})(Gear || (Gear = {}));
/// <reference path="app.ts" />
App.BuildRoute = Ember.Route.extend({
    model: function (params) {
        return App.Data.getBuild(params.bid);
    },
    actions: {
        saveBuild: function () {
            var _this = this;
            var model = this.modelFor(this.routeName);
            var data = JSON.stringify(model);
            if (data) {
                model.save().then(function () {
                    _this.growl.success('Saved changes');
                }, function (xhr) {
                    _this.growl.error('Error saving build: ' + xhr.responseText);
                });
            }
        }
    }
});
App.BuildController = Ember.ObjectController.extend({
    needs: ["Userbuilds"],
    tempModified: false,
    tempUnit: function () {
        var cel = this.get('IsCelsius');
        if (cel) {
            return 'C';
        }
        else {
            return 'F';
        }
    }.property('IsCelsius'),
    createdTime: function () {
        return moment(this.get('model.Created')).format('ll');
    }.property('model.Created'),
    selectedImage: function () {
        var i = this.get('model.Images');
        if (i && i.length > 0) {
            return i[0].Id;
        }
    }.property('model.images'),
    selectedImageCaption: function () {
        var images = this.get('model.Images');
        var guid = this.get('selectedImage');
        for (var i = 0; i < images.length; i++) {
            if (images[i].Id === guid) {
                return images[i].Title;
            }
        }
        return "No caption found";
    }.property('selectedImage', 'editCaption'),
    userLoginKey: function () {
        return window['UserIdentityKey'] || '';
    }.property('window.UserIdentityKey'),
    canEditBuild: function () {
        return this.get('model.Creator') === window['UserIdentityKey'];
    }.property('model.Creator'),
    editTitle: false,
    editDescription: false,
    editCaption: false,
    savedTitle: '',
    savedDescription: '',
    progressBars: [],
    selectedParts: [],
    actions: {
        addBuildToList: function (listId) {
            var _this = this;
            var build = this.get('model');
            App.Data.getList(listId).then(function (list) {
                list.addBuildToList(build).then(function () {
                    _this.growl.success('Build added to list');
                }, function (xhr) {
                    _this.growl.error('Error adding build to list: ' + xhr.responseText);
                });
            });
        },
        deletePart: function (part) {
            var parts = this.get('Parts');
            parts.removeObject(part);
            this.send('saveBuild');
        },
        deleteSelectedImage: function () {
            var _this = this;
            var image = this.get('selectedImage');
            var b = this.get('model');
            this.set('editCaption', false);
            //b.images.removeObject(image);
            console.log(b);
            console.log(image);
            b.deleteImageFromBuild(image).then(function (x) {
                var i = _this.get('model.Images');
                if (i && i.length > 0) {
                    _this.set('selectedImage', i[0].Id);
                }
                _this.growl.success('Image deleted');
            }, function (xhr) {
                _this.growl.error('Error adding build to list: ' + xhr.responseText);
            });
        },
        addPart: function () {
            if (this.get('canEditBuild')) {
                this.get('Parts').pushObject({ Title: '' });
            }
        },
        tryDeleteBuild: function () {
            this.set('tryDelete', !this.get('tryDelete'));
        },
        deleteBuild: function () {
            var _this = this;
            this.set('tryDelete', false); // try preventing doubleclick
            var model = this.get('model');
            model.deleteBuild().then(function (data) {
                console.log(data);
                _this.transitionToRoute('userbuilds', model.Creator);
            }, function (xhr) {
                console.log(xhr);
                _this.growl.error('Error deleting build: ' + xhr.responseText);
            });
        },
        startEditTitle: function () {
            if (this.get('canEditBuild')) {
                this.savedTitle = this.get('Title');
                this.set('editTitle', true);
            }
        },
        startEditImageCaption: function () {
            if (this.get('canEditBuild')) {
                this.imageCaption = this.get('selectedImageCaption');
                this.set('editCaption', true);
            }
        },
        startEditDescription: function () {
            if (this.get('canEditBuild')) {
                this.savedDescription = this.get('Description');
                this.set('editDescription', true);
            }
        },
        discardDescription: function () {
            this.set('editDescription', false);
            this.set('Title', this.savedDescription);
        },
        saveImageCaption: function () {
            this.set('editCaption', false);
            var c = this.get('imageCaption');
            var si = this.get('selectedImage');
            var images = this.get('Images');
            console.log(c);
            console.log(si);
            console.log(images);
            for (var i = 0; i < images.length; i++) {
                if (images[i].Id === si) {
                    Ember.set(images[i], 'Title', c);
                    break;
                }
            }
            this.send('saveBuild');
        },
        changeCelsius: function () {
            this.set('tempModified', true);
            this.set('IsCelsius', !this.get("IsCelsius"));
        },
        saveTemperature: function () {
            this.set('tempModified', false);
            this.send('saveBuild');
        },
        saveDescription: function () {
            this.set('editDescription', false);
            this.send('saveBuild');
        },
        discardImageCaption: function () {
            this.set('editCaption', false);
            this.set('imageCaption', '');
        },
        discardTitle: function () {
            this.set('editTitle', false);
            this.set('Title', this.savedTitle);
        },
        saveTitle: function () {
            this.set('editTitle', false);
            this.send('saveBuild');
        },
        selectImage: function (guid) {
            this.set('selectedImage', guid);
        },
        uploadFile: function (evt, bid) {
            var _this = this;
            var files = evt.files;
            for (var i = 0; i < files.length; i++) {
                var file = files[i];
                var progressFunc = function (guid, val) {
                    console.log('progress ' + guid + ' ' + val);
                    _this.get('progressBars').forEach(function (item, idx) {
                        if (item.guid === guid) {
                            Ember.set(item, 'progress', val);
                        }
                    });
                    _this.get('progressBars').setObjects(_this.progressBars);
                };
                var build = this.get('model');
                var guid = Math.random().toString(36);
                var supportedFileTypes = {
                    'image/png': true,
                    'image/jpeg': true,
                    'image/gif': true,
                };
                if (!supportedFileTypes[file.type]) {
                    this.growl.error('Filetype not supported: ' + file.type);
                    return;
                }
                this.get('progressBars').pushObject({ guid: guid, name: file.name, progress: 0 });
                build.addImageToBuild(file, guid, progressFunc).then(function (success) {
                    //console.log("Removing upload " + guid);
                    _this.get('progressBars').setObjects(_this.progressBars.filter(function (x) { return x.guid !== guid; }));
                    _this.growl.success('Uploaded file');
                }, function (failure) {
                    console.log('upload fail');
                    console.log(failure);
                    _this.growl.error('Error uploading file: ' + failure);
                    _this.get('progressBars').setObjects(_this.progressBars.filter(function (x) { return x.guid !== guid; }));
                });
            }
        }
    }
});
/// <reference path="app.ts" />
App.getJSON = function (url) {
    var promise = new Ember.RSVP.Promise(function (resolve, reject) {
        var client = new XMLHttpRequest();
        client.open("GET", url);
        client.onreadystatechange = handler;
        var responseTypeAware = true;
        try {
            client.responseType = "json";
        }
        catch (e) {
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
                        }
                        else {
                            var parsed = JSON.parse(this.response);
                            resolve(parsed);
                        }
                    }
                    catch (e) {
                        reject('Error parsing:' + this.response + ' error: ' + e);
                    }
                }
                else {
                    reject(this);
                }
            }
        }
        ;
    });
    return promise;
};
App.BuildObject = Ember.Object.extend({
    save: function () {
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
    deleteBuild: function () {
        var _this = this;
        var data = JSON.stringify(this);
        App.Track.track("DeleteBuild", { Build: this.Id });
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/build/delete',
            contentType: 'application/json',
            data: data,
            dataType: 'text'
        }).then(function (res) {
            //fixme todo does deleting a build remove it from lists???
            App.Data.removeBuildFromCache(_this.Id);
            return res;
        });
    },
    deleteImageFromBuild: function (guid) {
        var opts = { Build: this.Id, Image: guid };
        var data = JSON.stringify(opts);
        App.Track.track("DeleteImage", opts);
        var thisbuild = this;
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/build/delete-image',
            contentType: 'application/json',
            data: data,
            dataType: 'text'
        }).then(function (res) {
            var obj = null;
            for (var i = thisbuild.Images.length - 1; i >= 0; i--) {
                if (thisbuild.Images[i].Id === guid) {
                    obj = thisbuild.Images[i];
                    break;
                }
            }
            if (obj) {
                thisbuild.Images.removeObject(obj);
            }
            return res;
        });
    },
    addImageToBuild: function (file, guid, progressFunc) {
        var thisbuild = this;
        App.Track.track("AddImageToBuild", { Build: thisbuild.Id });
        return new Ember.RSVP.Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', '/api/build/add-image?buildid=' + thisbuild.Id, true);
            xhr.onload = function (e) {
                if (this.status == 200) {
                    var img = { Id: this.response, Title: "none" };
                    thisbuild.Images.pushObject(img);
                    resolve(this.response);
                }
                else {
                    reject(this.response);
                }
            };
            xhr.onerror = function (e) {
                reject('error ' + xhr.status + ' - ' + xhr.statusText);
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
        var parseBack = JSON.parse(data);
        parseBack.Builds = parseBack.Builds.map(function (x) { return x.Id; });
        data = JSON.stringify(parseBack);
        App.Track.track("SaveList", { List: this.Id });
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/list/save',
            contentType: 'application/json',
            data: data,
            dataType: 'text'
        });
    },
    addBuildToList: function (b) {
        this.Builds.pushObject(b);
        return this.save();
    },
    removeBuildFromList: function (bid) {
        this.Builds.removeObjects(this.Builds.filter(function (b) { return b.Id === bid; }));
        return this.save();
    },
    deleteList: function () {
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
    return new Ember.RSVP.Promise(function (res) {
        return res(val);
    });
}
var MyAppData = (function () {
    function MyAppData() {
        this.builds = {};
        this.userlists = {};
        this.userbuilds = {};
        this.buildlists = {};
        this.recentbuilds = null;
    }
    MyAppData.prototype.removeBuildFromCache = function (bid) {
        if (App.Data.builds[bid]) {
            delete App.Data.builds[bid];
        }
        for (var key in this.userbuilds) {
            var list = this.userbuilds[key];
            list.removeObjects(list.filter(function (item) {
                return item.Id === bid;
            }));
        }
    };
    MyAppData.prototype.removeListFromCache = function (lid) {
        // remove this from list cache
        if (App.Data.buildlists[lid]) {
            delete App.Data.buildlists[lid];
        }
        // remove this from user lists
        for (var name in App.Data.userlists) {
            var ul = App.Data.userlists[name];
            ul.get('Lists').removeObjects(ul.get('Lists').filter(function (li) {
                return li.Id === lid;
            }));
        }
    };
    MyAppData.prototype.createBuild = function () {
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/build/create',
            contentType: 'application/json',
        });
    };
    MyAppData.prototype.createList = function () {
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/list/create',
            contentType: 'application/json',
        });
    };
    MyAppData.prototype.getBuild = function (bid) {
        var _this = this;
        console.log('Get Build: ' + bid);
        var b = this.builds[bid];
        if (!b) {
            return App.getJSON('/api/build/get/' + bid).then(function (res) {
                b = App.BuildObject.create(res);
                console.log(b);
                _this.builds[bid] = b;
                var ubl = _this.userbuilds[b.get('Creator')];
                if (ubl) {
                    if (!ubl.get('Builds').findBy('Id', b.Id)) {
                        console.log('Adding new build ' + b.Id + ' to user build list ' + b.get('Creator'));
                        ubl.get('Builds').pushObject(b);
                    }
                }
                return b;
            });
        }
        else {
            console.log('..from cache');
            return promiseFor(b);
        }
    };
    // given a list of build ids, parameter: l, fetch the ones that aren't in the cache
    // then set the l.builds property to a ember array of actual build objects, using the cache,
    // so that build objects are reused and are updated together
    MyAppData.prototype.fillListBuilds = function (l) {
        var origbuilds = l.get('Builds').slice(0);
        // set builds to the ones we haven't already cached--so we can load them
        var notCachedBuilds = origbuilds.filter(function (el) { return !App.Data.builds[el.Id]; });
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
            }).then(function (data) {
                var barray = Ember.A();
                data.forEach(function (elem) {
                    if (!App.Data.builds[elem.Id]) {
                        var item = App.BuildObject.create(elem);
                        App.Data.builds[elem.Id] = item;
                    }
                });
                origbuilds.forEach(function (guid) {
                    barray.pushObject(App.Data.builds[guid]);
                });
                l.set('Builds', barray);
            }, function (xhr) {
                console.log(xhr);
                App.Track.track("GetListError", { Message: 'Error getting user build list: ' + xhr.responseJSON });
            });
            // set the list to empty until we load everything
            l.set('Builds', Ember.A());
        }
        else {
            l.set('Builds', Ember.A());
            origbuilds.forEach(function (b) {
                l.get('Builds').pushObject(App.Data.builds[b]);
            });
        }
    };
    // a list has a List<string> of BuildLists, after calling this, it will
    // be a List<BuildList>
    MyAppData.prototype.fillListLists = function (l) {
        var _this = this;
        console.log("Fill List Lists");
        console.log(l);
        var origList = l.get('Lists').slice(0);
        // set builds to the ones we haven't already cached--so we can load them
        var notCached = origList.filter(function (el) { return !App.Data.buildlists[el.Id]; });
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
            }).then(function (data) {
                var arr = Ember.A();
                data.forEach(function (elem) {
                    if (!App.Data.buildlists[elem.Id]) {
                        var item = App.BuildListObject.create(elem);
                        _this.fillListBuilds(item);
                        App.Data.buildlists[elem.Id] = item;
                    }
                });
                origList.forEach(function (guid) {
                    arr.pushObject(App.Data.buildlists[guid]);
                });
                l.set('Lists', arr);
            }, function (xhr) {
                console.log(xhr);
                App.Track.track("GetListError", { Message: 'Error getting user build list: ' + xhr.responseJSON });
            });
            // set the list to empty until we load everything
            l.set('Lists', Ember.A());
        }
        else {
            l.set('Lists', Ember.A());
            origList.forEach(function (b) {
                l.get('Lists').pushObject(App.Data.buildlists[b]);
            });
        }
    };
    MyAppData.prototype.getUserList = function (userKey) {
        var _this = this;
        console.log('Get User List: ' + userKey);
        var l = this.userlists[userKey];
        if (!l) {
            return new Ember.RSVP.Promise(function (resolve, reject) {
                Ember.$.ajax({
                    type: 'GET',
                    url: '/api/user/lists/' + userKey,
                    dataType: 'json',
                }).then(function (res) {
                    l = App.UserListsObject.create({ Lists: res });
                    _this.fillListLists(l);
                    _this.userlists[userKey] = l;
                    resolve(l);
                }).fail(function (err) {
                    reject(err);
                });
            });
        }
        else {
            console.log('cached list');
            console.log(l);
            return promiseFor(l);
        }
    };
    MyAppData.prototype.getList = function (lid) {
        var _this = this;
        console.log('Get List: ' + lid);
        var l = this.buildlists[lid];
        if (!l) {
            return App.getJSON('/api/list/get/' + lid).then(function (res) {
                var l = App.BuildListObject.create(res);
                _this.fillListBuilds(l);
                _this.buildlists[lid] = l;
                var ubl = _this.userlists[l.get('Creator')];
                if (ubl) {
                    if (!ubl.get('Lists').findBy('Id', l.Id)) {
                        console.log('Adding new list ' + l.Id + ' to user build list ' + l.get('Creator'));
                        ubl.get('Lists').pushObject(l);
                    }
                }
                return l;
            });
        }
        else {
            console.log('cached list');
            console.log(l);
            return promiseFor(l);
        }
        return;
    };
    MyAppData.prototype.getRecentBuilds = function () {
        var _this = this;
        console.log('Get Recent Builds:');
        if (this.recentbuilds) {
            console.log('cached recent builds');
            return promiseFor(this.recentbuilds);
        }
        else {
            return App.getJSON('/api/build/recent').then(function (builds) {
                var bl = App.BuildListObject.create({ Builds: Ember.A(builds) });
                console.log(bl);
                _this.fillListBuilds(bl);
                _this.recentbuilds = bl;
                return bl;
            }, function (fail) {
                var bl = App.BuildListObject.create({ Builds: Ember.A([]) });
                console.log('Failure getting recent builds: ' + fail);
                _this.fillListBuilds(bl);
                _this.recentbuilds = bl;
                App.Track.track("GetRecentError", { Message: 'Error getting recent list: ' + fail });
            });
        }
    };
    MyAppData.prototype.getUserBuilds = function (bid) {
        var _this = this;
        console.log('Get User Builds: ' + bid);
        if (this.userbuilds[bid]) {
            console.log('cached list');
            return promiseFor(this.userbuilds[bid]);
        }
        else {
            return App.getJSON('/api/user/builds/' + bid).then(function (builds) {
                var bl = App.BuildListObject.create({ Builds: Ember.A(builds) });
                console.log(bl);
                _this.fillListBuilds(bl);
                _this.userbuilds[bid] = bl;
                return bl;
            });
        }
    };
    MyAppData.prototype.searchIndex = function (text) {
        var prom = new Ember.RSVP.Promise(function (resolve, reject) {
            Ember.$.ajax({
                url: '/api/search/builds/' + text,
                type: 'GET',
                dataType: 'json',
                success: function (x) { resolve(x); },
                error: function (x) { reject(x); },
            });
        });
        return prom;
    };
    MyAppData.prototype.searchSuggest = function (text) {
        console.log('Suggest: ' + text);
        var prom = new Ember.RSVP.Promise(function (resolve, reject) {
            // fixme rename mainindexname 
            Ember.$.ajax({
                url: 'https://gearbuilder.search.windows.net/indexes/builds/docs/suggest?api-version=2015-02-28&suggesterName=default&fuzzy=true&search=' + text,
                type: 'GET',
                dataType: 'json',
                success: function (x) { resolve(x); },
                error: function (x) { reject(x); },
                beforeSend: function (xhr) { xhr.setRequestHeader('api-key', 'ECA36C018F6E3D63C24062C94AB43082'); }
            });
        });
        return prom;
    };
    return MyAppData;
})();
App.Data = new MyAppData();
/// <reference path="app.ts" />
App.DragDropComponent = Ember.Component.extend({
    classNames: ['draggable-dropzone'],
    classNameBindings: ['dragClass'],
    dragClass: 'deactivated',
    dragLeave: function (event) {
        event.preventDefault();
        Ember.set(this, 'dragClass', 'deactivated');
    },
    dragOver: function (event) {
        event.preventDefault();
        Ember.set(this, 'dragClass', 'activated');
    },
    drop: function (event) {
        event.preventDefault();
        var data = event.dataTransfer;
        this.sendAction('dropped', data, this.get('Id'));
        Ember.set(this, 'dragClass', 'deactivated');
    }
});
/// <reference path="app.ts" />
App.GrowlInstanceComponent = Ember.Component.extend({
    classNames: ['growl-instance'],
    classNameBindings: ['type'],
    type: function () {
        return this.get('notification.options.type');
    }.property(),
    click: function () {
        this.destroyAlert();
    },
    didInsertElement: function () {
        if (this.get('notification.options.fadeIn')) {
            this.$().hide().fadeIn();
        }
        if (this.get('notification.options.twitch')) {
            var el = this.$(), maxDegree = 1, negative;
            var interval = window.setInterval(function () {
                negative = negative ? '' : '-';
                el.css('transform', 'rotate(' + negative + maxDegree + 'deg)');
            }, 75);
            Ember.run.later(this, function () {
                el.css('transform', 'rotate(0deg)');
                window.clearInterval(interval);
            }, 400);
        }
        // unless a click-to-dismiss is required we auto close
        if (!this.get('notification.options.clickToDismiss')) {
            Ember.run.later(this, this.destroyAlert, this.get('notification.options.closeIn'));
        }
    },
    destroyAlert: function () {
        var self = this;
        if (this.$()) {
            this.$().fadeOut(Ember.run(this, function () {
                // send the action on up so the manager can remove this item from array
                self.sendAction('action', self.get('notification'));
            }));
        }
        else {
            self.sendAction('action', self.get('notification'));
        }
    },
    actions: {
        dismiss: function () {
            // a close button has been clicked
            this.destroyAlert();
        }
    }
});
App.GrowlManagerComponent = Ember.Component.extend({
    classNames: ['growl-manager'],
    actions: {
        dismiss: function (notification) {
            this.get('notifications').removeObject(notification);
        }
    }
});
App.Growl = Ember.Object.extend({
    notifications: Ember.A(),
    error: function (context, opts) {
        opts = opts || {};
        opts.type = 'error';
        this._notify.call(this, context, opts);
        appInsights.trackEvent("ToastError", { Message: context }, {});
    },
    alert: function (context, opts) {
        opts = opts || {};
        opts.type = 'alert';
        this._notify.call(this, context, opts);
    },
    info: function (context, opts) {
        opts = opts || {};
        opts.type = 'info';
        this._notify.call(this, context, opts);
    },
    success: function (context, opts) {
        opts = opts || {};
        opts.type = 'success';
        this._notify.call(this, context, opts);
    },
    _notify: function (context, opts) {
        // default options
        var options = {
            type: 'error',
            fadeIn: true,
            closeIn: 5000,
            clickToDismiss: false,
            twitch: false
        };
        Ember.merge(options, opts);
        // if the developer passed an identical message then we just update
        // the open notification balloon options
        var existing = this.get('notifications').findBy('content', context);
        if (existing) {
            return;
        }
        var notification = Ember.ObjectProxy.extend({
            // {{notification.content}} for a string or {{notification.foo}} if you
            // pass an object from a route via this.growl.error({foo: 'bar'});
            content: context,
            options: options,
            updated: 0,
            isSuccess: function () {
                return options.type === 'success';
            }.property(),
            isInfo: function () {
                return options.type === 'info';
            }.property(),
            isAlert: function () {
                return options.type === 'alert';
            }.property(),
            isError: function () {
                return options.type === 'error';
            }.property()
        }).create();
        this.get('notifications').pushObject(notification);
    }
});
App.register('growl:main', App.Growl);
App.inject('route', 'growl', 'growl:main');
App.inject('controller', 'growl', 'growl:main');
/// <reference path="app.ts" />
Ember.Handlebars.registerBoundHelper('modifiedFrom', function (value) {
    var m = moment(value).fromNow();
    return new Ember.Handlebars.SafeString('<span class="moment-from">' + m + '</span>');
});
Ember.Handlebars.registerBoundHelper('buildThumbnail', function (value) {
    if (!value) {
        return "<img class='build-image-empty-thumbnail' />";
    }
    return new Ember.Handlebars.SafeString('<img class="build-image-thumbnail" src="' + App.ImageEndpoint + '/' + value + '" />');
});
Ember.Handlebars.registerBoundHelper('buildImage', function (value) {
    if (!value) {
        return "<img class='build-image-no-thumbnail' />";
    }
    return new Ember.Handlebars.SafeString('<img class="build-image-main" src="' + App.ImageEndpoint + '/' + value + '" />');
});
Ember.Handlebars.registerBoundHelper('buildTitleAnchor', function (value) {
    if (!value) {
        return "";
    }
    return new Ember.Handlebars.SafeString('<a href="#/builds/' + value.Id + '">' + (value.Title || 'Untitled') + '</a>');
});
Ember.Handlebars.registerBoundHelper('listTitleAnchor', function (value) {
    if (!value) {
        return "";
    }
    return new Ember.Handlebars.SafeString('<a href="#/lists/' + value.Id + '">' + (value.Title || 'Untitled') + '</a>');
});
Ember.Handlebars.registerBoundHelper('buildImageAnchor', function (value, value2) {
    if (!value || !value2) {
        return "";
    }
    return new Ember.Handlebars.SafeString('<a target="_blank" href="' + App.ImageEndpoint + '/' + value + '">' + (value2 || 'Untitled') + '</a>');
});
Ember.Handlebars.registerBoundHelper('buildThumbnailAnchor', function (value) {
    if (!value || !value.Images || value.Images.length < 1) {
        return "";
    }
    return new Ember.Handlebars.SafeString('<a href="#/builds/' + value.Id + '"><img class="recent-build-image-thumbnail" src="' + App.ImageEndpoint + '/' + (value.Images[0] || { Id: 'empty' }).Id + '" /></a>');
});
/// <reference path="app.ts" />
App.ImageController = Ember.ObjectController.extend({
    editing: false,
    actions: {
        editImage: function (Image) {
            this.setProperties({
                editing: true,
                newTitle: this.get('Title'),
            });
        },
        saveImage: function (Image) {
            this.setProperties({
                editing: false,
                Title: this.get('newTitle'),
            });
            this.send('saveBuild');
        },
        discardImage: function (Image) {
            this.set('editing', false);
        },
    }
});
/// <reference path="app.ts" />
App.IndexRoute = Ember.Route.extend({
    model: function (params) {
        return App.Data.getRecentBuilds();
    },
    actions: {
        error: function (reason) {
            console.log(reason);
        }
    }
});
App.IndexController = Ember.Controller.extend({
    groupedBuilds: function () {
        var a = Ember.A();
        var row = Ember.A();
        var count = 0;
        var builds = this.get('model.Builds');
        if (builds) {
            builds.forEach(function (b) {
                if (count >= 3) {
                    count = 0;
                    a.pushObject(row);
                    row = Ember.A();
                }
                // only display builds that have an image
                if (b.Images.length > 0) {
                    count++;
                    row.pushObject(b);
                }
            });
        }
        if (row.length > 0) {
            a.pushObject(row);
        }
        return a;
    }.property('model.Builds')
});
/// <reference path="app.ts" />
App.ListRoute = Ember.Route.extend({
    model: function (params) {
        return App.Data.getList(params.bid);
    },
    resetController: function (controller, isExiting, transition) {
        if (isExiting) {
            controller.set('startLoadBuildList', false);
        }
    },
    actions: {
        invalidateModel: function () {
            Ember.Logger.log('Route is now refreshing...');
            this.refresh();
        },
        saveList: function () {
            var _this = this;
            var model = this.modelFor(this.routeName);
            var data = JSON.stringify(model);
            console.log("Saving List " + data);
            if (data) {
                model.save().then(function (data) {
                    _this.growl.success('Saved list');
                }, function (xhr) {
                    console.log(xhr);
                    _this.growl.error('Error saving build: ' + xhr.responseText);
                });
            }
        }
    }
});
App.ListController = Ember.ObjectController.extend({
    canEditList: function () {
        return this.get('model.Creator') === window['UserIdentityKey'];
    }.property('model'),
    editTitle: false,
    savedTitle: '',
    savedDescription: '',
    startLoadBuildList: false,
    actions: {
        removeFromList: function (bid) {
            var _this = this;
            var model = this.get('model');
            model.removeBuildFromList(bid).then(function (data) {
                console.log(status);
                console.log(data);
                //this.set('startLoadBuildList', false);
                //this.send('invalidateModel');
            }, function (xhr) {
                console.log(xhr);
                console.log(status);
                _this.growl.error('Error removing item: ' + xhr.responseText);
            });
        },
        tryDeleteList: function () {
            this.set('tryDelete', !this.get('tryDelete'));
        },
        deleteList: function () {
            var _this = this;
            this.set('tryDelete', false); // try preventing doubleclick
            var model = this.get('model');
            model.deleteList().then(function (data) {
                //console.log(status);
                _this.growl.success('List deleted');
                _this.transitionToRoute('userlists', model.Creator);
            }, function (xhr) {
                console.log(xhr);
                _this.growl.error('Error deleting list: ' + xhr.responseText);
            });
        },
        startEditTitle: function () {
            if (this.get('canEditList')) {
                this.savedTitle = this.get('Title');
                this.savedDescription = this.get('Description');
                this.set('editTitle', true);
            }
        },
        discardTitle: function () {
            this.set('editTitle', false);
            this.set('Title', this.savedTitle);
        },
        saveTitle: function () {
            this.set('editTitle', false);
            this.send('saveList');
        },
    }
});
/// <reference path="app.ts" />
App.NavView = Ember.View.extend({
    tagName: 'li',
    classNameBindings: 'active'.w(),
    didInsertElement: function () {
        var _this = this;
        this._super();
        this.get('parentView').on('click', function () {
            _this.notifyPropertyChange('active');
        });
    },
    active: function () {
        return this.get('childViews.firstObject.active');
    }.property()
});
/// <reference path="app.ts" />
App.PartController = Ember.ObjectController.extend({
    editing: function () {
        return this.get('model.title') === '';
    }.property('title'),
    actions: {
        editPart: function () {
            this.setProperties({
                editing: true,
                newTitle: this.get('Title'),
                newUrl: this.get('Url'),
                newPrice: this.get('Price')
            });
        },
        savePart: function () {
            this.setProperties({
                editing: false,
                Title: this.get('newTitle'),
                Url: this.get('newUrl'),
                Price: this.get('newPrice')
            });
            this.send('saveBuild');
        },
        discardPart: function () {
            this.set('editing', false);
        },
    }
});
/// <reference path="app.ts" />
App.RegisterRoute = Ember.Route.extend({
    setupController: function (controller) {
        // `controller` is the instance of ApplicationController
        controller.set('title', "Hello world!");
    }
});
App.RegisterController = Ember.Controller.extend({});
/// <reference path="app.ts" />
App.SearchRoute = Ember.Route.extend({
    model: function (params) {
        return App.Data.searchIndex(params.q).then(function (res) {
            var barray = Ember.A();
            res.forEach(function (elem) {
                if (!App.Data.builds[elem.Id]) {
                    var item = App.BuildObject.create(elem);
                    App.Data.builds[elem.Id] = item;
                }
                barray.pushObject(App.Data.builds[elem.Id]);
            });
            return { "Builds": barray, "Query": params.q };
        });
    },
    actions: {}
});
App.SearchController = Ember.ObjectController.extend({
    searchQuery: ''
});
/// <reference path="app.ts" />
App.BootstrapSliderComponent = Ember.TextField.extend({
    classNames: ['slider'],
    classNameBindings: ['type'],
    attributeBindings: ['type', 'min', 'max', 'value', 'handle', 'enabled'],
    updateValue: function () {
        //console.log('update value');
        //this.$().slider({
        //	min: this.get('min'),
        //	max: this.get('max'),
        //	value: this.get('value'),
        //	handle: this.get('handle'),
        //	tooltip: 'hide'
        //});
    }.observes('value'),
    didInsertElement: function () {
        var self = this;
        var opts = {
            min: this.get('min'),
            max: this.get('max'),
            value: [this.get('low') || 0, this.get('high') || 100],
            handle: this.get('handle'),
            enabled: !!this.get('enabled')
        };
        this.$().slider(opts);
        //this.$().on('slide', function () {
        //	self.set('value', this.value);
        //});
        this.$().on('slideStop', function () {
            var temp = this.value.split(',');
            self.set('low', parseInt(temp[0]));
            self.set('high', parseInt(temp[1]));
            self.set('modified', true);
        });
    },
    willDestroyElement: function () {
        this.$().off('slide');
        this.$().off('slideStop');
    }
});
/// <reference path="app.ts" />
var AITracker = (function () {
    function AITracker() {
    }
    AITracker.prototype.track = function (msg, properties, metrics) {
        if (appInsights && appInsights.context && appInsights.context.user) {
            appInsights.context.user.id = window['UserIdentityKey'];
            appInsights.trackEvent(msg, properties, metrics);
        }
    };
    return AITracker;
})();
App.Track = new AITracker();
//App.register('track:main', App.Track);
//App.inject('route', 'track', 'track:main');
//App.inject('controller', 'track', 'track:main');
/// <reference path="app.ts" />
App.TypeAheadComponent = Ember.TextField.extend({
    classNames: ['type-ahead', 'form-control'],
    didInsertElement: function () {
        var _this = this;
        this._super();
        var elem = $(this.element);
        var datasource = {
            name: this.$().attr('id') || "typeahead",
            limit: this.get("limit") || 15,
            source: function (q, sr, asr) {
                _this.sendAction('action', q, sr, asr);
            },
            async: true,
            displayKey: this.get("display"),
            valueKey: this.get("display") || this.get("valueKey")
        };
        console.log(datasource);
        this.typeahead = elem.typeahead({
            minLength: 3,
            highlight: true
        }, datasource);
        this.typeahead.on("typeahead:selected", function (event, item) {
            _this.set("selection", item.emberObject);
        });
        this.typeahead.on("typeahead:autocompleted", function (event, item) {
            _this.set("selection", item.emberObject);
        });
        if (this.get("selection")) {
            this.typeahead.val(this.get("selection.name"));
        }
    },
    selectionObserver: function () {
        if (Ember.isEmpty(this.get('selection'))) {
            return this.typeahead.val('');
        }
        return this.typeahead.val(this.get("selection").get(this.get("name")));
    }.observes("selection")
});
/// <reference path="app.ts" />
App.UserbuildsRoute = Ember.Route.extend({
    model: function (params) {
        var bid = params.bid;
        if (!bid) {
            bid = window['UserIdentityKey'] || '';
        }
        return App.Data.getUserBuilds(bid);
    }
});
App.UserbuildsController = Ember.ObjectController.extend({
    userLoginKey: function () {
        return window['UserIdentityKey'] || '';
    }.property('window.UserIdentityKey'),
    //fixme async property
    userBuildList: function (key, value, previousValue) {
        var _this = this;
        var userKey = this.get('userLoginKey');
        if (arguments.length > 1) {
            //setter
            return value;
        }
        if (userKey && !value) {
            App.Data.getUserList(userKey).then(function (data) {
                _this.set('userBuildList', data);
                return data;
            }, function (xhr) {
                console.log(xhr);
                _this.growl.error('Error getting user build list: ' + xhr.responseJSON);
            });
        }
        else {
            console.log('Not loading userbuild list');
            console.log(userKey);
            console.log(value);
        }
        return value || [];
    }.property('userLoginKey'),
    actions: {
        createBuild: function () {
            var _this = this;
            App.Data.createBuild().then(function (bid) {
                _this.transitionToRoute('build', bid.Id);
            });
        },
        addBuildToList: function (thelist, thebuild) {
            var _this = this;
            thelist.addBuildToList(thebuild).then(function () {
                _this.growl.success('Build added to list');
            }, function (xhr) {
                _this.growl.error('Error adding build to list: ' + xhr.responseText);
            });
        },
    }
});
/// <reference path="app.ts" />
App.UserlistsRoute = Ember.Route.extend({
    model: function (params) {
        var bid = params.bid;
        if (!bid) {
            bid = window['UserIdentityKey'] || '';
        }
        return App.Data.getUserList(bid);
    }
});
App.UserlistsController = Ember.ObjectController.extend({
    actions: {
        createList: function () {
            var _this = this;
            App.Data.createList().then(function (lid) {
                _this.transitionToRoute('list', lid.Id);
            });
        }
    }
});
