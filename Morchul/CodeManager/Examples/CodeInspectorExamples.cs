using Morchul.CodeManager;
using System.Collections.Generic;
using UnityEngine;

public class CodeInspectorExamples : MonoBehaviour
{

    private string filePath;
    private void Start()
    {
        filePath = Application.dataPath + "/Plugins/Morchul/CodeManager/Examples/ExampleFile.txt";
        //SimpleExample();
        //ReplaceExample();
        //FileExample();
        //SettingsExample();
        //FindNamesExample();
    }

    private void FindNamesExample()
    {
        string text = "Dear Steve, dear Elsa,\nI hope you are fine...";
        
        //Create an object to inspect the text
        CodeInspection textInspection = CodeInspector.InspectText(text);

        ///------first approach-----
        //All names are after the word dear so search for it
        if (textInspection.FindAll(@"[Dd]ear ", out LinkedListNode<CodePiece>[] codePieces1))
        {
            foreach (LinkedListNode<CodePiece> codePiece in codePieces1)
            {
                //output the name
                Debug.Log("Name: " + codePiece.Next.Value.Code.Substring(0, codePiece.Next.Value.Code.IndexOf(",")));
            }
        }
        else
        {
            Debug.Log("Nothing found.");
        }


        ///------second approach-----
        if (textInspection.FindAll(@"[Dd]ear (?<name>\b[A-Za-z]*)", out LinkedListNode<CodePiece>[] codePieces2))
        {
            foreach (LinkedListNode<CodePiece> codePiece in codePieces2)
            {
                //output the name found as group "name"
                Debug.Log("Name: " + codePiece.Next.Value.Match.Groups["name"].Value);
            }
        }
        else
        {
            Debug.Log("Nothing found.");
        }
    }

    private void SettingsExample()
    {
        CodeInspection fileInspection = CodeInspector.InspectFile(filePath, InspectionMode.READ);
        fileInspection.Settings = new CodeInspectionSettings()
        {
            AddLineIndex = true,
            RegexOptions = System.Text.RegularExpressions.RegexOptions.IgnoreCase,
            RegexTimeout = System.TimeSpan.Zero
        };

        //Search for the first c or C (RegexOptions.IgnoreCase)
        if(fileInspection.Find(@"C", out LinkedListNode<CodePiece> foundCodePiece))
        {
            int lineIndex = fileInspection.GetLineIndex(foundCodePiece); //possible because AddLineIndex is true
            Debug.Log("The first c occurs in line: " + lineIndex);
        }
        else
        {
            Debug.Log("No c found.");
        }
    }

    private void ReplaceExample()
    {
        string text = "Hey dear Neighbours.\nHow are you";

        //Create an object to inspect the text
        CodeInspection textInspection = CodeInspector.InspectText(text);

        //Search for all little e
        if (textInspection.FindAll(@"e", out LinkedListNode<CodePiece>[] codePieces))
        {
            foreach(LinkedListNode<CodePiece> codePiece in codePieces)
            {
                //Replace the small e through a big E
                codePiece.Value.Code = "E";
            }

            textInspection.Commit();

            text = textInspection.CompleteCode;

            Debug.Log("New text: " + text);
        }
        else
        {
            Debug.Log("Nothing found.");
        }
    }

    private void FileExample()
    {
        CodeInspection fileInspection = CodeInspector.InspectFile(filePath, InspectionMode.READ);

        Debug.Log("File name: " + fileInspection.FileName);
        //Search for words who start with a big letter and have at least 3 letters.
        if(fileInspection.FindAll(@"[A-Z][a-z]{2,}", out LinkedListNode<CodePiece>[] codePieces))
        {
            foreach(LinkedListNode<CodePiece> codePiece in codePieces)
            {
                Debug.Log("Found word: " + codePiece.Value.Code);
            }
        }
        else
        {
            Debug.Log("Nothing found.");

            LinkedListNode<CodePiece> fileText = fileInspection.First;
            if(fileText != null)
            {
                //Would throw an error because no write permission
                //fileInspection.AddAfter(fileText, "NewWord");

                CodeInspector.GrantWritePermission(fileInspection);

                fileInspection.GetEverything(out fileText);

                fileInspection.AddAfter(fileText, "NewWord");

                //Write new text to file
                fileInspection.Commit();
                Debug.Log("New file content written");
            }
        }

        //At the end stop file inspection or file will be blocked
        CodeInspector.StopFileInspection(fileInspection);

        //Would throw an error because fileInspection is inactive since StopFileInspection
        //fileInspection.Find(".", out LinkedListNode<CodePiece> codePiece);
    }

    private void SimpleExample()
    {
        string text = "Hey dear Neighbours.\nHow are you";

        //Create an object to inspect the text
        CodeInspection textInspection = CodeInspector.InspectText(text);

        //Search for a big N
        if (textInspection.Find(@"N", out LinkedListNode<CodePiece> codePiece))
        {
            Debug.Log("Text before match: " + codePiece.Previous.Value.Code);
            Debug.Log("Text: " + codePiece.Value.Code);
            Debug.Log("Text after match: " + codePiece.Next.Value.Code);

            textInspection.AddBefore(codePiece, "nice ");

            string currentCode = textInspection.CreateCurrentCode();
            Debug.Log("CurrentText: " + currentCode);

            textInspection.Commit();

            text = textInspection.CompleteCode;

            Debug.Log("New text: " + text);
        }
        else
        {
            Debug.Log("Nothing found.");
        }
    }
}
