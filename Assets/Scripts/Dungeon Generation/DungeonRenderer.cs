using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonRenderer : MonoBehaviour
{
    public bool autoUpdate;


    public Tilemap tilemap;
    public TileBase floorTile;
    public TileBase crackedFloorTile;
    public TileBase sideWallTile;
    public TileBase backWallTile;
    public TileBase dirtTile;


    public static Dictionary<int, DungeonRoom> dungeonRooms = new Dictionary<int, DungeonRoom>();
    public static HashSet<Vector2Int> usedTilePositions = new HashSet<Vector2Int>();


    private void Start()
    {
        GenerateDungeon();

    }

    public void GenerateDungeon()
    {
        GameObject.Find("Grid Generator").GetComponent<DungeonGenerator>().GenerateDungeonData();
        RenderDungeon();
        DungeonContents.instance.SpawnEnemies();

        PlayerMovement.boundsCurrentlyContainingPlayer = dungeonRooms[0].playerRoomBoundary;
        PlayerMovement.roomCurrentlyContainingPlayer = dungeonRooms[0];
        PlayerAttack.DefineBoundsHashset();
    }

    private void RenderDungeon()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < dungeonRooms.Count; x++)
        {
            DungeonRoom room = dungeonRooms[x];

            for (int i = 0; i < room.roomTilePositions.GetLength(0); i++)
            {
                for (int j = 0; j < room.roomTilePositions.GetLength(1); j++)
                {
                    Vector3Int tilePosition = new Vector3Int(room.roomTilePositions[i, j].x, room.roomTilePositions[i, j].y, 0);
                    PlaceTile(tilePosition, CalculateTile(room.roomData[i,j]));
                }
            }
        }
    }

    private TileBase CalculateTile(DungeonRoom.tile tileType)
    {
        TileBase floorTileToUse = null;

        if (tileType == DungeonRoom.tile.floor)
        {
            int randInt = Random.Range(0, 20);
            floorTileToUse = (randInt < 1) ? crackedFloorTile : floorTile;
        }

        return (tileType == DungeonRoom.tile.dirt) ? dirtTile : (tileType == DungeonRoom.tile.backWall) ? backWallTile : (tileType == DungeonRoom.tile.sideWall) ? sideWallTile : floorTileToUse;
    }

    private void PlaceTile(Vector3Int position, TileBase tile)
    {
        tilemap.SetTile(position, tile);
    }
}
