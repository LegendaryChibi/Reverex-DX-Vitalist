using UnityEngine;

public class BorderCollision : MonoBehaviour
{
    /*[SerializeField]
    Adrenaline adrenaline;*/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Bottle")
        {
            collision.gameObject.SetActive(false);
        }
    }
}
