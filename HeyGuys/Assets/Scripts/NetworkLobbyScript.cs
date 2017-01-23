using UnityEngine;
using UnityEngine.Networking;

public class NetworkLobbyScript : NetworkLobbyManager
{
    GameManager gameManager = null;
    
    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        if (gameManager == null)
        {
            GameObject gameManagerObj = Instantiate(spawnPrefabs[0]);
            NetworkServer.Spawn(gameManagerObj);
            gameManager = gameManagerObj.GetComponent<GameManager>();
        }
        PlayerScript playerScript = gamePlayer.GetComponent<PlayerScript>();
        NetworkLobbyPlayer lobbyPlayerScript = lobbyPlayer.GetComponent<NetworkLobbyPlayer>();
        playerScript.slot = lobbyPlayerScript.slot;
        playerScript.manager = gameManager;
        return true;
    }
}
