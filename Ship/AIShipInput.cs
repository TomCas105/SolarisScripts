using UnityEngine;

public class AIShipInput : MonoBehaviour, IShipInputProvider
{
    public Transform target;
    public Ship AttachedShip { get; set; }

    public ShipInputData GetInput()
    {
        ShipInputData input = new ShipInputData();

        if (target == null)
        {
            return input;
        }

        Vector2 toTarget = target.position - transform.position;
        Vector2 local = transform.InverseTransformDirection(toTarget.normalized);

        input.Horizontal = Mathf.Clamp(local.x, -1f, 1f);
        input.Vertical = Mathf.Clamp(local.y, -1f, 1f);
        input.HorizontalLimit = 1f;
        input.VerticalLimit = 1f;

        float angle = Vector2.SignedAngle(transform.up, toTarget);
        input.Turn = Mathf.Clamp(angle / 45f, -1f, 1f);

        return input;
    }
}
