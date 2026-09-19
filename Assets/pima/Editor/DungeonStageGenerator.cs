using UnityEditor;
using UnityEngine;

public static class DungeonStageGenerator
{
    private static readonly string[] StageMap = new string[]
    {
        "...........",
        ".3#..G..#L.",
        ".##..B..##.",
        "..B..#..B..",
        ".###.#.###.",
        ".#CCC#CCC#.",
        ".###.#.###.",
        "..B..#..B..",
        ".###.#.#I#.",
        ".1##.S.#I2.",
        "..........."
    };

    [MenuItem("Tools/Generate Dungeon Stage")]
    public static void Generate()
    {
        var existing = GameObject.Find("Stage_Root");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        var root = new GameObject("Stage_Root");
        root.transform.position = Vector3.zero;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        
        Material matFloor = CreateMaterial(shader, new Color(0.35f, 0.35f, 0.4f), "Mat_Floor");
        Material matWall = CreateMaterial(shader, new Color(0.2f, 0.2f, 0.25f), "Mat_RockWall");
        Material matStart = CreateMaterial(shader, new Color(0.2f, 0.8f, 0.3f), "Mat_Start");
        Material matGoal = CreateMaterial(shader, new Color(0.95f, 0.8f, 0.2f), "Mat_Goal");

        // 1. 床ブロックの生成
        for (int row = 0; row < StageMap.Length; row++)
        {
            string line = StageMap[row];
            for (int col = 0; col < line.Length; col++)
            {
                char tileType = line[col];
                Vector3 pos = new Vector3(col - 5f, 0f, 5f - row);

                if (tileType == '.') continue;

                if (tileType == 'B')
                {
                    var bridgePoint = new GameObject($"BridgePoint_{row}_{col}");
                    bridgePoint.transform.SetParent(root.transform, false);
                    bridgePoint.transform.localPosition = pos;
                    continue;
                }

                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.SetParent(root.transform, false);
                tile.transform.localPosition = pos;
                tile.transform.localScale = new Vector3(1f, 0.2f, 1f);

                var renderer = tile.GetComponent<Renderer>();

                switch (tileType)
                {
                    case 'S':
                        tile.name = "Point_Start";
                        renderer.sharedMaterial = matStart;
                        break;
                    case 'G':
                        tile.name = "Point_Goal";
                        renderer.sharedMaterial = matGoal;
                        break;
                    case '1': tile.name = "Spawn_Mini1"; renderer.sharedMaterial = matFloor; break;
                    case '2': tile.name = "Spawn_Mini2"; renderer.sharedMaterial = matFloor; break;
                    case '3': tile.name = "Spawn_Mini3"; renderer.sharedMaterial = matFloor; break;
                    case 'C': tile.name = "Floor_Conveyor"; renderer.sharedMaterial = matFloor; break;
                    case 'I': tile.name = "Hazard_IronBall"; renderer.sharedMaterial = matFloor; break;
                    case 'L': tile.name = "Point_Lever"; renderer.sharedMaterial = matFloor; break;
                    case '#':
                    default:
                        tile.name = $"Floor_{row}_{col}";
                        renderer.sharedMaterial = matFloor;
                        break;
                }
            }
        }

        // 2. 外周岩壁の生成（高さ4m）
        var wallGroup = new GameObject("Walls_Outer");
        wallGroup.transform.SetParent(root.transform, false);

        float wallHeight = 4.0f;
        float wallYPos = wallHeight / 2.0f;

        for (int r = 0; r <= 10; r++)
        {
            for (int c = 0; c <= 10; c++)
            {
                bool isOuterEdge = (r == 0 || r == 10 || c == 0 || c == 10);
                if (!isOuterEdge) continue;

                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = $"Wall_{r}_{c}";
                wall.transform.SetParent(wallGroup.transform, false);
                wall.transform.localPosition = new Vector3(c - 5f, wallYPos, 5f - r);
                wall.transform.localScale = new Vector3(1f, wallHeight, 1f);

                var renderer = wall.GetComponent<Renderer>();
                renderer.sharedMaterial = matWall;
            }
        }

        // 3. カメラの背景を漆黒（Solid Color）に設定
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = Color.black;
            EditorUtility.SetDirty(mainCam);
        }

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("ダンジョンステージ生成＆背景の黒色設定が完了しました！");
    }

    private static Material CreateMaterial(Shader shader, Color color, string name)
    {
        Material mat = new Material(shader) { name = name, color = color };
        return mat;
    }
}