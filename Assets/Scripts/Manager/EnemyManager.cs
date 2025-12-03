using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class EnemyManager : MonoBehaviour
{
    private List<EnemyBaseBehavior> enemies = new List<EnemyBaseBehavior>();
    private int maxEnemies = 10;
    private int respawnLowerBound = 10;
    private int respawnUpperBound = 30;
    private int respawnAmountLowerBound = 1;
    private int respawnAmountUpperBound = 4;
    private float timeRemaining = 0;
    public List<EnemyBaseBehavior> Enemies
    {
        get { return enemies; }
    }

    public Tilemap spawnTilemap;
    // public Tilemap walkableAreaTilemap; // No longer needed - butterflies fly over everything
    public EnemyBaseBehavior enemyPrefab;
    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart && spawnTilemap != null && enemyPrefab != null)
            SpawnRandomInTilemap();
    }

    void Update()
    {
        // Only run spawning logic if tilemap and prefab are assigned
        if (spawnTilemap == null || enemyPrefab == null)
            return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        } else
        {
            SpawnRandomInTilemap();
            timeRemaining = Random.Range(respawnLowerBound, respawnUpperBound);
        }
    }

    public void enemyKilled(EnemyBaseBehavior enemy)
    {
        enemies.Remove(enemy);
    }
    public void enemyDamaged(EnemyBaseBehavior enemy) // modify later
    {
        enemyKilled(enemy);
        Destroy(enemy.gameObject);
    }

    private List<Rect> GetAllTileWorldRects()
    {
        var rects = new List<Rect>();
        if (spawnTilemap == null) {
            Debug.LogError("Spawn tilemap is not assigned.");
            return rects;
        }

        var bounds = spawnTilemap.cellBounds;
        Vector3 cellSize = Vector3.one;
        if (spawnTilemap.layoutGrid != null)
            cellSize = spawnTilemap.layoutGrid.cellSize;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                var cellPos = new Vector3Int(x, y, 0);
                if (!spawnTilemap.HasTile(cellPos)) continue;

                Vector3 center = spawnTilemap.GetCellCenterWorld(cellPos);
                float halfX = cellSize.x * 0.5f;
                float halfY = cellSize.y * 0.5f;
                var rect = new Rect(center.x - halfX, center.y - halfY, cellSize.x, cellSize.y);
                rects.Add(rect);

                Debug.Log($"Tile at {cellPos} with world rect {rect}");
            }
        }

        return rects;
    }

    public void SpawnRandomInTilemap()
    {
        if (spawnTilemap == null || enemyPrefab == null) {
            Debug.LogError("Spawn tilemap or enemy prefab is not assigned.");
            return;
        }

        var tileRects = GetAllTileWorldRects();
        if (tileRects.Count == 0) {
            Debug.LogWarning("No valid tiles found in the spawn tilemap.");
            return;
        }

        for (int i = 0; i < Random.Range(respawnAmountLowerBound, respawnAmountUpperBound + 1); i++)
        {
            var rect = tileRects[Random.Range(0, tileRects.Count)];
            if (enemies.Count >= maxEnemies) return;

            float rx = Random.Range(rect.xMin, rect.xMax);
            float ry = Random.Range(rect.yMin, rect.yMax);
            var pos = new Vector3(rx, ry, 0f);

            Debug.Log($"Spawning enemy at position {pos} within tile rect {rect}");

            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemies.Add(enemy);
        }
    }
}