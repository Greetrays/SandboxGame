using UnityEngine;

[CreateAssetMenu(fileName = "New useObj", menuName = "Objects Game/Create button")]
public class ConfigureButton : ScriptableObject
{
    [field: SerializeField] public CreaterObjectButton ButtonPrefab { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public CreatbleObject ObjectPrefab { get; private set; }
}
