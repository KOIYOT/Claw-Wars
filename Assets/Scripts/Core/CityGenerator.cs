using UnityEngine;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour
{
    [Header("Prefabs - Casas")]
    public GameObject[] houseVariants;

    [Header("Prefabs - Puntos especiales")]
    public GameObject startPointPrefab;
    public GameObject endPointPrefab;

    [Header("Configuración de variantes de calles")]
    public StreetVariant variantDatabase;

    [Header("Grid Settings")]
    public int width = 50;
    public int height = 50;

    [Header("Road Generation")]
    public int roadBuilders = 3;
    public int maxSteps = 120;
    public float turnChance = 0.4f;
    public float branchChance = 0.3f;
    public int minStreetCount = 30;

    [Header("Escala del mundo")]
    public float cellSize = 3f;

    private string[,] grid;
    private List<Vector2Int> streetPositions = new List<Vector2Int>();
    private Vector2Int startPos;
    private Vector2Int endPos;

    private enum CellType { Empty, Street, House }

    private class RoadWalker
    {
        public Vector2Int position;
        public Vector2Int direction;
        public int stepsLeft;
    }

    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    void Start()
    {
        GenerateCity();
    }

    [ContextMenu("Generar Ciudad")]
    public void GenerateCity()
    {
        int attempts = 0;
        const int maxAttempts = 5;

        do
        {
            ClearPrevious();
            grid = new string[width, height];
            GenerateStreets();
            attempts++;
        }
        while (CountStreets() < minStreetCount && attempts < maxAttempts);

        MarkStartAndEnd();
        InstantiateStreets();
        PlaceHouses();
    }

    void GenerateStreets()
    {
        CellType[,] cellMap = new CellType[width, height];
        List<RoadWalker> walkers = new List<RoadWalker>();
        streetPositions.Clear();

        for (int i = 0; i < roadBuilders; i++)
        {
            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            walkers.Add(new RoadWalker
            {
                position = new Vector2Int(width / 2, height / 2),
                direction = dir,
                stepsLeft = maxSteps
            });
        }

        while (walkers.Count > 0)
        {
            List<RoadWalker> newWalkers = new List<RoadWalker>();

            for (int i = walkers.Count - 1; i >= 0; i--)
            {
                var w = walkers[i];

                if (!InBounds(w.position)) { walkers.RemoveAt(i); continue; }
                if (!IsValidStreetPosition(w.position, cellMap)) { walkers.RemoveAt(i); continue; }

                cellMap[w.position.x, w.position.y] = CellType.Street;
                grid[w.position.x, w.position.y] = "street";
                streetPositions.Add(w.position);

                if (Random.value < turnChance)
                    w.direction = Turn(w.direction);

                w.position += w.direction;
                w.stepsLeft--;

                if (Random.value < branchChance)
                {
                    Vector2Int newDir = Turn(w.direction, Random.value > 0.5f);
                    newWalkers.Add(new RoadWalker
                    {
                        position = w.position,
                        direction = newDir,
                        stepsLeft = w.stepsLeft / 2
                    });
                }

                if (w.stepsLeft <= 0) walkers.RemoveAt(i);
            }

            walkers.AddRange(newWalkers);
        }
    }

    void MarkStartAndEnd()
    {
        const float minDistanceBetweenPoints = 20f;
        List<Vector2Int> borderStreets = new List<Vector2Int>();

        foreach (var pos in streetPositions)
        {
            if (pos.x <= 1 || pos.x >= width - 2 || pos.y <= 1 || pos.y >= height - 2)
                borderStreets.Add(pos);
        }

        Vector2Int bestA = Vector2Int.zero;
        Vector2Int bestB = Vector2Int.zero;
        float bestDist = 0f;

        List<Vector2Int> candidates = borderStreets.Count >= 2 ? borderStreets : streetPositions;

        for (int i = 0; i < candidates.Count; i++)
        {
            for (int j = i + 1; j < candidates.Count; j++)
            {
                float dist = Vector2.Distance(candidates[i], candidates[j]);
                if (dist > bestDist)
                {
                    bestDist = dist;
                    bestA = candidates[i];
                    bestB = candidates[j];
                }
            }
        }

        startPos = bestA;
        endPos = bestB;
        grid[startPos.x, startPos.y] = "start";
        grid[endPos.x, endPos.y] = "end";
        ForceAsDeadEnd(startPos);
        ForceAsDeadEnd(endPos);
    }

    void InstantiateStreets()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                string cell = grid[x, z];
                if (cell == null) continue;
                
                bool up = IsStreet(x, z + 1);
                bool down = IsStreet(x, z - 1);
                bool left = IsStreet(x - 1, z);
                bool right = IsStreet(x + 1, z);

                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);

                if (cell == "start" || cell == "end")
                {
                    int connected = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

                    if (connected == 1)
                    {
                        float rotation = 0f;
                        if (up) rotation = 0f;
                        else if (down) rotation = 180f;
                        else if (left) rotation = -90f;
                        else if (right) rotation = 90f;

                        GameObject prefab = (cell == "start") ? startPointPrefab : endPointPrefab;
                        Instantiate(prefab, position, Quaternion.Euler(0, rotation, 0), transform);
                    }
                    else
                    {
                        Debug.LogWarning($"[{cell}] en ({x},{z}) tiene {connected} conexiones. NO se colocó porque no es un callejón cerrado.");
                    }

                    continue;
                }


                if (cell != "street") continue;
                
                string streetID = GetStreetID(up, down, left, right);
                float rotY = GetStreetRotation(streetID, up, down, left, right);

                if (variantDatabase != null)
                {
                    GameObject prefab = variantDatabase.GetVariant_(streetID);
                    if (prefab != null)
                        Instantiate(prefab, position, Quaternion.Euler(0, rotY, 0), transform);
                }
            }
        }
    }

    float GetStreetRotation(string id, bool up, bool down, bool left, bool right)
    {
        switch (id)
        {
            case "deadEnd":
                if (up) return 0f;
                if (down) return 180f;
                if (left) return -90f;
                if (right) return 90f;
                break;
            case "tUp": return 0f;
            case "tDown": return 180f;
            case "tLeft": return -90f;
            case "tRight": return 90f;
            case "cornerTL": return 90f;
            case "cornerTR": return 180f;
            case "cornerBR": return -90f;
            case "cornerBL": return 0f;
            case "horizontal": return 90f;
            case "vertical": return 0f;
        }
        return 0f;
    }
    
    void ForceAsDeadEnd(Vector2Int pos)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        int connected = 0;
        Vector2Int keep = Vector2Int.zero;

        foreach (var dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (InBounds(neighbor.x, neighbor.y) && grid[neighbor.x, neighbor.y] == "street")
            {
                connected++;
                if (connected == 1)
                    keep = neighbor;
                else
                    grid[neighbor.x, neighbor.y] = null; // eliminamos conexiones extra
            }
        }
    }

    string GetStreetID(bool up, bool down, bool left, bool right)
    {
        if (up && down && left && right) return "cross";
        if (up && down && left) return "tRight";
        if (up && down && right) return "tLeft";
        if (left && right && up) return "tDown";
        if (left && right && down) return "tUp";
        if (up && right) return "cornerBL";
        if (right && down) return "cornerTL";
        if (down && left) return "cornerTR";
        if (left && up) return "cornerBR";
        if (left && right) return "horizontal";
        if (up && down) return "vertical";
        return "deadEnd";
    }

    void PlaceHouses()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (grid[x, z] == null && HasStreetNearby(x, z) && !IsSurroundedByHouses(x, z))
                {
                    grid[x, z] = "house";
                    GameObject houseToSpawn = houseVariants[Random.Range(0, houseVariants.Length)];
                    Vector3 position = new Vector3(x * cellSize, 0f, z * cellSize);
                    Instantiate(houseToSpawn, position, Quaternion.identity, transform);
                }
            }
        }
    }

    int CountStreets()
    {
        int count = 0;
        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                if (grid[x, z] == "street")
                    count++;
        return count;
    }

    bool HasStreetNearby(int x, int z)
    {
        int[][] dirs = {
            new int[] { 0, 1 }, new int[] { 0, -1 },
            new int[] { 1, 0 }, new int[] { -1, 0 }
        };

        foreach (var d in dirs)
        {
            int nx = x + d[0], nz = z + d[1];
            if (InBounds(nx, nz) && (grid[nx, nz] == "street" || grid[nx, nz] == "start" || grid[nx, nz] == "end"))
                return true;
        }

        return false;
    }

    bool IsSurroundedByHouses(int x, int z)
    {
        int count = 0;
        int[][] dirs = {
            new int[] { 0, 1 }, new int[] { 0, -1 },
            new int[] { 1, 0 }, new int[] { -1, 0 }
        };

        foreach (var d in dirs)
        {
            int nx = x + d[0], nz = z + d[1];
            if (InBounds(nx, nz) && grid[nx, nz] == "house")
                count++;
        }

        return count == 4;
    }

    Vector2Int Turn(Vector2Int dir, bool right = true)
    {
        if (dir == Vector2Int.up) return right ? Vector2Int.right : Vector2Int.left;
        if (dir == Vector2Int.right) return right ? Vector2Int.down : Vector2Int.up;
        if (dir == Vector2Int.down) return right ? Vector2Int.left : Vector2Int.right;
        if (dir == Vector2Int.left) return right ? Vector2Int.up : Vector2Int.down;
        return dir;
    }

    bool IsValidStreetPosition(Vector2Int pos, CellType[,] map)
    {
        int[][] neighbors = {
            new[] { 1, 0 }, new[] { -1, 0 },
            new[] { 0, 1 }, new[] { 0, -1 }
        };

        int streetCount = 0;

        foreach (var n in neighbors)
        {
            int nx = pos.x + n[0], ny = pos.y + n[1];
            if (InBounds(nx, ny) && map[nx, ny] == CellType.Street)
                streetCount++;
        }

        return streetCount < 2;
    }

    bool InBounds(Vector2Int pos) => pos.x >= 1 && pos.x < width - 1 && pos.y >= 1 && pos.y < height - 1;
    bool InBounds(int x, int z) => x >= 1 && x < width - 1 && z >= 1 && z < height - 1;

    void ClearPrevious()
    {
#if UNITY_EDITOR
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            if (!Application.isPlaying)
                DestroyImmediate(obj);
            else
                Destroy(obj);
        }
#endif
    }

    bool IsStreet(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < height &&
               (grid[x, z] == "street" || grid[x, z] == "start" || grid[x, z] == "end");
    }
}