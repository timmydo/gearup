/// <reference path="../../typings/tsd.d.ts" />

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

	export class User {
		uid: string;
		title: string;

		constructor() {
			this.uid = UUID.v4();
			this.title = 'Random user';
		}

	}

	export class Image {
		iid: string;
		user: string;
		created: moment.Moment;
	}

	export class Build {
		version: number;
		bid: string;
		modified: number;
		created: number;
		creator: User;

		title: string;
		description: string;

		mfrLink: string;
		buyLink: string[];
		price: string;

		images: Image[];
		parts: Build[]

		constructor() {
			this.version = 1;
			this.bid = UUID.v4();
			this.modified = Date.now();
			this.created = Date.now();
			this.creator = new User();

			this.parts = [];
			this.images = [];

			this.title = 'Title';
			this.description = 'No description';

			this.price = 'N/A';
			this.buyLink = [];
			this.mfrLink = '';
		}

	}

}