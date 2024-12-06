using Cysharp.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Assets.Editor
{
    public class CombineCsFiles : EditorWindow
    {
        public class MethodUsageInfo
        {
            public string MethodName { get; set; }
            public bool IsOverride { get; set; }
            public bool IsGeneric { get; set; }
            public string BaseClassName { get; set; }
            public List<string> ImplementedInterfaces { get; set; } = new List<string>();
            public List<MethodReference> References { get; set; } = new List<MethodReference>();
            public List<string> DerivedClasses { get; set; } = new List<string>();
            public Dictionary<string, List<string>> GenericUsages { get; set; } = new Dictionary<string, List<string>>();
        }

        public class MethodReference
        {
            public string SourceFile { get; set; }
            public string ContainingMethod { get; set; }
            public int LineNumber { get; set; }
            public string CallType { get; set; } // "Direct", "Base", "Virtual", "Generic"
            public string GenericArguments { get; set; } // For generic method calls
        }

        private string sourceFolder = "Assets";
        private string outputPath = "Assets/CombinedFiles.txt";
        private Vector2 scrollPosition;
        private Dictionary<string, bool> fileSelections = new Dictionary<string, bool>();
        private bool selectAll = false;
        private Dictionary<string, MethodUsageInfo> methodUsages = new Dictionary<string, MethodUsageInfo>();

        [MenuItem("Tools/CS Files Combiner")]
        public static void ShowWindow()
        {
            GetWindow<CombineCsFiles>("CS Files Combiner");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source Folder:", GUILayout.Width(100));
            EditorGUILayout.LabelField(sourceFolder, EditorStyles.textField);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Select Source Folder", sourceFolder, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    sourceFolder = GetRelativePath(newPath);
                    RefreshFileList();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output File:", GUILayout.Width(100));
            EditorGUILayout.LabelField(outputPath, EditorStyles.textField);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.SaveFilePanel("Save Combined Files",
                    Path.GetDirectoryName(outputPath),
                    Path.GetFileName(outputPath),
                    "txt");
                if (!string.IsNullOrEmpty(newPath))
                {
                    outputPath = GetRelativePath(newPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Files to Combine:", EditorStyles.boldLabel);
            bool newSelectAll = EditorGUILayout.Toggle("Select All", selectAll);
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                foreach (string key in fileSelections.Keys.ToList())
                {
                    fileSelections[key] = selectAll;
                }
            }
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (string file in fileSelections.Keys.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                fileSelections[file] = EditorGUILayout.Toggle(fileSelections[file], GUILayout.Width(20));
                EditorGUILayout.LabelField(Path.GetFileName(file));
                EditorGUILayout.LabelField(GetRelativePath(file));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            GUI.enabled = fileSelections.Any(f => f.Value);
            if (GUILayout.Button("Combine Selected Files", GUILayout.Height(30)))
            {
                CombineFilesWithAnalysis();
            }
            GUI.enabled = true;
        }



        private async UniTask WriteFileWithUsageInfo(StreamWriter writer, string file)
        {
            await writer.WriteLineAsync($"// File: {Path.GetFileName(file)}").AsUniTask();
            string fileName = Path.GetFileNameWithoutExtension(file);

            string sourceText = await File.ReadAllTextAsync(file).AsUniTask();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
            SyntaxNode root = await tree.GetRootAsync().AsUniTask();

            // File level usage
            await writer.WriteLineAsync("// File Usage:").AsUniTask();
            List<MethodReference> fileReferences = methodUsages.Values
                .SelectMany(m => m.References)
                .Where(r => r.SourceFile.Contains(fileName))
                .Distinct()
                .ToList();

            if (fileReferences.Any())
            {
                foreach (MethodReference reference in fileReferences)
                {
                    await writer.WriteLineAsync($"//   Used in: {reference.SourceFile} -> {reference.ContainingMethod}").AsUniTask();
                }
            }
            else
            {
                await writer.WriteLineAsync("//   No direct file usage found").AsUniTask();
            }

            // Class Information
            ClassDeclarationSyntax classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration != null)
            {
                await writer.WriteLineAsync("\n// Class Information:").AsUniTask();
                await writer.WriteLineAsync($"//   Class: {classDeclaration.Identifier.Text}").AsUniTask();

                IEnumerable<string> baseList = classDeclaration.BaseList?.Types.Select(t => t.ToString());
                if (baseList != null)
                {
                    IEnumerable<string> baseTypes = baseList as string[] ?? baseList.ToArray();
                    if (baseTypes.Any())
                    {
                        await writer.WriteLineAsync("//   Inherits/Implements:").AsUniTask();
                        foreach (string baseType in baseTypes)
                        {
                            await writer.WriteLineAsync($"//     - {baseType}").AsUniTask();
                        }
                    }
                }
            }
            // Add this section in WriteFileWithUsageInfo after class information
            await writer.WriteLineAsync("\n// Event Bus Usage:").AsUniTask();
            List<MethodReference> eventBusRefs = methodUsages.Values
                .SelectMany(m => m.References)
                .Where(r => r.CallType == "Subscribe" || r.CallType == "SubscribeAsync" ||
                            r.CallType == "Publish" || r.CallType == "PublishAsync")
                .ToList();

            if (eventBusRefs.Any())
            {
                foreach (IGrouping<string, MethodReference> group in eventBusRefs.GroupBy(r => r.CallType))
                {
                    await writer.WriteLineAsync($"//   {group.Key}:").AsUniTask();
                    foreach (MethodReference reference in group)
                    {
                        string usage = $"//     - In {reference.SourceFile} -> {reference.ContainingMethod}";
                        if (!string.IsNullOrEmpty(reference.GenericArguments))
                            usage += $" (Handler: {reference.GenericArguments})";
                        usage += $" (Line {reference.LineNumber})";
                        await writer.WriteLineAsync(usage).AsUniTask();
                    }
                }
            }
            else
            {
                await writer.WriteLineAsync("//   No event bus usage found").AsUniTask();
            }
            // Method Analysis
            await writer.WriteLineAsync("\n// Methods:").AsUniTask();
            IEnumerable<MethodDeclarationSyntax> methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (MethodDeclarationSyntax method in methodDeclarations)
            {
                // Get full method signature
                string parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                string methodSignature = $"{string.Join(" ", method.Modifiers)} {method.ReturnType} {method.Identifier}({parameters})";

                await writer.WriteLineAsync($"// Method: {methodSignature}").AsUniTask();

                string methodName = $"{Path.GetFileNameWithoutExtension(file)}.{method.Identifier.Text}";
                if (methodUsages.TryGetValue(methodName, out MethodUsageInfo info))
                {
                    if (info.IsOverride)
                        await writer.WriteLineAsync($"//   Overrides: {info.BaseClassName}.{method.Identifier.Text}").AsUniTask();

                    if (info.IsGeneric)
                        if (method.TypeParameterList != null)
                        {
                            await writer
                                .WriteLineAsync(
                                    $"//   Generic Method with parameters: {string.Join(", ", method.TypeParameterList.Parameters.Select(p => p.Identifier.Text))}")
                                .AsUniTask();
                        }

                    if (info.References.Any())
                    {
                        await writer.WriteLineAsync("//   Usage:").AsUniTask();
                        foreach (MethodReference reference in info.References)
                        {
                            string usageInfo = $"//     - {reference.SourceFile} -> {reference.ContainingMethod}";
                            if (reference.CallType != "Direct")
                                usageInfo += $" ({reference.CallType} call)";
                            if (!string.IsNullOrEmpty(reference.GenericArguments))
                                usageInfo += $" with types: {reference.GenericArguments}";
                            usageInfo += $" (Line {reference.LineNumber})";
                            await writer.WriteLineAsync(usageInfo).AsUniTask();
                        }
                    }
                    else
                    {
                        await writer.WriteLineAsync("//   No usages found").AsUniTask();
                    }
                }
                await writer.WriteLineAsync("//").AsUniTask();
            }

            // Source Code
            await writer.WriteLineAsync("\n// Source Code:").AsUniTask();
            await writer.WriteLineAsync(sourceText).AsUniTask();
            await writer.WriteLineAsync("\n// --------------------------------\n").AsUniTask();
        }


        private void RefreshFileList()
        {
            fileSelections.Clear();
            if (Directory.Exists(sourceFolder))
            {
                string[] files = Directory.GetFiles(sourceFolder, "*.cs", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    fileSelections[file] = false;
                }
            }
        }

        private async UniTask AnalyzeClassDeclaration(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, string sourceFile, Dictionary<string, MethodUsageInfo> usages)
        {
            INamedTypeSymbol classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            if (classSymbol == null) return;

            // Check base types and interfaces
            foreach (INamedTypeSymbol baseType in classSymbol.AllInterfaces.Concat(new[] { classSymbol.BaseType }))
            {
                if (baseType != null)
                {
                    string key = baseType.Name;
                    if (usages.TryGetValue(key, out MethodUsageInfo usage))
                    {
                        MethodReference reference = new MethodReference
                        {
                            SourceFile = Path.GetFileName(sourceFile),
                            ContainingMethod = classDeclaration.Identifier.Text,
                            LineNumber = classDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            CallType = baseType.TypeKind == TypeKind.Interface ? "ImplementsInterface" : "InheritsFrom"
                        };

                        lock (usage.References)
                        {
                            usage.References.Add(reference);
                        }

                        if (baseType.TypeKind != TypeKind.Interface)
                        {
                            usage.BaseClassName = baseType.Name;
                        }
                    }
                }
            }

            // Check method parameters and return types
            foreach (MethodDeclarationSyntax method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(method);
                if (methodSymbol != null)
                {
                    // Check return type
                    string returnTypeName = methodSymbol.ReturnType.Name;
                    if (usages.TryGetValue(returnTypeName, out MethodUsageInfo returnTypeUsage))
                    {
                        MethodReference reference = new MethodReference
                        {
                            SourceFile = Path.GetFileName(sourceFile),
                            ContainingMethod = method.Identifier.Text,
                            LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            CallType = "ReturnType"
                        };

                        lock (returnTypeUsage.References)
                        {
                            returnTypeUsage.References.Add(reference);
                        }
                    }

                    // Check parameters
                    foreach (IParameterSymbol parameter in methodSymbol.Parameters)
                    {
                        string paramTypeName = parameter.Type.Name;
                        if (usages.TryGetValue(paramTypeName, out MethodUsageInfo paramTypeUsage))
                        {
                            MethodReference reference = new MethodReference
                            {
                                SourceFile = Path.GetFileName(sourceFile),
                                ContainingMethod = method.Identifier.Text,
                                LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                                CallType = "ParameterType"
                            };

                            lock (paramTypeUsage.References)
                            {
                                paramTypeUsage.References.Add(reference);
                            }
                        }
                    }
                }
            }
        }

        private async UniTask AnalyzeMethodInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, string sourceFile, Dictionary<string, MethodUsageInfo> usages)
        {
            IMethodSymbol symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null) return;

            if (IsLowLevelReference(symbol)) return;

            MethodDeclarationSyntax containingMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            FileLinePositionSpan lineSpan = invocation.GetLocation().GetLineSpan();

            // Check for Event Bus specific calls (Subscribe, Publish, etc.)
            if (symbol.ContainingType?.Name == "EventBus")
            {
                string methodName = symbol.Name;

                // Handle generic methods like Subscribe<T>() or Publish<T>()
                if (symbol.IsGenericMethod)
                {
                    foreach (ITypeSymbol typeArg in symbol.TypeArguments)
                    {
                        string key = typeArg.Name;
                        if (usages.TryGetValue(key, out MethodUsageInfo usage))
                        {
                            MethodReference reference = new MethodReference
                            {
                                SourceFile = Path.GetFileName(sourceFile),
                                ContainingMethod = containingMethod?.Identifier.Text ?? "Global Scope",
                                LineNumber = lineSpan.StartLinePosition.Line + 1,
                                CallType = methodName, // "Subscribe", "Publish", etc.
                                GenericArguments = string.Join(", ", symbol.TypeArguments.Select(t => t.Name))
                            };

                            lock (usage.References)
                            {
                                usage.References.Add(reference);
                            }
                        }
                    }
                }

                // Handle non-generic Publish and Subscribe with arguments
                foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
                {
                    if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                    {
                        TypeInfo typeInfo = semanticModel.GetTypeInfo(objectCreation.Type);
                        if (typeInfo.Type != null && usages.TryGetValue(typeInfo.Type.Name, out MethodUsageInfo usage))
                        {
                            MethodReference reference = new MethodReference
                            {
                                SourceFile = Path.GetFileName(sourceFile),
                                ContainingMethod = containingMethod?.Identifier.Text ?? "Global Scope",
                                LineNumber = lineSpan.StartLinePosition.Line + 1,
                                CallType = methodName
                            };

                            lock (usage.References)
                            {
                                usage.References.Add(reference);
                            }
                        }
                    }
                    else if (argument.Expression is IdentifierNameSyntax identifierName)
                    {
                        // Attempt to resolve the type of the identifier used as an argument
                        ILocalSymbol variableSymbol = semanticModel.GetSymbolInfo(identifierName).Symbol as ILocalSymbol;
                        ITypeSymbol type = variableSymbol?.Type;
                        if (type != null && usages.TryGetValue(type.Name, out MethodUsageInfo usage))
                        {
                            MethodReference reference = new MethodReference
                            {
                                SourceFile = Path.GetFileName(sourceFile),
                                ContainingMethod = containingMethod?.Identifier.Text ?? "Global Scope",
                                LineNumber = lineSpan.StartLinePosition.Line + 1,
                                CallType = methodName
                            };

                            lock (usage.References)
                            {
                                usage.References.Add(reference);
                            }
                        }
                    }
                }
            }

            // Check for other types of invocations related to generics
            if (symbol.IsGenericMethod)
            {
                foreach (ITypeSymbol typeArg in symbol.TypeArguments)
                {
                    string key = typeArg.Name;
                    if (usages.TryGetValue(key, out MethodUsageInfo usage))
                    {
                        MethodReference reference = new MethodReference
                        {
                            SourceFile = Path.GetFileName(sourceFile),
                            ContainingMethod = containingMethod?.Identifier.Text ?? "Global Scope",
                            LineNumber = lineSpan.StartLinePosition.Line + 1,
                            CallType = "GenericMethodUsage",
                            GenericArguments = string.Join(", ", symbol.TypeArguments.Select(t => t.Name))
                        };

                        lock (usage.References)
                        {
                            usage.References.Add(reference);
                        }
                    }
                }
            }

            // Handle constructor calls or object creation
            foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
            {
                if (argument.Expression is ObjectCreationExpressionSyntax objectCreation)
                {
                    TypeInfo typeInfo = semanticModel.GetTypeInfo(objectCreation.Type);
                    if (typeInfo.Type != null)
                    {
                        string key = typeInfo.Type.Name;
                        if (usages.TryGetValue(key, out MethodUsageInfo usage))
                        {
                            MethodReference reference = new MethodReference
                            {
                                SourceFile = Path.GetFileName(sourceFile),
                                ContainingMethod = containingMethod?.Identifier.Text ?? "Global Scope",
                                LineNumber = lineSpan.StartLinePosition.Line + 1,
                                CallType = "Creation"
                            };

                            lock (usage.References)
                            {
                                usage.References.Add(reference);
                            }
                        }
                    }
                }
            }
        }

        private bool IsLowLevelReference(IMethodSymbol symbol)
        {
            // Exclude namespaces like Cysharp.Threading.Tasks
            if (symbol.ContainingNamespace.ToString().StartsWith("Cysharp.Threading.Tasks")) return true;

            // Exclude specific types
            string[] excludedTypes = { "PlayerLoopHelper", "UniTaskSynchronizationContext" };
            if (excludedTypes.Contains(symbol.ContainingType?.Name)) return true;

            // Exclude specific methods
            string[] excludedMethods = { "MoveNext", "Post", "Run", "Add", "Remove" };
            if (excludedMethods.Contains(symbol.Name)) return true;

            return false;
        }

        private async UniTask AnalyzeFilesUsage(List<string> filesToAnalyze)
        {
            methodUsages.Clear();
            float progress = 0f;
            object progressLock = new object();

            try
            {
                // First pass: Collect ALL declarations (including constructors)
                List<UniTask<List<(string, MethodUsageInfo)>>> methodCollectionTasks = filesToAnalyze.Select(async file =>
                {
                    try
                    {
                        string sourceText = await File.ReadAllTextAsync(file).AsUniTask();
                        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
                        SyntaxNode root = await tree.GetRootAsync().AsUniTask();

                        List<(string Key, MethodUsageInfo Info)> methods = new List<(string Key, MethodUsageInfo Info)>();

                        // Analyze class declarations
                        IEnumerable<ClassDeclarationSyntax> classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                        foreach (ClassDeclarationSyntax classDeclaration in classDeclarations)
                        {
                            CSharpCompilation compilation = CSharpCompilation.Create("Analysis")
                                .AddReferences(
                                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                    MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location)
                                )
                                .AddSyntaxTrees(tree);

                            SemanticModel semanticModel = compilation.GetSemanticModel(tree);

                            // Skip low-level or system classes
                            if (IsLowLevelType(classDeclaration, semanticModel)) continue;

                            // Call AnalyzeClassDeclaration for each class
                            await AnalyzeClassDeclaration(classDeclaration, semanticModel, file, methodUsages);

                            // Collect constructors and methods
                            methods.AddRange(CollectConstructors(classDeclaration, file));
                            methods.AddRange(CollectRegularMethods(classDeclaration, file));
                        }

                        lock (progressLock)
                        {
                            progress++;
                            EditorUtility.DisplayProgressBar("Analyzing Methods",
                                $"Collecting declarations ({progress}/{filesToAnalyze.Count})",
                                progress / filesToAnalyze.Count);
                        }

                        return methods;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error analyzing file {file}: {ex.Message}");
                        return new List<(string, MethodUsageInfo)>();
                    }
                }).ToList();

                List<(string, MethodUsageInfo)>[] methodCollectionResults = await UniTask.WhenAll(methodCollectionTasks);

                foreach (List<(string, MethodUsageInfo)> collection in methodCollectionResults)
                {
                    foreach ((string key, MethodUsageInfo info) in collection)
                    {
                        methodUsages[key] = info;
                    }
                }

                // Second pass: Find ALL references
                string[] allCsFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
                const int batchSize = 20;
                progress = 0;

                CSharpCompilation compilation = CSharpCompilation.Create("Analysis")
                    .AddReferences(
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(UnityEngine.Object).Assembly.Location)
                    );

                for (int i = 0; i < allCsFiles.Length; i += batchSize)
                {
                    IEnumerable<string> currentBatch = allCsFiles.Skip(i).Take(batchSize);
                    IEnumerable<UniTask> batchTasks = currentBatch.Select(async file =>
                    {
                        try
                        {
                            string sourceText = await File.ReadAllTextAsync(file).AsUniTask();
                            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
                            SyntaxNode root = await tree.GetRootAsync().AsUniTask();

                            CSharpCompilation currentCompilation = compilation.AddSyntaxTrees(tree);
                            SemanticModel semanticModel = currentCompilation.GetSemanticModel(tree);

                            // Method invocations
                            IEnumerable<InvocationExpressionSyntax> invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();
                            foreach (InvocationExpressionSyntax invocation in invocations)
                            {
                                if (!IsLowLevelInvocation(invocation, semanticModel)) // Skip low-level calls
                                {
                                    await AnalyzeMethodInvocation(invocation, semanticModel, file, methodUsages);
                                }
                            }

                            // Constructor invocations
                            IEnumerable<ObjectCreationExpressionSyntax> objectCreations = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>();
                            foreach (ObjectCreationExpressionSyntax creation in objectCreations)
                            {
                                ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(creation.Type).Type;
                                if (typeSymbol != null && !IsLowLevelType(typeSymbol)) // Skip low-level constructors
                                {
                                    MethodDeclarationSyntax containingMethod = creation.Ancestors()
                                        .OfType<MethodDeclarationSyntax>()
                                        .FirstOrDefault();

                                    string key = $"{typeSymbol.Name}..ctor";
                                    if (methodUsages.TryGetValue(key, out MethodUsageInfo usageInfo))
                                    {
                                        MethodReference reference = new MethodReference
                                        {
                                            SourceFile = Path.GetFileName(file),
                                            ContainingMethod = containingMethod?.Identifier.Text ?? "Global Scope",
                                            LineNumber = creation.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                                            CallType = "Constructor"
                                        };
                                        lock (usageInfo.References)
                                        {
                                            usageInfo.References.Add(reference);
                                        }
                                    }
                                }
                            }

                            lock (progressLock)
                            {
                                progress++;
                                EditorUtility.DisplayProgressBar("Analyzing Usage",
                                    $"Processing files ({progress}/{allCsFiles.Length})",
                                    progress / allCsFiles.Length);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"Error analyzing file {file}: {ex.Message}");
                        }
                    });

                    await UniTask.WhenAll(batchTasks);
                    await UniTask.Yield();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private bool IsLowLevelType(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            // Retrieve the symbol for the class declaration
            var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            return IsLowLevelType(symbol); // Delegate to the existing ITypeSymbol version
        }
        private bool IsLowLevelType(ITypeSymbol symbol)
        {
            if (symbol == null) return false;

            // Exclude namespaces
            string[] excludedNamespaces =
            {
                "System",                    // Core .NET namespace
                "Cysharp.Threading.Tasks",   // UniTask low-level namespace
                "UnityEngine",               // Unity's core namespace
                "UnityEditor"                // Unity's editor namespace
            };
            if (excludedNamespaces.Any(ns => symbol.ContainingNamespace?.ToString().StartsWith(ns) == true))
                return true;

            // Exclude specific low-level types
            string[] excludedTypes = { "UniTask", "Task", "PlayerLoopHelper", "MonoBehaviour", "ScriptableObject" };
            if (excludedTypes.Contains(symbol.Name)) return true;

            return false;
        }


        // Utility to identify low-level invocations
        private bool IsLowLevelInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            IMethodSymbol symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            return symbol != null && IsLowLevelType(symbol.ContainingType);
        }

        private List<(string Key, MethodUsageInfo Info)> CollectConstructors(ClassDeclarationSyntax classDeclaration, string file)
        {
            List<(string Key, MethodUsageInfo Info)> constructors = new List<(string Key, MethodUsageInfo Info)>();

            IEnumerable<ConstructorDeclarationSyntax> constructorDeclarations = classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
            foreach (ConstructorDeclarationSyntax ctor in constructorDeclarations)
            {
                string parameters = string.Join(", ", ctor.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                string constructorName = $"{classDeclaration.Identifier.Text}({parameters})";

                MethodUsageInfo info = new MethodUsageInfo
                {
                    MethodName = constructorName,
                    IsOverride = false,
                    References = new List<MethodReference>()
                };

                string key = $"{Path.GetFileNameWithoutExtension(file)}.{classDeclaration.Identifier.Text}..ctor";
                constructors.Add((key, info));
            }

            return constructors;
        }

        private List<(string Key, MethodUsageInfo Info)> CollectRegularMethods(ClassDeclarationSyntax classDeclaration, string file)
        {
            List<(string Key, MethodUsageInfo Info)> methods = new List<(string Key, MethodUsageInfo Info)>();

            IEnumerable<MethodDeclarationSyntax> methodDeclarations = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax method in methodDeclarations)
            {
                string parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"));
                string methodName = $"{method.ReturnType} {method.Identifier}({parameters})";

                MethodUsageInfo info = new MethodUsageInfo
                {
                    MethodName = methodName,
                    IsOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
                    IsGeneric = method.TypeParameterList?.Parameters.Count > 0,
                    BaseClassName = classDeclaration.BaseList?.Types.FirstOrDefault()?.ToString(),
                    References = new List<MethodReference>()
                };

                string key = $"{Path.GetFileNameWithoutExtension(file)}.{method.Identifier.Text}";
                methods.Add((key, info));
            }

            return methods;
        }

        private async void CombineFilesWithAnalysis()
        {
            try
            {
                List<string> selectedFiles = fileSelections.Where(f => f.Value).Select(f => f.Key).ToList();
                if (!selectedFiles.Any())
                {
                    EditorUtility.DisplayDialog("Error", "No files selected!", "OK");
                    return;
                }

                EditorUtility.DisplayProgressBar("Processing Files", "Analyzing usage...", 0f);
                await AnalyzeFilesUsage(selectedFiles);

                string fullOutputPath = Path.Combine(Application.dataPath, "..", outputPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));

                using (StreamWriter writer = new StreamWriter(fullOutputPath))
                {
                    float progress = 0;
                    foreach (string file in selectedFiles)
                    {
                        EditorUtility.DisplayProgressBar("Combining Files",
                            $"Processing {Path.GetFileName(file)}",
                            progress / selectedFiles.Count);

                        await WriteFileWithUsageInfo(writer, file);
                        progress++;
                    }
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Files combined with usage analysis into {outputPath}", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", $"Error combining files: {ex.Message}", "OK");
                Debug.LogException(ex);
            }
        }

        private string GetRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "";

            fullPath = fullPath.Replace("\\", "/");
            string projectPath = Application.dataPath.Replace("Assets", "").Replace("\\", "/");

            if (fullPath.StartsWith(projectPath))
            {
                return fullPath.Substring(projectPath.Length);
            }

            return fullPath;
        }
    }
}