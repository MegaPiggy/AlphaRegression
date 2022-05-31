using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlphaRegression
{
    public class AlphaRegression : ModBehaviour
    {
        private TitleAnimationController gfxController;
        private static AssetBundle alpha;

        private void Awake()
        {
            alpha = LoadAssetbundle("alpha");
        }

        private void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(AlphaRegression)} loaded", MessageType.Success);
            SceneManager.sceneLoaded += OnSceneLoaded;

            //TitleScreen is already open
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (gfxController != null) gfxController.OnTitleLogoAnimationComplete -= OnTitleLogoAnimationComplete;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "TitleScreen") return;

            ModHelper.Console.WriteLine("Title screen has loaded!", MessageType.Success);

            Find("TitleCanvasHack/TitleLayoutGroup/OW_Logo_Anim/OW_Logo_Anim/OUTER").transform.localScale = Vector3.zero;
            Find("TitleCanvasHack/TitleLayoutGroup/OW_Logo_Anim/OW_Logo_Anim/WILDS").transform.localScale = Vector3.zero;

            gfxController = Find("TitleMenuManagers").GetComponent<TitleScreenManager>()._gfxController;
            gfxController.OnTitleLogoAnimationComplete += OnTitleLogoAnimationComplete;

            OWAudioSource menuSource = Find("Scene/AudioSource_Music").GetComponent<OWAudioSource>();
            AudioClip mainMenuMusic = alpha.LoadAsset<AudioClip>("Main Title 050913 AP");
            menuSource.clip = mainMenuMusic;
            menuSource._audioLibraryClip = AudioType.None;
            menuSource._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.MANUAL;
            menuSource.Stop();
            menuSource.Play();

            GameObject background = Find("Scene/Background");
            GameObject planetPivot = Find("Scene/Background/PlanetPivot");
            GameObject campfire = Find("Scene/Background/PlanetPivot/Prefab_HEA_Campfire");
            GameObject planetRoot = Find("Scene/Background/PlanetPivot/PlanetRoot");
            Vector3 position = planetRoot.transform.position;
            Quaternion rotation = planetRoot.transform.rotation;
            campfire.SetActive(false);
            planetRoot.SetActive(false);

            GameObject planetRootAlpha = Instantiate(alpha.LoadAsset<GameObject>("AlphaPlanetRoot"), position, rotation, planetPivot.transform);
            Transform root = planetRootAlpha.transform.Find("PlanetRoot").transform;
            root.localPosition = new Vector3(0, 2, 0);
            root.localEulerAngles = new Vector3(15, 0, 350);

            ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                foreach (GameObject child in GetAllChildren(background))
                {
                    if (child.name == "Pivot")
                        GameObject.Destroy(child);
                }
            });
        }

        public void OnTitleLogoAnimationComplete()
        {
            Texture2D newLogo = alpha.LoadAsset<Texture2D>("Logo_Fall_White");

            var logoSize = new Vector2(newLogo.width, newLogo.height);

            var logo = Find("TitleCanvasHack/TitleLayoutGroup/OW_Logo_Anim/OW_Logo_Anim");
            var image = logo.GetAddComponent<UnityEngine.UI.Image>();
            image.sprite = Sprite.Create(newLogo, new Rect(Vector2.zero, logoSize), logoSize / 2f);
            image.color = new Color32(62,124,40,byte.MaxValue);//new Color32(56,160,37,byte.MaxValue);

            var root = Find("TitleCanvasHack/TitleLayoutGroup/OW_Logo_Anim");
            root.transform.localRotation = Quaternion.Euler(0, 0, 0);
            root.transform.localScale = new Vector3(5, 2.5f, 1);
        }

        public static AssetBundle LoadAssetbundle(string name)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(AlphaRegression), name);
            if (stream == null) throw new System.Exception("AssetBundle " + name + " was not found");
            return AssetBundle.LoadFromStream(stream);
        }

        public static GameObject Find(string path)
        {
            try
            {
                var go = GameObject.Find(path);

                var names = path.Split(new char[] { '\\', '/' });
                if (go == null)
                {

                    // Get the root object and hope its the right one
                    var root = GameObject.Find(names[0]);
                    if (root == null) root = SceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name.Equals(names[0])).FirstOrDefault();

                    var t = root?.transform;
                    if (t != null)
                    {
                        for (int i = 1; i < names.Length; i++)
                        {
                            var child = t.transform.Find(names[i]);

                            if (child == null)
                            {
                                foreach (Transform c in t.GetComponentsInChildren<Transform>(true))
                                {
                                    if (c.name.Equals(names[i]))
                                    {
                                        child = c;
                                        break;
                                    }
                                }
                            }

                            if (child == null)
                            {
                                t = null;
                                break;
                            }

                            t = child;
                        }
                    }

                    go = t?.gameObject;
                }

                return go;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static List<GameObject> GetAllChildren(GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }
    }
}
