using UnityEngine;
using UnityEngine.Networking;

public class PlayerScript : NetworkBehaviour
{
    [SyncVar]
    public int slot;

    public int health;
    public int maxHealth;
    public float healthRechargeTimerMax;
    public float wave;
    public float maxWave;
    public float waveRechargeSpeed;
    public int bodyShotDamage;
    public int handShotDamage;
    public int headShotDamage;

    public ControlScript controlScript;
    public GameManager manager;

    private float sendTimer = 0;
    private float healthRechargeTimer = 0;
    static float SENDRATE = 0.05f;
    private bool doRaycast = false;
    private bool walking = false;
    
    [Client]
    public override void OnStartLocalPlayer()
    {
        manager = FindObjectOfType<GameManager>();
        manager.basePlayer = this;
        CmdInitializePlayer();
    }

    Ray ray;
    [Client]
	void Update ()
    {
        Debug.DrawRay(ray.origin, ray.direction);
        if (manager == null || slot != manager.basePlayer.slot  || controlScript == null)
        {
            return;
        }
        controlScript.healthBar.rectTransform.sizeDelta = new Vector2((float)health / (float)maxHealth * controlScript.healthBarMax.rectTransform.sizeDelta.x, controlScript.healthBar.rectTransform.sizeDelta.y);
        controlScript.waveBar.rectTransform.sizeDelta = new Vector2(wave / maxWave * controlScript.waveBarMax.rectTransform.sizeDelta.x, controlScript.waveBar.rectTransform.sizeDelta.y);
        if (healthRechargeTimer < healthRechargeTimerMax)
        {
            healthRechargeTimer += Time.deltaTime;
        }
        else
        {
            if (health > 0 && health < maxHealth)
            {
                ++health;
            }
            healthRechargeTimer = 0;
        }
        if (sendTimer < SENDRATE)
        {
            sendTimer += Time.deltaTime;
        }
        else
        {
            CmdTransmitMyPosition(controlScript.transform.position, Quaternion.Euler(0, controlScript.camera.transform.eulerAngles.y, 0));
            sendTimer = 0;
        }
        if (health > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (wave >= 1)
                {
                    controlScript.animator.Play("Wave", -1, 0);
                    wave -= 1;
                    doRaycast = true;
                    controlScript.SayHey();
                    CmdSayHey();
                }
            }
        }
        else
        {
            controlScript.animator.Play("Happy");
        }
        if (wave < maxWave)
        {
            wave += waveRechargeSpeed * Time.deltaTime;
        }
        else
        {
            wave = maxWave;
        }
        if (controlScript.bobTimer > 0 && !walking)
        {
            if (health > 0)
            {
                CmdDoAnimation("Walk");
            }
            else
            {
                CmdDoAnimation("HappyWalk");
            }
            walking = true;
        }
        else if (controlScript.bobTimer == 0 && walking)
        {
            if (health > 0)
            {
                CmdDoAnimation("Idle");
            }
            else
            {
                CmdDoAnimation("HappyIdle");
            }
            walking = false;
        }
    }

    void FixedUpdate()
    {
        if (doRaycast)
        {
            ray = new Ray();
            ray.origin = controlScript.camera.transform.position;
            ray.direction = controlScript.camera.transform.forward;
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 1000))
            {
                CmdMakeBulletTrail(ray.origin, rayHit.point);
                Debug.Log("Hit something!");
                if (rayHit.collider.gameObject.GetComponent<DamageHitBoxScript>() != null)
                {
                    if (rayHit.collider.gameObject.GetComponent<DamageHitBoxScript>().enemyScript.canTakeDamage)
                    {
                        int enemySlot = rayHit.collider.gameObject.GetComponent<DamageHitBoxScript>().enemyScript.playerSlot;
                        controlScript.hitSound.Play();
                        controlScript.FlashHit();
                        if (rayHit.collider.CompareTag("Body"))
                        {
                            Debug.Log("Hit " + enemySlot + " body!");
                            CmdDoDamage(enemySlot, bodyShotDamage);
                        }
                        else if (rayHit.collider.CompareTag("Hand"))
                        {
                            Debug.Log("Hit " + enemySlot + " hand!");
                            CmdDoDamage(enemySlot, handShotDamage);
                        }
                        else if (rayHit.collider.CompareTag("Head"))
                        {
                            Debug.Log("Hit " + enemySlot + " head!");
                            CmdDoDamage(enemySlot, headShotDamage);
                        }
                    }
                }
            }
            else
            {
                CmdMakeBulletTrail(ray.origin, ray.origin + ray.direction * 1000);
            }
            doRaycast = false;
        }
    }

    [Command]
    public void CmdInitializePlayer()
    {
        manager.NotifyPlayerInitialized();
    }
    
    [Command]
    public void CmdDoDamage(int enemySlot, int damage)
    {
        manager.RpcDoDamageToPlayer(enemySlot, damage);
    }

    [Command]
    public void CmdDoAnimation(string animationName)
    {
        manager.RpcAnimateEnemy(slot, animationName);
    }

    [Command]
    public void CmdSayHey()
    {
        manager.RpcSayHey(slot);
    }

    [Command]
    public void CmdReportDeadPlayer()
    {
        manager.ProcessDeadPlayer(slot);
    }

    [Command]
    public void CmdMakeBulletTrail(Vector3 startPosition, Vector3 endPosition)
    {
        manager.RpcMakeBulletTrail(startPosition, endPosition);
    }

    [Command(channel=1)]
    public void CmdTransmitMyPosition(Vector3 position, Quaternion rotation)
    {
        manager.EvaluatePlayerPosition(slot, position, rotation);
    }
    
    [Client]
    public void TakeDamage(int damage)
    {
        if (health > 0)
        {
            controlScript.FlashDamage();
            health -= damage;
            healthRechargeTimer = 0;
            if (health <= 0)
            {
                health = 0;
                if (walking)
                {
                    CmdDoAnimation("HappyWalk");
                }
                else
                {
                    CmdDoAnimation("HappyIdle");
                }
                foreach (EnemyScript enemy in manager.enemies)
                {
                    if (enemy != null)
                    {
                        enemy.collider.isTrigger = true;
                    }
                }
                CmdReportDeadPlayer();
            }
        }
    }
}