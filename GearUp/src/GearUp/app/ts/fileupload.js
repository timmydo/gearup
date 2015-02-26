/// <reference path="../../typings/tsd.d.ts" />
var FileUpload = (function () {
    function FileUpload(file, buildid, progressFunc) {
        this.file = file;
        this.buildid = buildid;
        this.progressFunc = progressFunc;
        this.percentDone = 0;
        this.statusString = 'Starting...';
        this.guid = Math.random().toString(36);
    }
    FileUpload.prototype.isSupported = function () {
        var supportedFileTypes = {
            'image/png': true,
            'image/jpeg': true,
            'image/gif': true,
        };
        return !!supportedFileTypes[this.file.type];
    };
    FileUpload.prototype.success = function (val) {
        this.percentDone = 100;
        console.log('success');
        console.log(val);
        this.statusString = 'Success';
    };
    FileUpload.prototype.error = function (val) {
        this.percentDone = 100;
        console.log('error');
        console.log(val);
        if (typeof (val) === 'string') {
            this.statusString = 'Error: ' + val;
        }
    };
    FileUpload.prototype.uploadAsync = function (file) {
        var fileuploadInstance = this;
        return new Ember.RSVP.Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', '/api/UploadImage?buildid=' + fileuploadInstance.buildid, true);
            xhr.onload = function (e) {
                if (this.status == 200) {
                    resolve(this.response);
                }
                else {
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
    };
    FileUpload.prototype.start = function () {
        if (!this.isSupported()) {
            return new Ember.RSVP.Promise(function (resolve, reject) {
                reject('Unsupported file type');
            });
        }
        return this.uploadAsync(this.file);
    };
    return FileUpload;
})();
