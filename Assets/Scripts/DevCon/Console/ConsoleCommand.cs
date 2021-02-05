using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewConCommand", menuName = "CustomTypes/ConsoleCommand")]
[ExecuteInEditMode]
public class ConsoleCommand : ScriptableObject
{
    public string command;
    public string commandDescription;
    public string argHelp = "No Arguments";

    [Tooltip("(This doesn't matter if the argtype isn't variable)")] 
    public int variableAmount = 0; // The amount of variables this command has

    // TODO: Automatically fill in the CommandDatabase scriptable object into the object type of the unityevent!
    public UnityEvent commandEvent;

    // What type of command is this? (Essentially an accessability level)
    public enum CommandTypes
    {
        PLAYER,
        DEV
    }

    public CommandTypes cmdType;

    // What arguments does this command take?
    public enum ArgumentTypes
    {
        NOARG, // (No argument)
        BOOL, // (0 = false, 1 = true)
        VARIABLE, // Can be set to multiple numbers
    }

    public ArgumentTypes argType;

    public void RunCommand() { commandEvent.Invoke(); }
}
