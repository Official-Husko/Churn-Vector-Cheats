using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ChurnVectorCheats
{
    [BepInPlugin("husko.churnvector.cheats", "Churn Vector Cheats", MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private enum Tab
        {
            MainCheats,
            SpecialCheats
        }

        private Tab _currentTab = Tab.MainCheats;
        private bool _showMenu;
        private Rect _menuRect = new(20, 20, 250, 240); // Initial position and size of the menu
        
        // Define separate arrays to store activation status for each tab
        private readonly bool[] _mainCheatsActivated = new bool[8];
        private readonly bool[] _specialCheatsActivated = new bool[2]; // Adjust the size as per your requirement
        
        private float _inflationLevel; // Variable to track inflation level
        private int _breedingStandUses = 3;
        private const string VersionLabel = MyPluginInfo.PLUGIN_VERSION;
        private static bool _invertDressed = true;
        private static bool _invertSee = true;
        private GameObject _orbitCameraObject;
        private MonoBehaviour _orbitCameraComponent;

        // List to store button labels and corresponding actions for the current cheats tab
        private readonly List<(string label, Action action)> _mainCheatsButtonActions = new()
        {
            ("Toggle Doors", ToggleDoorObjects),
            ("Toggle Sky and Fog", ToggleSkyAndFogObjects),
            ("Toggle Ball Colliders", ToggleBallColliders),
            ("Toggle Character Clothes", ToggleCharacterClothes),
            ("Toggle Character Visibility", ToggleCharacterVisibility),
            // Add more buttons and actions here
        };

        // Modify the ghostModeButtonActions list to include a button for Special Cheats
        private readonly List<(string label, Action action)> _specialCheatsButtonActions = new()
        {
            ("Flying Ghost Cock", ToggleFlyingGhostCock),
            ("Infinite Cumming", ToggleDickCumCumming),
            // Add more buttons for Special Cheats here
        };

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin Churn Vector Cheats v{VersionLabel} loaded!");
        }

        private void Update()
        {
            // Toggle menu visibility with Insert or F1 key
            if (Keyboard.current.insertKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
            {
                _showMenu = !_showMenu;
            }
        }

        private void OnGUI()
        {
            if (_showMenu)
            {
                // Find the GameObject with the name "OrbitCamera" if not found already
                if (_orbitCameraObject == null)
                {
                    _orbitCameraObject = GameObject.Find("OrbitCamera");
                }

                // Find the component named "OrbitCamera" inside the orbitCameraObject if not found already
                if (_orbitCameraObject != null && _orbitCameraComponent == null)
                {
                    _orbitCameraComponent = _orbitCameraObject.GetComponent("OrbitCamera") as MonoBehaviour;
                }

                // Disable the OrbitCamera component when the menu is open
                if (_orbitCameraComponent != null)
                {
                    _orbitCameraComponent.enabled = false;
                }


                // Unlock the cursor and make it visible when the menu is open
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Apply dark mode GUI style
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f);

                // Draw the IMGUI window
                _menuRect = GUI.Window(0, _menuRect, MenuWindow, "----< Cheats Menu >----");

                // Calculate position for version label at bottom left corner
                float versionLabelX = _menuRect.xMin + 10; // 10 pixels from left edge
                float versionLabelY = _menuRect.yMax - 20; // 20 pixels from bottom edge

                // Draw version label at bottom left corner
                GUI.contentColor = new Color(0.5f, 0.5f, 0.5f); // Dark grey silver color
                GUI.Label(new Rect(versionLabelX, versionLabelY, 100, 20), "v" + VersionLabel);

                // Calculate the width of the author label
                float authorLabelWidth = GUI.skin.label.CalcSize(new GUIContent("by Official-Husko")).x + 10; // Add some extra width for padding

                // Calculate position for author label at bottom right corner
                float authorLabelX = _menuRect.xMax - authorLabelWidth; // 10 pixels from right edge
                float authorLabelY = versionLabelY + 2; // Align with version label

                // Draw the author label as a clickable label
                if (GUI.Button(new Rect(authorLabelX, authorLabelY, authorLabelWidth, 20), "<color=cyan>by</color> <color=yellow>Official-Husko</color>", GUIStyle.none))
                {
                    // Open a link in the user's browser when the label is clicked
                    Application.OpenURL("https://github.com/Official-Husko/Churn-Vector-Cheats");
                }
            }
            else
            {

                // Enable the OrbitCamera component when the menu is closed
                if (_orbitCameraComponent != null)
                {
                    _orbitCameraComponent.enabled = true;
                }

                // Lock the cursor and hide it when the menu is closed
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void MenuWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, _menuRect.width, 20)); // Make the whole window draggable

            GUILayout.BeginVertical(); // Begin a vertical group for menu elements

            // Draw tabs
            GUILayout.BeginHorizontal();
            DrawTabButton(Tab.MainCheats, "Main Cheats");
            DrawTabButton(Tab.SpecialCheats, "Special Cheats");
            GUILayout.EndHorizontal();

            // Draw content based on the selected tab
            switch (_currentTab)
            {
                case Tab.MainCheats:
                    DrawMainCheatsTab();
                    break;
                case Tab.SpecialCheats:
                    DrawSpecialCheatsTab();
                    break;
            }

            GUILayout.EndVertical(); // End the vertical group
        }

        // Method to draw a tab button
        private void DrawTabButton(Tab tab, string label)
        {
            GUI.backgroundColor = _currentTab == tab ? Color.grey : Color.white; // Change background color based on the selected tab
            if (GUILayout.Button(label))
            {
                _currentTab = tab; // Set the current tab to the clicked tab
            }
        }
        
        // Helper method to get the activation status array for the current tab
        private bool[] GetCurrentTabActivationArray()
        {
            switch (_currentTab)
            {
                case Tab.MainCheats:
                    return _mainCheatsActivated;
                case Tab.SpecialCheats:
                    return _specialCheatsActivated;
                default:
                    return null;
            }
        }
        
        // Modify the button click handlers to use the correct activation status array based on the current tab
        private void ToggleButtonActivation(int buttonIndex)
        {
            bool[] currentTabActivationArray = GetCurrentTabActivationArray();
            if (currentTabActivationArray != null && buttonIndex >= 0 && buttonIndex < currentTabActivationArray.Length)
            {
                currentTabActivationArray[buttonIndex] = !currentTabActivationArray[buttonIndex];
            }
        }

        // Method to draw content for the Main Cheats tab
        private void DrawMainCheatsTab()
        {
            GUILayout.BeginVertical();

            // Draw buttons from the list
            for (int i = 0; i < _mainCheatsButtonActions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                DrawActivationDot(_mainCheatsActivated[i]); // Draw activation dot based on activation status
                if (GUILayout.Button(_mainCheatsButtonActions[i].label))
                {
                    ToggleButtonActivation(i); // Toggle activation status
                    _mainCheatsButtonActions[i].action.Invoke(); // Invoke the action associated with the button
                }
                GUILayout.EndHorizontal();
            }
            
            DrawBreedingStandUsesOption();
            
            DrawInflationOption();

            GUILayout.EndVertical();
        }

        // Modify the DrawSpecialCheatsTab method to include buttons for Special Cheats and use optionActivated array
        private void DrawSpecialCheatsTab()
        {
            GUILayout.BeginVertical();

            // Draw buttons from the list
            for (int i = 0; i < _specialCheatsButtonActions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                DrawActivationDot(_specialCheatsActivated[i]); // Draw activation dot based on activation status
                if (GUILayout.Button(_specialCheatsButtonActions[i].label))
                {
                    ToggleButtonActivation(i); // Toggle activation status
                    _specialCheatsButtonActions[i].action.Invoke(); // Invoke the action associated with the button
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
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
                    controller.SetClothes(!_invertDressed);
                }
            }

            _invertDressed = !_invertDressed;
        }

        private static void ToggleCharacterVisibility()
        {
            Debug.Log("Toggle Character Visibility button clicked!");
            CharacterDetector[] characterDetectors = FindObjectsOfType<CharacterDetector>();

            foreach (CharacterDetector detector in characterDetectors)
            {
                if (detector != null)
                {
                    detector.SetIgnorePlayer(_invertSee);
                }
            }

            _invertSee = !_invertSee;
        }

        private void ToggleBreedingStandUses()
        {
            Debug.Log("Toggle Breeding Stand Uses button clicked!");

            BreedingStand[] breedingStands = FindObjectsOfType<BreedingStand>();

            foreach (BreedingStand stand in breedingStands)
            {
                var type = stand.GetType();
                var field = type.GetField("condomsAllowedUntilBreak", BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null && field.FieldType == typeof(int))
                {
                    field.SetValue(stand, _breedingStandUses);
                }
            }
        }
        
        // Method to handle button click for toggling Flying Ghost Cock
        private static void ToggleFlyingGhostCock()
        {
            Debug.Log("Toggle Ghost Mode button clicked!");

            // Find the player object in the scene
            GameObject player = GameObject.Find("Player(Clone)");

            if (player != null)
            {
                // Find the "Gradual transform test" object under the player
                Transform gradualTransformTest = player.transform.Find("Gradual transform test");

                if (gradualTransformTest != null)
                {
                    // Enable/disable the "Body" child object of the "Gradual transform test" object
                    Transform bodyTransform = gradualTransformTest.Find("Body");
                    if (bodyTransform != null)
                    {
                        bodyTransform.gameObject.SetActive(!bodyTransform.gameObject.activeSelf);
                    }
                    else
                    {
                        Debug.LogWarning("Body object not found under Gradual transform test!");
                    }
                }
                else
                {
                    Debug.LogWarning("Gradual transform test object not found under player!");
                }
            }
            else
            {
                Debug.LogWarning("Player object not found in the scene!");
            }
        }
        
        private static void ToggleDickCumCumming()
        {
            Debug.Log("Toggle DickCum.Cumming button clicked!");

            // Find the player object in the scene
            GameObject player = GameObject.Find("Player(Clone)");

            if (player != null)
            {
                // Get the DickCum component attached to the player object
                DickCum dickCum = player.GetComponent<DickCum>();

                if (dickCum != null)
                {
                    // Use reflection to access the private 'cumming' field and toggle its value
                    FieldInfo field = dickCum.GetType().GetField("cumming", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (field != null)
                    {
                        // Get the current value of the 'cumming' field
                        bool cummingValue = (bool)field.GetValue(dickCum);
                        // Set the new value of the 'cumming' field
                        field.SetValue(dickCum, !cummingValue);
                    }
                    else
                    {
                        Debug.LogWarning("DickCum 'cumming' field not found!");
                    }
                }
                else
                {
                    Debug.LogWarning("DickCum component not found on player!");
                }
            }
            else
            {
                Debug.LogWarning("Player object not found in the scene!");
            }
        }

        // Modify the DrawActivationDot method to consider the current tab and use the corresponding activation status array
        private void DrawActivationDot(bool activated)
        {
            GetCurrentTabActivationArray();
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
            if (Math.Abs(_inflationLevel - newInflationLevel) > 0)
            {
                // Update the inflation level
                _inflationLevel = newInflationLevel;

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
                    controller.SetCumInflationAmount(_inflationLevel);
                }
            }
        }

        // Helper method to draw inflation option
        private void DrawInflationOption()
        {
            GUILayout.BeginHorizontal();
            DrawActivationDot(_inflationLevel != 0); // Use inflation level to set dot color
            GUILayout.Label("Inflation");

            // Plus button for increasing inflation
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                AdjustAndApplyInflation(_inflationLevel + 0.1f); // Increase inflation by 0.1
            }

            // Minus button for decreasing inflation
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                AdjustAndApplyInflation(_inflationLevel - 0.1f); // Decrease inflation by 0.1
            }

            // Input field for inflation level
            float newInflationLevel;
            float.TryParse(GUILayout.TextField(_inflationLevel.ToString(CultureInfo.InvariantCulture), GUILayout.Width(40)), out newInflationLevel);

            // Check if the new inflation level is different from the current level
            if (Math.Abs(newInflationLevel - _inflationLevel) > 0)
            {
                AdjustAndApplyInflation(newInflationLevel); // Adjust inflation to the new value
            }

            GUILayout.EndHorizontal();
        }

        // Method to draw max condoms option
        private void DrawBreedingStandUsesOption()
        {
            GUILayout.BeginHorizontal();
            DrawActivationDot(_breedingStandUses != 3); // Use breeding stand uses value to set dot color
            GUILayout.Label("Breeding Stand Uses:");

            // Draw the text field and capture user input
            string inputText = GUILayout.TextField(_breedingStandUses.ToString(), GUILayout.Width(40));

            // Parse the input text to check if it's a valid integer
            if (int.TryParse(inputText, out int newMaxUses))
            {
                // Check if the new value is different from the current value
                if (newMaxUses != _breedingStandUses)
                {
                    // Update the breeding stand uses value
                    _breedingStandUses = newMaxUses;

                    // Execute the corresponding code for the new input value
                    // For example, you can call a method here
                    ToggleBreedingStandUses();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}