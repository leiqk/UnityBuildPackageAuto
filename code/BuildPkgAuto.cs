using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

/// <summary>
/// 打包工具类
/// leiqk 314332613@qq.com
/// </summary>
public class BuildPkgAuto
{
    /// <summary>
    /// 所有要打包的场景
    /// </summary>
    /// <value>The get build scenes.</value>
    static string[] GetBuildScenes
    {
        get
        {
            List<string> list = new List<string>();
            foreach (EditorBuildSettingsScene es in EditorBuildSettings.scenes)
            {
                if (es != null && es.enabled)
                {
                    list.Add(es.path);
                }
            }
            return list.ToArray();
        }
    }

    static BuildTargetGroup CurrenBuildTarget
    {
        get
        {
            string[] strs = System.Environment.GetCommandLineArgs();
            foreach (string str in strs)
            {
                if (str.StartsWith("typePlatform"))
                {
                    string stra = str.Replace("typePlatform=", "");
                    switch (stra)
                    {
                        case "1":
                            return BuildTargetGroup.Android;
                        case "2":
                            return BuildTargetGroup.iOS;
                        default:
                            return BuildTargetGroup.Android;
                    }
                }
            }
            return BuildTargetGroup.Android;
        }
    }

    private static string versionBuild = "1.1.1";
    public static string VersionBuild
    {
        get
        {
            string[] strs = System.Environment.GetCommandLineArgs();
            foreach (string str in strs)
            {
                if (str.StartsWith("version"))
                {
                    return str.Replace("version=", "");
                }

            }
            return versionBuild;
        }
    }

    private static string installVersion = "1";
    public static string InstallVersion
    {
        get
        {
            string[] strs = System.Environment.GetCommandLineArgs();
            foreach (string str in strs)
            {
                if (str.StartsWith("InstallVersion"))
                {
                    return str.Replace("InstallVersion=", "");
                }

            }
            return installVersion;
        }
    }

    private static bool isNeedAB = false;
    public static bool IsNeedAB
    {
        get
        {
            string[] strs = System.Environment.GetCommandLineArgs();
            foreach (string str in strs)
            {
                if (str.StartsWith("isNeedAB"))
                {
                    int outValue = 0;
                    if (int.TryParse(str.Replace("isNeedAB=", ""), out outValue))
                    {
                        isNeedAB = outValue == 1 ? true : false;
                        return isNeedAB;
                    }
                }

            }
            return isNeedAB;
        }
    }

    private static int versionMode = 0;
    /// <summary>
    /// 版本模式，0 - dev，1 - test，2 - preview， 3 - release
    /// </summary>
    public static int VersionMode
    {
        get
        {
            string[] strs = System.Environment.GetCommandLineArgs();
            foreach (string str in strs)
            {
                if (str.StartsWith("versionMode"))
                {
                    versionMode = int.Parse(str.Replace("versionMode=", ""));
                    return versionMode;
                }
            }
            return versionMode;
        }
    }


    static string pathOut = Path.GetFullPath(Application.dataPath + "/../../apk/");
    /// <summary>
    /// 生成到到路径
    /// </summary>
    /// <value>The outpath.</value>
    static string outpath
    {
        get
        {
            if (!Directory.Exists(pathOut))
            {
                Directory.CreateDirectory(pathOut);
            }

            string name = "mgx_" + DateTime.Now.ToString("H_mm_ss");
#if UNITY_IOS
            return path + name + ".apk";
#elif UNITY_ANDROID
            return pathOut + name + ".apk";
#endif
        }
    }

    public enum ChannelType
    {
        ymcx = 1,
        zjtd = 2,
    }

