using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CallbackSample : MonoBehaviour
{
    public Button CallbackButton;

    public GameObject Target;

    public const float G = 9.8f;

    public float FallTime = 0.5f;


    private void Start()
    {
        CallbackButton.onClick.AddListener(UniTask.UnityAction(OnClickCallback));
    }


    private async UniTask FallTarget(Transform targetTrans, float fallTime, System.Action onHalf, UniTaskCompletionSource source)
    {
        float startTime = Time.time;

        Vector3 startPosition = targetTrans.position;
        float lastElapsedTime = 0;
        while (Time.time - startTime <= fallTime)
        {
            float elapsedTime = Mathf.Min(Time.time - startTime, fallTime);
            if (lastElapsedTime < fallTime * 0.5f && elapsedTime >= fallTime * 0.5f)
            {
                onHalf?.Invoke();
                source.TrySetResult();
                // 失败
                // source.TrySetException(new SystemException());
                // 取消
                // source.TrySetCanceled(someToken);

                // 泛型类UniTaskCompletionSource<T> SetResult是T类型，返回UniTask<T>
            }

            lastElapsedTime = elapsedTime;
            float fallY = 0 + 0.5f * G * elapsedTime * elapsedTime;
            targetTrans.position = startPosition + Vector3.down * fallY;
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }
    }

    private async UniTaskVoid OnClickCallback()
    {
        float time = Time.time;
        // source 就是为了手动控制
        UniTaskCompletionSource source = new UniTaskCompletionSource();
        FallTarget(Target.transform, FallTime, OnTargetHalf, source).Forget();
        // 当一半时间后 就可以执行下面代码
        await source.Task;// UniTaskCompletionSource产生的UnitTask是可以复用的
        Debug.Log($"当前缩放{Target.transform.localScale} 耗时 {Time.time - time}秒");
    }

    private void OnTargetHalf()
    {
        Target.transform.localScale *= 1.5f;
    }
}
