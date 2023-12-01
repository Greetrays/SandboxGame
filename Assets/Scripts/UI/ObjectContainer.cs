using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectContainer : MonoBehaviour
{
    private List<CharacterPlayer> _players = new List<CharacterPlayer>();
    private List<UseObject> _useObjects = new List<UseObject>();

    public IEnumerable<CharacterPlayer> Players => _players;
    public IEnumerable<UseObject> UseObjects => _useObjects;

    public event UnityAction<CharacterPlayer> Added;

    public void Add(CreatbleObject creatbleObject)
    {
        if (creatbleObject.gameObject.TryGetComponent(out CharacterBootstrap characterBootstrap))
        {
            _players.Add(characterBootstrap.ChatacterPlayer);
            Added?.Invoke(characterBootstrap.ChatacterPlayer);
        }
        else if (creatbleObject.gameObject.TryGetComponent(out UseObject useObject))
        {
            _useObjects.Add(useObject);
        }
    }
}