    /// <summary>
    /// 渠道类型
    /// </summary>
    static ChannelType channel = ChannelType.ymcx;
    static ChannelType Channel
    {
        get
        {
            string[] strs = System.Environment.GetCommandLineArgs();
            foreach (string str in strs)
            {
                if (str.StartsWith("channel"))
                {
                    string channelStr = str.Replace("channel=", "");
                    int outInt = 0;
                    if (int.TryParse(channelStr, out outInt))
                    {
                        channel = (ChannelType)outInt;
                        return channel;
                    }
                }
            }
            return channel;
        }
    }

    //宏编译选项
    static readonly string BUILD_MODE_DEV = "TRACE;DEBUG;UNITY_ANDROID;BUILD_MODE_DEV;DEBUG_COLLIDER;ENABLE_PROFILER";
    static readonly string BUILD_MODE_TEST = "TRACE;DEBUG;UNITY_ANDROID;BUILD_MODE_TEST;DEBUG_COLLIDER;ENABLE_PROFILER";
    static readonly string BUILD_MODE_PREVIEW = "TRACE;RELASE;UNITY_ANDROID;BUILD_MODE_PREVIEW";
    static readonly string BUILD_MODE_RELEASE = "TRACE;RELASE;UNITY_ANDROID;BUILD_MODE_RELEASE";

    //[UnityEditor.MenuItem("Tools/bulid")]
    public static void BuildProjected()
    {
        //根据选择编译库文件
        BuildCodeToDll();

        if (IsNeedAB)
        {
            Debug.LogError("生成AB : " + IsNeedAB);
            YMFrame.AssetbundlesMenuItems.BuildAssetBundles();
            AssetDatabase.Refresh();
            Debug.Log("生成ab完成");
        }

        //测试参数
        /*string outStr = "IsNeedAb :" + IsNeedAB + "\n";
        outStr += "VersionBuild :" + VersionBuild + "\n";
        outStr += "InstallVersion :" + InstallVersion + "\n";
        outStr += "VersionMode :" + VersionMode + "\n";
        outStr += "Channel :" + Channel + "\n";

        string[] strs = System.Environment.GetCommandLineArgs();
        foreach (var str in strs)
        {
            outStr += str + "\n";
        }

        OnException(outStr);
        */

        Debug.Log("准备生成APK");

        if (CurrenBuildTarget == BuildTargetGroup.Android)
        {
            BuildForAndroid();
        }
        else if (CurrenBuildTarget == BuildTargetGroup.iOS)
        {
            BuildForIos();
        }
        else if (CurrenBuildTarget == BuildTargetGroup.Standalone)
        {

        }
    }

    //编译源代码至库文件
    [MenuItem("Tools/TestBuildCode")]
    public static void BuildCodeToDll()
    {
        try
        {
            //根据输入的选项，替换编译宏
            ReplaceProjectCondition();

            //启动devenv 编译代码
            ProcessStartInfo procInfo = new ProcessStartInfo();
            Process proc = new Process();

            string targetProj = Application.dataPath + "sln文件路径";

            string[] arrStrParams;
            //preview和release，使用release模式
            if (VersionMode == 2 || VersionMode == 3)
            {
                arrStrParams = new string[] { "/build Release" };
            }
            else
            {
                arrStrParams = new string[] { "/build Debug"};
            }
            proc.StartInfo = procInfo;
            proc.StartInfo.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe";
            string argment = targetProj + " ";
            foreach(var item in arrStrParams)
            {
                argment += item + " ";
            }
            proc.StartInfo.Arguments = argment;

            proc.Start();
            proc.WaitForExit();
        }
        catch (Exception e)
        {
            Debug.LogError("BuildCodeToDll error :" + e.Message);
        }
        finally
        {
            Debug.Log("BuildCodeToDll success !");
        }
    }

