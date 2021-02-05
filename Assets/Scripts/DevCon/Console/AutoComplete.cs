using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Console;

// TODO: Make this NOT complete garbage, maybe then I'll add autocompleting commands to the dev console lol

[System.Obsolete("Complete garbo mc garbage trash, needs a major rewrite")]
public class AutoComplete : MonoBehaviour
{
    public InputField inputField;
    public RectTransform resultsParent;
    public RectTransform prefab;

    private void Awake()
    {
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string newText)
    {
        ClearResults();
        FillResults(GetResults(newText));
    }

    private void ClearResults()
    {
        // Reverse loop since destroying children
        for(int childIndex = resultsParent.childCount - 1; childIndex >= 0; --childIndex)
        {
            Transform child = resultsParent.GetChild(childIndex);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    private void FillResults(List<string> results)
    {
        for(int resultIndex = 0; resultIndex < results.Count; resultIndex++)
        {
            RectTransform child = Instantiate(prefab) as RectTransform;
            child.GetComponentInChildren<TextMeshProUGUI>().text = results[resultIndex];
            child.SetParent(resultsParent);
        }
    }

    private List<string> GetResults(string input)
    {
        List<string> mockData = new List<string>();

        foreach(KeyValuePair<string, ConsoleCommand> keyValue in DeveloperConsole.Commands)
        {
            mockData.Add(keyValue.Key);
        }

        return mockData.FindAll((str) => str.IndexOf(input) >= 0);
    }
}