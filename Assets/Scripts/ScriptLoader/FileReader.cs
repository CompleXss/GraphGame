using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class FileReader
{
	/// <summary> Читает все файлы с кодом в папке и вовзращает массив их содержимого ( string[] ). </summary>
	public string[] LoadCodeFilesFromFolder(string folderPath, string searchPattern = "*.cs")
	{
		//if (!Directory.Exists(folderPath))
		//	return new string[0];

		//// Getting all code file names
		//string[] fileNames = Directory.GetFiles(folderPath, searchPattern);
		//List<string> files = new List<string>(fileNames.Length);

		//// Loading all code files
		//foreach (var fileName in fileNames)
		//{
		//	if (File.Exists(fileName))
		//		files.Add(ReadFile(fileName));
		//}

		//return files.ToArray();

		return new string[1] { ReadFile("") };
	}



	/// <summary> Читает конкретный файл и возвращает его содержимое (текст). </summary>
	public string ReadFile(string fileName)
	{
		//return File.ReadAllText(fileName);

		// TODO: убрать заглушку из чтения файла

		///*

		return @"
                using System;

                class FirstClass
                {
					public static string Name => ""Имя первого класса"";

					static int a = 5;

                    public static int[] FindBestPath(int[,] graph, int s, int h)
                    {
                        return new int[1] { a };
                    }
                }

				class SecondClass
                {
					public static string Name { get; } = ""Имя второго класса"";

					public static int[] FindBestPath(int[,] graph, int a, int b)
                    {
                        return new int[1] { 10 };
                    }
                }";

		//*/
	}
}
