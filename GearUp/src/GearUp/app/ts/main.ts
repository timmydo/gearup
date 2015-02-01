requirejs.config({
	paths: {
		'text': '/lib/requirejs-text/text',
		'durandal': '/lib/durandal/js',
		'plugins': '/lib/durandal/js/plugins',
		'transitions': '/lib/durandal/js/transitions',
		'knockout': '/lib/knockout.js/knockout',
		'jquery': '/lib/jquery/js/jquery'
	}
});

define(function (require) {
	var system = require('durandal/system'),
		app = require('durandal/app');

	system.debug(true);

	app.title = 'Durandal Starter Kit';

	app.configurePlugins({
		router: true,
		dialog: true
	});

	app.start().then(function () {
		app.setRoot('shell');
	});
});

