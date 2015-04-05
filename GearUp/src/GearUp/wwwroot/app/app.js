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
    actions: {
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
                    _this.send('setInfo', 'Saved changes');
                }, function (xhr) {
                    _this.send('setError', 'Error saving build: ' + xhr.responseText);
                });
            }
        }
    }
});
App.BuildController = Ember.ObjectController.extend({
    createdTime: function () {
        return moment(this.get('model.created')).format('ll');
    }.property('model.created'),
    selectedImage: function () {
        var i = this.get('model.images');
        if (i && i.length > 0) {
            return i[0].guid;
        }
    }.property('model.images'),
    selectedImageCaption: function () {
        var images = this.get('model.images');
        var guid = this.get('selectedImage');
        for (var i = 0; i < images.length; i++) {
            if (images[i].guid === guid) {
                return images[i].title;
            }
        }
        return "No caption found";
    }.property('selectedImage', 'editCaption'),
    userLoginKey: function () {
        return window['UserIdentityKey'] || '';
    }.property('window.UserIdentityKey'),
    canEditBuild: function () {
        return this.get('model.creator') === window['UserIdentityKey'];
    }.property('model.creator'),
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
                _this.send('setError', 'Error getting user build list: ' + xhr.responseJSON);
            });
        }
        else {
            console.log('Not loading userbuild list');
            console.log(userKey);
            console.log(value);
        }
        return value || [];
    }.property('userLoginKey'),
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
                list.addBuildToList(build.id).then(function () {
                    _this.send('setInfo', 'Build added to list');
                }, function (xhr) {
                    _this.send('setError', 'Error adding build to list: ' + xhr.responseText);
                });
            });
        },
        deletePart: function (part) {
            var parts = this.get('parts');
            parts.removeObject(part);
            this.send('saveBuild');
        },
        deleteSelectedImage: function () {
            var _this = this;
            var image = this.get('selectedImage');
            var b = this.get('model');
            //b.images.removeObject(image);
            console.log(b);
            console.log(image);
            b.deleteImageFromBuild(image).then(function (x) {
                _this.send('setInfo', 'Image deleted');
            }, function (xhr) {
                _this.send('setError', 'Error adding build to list: ' + xhr.responseText);
            });
        },
        addPart: function () {
            if (this.get('canEditBuild')) {
                this.get('parts').pushObject({ title: '' });
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
                _this.transitionToRoute('userbuilds', model.creator);
            }, function (xhr) {
                console.log(xhr);
                _this.send('setError', 'Error deleting build: ' + xhr.responseText);
            });
        },
        startEditTitle: function () {
            if (this.get('canEditBuild')) {
                this.savedTitle = this.get('title');
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
                this.savedDescription = this.get('description');
                this.set('editDescription', true);
            }
        },
        discardDescription: function () {
            this.set('editDescription', false);
            this.set('title', this.savedDescription);
        },
        saveImageCaption: function () {
            this.set('editCaption', false);
            var c = this.get('imageCaption');
            var si = this.get('selectedImage');
            var images = this.get('images');
            console.log(c);
            console.log(si);
            console.log(images);
            for (var i = 0; i < images.length; i++) {
                if (images[i].guid === si) {
                    Ember.set(images[i], 'title', c);
                    break;
                }
            }
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
            this.set('title', this.savedTitle);
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
                    this.send('setError', 'Filetype not supported: ' + file.type);
                    return;
                }
                this.get('progressBars').pushObject({ guid: guid, name: file.name, progress: 0 });
                build.addImageToBuild(file, guid, progressFunc).then(function (success) {
                    console.log("Removing upload " + guid);
                    _this.get('progressBars').setObjects(_this.progressBars.filter(function (x) {
                        return x.guid !== guid;
                    }));
                    _this.send('setInfo', 'Uploaded file');
                }, function (failure) {
                    console.log('upload fail');
                    console.log(failure);
                    _this.send('setError', 'Error uploading file: ' + failure);
                    _this.get('progressBars').setObjects(_this.progressBars.filter(function (x) {
                        return x.guid !== guid;
                    }));
                });
            }
        }
    }
});
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
        var _this = this;
        var data = JSON.stringify(this);
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/DeleteBuild',
            contentType: 'application/json',
            data: data,
            dataType: 'text'
        }).then(function (res) {
            //fixme todo does deleting a build remove it from lists???
            var idx = App.Data.builds.indexOf(_this);
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
        }).then(function (res) {
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
                }
                else {
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
        var _this = this;
        var data = JSON.stringify({ build: bid, list: this.id });
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/AddBuildToList',
            contentType: 'application/json',
            data: data,
            dataType: 'text'
        }).then(function (success) {
            _this.builds.pushObject(bid);
        });
    },
    removeBuildFromList: function (bid) {
        var _this = this;
        var data = JSON.stringify({ 'build': bid, 'list': this.id });
        return Ember.$.ajax({
            type: 'POST',
            url: '/api/RemoveBuildFromList',
            contentType: 'application/json',
            data: data,
            dataType: 'text'
        }).then(function (success) {
            _this.builds.removeObject(bid);
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
var MyAppData = (function () {
    function MyAppData() {
        this.builds = {};
        this.userlists = {};
        this.buildlists = {};
    }
    MyAppData.prototype.getBuild = function (bid) {
        var _this = this;
        var b = this.builds[bid];
        if (!b) {
            return Ember.$.getJSON('/api/build/' + bid).then(function (res) {
                b = App.BuildObject.create(res);
                console.log('get build ' + bid);
                console.log(b);
                _this.builds[bid] = b;
                return b;
            });
        }
        else {
            return promiseFor(b);
        }
    };
    MyAppData.prototype.getUserList = function (userKey) {
        var _this = this;
        var l = this.userlists[userKey];
        if (!l) {
            return Ember.$.ajax({
                type: 'GET',
                url: '/api/UserLists/' + userKey,
                dataType: 'json',
            }).then(function (res) {
                l = App.UserListsObject.create({ lists: res });
                console.log('get list ' + userKey);
                console.log(l);
                _this.userlists[userKey] = l;
                return l;
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
        var l = this.buildlists[lid];
        if (!l) {
            return Ember.$.getJSON('/api/list/' + lid).then(function (res) {
                l = App.BuildListObject.create(res);
                console.log('get list ' + lid);
                console.log(l);
                _this.buildlists[lid] = l;
                return l;
            });
        }
        else {
            return promiseFor(l);
        }
        return;
    };
    MyAppData.prototype.getUserBuilds = function (bid) {
        return Ember.$.getJSON('/api/UserBuilds/' + bid);
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
        this.sendAction('dropped', data, this.get('guid'));
        Ember.set(this, 'dragClass', 'deactivated');
    }
});
/// <reference path="app.ts" />
Ember.Handlebars.registerBoundHelper('modifiedFrom', function (value) {
    var m = moment(value).fromNow();
    return new Ember.Handlebars.SafeString('<span class="moment-from">' + m + '</span>');
});
Ember.Handlebars.registerBoundHelper('buildThumbnail', function (value) {
    return new Ember.Handlebars.SafeString('<img class="build-image-thumbnail" src="' + App.ImageEndpoint + '/' + value + '" />');
});
Ember.Handlebars.registerBoundHelper('buildImage', function (value) {
    return new Ember.Handlebars.SafeString('<img class="build-image-main" src="' + App.ImageEndpoint + '/' + value + '" />');
});
Ember.Handlebars.registerBoundHelper('buildTitleAnchor', function (value) {
    return new Ember.Handlebars.SafeString('<a href="#/builds/' + value.id + '">' + value.title + '</a>');
});
Ember.Handlebars.registerBoundHelper('listTitleAnchor', function (value) {
    return new Ember.Handlebars.SafeString('<a href="#/lists/' + value.id + '">' + value.title + '</a>');
});
Ember.Handlebars.registerBoundHelper('buildImageAnchor', function (value, value2) {
    return new Ember.Handlebars.SafeString('<a target="_blank" href="' + App.ImageEndpoint + '/' + value + '">' + (value2 || 'Untitled') + '</a>');
});
/// <reference path="app.ts" />
App.ImageController = Ember.ObjectController.extend({
    editing: false,
    actions: {
        editImage: function (Image) {
            this.setProperties({
                editing: true,
                newTitle: this.get('title'),
            });
        },
        saveImage: function (Image) {
            this.setProperties({
                editing: false,
                title: this.get('newTitle'),
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
    setupController: function (controller) {
        // `controller` is the instance of ApplicationController
        controller.set('title', "Hello world!");
    }
});
App.IndexController = Ember.Controller.extend({});
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
                    console.log(data);
                    _this.send('setInfo', 'Saved list');
                }, function (xhr) {
                    console.log(xhr);
                    _this.send('setError', 'Error saving build: ' + xhr.responseText);
                });
            }
        }
    }
});
App.ListController = Ember.ObjectController.extend({
    canEditList: function () {
        return this.get('model.creator') === window['UserIdentityKey'];
    }.property('model'),
    editTitle: false,
    savedTitle: '',
    savedDescription: '',
    startLoadBuildList: false,
    buildList: function (key, value, previousValue) {
        var _this = this;
        var list = [];
        var firstCall = true;
        var model = this.get('model');
        //setter
        if (arguments.length > 1) {
            list = value;
        }
        else {
            if (!this.get('startLoadBuildList')) {
                this.set('startLoadBuildList', true);
                model.getBuilds().then(function (data, status) {
                    console.log(data);
                    _this.set('buildList', data);
                }, function (xhr, status, err) {
                    console.log(xhr);
                    _this.send('setError', 'Error getting build list: ' + xhr.responseJSON);
                });
            }
        }
        //getter
        return list;
    }.property('model'),
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
                _this.send('setError', 'Error removing item: ' + xhr.responseText);
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
                console.log(status);
                _this.send('setInfo', 'List deleted');
                _this.transitionToRoute('userlists', model.creator);
            }, function (xhr) {
                console.log(xhr);
                _this.send('setError', 'Error deleting list: ' + xhr.responseText);
            });
        },
        startEditTitle: function () {
            if (this.get('canEditList')) {
                this.savedTitle = this.get('title');
                this.savedDescription = this.get('description');
                this.set('editTitle', true);
            }
        },
        discardTitle: function () {
            this.set('editTitle', false);
            this.set('title', this.savedTitle);
        },
        saveTitle: function () {
            this.set('editTitle', false);
            this.send('saveList');
        },
    }
});
/// <reference path="app.ts" />
App.LoginCreds = Ember.Object.extend({
    username: null,
    password: null,
    remember: false,
    json: function () {
        return {
            username: this.get('username') || '',
            password: this.get('password') || '',
            remember: !!this.get('remember')
        };
    }
});
App.LoginRoute = Ember.Route.extend({
    model: function () {
        // let our login template fill in the properties of a creds object
        return App.LoginCreds.create({});
    },
});
App.LoginController = Ember.ObjectController.extend({});
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
    editing: false,
    actions: {
        editPart: function (part) {
            this.setProperties({
                editing: true,
                newTitle: this.get('title'),
                newUrl: this.get('url'),
                newPrice: this.get('price')
            });
        },
        savePart: function (part) {
            this.setProperties({
                editing: false,
                title: this.get('newTitle'),
                url: this.get('newUrl'),
                price: this.get('newPrice')
            });
            this.send('saveBuild');
        },
        discardPart: function (part) {
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
App.UserbuildsRoute = Ember.Route.extend({
    model: function (params) {
        var bid = params.bid;
        if (!bid) {
            bid = window['UserIdentityKey'] || '';
        }
        return App.Data.getUserBuilds(bid);
    }
});
App.UserbuildsController = Ember.ArrayController.extend({
    actions: {
        createBuild: function () {
            this.transitionToRoute('build', Gear.UUID.v4());
        }
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
            this.transitionToRoute('list', Gear.UUID.v4());
        }
    }
});
//# sourceMappingURL=app.js.map