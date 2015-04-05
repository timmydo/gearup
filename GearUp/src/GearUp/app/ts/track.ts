/// <reference path="app.ts" />


class AITracker {

	track(msg, properties, metrics) {
		if (appInsights && appInsights.context && appInsights.context.user) {
			appInsights.context.user.id = window['UserIdentityKey'];
			appInsights.trackEvent(msg, properties, metrics);
		}
	}
}


App.Track = new AITracker();
//App.register('track:main', App.Track);
//App.inject('route', 'track', 'track:main');
//App.inject('controller', 'track', 'track:main');
