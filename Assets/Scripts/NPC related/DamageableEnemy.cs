using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageableEnemy : MonoBehaviour, IsDamageable
{
    public GameObject healthBar;

    public int maxHealth;
    private int currentHealth;
    public float healthBarShrinkDuration;

    private Coroutine currentShrinkCoroutine;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // this method is from the IsDamageable interface
    // it's called whenever the entity is hit with a weapon, reducing the health by the damage input
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        ApplyHealthBarChanges();

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    // calculates the new size of the healthbar
    // this size is instantly applied to the main part of the healthbar
    // then a coroutine is started to change the size of the background healthbar smoothly
    private void ApplyHealthBarChanges()
    {
        float toScale = currentHealth / (float)maxHealth;

        healthBar.transform.GetChild(1).localScale = new Vector3(toScale, 1, 1);

        if (currentShrinkCoroutine != null)
        {
            StopCoroutine(currentShrinkCoroutine);
        }

        currentShrinkCoroutine = StartCoroutine(ShrinkHealthBar(toScale));
    }

    // smoothly (through time) changes the size of the healthbar background
    private IEnumerator ShrinkHealthBar(float toScale)
    {
        Image healthBarBackground = healthBar.transform.GetChild(0).gameObject.GetComponent<Image>();
        float fromScale = healthBarBackground.transform.localScale.x;
        float timer = 0;

        while (timer < healthBarShrinkDuration)
        {
            float xScale = Mathf.Lerp(fromScale, toScale, timer/healthBarShrinkDuration);
            healthBarBackground.transform.localScale = new Vector3(xScale, 1, 1);
            timer += Time.deltaTime;
            yield return null;
        }

        healthBarBackground.transform.localScale = new Vector3(toScale, 1, 1);
    }
}
