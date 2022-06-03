using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using UnityEngine;

public class ScriptLoader
{
	private readonly string folderPath = Application.streamingAssetsPath;



	public PathfindingAlgorithm[] LoadAlgorithms()
	{
		var reader = new FileReader();
		var files = reader.LoadCodeFilesFromFolder(folderPath);
		var dllFileNames = reader.GetFileNamesInFolder(folderPath, "*.dll");

		var algorithms = new List<PathfindingAlgorithm>();

		foreach (var file in files)
			algorithms.AddRange(GetAlgorithmsFromText(file));

		foreach (var dll in dllFileNames)
			algorithms.AddRange(GetAlgorithmsFromAssembly(Assembly.LoadFrom(dll)));

		return algorithms.ToArray();
	}



	/// <summary> Возвращает массив удовлетворяющих определенным условиям методов (делегатов) из текста (исходного кода). 
	/// В процессе исходный код компилируется, и в ОЗУ создается DLL. </summary>
	private PathfindingAlgorithm[] GetAlgorithmsFromText(string text)
	{
		Assembly assembly = GetAssemblyFromTextOrNull(text);
		if (assembly == null)
			return new PathfindingAlgorithm[0];

		return GetAlgorithmsFromAssembly(assembly);
	}

	private PathfindingAlgorithm[] GetAlgorithmsFromAssembly(Assembly assembly)
	{
		var algorithms = new List<PathfindingAlgorithm>();

		foreach (var t in assembly.DefinedTypes)
		{
			// Нам нужны только классы (не абстрактные) и структуры
			if ((!t.IsClass || t.IsAbstract) && !t.IsValueType)
				continue;

			string[] logs = new string[3];

			bool nameIsNull;

			var name = GetPropertyValueOrNull(assembly, t, "Name", out logs[0]);
			if (nameIsNull = string.IsNullOrWhiteSpace(name))
				name = t.Name;

			var findBestPathMethod = GetMethodOrNull<FindBestPathDelegate>(assembly, t, "FindBestPath", out logs[1]);
			var algorithmTeaching = GetMethodOrNull<AlgorithmTeaching>(assembly, t, "GetAlgorithmStep", out logs[2]);



			// Если в типе нет ни подходящих свойств, ни подходящих методов, ничего не выводить в логи
			if (nameIsNull && findBestPathMethod == null && algorithmTeaching == null)
			{
				foreach (var log in logs)
					if (!string.IsNullOrWhiteSpace(log))
						ScreenDebug.Log(log);
			}

			algorithms.Add(new PathfindingAlgorithm(name, findBestPathMethod, algorithmTeaching));
		}

		return algorithms.ToArray();
	}

	/// <summary> Ищет в сборке string свойство с данным именем. Если такого свойства нет, возвращает null. </summary>
	private string GetPropertyValueOrNull(Assembly assembly, System.Reflection.TypeInfo t, string propName, out string logmessage)
	{
		logmessage = null;

		if (!t.DeclaredProperties.Any(x => x.Name == propName
										 && x.CanRead
										 && x.PropertyType == typeof(string)))
		{
			logmessage = $"В классе \"{t.FullName}\" нет свойства get string {propName}";
			return null;
		}

		try
		{
			Type type = assembly.GetType(t.FullName);

			object obj = null;
			if (!type.IsAbstract) // Если класс не статический
				obj = Activator.CreateInstance(type);

			var prop = type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			string name = (string)prop.GetValue(obj);

			return name;
		}
		catch (Exception e)
		{
			ScreenDebug.LogWarning($"Неправильная загрузка свойства с именем | {t.FullName}: {e.Message}");
			return null;
		}
	}

	/// <summary> Ищет в сборке метод с данным именем, соответствующий данному делегату. Если такого метода нет, возвращает null. </summary>
	private T GetMethodOrNull<T>(Assembly assembly, System.Reflection.TypeInfo t, string methodName, out string logMessage) where T : Delegate
	{
		logMessage = null;

		if (!t.DeclaredMethods.Any(x => x.Name == methodName))
		{
			logMessage = $"В классе \"{t.FullName}\" нет метода с именем \"{methodName}\"";
			return null;
		}

		try
		{
			Type type = assembly.GetType(t.FullName);

			object obj = null;
			if (!type.IsAbstract) // Если класс не статический
				obj = Activator.CreateInstance(type);

			var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			var method = (T)methodInfo.CreateDelegate(typeof(T), obj);

			return method;
		}
		catch (Exception e)
		{
			ScreenDebug.LogWarning($"Неправильная загрузка метода | {t.FullName}: {e.Message}");
			return null;
		}
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
					ScreenDebug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
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