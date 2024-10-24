using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 注意：启用此组件可能需要提供隐私收集政策相关
/// 此参考<see cref="https://bugly.qq.com/docs/user-guide/instruction-manual-privacy/"/>
/// 代码仅供参考
/// </summary>
internal static class AliLogHandler
{
    private const bool IsEnable = false;
    private const string project = "cn-shanghai-intranet.log.aliyuncs.com";
    private const string host = "cn-shanghai.log.aliyuncs.com";
    private const string logstore = "firstlogstore";
    private const int MaxLen = 2048;

    private static string serviceAddr =
        $"http://{project}.{host}/logstores/{logstore}/track?APIVersion=0.6.0"; //&key1=val1&key2=val2

    private static int order = 0;
    private static string id = Guid.NewGuid().ToString("N");

    [ThreadStatic] private static StringBuilder _stringBuilder;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void OnLoad()
    {
        if (IsEnable)
        {
            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;
            //防止UnityEditor停止播放后仍将日志发送到服务器上
            Application.quitting += () => Application.logMessageReceived -= HandleLog;
            Debug.Log($"[{nameof(AliLogHandler)}]register done id={id}");
        }
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Application.logMessageReceived -= HandleLog;
            return;
        }
#endif
        switch (type)
        {
            case LogType.Error:
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                break;
            case LogType.Log:
                break;
            case LogType.Exception:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (_stringBuilder == null) _stringBuilder = new();

        _stringBuilder.Append(serviceAddr);
        _stringBuilder.Append($"&I=");//id
        _stringBuilder.Append(id);
        _stringBuilder.Append("&O="); //order
        _stringBuilder.Append(Interlocked.Increment(ref order));
        _stringBuilder.Append("&T="); //type
        _stringBuilder.Append((int)type);
        _stringBuilder.Append("&M="); //message
        _stringBuilder.Append(Uri.EscapeDataString(logString));
        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            _stringBuilder.Append("&S="); //stack
            _stringBuilder.Append(Uri.EscapeDataString(stackTrace));
        }

        //请求体太大，为了防止被路由屏蔽，截断后面的参数
        if (_stringBuilder.Length > MaxLen)
        {
            _stringBuilder.Remove(MaxLen, _stringBuilder.Length - MaxLen);
        }

        var url = _stringBuilder.ToString();
        _stringBuilder.Clear();
        var r = UnityWebRequest.Get(url);
        var operation = r.SendWebRequest();
        operation.completed += static asyncOperation =>
        {
            var o = (UnityWebRequestAsyncOperation)asyncOperation;
            o.webRequest.Dispose();
        };
    }
}