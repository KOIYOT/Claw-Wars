using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public float TileSize_ = 3f;

    public GameObject[] HouseVariants_;

    public GameObject StartPointPrefab_;
    public GameObject EndPointPrefab_;

    public StreetVariant VariantDatabase_;

    public int Width_ = 50;
    public int Height_ = 50;

    public int RoadBuilders_ = 5;
    public int MaxSteps_ = 50;
    public float TurnChance_ = 0.0125f;
    public float BranchChance_ = 0.125f;
    public int MinStreetCount_ = 150;

    private string[,] Grid_;
    private List<Vector2Int> StreetPositions_ = new List<Vector2Int>();
    private Vector2Int StartPos_;
    private Vector2Int EndPos_;

    private enum CellType_ { Empty, Street, House }

    private class RoadWalker_
    {
        public Vector2Int Position_;
        public Vector2Int Direction_;
        public int StepsLeft_;
    }

    private readonly Vector2Int[] Directions_ = new Vector2Int[]
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
        int Attempts_ = 0;
        const int MaxAttempts_ = 5;

        do
        {
            ClearPrevious_();
            Grid_ = new string[Width_, Height_];
            GenerateStreets_();
            Attempts_++;
        }
        while (CountStreets_() < MinStreetCount_ && Attempts_ < MaxAttempts_);

        if (CountStreets_() < MinStreetCount_)
        {
            Debug.LogWarning("Failed to generate a city with enough streets after " + MaxAttempts_ + " attempts.");
        }

        MarkStartAndEnd_();
        InstantiateStreets_();
        PlaceHouses_();
    }

    void GenerateStreets_()
    {
        CellType_[,] CellMap_ = new CellType_[Width_, Height_];
        List<RoadWalker_> Walkers_ = new List<RoadWalker_>();
        StreetPositions_.Clear();

        for (int i = 0; i < RoadBuilders_; i++)
        {
            Vector2Int Dir_ = Directions_[Random.Range(0, Directions_.Length)];
            Walkers_.Add(new RoadWalker_
            {
                Position_ = new Vector2Int(Width_ / 2, Height_ / 2),
                Direction_ = Dir_,
                StepsLeft_ = MaxSteps_
            });
        }

        while (Walkers_.Count > 0)
        {
            List<RoadWalker_> NewWalkers_ = new List<RoadWalker_>();

            for (int i = Walkers_.Count - 1; i >= 0; i--)
            {
                var W_ = Walkers_[i];

                if (!InBounds_(W_.Position_)) { Walkers_.RemoveAt(i); continue; }
                if (!IsValidStreetPosition_(W_.Position_, CellMap_)) { Walkers_.RemoveAt(i); continue; }

                CellMap_[W_.Position_.x, W_.Position_.y] = CellType_.Street;
                Grid_[W_.Position_.x, W_.Position_.y] = "street";
                StreetPositions_.Add(W_.Position_);

                if (Random.value < TurnChance_)
                    W_.Direction_ = Turn_(W_.Direction_);

                W_.Position_ += W_.Direction_;
                W_.StepsLeft_--;

                if (Random.value < BranchChance_)
                {
                    Vector2Int NewDir_ = Turn_(W_.Direction_, Random.value > 0.5f);
                    NewWalkers_.Add(new RoadWalker_
                    {
                        Position_ = W_.Position_,
                        Direction_ = NewDir_,
                        StepsLeft_ = W_.StepsLeft_ / 2
                    });
                }

                if (W_.StepsLeft_ <= 0) Walkers_.RemoveAt(i);
            }

            Walkers_.AddRange(NewWalkers_);
        }
    }

    void MarkStartAndEnd_()
    {
        List<Vector2Int> BorderStreets_ = new List<Vector2Int>();

        foreach (var Pos_ in StreetPositions_)
        {
            if (Pos_.x <= 1 || Pos_.x >= Width_ - 2 || Pos_.y <= 1 || Pos_.y >= Height_ - 2)
                BorderStreets_.Add(Pos_);
        }

        Vector2Int BestA_ = Vector2Int.zero;
        Vector2Int BestB_ = Vector2Int.zero;
        float BestDist_ = 0f;

        List<Vector2Int> Candidates_ = BorderStreets_.Count >= 2 ? BorderStreets_ : StreetPositions_;

        for (int i = 0; i < Candidates_.Count; i++)
        {
            for (int j = i + 1; j < Candidates_.Count; j++)
            {
                float Dist_ = Vector2.Distance(Candidates_[i], Candidates_[j]);
                if (Dist_ > BestDist_)
                {
                    BestDist_ = Dist_;
                    BestA_ = Candidates_[i];
                    BestB_ = Candidates_[j];
                }
            }
        }

        StartPos_ = BestA_;
        EndPos_ = BestB_;

        if (InBounds_(StartPos_)) Grid_[StartPos_.x, StartPos_.y] = "start";
        if (InBounds_(EndPos_)) Grid_[EndPos_.x, EndPos_.y] = "end";
    }

    void InstantiateStreets_()
    {
        for (int x = 0; x < Width_; x++)
        {
            for (int z = 0; z < Height_; z++)
            {
                string Cell_ = Grid_[x, z];

                if (Cell_ == null) continue;

                if (Cell_ == "start" || Cell_ == "end")
                {
                    bool Uup_ = IsStreet_(x, z + 1);
                    bool Ddown_ = IsStreet_(x, z - 1);
                    bool Lleft_ = IsStreet_(x - 1, z);
                    bool Rright_ = IsStreet_(x + 1, z);

                    float RotaY_ = GetStreetRotation_("deadEnd", Uup_, Ddown_, Lleft_, Rright_);

                    GameObject Prefab_ = Cell_ == "start" ? StartPointPrefab_ : EndPointPrefab_;
                    if (Prefab_ != null)
                    {
                         Instantiate(Prefab_, new Vector3(x * TileSize_, 0, z * TileSize_), Quaternion.Euler(0, RotaY_, 0), transform);
                    } else {
                         Debug.LogWarning(Cell_ + " Point Prefab is not assigned in the Inspector.");
                    }
                    continue;
                }

                if (Cell_ != "street") continue;

                bool Up_ = IsStreet_(x, z + 1);
                bool Down_ = IsStreet_(x, z - 1);
                bool Left_ = IsStreet_(x - 1, z);
                bool Right_ = IsStreet_(x + 1, z);

                string StreetID_ = GetStreetID_(Up_, Down_, Left_, Right_);
                float RotY_ = GetStreetRotation_(StreetID_, Up_, Down_, Left_, Right_);

                if (VariantDatabase_ != null)
                {
                    GameObject Prefab_ = VariantDatabase_.GetVariant_(StreetID_);
                    if (Prefab_ != null)
                        Instantiate(Prefab_, new Vector3(x * TileSize_, 0, z * TileSize_), Quaternion.Euler(0, RotY_, 0), transform);
                     else {
                         Debug.LogWarning("No street variant prefab found for ID: " + StreetID_);
                     }
                } else {
                     Debug.LogWarning("Street Variant Database is not assigned.");
                }
            }
        }
    }

    float GetStreetRotation_(string id, bool up, bool down, bool left, bool right)
    {
        switch (id)
        {
            case "deadEnd":
                if (up && !down && !left && !right) return 0f;
                if (!up && down && !left && !right) return 180f;
                if (!up && !down && left && !right) return -90f;
                if (!up && !down && !left && right) return 90f;
                break;
            case "tUp": return 0f;
            case "tDown": return 180f;
            case "tLeft": return -90f;
            case "tRight": return 90f;

            case "cornerTL": return 90f;
            case "cornerTR": return 180f;
            case "cornerBR": return -90f;
            case "cornerBL": return 0f;

            case "horizontal": return 0f;
            case "vertical": return 90f;
            case "cross": return 0f;
        }
        return 0f;
    }

    string GetStreetID_(bool up, bool down, bool left, bool right)
    {
        int Connections_ = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        if (Connections_ == 4) return "cross";
        if (Connections_ == 3)
        {
            if (!right) return "tRight";
            if (!left) return "tLeft";
            if (!down) return "tUp";
            if (!up) return "tDown";
        }
        if (Connections_ == 2)
        {
            if (up && right) return "cornerBL";
            if (right && down) return "cornerTL";
            if (down && left) return "cornerTR";
            if (left && up) return "cornerBR";
            if (left && right) return "horizontal";
            if (up && down) return "vertical";
        }
        if (Connections_ == 1)
        {
            return "deadEnd";
        }

        return "empty";
    }


    void PlaceHouses_()
    {
        for (int x = 0; x < Width_; x++)
        {
            for (int z = 0; z < Height_; z++)
            {
                if (Grid_[x, z] == null && HasStreetNearby_(x, z) && !IsSurroundedByHouses_(x, z))
                {
                    Grid_[x, z] = "house";
                    if (HouseVariants_ != null && HouseVariants_.Length > 0)
                    {
                        GameObject HouseToSpawn_ = HouseVariants_[Random.Range(0, HouseVariants_.Length)];
                        if (HouseToSpawn_ != null)
                             Instantiate(HouseToSpawn_, new Vector3(x * TileSize_, 0, z * TileSize_), Quaternion.identity, transform);
                         else {
                             Debug.LogWarning("One or more House Variants prefabs are not assigned.");
                         }
                    } else {
                         Debug.LogWarning("House Variants array is empty or not assigned.");
                    }
                }
            }
        }
    }

    int CountStreets_()
    {
        int Count_ = 0;
        for (int x = 0; x < Width_; x++)
            for (int z = 0; z < Height_; z++)
                if (Grid_[x, z] == "street" || Grid_[x, z] == "start" || Grid_[x, z] == "end")
                    Count_++;
        return Count_;
    }

    bool HasStreetNearby_(int x, int z)
    {
        int[][] Dirs_ = {
            new int[] { 0, 1 }, new int[] { 0, -1 },
            new int[] { 1, 0 }, new int[] { -1, 0 }
        };

        foreach (var D_ in Dirs_)
        {
            int Nx_ = x + D_[0], Nz_ = z + D_[1];
            if (InBounds_(Nx_, Nz_) && IsStreet_(Nx_, Nz_))
                return true;
        }

        return false;
    }

    bool IsSurroundedByHouses_(int x, int z)
    {
        int Count_ = 0;
        int[][] Dirs_ = {
            new int[] { 0, 1 }, new int[] { 0, -1 },
            new int[] { 1, 0 }, new int[] { -1, 0 }
        };

        foreach (var D_ in Dirs_)
        {
            int Nx_ = x + D_[0], Nz_ = z + D_[1];
            if (InBounds_(Nx_, Nz_) && Grid_[Nx_, Nz_] == "house")
                Count_++;
        }

        return Count_ == 4;
    }

    Vector2Int Turn_(Vector2Int dir, bool right = true)
    {
        if (dir == Vector2Int.up) return right ? Vector2Int.right : Vector2Int.left;
        if (dir == Vector2Int.right) return right ? Vector2Int.down : Vector2Int.up;
        if (dir == Vector2Int.down) return right ? Vector2Int.left : Vector2Int.right;
        if (dir == Vector2Int.left) return right ? Vector2Int.up : Vector2Int.down;
        return dir;
    }

    bool IsValidStreetPosition_(Vector2Int pos, CellType_[,] map)
    {
        if (!InBounds_(pos)) return false;
        if (map[pos.x, pos.y] != CellType_.Empty) return false;

        int[][] Neighbors_ = {
            new[] { 1, 0 }, new[] { -1, 0 },
            new[] { 0, 1 }, new[] { 0, -1 }
        };

        int StreetCount_ = 0;

        foreach (var N_ in Neighbors_)
        {
            int Nx_ = pos.x + N_[0], Ny_ = pos.y + N_[1];
            if (InBounds_(Nx_, Ny_) && map[Nx_, Ny_] == CellType_.Street)
                StreetCount_++;
        }

        return StreetCount_ < 2;
    }

    bool InBounds_(Vector2Int pos) => pos.x >= 1 && pos.x < Width_ - 1 && pos.y >= 1 && pos.y < Height_ - 1;
    bool InBounds_(int x, int z) => x >= 1 && x < Width_ - 1 && z >= 1 && z < Height_ - 1;

#if UNITY_EDITOR
    void ClearPrevious_()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject Obj_ = transform.GetChild(i).gameObject;
            if (!Application.isPlaying)
                DestroyImmediate(Obj_);
            else
                Destroy(Obj_);
        }
    }
#endif

    bool IsStreet_(int x, int z)
    {
        return x >= 0 && x < Width_ && z >= 0 && z < Height_ &&
               (Grid_[x, z] == "street" || Grid_[x, z] == "start" || Grid_[x, z] == "end");
    }
}

// © 2025 KOIYOT. All rights reserved.