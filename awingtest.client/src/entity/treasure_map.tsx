export class treasure_map {
    id?: number;
    rows?: number;
    columns?: number;
    chest_types?: number;
    min_total_distance?: number;
    map_client_input?: (number | null)[][];
    path_x?: number[];
    path_y?: number[];
}