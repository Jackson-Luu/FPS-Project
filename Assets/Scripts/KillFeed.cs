using UnityEngine;

public class KillFeed : MonoBehaviour
{
    [SerializeField]
    GameObject killFeedItemPrefab;

    private float killFeedDuration = 10f;

    void Start()
    {
        GameManager.instance.onPlayerKilledCallback += OnKill;
    }

    public void OnKill (string player, string source)
    {
        GameObject go = (GameObject)Instantiate(killFeedItemPrefab, this.transform);
        go.GetComponent<KillFeedItem>().Setup(player, source);

        Destroy(go, killFeedDuration);
    }
}
