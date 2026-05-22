using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor = 0.5f;   // 0 = locked to camera, 1 = moves with world
    private Transform cam;
    private Vector3 lastCamPos;
    private float spriteWidth;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxFactor, 0);
        lastCamPos = cam.position;

        float distX = cam.position.x - transform.position.x;
        if (Mathf.Abs(distX) > spriteWidth * 0.5f)   // loop the sprite for an endless background
            transform.position += new Vector3(Mathf.Sign(distX) * spriteWidth, 0);
    }
}