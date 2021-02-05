using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CommandDatabase", menuName = "DevConsole/CommandDatabase")]
public class CommandDatabase : ScriptableObject
{
    // TODO/Note: Add more command behaviours here!

    public void CommandTest()
    {
        Console.DeveloperConsole.AddStaticMessageToDevConsole("Abc123!");
    }

    public void ClearLog() { Console.DeveloperConsole.Instance.ConsoleText.text = null; }

    public void CmdList()
    {
        ClearLog(); // Fuck off

        // Write out all commands names and descriptions to the console...
        foreach(KeyValuePair<string, ConsoleCommand> keyValue in Console.DeveloperConsole.Commands)
        {
            string commandInfo = $"{ keyValue.Value.command }: { keyValue.Value.commandDescription } [{ keyValue.Value.argHelp }] \n";

            Console.DeveloperConsole.AddStaticMessageToDevConsole(commandInfo);
        }
    }

    public void DebugInfo()
    {
        string arg = Console.DeveloperConsole.Instance._input[1];

        if(arg == "1")
        {
            Console.DeveloperConsole.AddStaticMessageToDevConsole("Enabled!");
        }
        else if(arg == "0")
        {
            Console.DeveloperConsole.AddStaticMessageToDevConsole("Disabled!");
        }
    }
}
