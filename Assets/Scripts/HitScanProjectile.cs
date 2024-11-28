using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HitScanProjectile : MonoBehaviour
{
    public float speed;
    public int damage;
    private bool isCollectable;

    public void MoveProjectile(TargetInfo targetInfo, Vector3 fromPos)
    {
        ApplyTransformChanges(fromPos, targetInfo.position);
        StartCoroutine(Fly(targetInfo, fromPos));
    }

    // calculates the time of flight from the distance and speed of the projectile
    // then moves the projectile at a constant speed to the target position
    // when at the target position, it will stop moving.
    private IEnumerator Fly(TargetInfo targetInfo, Vector3 fromPos)
    {
        float dist = DistanceFromPosition(targetInfo.position);
        float duration = dist/speed;
        float timer = 0f;

        while (timer <= duration)
        {
            transform.position = Vector3.Lerp(fromPos, targetInfo.position, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = targetInfo.position;
        DamageTargetObject(targetInfo.enemy);
        isCollectable = true;
    }

    private void ApplyTransformChanges(Vector3 fromPos, Vector3 toPos)
    {
        Vector3 dir = (toPos - fromPos).normalized;
        float angle = Vector3.Angle(dir, Vector3.right);

        transform.Rotate(new Vector3(0, 0, (dir.y < 0) ? 360 - angle : angle));
        transform.position = fromPos;
    }

    // if the projectile hits an object, looks for the 'isDamageable' interface and applies it's 'TakeDamage' method
    // also sets the parent of the projectile to that target. (makes the arrow 'stick' to the target)
    private void DamageTargetObject(GameObject target)
    {
        if (target != null)
        {
            transform.parent = target.transform;
            target.GetComponent<IsDamageable>().TakeDamage(damage);
        }
    }

    private void ResetArrow()
    {
        transform.rotation = Quaternion.identity;
        isCollectable = false;
    }

    private float DistanceFromPosition(Vector3 targetPos)
    {
        return Mathf.Abs((transform.position - targetPos).magnitude);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3 && isCollectable)
        {
            ResetArrow();
            ArrowPooler.instance.ReturnObject(gameObject);
        }
    }
}
