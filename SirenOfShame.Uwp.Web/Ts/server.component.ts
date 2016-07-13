﻿import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CiEntryPointSetting } from './models/ciEntryPointSetting';
import { GetBuildDefinitionsCommand } from './commands/get-builddefinitions.command';
import { GetCiEntryPointSettingCommand } from './commands/get-cientrypointsetting.command';
import { MyBuildDefinition } from './models/myBuildDefinition';

@Component({
    templateUrl: './components/server.html'
})
export class Server {
    constructor(
        private getBuildDefinitionsCommand: GetBuildDefinitionsCommand,
        private getCiEntryPointSettingCommand: GetCiEntryPointSettingCommand,
        private route: ActivatedRoute,
        private router: Router
    ) {
        
    }

    private sub: any;

    ngOnInit() {
        this.sub = this.route.params.subscribe(params => {
            let id = +params['id'];
            if (id) {
                this.getCiEntryPointSettingCommand.invoke(id)
                    .then(ciEntryPointSetting => {
                        this.ciEntryPointSetting.url = ciEntryPointSetting.url;
                    });
            }
        });
    }

    ngOnDestroy() {
        this.sub.unsubscribe();
    }

    public ciEntryPointSetting = new CiEntryPointSetting();

    public projects: MyBuildDefinition[];

    public loadingProjects: boolean = false;

    public errorMessage: string = null;

    public getProjects() {
        this.loadingProjects = true;
        this.getBuildDefinitionsCommand.getBuildDefinitions(this.ciEntryPointSetting)
            .then(projects => {
                this.projects = projects;
            }, ex => {
                this.errorMessage = ex;
            })
            .then(() => {
                this.loadingProjects = false;
            });
    }

    public onSave() {
        this.projects.filter(i => i.selected).forEach(project => {
            alert('you selected ' + project.name);
        });
    }
}