using UnityEngine;
using Mirror;

public class CustomNetworkManager : NodeListServer.NodeListServerNetworkManager
{
    private Vector3[] spawnPositions;
    private int mapBoundary = 5980;

    public override void ServerChangeScene(string newSceneName)
    {
        if (newSceneName == RoomScene)
        {
            // Reset countdown
            countingDown = -1;

            foreach (NetworkRoomPlayer roomPlayer in roomSlots)
            {
                if (roomPlayer == null)
                    continue;

                // find the game-player object for this connection, and destroy it
                NetworkIdentity identity = roomPlayer.GetComponent<NetworkIdentity>();

                if (NetworkServer.active)
                {
                    // re-add the room object
                    roomPlayer.GetComponent<NetworkRoomPlayer>().readyToBegin = false;
                    NetworkServer.ReplacePlayerForConnection(identity.connectionToClient, roomPlayer.gameObject);
                    Vector3 spawnPoint = roomPlayer.gameObject.transform.position;
                    roomPlayer.gameObject.transform.position = new Vector3(spawnPoint.x, 200, spawnPoint.z);
                    roomPlayer.gameObject.SetActive(true);
                }
            }

            allPlayersReady = false;
        }
        else
        {
            GenSpawnPositions();
        }

        base.ServerChangeScene(newSceneName);
    }

    public override Vector3 GetStartPosition()
    {
        if (IsSceneActive(GameplayScene))
        {
            if (startPositionIndex < spawnPositions.Length)
            {
                return spawnPositions[startPositionIndex];
            } else
            {
                return new Vector3(UnityEngine.Random.Range(-200, 200), 200, UnityEngine.Random.Range(-200, 200));
            }
        } else
        {
            return new Vector3(0,200,0);
            //return new Vector3(UnityEngine.Random.Range(-2000, 2000), 200, UnityEngine.Random.Range(-2000, 2000));
        }
    }

    public void GenSpawnPositions()
    {
        int spawnLength = mapBoundary * 4;
        spawnPositions = new Vector3[numPlayers];
        int spawnGap = Mathf.FloorToInt(spawnLength / (numPlayers + 1));
        int halfMapBoundary = mapBoundary / 2;
        int spawnPos = -halfMapBoundary;
        int side = 0;
        for (int i = 0; i < numPlayers; i++)
        {
            if (side == 0)
            {
                spawnPositions[i] = new Vector3(spawnPos, 200, halfMapBoundary);
            }
            else if (side == 1)
            {
                spawnPositions[i] = new Vector3(halfMapBoundary, 200, -spawnPos);
            }
            else if (side == 2)
            {
                spawnPositions[i] = new Vector3(-spawnPos, 200, -halfMapBoundary);
            }
            else
            {
                spawnPositions[i] = new Vector3(-halfMapBoundary, 200, spawnPos);
            }

            spawnPos += spawnGap;

            //If spawnpos exceeds map edge, reset and move to next edge (top, right, bottom, left)
            if (spawnPos > halfMapBoundary) { spawnPos -= mapBoundary; side++; }
        }
    }
}
