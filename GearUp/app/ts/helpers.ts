﻿/// <reference path="app.ts" />



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
	return new Ember.Handlebars.SafeString('<a href="#/builds/' + value.Id + '">' + (value.Title||'Untitled') + '</a>');
});

Ember.Handlebars.registerBoundHelper('listTitleAnchor', function (value) {
	if (!value) {
		return "";
	}
	return new Ember.Handlebars.SafeString('<a href="#/lists/' + value.Id + '">' + (value.Title||'Untitled') + '</a>');
});


Ember.Handlebars.registerBoundHelper('buildImageAnchor', function (value, value2) {
	if (!value || !value2) {
		return "";
	}
	return new Ember.Handlebars.SafeString('<a target="_blank" href="' + App.ImageEndpoint + '/' + value + '">' + (value2||'Untitled') + '</a>');
});


Ember.Handlebars.registerBoundHelper('buildThumbnailAnchor', function (value) {
	if (!value || !value.Images || value.Images.length < 1) {
		return "";
	}
	return new Ember.Handlebars.SafeString('<a href="#/builds/' + value.Id + '"><img class="recent-build-image-thumbnail" src="' + App.ImageEndpoint + '/' + (value.Images[0] || {Id:'empty'}).Id + '" /></a>');
});
