using RootMotion.Dynamics;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private PuppetMaster _puppetMaster;
    [SerializeField] private CharacterNotDerstroyPart _head;
    [SerializeField] private List<CharacterPart> _partsDestroyble = new List<CharacterPart>();
    [SerializeField] private List<CharacterNotDerstroyPart> _partsNotDestroyeble = new List<CharacterNotDerstroyPart>();
    
    private void Start()
    {
        foreach (CharacterPart part in _partsDestroyble)
        {
            part.Initialize(_puppetMaster);
        }

        foreach (CharacterNotDerstroyPart part in _partsNotDestroyeble)
        {
            part.Initialize(_puppetMaster);
        }
    }
}
