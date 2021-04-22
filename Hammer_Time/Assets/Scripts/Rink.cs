using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rink : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collided with" + other.gameObject);
    }
}
