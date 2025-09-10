drop table if exists treasure_map;
create table treasure_map(
	id serial primary key,
	rows int4,
	columns int4,
	chest_types int4,
	min_total_distance double precision,
	map_offset_str text,
	path_x_offset_str text,
	path_y_offset_str text
);