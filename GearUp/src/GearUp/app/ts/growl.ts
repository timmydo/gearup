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
			var el = this.$(),
				maxDegree = 1,
				negative;
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
		} else {
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
		appInsights.trackEvent("ToastError", {Message: context}, {});
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
			closeIn: 5000, // automatically close in 5 seconds.
			clickToDismiss: false, // stay open until it receives a click?
			twitch: false
		};

		Ember.merge(options, opts);

		// if the developer passed an identical message then we just update
		// the open notification balloon options
		var existing = this.get('notifications').findBy('content', context);
		if (existing) {
			return;
		}

		var notification = (<any>Ember.ObjectProxy.extend({
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
		})).create();

		this.get('notifications').pushObject(notification);
	}
});


App.register('growl:main', App.Growl);
App.inject('route', 'growl', 'growl:main');
App.inject('controller', 'growl', 'growl:main');
