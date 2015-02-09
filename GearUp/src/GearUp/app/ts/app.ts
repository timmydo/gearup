/// <reference path="../../typings/tsd.d.ts" />

interface Window { App: any }

declare var App: any;

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

	export class Part {
		pid: string;
		creator: User;
		title: string;
		description: string;
		mfrLink: string;
		buyLink: string[];
		price: string;
		type: string;
		img: Image[];
		quantity: number;

		constructor() {
			this.pid = UUID.v4();
			this.creator = new User();
			this.title = 'part';
			this.description = 'part desc';
			this.mfrLink = 'mfrlink';
			this.buyLink = [];
			this.price = 'price';
			this.type = 'parttype';
			this.img = [];
		}

	}

	export class Build {
		bid: string;
		modified: moment.Moment;
		created: moment.Moment;
		creator: string;
		title: string;

		parts: Part[]

		constructor() {
			this.bid = UUID.v4();
			this.modified = moment();
			this.created = moment();
			this.creator = '';
			this.parts = [];
			this.title = 'title'
		}

	}

}