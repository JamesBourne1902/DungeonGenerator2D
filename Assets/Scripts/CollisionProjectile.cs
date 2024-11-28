using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProjectile : MonoBehaviour
{
    private Vector3 dir;

    public void MoveProjectile(Vector3 fromPos, Vector3 toPos, float speed)
    {
        CalculateDirectionOfFlight(fromPos, toPos);
        ChangeOrientation();
        StartCoroutine(Fly(speed));
    }

    // calculates the time of flight from the distance and speed of the projectile
    // then moves the projectile at a constant speed to the target position
    // when at the target position, it will stop moving.
    private IEnumerator Fly(float speed)
    {
        float duration = 10f;
        float timer = 0f;

        while (timer <= duration)
        {
            transform.position += dir * speed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ChangeOrientation()
    {
        float angle = Vector3.Angle(dir, Vector3.right);

        transform.Rotate(new Vector3(0, 0, (dir.y < 0) ? 360 - angle : angle));
    }

    private void CalculateDirectionOfFlight(Vector3 fromPos, Vector3 toPos)
    {
        dir = (toPos - fromPos).normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<IsDamageable>().TakeDamage(2);
            Destroy(gameObject);
        }
    }
}
