/// <reference path="../../typings/tsd.d.ts" />

interface Window { App: any }

declare var App: any;

window.App = Ember.Application.create({
	LOG_TRANSITIONS: true
});
