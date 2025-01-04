using UnityEngine;

public class GermCollision : MonoBehaviour
{
    public static Collider2D currentCollision;
    [SerializeField] private GameObject antibioticsParent;

    public static NetAction<String> onGermCollided = new NetAction<String>("net_antibiotics_germ_collided");

    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentCollision = collision;
        if (antibioticsParent.activeInHierarchy)
        {
            onGermCollided.Invoke(() => Whoami.AmIP2(), currentCollision.name);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        currentCollision = collision;
        if (antibioticsParent.activeInHierarchy)
        {
            onGermCollided.Invoke(() => Whoami.AmIP2(), currentCollision.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        currentCollision = null;
        if (antibioticsParent.activeInHierarchy)
        {
            onGermCollided.Invoke(() => Whoami.AmIP2(), "NONE");
        }
    }
}
