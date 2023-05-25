using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UniTaskTutorial.Advance.Scripts
{
    public class AsyncReactivePropertySample: MonoBehaviour
    {
        private AsyncReactiveProperty<int> currentHp;
        public int maxHp = 100;
        public float totalChangeTime = 1f;
        public Text ShowHpText;
        public Text StateText;
        public Text ChangeText;


        public Slider HpSlider;
        public Image HpBarImage;

        public Button HealButton;
        public Button HurtButton;


        private int maxHeal = 10;
        private int maxHurt = 10;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _linkedTokenSource;
        
        
        private void Start()
        {
            // 设置AsyncReactiveProperty
            currentHp = new AsyncReactiveProperty<int>(maxHp);
            HpSlider.maxValue = maxHp;
            HpSlider.value = maxHp;
            
            currentHp.Subscribe(OnHpChange);
            CheckHpChange(currentHp).Forget();
            CheckFirstLowHp(currentHp).Forget();
            
            currentHp.BindTo(ShowHpText);
            
            HealButton.onClick.AddListener(OnClickHeal);
            HurtButton.onClick.AddListener(OnClickHurt);
            
            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, this.GetCancellationTokenOnDestroy());
        }

        private async UniTaskVoid CheckHpChange(AsyncReactiveProperty<int> hp)
        {
            int hpValue = hp.Value;
            await hp.WithoutCurrent().ForEachAsync((_, index) =>
            {
                ChangeText.text = $"血量发生变化 第{index}次 变化{hp.Value - hpValue}";
                hpValue = hp.Value;
            }, this.GetCancellationTokenOnDestroy());
        }


        private void OnClickHeal()
        {
            ChangeHp(Random.Range(0, maxHeal));
        }
        
        private void OnClickHurt()
        {
            ChangeHp(-Random.Range(0, maxHurt));
        }

        private void ChangeHp(int deltaHp)
        {
            currentHp.Value = Mathf.Clamp(currentHp.Value + deltaHp, 0, maxHp);
        }

        private async UniTaskVoid CheckFirstLowHp(AsyncReactiveProperty<int> hp)
        {
            await hp.FirstAsync((value) => value < maxHp * 0.4f, this.GetCancellationTokenOnDestroy());
            StateText.text = "首次血量低于界限，请注意!";
        }

        private async UniTaskVoid OnHpChange(int hp)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, this.GetCancellationTokenOnDestroy());
            await SyncSlider(hp, _linkedTokenSource.Token);
        }

        /// <summary>
        /// 同步血条
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask SyncSlider(int hp, CancellationToken token)
        {
            var sliderValue = HpSlider.value;
            float needTime = Mathf.Abs((sliderValue - hp) / maxHp * totalChangeTime);
            float useTime = 0;
            while (useTime < needTime)
            {
                useTime += Time.deltaTime;
                bool result = await UniTask.Yield(PlayerLoopTiming.Update, token)
                    .SuppressCancellationThrow();
                if (result)
                {
                    return;
                }

                var newValue = (sliderValue + (hp - sliderValue) * (useTime / needTime));
                SetNewValue(newValue);
            }
        }

        private void SetNewValue(float newValue)
        {
            if (!HpSlider) return;
            HpSlider.value = newValue;
            HpBarImage.color = HpSlider.value / maxHp < 0.4f ? Color.red : Color.white;
        }
    }
}