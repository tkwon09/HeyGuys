using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int playerSlot;
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    public Animator animator;
    public new Collider collider;
    public bool canTakeDamage = true;
    float timer;
    public float maxTimer;
    public AudioSource[] footsteps;
    public AudioSource[] heys;
    static int walkState = Animator.StringToHash("Walk");
    static int happyWalkState = Animator.StringToHash("HappyWalk");

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        timer = 0;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, GameManager.SMOOTHING * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, GameManager.SMOOTHING * 100 * Time.deltaTime);
        if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == walkState || animator.GetCurrentAnimatorStateInfo(0).fullPathHash == happyWalkState)
        {
            if (timer >= maxTimer)
            {
                footsteps[Random.Range(0, footsteps.Length)].Play();
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        else
        {
            timer = maxTimer;
        }
    }
    
    public void SetPosition(Vector3 newPosition, Quaternion newRotation)
    {
        // @TODO: Compare simulation times and jump if necessary instead of smoothing
        targetPosition = newPosition;
        targetRotation = newRotation;
    }

    public void SayHey()
    {
        heys[Random.Range(0, heys.Length)].Play();
    }
}
