using UnityEngine;

public class FunctionUnlockView : MonoBehaviour
{
    [SerializeField] private string functionId;

    private UnlockFuncContext Context => UnlockFuncContext.Instance;

    private void Start()
    {
        Refresh();

        if (Context != null)
            Context.OnFunctionUnlocked += HandleFunctionUnlocked;
    }

    private void OnDestroy()
    {
        if (Context != null)
            Context.OnFunctionUnlocked -= HandleFunctionUnlocked;
    }

    private void HandleFunctionUnlocked(string unlockedId)
    {
        if (unlockedId != functionId)
            return;

        Refresh();
    }

    private void Refresh()
    {
        bool unlocked =
            Context != null &&
            Context.IsUnlocked(functionId);

        gameObject.SetActive(unlocked);
    }
}