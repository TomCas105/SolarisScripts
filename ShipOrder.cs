using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract record ShipOrder
{
    protected Ship orderShip;
    public string order = "idle";
    public bool strafeToTarget = false;
    public bool interruptible = false;
    public abstract bool GetOrderActive();
    public abstract float GetTargetAngle();
    public abstract Vector2 GetTargetPosition();
    public abstract string GetOrderInfo();
}

[System.Serializable]
public record ShipOrderAttack : ShipOrder
{
    private Ship targetShip;
    private bool orbitTarget;

    public ShipOrderAttack(Ship _orderShip, Ship _target, bool _strafe, bool _interruptible = false)
    {
        order = _strafe ? "attacking target" : "attacking target (orbit)";
        targetShip = _target;
        strafeToTarget = _strafe;
        orbitTarget = !_strafe;
        orderShip = _orderShip;
        interruptible = _interruptible;
    }

    public override bool GetOrderActive()
    {
        return targetShip != null && targetShip.IsAlive;
    }

    public override string GetOrderInfo()
    {
        return order + $": {targetShip.ShipClass}-{targetShip.ShipClassSuffix} {targetShip.ShipType}";
    }

    public override float GetTargetAngle()
    {
        var _targetVector = targetShip.transform.position - orderShip.transform.position;
        return Mathf.Atan2(_targetVector.y, _targetVector.x) * Mathf.Rad2Deg;
    }

    public override Vector2 GetTargetPosition()
    {
        return targetShip.transform.position;
    }

    public TargetFilter GetTarget()
    {
        return targetShip.ShipTargetFilter;
    }
}

public record ShipOrderMove : ShipOrder
{
    private Vector2 targetPoint;
    private float targetAngle;

    public ShipOrderMove(Ship _orderShip, Vector2 _target, bool _strafe, bool _interruptible = false)
    {
        order = "moving to position";
        targetPoint = _target;
        strafeToTarget = _strafe;
        orderShip = _orderShip;
        interruptible = _interruptible;

        var _targetVectorDelta = targetPoint - (Vector2)orderShip.transform.position;
        targetAngle = Mathf.Atan2(_targetVectorDelta.y, _targetVectorDelta.x) * Mathf.Rad2Deg;
    }

    public ShipOrderMove(Ship _orderShip, Vector2 target, bool strafe, float angle)
    {
        order = "strafing to position";
        targetPoint = target;
        targetAngle = angle;
        strafeToTarget = strafe;
        orderShip = _orderShip;
    }

    public override bool GetOrderActive()
    {
        return true;
    }

    public override string GetOrderInfo()
    {
        return order + $": {(int)targetPoint.x:F2}, {(int)targetPoint.y:F2}";
    }

    public override float GetTargetAngle()
    {
        return targetAngle;
    }

    public override Vector2 GetTargetPosition()
    {
        return targetPoint;
    }
}

public record ShipOrderFollow : ShipOrder
{
    private Ship targetShip;

    public ShipOrderFollow(Ship _orderShip, Ship _target, bool _strafe)
    {
        order = "following target";
        targetShip = _target;
        strafeToTarget = _strafe;
        orderShip = _orderShip;
    }

    public override bool GetOrderActive()
    {
        return targetShip != null && targetShip.IsAlive;
    }

    public override string GetOrderInfo()
    {
        return order + $": {targetShip.ShipClass}-{targetShip.ShipClassSuffix} {targetShip.ShipType}";
    }

    public override float GetTargetAngle()
    {
        var _targetVector = targetShip.transform.position - orderShip.transform.position;
        return Mathf.Atan2(_targetVector.y, _targetVector.x) * Mathf.Rad2Deg;
    }

    public override Vector2 GetTargetPosition()
    {
        return targetShip.transform.position;
    }
}
