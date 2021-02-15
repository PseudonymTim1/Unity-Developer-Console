using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEditor;

///////////////////////////////////
///     Unity Scriptable Object Dev Console ///
///   Written By Pseudonym_Tim  ///
///////////////////////////////////

// TODO: CLEANUP
// TODO: Command autofill source engine style
// TODO Allow resizing the panel

namespace Console
{
	public class DeveloperConsole : MonoBehaviour, IDragHandler
    {
		public static DeveloperConsole Instance { get; private set; }
		public static Dictionary<string, ConsoleCommand> Commands { get; private set; }
        public List<ConsoleCommand> consoleCommands;

        [Header("UI Stuffs")]
        public InputField inputField;
        public Canvas ConsoleCanvas;
		public ScrollRect RectScroll;
		public TextMeshProUGUI ConsoleText;
		public Text InputText;
        public GameObject ResultsField;
        public RectTransform dragRect;

        public string[] _input;

        // TODO/NOTE: Set this check to 0 when shipping!
        public int devCheck = 1; // Allow/dissallow dev only commands 
        private bool draggingWindow = false;

        // Cursor
        public Vector3 cursorHotspotOffset;
        public Image cursorImage;
        public Sprite cursorNormal;
        public Sprite cursorDrag;
        private bool isInDevConsole = false;

        private void Awake()
		{
			if(Instance != null) { return; }

			Instance = this;
			Commands = new Dictionary<string, ConsoleCommand>();
            Cursor.visible = false;
		}

        private void Start()
		{
            ConsoleCanvas.gameObject.SetActive(false);

            CreateConsoleCommands();
        }

        private void Update()
		{
            if(Input.GetKeyDown(KeyCode.BackQuote)) { ToggleConsole(); }

            // Submit command via return keypress...
			if(ConsoleCanvas.gameObject.activeInHierarchy)
			{
				if(Input.GetKeyDown(KeyCode.Return))
				{
                    SubmitCommand();
                }

                CursorUpdate();
            }
        }

        public void CreateConsoleCommands() { PopulateList(); Debug.Log("Created console commands successfully!"); }

        /// <summary>
        /// Populate the list with console commands
        /// </summary>
        private void PopulateList()
        {
            foreach(ConsoleCommand consolecommand in consoleCommands)
            {
                // If it's a dev only command and we don't have devcheck on then skip adding it
                if(consolecommand.cmdType == ConsoleCommand.CommandTypes.DEV && devCheck == 0)
                {
                    Debug.Log("Skipped adding the '" + consolecommand + "' command because it's a dev only command and devCheck is set to 0!");
                    continue;
                }

                AddCommandsToDevConsole(consolecommand.command, consolecommand);
            }
        }

        public static void AddCommandsToDevConsole(string _name, ConsoleCommand _command)
        {
            if(!Commands.ContainsKey(_name))
            {
                Commands.Add(_name, _command);
            }
        }

        /// <summary>
        /// Add selected objects command(s) to the console for use 
        /// </summary>
        public void AddObjectCommand(string _name, ConsoleCommand _command)
        {
            // (Don't add devcommands for objects if the devCheck is 0)
            if(_command.cmdType == ConsoleCommand.CommandTypes.DEV && devCheck == 0) { return; }

            // (If we don't already have this command)
            if(!Commands.ContainsKey(_name))
            {
                Commands.Add(_name, _command);
                consoleCommands.Add(_command);
                AddMessageToDevConsole("New command added: " + _command.command);
            }
        }

        /// <summary>
        /// Remove a selected objects command(s) from use
        /// </summary>
        public void RemoveObjectCommand(string _name, ConsoleCommand _command)
        {
            if(Commands.ContainsKey(_name))
            {
                Commands.Remove(_name);
                consoleCommands.Remove(_command);
                AddMessageToDevConsole("Removed command: " + _command.command);
            }
        }

        public void SubmitCommand()
        {
            if(!string.IsNullOrWhiteSpace(InputText.text))
            {
                AddMessageToDevConsole(InputText.text);
                ParseInput(InputText.text); // parse the input
                inputField.text = null; // Clear what we typed out
                StartCoroutine(WaitForInputActivation());
            }
        }

        // Stupid garbage hack to automatically select input field
        public IEnumerator WaitForInputActivation()
        {
            yield return 0;
            inputField.ActivateInputField();
        }

        private void ToggleConsole()
        {
            cursorImage.enabled = !!cursorImage.enabled; // Hide/cursor

            ConsoleCanvas.gameObject.SetActive(!ConsoleCanvas.gameObject.activeInHierarchy);

            isInDevConsole = !isInDevConsole;

            if(isInDevConsole) { UnlockCursor(); } else { LockCursor(); } // Lock/unlock toggle

            if(ConsoleCanvas.gameObject.activeInHierarchy)
            {
                StartCoroutine(WaitForInputActivation());
            }

            // Just clear what we inputted when we disable the console to solve an issue with inputting the key to access into the field
            if(!ConsoleCanvas.gameObject.activeInHierarchy) { inputField.text = null; }
        }