    public static void ReplaceProjectCondition()
    {
        string projPath = Application.dataPath + "vsproject工程路径";
        FileStream fStream = null;
        try
        {
            fStream = File.Open(projPath, FileMode.Open, FileAccess.Read);

            byte[] bytes = new byte[fStream.Length];
            fStream.Read(bytes, 0, bytes.Length);
            fStream.Close();
            fStream = null;

            string str = System.Text.Encoding.UTF8.GetString(bytes);

            if (VersionMode == 0)
            {
                str = str.Replace(BUILD_MODE_TEST, BUILD_MODE_DEV);
                str = str.Replace(BUILD_MODE_PREVIEW, BUILD_MODE_DEV);
                str = str.Replace(BUILD_MODE_RELEASE, BUILD_MODE_DEV);
            }
            else if (VersionMode == 1)
            {
                str = str.Replace(BUILD_MODE_DEV, BUILD_MODE_TEST);
                str = str.Replace(BUILD_MODE_PREVIEW, BUILD_MODE_TEST);
                str = str.Replace(BUILD_MODE_RELEASE, BUILD_MODE_TEST);
            }
            else if (VersionMode == 2)
            {
                str = str.Replace(BUILD_MODE_DEV, BUILD_MODE_PREVIEW);
                str = str.Replace(BUILD_MODE_TEST, BUILD_MODE_PREVIEW);
                str = str.Replace(BUILD_MODE_RELEASE, BUILD_MODE_PREVIEW);
            }
            else if (VersionMode == 3)
            {
                str = str.Replace(BUILD_MODE_DEV, BUILD_MODE_RELEASE);
                str = str.Replace(BUILD_MODE_TEST, BUILD_MODE_RELEASE);
                str = str.Replace(BUILD_MODE_PREVIEW, BUILD_MODE_RELEASE);
            }

            fStream = File.Open(projPath, FileMode.Truncate, FileAccess.Write);
            var replaceByte = System.Text.Encoding.UTF8.GetBytes(str);
            fStream.Write(replaceByte, 0, replaceByte.Length);
            fStream.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError("export error:" + e.Message);
        }
        finally
        {
            fStream.Close();
        }
    }

    [MenuItem("Tools/Test")]
    public static void BuildTestOnUnity()
    {
        BuildForAndroid();
    }

    /// <summary>
    /// 打包Android的apk
    /// </summary>
    public static void BuildForAndroid()
    {
        try
        {
            PlayerSettings.Android.useAPKExpansionFiles = false;
            PlayerSettings.Android.keystoreName = Application.dataPath + "/../keystore/android.keystore";
            PlayerSettings.Android.keyaliasName = "android.keystore";
            PlayerSettings.Android.keystorePass = "Ymcx5201@";
            PlayerSettings.Android.keyaliasPass = "Ymcx5201@";

            string[] scenes = null;
            scenes = GetBuildScenes;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            PlayerSettings.bundleVersion = VersionBuild;

            BuildPipeline.BuildPlayer(scenes, outpath, BuildTarget.Android, BuildOptions.None);
        }
        catch (Exception e)
        {
            OnException(e.Message);
        }
    }

    /// <summary>
    /// 打包ios平台
    /// </summary>
    public static void BuildForIos()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2);
        PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
        PlayerSettings.iOS.backgroundModes = iOSBackgroundMode.Audio | iOSBackgroundMode.RemoteNotification;

        string[] scenes = null;

        scenes = GetBuildScenes;
#if UNITY_IOS
        PlayerSettings.iOS.allowHTTPDownload = true;
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        string res = BuildPipeline.BuildPlayer(scenes, outpath, BuildTarget.iOS, BuildOptions.None);
#endif
    }

    /// <summary>
    /// 异常时，把异常信息写入文件
    /// </summary>
    /// <param name="info"></param>
    static void OnException(string info)
    {
        string cfgPath = pathOut + "/" + DateTime.Now.ToString("H-mm-ss") + ".txt";
        FileStream fStream = null;
        try
        {
            fStream = File.Open(cfgPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var bytes = System.Text.Encoding.UTF8.GetBytes(info);
            fStream.Write(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("export error:" + e.Message);
        }
        finally
        {
            fStream.Close();
        }
    }
}