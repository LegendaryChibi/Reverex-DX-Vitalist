using UnityEngine;

public class RunnerCollision : MonoBehaviour
{
    [SerializeField] private GameObject exhaustionParent;

    public static NetAction<String> onRunnerCollsion = new NetAction<String>("net_exhaustion_collision");

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (exhaustionParent.activeInHierarchy)
        {
            onRunnerCollsion.Invoke(() => Whoami.AmIP2(), collision.name);
        }
    }
}
