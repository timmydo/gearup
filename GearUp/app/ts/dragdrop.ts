/// <reference path="app.ts" />

App.DragDropComponent = Ember.Component.extend({
	classNames: ['draggable-dropzone'],
	classNameBindings: ['dragClass'],
	dragClass: 'deactivated',

	dragLeave(event) {
		event.preventDefault();
		Ember.set(this, 'dragClass', 'deactivated');
	},

	dragOver(event) {
		event.preventDefault();
		Ember.set(this, 'dragClass', 'activated');
	},

	drop(event) {
		event.preventDefault();
		var data = event.dataTransfer;
		this.sendAction('dropped', data, this.get('Id'));

		Ember.set(this, 'dragClass', 'deactivated');
	}
});

