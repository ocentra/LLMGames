#if UNITY_EDITOR


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoslynTest : EditorWindow
{
    [MenuItem("Tools/Roslyn Test")]
    public static void ShowWindow()
    {
        GetWindow<RoslynTest>("Roslyn Test");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Test Roslyn"))
        {
            TestRoslynIntegration();
        }
    }

    private void TestRoslynIntegration()
    {
        string code = @"
            using System;
            class Test {
                void HelloWorld() {
                    Console.WriteLine(""Hello, World!"");
                }
            }";

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var methods = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .Select(m => m.Identifier.Text);

        foreach (var method in methods)
        {
            Debug.Log($"Method found: {method}");
        }
    }
}

#endif
