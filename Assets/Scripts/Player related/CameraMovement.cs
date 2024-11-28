using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float playerCentreOffset;
    public float walkCameraMoveDelay;
    public float runCameraMoveDelay;

    private float currentMoveDelay;
    private Vector3 currentPosition = Vector3.zero;
    private Coroutine currentMovementCoroutine;

    private enum movementType { Default, lookAhead}
    [SerializeField]
    private movementType cameraMovement;

    private void Update()
    {
        if (!PlayerHealth.dead && cameraMovement == movementType.lookAhead)
        {
            CheckCameraPosition();
        }
    }

    private void CheckCameraPosition()
    {
        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            float moveDelay = (Input.GetKey(KeyCode.LeftShift)) ? runCameraMoveDelay : walkCameraMoveDelay;
            Vector3 newPosition = PositionCameraShouldBe();

            if (newPosition != currentPosition || moveDelay != currentMoveDelay)
            {
                currentMoveDelay = moveDelay;
                currentPosition = newPosition;

                if (currentMovementCoroutine != null)
                {
                    StopCoroutine(currentMovementCoroutine);
                }

                currentMovementCoroutine = StartCoroutine(MoveCameraPosition(transform.localPosition, newPosition));
            }
        }
        else if (currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
            currentMovementCoroutine = null;
        }
    }

    // returns the local position that the camera should be in based on the player's inputs
    private Vector3 PositionCameraShouldBe()
    {
        int upOrDownMovement = (Input.GetAxisRaw("Vertical") > 0) ? 1 : (Input.GetAxisRaw("Vertical") < 0) ? -1 : 0;
        int leftOrRightMovement = (Input.GetAxisRaw("Horizontal") > 0) ? 1 : (Input.GetAxisRaw("Horizontal") < 0) ? -1 : 0;
        Vector2 newPosition = new Vector2(leftOrRightMovement, upOrDownMovement).normalized * playerCentreOffset;

        return new Vector3(newPosition.x, newPosition.y, -10);
    }

    private IEnumerator MoveCameraPosition(Vector3 fromPosition, Vector3 toPosition)
    {
        float timer = 0;

        while (timer < currentMoveDelay)
        {
            transform.localPosition = Vector3.Lerp(fromPosition, toPosition, timer / currentMoveDelay);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = toPosition;
        currentMovementCoroutine = null;
    }
}
