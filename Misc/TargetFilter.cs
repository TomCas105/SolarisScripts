using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFilter : MonoBehaviour
{
    public static List<TargetFilter> targets = new List<TargetFilter>();

    public static List<TargetFilter> GetHostileTargets(FactionData _ownFaction, int _targetType)
    {
        if (_ownFaction == null)
        {
            return null;
        }

        var _hostileTargets = new List<TargetFilter>();

        foreach (var target in targets)
        {
            if (target.Type == _targetType && target.TargetFaction != null && _ownFaction.hostileFactions.Contains(target.TargetFaction.id))
            {
                _hostileTargets.Add(target);
            }
        }

        return _hostileTargets;
    }

    public FactionData TargetFaction { get; set; }
    public int Type { get; private set; } //0 ship, 1 fighter, 2 missile

    void Start()
    {
        if (GetComponent<Ship>() != null)
        {
            Type = 0;
        }

        //if (GetComponents<Fighter>() != null)
        //{
        //    type = 1;
        //}

        if (GetComponent<Projectile>() != null)
        {
            Type = 2;
        }

        targets.Add(this);
    }

    public Vector2 GetVelocity()
    {
        if (Type == 0)
        {
            return GetComponent<Rigidbody2D>().linearVelocity;
        }

        if (Type == 1)
        {
            //return GetComponent<Fighter>().Velocity;
        }

        if (Type == 2)
        {
            return GetComponent<Projectile>().GetVelocity();
        }

        return Vector2.zero;
    }

    public bool TakeDamage(DamageData _damage, Ship _attacker = null, float? _distance = null, float? _maxRange = null, float _hitAngle = 90f)
    {
        if (Type == 2)
        {
            var _missile = GetComponent<Projectile>();
            return _missile.TakeDamage(_damage.hullDamage * Random.Range(1f - _damage.damageRandomness, 1f + _damage.damageRandomness));
        }

        if (Type == 0)
        {
            var _ship = GetComponent<Ship>();
            return _ship.TakeDamage(_damage, _attacker, _distance, _maxRange, _hitAngle);
        }

        return false;
    }

    private void OnDestroy()
    {
        targets.Remove(this);
    }
}
