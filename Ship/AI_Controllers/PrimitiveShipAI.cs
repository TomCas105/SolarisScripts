using UnityEngine;

public class PrimitiveShipAI : MonoBehaviour, IShipInputProvider
{
    public TargetFilter CurrentTarget { get; set; }
    public Ship AttachedShip { get; set; }

    private float retargetTimer = 0f;

    void Start()
    {
        if (AttachedShip == null)
        {
            AttachedShip = GetComponent<Ship>();
            AttachedShip.ShipInputProvider = this;
        }
    }

    void Update()
    {
        if (AttachedShip == null)
        {
            return;
        }

        if (retargetTimer > 0f)
        {
            retargetTimer -= Time.deltaTime;
        }
        else
        {
            FindNewTarget();
            retargetTimer = 2f;
        }
    }

    public ShipInputData GetInput()
    {
        ShipInputData _input = new ShipInputData();

        if (CurrentTarget == null || AttachedShip == null)
        {
            return _input;
        }

        float _desiredDistance = AttachedShip.GetTurretsRange(Ship.TurretRangeType.NORMAL);
        Vector3 _shipPosition = AttachedShip.transform.position;

        Vector2 _toTarget = (CurrentTarget.transform.position - _shipPosition);
        float _angle = Vector2.SignedAngle(transform.up, _toTarget);

        if (Mathf.Abs(_angle) > 2f)
        {
            _input.Turn = -Mathf.Clamp(_angle / AttachedShip.TurningRate, -1f, 1f);
        }

        if (_toTarget.magnitude < _desiredDistance + AttachedShip.ShipRadius)
        {
            return _input;
        }

        Vector2 _local = transform.InverseTransformDirection(_toTarget.normalized);

        _input.Horizontal = Mathf.Clamp(_local.x, -1f, 1f);
        _input.Vertical = Mathf.Clamp(_local.y, -1f, 1f);
        _input.HorizontalLimit = 1f;
        _input.VerticalLimit = 1f;

        return _input;
    }

    private void FindNewTarget()
    {
        var _shipFaction = AttachedShip.ShipFaction;

        TargetFilter _newTarget = CurrentTarget;
        float _max = float.MaxValue;

        if (_newTarget == null)
        {
            var _targets = TargetFilter.GetHostileTargets(_shipFaction, 0);

            if (_targets != null && _targets.Count > 0)
            {
                foreach (var _target in _targets)
                {
                    if (_target == AttachedShip.ShipTargetFilter)
                    {
                        continue;
                    }

                    float _rangeSqr = (_target.transform.position - transform.position).sqrMagnitude;

                    if (_rangeSqr < _max)
                    {
                        _max = _rangeSqr;
                        _newTarget = _target;
                    }
                }
            }
        }

        CurrentTarget = _newTarget;
    }
}
