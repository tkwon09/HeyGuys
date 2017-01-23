using UnityEngine;
using UnityEngine.UI;

public class ControlScript : MonoBehaviour
{
    public new Camera camera;
    public Animator animator;

    public int playerSlot;

    public KeyCode forwardKey;
    public KeyCode backKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public float moveSpeed;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float cameraBob;
    public float bobFrequency;
    public float damageFadeSpeed;
    public AudioSource[] footstepSounds;
    public AudioSource[] heys;
    public AudioSource hitSound;

    public Image healthBar;
    public Image healthBarMax;
    public Image waveBar;
    public Image waveBarMax;
    public Image hitPoint;
    public Image damageFlash;
    public Image[] hitFlash;

    private float originalCameraOffset;
    public float bobTimer;
    private Vector3 previousMousePosition;
    private float mouseRotationY;
    private bool canMakeSound = true;

    void Start()
    {
        originalCameraOffset = camera.transform.position.y - transform.position.y;
        bobTimer = 0;
        mouseRotationY = 0;
    }

    void Update()
    {
        float mouseRotationX = camera.transform.eulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
        mouseRotationY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        mouseRotationY = Mathf.Clamp(mouseRotationY, -60, 60);
        camera.transform.eulerAngles = new Vector3(-mouseRotationY, mouseRotationX);
        //transform.forward = new Vector3(camera.transform.forward.x, transform.forward.y, camera.transform.forward.z);

        Vector3 movement = Vector3.zero;
        if (Input.GetKey(forwardKey))
        {
            movement += camera.transform.forward;
        }
        if (Input.GetKey(backKey))
        {
            movement += -camera.transform.forward;
        }
        if (Input.GetKey(leftKey))
        {
            float x = camera.transform.forward.x * Mathf.Cos(Mathf.PI / 2) - camera.transform.forward.z * Mathf.Sin(Mathf.PI / 2);
            float z = camera.transform.forward.x * Mathf.Sin(Mathf.PI / 2) + camera.transform.forward.z * Mathf.Cos(Mathf.PI / 2);
            movement += new Vector3(x, 0, z);
        }
        if (Input.GetKey(rightKey))
        {
            float x = camera.transform.forward.x * Mathf.Cos(3 * Mathf.PI / 2) - camera.transform.forward.z * Mathf.Sin(3 * Mathf.PI / 2);
            float z = camera.transform.forward.x * Mathf.Sin(3 * Mathf.PI / 2) + camera.transform.forward.z * Mathf.Cos(3 * Mathf.PI / 2);
            movement += new Vector3(x, 0, z);
        }
        GetComponent<CharacterController>().SimpleMove(movement.normalized * moveSpeed);
        if (movement != Vector3.zero)
        {
            float bounceOffset = cameraBob * Mathf.Abs(Mathf.Sin(2 * Mathf.PI * bobFrequency * bobTimer));
            camera.transform.position = new Vector3(transform.position.x, transform.position.y + originalCameraOffset + bounceOffset, transform.position.z);
            bobTimer += Time.fixedDeltaTime;
            if (bounceOffset < cameraBob / 4)
            {
                if (canMakeSound)
                {
                    footstepSounds[Random.Range(0, footstepSounds.Length)].Play();
                    canMakeSound = false;
                }
            }
            else
            {
                canMakeSound = true;
            }
        }
        else
        {
            camera.transform.position = new Vector3(transform.position.x, transform.position.y + originalCameraOffset, transform.position.z);
            bobTimer = 0;
        }
        if (damageFlash.color.a > 0)
        {
            damageFlash.color = new Color(damageFlash.color.r, damageFlash.color.g, damageFlash.color.b, damageFlash.color.a - damageFadeSpeed * Time.deltaTime);
        }
        if (hitFlash[0].color.a > 0)
        {
            foreach (Image image in hitFlash)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - damageFadeSpeed * Time.deltaTime);
            }
        }
    }

    public void SayHey()
    {
        heys[Random.Range(0, heys.Length)].Play();
    }

    public void FlashDamage()
    {
        damageFlash.color = new Color(damageFlash.color.r, damageFlash.color.g, damageFlash.color.b, 0.4f);
    }

    public void FlashHit()
    {
        foreach (Image image in hitFlash)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        }
    }
}
