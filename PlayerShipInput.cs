using UnityEngine;

public class PlayerShipInput : MonoBehaviour, IShipInputProvider
{
    public Ship attachedShip;

    public int cruiseControl;

    void Awake()
    {
        attachedShip.ShipInputProvider = this;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            cruiseControl = Mathf.Clamp(cruiseControl + 1, -3, 3);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            cruiseControl = Mathf.Clamp(cruiseControl - 1, -3, 3);
        }
        else if (Input.GetKeyDown(KeyCode.C) || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.001f)
        {
            cruiseControl = 0;
        }
    }

    public ShipInputData GetInput()
    {
        ShipInputData input;

        input.Horizontal = Input.GetAxisRaw("Horizontal");
        input.HorizontalLimit = Mathf.Abs(input.Horizontal);

        if (cruiseControl != 0)
        {
            input.Vertical = Mathf.Sign(cruiseControl);
            input.VerticalLimit = Mathf.Abs(cruiseControl/3f);
        }
        else
        {
            input.Vertical = Input.GetAxisRaw("Vertical");
            input.VerticalLimit = Mathf.Abs(input.Vertical);
        }

        input.Turn = Input.GetAxisRaw("Turn");

        return input;
    }
}