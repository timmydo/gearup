﻿/// <reference path="app.ts" />



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


