/// <reference path="../../typings/tsd.d.ts" />


class FileUpload {

    public percentDone: number;
    public statusString: string;
    public fileid: string;

    constructor(public file: File) {
        this.percentDone = 0;
        this.statusString = 'Starting...';
    }

    public isSupported() {
        var supportedFileTypes = {
            'image/png': true,
            'image/jpeg': true,
            'image/gif': true,
        };
        return !!supportedFileTypes[this.file.type];
    }

    private success(val) {
        this.percentDone = 100;
        console.log('success');
        console.log(val);
        this.statusString = 'Success';
    }

    private error(val) {
        this.percentDone = 100;
        console.log('error');
        console.log(val);
        if (typeof (val) === 'string') {
            this.statusString = 'Error: ' + val;
        }
    }
    private progress(val) {
        console.log('progress');
        console.log(val);
    }

	private uploadAsync(file: File) {
        var formData = new FormData();
        formData.append('fileData', file);
        return new Ember.RSVP.Promise(function (resolve, reject) {
			$.ajax({
				type: 'POST',
				url: '/api/UploadImage',
				data: formData,
				cache: false,
				contentType: false, //read from formData
				processData: false,
				success: (resp, status, jqxhr) => {
					resolve(resp);
				},
				error: (jqxhr, status, error) => {
					reject(error);
				},
				xhr: () => {
					var xhr = $.ajaxSettings.xhr();
					if (xhr.upload) {
						xhr.upload.addEventListener('progress',(event) => {
							this.progress(event);
						}, false);
					}
					return xhr;
				}
			});
		});
	}

    public start() {
        if (!this.isSupported()) {
            this.error('Unsupported file type');
            return;
        }
        var prom = this.uploadAsync(this.file);
        return prom.then((val) => {
            this.success(val);
        },(val) => {
                this.error(val);
            }/*,(val) => {
                this.progress(val);
		}*/
			);
    }

}