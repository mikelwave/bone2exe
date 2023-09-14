using UnityEngine;

[ExecuteInEditMode]
public class ChainReactionStart : MonoBehaviour
{
    [Tooltip ("Set the position of the first lock tile to activate.")]
    public Vector2Int startPoint;
    public void Trigger()
    {
        GameMaster.StartChainReaction((Vector3Int)startPoint);
    }
    #if UNITY_EDITOR
    void Update()
    {
        if(!Application.isPlaying)
        {
            Vector2 pos = (Vector2)transform.position;
            Debug.DrawLine(transform.position,(Vector2)startPoint+(Vector2.one*0.5f),Color.red);
        }
    }
    #endif
}
