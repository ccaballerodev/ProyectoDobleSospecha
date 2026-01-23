using TMPro;
using UnityEngine;

public class JoinUI : MonoBehaviour
{
    public TMP_InputField joinCodeInput;
    public RelayManager relay;

    public void Join()
    {
        relay.Client(joinCodeInput.text);
    }
}
