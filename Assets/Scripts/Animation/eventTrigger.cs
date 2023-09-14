using UnityEngine;
using UnityEngine.Events;

public class eventTrigger : MonoBehaviour
{
    #region main
    void trigFunc()
    {
        OnTrigEvent?.Invoke();
    }
    #endregion
    [SerializeField]
    UnityEvent trigEvent = new UnityEvent();
    public UnityEvent OnTrigEvent { get { return trigEvent; } set { trigEvent = value; }}
}
