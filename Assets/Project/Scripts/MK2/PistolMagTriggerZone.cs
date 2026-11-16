using System;
using UnityEngine;

public class PistolMagTriggerZone : MonoBehaviour
{
    [SerializeField] private Pistol _pistol;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PistolMag mag) && other.gameObject.layer != LayerMask.NameToLayer("Pistol"))
        {
            if (!mag.IsInside)
            {
                _pistol.AcceptMag(mag);
            }
        }
    }
}
