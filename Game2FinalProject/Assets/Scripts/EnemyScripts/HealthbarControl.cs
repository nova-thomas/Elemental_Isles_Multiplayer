using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;
using Unity.VisualScripting;

public class HealthbarControl : MonoBehaviour
{
    public Image healthBarFill;
    public Enemy enemy;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        UpdateHealthBar();
    }

    private void Update()
    {
            transform.LookAt(Camera.main.transform.position, Vector3.up);
    }


    public void UpdateHealthBar()
    {
        // Update the fill amount of the health bar
        float fill = enemy.health / enemy.maxHealth;
        healthBarFill.fillAmount = fill;
    }
}
