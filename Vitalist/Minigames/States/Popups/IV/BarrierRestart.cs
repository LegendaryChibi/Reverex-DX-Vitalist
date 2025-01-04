using System;
using UnityEngine;

public class BarrierRestart : MonoBehaviour
{
    [SerializeField] private GameObject IVParent;

    public static NetAction onBarrierCollision = new NetAction("net_iv_barrier_collision");

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IVParent.activeInHierarchy)
        {
            onBarrierCollision.Invoke(() => Whoami.AmIP2());
        }
    }
}
