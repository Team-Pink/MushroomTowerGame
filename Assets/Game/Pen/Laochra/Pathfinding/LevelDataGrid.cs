using System.Collections.Generic;
using UnityEngine;

public enum TileStatus
{
    Unreached,
    Pending,
    Closed
}

public class DijkstraTile
{
    public int x { get; private set; }
    public int z { get; private set; }

    public DijkstraTile currentBestNextTile;
    public float distanceFromCentreTile { get; private set; }

    public DijkstraTile(int xIndex, int zIndex)
    {
        x = xIndex;
        z = zIndex;

        currentBestNextTile = null;
        distanceFromCentreTile = float.PositiveInfinity;
    }

    public void SetDistance(float distance)
    {
        distanceFromCentreTile = distance;
    }

    public void SetNextTile(DijkstraTile tile)
    {
        currentBestNextTile = tile;
    }

    public List<DijkstraTile> GetConnectedTiles(Tile[,] indexingReference)
    {
        int xMax = indexingReference.GetLength(0) - 1;
        int zMax = indexingReference.GetLength(1) - 1;

        bool canGetAbove = z != 0;
        bool canGetBelow = z != zMax;
        bool canGetLeft = x != 0;
        bool canGetRight = x != xMax;

        List<DijkstraTile> connectedTiles = new();

        if (canGetAbove)
        {
            connectedTiles.Add(new(x, z - 1));
            if (canGetLeft)
                connectedTiles.Add(new(x - 1, z - 1));
            if (canGetRight)
                connectedTiles.Add(new(x + 1, z - 1));
        }
        if (canGetBelow)
        {
            connectedTiles.Add(new(x, z + 1));
            if (canGetLeft)
                connectedTiles.Add(new(x - 1, z + 1));
            if (canGetRight)
                connectedTiles.Add(new(x + 1, z + 1));
        }
        if (canGetLeft)
            connectedTiles.Add(new(x - 1, z));
        if (canGetRight)
            connectedTiles.Add(new(x + 1, z));

        foreach (DijkstraTile tile in connectedTiles)
        {
            float distanceToTile;
            if (Mathf.Abs(tile.x - x) + Mathf.Abs(tile.z - z) == 1)
                distanceToTile = 1;
            else
                distanceToTile = 1.41f;

            if (indexingReference[x, z].muddy)
                distanceToTile *= LevelDataGrid.mudCost; // This is causing things to chug?????

            tile.SetDistance(distanceToTile + distanceFromCentreTile);
            tile.SetNextTile(this);
        }
        return connectedTiles;
    }
}

public class Tile
{
    public int x { get; private set; }
    public int z { get; private set; }

    public TileStatus tileStatus = TileStatus.Unreached;
    public Tile bestNextTile;
    public Vector2 FlowDirection { get; private set; }
    public float InkThickness { get; private set; }
    public bool muddy;
    public float SpeedMultiplier
    {
        get
        {
            if (muddy)
                return 0.5f * (1 - InkThickness);
            else
                return 1 - InkThickness;
        }
    }

    public Tile(int xIndex, int zIndex)
    {
        x = xIndex;
        z = zIndex;
    }

    public void ApplyDirection(Vector2 flowDirectionInit)
    {
        FlowDirection = flowDirectionInit;
    }

    public void ClearInk()
    {
        InkThickness = 0;
    }

    public void SetInkThickness(float inkThicknessInit)
    {
        InkThickness = inkThicknessInit;
    }
}

[ExecuteInEditMode]
public class LevelDataGrid : MonoBehaviour
{
    // Grid Data
    private float gridWidth = 120;
    public float GridWidth { get => gridWidth; set => gridWidth = value; }
    private float gridHeight = 120;
    public float GridHeight { get => gridHeight; set => gridHeight = value; }
    private float TileWidth { get => gridWidth / xSubdivs; }
    private float TileHeight { get => gridHeight / zSubdivs; }

    // Tile Data
    public bool initialised = false;

    private Texture2D textureMap;
    public Texture2D TextureMap
    {
        get => textureMap;
        set
        {
            textureMap = value;
            InitialiseGrid();
        }
    }
    public static float mudCost { get; private set; } = 20;
    public float MudCost
    {
        get => mudCost;
        set
        {
            mudCost = value;
            InitialiseGrid();
        }
    }

