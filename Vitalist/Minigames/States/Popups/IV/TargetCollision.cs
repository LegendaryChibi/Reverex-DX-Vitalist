using System;
using UnityEngine;

public class TargetCollision : MonoBehaviour
{
    [SerializeField] private GameObject IVParent;

    public static NetAction onNeedleCollision = new NetAction("net_iv_needle_collision");

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IVParent.activeInHierarchy)
        {
            onNeedleCollision.Invoke(() => Whoami.AmIP2());
        }
    }
}
