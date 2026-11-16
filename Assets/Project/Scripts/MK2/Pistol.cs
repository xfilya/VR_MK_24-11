using System;
using Sounds;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    [SerializeField] private Transform _magInsertedPos;
    [SerializeField] private Transform _magOutPos;

    [SerializeField] private GameObject _laser;
    [SerializeField] private SoundObject _shotSound;
    [SerializeField] private SoundObject _dryFireSound;
    
    private bool _isPickedUp = false;
    public bool IsPickedUp
    {
        get => _isPickedUp;
        set
        {
            _laser.SetActive(value);
            _isPickedUp = value;
        }
    }

    public bool IsBulletInChamber { get; private set; }
    
    public PistolMag CurrentMag { get; private set; }

    public event Action OnShoot;

    // Выстрел должен производится на триггер контроллера в котором находится пистолет
    public void Shoot()
    {
        if (!IsBulletInChamber)
        {
            _dryFireSound.Play();
            return;
        }

        IsBulletInChamber = false;
        _shotSound.Play();
        OnShoot?.Invoke();
        
        // Здесь выполняется логика самого выстрела.
        
        LoadBulletIntoChamber();
    }

    public void LoadBulletIntoChamber()
    {
        if (IsBulletInChamber || CurrentMag == null || !CurrentMag.TryFeedBullet())
        {
            return;
        }

        Debug.Log("Loaded");
        IsBulletInChamber = true;
    }
    
    public void AcceptMag(PistolMag mag)
    {
        if (CurrentMag != null)
        {
            return;
        }
        
        CurrentMag = mag;
        CurrentMag.transform.parent = transform;
        CurrentMag.SlideIn(_magOutPos.localRotation, _magOutPos.localPosition, _magInsertedPos.localPosition);
    }

    public void ReleaseMag()
    {
        if (CurrentMag == null)
        {
            return;
        }
        
        CurrentMag.SlideOut(_magOutPos.position);
        CurrentMag = null;
    }
}
