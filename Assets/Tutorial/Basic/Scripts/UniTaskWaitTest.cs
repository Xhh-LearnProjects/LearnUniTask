using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.LowLevel;

public class UniTaskAsyncSample_Wait
{
    public async UniTask<int> WaitYield(PlayerLoopTiming loopTiming)
    {
        await UniTask.Yield(loopTiming);
        return 0;
    }

    public async UniTask<int> WaitNextFrame()
    {
        await UniTask.NextFrame();
        return Time.frameCount;
    }

    public async UniTask<int> WaitEndOfFrame(MonoBehaviour behaviour)
    {
        await UniTask.WaitForEndOfFrame(behaviour);
        return Time.frameCount;
    }
}

public class UniTaskWaitTest : MonoBehaviour
{
    public PlayerLoopTiming TestYieldTiming = PlayerLoopTiming.PreUpdate;
    public Button TestDelayButton;
    public Button TestDelayFrameButton;
    public Button TestYieldButton;
    public Button TestNextFrameButton;
    public Button TestEndOfFrameButton;
    public Button ClearButton;

    public Text ShowLogText;

    private List<PlayerLoopSystem.UpdateFunction> _injectUpdateFunctions = new List<PlayerLoopSystem.UpdateFunction>();
    private UniTaskAsyncSample_Wait unitaskWaiter;
    private bool _showUpdateLog = false;

    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += ShowLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceivedThreaded -= ShowLog;
    }

    private void ShowLog(string condition, string stacktrace, LogType type)
    {
        ShowLogText.text = $"{ShowLogText.text}\n{condition}";
    }


    private void Start()
    {
        TestDelayButton.onClick.AddListener(OnClickTestDelay);
        TestDelayFrameButton.onClick.AddListener(OnClickTestDelayFrame);
        TestYieldButton.onClick.AddListener(OnClickTestYield);
        TestNextFrameButton.onClick.AddListener(OnClickTestNextFrame);
        TestEndOfFrameButton.onClick.AddListener(OnClickTestEndOfFrame);
        ClearButton.onClick.AddListener(OnClickClear);

        unitaskWaiter = new UniTaskAsyncSample_Wait();
        InjectFunction();
    }

    private void InjectFunction()
    {
        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        var subsystems = playerLoop.subSystemList;
        playerLoop.updateDelegate += OnUpdate;
        for (int i = 0; i < subsystems.Length; i++)
        {
            int index = i;
            PlayerLoopSystem.UpdateFunction injectFunction = () =>
            {
                if (!_showUpdateLog) return;
                Debug.Log($"执行子系统 {_showUpdateLog} {subsystems[index]} 当前帧 {Time.frameCount}");
            };
            _injectUpdateFunctions.Add(injectFunction);
            subsystems[i].updateDelegate += injectFunction;
        }

        PlayerLoop.SetPlayerLoop(playerLoop);
    }

    private void OnUpdate()
    {
        Debug.Log($"当前帧{Time.frameCount}");
    }

    private async void OnClickTestDelay()
    {
        Debug.Log($"执行Delay开始，当前时间{Time.time}");
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        Debug.Log($"执行Delay结束，当前时间{Time.time}");
    }

    private async void OnClickTestDelayFrame()
    {
        Debug.Log($"执行DelayFrame开始，当前帧{Time.frameCount}");
        await UniTask.DelayFrame(5);
        Debug.Log($"执行DelayFrame结束，当前帧{Time.frameCount}");
    }

    private void OnClickClear()
    {
        ShowLogText.text = "Log:";
    }


    private async void OnClickTestEndOfFrame()
    {
        _showUpdateLog = true;
        Debug.Log($"执行WaitEndOfFrame开始");
        await unitaskWaiter.WaitEndOfFrame(this);
        Debug.Log($"执行WaitEndOfFrame结束");
        _showUpdateLog = false;
    }

    private async void OnClickTestNextFrame()
    {
        _showUpdateLog = true;
        Debug.Log($"执行NextFrame开始");
        await unitaskWaiter.WaitNextFrame();
        Debug.Log($"执行NextFrame结束");
        _showUpdateLog = false;
    }

    private async void OnClickTestYield()
    {
        _showUpdateLog = true;
        Debug.Log($"执行yield开始{TestYieldTiming}");
        await unitaskWaiter.WaitYield(TestYieldTiming);
        Debug.Log($"执行yield结束{TestYieldTiming}");
        _showUpdateLog = false;
    }
}
