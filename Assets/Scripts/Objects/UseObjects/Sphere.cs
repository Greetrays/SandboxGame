using UnityEngine;

public class Sphere : UseObject, ISelectableObject
{
    public GameObject GameObject
    {
        get => gameObject;
    }

    public void Select(bool isSelect)
    {
    }
}
