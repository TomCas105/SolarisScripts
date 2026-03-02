using UnityEngine;

public class BeamTurret : Turret
{
    public static Material beamMaterial;

    protected override void Reload()
    {
        if (currentAttackDelay > -Period)
        {
            currentAttackDelay -= Time.fixedDeltaTime;
        }
        else if (BurstCount > 1 && burstCountRemaining > 0)
        {
            burstCountRemaining = BurstCount;
        }
    }

    protected override void Attack(int _launchPointIndex)
    {
        Vector2 _launchPointOffset = turretData.launchPoints[_launchPointIndex];
        Vector2 _launchPoint = transform.up * _launchPointOffset.y + transform.right * _launchPointOffset.x + transform.position;
        float _angle = GetAngleToTarget(currentTarget);
        Vector2 _targetVector = ((Vector2)currentTarget.transform.position - _launchPoint).normalized;

        var _hits = Physics2D.RaycastAll(_launchPoint, _targetVector, Range);
        float _nearestDist = float.MaxValue;
        RaycastHit2D? _nearest = null;

        foreach (var _hit in _hits)
        {
            if (CheckHostile(_hit) && Vector2.Distance(_hit.point, _launchPoint) < _nearestDist)
            {
                _nearest = _hit;
                _nearestDist = Vector2.Distance(_hit.point, _launchPoint);
            }
        }

        if (_nearest != null)
        {
            var _hit = (RaycastHit2D)_nearest;
            var _target = _hit.collider.GetComponent<TargetFilter>();

            if (beamMaterial == null)
            {
                beamMaterial = AssetManager.Instance.GetMaterial("TrailMaterial");
            }

            var _beam = new GameObject(turretData.name + "_beam");
            _beam.transform.position = _launchPoint;
            var _trail = _beam.AddComponent<TrailRenderer>();
            _trail.startColor = turretData.trailData.startColor;
            _trail.endColor = turretData.trailData.endColor;
            _trail.startWidth = turretData.trailData.startWidth;
            _trail.endWidth = turretData.trailData.endWidth;
            _trail.time = turretData.trailData.duration;
            _trail.sortingOrder = 3;
            _trail.materials = new Material[] { beamMaterial };
            _trail.AddPosition(_hit.point);

            var _hitNormal = _hit.normal;
            float _surfaceAngle = Mathf.Atan2(_hitNormal.y, _hitNormal.x) * Mathf.Rad2Deg;
            var _projectileDirection = -transform.up;
            float _projectileAngle = Mathf.Atan2(_projectileDirection.y, _projectileDirection.x) * Mathf.Rad2Deg;
            float _impactAngle = 90f - Mathf.Abs(Mathf.DeltaAngle(_surfaceAngle, _projectileAngle));

            Ship _ownerShip = null;
            if (OwnerShip != null)
            {
                _ownerShip = OwnerShip;
            }

            _target.TakeDamage(TurretDamageData, _ownerShip, Vector2.Distance(_launchPoint, _hit.point), Range, _impactAngle);

            Destroy(_beam, turretData.trailData.duration);
        }
    }

    private bool CheckHostile(RaycastHit2D _hit)
    {
        if (_hit.collider == null)
        {
            return false;
        }

        var _target = _hit.collider.GetComponent<TargetFilter>();
        if (_target == null || _target == OwnerShip.ShipTargetFilter || _target.TargetFaction == null || OwnerShip.ShipFaction == null)
        {
            return false;
        }

        return OwnerShip.ShipFaction.hostileFactions.Contains(_target.TargetFaction.id);
    }
}
