﻿import { Injectable, Output, EventEmitter } from '@angular/core';
import { BaseCommand } from "../commands/base.command";
import { CiEntryPointSetting } from '../models/ciEntryPointSetting';

@Injectable()
export class ServerService {
    private ws;
    private commands: BaseCommand[] = [];

    constructor() {
        this.isConnected = false;
        if ("WebSocket" in window) {
            this.connectToServer();
        } else {
            this.connectionError.emit("This browser does not support websockets");
        }
    }

    registerCommand(command: BaseCommand) {
        this.commands.push(command);
    }

    private connectToServer() {
        var wsUrl = this.getUrl();
        var connection = new WebSocket(wsUrl, ["echo"]);
        connection.onopen = () => {
            this.isConnected = true;
            this.ws = connection;
            this.ws.onmessage = (e) => {
                let data = JSON.parse(e.data);

                this.commands.forEach((command : BaseCommand) => {
                    if (command.type === data.type) {
                        command.response(data);
                        return;
                    }
                });

                if (data.type === 'deviceConnectionChanged') {
                    if (data.responseCode === 200) {
                        this.deviceConnectionChanged.emit(data.result);
                    } else {
                        console.error(data.result);
                    }
                }

                if (data.type === 'alertResult') {
                    if (data.responseCode === 200) {
                        alert(data.result);
                    } else {
                        console.error(data.result);
                    }
                }
            };

            this.connected.emit(null);
        };
        connection.onerror = () => {
            this.isConnected = false;
            this.connectionError.emit("Error connecting via websockets.");
        };
        connection.onclose = () => {
            this.isConnected = false;
            this.connectionError.emit("Lost connection to server, attempting to reconnect.");
            this.connectToServer();
        };
    }

    @Output() public connected: EventEmitter<any> = new EventEmitter<any>(true);
    @Output() public connectionError: EventEmitter<string> = new EventEmitter<string>(true);
    @Output() public deviceConnectionChanged: EventEmitter<boolean> = new EventEmitter<any>(true);
    @Output() public refreshCiEntryPoints: EventEmitter<any> = new EventEmitter<any>(true);

    public isConnected: boolean;

    private getBaseUrl(location) {
        if (location.port) {
            // we're hitting the admin portal using a port (probably 8080) if we're in dev
            //    in that case connect to the back end on port 8001 (as specified in SirenOfShame.Uwp.TestServer\Services\WebServer.cs)
            let testServerPort = "8001";
            return location.hostname + ":" + testServerPort;
        }
        return location.hostname;
    }

    private getUrl() {
        let baseUrl = this.getBaseUrl(location);
        return "ws://" + baseUrl + "/sockets/";
    }

    public send(sendRequest, err?: any) {
        if (this.ws && this.isConnected) {
            this.ws.send(JSON.stringify(sendRequest));
        } else {
            var subscription = this.connected.subscribe(() => {
                this.ws.send(JSON.stringify(sendRequest));
                subscription.unsubscribe();
            });
        }
    }
}