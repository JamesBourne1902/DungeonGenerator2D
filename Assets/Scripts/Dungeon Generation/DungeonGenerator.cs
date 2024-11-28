using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Range(1,20)]
    public int numberOfMainRooms;
    [Range (1,20)]
    public int maxNumberOfSecondRooms;
    [Range (15,50)]
    public int maxRoomSize;
    [Range(5,15)]
    public int minRoomSize;
    [Range (4,30)]
    public int bossRoomSize;

    public GameObject gridCell;
    public Vector2Int startPosition;

    public enum directions { north, east, south, west };
    private directions primaryDirection;
    private enum dungeonBranch { primary, secondary, tertiary };
    private Dictionary<int, DungeonRoom> mainDungeonRooms = new Dictionary<int, DungeonRoom>();

    private int furthestRoomFromStartDist;
    private DungeonRoom furthestRoomFromStart;
    private directions finalRoomDirection;

    private int roomIndex;


    public void GenerateDungeonData()
    {
        DestroyOldDungeonData();

        GeneratePrimaryDungeonBranch();
        GenerateSecondaryDungeonBranches();
        GenerateBossRoom();
    }

    private void GeneratePrimaryDungeonBranch()
    {
        DungeonRoomInfo roomInfo = new DungeonRoomInfo(maxRoomSize, minRoomSize, false);
        primaryDirection = ReturnRandomDirection(directions.west, false);
        Vector2Int localOrigin = startPosition;

        DungeonRoom currentRoom = new DungeonRoom(localOrigin, roomInfo);
        currentRoom.PlaceExit(primaryDirection);
        DungeonRenderer.dungeonRooms.Add(roomIndex, currentRoom);
        roomIndex++;

        for (int i = 1; i <= numberOfMainRooms; i++)
        {
            DungeonRoom previousRoom = currentRoom;
            currentRoom = GenerateDungeonRoom(previousRoom, primaryDirection);

            if (currentRoom != null)
            {
                previousRoom.PlaceExit(primaryDirection);
                currentRoom.PlaceExit(ReturnOppositeDirection(primaryDirection));
                DungeonRenderer.dungeonRooms.Add(roomIndex, currentRoom);
                roomIndex++;
                mainDungeonRooms.Add(i, currentRoom);

                if (i == numberOfMainRooms)
                {
                    furthestRoomFromStart = currentRoom;
                    furthestRoomFromStartDist = i;
                    finalRoomDirection = primaryDirection;
                }

            }
        }
    }

    private void GenerateSecondaryDungeonBranches()
    {
        int mainRoomIndex = 1;

        while (mainRoomIndex <= mainDungeonRooms.Count)
        {
            mainRoomIndex += Random.Range(1, 5);

            directions secondaryDirection = ReturnRandomDirection(primaryDirection, true);

            if (mainDungeonRooms.TryGetValue(Mathf.Clamp(mainRoomIndex, 1, mainDungeonRooms.Count), out DungeonRoom value))
            {
                DungeonRoom previousBranchRoom = value;
                int branchLength = Random.Range(1, maxNumberOfSecondRooms);

                for (int j = 1; j <= branchLength; j++)
                {
                    DungeonRoom currentBranchRoom = GenerateDungeonRoom(previousBranchRoom, secondaryDirection);

                    if (currentBranchRoom == null && j == 0)
                    {
                        secondaryDirection = ReturnOppositeDirection(secondaryDirection);
                        currentBranchRoom = GenerateDungeonRoom(previousBranchRoom, secondaryDirection);
                    }

                    if (currentBranchRoom != null)
                    {
                        previousBranchRoom.PlaceExit(secondaryDirection);
                        currentBranchRoom.PlaceExit(ReturnOppositeDirection(secondaryDirection));
                        DungeonRenderer.dungeonRooms.Add(roomIndex, currentBranchRoom);
                        roomIndex++;
                        previousBranchRoom = currentBranchRoom;
                        CheckRoomIsFurtherstFromStart(j + mainRoomIndex, currentBranchRoom, secondaryDirection);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    private void GenerateBossRoom()
    {
        DungeonRoomInfo bossRoomInfo = new DungeonRoomInfo(bossRoomSize, minRoomSize, true);
        Vector2Int localOrigin = CalculateNextRoomsLocalOrigin(furthestRoomFromStart, finalRoomDirection, bossRoomInfo);
        DungeonRoom bossRoom = new DungeonRoom(localOrigin, bossRoomInfo);
        bossRoom.PlaceExit(ReturnOppositeDirection(finalRoomDirection));
        furthestRoomFromStart.PlaceExit(finalRoomDirection);

        DungeonRenderer.dungeonRooms.Add(roomIndex, bossRoom);
    }

    private DungeonRoom GenerateDungeonRoom(DungeonRoom prevRoom, directions branchDirection)
    {
        DungeonRoomInfo roomInfo = new DungeonRoomInfo(maxRoomSize, minRoomSize, false);
        Vector2Int localOrigin = CalculateNextRoomsLocalOrigin(prevRoom, branchDirection, roomInfo);
        DungeonRoom currentRoom = new DungeonRoom(localOrigin, roomInfo);

        if (currentRoom.wipeRoom)
        {
            currentRoom = null;
        }

        return currentRoom;
    }

    public void DestroyOldDungeonData()
    {
        try
        {
            DungeonRenderer.usedTilePositions.Clear();
            DungeonRenderer.dungeonRooms.Clear();
            PlayerAttack.projectileRoomBounds.Clear();
            mainDungeonRooms.Clear();
            furthestRoomFromStartDist = 0;
            furthestRoomFromStart = null;
            roomIndex = 0;
        }
        catch
        {

        }
    }

    private void CheckRoomIsFurtherstFromStart(int roomDistFromStart, DungeonRoom room, directions branchDirection)
    {
        if (roomDistFromStart > furthestRoomFromStartDist)
        {
            furthestRoomFromStart = room;
            furthestRoomFromStartDist = roomDistFromStart;
            finalRoomDirection = branchDirection;
        }
    }

    private Vector2Int CalculateNextRoomsLocalOrigin(DungeonRoom currentRoom, directions directionOfBranch, DungeonRoomInfo nextRoomSizeInfo)
    {
        Vector2Int nextRoomEntrancePosition = currentRoom.ReturnNextRoomEntrancePosition(directionOfBranch);
        Vector2Int nextRoomLocalOrigin;

        if (directionOfBranch == directions.north)
        {
            nextRoomLocalOrigin = nextRoomEntrancePosition - new Vector2Int(1 + nextRoomSizeInfo.southExitHorizontalPosition, 0);
        }
        else if (directionOfBranch == directions.south)
        {
            nextRoomLocalOrigin = nextRoomEntrancePosition - new Vector2Int(1 + nextRoomSizeInfo.northExitHorizontalPosition, nextRoomSizeInfo.roomHeight);
        }
        else if (directionOfBranch == directions.east)
        {
            nextRoomLocalOrigin = nextRoomEntrancePosition - new Vector2Int(0, 1 + nextRoomSizeInfo.westExitVerticalPosition);
        }
        else
        {
            nextRoomLocalOrigin = nextRoomEntrancePosition - new Vector2Int(nextRoomSizeInfo.roomWidth, 1 + nextRoomSizeInfo.eastExitVerticalPosition);
        }

        return nextRoomLocalOrigin;
    }

    private directions ReturnRandomDirection(directions excludedDirection, bool excludeReverseDirection)
    {
        System.Array directionsArray = System.Enum.GetValues(typeof(directions));
        directions newDirection = (directions)directionsArray.GetValue(Random.Range(0, directionsArray.Length));
        directions reverseDirection = ReturnOppositeDirection(excludedDirection);


        if (newDirection == excludedDirection || (excludeReverseDirection && newDirection == reverseDirection))
        {
            return ReturnRandomDirection(excludedDirection, excludeReverseDirection);
        }
        else
        {
            return newDirection;
        }
    }

    private directions ReturnOppositeDirection(directions direction)
    {
        return (direction == directions.north) ? directions.south : (direction == directions.south) ? directions.north : (direction == directions.east) ? directions.west : directions.east;
    }
}

public struct DungeonRoomInfo
{
    public int roomWidth;
    public int roomHeight;
    public int northExitHorizontalPosition;
    public int eastExitVerticalPosition;
    public int southExitHorizontalPosition;
    public int westExitVerticalPosition;

    public DungeonRoomInfo(int maxSize, int minSize, bool forceMaxSize)
    {
        roomHeight = (!forceMaxSize) ? Random.Range(minSize, maxSize) : maxSize;
        roomWidth = (!forceMaxSize) ? Random.Range(minSize, maxSize) : maxSize;
        northExitHorizontalPosition = Random.Range(1, roomWidth - 1);
        eastExitVerticalPosition = Random.Range(1, roomHeight - 1);
        southExitHorizontalPosition = Random.Range(1, roomWidth - 1);
        westExitVerticalPosition = Random.Range(1, roomHeight - 1);
    }
}

public class DungeonRoom
{
    public enum tile { floor, backWall, sideWall, dirt}

    public DungeonRoomInfo roomInfo;

    public tile[,] roomData;
    public Vector2Int[,] roomTilePositions;

    private HashSet<Vector2Int> tilesAddedToHashset = new HashSet<Vector2Int>();
    public bool wipeRoom;

    public Bounds playerRoomBoundary;
    public Bounds projectileRoomBoundary;


    public DungeonRoom(Vector2Int bottomLeftPosition, DungeonRoomInfo roomInfo)
    {
        this.wipeRoom = false;
        this.roomInfo = roomInfo;

        roomData = new tile[roomInfo.roomHeight + 2, roomInfo.roomWidth + 2];
        roomTilePositions = new Vector2Int[roomInfo.roomHeight + 2, roomInfo.roomWidth + 2];
        roomTilePositions[0, 0] = bottomLeftPosition;

        DefineRoomPositions();
        if (!wipeRoom)
        {
            FillRoom();
            DefineBounds();
        }
    }

    // the constructor for creating a copy of an instance of this class
    public DungeonRoom(DungeonRoom dungeonRoom)
    {
        this.roomData = dungeonRoom.roomData;
        this.roomTilePositions = dungeonRoom.roomTilePositions;
        this.roomInfo = dungeonRoom.roomInfo;
    }

    private void DefineRoomPositions()
    {
        for (int i = 0; i < roomTilePositions.GetLength(0); i++)
        {
            for (int j = 0; j < roomTilePositions.GetLength(1); j++)
            {
                if (i != 0 || j != 0)
                {
                    roomTilePositions[i, j] = roomTilePositions[0, 0] + new Vector2Int(j, i);
                    if (!TilePositionIsValid(roomTilePositions[i, j]))
                    {
                        wipeRoom = true;

                        goto EndLoops;
                    }
                }
            }
        }
    EndLoops:;
    }

    private bool TilePositionIsValid(Vector2Int tilePosition)
    {
        if (DungeonRenderer.usedTilePositions.Contains(tilePosition))
        {
            ClearThisRoomsTilesFromHashset();
            return false;
        }
        else
        {
            AddTileToHashSet(tilePosition);
            return true;
        }
    }

    private void ClearThisRoomsTilesFromHashset()
    {
        foreach (Vector2Int pos in tilesAddedToHashset)
        {
            DungeonRenderer.usedTilePositions.Remove(pos);
        }
    }

    private void AddTileToHashSet(Vector2Int tilePosition)
    {
        tilesAddedToHashset.Add(tilePosition);
        DungeonRenderer.usedTilePositions.Add(tilePosition);
    }

    public void PlaceExit(DungeonGenerator.directions exit)
    {
        if (exit == DungeonGenerator.directions.north)
        {
            roomData[roomData.GetLength(0) - 1, 1 + roomInfo.northExitHorizontalPosition] = tile.floor;
            PlaceExitBounds(roomData.GetLength(0) - 1, 1 + roomInfo.northExitHorizontalPosition, exit);
        }
        else if (exit == DungeonGenerator.directions.south)
        {
            roomData[0, 1 + roomInfo.southExitHorizontalPosition] = tile.floor;
            roomData[0, roomInfo.southExitHorizontalPosition] = tile.sideWall;
            roomData[0, 2 + roomInfo.southExitHorizontalPosition] = tile.sideWall;
            PlaceExitBounds(0, 1 + roomInfo.southExitHorizontalPosition, exit);
        }
        else if (exit == DungeonGenerator.directions.east)
        {
            roomData[1 + roomInfo.eastExitVerticalPosition, roomData.GetLength(1) - 1] = tile.floor;
            roomData[2 + roomInfo.eastExitVerticalPosition, roomData.GetLength(1) - 1] = tile.backWall;
            PlaceExitBounds(1 + roomInfo.eastExitVerticalPosition, roomData.GetLength(1) - 1, exit);
        }
        else
        {
            roomData[1 + roomInfo.westExitVerticalPosition, 0] = tile.floor;
            roomData[2 + roomInfo.westExitVerticalPosition, 0] = tile.backWall;
            PlaceExitBounds(1 + roomInfo.westExitVerticalPosition, 0, exit);
        }
    }

    private void FillRoom()
    {
        for (int i = 0; i < roomData.GetLength(0); i++)
        {
            for (int j = 0; j < roomData.GetLength(1); j++)
            {
                roomData[i, j] = (i == 0) ? tile.dirt : (j == 0 || j == roomData.GetLength(1) - 1) ? tile.sideWall : (i == roomData.GetLength(0) - 1) ? tile.backWall : tile.floor;
            }
        }
    }

    // takes in a direction for the exit of the current room to use
    // outputs the coordinates of the next rooms entrance such that it will allign with the input exit
    public Vector2Int ReturnNextRoomEntrancePosition(DungeonGenerator.directions currentRoomExitDirection)
    {
        Vector2Int position;

        if (currentRoomExitDirection == DungeonGenerator.directions.north)
        {
            position = roomTilePositions[roomData.GetLength(0) - 1, 1 + roomInfo.northExitHorizontalPosition];
            position += new Vector2Int(0, 1);
        }
        else if (currentRoomExitDirection == DungeonGenerator.directions.south)
        {
            position = roomTilePositions[0, 1 + roomInfo.southExitHorizontalPosition];
            position += new Vector2Int(0, -2);
        }
        else if (currentRoomExitDirection == DungeonGenerator.directions.east)
        {
            position = roomTilePositions[1 + roomInfo.eastExitVerticalPosition, roomData.GetLength(1) - 1];
            position += new Vector2Int(1, 0);
        }
        else
        {
            position = roomTilePositions[1+roomInfo.westExitVerticalPosition, 0];
            position += new Vector2Int(-2, 0);
        }

        return position;
    }

    private Vector3 CalculateRoomCentre()
    {
        float xCentreCoord = Mathf.Lerp(roomTilePositions[0, 0].x, roomTilePositions[roomTilePositions.GetLength(0)-1, roomTilePositions.GetLength(1)-1].x, 0.5f);
        float yCentreCoord = Mathf.Lerp(roomTilePositions[0, 0].y, roomTilePositions[roomTilePositions.GetLength(0) - 1, roomTilePositions.GetLength(1) - 1].y, 0.5f);


        return new Vector3(xCentreCoord, yCentreCoord, 0);
    }

    private void DefineBounds()
    {
        Vector3 roomCentre = CalculateRoomCentre() + new Vector3(0, 0.25f, 0);
        Vector3 roomSize = new Vector3(roomInfo.roomWidth-1, roomInfo.roomHeight-0.5f, 1f);
        Vector3 projectileRoomSize = new Vector3(roomInfo.roomWidth, roomInfo.roomHeight + 0.5f, 1f);

        playerRoomBoundary = new Bounds(roomCentre, roomSize);

        projectileRoomBoundary = (new Bounds(roomCentre, projectileRoomSize));
    }

    private void PlaceExitBounds(int row, int col, DungeonGenerator.directions dir)
    {
        Vector3 roomCentre = new Vector3(roomTilePositions[row, col].x, roomTilePositions[row, col].y, 0);
        Vector3 roomSize = (dir == DungeonGenerator.directions.north || dir == DungeonGenerator.directions.south)? new Vector3(0.35f, 2.05f, 1f): new Vector3(2f, 0.55f, 1f);

        if (dir == DungeonGenerator.directions.east || dir == DungeonGenerator.directions.west)
        {
            roomCentre += new Vector3(0, 0.22f, 0);
        }

        Bounds roomBoundary = new Bounds(roomCentre, roomSize);

        PlayerMovement.roomConnectionBounds.Add(roomBoundary);
    }
}