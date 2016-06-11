﻿import {Component} from '@angular/core';
import {Routes, Router, Route, ROUTER_DIRECTIVES} from '@angular/router';
import 'rxjs/add/operator/toPromise';
import { Headers, Http } from '@angular/http';

@Component({
    template: `
<h1>Show Off</h1>
<form class="form-horizontal">
    <div class="form-group">
        <label for="ledPattern" class="col-sm-2 control-label">LED Pattern</label>
        <div class="col-sm-10">
            <select id="ledPattern" class="form-control">
                <option *ngFor="let pattern of ledPatterns">{{pattern}}</option>
            </select>
        </div>
    </div>
    <div class="form-group">
        <label for="audioPattern" class="col-sm-2 control-label">Audio Pattern</label>
        <div class="col-sm-10">
            <select id="audioPattern" class="form-control">
                <option *ngFor="let pattern of audioPatterns">{{pattern}}</option>
            </select>
        </div>
    </div>
    <div class="form-group">
        <div class="col-sm-offset-2 col-sm-10">
            <button type="submit" class="btn btn-primary">Play</button>
        </div>
    </div>
</form>
`
})
export class ShowOff {
    constructor(private http: Http) {
        http.get('/api/ledPatterns')
            .toPromise()
            .then(result => this.ledPatterns = result.json());
        http.get('/api/audioPatterns')
            .toPromise()
            .then(result => this.audioPatterns = result.json());
    }

    public ledPatterns:string[];
    public audioPatterns:string[];
}