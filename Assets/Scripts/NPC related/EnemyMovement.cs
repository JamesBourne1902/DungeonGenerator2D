using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Bounds movementBounds;
    Vector3 movementTargetLocation;
    Vector3 movementDirection;

    public float velocity;
    public float minimumDistanceThreshold;

    private void Update()
    {
        CheckMovementTargetDistance();
        transform.position += movementDirection * velocity * Time.deltaTime;
        KeepEntityInsideBounds();
    }

    public void SpawnSetup(Bounds roomBounds)
    {
        movementBounds = roomBounds;
        transform.position = roomBounds.center;
        movementTargetLocation = transform.position;
    }

    private void CheckMovementTargetDistance()
    {
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        float distanceToTargetLocation = Mathf.Abs((transform.position - movementTargetLocation).magnitude);
        float distanceToPlayerLocation = Mathf.Abs((transform.position - playerPosition).magnitude);

        ChangeMovementTargetPosition((distanceToPlayerLocation < minimumDistanceThreshold) ? transform.position : (movementBounds.Contains(playerPosition)) ? playerPosition : (distanceToTargetLocation < 1f) ? RandomLocationWithinBounds() : movementTargetLocation);
    }

    private void ChangeMovementTargetPosition(Vector3 targetPosition)
    {
        movementTargetLocation = targetPosition;

        movementDirection = (movementTargetLocation - transform.position).normalized;
    }

    private Vector3 RandomLocationWithinBounds()
    {
        float XBoundSize = movementBounds.size.x;
        float YBoundSize = movementBounds.size.y;
        float xCoord = Random.Range(movementBounds.center.x - XBoundSize / 2f, movementBounds.center.x + XBoundSize / 2f);
        float yCoord = Random.Range(movementBounds.center.y - YBoundSize / 2f, movementBounds.center.y + YBoundSize / 2f);

        return new Vector3(xCoord, yCoord, 0);
    }

    private void KeepEntityInsideBounds()
    {
        if (!movementBounds.Contains(transform.position))
        {
            transform.position = movementBounds.ClosestPoint(transform.position);
        }
    }
}
