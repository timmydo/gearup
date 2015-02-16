/// <reference path="app.ts" />

declare var DropletView: any;
declare var DropletController: any;


App.BuildRoute = Ember.Route.extend({
	model: function (params) {
		return Ember.$.getJSON('/api/build/' + params.bid);
	}
});

App.BuildController = Ember.ObjectController.extend({
	createdTime: function () {
		return moment(this.get('model.created')).format('ll');
	}.property('model.created'),



});


App.BuildView = Ember.View.extend({
	attributeBindings: ['data-guid'],
    didInsertElement: function () {
		console.log('run holder');
        Ember.run.next(null, function () {
            Holder.run();

			var preventDefault = (e: JQueryEventObject) => {
                e.stopPropagation();
                e.preventDefault();
                return false;
            };

            var drop = (e: JQueryEventObject) => {
                e.stopPropagation();
                e.preventDefault();
                var files = (<any>e).dataTransfer.files;
				var bid = $('#upload-box').attr('data-guid');
				//$log.log('Upload');
				//$log.log(files);
				for (var i = 0; i < files.length; i++) {
					var file = files[i];
					var upload = new FileUpload(file, bid);
					upload.start();
					//fileUploads.push(upload);
				}
                return false;
            };

            console.log('binding events');
            $('#upload-box').bind('dragover', preventDefault);
            $('#upload-box').bind('dragenter', preventDefault);
            $('#upload-box').bind('drop', drop);
        })
	}
});


