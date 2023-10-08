using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using HMFix.Tools;
using Screen = UnityEngine.Device.Screen;

namespace HMFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Harvest Moon The Winds of Anthos.exe")]
    public partial class HMFix : BaseUnityPlugin
    {
        private static ManualLogSource Log;
        
        private void Awake()
        {
            Log = base.Logger;
            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            // Reads or creates our config file.
            InitConfig();
            LoadGraphicsSettings(); // Initializes our graphics options
            var createdFramelimiter = InitializeFramelimiter();
            if (createdFramelimiter) {
                Log.LogInfo("Created Framelimiter.");
            }
            else { Log.LogError("Couldn't create Framelimiter Actor."); }
            Harmony.CreateAndPatchAll(typeof(ResolutionPatches));
        }
        
        private static bool InitializeFramelimiter()
        {
            var frObject = new GameObject {
                name = "FramerateLimiter",
                transform = {
                    position = new Vector3(0, 0, 0),
                    rotation = Quaternion.identity
                }
            };
            DontDestroyOnLoad(frObject);
            var frLimiterComponent = frObject.AddComponent<FramerateLimitManager>();
            frLimiterComponent.fpsLimit = (double)Screen.currentResolution.refreshRate / _iFrameInterval.Value;
            return true;
        }

        [HarmonyPatch]
        public class ResolutionPatches
        {
            private const float OriginalAspectRatio = 1.7777778f;
            
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
                    Screen.SetResolution(HMFix._iHorizontalResolution.Value, HMFix._iVerticalResolution.Value, FullScreenMode.Windowed);
                    return false;
                }
                Screen.SetResolution(HMFix._iHorizontalResolution.Value, HMFix._iVerticalResolution.Value, FullScreenMode.FullScreenWindow);
                Application.targetFrameRate = 0; // Disables any external framelimit from Unity. We will be using our own framerate limiting logic anyways.
                QualitySettings.vSyncCount = HMFix._bvSync.Value ? 1 : 0;
                return false;
            }
        }
    }
}
