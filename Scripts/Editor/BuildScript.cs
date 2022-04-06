
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kutil {
    public class BuildScript {

        public static string buildroot = "/builds/auto/";
        public static string gamename => PlayerSettings.productName;
        public static string ver => PlayerSettings.bundleVersion;

        [MenuItem("File/Build All")]
        static void BuildAll() {
            var scenes = EditorBuildSettings.scenes;
            BuildWindows();
            BuildOSX();
            BuildLinux();
            BuildWebGL();
        }

        static string GetLocation(string platform) {
            return $"{buildroot}{platform}/{gamename}{ver}_{platform}";
        }

        [MenuItem("File/Build Windows")]
        static void BuildWindows() {
            string platform = "win";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{buildroot}Windows/{gamename}{ver}_{platform}/{gamename}.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        }

        [MenuItem("File/Build Linux")]
        static void BuildLinux() {
            string platform = "linux";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{buildroot}Linux/{gamename}{ver}_{platform}/{gamename}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.None);
        }

        [MenuItem("File/Build OS X")]
        static void BuildOSX() {
            string platform = "mac";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{buildroot}MacOSX/{gamename}{ver}_{platform}.x64", BuildTarget.StandaloneOSX, BuildOptions.None);
        }

        [MenuItem("File/Build WebGL")]
        static void BuildWebGL() {
            string platform = "web";
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{buildroot}Web/{gamename}{ver}_{platform}", BuildTarget.WebGL, BuildOptions.None);
        }

        static void PerformAssetBundleBuild() {
            BuildPipeline.BuildAssetBundles("../AssetBundles/", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneLinux64);
            BuildPipeline.BuildAssetBundles("../AssetBundles/", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
            BuildPipeline.BuildAssetBundles("../AssetBundles/", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSX);
            BuildPipeline.BuildAssetBundles("../AssetBundles/", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
        }
    }
    // view rawBuildScript.cs hosted with ❤ by GitHub
}