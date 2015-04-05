/// <reference path="../../typings/tsd.d.ts" />
interface Window {
    App: any;
}
declare var App: any;
declare var Holder: any;
declare module Gear {
    class UUID {
        static v4(): string;
    }
}
declare function promiseFor(val: any): Ember.RSVP.Promise;
declare class MyAppData {
    builds: any;
    userlists: any;
    buildlists: any;
    constructor();
    getBuild(bid: any): Ember.RSVP.Promise;
    getUserList(userKey: any): Ember.RSVP.Promise;
    getList(lid: any): Ember.RSVP.Promise;
    getUserBuilds(bid: any): JQueryXHR;
}
