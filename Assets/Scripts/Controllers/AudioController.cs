using System.Collections;
using UnityEngine;
using Mirror;

public class AudioController : NetworkBehaviour
{
    [SerializeField]
    private AudioSource audioSource;
    private AudioClip footsteps0;
    private AudioClip footsteps1;

    private Coroutine footstepsCoroutine;

    private void Start()
    {
        footsteps0 = AudioManager.instance.soundLibrary.GetClip("Footsteps", 0);
        footsteps1 = AudioManager.instance.soundLibrary.GetClip("Footsteps", 1);
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            audioSource.clip = footsteps0;
            audioSource.Play();
            yield return new WaitForSeconds(footsteps0.length);
            audioSource.clip = footsteps1;
            audioSource.Play();
            yield return new WaitForSeconds(footsteps1.length);
        }
    }

    public void PlayClip()
    {
        Debug.Log("Footsetpping");
        footstepsCoroutine = StartCoroutine(PlayFootsteps());
    }

    public void StopClip()
    {
        Debug.Log("NOT Footsetpping");
        if (footstepsCoroutine != null)
        {
            StopCoroutine(footstepsCoroutine);
        }
    }

    // Network RPCs for footsteps audio
    [Command]
    void CmdPlayClip()
    {
        RpcPlayClip();
    }

    [ClientRpc]
    void RpcPlayClip()
    {
        if (!isLocalPlayer)
        {
            PlayClip();
        }
    }

    [Command]
    void CmdStopClip()
    {
        RpcStopClip();
    }

    [ClientRpc]
    void RpcStopClip()
    {
        if (!isLocalPlayer)
        {
            StopClip();
        }
    }
}
