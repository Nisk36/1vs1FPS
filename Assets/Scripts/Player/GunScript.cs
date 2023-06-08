using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunScript : MonoBehaviour
{
    public float shootInterval = 0.1f;
    public int shootDamage;
    public float adsSpeed;
    public float adsZoom;

    public GameObject bulletImpact;

    //�e��SE
    public AudioSource shotSound;

    //�P���e�̏e����炷
    public void SoundGunShot()
    {
        shotSound.Play();
    }

    public void LoopOnARGun()
    {
        if (!shotSound.isPlaying)
        {
            shotSound.loop = true;
            shotSound.Play();
        }
    }

    public void LoopOFF_ARGun()
    {
        shotSound.loop = false;
        shotSound.Stop();
    }
}
