import type { treasure_map } from "../entity/treasure_map";

export class TreasureMapRequest {
    id?: number;
    map?: treasure_map;
    listError?: string[];

    public constructor(obj?: Partial<TreasureMapRequest>) {
        Object.assign(this, obj);
    }

}
