using UnityEngine;
public class BoilState
{
    int[] range;
    public int FirstInt { get { return range[0];} }
    public int LastInt { get { return range[range.Length-1];} }
    public int Length { get { return range.Length; } }

    int currentIndex = 0;

    public int GetIndex(int index)
    {
        return range[index];
    }
    public void IncreaseIndex()
    {
        currentIndex = Length > 1 ? (int)Mathf.Repeat(currentIndex+=1,Length) : 0;
    }
    public int GetIndexAndIncrease()
    {
        currentIndex = Length > 1 ? (int)Mathf.Repeat(currentIndex+=1,Length) : 0;
        return range[currentIndex];
    }

    public BoilState (int[] range)
    {
        this.range = range;
        currentIndex = 0;
    }
}