        private void CursorUpdate()
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ConsoleCanvas.transform as RectTransform, Input.mousePosition, ConsoleCanvas.worldCamera, out pos);
            cursorImage.transform.position = ConsoleCanvas.transform.TransformPoint(pos) + cursorHotspotOffset;

            CursorGFXUpdate();
        }

        public void AddMessageToDevConsole(string msg)
		{
			ConsoleText.text += msg + "\n"; // create new line for each msg
			RectScroll.verticalNormalizedPosition = 0f;
		}

		public static void AddStaticMessageToDevConsole(string msg)
		{
			Instance.ConsoleText.text += msg + "\n"; // create new line
			Instance.RectScroll.verticalNormalizedPosition  = 0f;
		}

        private void UnlockCursor() { Cursor.lockState = CursorLockMode.None; }
        private void LockCursor() { Cursor.lockState = CursorLockMode.Locked; }

        // TODO: Parsing could be improved...
        /// <summary>
        /// Parse the string we passed into the console for a command and possible arguments
        /// </summary>
        private void ParseInput(string input)
        {
            input = input.ToLower(); // Ignore any capitalization

            _input = input.Split(null); // Split up the input	

            if(_input.Length > 2) { AddStaticMessageToDevConsole("Only two arguments are allowed for any command!"); }

            // (Little hack to fix the player inputting whitespace just before a command name)
            if(string.IsNullOrWhiteSpace(_input[0]) && !string.IsNullOrWhiteSpace(_input[1])) { _input[0] = _input[1]; }

            // If we got an argument passed in, then use regex's replace function to replace whitespace 
            if(_input.Length >= 2) {  Regex.Replace(_input[1], @"\s", ""); Regex.Replace(_input[0], @"\s", ""); }

            // Detect if they put in any whitespace aka spaces
            if(!string.IsNullOrWhiteSpace(_input[0])) 
            {
                // If the input we put in wasn't in the list of command names
                if(!Commands.ContainsKey(_input[0]) && !string.IsNullOrWhiteSpace(_input[0])) 
                {
                    AddStaticMessageToDevConsole("Oops. Need a reminder of the commands? Type 'cmdlist' for a list of available commands and info about them!"); // Tell them they are an idiot
                }
                else
                {
                    // If the argcheck checks out
                    if(ArgCheck(_input[0], (int)Commands[_input[0]].argType))
                    {
                        Commands[_input[0]].RunCommand();
                    }
                }
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            draggingWindow = true;
            cursorImage.sprite = cursorDrag;

            dragRect.anchoredPosition += eventData.delta;
        }

        private void CursorGFXUpdate()
        {
            if(!draggingWindow)
            {
                // cursor is going to be set to the normal one
                cursorImage.sprite = cursorNormal;
            } 
            else if(draggingWindow && !Input.GetMouseButton(0))
            {
                draggingWindow = false;
            }
        }

        // Exit out of dev con via exit button
        public void DevConExit()
        {
            ConsoleCanvas.gameObject.SetActive(false);
            isInDevConsole = false;
            Cursor.visible = false; 
        }

        /// <summary>
        /// Parse a commands arguments depending on their type before we run the command
        /// </summary>
        public bool ArgCheck(string commandName, int ArgType)
        {
            switch(ArgType)
            {
                case 1: // Bool

                    // If we didn't get an argument, it's not a digit, or if the argument isn't specifically a 1 or a 0
                    if(_input.Length < 2 || !_input[1].Any(c => char.IsDigit(c)) || _input[1] != "0" && _input[1] != "1")
                    {
                        // Tell them they are an idiot and fuck off
                        AddStaticMessageToDevConsole(commandName + " requires either a 0 or 1 argument value!"); 
                        return false;
                    }

                    break;

                case 2: // Variable

                    // If we didn't get an argument, it's not a digit, if the argument is less than 0 or if it's greater than the max variable amount we have functionality for
                    if(_input.Length < 2 || !_input[1].Any(c => char.IsDigit(c)) || System.Convert.ToInt32(_input[1]) <= 0 || System.Convert.ToInt32(_input[1]) > Commands[commandName].variableAmount)
                    {
                        // Tell them they are an idiot and fuck off
                        AddStaticMessageToDevConsole(commandName + " requires a value within a given range! (0 through " + Commands[commandName].variableAmount + ")"); 
                        return false;
                    }

                    break;
            }

            return true;
        }
    }

    
}
