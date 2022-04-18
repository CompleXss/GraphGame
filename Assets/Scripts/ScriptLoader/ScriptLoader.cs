using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using UnityEngine;



public delegate string PathfindingAlgorithm(string message);



public class ScriptLoader
{
	private readonly string folderPath = Application.dataPath + "/PathFinding";



	public PathfindingAlgorithm[] LoadAlgorithms()
	{
		var reader = new FileReader();
		var files = reader.LoadCodeFilesFromFolder(folderPath);

		var algorithms = new List<PathfindingAlgorithm>();

		foreach (var file in files)
			algorithms.AddRange(GetAlgorithmsFromText(file));

		return algorithms.ToArray();
	}



	/// <summary> Возвращает массив удовлетворяющих определенным условиям методов (делегатов) из текста (исходного кода). 
	/// В процессе исходный код компилируется, и в ОЗУ создается DLL. </summary>
	private PathfindingAlgorithm[] GetAlgorithmsFromText(string text)
	{
		Assembly assembly = GetAssemblyFromTextOrNull(text);
		if (assembly == null)
			return new PathfindingAlgorithm[0];



		var algorithms = new List<PathfindingAlgorithm>();

		foreach (var t in assembly.DefinedTypes)
		{
			if (!t.DeclaredMethods.Any(x => x.Name == "Main"
										 && x.IsPublic
										 && x.IsStatic))
			{
				Debug.LogWarning($"There is no public static method named \"Main\" in class \"{t.FullName}\"");
				continue;
			}

			try
			{
				// create instance of the desired class and call the desired function
				Type type = assembly.GetType(t.FullName);
				object obj = Activator.CreateInstance(type);

				var methodInfo = type.GetMethod("Main");
				var func = (PathfindingAlgorithm)methodInfo.CreateDelegate(typeof(PathfindingAlgorithm));

				algorithms.Add(func);
			}
			catch (Exception e)
			{
				// TODO: логи при неправильной загрузке
				Debug.LogWarning($"{t.FullName}: {e.Message}");
			}
		}

		return algorithms.ToArray();
	}



	/// <summary> Создает DLL из текста (исходного кода) и загружает его в ОЗУ. Если произошла ошибка, возвращает null. </summary>
	private Assembly GetAssemblyFromTextOrNull(string text)
	{
		// define source code, then parse it (to the type used for compilation)
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text);

		// define other necessary objects for compilation
		string assemblyName = Path.GetRandomFileName();
		MetadataReference[] references = new MetadataReference[]
		{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
		};

		// analyse and generate IL code from syntax tree
		CSharpCompilation compilation = CSharpCompilation.Create(
			assemblyName,
			syntaxTrees: new[] { syntaxTree },
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using (var ms = new MemoryStream())
		{
			// write IL code into memory
			EmitResult result = compilation.Emit(ms);

			if (!result.Success)
			{
				// handle exceptions
				IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
					diagnostic.IsWarningAsError ||
					diagnostic.Severity == DiagnosticSeverity.Error);

				foreach (Diagnostic diagnostic in failures)
				{
					// TODO: добавить логи при ошибке
					Debug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
				}
			}
			else
			{
				// load this 'virtual' DLL so that we can use
				ms.Seek(0, SeekOrigin.Begin);
				Assembly assembly = Assembly.Load(ms.ToArray());

				return assembly;
			}
		}

		// Если загрузка не удалась
		return null;
	}
}



// Примеры кода, мб пригодятся, хз

//// create instance of the desired class and call the desired function
//Type type = assembly.GetType("Writer");
//object obj = Activator.CreateInstance(type);

//var methodInfo = type.GetMethod("Write");
//var del = (Writer)methodInfo.CreateDelegate(typeof(Writer));

//Debug.Log(del("Text"));

// ===============================================================================

//var output = type.InvokeMember("Write",
//	BindingFlags.Default | BindingFlags.InvokeMethod,
//	null,
//	obj,
//	new object[] { "Hello World" });

//Debug.Log(output);