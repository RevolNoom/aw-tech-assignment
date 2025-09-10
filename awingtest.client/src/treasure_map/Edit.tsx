import { Box, Button, Grid, Stack } from '@mui/material';
import { useEffect, useState } from 'react';
import { AwingNumberField } from '../component/AwingNumberField';
import { Utility } from '../utility/Utility';
import { AwingUrl } from '../utility/Url';
import { TreasureMapRequest } from "../request/TreasureMapRequest";
import { useNavigate, useParams } from 'react-router';

export function TreasureMapEdit() {
    const params = useParams();
    const id = Number(params.id);
    const [readyToDisplay, setReadyToDisplay] = useState<boolean>(false);

    const navigate = useNavigate();

    const [rows, set_rows] = useState<number | null>(1);
    const [columns, set_columns] = useState<number|null>(1);
    const [chest_types, set_chest_types] = useState<number|null>(1);
    const [map_client_input, set_map_client_input] = useState<(number | null)[][]>([[1]]);

    useEffect(() => {
        const request = new TreasureMapRequest({
            id: id,
        });
        Utility
            .post(AwingUrl.treasureMapSetupEdit, request)
            .then((response) => {
                if (response.listError && response.listError.length > 0) {
                    alert('Lỗi: ' + response.listError.join(', '));
                    return;
                }

                const map = response.map!;
                set_rows(map.rows!);
                set_columns(map.columns!);
                set_chest_types(map.chest_types!);
                set_map_client_input(map.map_client_input!);

                setReadyToDisplay(true);
            })
            .catch((e) => {
                alert('Lỗi khi tải dữ liệu: ' + e);
                setReadyToDisplay(false);
            });
    }, []);

    function on_rows_change(value: number | null) {
        set_rows(value);
        expandMap(value, columns);
    }
    function on_columns_change(value: number | null) {
        set_columns(value);
        expandMap(rows, value);
    }

    function expandMap(new_rows: number | null, new_columns: number | null) {
        if (!new_rows || !new_columns) {
            set_map_client_input([[1]]);
            return;
        }

        const new_map_client_input: (number | null)[][] = [];
        new_map_client_input.length = new_rows;
        new_map_client_input.fill([]);

        const newRow: (number | null)[] = [];
        newRow.length = new_columns;
        newRow.fill(1);

        for (let r = 0; r < new_rows; r++) {
            new_map_client_input[r] = [...newRow]
        }

        for (let r = 0; r < Math.min(new_rows, map_client_input.length); r++) {
            for (let c = 0; c < Math.min(new_columns, map_client_input[r].length); c++) {
                new_map_client_input[r][c] = map_client_input[r][c];
            }
        }
        set_map_client_input(new_map_client_input);
    }

    async function saveData() {
        try {
            const request = new TreasureMapRequest({
                map: {
                    id: id,
                    rows: rows ?? undefined,
                    columns: columns ?? undefined,
                    chest_types: chest_types ?? undefined,
                    map_client_input: map_client_input,
                }
            });
            const response = await Utility.post(AwingUrl.treasureMapEdit, request);
            if (response.listError && response.listError.length > 0) {
                alert('Lỗi: ' + response.listError.join(', '));
                return;
            }
            navigate('/treasure_map/display/' + id);
        } catch (e) {
            alert('Lỗi khi lưu dữ liệu: ' + e);
        }
    }

    async function cancelEdit() {
        navigate('/treasure_map/display/' + id);
    }

    if (readyToDisplay !== true)
        return <div></div>

    const max_chest_types = (rows && columns) ? (rows * columns) : 1;
    return (
        <Box
                sx={{
                    width: '100vw',
                    height: '100vh',
                    display: 'flex',
                justifyContent: 'begin',
                    alignItems: 'begin',
                }}
        >
            <Stack spacing={2} direction="column" marginTop={3} marginLeft={5} marginRight={5}>
                <h1>Chỉnh sửa bản đồ</h1>
                <h2>Thông tin bản đồ</h2>
                <Stack spacing={2} direction="row" marginBottom={5}>
                        <AwingNumberField
                            title="Số hàng (n)"
                            min={1}
                            max={500}
                            required
                        value={rows}
                        onValueChange={on_rows_change}
                        />
                        <AwingNumberField
                            title="Số cột (m)"
                            min={1}
                            max={500}
                            required
                        value={columns}
                        onValueChange={on_columns_change}
                        />
                        <AwingNumberField
                            title="Số loại rương (p)"
                            min={1}
                            max={max_chest_types}
                            required
                            value={chest_types}
                            onValueChange={set_chest_types}
                        />
                    </Stack>
                <h2>Bản đồ rương kho báu:</h2>
                <Grid container
                    rowSpacing={0}
                    columnSpacing={0}
                    columns={columns ?? 1}
                >
                    {map_client_input.flat().map((value, index) => {
                        return (
                            <Grid key={index} size={1}>
                                <AwingNumberField
                                    title={undefined}
                                    min={1}
                                    max={chest_types || 1}
                                    required
                                    value={value}
                                    //styleRoot={{ width: `calc(100%/${columns})` }}
                                    styleInput={{ width: `100%` }}
                                    onValueChange={(new_value) => {
                                        const new_map_client_input = [...map_client_input];
                                        const r = Math.floor(index / (columns || 1));
                                        const c = index % (columns || 1);
                                        new_map_client_input[r][c] = new_value;
                                        set_map_client_input(new_map_client_input);
                                    }}
                                />
                            </Grid>
                        );
                    })}
                </Grid>
                <Stack direction="row" spacing={1 }>
                    <Button
                        variant="contained"
                        onClick={saveData}
                    >
                        Lưu thay đổi
                    </Button>
                    <Button
                        variant="outlined"
                        onClick={cancelEdit}
                    >
                        Hủy
                    </Button>
                </Stack>
            </Stack>

            </Box>
    );
}