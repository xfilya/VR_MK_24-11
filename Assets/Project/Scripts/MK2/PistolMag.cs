using System;
using DG.Tweening;
using UnityEngine;

public class PistolMag : MonoBehaviour
{
    [SerializeField] private GameObject _bullet1;
    [SerializeField] private GameObject _bullet2;

    [SerializeField] private int _capacity = 7;
    private int _ammoAmount;
    public int AmmoAmount
    {
        get => _ammoAmount;
        set
        {
            AdjustBulletVisuals(value);
            _ammoAmount = value;
        }

    }

    private Rigidbody _rb;
    private BoxCollider _col;
    
    public bool IsInside { get; private set; }

    public event Action OnTake;
    public event Action OnRelease;
    
    private void Awake()
    {
        AmmoAmount = _capacity;
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<BoxCollider>();
    }

    public bool TryShoot()
    {
        if (AmmoAmount > 0 && IsInside)
        {
            AmmoAmount--;
            return true;
        }
        return false;
    }

    public void EjectBullet()
    {
        AmmoAmount = Mathf.Max(0, AmmoAmount - 1);
    }

    public void AddBullet()
    {
        AmmoAmount = Mathf.Min(_capacity, AmmoAmount + 1);
    }
    
    // слайд внутрь пистолета когда магазин в зоне тригера
    public void SlideIn(Quaternion rotation, Vector3 from, Vector3 to)
    {
        // зафорсить отпускание магазина, тк он пытается зайти внутрь пистолета
        Release();
        DisableRbAndCollision();
        
        //тут еще надо чтобы отключалась возможность брать магазин
        
        // ставим на стартовую позицию и твином перемещаем внутрь пистолета
        transform.SetPositionAndRotation(from, rotation);
        transform.DOMove(to, 0.3f).SetEase(Ease.Linear).onComplete += () =>
        {
            IsInside = true;
            gameObject.layer = LayerMask.NameToLayer("Pistol");
        };
    }

    // слайд из пистолета наружу при нажатии на выпуск магазина
    // (верхняя кнопка контроллера quest 2/3 Y/B, взависимости в какой руке пистолет, в левой - Y, в правой - B)
    public void SlideOut(Vector3 to)
    {
        IsInside = false;
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOMove(to, 0.3f).SetEase(Ease.Linear));
        s.AppendCallback(EnableRbAndCollision);
        s.AppendCallback(() => _rb.AddForce(-transform.up, ForceMode.Impulse));
        s.AppendInterval(1);
        s.AppendCallback(() => gameObject.layer = LayerMask.NameToLayer("Mag"));
        
        
        // сюда еще надо s.AppendCallback() чтобы включалась возможность брать магазин
        
    }

    // когда берется в руки магазин
    public void Take()
    {
        EnableRbAndCollision();
        OnTake?.Invoke();
    }

    // когда отпускается из рук магазин
    public void Release()
    {
        EnableRbAndCollision();
        OnRelease?.Invoke();
    }

    public void EnableRbAndCollision()
    {
        _rb.constraints = RigidbodyConstraints.None;
        _rb.isKinematic = false;
        _col.enabled = true;
    }

    public void DisableRbAndCollision()
    {
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        _rb.isKinematic = true;
        _col.enabled = false;
    }
    
    private void AdjustBulletVisuals(int value)
    {
        if (value >= 2)
        {
            _bullet1.SetActive(true);
            _bullet2.SetActive(true);
        }
        else if (value == 1)
        {
            _bullet1.SetActive(true);
            _bullet2.SetActive(false);
        }
        else
        {
            _bullet1.SetActive(false);
            _bullet2.SetActive(false);
        }
    }
}