    private int xSubdivs = 200;
    public int XSubdivs
    {
        get => xSubdivs;
        set
        {
            xSubdivs = value;
            initialised = false;
        }
    }
    private int zSubdivs = 200;
    public int ZSubdivs
    {
        get => zSubdivs;
        set
        {
            zSubdivs = value;
            initialised = false;
        }
    }

    private Tile[,] tiles = new Tile[0, 0];

    private readonly List<DijkstraTile> pendingList = new();

    private int amountClosed = 0;

    // Debug
    public bool drawGrid;
    public bool drawFlowLines;

    public void InitialiseGrid()
    {
        Clear();
        CreateGrid();
        PopulateGridMuddiness();
        initialised = true;
    }

    private void Clear()
    {
        if (tiles != null)
        {
            foreach (Tile tile in tiles)
            {
                tile.tileStatus = TileStatus.Unreached;
                tile.bestNextTile = null;
                tile.muddy = false;
            }
        }
        amountClosed = 0;
    }

    private void CreateGrid()
    {
        tiles = new Tile[xSubdivs, zSubdivs];

        for (int xIndex = 0; xIndex < xSubdivs; xIndex++)
        {
            for (int zIndex = 0; zIndex < zSubdivs; zIndex++)
            {
                tiles[xIndex, zIndex] = new Tile(xIndex, zIndex);
            }
        }
    }

