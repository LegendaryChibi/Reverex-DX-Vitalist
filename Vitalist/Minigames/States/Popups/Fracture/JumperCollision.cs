using System;
using UnityEngine;

public class JumperCollision : MonoBehaviour
{
    [SerializeField] private GameObject fractureParent;

    public static NetAction<String> onJumperCollision = new NetAction<String>("net_fracture_jump_collide");

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (fractureParent.activeInHierarchy)
        {
            onJumperCollision.Invoke(() => Whoami.AmIP2(), collision.name);
        }
    }
}
