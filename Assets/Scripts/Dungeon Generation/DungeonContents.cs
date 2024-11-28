using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonContents : MonoBehaviour
{
    public static DungeonContents instance;
    public GameObject enemy;

    private void Awake()
    {
        instance = this;
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < DungeonRenderer.dungeonRooms.Count; i++)
        {
            Bounds roomBounds = DungeonRenderer.dungeonRooms[i].playerRoomBoundary;
            SpawnRandomNumOfEnemies(roomBounds);
        }
    }

    private void SpawnRandomNumOfEnemies(Bounds roomBounds)
    {
        for (int i = 0; i < Random.Range(1, 4); i++)
        {
            GameObject newEnemy = Instantiate(enemy);
            newEnemy.GetComponent<EnemyMovement>().SpawnSetup(roomBounds);
        }
    }
}
