﻿import {Component } from '@angular/core';
import {CiServer} from './models/ciServer';
import { ServerService } from './server.service';
import { MyBuildDefinition } from './models/myBuildDefinition';

@Component({
    templateUrl: './components/server.html'
})
export class Server {
    constructor(private serverService: ServerService) {
        
    }

    public ciServer = new CiServer();

    public projects: MyBuildDefinition[];

    public loadingProjects: boolean = false;

    public errorMessage: string = null;

    public getProjects() {
        this.loadingProjects = true;
        this.serverService.getProjects(this.ciServer)
            .then(projects => {
                this.projects = projects;
            }, ex => {
                this.errorMessage = ex;
            })
            .then(something => {
                this.loadingProjects = false;
            });
    }

    public onSave() {
        this.projects.forEach(project => {
            if (project.selected) {
                alert('you selected ' + project.name);
            }
        });
    }
}