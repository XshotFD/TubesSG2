using UnityEngine;
using UnityEngine.InputSystem;

public class FireballSystem : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public bool aimAtCursor = true;
    public string castTriggerName = "Cast";

    private float nextFireTime;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (fireballPrefab == null) Debug.LogError("Fireball prefab missing!");
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
        Vector2 direction;
        if (aimAtCursor && Camera.main != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;
        }
        else
        {
            float facing = Mathf.Sign(transform.localScale.x);
            direction = new Vector2(facing, 0);
        }

        GameObject fb = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fbScript = fb.GetComponent<Fireball>();
        if (fbScript != null) fbScript.travelDirection = direction;

        if (anim != null && !string.IsNullOrEmpty(castTriggerName))
            anim.SetTrigger(castTriggerName);
    }
}