    private void PopulateGridMuddiness()
    {
        if (TextureMap == null)
            return;

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int z = 0; z < tiles.GetLength(1); z++)
            {
                if (textureMap.GetPixel(x, z).r == float.Epsilon + 1)
                    tiles[x, z].muddy = true;
            }
        }
    }

    public void PopulateGridDistances()
    {
        int xMidpoint = xSubdivs / 2;
        int zMidpoint = zSubdivs / 2;

        // Dijkstra's Algorithm
        DijkstraTile centreTile = new(xMidpoint, zMidpoint);
        pendingList.Add(centreTile);
        centreTile.SetDistance(0);
        tiles[xMidpoint, zMidpoint].tileStatus = TileStatus.Pending;
        
        while (pendingList.Count > 0)
        {
            DijkstraTile currentCheapestTile = null;

            foreach(DijkstraTile tile in pendingList)
            {
                if (currentCheapestTile == null)
                    currentCheapestTile = tile;
                else
                {
                    if (tile.distanceFromCentreTile < currentCheapestTile.distanceFromCentreTile)
                        currentCheapestTile = tile;
                }
            }

            AddToPendingList(currentCheapestTile.GetConnectedTiles(tiles));
            Close(currentCheapestTile);
        }
        Debug.Log("Successfully set " + amountClosed.ToString() + " tiles");
    }

    private void Close(DijkstraTile tileToSet)
    {
        Tile tile = tiles[tileToSet.x, tileToSet.z];

        tile.tileStatus = TileStatus.Closed;
        if (tileToSet.currentBestNextTile != null)
        {
            tile.bestNextTile = tiles[tileToSet.currentBestNextTile.x, tileToSet.currentBestNextTile.z];
            //tile.ApplyDirection(new Vector2(tile.bestNextTile.x - tile.x, tile.bestNextTile.z - tile.z).normalized);
        }

        pendingList.Remove(tileToSet);
        amountClosed++;
    }

    private void SetPending(DijkstraTile tileToSet)
    {
        //Debug.Log("Setting tile " + tileToSet.x + ", " + tileToSet.z + " to pending.");
        if (tiles[tileToSet.x, tileToSet.z].tileStatus == TileStatus.Closed) return;
        if (tiles[tileToSet.x, tileToSet.z].tileStatus == TileStatus.Unreached)
        {
            pendingList.Add(tileToSet);
            tiles[tileToSet.x, tileToSet.z].tileStatus = TileStatus.Pending;
            return;
        }
        foreach (DijkstraTile tile in pendingList)
        {
            if (tile.x == tileToSet.x && tile.z == tileToSet.z)
            {
                if (tileToSet.distanceFromCentreTile < tile.distanceFromCentreTile)
                {
                    tile.SetDistance(tileToSet.distanceFromCentreTile);
                    tile.SetNextTile(tileToSet.currentBestNextTile);
                }
                return;
            }
        }

    }
    private void AddToPendingList(List<DijkstraTile> listToAssign)
    {
        foreach (DijkstraTile tile in listToAssign)
        {
            SetPending(tile);
        }
    }

    public Vector3 SwizzleXZY(float xCoord, float zCoord, float yCoord = 0)
    {
        return new Vector3(xCoord, yCoord, zCoord);
    }

    public void OnDrawGizmos()
    {
        if (drawGrid)
        {
            // Draw the grid
            for (int xIndex = 0; xIndex < xSubdivs + 1; xIndex++)
            {
                float xCoord = TileWidth * (xIndex - xSubdivs * 0.5f);
                float nextXCoord = xCoord + TileWidth;

                for (int zIndex = 0; zIndex < zSubdivs + 1; zIndex++)
                {
                    float zCoord = TileHeight * (zIndex - zSubdivs * 0.5f);
                    float nextZCoord = zCoord + TileHeight;

                    if (xIndex < xSubdivs)
                        Gizmos.DrawLine(SwizzleXZY(nextXCoord, zCoord), SwizzleXZY(xCoord, zCoord));
                    if (zIndex < zSubdivs)
                        Gizmos.DrawLine(SwizzleXZY(xCoord, zCoord), SwizzleXZY(xCoord, nextZCoord));
                }
            }
        }

        if (drawFlowLines)
        {
            // Draw lines to next tiles
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int z = 0; z < tiles.GetLength(1); z++)
                {
                    Tile currentTile = tiles[x, z];

                    if (currentTile.bestNextTile != null)
                    {
                        if (tiles[x, z].bestNextTile.muddy)
                            Gizmos.color = Color.yellow;
                        else
                            Gizmos.color = Color.magenta;

                        Gizmos.DrawLine(TileToWorldSpace(x, z), TileToWorldSpace(currentTile.bestNextTile.x, currentTile.bestNextTile.z));
                    }
                }
            }
        }

        // Gizmos.DrawCube() Draw a cube in the tile that the mouse is over. Make use of GetTileCoords
    }

    public void GetTileAtCoords(Vector3 position, out int xCoord, out int zCoord)
    {
        xCoord = Mathf.RoundToInt((position.x + xSubdivs * 0.5f) / TileWidth);
        zCoord = Mathf.RoundToInt((-position.z - zSubdivs * 0.5f) / TileHeight);
    }

    public Vector3 TileToWorldSpace(int xIndex, int zIndex)
    {

        return new Vector3((xIndex - xSubdivs * 0.5f) * TileWidth + TileWidth * 0.5f, 0,
            (-zIndex + zSubdivs * 0.5f) * TileHeight - TileWidth * 0.5f);
    }

    private Vector2 GetFlowAtPoint(Vector3 position)
    {
        GetTileAtCoords(position, out int xPos, out int zPos);

        int xCoord = Mathf.RoundToInt(xPos + xSubdivs * 0.5f);
        int zCoord = Mathf.RoundToInt(zPos + zSubdivs * 0.5f);
        return tiles[xCoord, zCoord].FlowDirection;
    }
}

#if UNITY_EDITOR
namespace Editor
{
    using UnityEditor;
    using static UnityEngine.GUILayout;
    using GUI = UnityEditor.EditorGUILayout;

    [CustomEditor(typeof(LevelDataGrid))]
    public class LevelDataGridEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LevelDataGrid dataGrid = target as LevelDataGrid;

            EditorGUI.BeginChangeCheck();


            GUI.Space(10);


            GUI.LabelField("Grid Data", Stylesheet.Heading);

            BeginHorizontal();
            GUI.Space();
            GUI.LabelField("Width", MaxWidth(60));
            float gridWidth = GUI.FloatField(dataGrid.GridWidth, MaxWidth(40));
            GUI.Space();
            EndHorizontal();

            GUI.Space();

