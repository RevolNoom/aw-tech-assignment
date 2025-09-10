import { Box, Button, Grid, Stack } from '@mui/material';
import { useEffect, useState } from 'react';
import { AwingNumberField } from '../component/AwingNumberField';
import { Utility } from '../utility/Utility';
import { AwingUrl } from '../utility/Url';
import { TreasureMapRequest } from "../request/TreasureMapRequest";
import { useNavigate, useParams } from 'react-router';
import type { treasure_map } from '../entity/treasure_map';

export function TreasureMapDisplay() {
    const params = useParams();
    const id = Number(params.id);
    const [readyToDisplay, setReadyToDisplay] = useState<boolean>(false);

    const navigate = useNavigate();

    const [map, set_map] = useState<treasure_map | undefined>(undefined);
    const [pathStep, setPathStep] = useState<number>(0);

    useEffect(() => {
        const request = new TreasureMapRequest({
            id: id,
        });
        Utility
            .post(AwingUrl.treasureMapSetupDisplay, request)
            .then((response) => {
                if (response.listError && response.listError.length > 0) {
                    alert('Lỗi: ' + response.listError.join(', '));
                    return;
                }

                set_map(response.map!);
                setReadyToDisplay(true);
            })
            .catch((e) => {
                alert('Lỗi khi tải dữ liệu: ' + e);
                setReadyToDisplay(false);
            });
    }, []);

    async function toEdit() {
        navigate('/treasure_map/edit/' + id);
    }

    async function toAdd() {
        navigate('/treasure_map/add/');
    }

    async function stepForward() {
        setPathStep(pathStep + 1);
    }

    async function stepBackward() {
        setPathStep(pathStep - 1);
    }

    if (readyToDisplay !== true)
        return <div></div>

    const _map = map!;
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
                <h1>Lời giải bản đồ</h1>
                <h2>Thông tin bản đồ</h2>
                <Stack spacing={2} direction="row" marginBottom={5}>
                        <AwingNumberField
                        title="Số hàng (n)"
                        disabled
                        value={_map.rows}
                        />
                        <AwingNumberField
                        title="Số cột (m)"
                        disabled
                        value={_map.columns}
                        />
                        <AwingNumberField
                            title="Số loại rương (p)"
                        disabled
                            value={_map.chest_types}
                        />
                    </Stack>
                <h2>Bản đồ rương kho báu:</h2>
                <Grid container
                    rowSpacing={0}
                    columnSpacing={0}
                    columns={_map.columns ?? 1}
                >
                    {_map.map_client_input!.flat().map((value, index) => {
                        let x = index % _map.columns! + 1;
                        let y = Math.floor(index / _map.columns!) + 1;

                        let color = "";

                        if (_map.path_x![pathStep] === x && _map.path_y![pathStep] === y) {
                            color = "green";
                        } else {
                            for (let i = 0; i < pathStep; ++i) {
                                if (_map.path_x![i] === x && _map.path_y![i] === y) {
                                    color = "darkblue";
                                    break;
                                } 
                            }
                        }
                        return (
                            <Grid key={index} size={1}>
                                <AwingNumberField
                                    disabled
                                    value={value}
                                    //styleRoot={{ width: `calc(100%/${columns})` }}
                                    styleInput={{ width: `100%`, backgroundColor: color }}
                                />
                            </Grid>
                        );
                    })}
                </Grid>
                <h2>Lời giải</h2>
                <div>Lượng nhiên liệu nhỏ nhất cần có để lấy được rương kho báu: {_map.min_total_distance}</div>
                <div>Đường đi: {_map.path_x?.map((_, idx) => {
                        const x = _map.path_x![idx];
                        const y = _map.path_y![idx];
                        let color = "inherit";
                        if (idx === pathStep) {
                            color = "green";
                        }
                        else if (idx < pathStep) {
                            color = "darkblue"
                        }

                        return <text style={{ color: color }}>({x}, {y}) </text>
                })}</div>

                <Stack direction="row" spacing={1}>
                    <Button
                        variant="text"
                        onClick={stepBackward}
                        disabled={pathStep === 0}
                    >
                        Lùi
                    </Button>
                    <Button
                        variant="text"
                        onClick={stepForward}
                        disabled={pathStep === _map.path_x!.length-1}
                    >
                        Tiến
                    </Button>
                </Stack>

                <Stack direction="row" spacing={1 }>
                    <Button
                        variant="outlined"
                        onClick={toEdit}
                    >
                        Chỉnh sửa bản đồ
                    </Button>
                    <Button
                        variant="text"
                        onClick={toAdd}
                    >
                        Thoát
                    </Button>
                </Stack>
            </Stack>

            </Box>
    );
}