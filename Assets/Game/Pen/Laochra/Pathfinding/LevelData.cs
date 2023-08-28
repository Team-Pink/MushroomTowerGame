using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public enum TileStatus
{
    Unreached,
    Pending,
    Closed
}

public class PendingTile
{
    public int x { get; private set; }
    public int z { get; private set; }

    public PendingTile currentBestNextTile;
    public float distanceFromCentreTile { get; private set; }

    public PendingTile(int xIndex, int zIndex)
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

    public void SetNextTile(PendingTile tile)
    {
        currentBestNextTile = tile;
    }

    public List<PendingTile> GetConnectedTiles(ClosedTile[,] tilesReference)
    {
        int xMax = tilesReference.GetLength(0) - 1;
        int zMax = tilesReference.GetLength(1) - 1;

        bool canGetAbove = z != 0;
        bool canGetBelow = z != zMax;
        bool canGetLeft = x != 0;
        bool canGetRight = x != xMax;

        List<PendingTile> connectedTiles = new();

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

        foreach (PendingTile tile in connectedTiles)
        {
            float distanceToTile;
            if (Mathf.Abs(tile.x - x) + Mathf.Abs(tile.z - z) == 1) // Try caching Abs values before the if: https://www.jacksondunstan.com/articles/5361
                distanceToTile = 1;
            else
                distanceToTile = 1.41f;

            if (tilesReference[x, z].muddy)
                distanceToTile *= LevelData.mudCost;

            tile.SetDistance(distanceToTile + distanceFromCentreTile);
            tile.SetNextTile(this);
        }
        return connectedTiles;
    }

    public bool SharesCoordsWith(PendingTile otherTile) => x == otherTile.x && z == otherTile.z;

    public override int GetHashCode() => HashCode.Combine(x, z);
}

[Serializable]
public class ClosedTile
{
    public int x { get; private set; }
    public int z { get; private set; }

    [NonSerialized] public TileStatus tileStatus = TileStatus.Unreached;
    public ClosedTile bestNextTile;
    [SerializeField] private Vector2 flowDirection;
    public Vector2 FlowDirection { get => flowDirection; private set => flowDirection = value; }
    public bool muddy;

    public ClosedTile()
    {

    }

    public ClosedTile(int xIndex, int zIndex)
    {
        x = xIndex;
        z = zIndex;
    }

    public void ApplyDirection(Vector2 direction)
    {
        FlowDirection = direction;
    }
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Mushroom Tower Game/Level Data")]
public class LevelData : ScriptableObject
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
    public bool generated = false;

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

    private ClosedTile[,] tiles = new ClosedTile[0, 0];

    private readonly List<PendingTile> pendingList = new();

    private readonly Dictionary<int, PendingTile> pendingDictionary = new();

    private int amountClosed = 0;

    // Debug
    [NonSerialized] public bool drawGrid;
    [NonSerialized] public bool drawFlowLines;

    private void OnEnable()
    {
        InitialiseGrid();
    }

    public void InitialiseGrid()
    {
        Clear();
        CreateGrid();
        PopulateGridMuddiness();
        initialised = true;
        generated = false;
    }

    private void Clear()
    {
        if (tiles != null)
        {
            foreach (ClosedTile tile in tiles)
            {
                tile.tileStatus = TileStatus.Unreached;
                tile.bestNextTile = null;
                tile.muddy = false;
            }
        }
        amountClosed = 0;

        pendingDictionary.Clear();
        pendingList.Clear();
    }

    private void CreateGrid()
    {
        tiles = new ClosedTile[xSubdivs, zSubdivs];

        for (int xIndex = 0; xIndex < xSubdivs; xIndex++)
        {
            for (int zIndex = 0; zIndex < zSubdivs; zIndex++)
            {
                tiles[xIndex, zIndex] = new ClosedTile(xIndex, zIndex);
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
        float cachedTime = Time.realtimeSinceStartup;
        int xMidpoint = xSubdivs / 2;
        int zMidpoint = zSubdivs / 2;

        // Dijkstra's Algorithm
        PendingTile centreTile = new(xMidpoint, zMidpoint);
        pendingDictionary.Add(centreTile.GetHashCode(), centreTile);
        pendingList.Add(centreTile);
        centreTile.SetDistance(0);
        tiles[xMidpoint, zMidpoint].tileStatus = TileStatus.Pending;
        
        while (pendingList.Count > 0)
        {
            AddToPendingList(pendingList[0].GetConnectedTiles(tiles));
            Close(pendingList[0]);
        }
        float duration = Mathf.Round((Time.realtimeSinceStartup - cachedTime) * 100) / 100;

        Debug.Log("Successfully set " + amountClosed.ToString() + " tiles (" + duration + " seconds)");

        WriteToTexture();

        generated = true;
    }

    private void WriteToTexture()
    {
        int columns = tiles.GetLength(0);
        int rows = tiles.GetLength(1);

        Texture2D texture = new Texture2D(columns, rows);

        float muddy;
        float flowDirectionX;
        float flowDirectionZ;
        Color color;

        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                if (tiles[x, z].muddy)
                    muddy = 1.0f;
                else
                    muddy = 0.0f;

                flowDirectionX = tiles[x, z].FlowDirection.x;
                flowDirectionZ = tiles[x, z].FlowDirection.y;

                color = new(muddy, flowDirectionX, flowDirectionZ);

                texture.SetPixel(x, z, color);
            }
        }

        byte[] bytes = texture.EncodeToPNG();

        var directory = Application.dataPath + "/Resources/";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllBytes(directory + "Texture.png", bytes);
    }