            BeginHorizontal();
            GUI.Space();
            GUI.LabelField("Height", MaxWidth(60));
            float gridHeight = GUI.FloatField(dataGrid.GridHeight, MaxWidth(40));
            GUI.Space();
            EndHorizontal();


            GUI.Space(20);


            GUI.LabelField("Tile Data", Stylesheet.Heading);

            BeginHorizontal();
                GUI.Space();
                GUI.LabelField("Tile Map", MaxWidth(60));
                Texture2D textureMap = (Texture2D)GUI.ObjectField(dataGrid.TextureMap, typeof(Texture2D), false, MaxWidth(68));
                GUI.Space();
            EndHorizontal();

            GUI.Space();

            float mudCost = 0;
            int xSubdivs;
            int zSubdivs;
            bool initialiseGrid = false;
            if (textureMap != null)
            {
                BeginHorizontal();
                    GUI.Space();
                    GUI.LabelField("Mud Cost Multiplier", MaxWidth(120));
                    mudCost = GUI.FloatField(dataGrid.MudCost, MaxWidth(40));
                    GUI.Space();
                EndHorizontal();

                xSubdivs = textureMap.width;
                zSubdivs = textureMap.height;
            }
            else
            {
                BeginHorizontal();
                    GUI.Space();
                    GUI.LabelField("X Subdivs", MaxWidth(60));
                    xSubdivs = (int)GUI.Slider(dataGrid.XSubdivs, 3, 4096, MaxWidth(120));
                    GUI.Space();
                EndHorizontal();

                GUI.Space();

                BeginHorizontal();
                    GUI.Space();
                    GUI.LabelField("Z Subdivs", MaxWidth(60));
                    zSubdivs = (int)GUI.Slider(dataGrid.ZSubdivs, 3, 4096, MaxWidth(120));
                    GUI.Space();
                EndHorizontal();

                BeginHorizontal();
                    GUI.Space();
                    BeginVertical("box", MaxWidth(400));
                        GUI.LabelField("Assigning a Tile Map will automatically populate subdivisions", Stylesheet.Note);
                    EndVertical();
                    GUI.Space();
                EndHorizontal();

                if (!dataGrid.initialised)
                {
                    GUI.Space(20);

                    BeginHorizontal();
                        GUI.Space();
                        initialiseGrid = Button("Initialise Grid", MaxWidth(120));
                        GUI.Space();
                    EndHorizontal();
                }
            }

            bool generateFlow = false;
            if (dataGrid.initialised)
            {
                GUI.Space(20);

                BeginHorizontal();
                    GUI.Space();
                    generateFlow = Button("Generate Flow", MaxWidth(120));
                    GUI.Space();
                EndHorizontal();
            }


            GUI.Space(40);


            GUI.LabelField("Debug Options", Stylesheet.Heading);

            BeginHorizontal();
                GUI.Space();
                GUI.LabelField("Display Grid Tiles", MaxWidth(110));
                bool drawGrid = GUI.Toggle(dataGrid.drawGrid, MaxWidth(20));
                GUI.Space();
            EndHorizontal();

            GUI.Space();

            BeginHorizontal();
                GUI.Space();
                GUI.LabelField("Display Flow Lines", MaxWidth(110));
                bool drawFlowLines = GUI.Toggle(dataGrid.drawFlowLines, MaxWidth(20));
                GUI.Space();
            EndHorizontal();


            GUI.Space(20);


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "World Data Grid was modified");

                dataGrid.GridWidth = gridWidth;
                dataGrid.GridHeight = gridHeight;

                dataGrid.TextureMap = textureMap;

                if (textureMap != null)
                {
                    if (mudCost != float.Epsilon) dataGrid.MudCost = mudCost;
                }
                else
                {
                    if (xSubdivs > 0) dataGrid.XSubdivs = xSubdivs;
                    if (zSubdivs > 0) dataGrid.ZSubdivs = zSubdivs;

                    if (initialiseGrid) dataGrid.InitialiseGrid();
                }

                if (generateFlow) dataGrid.PopulateGridDistances();

                dataGrid.drawGrid = drawGrid;
                dataGrid.drawFlowLines = drawFlowLines;

                SceneView.RepaintAll();
            } // If data is to be changed, log an undo screenshot, then change the data
        }
    }
}
#endif