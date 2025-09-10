using AwingTest.Server.Entities;
using AwingTest.Server.Utility;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace AwingTest.Server.Handler
{
    public class TreasureMapHandler
    {
        private readonly ILogger<TreasureMapHandler> _logger;
        private readonly AwingTestDbContext _dbContext;

        public TreasureMapHandler(
            ILogger<TreasureMapHandler> logger,
            AwingTestDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<(List<string>? ListError, int? id)> Add(treasure_map map)
        {
            #region Validate
            List<string> ListError = Validate(map);
            if (ListError.Count > 0)
            {
                return (ListError, null);
            }
            #endregion

            try
            {
                map.id = null; // Ensure id is null for new entry
                List<List<int>> map_offset = OffsetMap(CastMapToNonNull(map.map_client_input), -1);
                map.map_offset = map_offset;
                map.map_offset_str = JsonSerializer.Serialize(map.map_offset);

                var path = SolvePath(map);
                map.min_total_distance = path.min_total_distance;
                map.path_x_offset_str = JsonSerializer.Serialize(path.path_x_offset);
                map.path_y_offset_str = JsonSerializer.Serialize(path.path_y_offset);

                _dbContext.TreasureMaps.Add(map);
                await _dbContext.SaveChangesAsync();
                return (null, map.id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding treasure map");
                ListError.Add("Có lỗi xảy ra trong quá trình thêm bản đồ kho báu. Vui lòng thử lại.");
            }

            return (ListError, null);
        }

        public async Task<(List<string>? ListError, treasure_map? map)> SetupDisplay(int id)
        {
            treasure_map? map = await _dbContext.TreasureMaps.FindAsync(id);
            if (map == null)
            {
                return (new List<string> { "Bản đồ kho báu không tồn tại." }, null);
            }
            map.map_offset = JsonSerializer.Deserialize<List<List<int>>>(map.map_offset_str);
            map.map_client_input = CastMapToNullable(OffsetMap(map.map_offset, 1));
            map.path_x_offset = JsonSerializer.Deserialize<List<int>>(map.path_x_offset_str);
            map.path_y_offset = JsonSerializer.Deserialize<List<int>>(map.path_y_offset_str);
            map.path_x = OffsetPath(map.path_x_offset, 1);
            map.path_y = OffsetPath(map.path_y_offset, 1);

            map.map_offset = null;
            map.map_offset_str = null;
            map.path_x_offset = null;
            map.path_y_offset = null;
            map.path_x_offset_str = null;
            map.path_y_offset_str = null;

            return (null, map);
        }

        public async Task<(List<string>? ListError, treasure_map? map)> SetupEdit(int id)
        {
            treasure_map? map = await _dbContext.TreasureMaps.FindAsync(id);
            if (map == null)
            {
                return (new List<string> { "Bản đồ kho báu không tồn tại." }, null);
            }
            map.map_offset = JsonSerializer.Deserialize<List<List<int>>>(map.map_offset_str);
            map.map_client_input = CastMapToNullable(OffsetMap(map.map_offset, 1));
            //map.path_x_offset = JsonSerializer.Deserialize<List<int>>(map.path_x_offset_str);
            //map.path_y_offset = JsonSerializer.Deserialize<List<int>>(map.path_y_offset_str);
            //map.path_x = OffsetPath(map.path_x_offset, 1);
            //map.path_y = OffsetPath(map.path_y_offset, 1);

            map.map_offset = null;
            map.map_offset_str = null;
            map.path_x_offset = null;
            map.path_y_offset = null;
            map.path_x_offset_str = null;
            map.path_y_offset_str = null;

            return (null, map);
        }

        public async Task<(List<string>? ListError, object? nullObj)> Edit(treasure_map map)
        {
            #region Validate
            if (map.id == null || !await _dbContext.TreasureMaps.AnyAsync(m => m.id == map.id))
            {
                return (new List<string> { "Bản đồ kho báu không tồn tại." }, null);
            }

            List<string> ListError = Validate(map);
            if (ListError.Count > 0)
            {
                return (ListError, null);
            }
            #endregion

            try
            {
                List<List<int>> map_offset = OffsetMap(CastMapToNonNull(map.map_client_input), -1);
                map.map_offset = map_offset;
                map.map_offset_str = JsonSerializer.Serialize(map.map_offset);

                var path = SolvePath(map);
                map.min_total_distance = path.min_total_distance;
                map.path_x_offset_str = JsonSerializer.Serialize(path.path_x_offset);
                map.path_y_offset_str = JsonSerializer.Serialize(path.path_y_offset);

                _dbContext.TreasureMaps.Update(map);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ListError.Add("Có lỗi xảy ra trong quá trình cập nhật bản đồ kho báu. Vui lòng thử lại.");
            }

            return (ListError, null);
        }

        /// <summary>
        /// Dùng cho map nhập từ Client (chưa offset, gốc tọa độ (1,1))
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public List<string> Validate(treasure_map map)
        {
            List<string> ListError = new List<string>();
            bool isRowsValid = (map.rows != null && 1 <= map.rows && map.rows <= 500);
            if (!isRowsValid)
            {
                ListError.Add("Số hàng (n) phải nằm trong khoảng từ 1 đến 500.");
            }
            bool isColumnsValid = (map.columns != null && 1 <= map.columns && map.columns <= 500);
            if (!isColumnsValid)
            {
                ListError.Add("Số cột (m) phải nằm trong khoảng từ 1 đến 500.");
            }

            if (!isRowsValid || !isColumnsValid)
                return ListError;


            int max_chest_types = map.rows.Value * map.columns.Value;
            bool isChestTypesValid = (map.chest_types != null && 1 <= map.chest_types && map.chest_types <= max_chest_types);
            if (!isChestTypesValid)
            {
                ListError.Add($"Số loại hòm kho báu (p) phải nằm trong khoảng từ 1 đến {max_chest_types}.");
                return ListError;
            }


            bool isMapValid = (map.map_client_input != null && map.map_client_input.Count == map.rows
                && map.map_client_input.All(row => row != null && row.Count == map.columns));
            if (!isMapValid)
            {
                ListError.Add("Có lỗi dữ liệu. Vui lòng thử lại.");
                return ListError;
            }


            bool mapHasNullValue = false;
            bool mapHasInvalidValue = false;
            for (int i = 0; i < map.map_client_input.Count; i++)
            {
                for (int j = 0; j < map.map_client_input[i].Count; j++)
                {
                    mapHasNullValue = mapHasNullValue || (map.map_client_input[i][j] == null);
                    if (map.map_client_input[i][j] != null)
                        mapHasInvalidValue = mapHasInvalidValue || (map.map_client_input[i][j] < 1 || map.map_client_input[i][j] > map.chest_types);
                }
            }
            if (mapHasNullValue)
            {
                ListError.Add("Vui lòng điền đầy đủ thông tin bản đồ kho báu.");
            }
            if (mapHasInvalidValue)
            {
                ListError.Add($"Bản đồ kho báu chỉ có thể chứa rương có giá trị từ 1 đến {map.chest_types}.");
            }
            if (mapHasNullValue || mapHasInvalidValue)
            {
                return ListError;
            }

            List<List<int>> map_offset = CastMapToNonNull(map.map_client_input);
            map_offset = OffsetMap(map_offset, -1);

            List<int> chestPerType = CountChestPerType(map_offset, map.chest_types.Value);
            bool hasPathToTreasure = true;
            for (int i = 0; i < chestPerType.Count - 1; i++)
            {
                hasPathToTreasure = hasPathToTreasure && (chestPerType[i] > 0);
            }
            if (!hasPathToTreasure)
                ListError.Add($"Bản đồ không có đường dẫn đến kho báu.");
            if (chestPerType[chestPerType.Count - 1] != 1)
                ListError.Add($"Bản đồ bắt buộc phải có duy nhất 1 hòm kho báu (1 ô có giá trị {map.chest_types})");
            return ListError;
        }

        /// <summary>
        /// Sử dụng cho map đã offset về gốc tọa độ (0,0)
        /// </summary>
        /// <param name="mapOffset"></param>
        /// <param name="chest_types"></param>
        /// <returns></returns>
        public List<int> CountChestPerType(List<List<int>> mapOffset, int chest_types)
        {
            List<int> chestPerType = new List<int>();
            CollectionsMarshal.SetCount(chestPerType, chest_types);
            for (int i = 0; i < chestPerType.Count; i++)
            {
                chestPerType[i] = 0;
            }

            for (int i = 0; i < mapOffset.Count; i++)
            {
                for (int j = 0; j < mapOffset[i].Count; j++)
                {
                    chestPerType[mapOffset[i][j]]++;
                }
            }

            return chestPerType;
        }

        public (List<int> path_x_offset, List<int> path_y_offset, double min_total_distance) SolvePath(treasure_map map)
        {
            List<int> numberOfChestsPerType = CountChestPerType(map.map_offset, map.chest_types.Value);

            // array[chestType][chestIndex]
            List<List<int>> x_coordinate = new List<List<int>>();
            List<List<int>> y_coordinate = new List<List<int>>();
            List<List<double>> min_distance = new List<List<double>>();

            List<int> path_x_offset = new List<int>();
            List<int> path_y_offset = new List<int>();

            #region Initialize Lists
            CollectionsMarshal.SetCount(x_coordinate, numberOfChestsPerType.Count);
            CollectionsMarshal.SetCount(y_coordinate, numberOfChestsPerType.Count);
            CollectionsMarshal.SetCount(min_distance, numberOfChestsPerType.Count);
            CollectionsMarshal.SetCount(path_x_offset, numberOfChestsPerType.Count);
            CollectionsMarshal.SetCount(path_y_offset, numberOfChestsPerType.Count);

            for (int i = 0; i < numberOfChestsPerType.Count; i++)
            {
                x_coordinate[i] = new List<int>();
                y_coordinate[i] = new List<int>();
                min_distance[i] = new List<double>();
                x_coordinate[i].Capacity = numberOfChestsPerType[i];
                y_coordinate[i].Capacity = numberOfChestsPerType[i];
                CollectionsMarshal.SetCount(min_distance[i], numberOfChestsPerType[i]);
                for (int j = 0; j < numberOfChestsPerType[i]; j++)
                {
                    min_distance[i][j] = double.MaxValue;
                }
            }

            for (int y = 0; y < map.map_offset.Count; y++)
            {
                for (int x = 0; x < map.map_offset[y].Count; x++)
                {
                    int chestType = map.map_offset[y][x];
                    x_coordinate[chestType].Add(x);
                    y_coordinate[chestType].Add(y);
                }
            }
            #endregion

            // Calculate distance from ship initial position (0,0) to all chests of type 0
            for (int chestType0 = 0; chestType0 < min_distance[0].Count; chestType0++)
            {
                min_distance[0][chestType0] = CalculateDistance(0, 0,
                    x_coordinate[0][chestType0], y_coordinate[0][chestType0]);
            }

            // Calculate distance from chests of type i to all chests of type i+1
            for (int chestType = 0; chestType < numberOfChestsPerType.Count - 1; chestType++)
            {
                int outerLoop = chestType;
                if (chestType == 1)
                {
                    int abc = 1;
                }
                for (int current_layer_chest = 0; current_layer_chest < x_coordinate[chestType].Count; current_layer_chest++)
                {
                    for (int next_layer_chest = 0; next_layer_chest < x_coordinate[chestType + 1].Count; next_layer_chest++)
                    {
                        int x_from = x_coordinate[chestType][current_layer_chest];
                        int y_from = y_coordinate[chestType][current_layer_chest];
                        int x_to = x_coordinate[chestType+1][next_layer_chest];
                        int y_to = y_coordinate[chestType+1][next_layer_chest];
                        double distance = CalculateDistance(x_from, y_from, x_to, y_to);
                        double total_distance = min_distance[chestType][current_layer_chest] + distance;
                        double min_distance_next_layer_chest = min_distance[chestType + 1][next_layer_chest];
                        if (total_distance < min_distance_next_layer_chest)
                        {
                            min_distance[chestType + 1][next_layer_chest] = total_distance;
                        }
                    }
                }
                int endLoop = 0;
            }

            List<double> min_distance_back = min_distance[min_distance.Count - 1];
            double min_total_distance = min_distance_back[min_distance_back.Count-1];

            // Trace backward from the destination to the starting point for the shortest path
            List<int> x_coordinate_back = x_coordinate[x_coordinate.Count - 1];
            List<int> y_coordinate_back = y_coordinate[y_coordinate.Count - 1];
            path_x_offset[path_x_offset.Count - 1] = x_coordinate_back[x_coordinate_back.Count - 1];
            path_y_offset[path_y_offset.Count - 1] = y_coordinate_back[y_coordinate_back.Count - 1];

            double min_distance_current = min_total_distance;

            for (int chestType = numberOfChestsPerType.Count - 2; chestType >= 0; chestType--)
            {
                List<int> x_coordinate_current = x_coordinate[chestType];
                List<int> y_coordinate_current = y_coordinate[chestType];
                List<double> min_distance_current_layer = min_distance[chestType];
                int x_next = path_x_offset[chestType + 1];
                int y_next = path_y_offset[chestType + 1];
                for (int current_layer_chest = 0; current_layer_chest < x_coordinate_current.Count; current_layer_chest++)
                {
                    double distance = CalculateDistance(
                        x_coordinate_current[current_layer_chest], y_coordinate_current[current_layer_chest],
                        x_next, y_next);
                    double total_distance = min_distance_current_layer[current_layer_chest] + distance;
                    if (Utility.Utility.ApproxEqual(total_distance, min_distance_current))
                    {
                        path_x_offset[chestType] = x_coordinate_current[current_layer_chest];
                        path_y_offset[chestType] = y_coordinate_current[current_layer_chest];
                        min_distance_current = min_distance_current_layer[current_layer_chest];
                        break;
                    }
                }
            }

            return (path_x_offset, path_y_offset, min_total_distance);
        }

        public double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Lùi gốc chest type của bản đồ từ 1 về 0: offsetValue = -1
        /// Lùi gốc chest type của bản đồ từ 0 về 1: offsetValue = 11
        /// </summary>
        /// <param name="map"></param>
        /// <param name="offsetValue"></param>
        /// <returns></returns>
        public List<List<int>> OffsetMap(List<List<int>> map, int offsetValue)
        {
            List<List<int>> offsetMap = new List<List<int>>();
            CollectionsMarshal.SetCount(offsetMap, map.Count);
            for (int i = 0; i < offsetMap.Count; i++)
            {
                offsetMap[i] = new List<int>();
                CollectionsMarshal.SetCount(offsetMap[i], map[i].Count);
                for (int j = 0; j < offsetMap[i].Count; j++)
                {
                        offsetMap[i][j] = map[i][j] + offsetValue;
                }
            }
            return offsetMap;
        }

        public List<int> OffsetPath(List<int> path, int offsetValue)
        {
            List<int> offsetPath = new List<int>();
            CollectionsMarshal.SetCount(offsetPath, path.Count);
            for (int i = 0; i < offsetPath.Count; i++)
            {
                offsetPath[i] = path[i] + offsetValue;
            }
            return offsetPath;
        }

        public List<List<int>> CastMapToNonNull(List<List<int?>> map)
        {
            List<List<int>> result = new();
            CollectionsMarshal.SetCount(result, map.Count);
            for (int i = 0; i < result.Count; ++i)
            {
                result[i] = map[i].Cast<int>().ToList();
            }
            return result;
        }

        public List<List<int?>> CastMapToNullable(List<List<int>> map)
        {
            List<List<int?>> result = new();
            CollectionsMarshal.SetCount(result, map.Count);
            for (int i = 0; i < result.Count; ++i)
            {
                result[i] = map[i].Cast<int?>().ToList();
            }
            return result;
        }

        public async Task<Dictionary<string, bool>> Test()
        {
            Dictionary<string, bool> testResults = new Dictionary<string, bool>();

            #region Test Validate
            treasure_map invalidRowsColumns = new treasure_map
            {
                rows = -1,
                columns = null,
            };
            testResults["Invalid Rows and Columns"] = (Validate(invalidRowsColumns).Count == 2);

            treasure_map invalidChestTypes = new treasure_map
            {
                rows = 5,
                columns = 5,
                chest_types = 26,
            };
            testResults["Invalid Chest Types"] = (Validate(invalidChestTypes).Count == 1);

            treasure_map invalidMapSize = new treasure_map
            {
                rows = 5,
                columns = 5,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 1, 1, 2, 1, 1 },
                    new List<int?> { 1, 2, 1, 1, 2 },
                    new List<int?> { 2, 1, 1, 2, 0 },
                    new List<int?> { 1, 1, 2, 0 },
                    new List<int?> { 1, 2, 1, 1, 2 }
                }
            };
            testResults["Invalid Map Size"] = (Validate(invalidMapSize).Count == 1);

            treasure_map mapHasNullValue = new treasure_map
            {
                rows = 5,
                columns = 5,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 3, 1, 2, 1, 1 },
                    new List<int?> { 1, null, 1, 1, 2 },
                    new List<int?> { 2, 1, 1, 2, 1 },
                    new List<int?> { 1, 1, 2, 1, 1 },
                    new List<int?> { 1, 2, 1, 1, 2 }
                }
            };
            testResults["Map Has Null Value"] = (Validate(mapHasNullValue).Count == 1);

            treasure_map mapHasInvalidValue = new treasure_map
            {
                rows = 5,
                columns = 5,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 1, 1, 2, 1, 1 },
                    new List<int?> { 1, 4, 1, 1, 2 },
                    new List<int?> { 2, 1, 1, 2, 0 },
                    new List<int?> { 1, 1, 2, 1, 1 },
                    new List<int?> { 1, 2, 1, 1, 2 }
                }
            };
            testResults["Map Has Invalid Value"] = (Validate(mapHasInvalidValue).Count == 1);

            treasure_map mapNoPathToTreasure = new treasure_map
            {
                rows = 5,
                columns = 5,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 1, 1, 1, 1, 1 },
                    new List<int?> { 1, 1, 1, 1, 1 },
                    new List<int?> { 1, 1, 1, 1, 1 },
                    new List<int?> { 1, 1, 1, 1, 1 },
                    new List<int?> { 1, 1, 1, 1, 3 }
                }
            };
            testResults["Map No Path To Treasure"] = (Validate(mapNoPathToTreasure).Count == 1);
            treasure_map mapMultipleTreasures = new treasure_map
            {
                rows = 5,
                columns = 5,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 1, 2, 3, 3, 3 },
                    new List<int?> { 3, 3, 3, 3, 3 },
                    new List<int?> { 3, 3, 3, 3, 3 },
                    new List<int?> { 3, 3, 3, 3, 3 },
                    new List<int?> { 3, 3, 3, 3, 3 },
                }
            };
            testResults["Map Multiple Treasures"] = (Validate(mapMultipleTreasures).Count == 1);
            treasure_map validMap = new treasure_map
            {
                rows = 3,
                columns = 3,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 3, 2, 2 },
                    new List<int?> { 2, 2, 2 },
                    new List<int?> { 2, 2, 1 }
                }
            };
            testResults["Valid Map"] = (Validate(validMap).Count == 0);

            #endregion
            #region Test SolvePath
            treasure_map solvePath1 = new treasure_map
            {
                rows = 3,
                columns = 3,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 3, 2, 2 },
                    new List<int?> { 2, 2, 2 },
                    new List<int?> { 2, 2, 1 }
                }
            };
            solvePath1.map_offset = OffsetMap(CastMapToNonNull(solvePath1.map_client_input), -1);
            var path1 = SolvePath(solvePath1);
            testResults["Solve Path 1 - min_total_distance"] = Utility.Utility.ApproxEqual(path1.min_total_distance, 4 * MathF.Sqrt(2));
            testResults["Solve Path 1 - path_x_offset"] = path1.path_x_offset.SequenceEqual(new List<int> { 2, 1, 0 });
            testResults["Solve Path 1 - path_y_offset"] = path1.path_y_offset.SequenceEqual(new List<int> { 2, 1, 0 });


            treasure_map solvePath2 = new treasure_map
            {
                rows = 3,
                columns = 4,
                chest_types = 3,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 2, 1, 1, 1 },
                    new List<int?> { 1, 1, 1, 1 },
                    new List<int?> { 2, 1, 1, 3 },
                }
            };
            solvePath2.map_offset = OffsetMap(CastMapToNonNull(solvePath2.map_client_input), -1);
            var path2 = SolvePath(solvePath2);
            testResults["Solve Path 2 - min_total_distance"] = path2.min_total_distance == 5;
            testResults["Solve Path 2 - path_x_offset"] = path2.path_x_offset.SequenceEqual(new List<int> { 0, 0, 3 });
            testResults["Solve Path 2 - path_y_offset"] = path2.path_y_offset.SequenceEqual(new List<int> { 1, 2, 2 });


            treasure_map solvePath3 = new treasure_map
            {
                rows = 3,
                columns = 4,
                chest_types = 12,
                map_client_input = new List<List<int?>> {
                    new List<int?> { 1, 2, 3, 4 },
                    new List<int?> { 8, 7, 6, 5 },
                    new List<int?> { 9, 10, 11, 12 },
                }
            };
            solvePath3.map_offset = OffsetMap(CastMapToNonNull(solvePath3.map_client_input), -1);
            var path3 = SolvePath(solvePath3);
            testResults["Solve Path 3 - min_total_distance"] = path3.min_total_distance == 11;
            testResults["Solve Path 3 - path_x_offset"] = path3.path_x_offset.SequenceEqual(new List<int> { 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3 });
            testResults["Solve Path 3 - path_y_offset"] = path3.path_y_offset.SequenceEqual(new List<int> { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2 });

            #endregion


            return testResults;
        }
    }
}