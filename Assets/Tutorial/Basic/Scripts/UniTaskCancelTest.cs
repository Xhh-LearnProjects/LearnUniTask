using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UniTaskCancelTest : MonoBehaviour
{
    public Runner FirstRunner;
    public Runner SecondRunner;

    public Button FirstRunButton;
    public Button SecondRunButton;
    public Button ResetButton;

    public Button FirstCancelButton;
    public Button SecondCancelButton;

    public float TotalDistance = 15;

    private CancellationTokenSource _firstCancelToken;
    private CancellationTokenSource _secondCancelToken;
    private CancellationTokenSource _linkedCancelToken;

    public Text FirstText;
    public Text SecondText;

    private void Start()
    {
        FirstRunButton.onClick.AddListener(OnClickFirstRun);
        SecondRunButton.onClick.AddListener(OnClickSecondRun);

        FirstCancelButton.onClick.AddListener(OnClickFirstCancel);
        SecondCancelButton.onClick.AddListener(OnClickSecondCancel);

        ResetButton.onClick.AddListener(OnClickReset);
        _firstCancelToken = new CancellationTokenSource();
        // 注意这里可以直接先行设置多久以后取消
        // _firstCancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(1.5f));
        _secondCancelToken = new CancellationTokenSource();
        _linkedCancelToken =
            CancellationTokenSource.CreateLinkedTokenSource(_firstCancelToken.Token, _secondCancelToken.Token);
    }

    private async void OnClickFirstRun()
    {
        try
        {
            await RunSomeOne(FirstRunner, _firstCancelToken.Token);
        }
        //第一种取消方式 通过try catch捕获到取消
        catch (OperationCanceledException e)
        {
            FirstText.text = ("1号跑已经被取消");
        }
    }

    private async void OnClickSecondRun()
    {
        //这种方式性能更好
        //另外还是用了_linkedCancelToken 这样点击一号球停止 二号求也会同步停止
        var (cancelled, _) = await RunSomeOne(SecondRunner, _linkedCancelToken.Token).SuppressCancellationThrow();
        if (cancelled)
        {
            SecondText.text = ("2号跑已经被取消");
        }
    }

    private void OnClickFirstCancel()
    {
        _firstCancelToken.Cancel();
        _firstCancelToken.Dispose();
        _firstCancelToken = new CancellationTokenSource();
        _linkedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(_firstCancelToken.Token, _secondCancelToken.Token);
    }

    private void OnClickSecondCancel()
    {
        _secondCancelToken.Cancel();
        //因为只能使用一次 Dispose 掉然后创建新的继续执行
        _secondCancelToken.Dispose();
        _secondCancelToken = new CancellationTokenSource();
        _linkedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(_firstCancelToken.Token, _secondCancelToken.Token);
    }



    private void OnDestroy()
    {
        _firstCancelToken.Dispose();
        _secondCancelToken.Dispose();
        _linkedCancelToken.Dispose();
    }



    private async UniTask<int> RunSomeOne(Runner runner, CancellationToken cancellationToken)
    {
        runner.Reset();
        float totalTime = TotalDistance / runner.Speed;
        float timeElapsed = 0;
        while (timeElapsed <= totalTime)
        {
            timeElapsed += Time.deltaTime;
            await UniTask.NextFrame(cancellationToken);


            float runDistance = Mathf.Min(timeElapsed, totalTime) * runner.Speed;
            runner.Target.position = runner.StartPos + Vector3.right * runDistance;
        }

        runner.ReachGoal = true;
        return 0;
    }


    private void OnClickReset()
    {
        _firstCancelToken.Cancel();
        _firstCancelToken = new CancellationTokenSource();
        _secondCancelToken = new CancellationTokenSource();
        _linkedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(_firstCancelToken.Token, _secondCancelToken.Token);
        FirstRunner.Reset();
        SecondRunner.Reset();
        FirstText.text = "";
        SecondText.text = "";
    }
}
