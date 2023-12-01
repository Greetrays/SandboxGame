using RootMotion.Dynamics;
using System;
using UnityEngine;

public class CharacterNotDerstroyPart : MonoBehaviour, ISelectableObject
{
    private PuppetMaster _puppetMaster;

    public void Select(bool isSelect)
    {
        _puppetMaster.state = isSelect ? PuppetMaster.State.Alive : PuppetMaster.State.Dead;
    }

    public void Initialize(PuppetMaster puppetMaster)
    {
        if (puppetMaster == null) 
            throw new ArgumentException(nameof(puppetMaster));

        _puppetMaster = puppetMaster;
    }
}
