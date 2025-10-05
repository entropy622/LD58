using System;
using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class CrystalAnimation : MonoBehaviour
{
    
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayAbsorbedAnimation()
    {
        anim.SetBool("Absorbed", true);
    }
    
}
