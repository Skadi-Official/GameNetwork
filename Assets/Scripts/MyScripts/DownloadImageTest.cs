
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyHTTPRequest
{
    const string ERR_InvalidArguments = "invalid arguments";
    const string ERR_Timeout = "timeout";
    
    const float DEFAULT_TIMEOUT = 5f;                        // 超时时间
    
    private UnityWebRequest m_Request;                      // 实际封装的Unity网络请求
    private Action<bool, byte[]> m_RequestFinishedAction;   // 请求完成时触发的回调，bool-请求是否成功，byte[]-返回的原始数据
    private string m_LastError = string.Empty;              // 最后一次收到的错误
    private float m_Timeout;                                // 网络请求被判定为超时的最终时间
    private float m_LastDownloadProgress = 0f;              // 记录最后获取的进度，与当前进度做对比
    
    public string LastError => m_LastError;                 // 对外的错误访问

    public float progress
    {
        get
        {
            if (m_Request != null)
            {
                return m_Request.downloadProgress;
            }
            return 0f;
        }
    }
    /// <summary>
    /// 发送特定的GET请求
    /// </summary>
    /// <param name="address">服务器地址</param>
    /// <param name="command">服务器的接口</param>
    /// <param name="onRequestFinished">请求完成的回调</param>
    /// <param name="args">GET 请求的 URL 参数</param>
    public void Get(string address, string command, Action<bool, byte[]> onRequestFinished, params object[] args)
    {
        // 参数不合法，直接返回
        if (args.Length % 2 != 0)
        {
            m_LastError = ERR_InvalidArguments;
            onRequestFinished.Invoke(false, null);
            return;
        }
        // 拼接完整的URL请求，分为三部分
        int argsLength = args.Length / 2;
        StringBuilder paramStr = new StringBuilder();
        for (int i = 0; i < argsLength; i++)
        {
            paramStr.Append(i == 0 ? "?" : "&");
            // params的key部分是固定的，而value可能包括特殊字符，所以只需要处理value
            paramStr.Append(args[i * 2] + "=" + Uri.EscapeDataString(args[i * 2 + 1].ToString()));
        }
        // 这里只处理address，因为command通常是固定ASCII，paramStr的value已经编码过
        // 不要对整个URL这样做，重复编码会破坏 URL
        string finalURL = Uri.EscapeUriString(address) + command + paramStr;
        
        // 拼接完成后实际发送请求
        m_RequestFinishedAction = onRequestFinished;
        m_Timeout = Time.time + DEFAULT_TIMEOUT;
        m_Request = new UnityWebRequest(finalURL);
        m_Request.downloadHandler = new DownloadHandlerBuffer();
        m_Request.SendWebRequest();
    }

    
    /// <summary>
    /// 发送特定的Post请求
    /// </summary>
    /// <param name="address">服务器地址</param>
    /// <param name="command">服务器接口</param>
    /// <param name="onRequestFinished">请求完成回调</param>
    /// <param name="data">POST请求体</param>
    /// <param name="header">自定义请求头</param>
    public void Post(string address, string command, Action<bool, byte[]> onRequestFinished, byte[] data, Dictionary<string, string> header = null)
    {
        string url = Uri.EscapeUriString(address) + command;
        m_RequestFinishedAction = onRequestFinished;
        if (data != null && data.Length == 0) data = null;
        m_Timeout = Time.time + DEFAULT_TIMEOUT;
        // 开始组装请求
        m_Request = new UnityWebRequest(url, "POST");
        // UploadHandlerRaw 可以直接发送 byte[] 数据
        m_Request.uploadHandler = new UploadHandlerRaw(data);
        // DownloadHandlerBuffer 会把服务器返回的数据全部缓存到内存里（byte[]）
        m_Request.downloadHandler = new DownloadHandlerBuffer();
        if (header != null)
        {
            foreach (var pair in header)
            {
                // 设置自定义 HTTP 请求头，遍历 header 字典，把每一对 Key-Value 设置到请求里
                m_Request.SetRequestHeader(pair.Key, pair.Value);
            }
        }
        m_Request.SendWebRequest();
    }
    
    /// <summary>
    /// 检查网络请求状态，这个方法应该被每帧或者定时调用
    /// </summary>
    public void CheckPendingRequest()
    {
        if(m_Request==null) return;
        if (m_Request.isDone)
        {
            // 当请求完成时我们就尝试触发回调，如果请求失败，error返回错误描述字符串；如果成功，没有内容。
            m_LastError = m_Request.error;
            m_RequestFinishedAction.Invoke(string.IsNullOrEmpty(m_LastError), m_Request.downloadHandler.data);
            Dispose();
        }
        else
        {
            if (m_Request.downloadProgress > m_LastDownloadProgress)
            {
                // 如果进度还有在变化，那么重置超时的最终时间
                m_LastDownloadProgress = m_Request.downloadProgress;
                m_Timeout = Time.time + DEFAULT_TIMEOUT;
            }
            // 请求没有完成时要判断是否超时
            if (Time.time <= m_Timeout) return;
            // 超时触发回调
            m_LastError = ERR_Timeout;
            m_RequestFinishedAction.Invoke(false, null);
            Dispose();
        }
    }

    private void Dispose()
    {
        m_Request.Dispose();
        m_Request = null;
        m_RequestFinishedAction = null;
    }
}

public class DownloadImageTest : MonoBehaviour
{
    public Image image;

    private MyHTTPRequest m_RequestHelper = new MyHTTPRequest();
    private bool m_IsLoaded = false;

    private const string ImageURL =
        "https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/60593745-10ed-4768-a257-37f9bfbaa024/dfd12iu-c57e0d16-9624-4ee1-8d78-4e54f1bad35e.png/v1/fit/w_512,h_512,q_70,strp/seamless_brick_wall_texture_512_512px_by_thomasbaijot_dfd12iu-375w-2x.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9NTEyIiwicGF0aCI6IlwvZlwvNjA1OTM3NDUtMTBlZC00NzY4LWEyNTctMzdmOWJmYmFhMDI0XC9kZmQxMml1LWM1N2UwZDE2LTk2MjQtNGVlMS04ZDc4LTRlNTRmMWJhZDM1ZS5wbmciLCJ3aWR0aCI6Ijw9NTEyIn1dXSwiYXVkIjpbInVybjpzZXJ2aWNlOmltYWdlLm9wZXJhdGlvbnMiXX0.66m616YqWjL7SLJf37uicBqFc1PGcxzbWIYu8WS21cc";

    private void Start()
    {
        // 使用封装的 GET 方法发送请求
        m_RequestHelper.Get(
            ImageURL,       // address（这里直接用完整 URL）
            "",           // command，这里空字符串即可
            OnImageDownloaded
        );
    }

    private void Update()
    {
        // 每帧检查请求状态
        if (!m_IsLoaded)
        {
            m_RequestHelper.CheckPendingRequest();
        }
    }

    /// <summary>
    /// GET 请求完成的回调
    /// </summary>
    private void OnImageDownloaded(bool success, byte[] data)
    {
        if (!success)
        {
            Debug.LogError($"Failed to load image, {m_RequestHelper.LastError}");
            return;
        }
        Debug.Log("GET Success, start Load Image");
        var tex = new Texture2D(1, 1);
        if (tex.LoadImage(data))
        {
            image.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
            m_IsLoaded = true;
            Debug.Log("load finish");
        }
        else
        {
            Debug.LogError("Failed to create texture from downloaded data");
        }
    }
}
