using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System;
using Object = UnityEngine.Object;

// 这下面所有的await 都是UniTask的转化后的

public class UniTaskBaseTest : MonoBehaviour
{
    public Button LoadTextButton;
    public Text TargetText;

    public Button SwitchSceneButton;
    public Slider SwitchSceneSlider;

    public Button WebRequestButton;
    public Image DownloadImage;

    private void Start()
    {
        LoadTextButton.onClick.AddListener(OnClickLoadText);
        SwitchSceneButton.onClick.AddListener(OnClickSwitchScene);
        WebRequestButton.onClick.AddListener(OnClickWebRequest);
    }

    async void OnClickLoadText()
    {
        TargetText.text = "";

        // var loadOperation = Resources.LoadAsync<TextAsset>("text");
        // var text = await loadOperation;

        // TargetText.text = ((TextAsset)text).text;

        //好处是不需要继承Mono
        UniTaskResourceLoad loader = new UniTaskResourceLoad();
        TargetText.text = ((TextAsset)await loader.LoadAsync<TextAsset>("text")).text;
    }


    async void OnClickSwitchScene()
    {
        await SceneManager.LoadSceneAsync("Tutorial/Basic/Scenes/TargetLoadScene").ToUniTask(
            Progress.Create<float>((p) =>
            {
                SwitchSceneSlider.value = p;
            })
        );
    }

    async void OnClickWebRequest()
    {
        var webRequest = UnityWebRequestTexture.GetTexture("https://s1.hdslb.com/bfs/static/jinkela/video/asserts/33-coin-ani.png");
        var result = (await webRequest.SendWebRequest());
        var texture = ((DownloadHandlerTexture)result.downloadHandler).texture;

        int totalSpriteCount = 24;
        int perSpriteWidth = texture.width / totalSpriteCount;
        Sprite[] sprites = new Sprite[totalSpriteCount];
        for (int i = 0; i < totalSpriteCount; i++)
        {
            sprites[i] = Sprite.Create(texture,
                new Rect(new Vector2(perSpriteWidth * i, 0), new Vector2(perSpriteWidth, texture.height)),
                new Vector2(0.5f, 0.5f));
        }

        float perFrameTime = 0.1f;
        while (true)
        {
            for (int i = 0; i < totalSpriteCount; i++)
            {
                await Cysharp.Threading.Tasks.UniTask.Delay(TimeSpan.FromSeconds(perFrameTime));
                var sprite = sprites[i];
                DownloadImage.sprite = sprite;
            }
        }
    }
}


public class UniTaskResourceLoad
{
    public async UniTask<Object> LoadAsync<T>(string path) where T : Object
    {
        var asyncOperation = Resources.LoadAsync<T>(path);
        //使用Awaiter返回结果
        return await asyncOperation;
    }
}
