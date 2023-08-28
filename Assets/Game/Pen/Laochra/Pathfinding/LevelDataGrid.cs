using System.Collections;
using UnityEngine;

public class LevelDataGrid : MonoBehaviour
{
    private LevelData levelDataGrid;
    private Tile[,] tiles = new Tile[0, 0];

    private void Awake()
    {
        levelDataGrid = Resources.Load("LevelData") as LevelData;
        StartCoroutine(LoadLevelData());
    }

    private void OnDrawGizmos()
    {
        if (levelDataGrid != null) levelDataGrid.OnDrawGizmos();
    }

    private IEnumerator LoadLevelData()
    {
        int columns = levelDataGrid.tileColumns;
        int rows = levelDataGrid.tilesArray.Length / levelDataGrid.tileColumns;

        tiles = new Tile[rows, columns];
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                tiles[x, z] = levelDataGrid.tilesArray[(columns * x) + z];
            }
            yield return null;
        }
        
        Resources.UnloadAsset(levelDataGrid);
        levelDataGrid = null;
    }
}