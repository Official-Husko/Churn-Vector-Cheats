using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ChurnVectorCheats
{
    [BepInPlugin("husko.churnvector.cheats", "Churn Vector Cheats", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private bool invertDressed = false;
        private bool invertSee = false;
        private int inflateAll = 0;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin Churn Vector Cheats v1.0.0 is loaded!");
        }

        private void Update()
        {
            // Check for key press to toggle door objects
            if ((Keyboard.current != null && Keyboard.current.numpad1Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit1Key.wasPressedThisFrame))
            {
                ToggleDoorObjects();
            }

            // Check for key press to toggle sky and fog objects
            if ((Keyboard.current != null && Keyboard.current.numpad2Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit2Key.wasPressedThisFrame))
            {
                ToggleSkyAndFogObjects();
            }

            // Check for key press to toggle ball colliders
            if ((Keyboard.current != null && Keyboard.current.numpad3Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit3Key.wasPressedThisFrame))
            {
                ToggleBallColliders();
            }

            // Check for key press to toggle character clothes
            if ((Keyboard.current != null && Keyboard.current.numpad4Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit4Key.wasPressedThisFrame))
            {
                ToggleCharacterClothes();
            }
            // Check for key press to toggle character visibility
            if ((Keyboard.current != null && Keyboard.current.numpad5Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit5Key.wasPressedThisFrame))
            {
                ToggleCharacterVisibility();
            }
            
            // Check for key press to deflate all characters
            if ((Keyboard.current != null && Keyboard.current.numpad6Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit6Key.wasPressedThisFrame))
            {
                DeflateAllCharacters();
            }
            
            // Check for key press to inflate all characters
            if ((Keyboard.current != null && Keyboard.current.numpad7Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit7Key.wasPressedThisFrame))
            {
                InflateAllCharacters();
            }
            
            // Check for key press to increase condom's before break to 999
            if ((Keyboard.current != null && Keyboard.current.numpad8Key.wasPressedThisFrame) ||
                (Keyboard.current != null && Keyboard.current.altKey.isPressed && Keyboard.current.digit8Key.wasPressedThisFrame))
            {
                IncreaseCondomsBeforeBreak();
            }
        }

        private void ToggleDoorObjects()
        {
            string doorPattern = "DoorPrefab(\\s*\\(\\d+\\))*";
            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in objects)
            {
                if (Regex.IsMatch(obj.name, doorPattern))
                {
                    obj.SetActive(!obj.activeSelf);
                }
            }
        }

        private void ToggleSkyAndFogObjects()
        {
            string skyAndFogPattern = "Sky and Fog Volume";
            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in objects)
            {
                if (Regex.IsMatch(obj.name, skyAndFogPattern))
                {
                    obj.SetActive(!obj.activeSelf);
                }
            }
        }

        private void ToggleBallColliders()
        {
            string ballPattern = "Balls";
            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in objects)
            {
                if (Regex.IsMatch(obj.name, ballPattern))
                {
                    Collider collider = obj.GetComponent<Collider>();
                    if (collider != null)
                    {
                        collider.enabled = !collider.enabled;
                    }
                }
            }
        }

        private void ToggleCharacterClothes()
        {
            Logger.LogInfo("Get Naked?");
            CharacterAnimatorController[] characterControllers = Object.FindObjectsOfType<CharacterAnimatorController>();

            foreach (CharacterAnimatorController controller in characterControllers)
            {
                if (controller != null)
                {
                    controller.SetClothes(!invertDressed);
                }
                else
                {
                    Logger.LogError("CharacterAnimatorController component is null on a GameObject.");
                }
            }

            invertDressed = !invertDressed;
        }
        
        private void ToggleCharacterVisibility()
        {
            Logger.LogInfo("Be Invis");
            CharacterDetector[] characterDetectors = Object.FindObjectsOfType<CharacterDetector>();

            foreach (CharacterDetector detector in characterDetectors)
            {
                if (detector != null)
                {
                    detector.SetIgnorePlayer(invertSee);
                }
                else
                {
                    Logger.LogError("CharacterDetector component is null on a GameObject.");
                }
            }

            invertSee = !invertSee;
        }
        
        private void DeflateAllCharacters()
        {
            inflateAll--;
            Logger.LogInfo("Deflate " + inflateAll.ToString());
            CharacterAnimatorController[] characterControllers = Object.FindObjectsOfType<CharacterAnimatorController>();

            foreach (CharacterAnimatorController controller in characterControllers)
            {
                if (controller != null)
                {
                    controller.SetCumInflationAmount((float)inflateAll);
                }
                else
                {
                    Logger.LogError("CharacterAnimatorController component is null on a GameObject.");
                }
            }
        }
        
        private void InflateAllCharacters()
        {
            inflateAll++;
            Logger.LogInfo("Inflate " + inflateAll.ToString());
            CharacterAnimatorController[] characterControllers = Object.FindObjectsOfType<CharacterAnimatorController>();

            foreach (CharacterAnimatorController controller in characterControllers)
            {
                if (controller != null)
                {
                    controller.SetCumInflationAmount((float)inflateAll);
                }
                else
                {
                    Logger.LogError("CharacterAnimatorController component is null on a GameObject.");
                }
            }
        }
        
        private void IncreaseCondomsBeforeBreak()
        {
            Logger.LogInfo("Increase Condom's before break to 999");
            BreedingStand[] breedingStands = Object.FindObjectsOfType<BreedingStand>();

            foreach (BreedingStand stand in breedingStands)
            {
                System.Type type = stand.GetType();
                FieldInfo field = type.GetField("condomsAllowedUntilBreak", BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (field != null && field.FieldType == typeof(int))
                {
                    field.SetValue(stand, 999);
                }
            }
        }
    }
}
