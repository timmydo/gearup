/// <reference path="../../typings/tsd.d.ts" />
interface Window {
    App: any;
}
declare var App: any;
declare var Holder: any;
declare var appInsights: any;
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
    removeFromListCache(list: any): void;
    getBuild(bid: any): Ember.RSVP.Promise;
    fillListBuilds(l: any): void;
    getUserList(userKey: any): Ember.RSVP.Promise;
    getList(lid: any): Ember.RSVP.Promise;
    getUserBuilds(bid: any): JQueryXHR;
}
declare class AITracker {
    track(msg: any, properties: any, metrics: any): void;
}
