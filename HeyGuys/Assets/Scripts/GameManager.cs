using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static float SMOOTHING = 5;

    [SyncVar]
    int playersInitialized = 0;

    [SyncVar]
    int playersLeft;

    // Client Only
    public PlayerScript basePlayer;
    public EnemyScript[] enemies;

    bool[] dead;

    public GameObject controlPrefab;
    public GameObject enemyPrefab;
    public GameObject bulletTrailPrefab;

    float endGameCountdown = 5f;
    bool gameEnded = false;
    int winnerPlayer = 0;
    Vector3 targetCameraPosition;

    Camera observerCamera = null;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        if (gameEnded)
        {
            if (isClient && winnerPlayer != basePlayer.slot && observerCamera != null)
            {
                targetCameraPosition = enemies[winnerPlayer].transform.position + Vector3.up * 2 + enemies[winnerPlayer].transform.forward * 5;
                observerCamera.transform.position = Vector3.MoveTowards(observerCamera.transform.position, targetCameraPosition, SMOOTHING * Time.deltaTime);
                observerCamera.transform.LookAt(enemies[winnerPlayer].transform);
            }
            if (isServer)
            {
                if (endGameCountdown <= 0)
                {
                    NetworkManager.singleton.ServerChangeScene("Lobby");
                    gameEnded = false;
                    endGameCountdown = 5f;
                }
                else
                {
                    endGameCountdown -= Time.deltaTime;
                }
            }
        }
    }
    
    [Server]
    public void NotifyPlayerInitialized()
    {
        playersInitialized++;
        if (playersInitialized == NetworkManager.singleton.numPlayers)
        {
            playersLeft = playersInitialized;
            dead = new bool[playersInitialized];
            for (int i = 0; i < playersInitialized; ++i)
            {
                dead[i] = false;
            }
            Debug.Log("Called RPC Initialize Stuff");
            RpcInitializeStuff(playersInitialized);
        }
    }

    [ClientRpc]
    public void RpcInitializeStuff(int numPlayers)
    {
        Debug.Log("Running RPC Initialize Stuff");
        foreach (Camera camera in FindObjectsOfType<Camera>())
        {
            if (camera.gameObject.tag == "ObserverCam")
            {
                observerCamera = camera;
                break;
            }
        }
        observerCamera.gameObject.SetActive(false);
        enemies = new EnemyScript[numPlayers];
        foreach (SpawnSpotScript spawnSpot in FindObjectsOfType<SpawnSpotScript>())
        {
            if (spawnSpot.playerSlot == basePlayer.slot)
            {
                GameObject newControl = Instantiate(controlPrefab);
                newControl.transform.position = spawnSpot.transform.position;
                basePlayer.controlScript = newControl.GetComponent<ControlScript>();
                basePlayer.controlScript.playerSlot = basePlayer.slot;
            }
            else
            {
                if (spawnSpot.playerSlot < numPlayers)
                {
                    GameObject newEnemy = Instantiate(enemyPrefab);
                    enemies[spawnSpot.playerSlot] = newEnemy.GetComponent<EnemyScript>();
                    newEnemy.transform.position = spawnSpot.transform.position;
                    enemies[spawnSpot.playerSlot].playerSlot = spawnSpot.playerSlot;
                }
            }
        }
    }
    
    [Server]
    public void EvaluatePlayerPosition(int playerSlot, Vector3 position, Quaternion rotation)
    {
        // @TODO : Simulate and roll back player if necessary (e.g. player has move speed of 2 units per second but has moved 4 units over 1 second)
        RpcUpdateEnemy(playerSlot, position, rotation);
    }

    [ClientRpc]
    public void RpcUpdateEnemy(int playerSlot, Vector3 position, Quaternion rotation)
    {
        if (basePlayer.slot != playerSlot && enemies != null && playerSlot < enemies.Length && enemies[playerSlot] != null)
        {
            enemies[playerSlot].SetPosition(position, rotation);
        }
    }

    [Server]
    public void ProcessDeadPlayer(int playerSlot)
    {
        dead[playerSlot] = true;
        --playersLeft;
        if (playersLeft == 1)
        {
            for (winnerPlayer = 0; winnerPlayer < dead.Length; ++winnerPlayer)
            {
                if (!dead[winnerPlayer])
                {
                    break;
                }
            }
            gameEnded = true;
            RpcLookAtWinner(winnerPlayer);
        }
        RpcDisableEnemyCollider(playerSlot);
    }

    [ClientRpc]
    public void RpcLookAtWinner(int playerSlot)
    {
        gameEnded = true;
        winnerPlayer = playerSlot;
        if (basePlayer.slot == winnerPlayer)
        {
            return;
        }
        else
        {
            Destroy(basePlayer.controlScript.gameObject);
        }
        foreach (EnemyScript enemy in enemies)
        {
            if (enemy != null && enemy.playerSlot == winnerPlayer)
            {
                observerCamera.gameObject.SetActive(true);
                observerCamera.targetDisplay = 0;
            }
            else
            {
                Destroy(enemy);
            }
        }
    }

    [ClientRpc]
    public void RpcDisableEnemyCollider(int playerSlot)
    {
        if (enemies[playerSlot] != null)
        {
            enemies[playerSlot].collider.isTrigger = true;
            enemies[playerSlot].canTakeDamage = false;
        }
    }

    [ClientRpc]
    public void RpcDoDamageToPlayer(int playerSlot, int damage)
    {
        Debug.Log("Requested damage to player " + playerSlot + ", am player " + basePlayer.slot);
        if (basePlayer.slot == playerSlot)
        {
            Debug.Log("Did damage to player " + basePlayer.slot);
            basePlayer.TakeDamage(damage);
        }
    }

    [ClientRpc]
    public void RpcAnimateEnemy(int playerSlot, string animationName)
    {
        if (basePlayer.slot != playerSlot && enemies != null && playerSlot < enemies.Length && enemies[playerSlot] != null)
        {
            enemies[playerSlot].animator.Play(animationName);
        }
    }

    [ClientRpc]
    public void RpcSayHey(int playerSlot)
    {
        if (enemies[playerSlot] != null)
        {
            enemies[playerSlot].SayHey();
        }
    }

    [ClientRpc]
    public void RpcMakeBulletTrail(Vector3 startPosition, Vector3 endPosition)
    {
        GameObject bulletTrail = Instantiate(bulletTrailPrefab);
        BulletTrailScript bulletTrailScript = bulletTrail.GetComponent<BulletTrailScript>();
        bulletTrailScript.startPoint = startPosition;
        bulletTrailScript.endPoint = endPosition;
    }
}
