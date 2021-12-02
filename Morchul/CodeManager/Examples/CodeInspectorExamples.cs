using Morchul.CodeManager;
using System.Collections.Generic;
using UnityEngine;

public class CodeInspectorExamples : MonoBehaviour
{
    private void Start()
    {
        //FirstExample();
        //ReplaceExample();
        //FileExample();
        SettingsExample();
    }

    private void SettingsExample()
    {
        //TODO
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
        string path = Application.dataPath + "/Plugins/Morchul/CodeManager/Examples/ExampleFile.txt";
        CodeInspection fileInspection = CodeInspector.InspectFile(path, InspectionMode.READ);

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

    private void FirstExample()
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
