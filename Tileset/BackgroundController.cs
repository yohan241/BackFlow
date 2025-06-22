using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float startpos , length;
    public GameObject cam;
    public float parallax;


    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallax;
        float movement = cam.transform.position.x * (1- parallax);
        transform.position = new Vector3(startpos+distance, transform.position.y, transform.position.z);

        if (movement > startpos + length)
        {
            startpos += length;
        }
        else if (movement < startpos - length)
        {
            startpos -= length;
        }

    }
}
