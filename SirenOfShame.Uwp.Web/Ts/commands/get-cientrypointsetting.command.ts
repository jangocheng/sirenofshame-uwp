﻿import { Injectable } from '@angular/core';
import { BaseCommand } from './base.command';
import { ServerService } from '../services/server.service';
import { CiEntryPointSetting } from '../models/ciEntryPointSetting';

@Injectable()
export class GetCiEntryPointSettingCommand extends BaseCommand {
    constructor(protected serverService: ServerService) {
        super(serverService);

        serverService.registerCommand(this);
    }

    get type() {
        return "getCiEntryPointSetting";
    }

    public response(data) { }

    public invoke(id: number): Promise<CiEntryPointSetting> {
        return new Promise<CiEntryPointSetting>((resolve, err) => {
            this.response = (result) => {
                if (result.responseCode === 200) {
                    resolve(result.result);
                } else {
                    err(result.result);
                }
            };
            var sendRequest = {
                type: this.type,
                id: id
            }
            this.serverService.send(sendRequest, err);
        });
    }
}