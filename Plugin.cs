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
            ("Continous Cumming", ToggleContinuousCumming),
            // Add more buttons for Special Cheats here
        };

        /// <summary>
        /// Initializes the plugin on Awake event
        /// </summary>
        private void Awake()
        {
            // Log the plugin's version number and successful startup
            Logger.LogInfo($"Plugin Churn Vector Cheats v{VersionLabel} loaded!");
        }

        /// <summary>
        /// Handles toggling the menu on and off with the Insert or F1 key.
        /// </summary>
        private void Update()
        {
            // Toggle menu visibility with Insert or F1 key
            if (Keyboard.current.insertKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
            {
                _showMenu = !_showMenu;
            }
        }


        /// <summary>
        /// Handles drawing the menu and all of its elements on the screen.
        /// </summary>
        private void OnGUI()
        {
            // Only draw the menu if it's supposed to be shown
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

        /// <summary>
        /// Handles the GUI for the main menu
        /// </summary>
        /// <param name="windowID">The ID of the window</param>
        private void MenuWindow(int windowID)
        {
            // Make the whole window draggable
            GUI.DragWindow(new Rect(0, 0, _menuRect.width, 20));

            // Begin a vertical group for menu elements
            GUILayout.BeginVertical();

            // Draw tabs
            GUILayout.BeginHorizontal();
            // Draw the Main Cheats tab button
            DrawTabButton(Tab.MainCheats, "Main Cheats");
            // Draw the Special Cheats tab button
            DrawTabButton(Tab.SpecialCheats, "Special Cheats");
            GUILayout.EndHorizontal();

            // Draw content based on the selected tab
            switch (_currentTab)
            {
                // Draw the Main Cheats tab
                case Tab.MainCheats:
                    DrawMainCheatsTab();
                    break;
                // Draw the Special Cheats tab
                case Tab.SpecialCheats:
                    DrawSpecialCheatsTab();
                    break;
            }

            // End the vertical group
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a tab button
        /// </summary>
        /// <param name="tab">The tab to draw</param>
        /// <param name="label">The label to display on the button</param>
        private void DrawTabButton(Tab tab, string label)
        {
            // Change background color based on the selected tab
            GUI.backgroundColor = _currentTab == tab ? Color.grey : Color.white;

            // If the button is clicked, set the current tab to the clicked tab
            if (GUILayout.Button(label))
            {
                _currentTab = tab;
            }
        }
        
        /// <summary>
        /// Gets the activation status array for the currently selected tab
        /// </summary>
        /// <returns>The activation status array for the current tab. If the tab is not recognized, null is returned.</returns>
        private bool[] GetCurrentTabActivationArray()
        {
            switch (_currentTab)
            {
                case Tab.MainCheats:
                    // Return the activation status array for the main cheats tab
                    return _mainCheatsActivated;
                case Tab.SpecialCheats:
                    // Return the activation status array for the special cheats tab
                    return _specialCheatsActivated;
                default:
                    // If the tab is not recognized, return null
                    return null;
            }
        }
        
        /// <summary>
        /// Toggles the activation state of the button at the given index on the currently selected tab.
        /// If the index is not within the range of the activation status array for the current tab, nothing is done.
        /// </summary>
        /// <param name="buttonIndex">The index of the button to toggle activation status for</param>
        private void ToggleButtonActivation(int buttonIndex)
        {
            // Get the activation status array for the current tab. If the tab is not recognized, return.
            bool[] currentTabActivationArray = GetCurrentTabActivationArray();
            if (currentTabActivationArray == null)
            {
                return;
            }

            // If the index is within the range of the activation status array, toggle the activation status
            if (buttonIndex >= 0 && buttonIndex < currentTabActivationArray.Length)
            {
                currentTabActivationArray[buttonIndex] = !currentTabActivationArray[buttonIndex];
            }
        }

        /// <summary>
        /// Method to draw content for the Main Cheats tab
        /// </summary>
        private void DrawMainCheatsTab()
        {
            GUILayout.BeginVertical();

            // Draw buttons from the list
            for (int i = 0; i < _mainCheatsButtonActions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                DrawActivationDot(_mainCheatsActivated[i]); // Draw activation dot based on activation status
                
                // Draws a button for each cheat with the label, 
                // activation status, and invokes the action associated 
                // with the button when pressed
                if (GUILayout.Button(_mainCheatsButtonActions[i].label))
                {
                    ToggleButtonActivation(i); // Toggle activation status
                    _mainCheatsButtonActions[i].action.Invoke(); // Invoke the action associated with the button
                }
                GUILayout.EndHorizontal();
            }
            
            // Draws an option to toggle and edit breeding stands uses
            DrawBreedingStandUsesOption();
            
            // Draws an option to toggle and edit inflation
            DrawInflationOption();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the Special Cheats tab in the mod's UI
        /// </summary>
        private void DrawSpecialCheatsTab()
        {
            // Begin vertical layout for the tab
            GUILayout.BeginVertical();

            // Iterate through the list of special cheat buttons
            for (int i = 0; i < _specialCheatsButtonActions.Count; i++)
            {
                // Begin horizontal layout for the button row
                GUILayout.BeginHorizontal();

                // Draw an activation dot based on the activation status
                DrawActivationDot(_specialCheatsActivated[i]);

                // Draw a button for the special cheat
                if (GUILayout.Button(_specialCheatsButtonActions[i].label))
                {
                    // Toggle the activation status of the button
                    ToggleButtonActivation(i);

                    // Invoke the action associated with the button
                    _specialCheatsButtonActions[i].action.Invoke();
                }

                // End the horizontal layout for the button row
                GUILayout.EndHorizontal();
            }

            // End the vertical layout for the tab
            GUILayout.EndVertical();
        }


        /// <summary>
        /// Handles button click for toggling doors in the scene
        /// </summary>
        private static void ToggleDoorObjects()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Doors");

            // Pattern to match door GameObjects
            // Allows for door names to have optional (N) at the end, where N is a number
            string doorPattern = "DoorPrefab(\\s*\\(\\d+\\))*";

            // Find all GameObjects in the scene, including inactive ones
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            // Iterate through all GameObjects in the scene
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

        /// <summary>
        /// Handles button click for toggling sky and fog objects in the scene
        /// </summary>
        private static void ToggleSkyAndFogObjects()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Sky and Fog");

            // Pattern to match sky and fog GameObjects
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

        /// <summary>
        /// Handles button click for toggling balls collision
        /// </summary>
        private static void ToggleBallColliders()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Balls Collision");

            // Pattern to match ball GameObjects
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

        /// <summary>
        /// Toggles the visibility of the clothes on all characters in the scene
        /// </summary>
        private static void ToggleCharacterClothes()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Character Clothes");

            // Find all CharacterAnimatorControllers in the scene
            CharacterAnimatorController[] characterControllers = FindObjectsOfType<CharacterAnimatorController>();

            // Loop through each controller and toggle the clothes visibility
            foreach (CharacterAnimatorController controller in characterControllers)
            {
                if (controller != null)
                {
                    // Invert the state of the clothes visibility if the _invertDressed flag is set
                    controller.SetClothes(!_invertDressed);
                }
            }

            // Toggle the _invertDressed flag to keep track of the current state
            _invertDressed = !_invertDressed;
        }

        /// <summary>
        /// Toggles the visibility of all characters in the scene
        /// </summary>
        private static void ToggleCharacterVisibility()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Character Visibility");

            // Find all CharacterDetectors in the scene
            CharacterDetector[] characterDetectors = FindObjectsOfType<CharacterDetector>();

            // Loop through each CharacterDetector and toggle the visibility setting
            foreach (CharacterDetector detector in characterDetectors)
            {
                if (detector != null)
                {
                    // Invert the current visibility setting when toggling
                    detector.SetIgnorePlayer(_invertSee);
                }
            }

            // Toggle the _invertSee flag to keep track of the current state
            _invertSee = !_invertSee;
        }

        /// <summary>
        /// Toggles the number of uses each breeding stand can be used
        /// </summary>
        private void ToggleBreedingStandUses()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Breeding Stand Uses");

            // Find all breeding stands in the scene
            BreedingStand[] breedingStands = FindObjectsOfType<BreedingStand>();

            // Loop through each breeding stand and adjust its number of uses
            foreach (BreedingStand stand in breedingStands)
            {
                var type = stand.GetType();
                
                // Get a reference to the field that stores the number of uses
                var field = type.GetField("condomsAllowedUntilBreak", BindingFlags.Instance | BindingFlags.NonPublic);

                // If the field exists and is of the correct type, set its value
                if (field != null && field.FieldType == typeof(int))
                {
                    field.SetValue(stand, _breedingStandUses);
                }
            }
        }
        
        /// <summary>
        /// Handles button click for toggling Flying Ghost Cock.
        /// </summary>
        private static void ToggleFlyingGhostCock()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Flying Ghost Cock");

            // Find the player object in the scene
            GameObject player = GameObject.Find("Player(Clone)");

            if (player == null)
            {
                return;
            }

            // Find the "Gradual transform test" object under the player
            Transform gradualTransformTest = player.transform.Find("Gradual transform test");

            if (gradualTransformTest == null)
            {
                return;
            }

            // Enable/disable the "Body" child object of the "Gradual transform test" object
            Transform bodyTransform = gradualTransformTest.Find("Body");
            if (bodyTransform != null)
            {
                bodyTransform.gameObject.SetActive(!bodyTransform.gameObject.activeSelf);
            }
        }
        
        /// <summary>
        /// Toggles the continuous cumming for the player's Dick.
        /// </summary>
        private static void ToggleContinuousCumming()
        {
            // Debug log the action being performed
            Debug.Log("Toggle Continuous Cumming");

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
                }
            }
        }

        /// <summary>
        /// Draws a small dot with a green color if the activation status is true, and red if it's false.
        /// This method uses the current tab activation status array to determine the dot color.
        /// </summary>
        /// <param name="activated">The activation status to determine the dot color.</param>
        private void DrawActivationDot(bool activated)
        {
            GetCurrentTabActivationArray(); // Consider current tab activation status array
            GUILayout.Space(10); // Add some space to center the dot vertically
            Color dotColor = activated ? Color.green : Color.red; // Determine dot color based on activation status
            GUIStyle dotStyle = new GUIStyle(GUI.skin.label); // Create a new GUIStyle for the dot label
            dotStyle.normal.textColor = dotColor; // Set the color of the dot label
            GUILayout.Label("●", dotStyle, GUILayout.Width(20), GUILayout.Height(20)); // Draw dot with the specified style
        }

        /// <summary>
        /// Adjusts the inflation level and applies it to all characters if the new inflation level is different from the current level.
        /// </summary>
        /// <param name="newInflationLevel">The new inflation level to apply.</param>
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

        /// <summary>
        /// Applies the current inflation level to all characters in the scene.
        /// </summary>
        private void ApplyInflationToCharacters()
        {
            // Find all CharacterAnimatorControllers in the scene
            CharacterAnimatorController[] characterControllers = FindObjectsOfType<CharacterAnimatorController>();

            // Loop through each controller and apply the current inflation level
            foreach (CharacterAnimatorController controller in characterControllers)
            {
                // Check if the controller is not null
                if (controller != null)
                {
                    // Apply the current inflation level to the controller
                    controller.SetCumInflationAmount(_inflationLevel);
                }
            }
        }

        /// <summary>
        /// Draws the inflation option GUI. This includes a dot indicating if inflation is enabled or disabled,
        /// a label indicating inflation, and two buttons for adjusting the inflation level. Finally, there is an input
        /// field for manually setting the inflation level.
        /// </summary>
        private void DrawInflationOption()
        {
            // Begin horizontal layout for the inflation option
            GUILayout.BeginHorizontal();

            // Draw dot indicating if inflation is enabled or disabled
            DrawActivationDot(_inflationLevel != 0); // Use inflation level to set dot color

            // Label indicating inflation
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

            // End horizontal layout for the inflation option
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the max condoms option in the mod menu
        /// </summary>
        private void DrawBreedingStandUsesOption()
        {
            // Begin horizontal layout for the max condoms option
            GUILayout.BeginHorizontal();

            // Draw the activation dot and use the breeding stand uses value to set its color
            DrawActivationDot(_breedingStandUses != 3);

            // Add a label for the text field
            GUILayout.Label("Breeding Stand Uses:");

            // Draw the text field and capture user input
            string inputText = GUILayout.TextField(_breedingStandUses.ToString(), GUILayout.Width(40));

            // Try to parse the input text as an integer
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

            // End horizontal layout for the max condoms option
            GUILayout.EndHorizontal();
        }
    }
}