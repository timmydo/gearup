/// <reference path="../../typings/tsd.d.ts" />
/// <reference path="fileupload.ts" />

interface Window { App: any }

declare var App: any;
declare var Holder: any;

window.App = Ember.Application.create({
	LOG_TRANSITIONS: true,
    //authToken: localStorage['authToken'],
	User: '',
    error: null
});



module Gear {

	export class UUID {
		static v4() {
			var d = Date.now();
			var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				var r = (d + Math.random() * 16) % 16 | 0;
				d = Math.floor(d / 16);
				return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
			});
			return uuid;
		}
	}


}