    private void Close(PendingTile tileToSet)
    {
        ClosedTile tile = tiles[tileToSet.x, tileToSet.z];

        tile.tileStatus = TileStatus.Closed;
        if (tileToSet.currentBestNextTile != null)
        {
            tile.bestNextTile = tiles[tileToSet.currentBestNextTile.x, tileToSet.currentBestNextTile.z];
            tile.ApplyDirection(new Vector2(tile.bestNextTile.x - tile.x, tile.bestNextTile.z - tile.z).normalized);
        }

        pendingList.Remove(tileToSet);
        pendingDictionary.Remove(tileToSet.GetHashCode());
        amountClosed++;
    }

    private void SetPending(PendingTile tileToSet)
    {
        if (tiles[tileToSet.x, tileToSet.z].tileStatus == TileStatus.Closed) return;

        if (tiles[tileToSet.x, tileToSet.z].tileStatus == TileStatus.Unreached)
        {
            pendingDictionary.Add(tileToSet.GetHashCode(), tileToSet);
            InsertInPendingList(tileToSet);
            tiles[tileToSet.x, tileToSet.z].tileStatus = TileStatus.Pending;
            return;
        }

        PendingTile oldTile = pendingDictionary[tileToSet.GetHashCode()];
        
        if (tileToSet.distanceFromCentreTile < oldTile.distanceFromCentreTile)
        {
            pendingList.Remove(oldTile);
            pendingDictionary.Remove(tileToSet.GetHashCode());

            pendingDictionary.Add(tileToSet.GetHashCode(), tileToSet);
            InsertInPendingList(tileToSet);
        }

    }
    private void AddToPendingList(List<PendingTile> listToAssign)
    {
        foreach (PendingTile tile in listToAssign)
        {
            SetPending(tile);
        }
    }

    private void InsertInPendingList(PendingTile tile, int startingPoint = -1 /*'Null' default initialiser*/)
    {
        if (pendingList.Count <= 1)
        {
            pendingList.Add(tile);
            return;
        }

        // Binary search to find index to slot in the element
        int testingIndex;
        int lastTestedIndex = -1; // 'Null' initialise

        if (startingPoint >= 0)
            testingIndex = startingPoint;
        else
            testingIndex = (pendingList.Count - 1) / 2;

        while (tile.distanceFromCentreTile != pendingList[testingIndex].distanceFromCentreTile)
        {
            if (testingIndex == 0 || testingIndex == lastTestedIndex)
            {
                pendingList.Insert(testingIndex, tile);
                return;
            }

            if (testingIndex == pendingList.Count - 1)
            {
                pendingList.Add(tile);
                return;
            }

            if (lastTestedIndex >= 0) // 'Null' check
            {
                if (tile.distanceFromCentreTile < pendingList[testingIndex].distanceFromCentreTile)
                {
                    if (tile.distanceFromCentreTile > pendingList[lastTestedIndex].distanceFromCentreTile)
                    {
                        pendingList.Insert(testingIndex, tile);
                        return;
                    }
                }
                else
                {
                    if (tile.distanceFromCentreTile < pendingList[lastTestedIndex].distanceFromCentreTile)
                    {
                        pendingList.Insert(lastTestedIndex, tile);
                        return;
                    }
                }
            }
            
            if (tile.distanceFromCentreTile < pendingList[testingIndex].distanceFromCentreTile)
            {
                lastTestedIndex = testingIndex;
                testingIndex -=  testingIndex / 2;
            }
            else
            {
                lastTestedIndex = testingIndex;
                testingIndex += (pendingList.Count - 1 - testingIndex) / 2;
            }
        }

        pendingList.Insert(testingIndex, tile);
    }

    public Vector3 SwizzleXZY(float xCoord, float zCoord, float yCoord = 0)
    {
        return new Vector3(xCoord, yCoord, zCoord);
    }

    public void OnDrawGizmos() // move into scene object
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
                    ClosedTile currentTile = tiles[x, z];

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

    [CustomEditor(typeof(LevelData))]
    public class LevelDataGridEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LevelData dataGrid = target as LevelData;

            if (!dataGrid.initialised || !dataGrid.generated)
            {
                BeginVertical("box");
                    GUI.LabelField("Your changes have not been saved", Stylesheet.Note);
                EndVertical();
            }

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

                if (dataGrid.XSubdivs != xSubdivs) dataGrid.XSubdivs = xSubdivs;
                if (dataGrid.ZSubdivs != zSubdivs) dataGrid.ZSubdivs = zSubdivs;

                if (dataGrid.TextureMap != textureMap)
                {
                    dataGrid.TextureMap = textureMap;
                    dataGrid.InitialiseGrid();
                }

                if (textureMap != null)
                {
                    if (mudCost != float.Epsilon) dataGrid.MudCost = mudCost;
                }
                else
                {
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