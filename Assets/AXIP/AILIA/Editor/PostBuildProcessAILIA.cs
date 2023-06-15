//必要なフレームワークの追加

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
#endif
namespace ailiaSDK
{
    public class PostBuildProcessAILIA
    {

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
        // Add Framework to build
#if UNITY_IOS
        string projPath = Path.Combine (path, "Unity-iPhone.xcodeproj/project.pbxproj");

        PBXProject proj = new PBXProject ();
        proj.ReadFromString (File.ReadAllText (projPath));

#if UNITY_2019_1_OR_NEWER
        string target =  proj.GetUnityFrameworkTargetGuid();
#else
        string target = proj.TargetGuidByName ("Unity-iPhone");
#endif

        List<string> frameworks = new List<string> () {
            "Accelerate.framework",
            "MetalPerformanceShaders.framework"
        };

        foreach (var framework in frameworks) {
            proj.AddFrameworkToProject (target, framework, false);
        }

        File.WriteAllText (projPath, proj.WriteToString ());
#endif

        // Copy License File If Exist
        if(buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64 || 
           buildTarget == BuildTarget.StandaloneLinux || buildTarget == BuildTarget.StandaloneLinux64 || buildTarget == BuildTarget.StandaloneLinuxUniversal){
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.dataPath);
            System.IO.FileInfo[] files = di.GetFiles("*.lic", System.IO.SearchOption.AllDirectories);
            foreach (System.IO.FileInfo f in files) {
                string copyTo=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), f.Name);
                System.IO.File.Copy(f.FullName,copyTo,true);
            }
        }

        }
    }
}
