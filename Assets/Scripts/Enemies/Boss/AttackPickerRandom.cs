using System.Linq;
using UnityEngine;

// Generation method is pocket based - each attack in list is performed once in random order
[RequireComponent (typeof (BossGlobalStats))]
public class AttackPickerRandom : MonoBehaviour
{
    int index = 0;
    int[] attackArray;
    byte attackLength;
    BossGlobalStats bossGlobalStats;

    // Shuffle list
    int[] Shuffle(int [] array)
    {
        int halfWidth = attackLength/2;
        int opCount = halfWidth + (array.Length % 2 != 0 ? 1 : 0);
        for(int i = 0; i<opCount;i++)
        {
            if(Random.Range(0,halfWidth*2) == 0) continue;
            // Get element from other half of array
            int randomIndex = Random.Range(0,halfWidth)+(Random.Range(0,2) == 0 ? 0 : opCount);
            int temp = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = temp; 
        }
        return array;
    }
    // Generate a list of attacks to follow
    void GenerateNewList()
    {
        attackArray = Enumerable.Range(0, attackLength).ToArray();
        attackArray = Shuffle(attackArray);
        
        string s = "";
        foreach(int i in attackArray)
        {
            s+=i;
        }
        ///print(s);
    }
    // Pick the next attack from the list
    public int GetNextAttack(bool increment)
    {
        if(increment)
        index++;
        if(index == attackLength)
        {
            GenerateNewList();
            index = 0;
        }
        return attackArray[index];
    }

    // Start is called before the first frame update
    void Awake()
    {
        bossGlobalStats = GetComponent<BossGlobalStats>();
        attackLength = (byte)bossGlobalStats.attacks.Length;
        GenerateNewList();
    }
}
