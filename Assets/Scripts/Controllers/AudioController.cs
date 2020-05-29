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
    WaitForSeconds footstepsWait = new WaitForSeconds(0.2f);
    WaitForSeconds footsteps0Wait;
    WaitForSeconds footsteps1Wait;

    private void Start()
    {
        footsteps0 = AudioManager.instance.soundLibrary.GetClip("Footsteps", 0);
        footsteps1 = AudioManager.instance.soundLibrary.GetClip("Footsteps", 1);
        footsteps0Wait = new WaitForSeconds(footsteps0.length);
        footsteps1Wait = new WaitForSeconds(footsteps1.length);
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            yield return footstepsWait;
            audioSource.clip = footsteps0;
            audioSource.Play();
            yield return footsteps0Wait;
            audioSource.clip = footsteps1;
            audioSource.Play();
            yield return footsteps1Wait;
        }
    }

    public void PlayClip()
    {
        footstepsCoroutine = StartCoroutine(PlayFootsteps());
    }

    public void StopClip()
    {
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
