using UnityEngine;

public class Pistol : MonoBehaviour
{
    [SerializeField] private Transform _magInsertedPos;
    [SerializeField] private Transform _magOutPos;

    [SerializeField] private GameObject _laser;
    
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

    public bool IsBulletInChamber { get; set; }
    
    public PistolMag CurrentMag { get; private set; }

    // Выстрел должен производится на триггер контроллера в котором находится пистолет
    public void Shoot()
    {
        if (!IsBulletInChamber)
        {
            // холостой звук
            return;
        }
        // выстрел
    }
    
    public void AcceptMag(PistolMag mag)
    {
        if (CurrentMag != null)
        {
            return;
        }
        
        CurrentMag = mag;
        CurrentMag.SlideIn(_magOutPos.rotation, _magOutPos.position, _magInsertedPos.position);
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
