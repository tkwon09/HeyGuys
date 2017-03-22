using UnityEngine;
using UnityEngine.UI;

public class ControlScript : MonoBehaviour
{
    public new Camera camera;
    public Animator animator;
    public Rigidbody rigidBody;
    public CapsuleCollider capsuleCollider;

    public int playerSlot;

    public KeyCode forwardKey;
    public KeyCode backKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public KeyCode crouchKey;
    public float moveSpeed;
    public float airMoveSpeed;
    public float crouchMoveSpeed;
    public float jumpSpeed;
    public float fallAcceleration;
    public float checkGroundDistance;
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

    public bool walking = false;
    public bool jumped = false;
    public bool crouching = false;
    public bool onGround = false;

    private Vector3 moveVelocity;
    private float originalCameraOffset;
    public float bobTimer;
    private Vector3 previousMousePosition;
    private float mouseRotationY;
    private bool canMakeSound = true;
    private float verticalSpeed;
    private float currentMoveSpeed;

    void Start()
    {
        originalCameraOffset = camera.transform.position.y - transform.position.y;
        bobTimer = 0;
        mouseRotationY = 0;
        moveVelocity = Vector3.zero;
        verticalSpeed = 0;
        currentMoveSpeed = moveSpeed;
    }
    
    void Update()
    {
        float mouseRotationX = camera.transform.eulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
        mouseRotationY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        mouseRotationY = Mathf.Clamp(mouseRotationY, -60, 60);
        camera.transform.eulerAngles = new Vector3(-mouseRotationY, mouseRotationX);
        //transform.forward = new Vector3(camera.transform.forward.x, transform.forward.y, camera.transform.forward.z);

        moveVelocity = Vector3.zero;
        if (Input.GetKey(forwardKey))
        {
            moveVelocity = camera.transform.forward;
        }
        if (Input.GetKey(backKey))
        {
            moveVelocity = -camera.transform.forward;
        }
        if (Input.GetKey(leftKey))
        {
            float x = camera.transform.forward.x * Mathf.Cos(Mathf.PI / 2) - camera.transform.forward.z * Mathf.Sin(Mathf.PI / 2);
            float z = camera.transform.forward.x * Mathf.Sin(Mathf.PI / 2) + camera.transform.forward.z * Mathf.Cos(Mathf.PI / 2);
            moveVelocity = new Vector3(x, 0, z);
        }
        if (Input.GetKey(rightKey))
        {
            float x = camera.transform.forward.x * Mathf.Cos(3 * Mathf.PI / 2) - camera.transform.forward.z * Mathf.Sin(3 * Mathf.PI / 2);
            float z = camera.transform.forward.x * Mathf.Sin(3 * Mathf.PI / 2) + camera.transform.forward.z * Mathf.Cos(3 * Mathf.PI / 2);
            moveVelocity = new Vector3(x, 0, z);
        }

        walking = moveVelocity != Vector3.zero;
        crouching = Input.GetKey(crouchKey);
        
        if (onGround)
        {
            if (walking)
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

            verticalSpeed = 0;
            if (Input.GetKeyDown(jumpKey))
            {
                jumped = true;
            }
            if (crouching)
            {
                currentMoveSpeed = crouchMoveSpeed;
            }
            else
            {
                currentMoveSpeed = moveSpeed;
            }
        }
        else
        {
            currentMoveSpeed = airMoveSpeed;
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

    void FixedUpdate()
    {
        RaycastHit hitInfo;
        onGround = Physics.SphereCast(transform.position, capsuleCollider.radius - 0.01f, Vector3.down, out hitInfo, capsuleCollider.height / 2 + checkGroundDistance);
        Debug.DrawRay(transform.position, Vector3.down * (capsuleCollider.height / 2 + checkGroundDistance), Color.red);
        // TODO : Implement friction and acceleration based movement?
        if (jumped)
        {
            verticalSpeed = jumpSpeed;
            jumped = false;
        }
        rigidBody.velocity = moveVelocity.normalized * currentMoveSpeed + new Vector3(0, verticalSpeed, 0);
        verticalSpeed -= fallAcceleration * Time.fixedDeltaTime;
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

    void OnCollisionStay(Collision collision)
    {
        if (Vector3.Angle(Vector3.up, collision.contacts[0].normal) < 45)
        {
            verticalSpeed = 0;
        }
    }
}
