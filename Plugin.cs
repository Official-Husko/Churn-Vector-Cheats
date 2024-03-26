using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ChurnVectorCheats
{
    [BepInPlugin("husko.churnvector.cheats", "Churn Vector Cheats", MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private bool showMenu = false;
        private Rect menuRect = new Rect(20, 20, 250, 215); // Initial position and size of the menu
        private bool[] optionActivated = new bool[8]; // Array to store activation status for each option
        private float inflationLevel = 0; // Variable to track inflation level
        private int breedingStandUses = 3;
        private string versionLabel = MyPluginInfo.PLUGIN_VERSION;
        private static bool invertDressed = true;
        private static bool invertSee = true;
        private GameObject orbitCameraObject;
        private MonoBehaviour orbitCameraComponent;
        
        // List to store button labels and corresponding actions
        private List<(string label, Action action)> buttonActions = new List<(string label, Action action)>
        {
            ("Toggle Doors", ToggleDoorObjects),
            ("Toggle Sky and Fog", ToggleSkyAndFogObjects),
            ("Toggle Ball Colliders", ToggleBallColliders),
            ("Toggle Character Clothes", ToggleCharacterClothes),
            ("Toggle Character Visibility", ToggleCharacterVisibility),
            // Add more buttons and actions here
        };

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin Churn Vector Cheats v{versionLabel} loaded!");
        }

        private void Update()
        {
            // Toggle menu visibility with Insert or F1 key
            if (Keyboard.current.insertKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
            {
                showMenu = !showMenu;
            }
        }

        private void OnGUI()
        {
            if (showMenu)
            {
                // Find the GameObject with the name "OrbitCamera" if not found already
                if (orbitCameraObject == null)
                {
                    orbitCameraObject = GameObject.Find("OrbitCamera");
                }

                // Find the component named "OrbitCamera" inside the orbitCameraObject if not found already
                if (orbitCameraObject != null && orbitCameraComponent == null)
                {
                    orbitCameraComponent = orbitCameraObject.GetComponent("OrbitCamera") as MonoBehaviour;
                }

                // Disable the OrbitCamera component when the menu is open
                if (orbitCameraComponent != null)
                {
                    orbitCameraComponent.enabled = false;
                }

                
                // Unlock the cursor and make it visible when the menu is open
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Apply dark mode GUI style
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

                // Draw the IMGUI window
                menuRect = GUI.Window(0, menuRect, MenuWindow, "Test Menu");

                // Calculate position for version label at bottom left corner
                float versionLabelX = menuRect.xMin + 10; // 10 pixels from left edge
                float versionLabelY = menuRect.yMax - 20; // 20 pixels from bottom edge

                // Draw version label at bottom left corner
                GUI.contentColor = new Color(0.5f, 0.5f, 0.5f); // Dark grey silver color
                GUI.Label(new Rect(versionLabelX, versionLabelY, 100, 20), "v" + versionLabel);

                // Calculate the width of the author label
                float authorLabelWidth = GUI.skin.label.CalcSize(new GUIContent("by Official-Husko")).x + 10; // Add some extra width for padding

                // Calculate position for author label at bottom right corner
                float authorLabelX = menuRect.xMax - authorLabelWidth; // 10 pixels from right edge
                float authorLabelY = versionLabelY + 2; // Align with version label

                // Draw the author label as a clickable label
                if (GUI.Button(new Rect(authorLabelX, authorLabelY, authorLabelWidth, 20), "<color=cyan>by</color> <color=yellow>Official-Husko</color>", GUIStyle.none))
                {
                    // Open a link in the user's browser when the label is clicked
                    Application.OpenURL("https://example.com");
                }
            }
            else
            {
                
                // Enable the OrbitCamera component when the menu is closed
                if (orbitCameraComponent != null)
                {
                    orbitCameraComponent.enabled = true;
                }
                
                // Lock the cursor and hide it when the menu is closed
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void MenuWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, menuRect.width, 20)); // Make the whole window draggable
            
            GUILayout.BeginVertical(); // Begin a vertical group for menu elements

            // Draw buttons from the list
            for (int i = 0; i < buttonActions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                DrawActivationDot(optionActivated[i]); // Draw activation dot based on activation status
                if (GUILayout.Button(buttonActions[i].label))
                {
                    optionActivated[i] = !optionActivated[i]; // Toggle activation status
                    buttonActions[i].action.Invoke(); // Invoke the action associated with the button
                }
                GUILayout.EndHorizontal();
            }

            // Draw max Condoms option
            DrawBreedingStandUsesOption();
    
            // Draw inflation option at the bottom
            DrawInflationOption();

            GUILayout.EndVertical(); // End the vertical group
        }

        // Method to handle button click for toggling doors
        private static void ToggleDoorObjects()
        {
            Debug.Log("Toggle Doors button clicked!");
            string doorPattern = "DoorPrefab(\\s*\\(\\d+\\))*";

            // Find all GameObjects in the scene, including inactive ones
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // Check if the name of the GameObject matches the pattern
                if (Regex.IsMatch(obj.name, doorPattern))
                {
                    // Toggle the active state of the GameObject
                    obj.SetActive(!obj.activeSelf);
                }
            }
        }

        // Method to handle button click for toggling sky and fog
        private static void ToggleSkyAndFogObjects()
        {
            Debug.Log("Toggle Sky and Fog button clicked!");
            string skyAndFogPattern = "Sky and Fog Volume";

            // Find all GameObjects in the scene, including inactive ones
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // Check if the name of the GameObject matches the pattern
                if (obj.name.Contains(skyAndFogPattern))
                {
                    // Toggle the active state of the GameObject
                    obj.SetActive(!obj.activeSelf);
                }
            }
        }
        // Method to handle button click for toggling balls collision
        private static void ToggleBallColliders()
        {
            Debug.Log("Toggle Balls button clicked!");
            string ballPattern = "Balls";

            // Find all GameObjects in the scene, including inactive ones
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                // Check if the name of the GameObject matches the pattern
                if (Regex.IsMatch(obj.name, ballPattern))
                {
                    // Get the collider component attached to the GameObject
                    Collider collider = obj.GetComponent<Collider>();
            
                    // If the collider component exists
                    if (collider != null)
                    {
                        // Toggle the enabled state of the collider
                        collider.enabled = !collider.enabled;
                    }
                }
            }
        }
        
        private static void ToggleCharacterClothes()
        {
            Debug.Log("Toggle Character Clothes button clicked!");
            CharacterAnimatorController[] characterControllers = FindObjectsOfType<CharacterAnimatorController>();

            foreach (CharacterAnimatorController controller in characterControllers)
            {
                if (controller != null)
                {
                    controller.SetClothes(!invertDressed);
                }
            }

            invertDressed = !invertDressed;
        }
        
        private static void ToggleCharacterVisibility()
        {
            Debug.Log("Toggle Character Visibility button clicked!");
            CharacterDetector[] characterDetectors = FindObjectsOfType<CharacterDetector>();

            foreach (CharacterDetector detector in characterDetectors)
            {
                if (detector != null)
                {
                    detector.SetIgnorePlayer(invertSee);
                }
            }

            invertSee = !invertSee;
        }

        private void ToggleBreedingStandUses()
        {
            Debug.Log("Toggle Breeding Stand Uses button clicked!");

            BreedingStand[] breedingStands = FindObjectsOfType<BreedingStand>();

            foreach (BreedingStand stand in breedingStands)
            {
                System.Type type = stand.GetType();
                FieldInfo field = type.GetField("condomsAllowedUntilBreak", BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null && field.FieldType == typeof(int))
                {
                    field.SetValue(stand, breedingStandUses);
                }
            }
        }

        // Method to draw activation dot
        private void DrawActivationDot(bool activated)
        {
            GUILayout.Space(10); // Add some space to center the dot vertically
            Color dotColor = activated ? Color.green : Color.red; // Determine dot color based on activation status
            GUIStyle dotStyle = new GUIStyle(GUI.skin.label); // Create a new GUIStyle for the dot label
            dotStyle.normal.textColor = dotColor; // Set the color of the dot label
            GUILayout.Label("●", dotStyle, GUILayout.Width(20), GUILayout.Height(20)); // Draw dot with the specified style
        }
        
        // Method to adjust inflation level and apply it to characters
        private void AdjustAndApplyInflation(float newInflationLevel)
        {
            // Check if the new inflation level is different from the current level
            if (inflationLevel != newInflationLevel)
            {
                // Update the inflation level
                inflationLevel = newInflationLevel;

                // Apply inflation to all characters
                ApplyInflationToCharacters();
            }
        }

        // Method to apply inflation to all characters
        private void ApplyInflationToCharacters()
        {
            CharacterAnimatorController[] characterControllers = FindObjectsOfType<CharacterAnimatorController>();

            foreach (CharacterAnimatorController controller in characterControllers)
            {
                if (controller != null)
                {
                    controller.SetCumInflationAmount(inflationLevel);
                }
            }
        }

        // Helper method to draw inflation option
        private void DrawInflationOption()
        {
            GUILayout.BeginHorizontal();
            DrawActivationDot(inflationLevel != 0); // Use inflation level to set dot color
            GUILayout.Label("Inflation");

            // Plus button for increasing inflation
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                AdjustAndApplyInflation(inflationLevel + 0.1f); // Increase inflation by 0.1
            }

            // Minus button for decreasing inflation
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                AdjustAndApplyInflation(inflationLevel - 0.1f); // Decrease inflation by 0.1
            }

            // Input field for inflation level
            float newInflationLevel;
            float.TryParse(GUILayout.TextField(inflationLevel.ToString(), GUILayout.Width(40)), out newInflationLevel);

            // Check if the new inflation level is different from the current level
            if (newInflationLevel != inflationLevel)
            {
                AdjustAndApplyInflation(newInflationLevel); // Adjust inflation to the new value
            }

            GUILayout.EndHorizontal();
        }
        
        // Method to draw max condos option
        private void DrawBreedingStandUsesOption()
        {
            GUILayout.BeginHorizontal();
            DrawActivationDot(breedingStandUses != 3); // Use breeding stand uses value to set dot color
            GUILayout.Label("Breeding Stand Uses:");

            // Draw the text field and capture user input
            string inputText = GUILayout.TextField(breedingStandUses.ToString(), GUILayout.Width(40));

            // Parse the input text to check if it's a valid integer
            if (int.TryParse(inputText, out int newMaxUses))
            {
                // Check if the new value is different from the current value
                if (newMaxUses != breedingStandUses)
                {
                    // Update the breeding stand uses value
                    breedingStandUses = newMaxUses;

                    // Execute the corresponding code for the new input value
                    // For example, you can call a method here
                    ToggleBreedingStandUses();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}
