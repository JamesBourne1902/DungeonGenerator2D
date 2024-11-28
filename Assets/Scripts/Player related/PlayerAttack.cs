using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int staminaCost;
    public static Bounds currentRoomBounds;
    public static HashSet<Bounds> projectileRoomBounds = new HashSet<Bounds>();
    public LayerMask layersToScanForAttacking;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !PlayerHealth.dead)
        {
            TargetInfo info = CalculateTargetInfo();
            FireProjectile(info);
        }
    }

    // creates a projectile gameObject at the player position and calls a method from the projectile script attached to it
    private void FireProjectile(TargetInfo targetInfo)
    {
        PlayerMovement stamina = gameObject.GetComponent<PlayerMovement>();

        if (stamina.currentStamina >= staminaCost)
        {
            GameObject arrow = ArrowPooler.instance.GetObject();
            arrow.GetComponent<HitScanProjectile>().MoveProjectile(targetInfo, transform.position);
            stamina.DrainStamina(staminaCost);
        }
    }

    // returns the world coordinates of the current mouse position
    // need to set the z position to be the distance between the camera and the plane of the game objects
    private Vector3 ReturnMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = 10;
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    // this method is passed into the BoundsThread class to be run on a separate thread
    // it takes the target position of the projectile and determines if it's within the dungeon bounds
    public Bounds LocateCurrentRoomBoundsThread(HashSet<Bounds> boundHashset, Vector3 targetPos)
    {
        Bounds roomBounds = new Bounds();

        foreach (Bounds bounds in boundHashset)
        {
            if (bounds.Contains(targetPos))
            {
                roomBounds = bounds;
                break;
            }
        }

        return roomBounds;
    }

    // returns the TargetInfo struct data type. this contains a reference to the target location and enemy GameObject
    // if no enemy is detected then the enemy GameObject is returned as null
    // if the target position is outside the room then the rooms edge between the player and the target is returned as the target position
    private TargetInfo CalculateTargetInfo()
    {
        Vector3 mousePos = ReturnMouseWorldPosition();

        if (EnemyDetected(mousePos, out TargetInfo enemyInfo))
        {
            return enemyInfo;
        }
        else if (PlayerMovement.roomCurrentlyContainingPlayer.projectileRoomBoundary.Contains(mousePos))
        {
            return new TargetInfo(null, mousePos);
        }
        else
        {
            return new TargetInfo(null, CalculateBoundEdgePosition(mousePos));
        }
    }

    // Calculates the position of the bound edge in the direction of the target
    // used to change the target position to be at the bounds edge if the target is outside of the bounds
    // 'dir' is the direction vector from the mouse position to the player
    public Vector3 CalculateBoundEdgePosition(Vector3 currentTargetPos)
    {
        Vector3 newTargetPos = currentTargetPos;
        Vector3 dir = (transform.position - currentTargetPos).normalized;

        Ray ray = new Ray(currentTargetPos, dir);
        float distanceFromTargetPositionToBoundsEdge;

        if (PlayerMovement.roomCurrentlyContainingPlayer.projectileRoomBoundary.IntersectRay(ray, out distanceFromTargetPositionToBoundsEdge))
        {
            newTargetPos += (dir * distanceFromTargetPositionToBoundsEdge);
        }

        return newTargetPos;
    }

    // fills the hashset containing the projectile Bounds
    // using the player movement bounds was not suitable determining projectiles movement limits
    public static void DefineBoundsHashset()
    {
        for (int i = 0; i < DungeonRenderer.dungeonRooms.Count; i++)
        {
            projectileRoomBounds.Add(DungeonRenderer.dungeonRooms[i].projectileRoomBoundary);
        }
    }

    // performs a 2D raycast to detected if any enemies are in the path of the projectile
    // this is more effective for high speed projectiles than using Unity's colliders
    private bool EnemyDetected(Vector3 targetPos, out TargetInfo targetInfo)
    {
        targetInfo = new TargetInfo(null, Vector3.zero);

        Vector3 origin = transform.position;
        Vector3 dir = (targetPos - origin).normalized;
        float rayDist = Mathf.Abs((targetPos - origin).magnitude);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayDist, layersToScanForAttacking);

        if (hit.collider != null && hit.collider.gameObject.layer == 6)
        {
            targetInfo.position = hit.collider.gameObject.transform.position;
            targetInfo.enemy = hit.collider.gameObject;
            return true;
        }

        return false;
    }
}

public struct TargetInfo
{
    public GameObject enemy;
    public Vector3 position;

    public TargetInfo(GameObject enemy, Vector3 position)
    {
        this.enemy = enemy;
        this.position = position;
    }
}
