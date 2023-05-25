using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PerformanceTest : MonoBehaviour
{
    public Button CoroutineRunButton;

    public Button UnitaskRunButton;

    public int LoopTimes;

    public Text CoroutineTestText;
    public Text UnitaskTestText;

    // Start is called before the first frame update
    void Start()
    {
        CoroutineRunButton.onClick.AddListener(OnClickCoroutineTest);
        UnitaskRunButton.onClick.AddListener(OnClickUnitTaskTest);
    }

    [Conditional("Enable_Test_Func")]
    private void TestFunc()
    {
    }

    private async void OnClickUnitTaskTest()
    {
        int times = 0;
        float elapsedTime = 0;
        while (times < LoopTimes)
        {
            times++;
            float time = Time.realtimeSinceStartup;
            var unitask = EmptyUnitTask();
            elapsedTime += (Time.realtimeSinceStartup - time);
            await unitask;
        }

        UnitaskTestText.text = $"UniTask耗时测试:{LoopTimes}次: 耗时{elapsedTime*1000:F6}毫秒";


    }

    private async UniTask EmptyUnitTask()
    {
        await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
    }

    private void OnClickCoroutineTest()
    {
        StartCoroutine(CoroutineTest());
    }

    private IEnumerator CoroutineTest()
    {
        float elapsedTime = 0;
        for (int i = 0; i < LoopTimes; i++)
        {
            float time = Time.realtimeSinceStartup;
            var coroutine = StartCoroutine(EmptyCoroutine());
            elapsedTime += (Time.realtimeSinceStartup - time);
            yield return coroutine;
        }

        CoroutineTestText.text = $"协程耗时测试:{LoopTimes}次: 耗时{elapsedTime*1000:F6}豪秒";
    }

    private IEnumerator EmptyCoroutine()
    {
        yield return null;
    }
}