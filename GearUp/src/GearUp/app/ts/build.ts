/// <reference path="app.ts" />

declare var DropletView: any;
declare var DropletController: any;

var testbuild = new Gear.Build();
testbuild.parts = [new Gear.Build()];


App.BuildRoute = Ember.Route.extend({
	model: function () {
		console.log(testbuild);
		return testbuild;
	}
});

App.BuildController = (<any>Ember.ObjectController).extend(DropletController, {
	createdTime: function () {
		return moment(this.get('model.created')).format('ll');
	}.property('model.created'),

	    /**
     * Path that handles the file uploads.
     *
     * @property dropletUrl
     * @type {String}
     */
    dropletUrl: '/api/UploadImage',

    /**
     * @property dropletOptions
     * @type {Object}
     */
    dropletOptions: {
        fileSizeHeader: true,
        useArray: false
    },

    /**
     * Specifies the valid MIME types. Can used in an additive fashion by using the
     * property below.
     *
     * @property mimeTypes
     * @type {Array}
     */
    mimeTypes: ['image/bmp'],

    /**
     * Apply this property if you want your MIME types above to be appended to the white-list
     * as opposed to replacing the white-list entirely.
     *
     * @property concatenatedProperties
     * @type {Array}
     */
    concatenatedProperties: ['mimeTypes'],

    /**
     * @method didUploadFiles
     * @param response {Object}
     * @return {void}
     */
    didUploadFiles: function didUploadFiles(response) {
        console.log(response);
    },

    /**
     * @method didAddFiles
     * @param fileList {Array}
     * @return {void}
     */
    didAddFiles: function didAddFiles(fileList) {
        //this.send('uploadAllFiles');
        console.log(fileList);
    }

});


App.BuildView = Ember.View.extend({
	DragDrop: DropletView.extend(),
    didInsertElement: function () {
		console.log('run holder');
        Ember.run.next(null, function () {
            Holder.run();
        })
	}
});


