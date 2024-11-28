using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IsDamageable
{
    public int maxHealth;
    private int currentHealth;

    public GameObject healthbar;
    public Text healthText;

    public GameObject deathScreen;
    public static bool dead;

    private void Start()
    {
        currentHealth = maxHealth;
        dead = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        HealthBarChanges();

        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    private void HealthBarChanges()
    {
        float scale = Mathf.Clamp01(currentHealth / (float)maxHealth);
        healthbar.transform.localScale = new Vector3(scale, 1, 1);
        healthText.text = Mathf.Round(scale*100) + "%";
    }

    private void OnDeath()
    {
        dead = true;
        deathScreen.SetActive(true);
    }
}
