namespace MukiaEngine.UI;

public struct UIVec
{
    public UIAxis X;
    public float ScaleX
    {
        get => X.Scale;
        set
        {
            X.Scale = value;
        }
    }
    public int OffsetX
    {
        get => X.Offset;
        set
        {
            X.Offset = value;
        }
    }

    public UIAxis Y;
    public float ScaleY
    {
        get => Y.Scale;
        set
        {
            Y.Scale = value;
        }
    }
    public int OffsetY
    {
        get => Y.Offset;
        set
        {
            Y.Offset = value;
        }
    }

    public UIVec(UIAxis x, UIAxis y)
    {
        X = x;
        Y = y;
    }

    public UIVec(float scaleX, float scaleY)
    {
        ScaleX = scaleX;
        ScaleY = scaleY;
    }

    public UIVec(int offsetX, int offsetY)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public UIVec(int offsetX, float scaleX, int offsetY, float scaleY)
    {
        OffsetX = offsetX;
        ScaleX = scaleX;

        OffsetY = offsetY;
        ScaleX = scaleX;
    }
}

public struct UIAxis(float scale = 0, int offset = 0)
{
    public float Scale = scale;
    public int Offset = offset;
}