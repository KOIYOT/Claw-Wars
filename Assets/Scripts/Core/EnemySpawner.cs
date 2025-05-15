using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int minEnemies = 1;
    public int maxEnemies = 5;

    [Header("Offsets")]
    public float spawnHeight = 1.0f;
    public float spacing = 1.5f;

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

        float startX = -(enemyCount - 1) * spacing / 2;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 enemyPosition = new Vector3(
                transform.position.x + startX + i * spacing,
                transform.position.y + spawnHeight,
                transform.position.z
            );

            Instantiate(enemyPrefab, enemyPosition, Quaternion.identity);
        }
    }
}