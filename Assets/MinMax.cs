[System.Serializable]
public class MinMax
{
    public float Max;
    public float Min;

    public MinMax()
    {
        Min = float.MaxValue;
        Max = float.MinValue;
    }

    public void AddValue(float v)
    {
        if (v > Max)
            Max = v;
        if (v < Min)
            Min = v;
    }
}
