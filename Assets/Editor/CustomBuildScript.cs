using UnityEditor;
using System.IO;
using UnityEngine;

public class CustomBuildScript 
{
    [MenuItem("Tools/Build")]
    public static void PerformBuild()
    { 
        string pathArgs = Path.Combine(Application.dataPath, "Args.txt");
        string[] arguments = File.ReadAllLines(pathArgs);
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/TestScene/3_MyInstanceShader.unity"};
        buildPlayerOptions.locationPathName = arguments[0];
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

}
