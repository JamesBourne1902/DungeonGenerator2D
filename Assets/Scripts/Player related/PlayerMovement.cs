using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    #region speeds

    public float maxPlayerWalkSpeed;
    public float maxPlayerRunSpeed;
    public float playerAcceleration;
    public float playerDecceleration;
    private float currentPlayerSpeed = 0;

    #endregion

    public KeyCode runKey;
    private bool playerIsRunning;
    private bool playerAtMaxSpeed;
    private Vector3 currentMovementDirection = Vector3.zero;

    #region stamina

    public GameObject staminaBar;
    public Text staminaText;
    public int maxStamina;
    public float currentStamina;
    public int staminaConsumptionRate;

    #endregion

    #region Walking Bounds

    public static HashSet<Bounds> roomConnectionBounds = new HashSet<Bounds>();
    public static Bounds boundsCurrentlyContainingPlayer;
    public static DungeonRoom roomCurrentlyContainingPlayer;

    #endregion

    private void Start()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        CalculatePlayerMovementDirection();
        ChangePlayerVelocity();
        AttemptToRestoreStamina();

        if (currentPlayerSpeed != 0 && !PlayerHealth.dead)
        {
            MovePlayer();
        }
    }

    // calculates the direction vector to use for the players movement from the vertical and horizontal inputs
    private void CalculatePlayerMovementDirection()
    {
        float upOrDownMovement = (Input.GetAxisRaw("Vertical") > 0) ? 1: (Input.GetAxisRaw("Vertical") < 0) ? -1 : 0;
        float leftOrRightMovement = (Input.GetAxisRaw("Horizontal") > 0) ? 1 : (Input.GetAxisRaw("Horizontal") < 0) ? -1 : 0;

        currentMovementDirection = (upOrDownMovement == 0 && leftOrRightMovement == 0) ? currentMovementDirection : new Vector3(leftOrRightMovement, upOrDownMovement, 0).normalized;
    }

    // accelerates and decelerates the player
    // the player decelerates if there are no movement inputs and the velocity != 0
    // the player accelerates to a maximum speed. this max speed changes if the player is sprinting
    private void ChangePlayerVelocity()
    {
        playerIsRunning = PlayerIsRunning();
        float maxSpeed = (!playerIsRunning) ? maxPlayerWalkSpeed : maxPlayerRunSpeed;

        if ((Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0) && !playerAtMaxSpeed)
        {
            currentPlayerSpeed += playerAcceleration * Time.deltaTime;
            currentPlayerSpeed = Mathf.Clamp(currentPlayerSpeed, 0, maxSpeed);
        }
        else if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0 && currentPlayerSpeed != 0)
        {
            currentPlayerSpeed -= playerDecceleration * Time.deltaTime;
            currentPlayerSpeed = Mathf.Clamp(currentPlayerSpeed, 0, maxSpeed);
        }

        playerAtMaxSpeed = (currentPlayerSpeed == maxSpeed) ? true : false;
    }

    private void MovePlayer()
    {
        transform.position += (currentMovementDirection * currentPlayerSpeed * Time.deltaTime);
        Vector3 closestBoundPosition;

        if (!PlayerIsInBounds(out closestBoundPosition))
        {
            gameObject.transform.position = closestBoundPosition;
        }
    }

    private bool PlayerIsInBounds(out Vector3 closestBoundPosition)
    {
        float closestDist = float.MaxValue;
        closestBoundPosition = Vector3.zero;

        for (int i = 0; i < DungeonRenderer.dungeonRooms.Count; i++)
        {
            Bounds bounds1 = DungeonRenderer.dungeonRooms[i].playerRoomBoundary;

            if (bounds1.Contains(transform.position))
            {
                boundsCurrentlyContainingPlayer = bounds1;
                roomCurrentlyContainingPlayer = DungeonRenderer.dungeonRooms[i];
                return true;
            }
            else
            {
                float dist = (bounds1.ClosestPoint(transform.position) - transform.position).magnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBoundPosition = bounds1.ClosestPoint(transform.position);
                }
            }
        }

        foreach (Bounds bounds2 in roomConnectionBounds)
        {
            if (bounds2.Contains(transform.position))
            {
                boundsCurrentlyContainingPlayer = bounds2;
                return true;
            }
            else
            {
                float dist = (bounds2.ClosestPoint(transform.position) - transform.position).magnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBoundPosition = bounds2.ClosestPoint(transform.position);
                }
            }
        }

        return false;
    }

    private bool PlayerIsRunning()
    {
        if (Input.GetKey(runKey) && currentStamina > 0)
        {
            DrainStamina(staminaConsumptionRate * Time.deltaTime);
            return true;
        }

        return false;
    }

    private void StaminaBarChanges()
    {
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        float scale = currentStamina / (float)maxStamina;
        staminaBar.transform.localScale = new Vector3(scale, 1, 1);
        staminaText.text = Mathf.Round(scale * 1000)/10 + "%";
    }

    private void AttemptToRestoreStamina()
    {
        if (!Input.GetKey(runKey) && currentStamina < maxStamina)
        {
            DrainStamina(-staminaConsumptionRate * Time.deltaTime);
        }
    }

    public void DrainStamina(float quantity)
    {
        currentStamina -= quantity;
        StaminaBarChanges();
    }
}
