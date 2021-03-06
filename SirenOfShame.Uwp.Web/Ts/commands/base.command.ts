﻿import { ServerService } from "../services/server.service";

export abstract class BaseCommand {
    constructor(protected serverService: ServerService) {
    }

    get type(): string { return null; }

    abstract response(data);
}