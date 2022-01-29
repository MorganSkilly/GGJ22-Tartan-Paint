using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    public float health = 100f;
    private float healthLast;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health < healthLast)
            Debug.Log(gameObject.name + "'s health is: " + health);
        healthLast = health;
    }
}
