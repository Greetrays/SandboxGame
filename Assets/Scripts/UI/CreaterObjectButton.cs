using UnityEngine;
using UnityEngine.UI;

public class CreaterObjectButton : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;

    private ObjectContainer _container;
    private Transform _spawnPoint;
    private CreatbleObject _objectPrefab;

    public void Initialize(Sprite _sprite, CreatbleObject objectPrefab, Transform spawnPoint, ObjectContainer container)
    {
        _image.sprite = _sprite;
        _objectPrefab = objectPrefab;
        _spawnPoint = spawnPoint;
        _container = container;
    }

    private void Start()
    {
        _button.onClick.AddListener(() => CreateObject());
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(() => CreateObject());
    }

    private void CreateObject()
    {
        CreatbleObject objectTemplate = Instantiate(_objectPrefab, _spawnPoint.position, Quaternion.identity);
        _container.Add(objectTemplate);
    }
}
