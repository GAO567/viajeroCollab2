using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum RealVirtualType
{
    //real avatar_virutal arrow, real arrow_virtual avatar
    RAvatar_VArrow, RArrow_VAvatar
}

public enum BystanderType
{
    Avatar,Capsule,Mix_90
}

public class RotationManager : MonoBehaviour
{
    public RealVirtualType real_virtualType = RealVirtualType.RArrow_VAvatar;
    public BystanderType bystanderType = BystanderType.Avatar;
    public bool isHeadGain = true;
    public bool isArrow = false;
    public bool isArrow_selfcentered = false;
    public bool isFrustum = false;
    public bool isEyeIcon = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
