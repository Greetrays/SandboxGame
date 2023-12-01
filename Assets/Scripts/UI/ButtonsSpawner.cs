using System.Collections.Generic;
using UnityEngine;

public class ButtonsSpawner : MonoBehaviour
{
    [SerializeField] private List<ConfigureButton> _buttonConfigs = new List<ConfigureButton>();
    [SerializeField] private Transform _container;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private ObjectContainer _objectContainer;

    private void Start()
    {
        FillContainer();
    }

    private void FillContainer()
    {
        foreach (var button in _buttonConfigs)
        {
            AddButton(button);
        }
    }

    private void AddButton(ConfigureButton buttonConfig)
    {
        CreaterObjectButton button = Instantiate(buttonConfig.ButtonPrefab, _container);
        button.Initialize(buttonConfig.Sprite, buttonConfig.ObjectPrefab, _spawnPoint, _objectContainer);
    }
}
