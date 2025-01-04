using System;
using UnityEngine;

public class PillCollision : MonoBehaviour
{
    [SerializeField] private GameObject adrenalineParent;

    public static NetAction<String> onPillCollision = new NetAction<String>("net_adrenaline_pill_collision");

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (adrenalineParent.activeInHierarchy)
        {
            onPillCollision.Invoke(() => Whoami.AmIP2(), other.name);
        }
    }
}
