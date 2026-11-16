using System;
using UnityEngine;

public class MagPouch : MonoBehaviour
{
    [SerializeField] private int _magAmount = 10;
    [SerializeField] private PistolMag _magPrefab;
    [SerializeField] private Transform _magSpawnPos;
    private PistolMag _lastSpawnedMag;
    
    private void Start()
    {
        SpawnMag();
    }

    private void SpawnMag()
    {
        if (_magAmount > 0)
        {
            _magAmount--;
            _lastSpawnedMag = Instantiate(_magPrefab, _magSpawnPos.position, _magSpawnPos.rotation, transform);
            _lastSpawnedMag.DisableRbAndCollision();
            _lastSpawnedMag.OnTake += TakeMag;
        }
    }

    // когда в зоне магпауча жмешь граб (левое плечо)
    public void TakeMag()
    {
        _lastSpawnedMag.EnableRbAndCollision();
        _lastSpawnedMag.OnTake -= TakeMag;
        _lastSpawnedMag = null;
        SpawnMag();
    }
}
