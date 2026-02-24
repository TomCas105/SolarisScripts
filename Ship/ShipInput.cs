public struct ShipInputData
{
    public float Horizontal;
    public float Vertical;
    public float Turn;
    public float HorizontalLimit;
    public float VerticalLimit;
}

public interface IShipInputProvider
{
    ShipInputData GetInput();
}