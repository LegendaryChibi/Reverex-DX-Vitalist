using UnityEngine;

public class FracturePlatform : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    private void FixedUpdate()
    {
        float translation = speed * Time.deltaTime;
        transform.Translate(new Vector3(translation, 0, 0));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Barrier")
        {
            speed *= -1;
        }
    }
}
