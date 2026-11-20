using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace OrbitLanes
{
    public class OrbitSnapTurn : SnapTurnProvider
    {
        bool latched;

        protected override float GetTurnAmount(Vector2 input)
        {
            if (input.magnitude < .35f) latched = false;
            if (latched || Mathf.Abs(input.x) < .7f || Mathf.Abs(input.x) < Mathf.Abs(input.y)) return 0f;
            latched = true;
            return Mathf.Sign(input.x) * turnAmount;
        }
    }
}
