/// <reference path="../../typings/tsd.d.ts" />


class FileUpload {

    public percentDone: number;
    public statusString: string;
	public guid: string;

    constructor(public file: File, public buildid, public progressFunc) {
        this.percentDone = 0;
        this.statusString = 'Starting...';
		this.guid = Math.random().toString(36);
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

	private uploadAsync(file: File) {
		var fileuploadInstance = this;
		
        return new Ember.RSVP.Promise(function (resolve, reject) {
			var xhr = new XMLHttpRequest();
			xhr.open('POST', '/api/UploadImage?buildid=' + fileuploadInstance.buildid, true);
			xhr.onload = function (e) {
				if (this.status == 200) {
					resolve(this.response);
				} else {
					reject(this.response);
				}
			};
			xhr.onerror = function (e) {
				reject(e.error);
			};
			xhr.upload.onprogress = function (e) {
				if (e.lengthComputable) {
					var value = (e.loaded / e.total) * 100;

					if (fileuploadInstance.progressFunc) {
						fileuploadInstance.progressFunc(fileuploadInstance.guid, value);
					}
				}
			};

			xhr.setRequestHeader('Content-Type', file.type);
			xhr.send(file);
		});
	}

    public start() {
        if (!this.isSupported()) {
            this.error('Unsupported file type');
            return;
        }
        var prom = this.uploadAsync(this.file);
        return prom;
    }

}