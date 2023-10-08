using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Device.Screen;

namespace HMFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class HMFix : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(ResolutionPatches));
        }

        [HarmonyPatch]
        public class ResolutionPatches
        {
            private const  float             OriginalAspectRatio            = 1.7777778f;
            // Set screen match mode when object has canvas scaler enabled
            [HarmonyPatch(typeof(CanvasScaler), "OnEnable")]
            [HarmonyPostfix]
            public static void SetScreenMatchMode(CanvasScaler __instance)
            {
                var currentAspectRatio = (float)Screen.currentResolution.width / Screen.currentResolution.height;
                if (currentAspectRatio is > OriginalAspectRatio or < OriginalAspectRatio)
                {
                    __instance.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                }
            }
            
            [HarmonyPatch(typeof(ui.TitleUI), nameof(ui.TitleUI.ChangeFullScreen))]
            [HarmonyPrefix]
            public static bool CustomResolutionPatch()
            {
                if (Screen.fullScreen) {
                    Screen.SetResolution(3440, 1440, FullScreenMode.Windowed);
                    return false;
                }
                Screen.SetResolution(3440,1440, FullScreenMode.FullScreenWindow);
                return false;
            }
        }
    }
}
