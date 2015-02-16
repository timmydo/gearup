/// <reference path="../../typings/tsd.d.ts" />


class FileUpload {

    public percentDone: number;
    public statusString: string;
    public fileid: string;

    constructor(public file: File, public buildid) {
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
        //var formData = new FormData();
        //formData.append('fileData', file);
		var fileuploadInstance = this;
		
        return new Ember.RSVP.Promise(function (resolve, reject) {
			var xhr = new XMLHttpRequest();
			xhr.open('POST', '/api/UploadImage?buildid=' + fileuploadInstance.buildid, true);
			xhr.onload = function (e) {
				if (this.status == 200) {
					console.log(this.response);
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
					fileuploadInstance.progress(value);
				}
			};

			xhr.setRequestHeader('Content-Type', file.type);
			xhr.send(file);

			//var reader = new FileReader();
			//reader.onload = function (e) {
			//	console.log('reader onload');
			//	console.log(e);
			//	//xhr.send(e.target);
			//}
			//reader.readAsArrayBuffer(file);


			//$.ajax({
			//	type: 'POST',
			//	url: '/api/UploadImage',
			//	data: formData,
			//	cache: false,
			//	contentType: false, //read from formData
			//	processData: false,
			//	success: (resp, status, jqxhr) => {
			//		resolve(resp);
			//	},
			//	error: (jqxhr, status, error) => {
			//		reject(error);
			//	},
			//	xhr: () => {
			//		var xhr = $.ajaxSettings.xhr();
			//		if (xhr.upload) {
			//			xhr.upload.addEventListener('progress',(event) => {
			//				this.progress(event);
			//			}, false);
			//		}
			//		return xhr;
			//	}
			//});
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