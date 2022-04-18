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
		if (!Directory.Exists(folderPath))
			return new string[0];

		// Getting all code file names
		string[] fileNames = Directory.GetFiles(folderPath, searchPattern);
		List<string> files = new List<string>(fileNames.Length);

		// Loading all code files
		foreach (var fileName in fileNames)
		{
			if (File.Exists(fileName))
				files.Add(ReadFile(fileName));
		}

		return files.ToArray();
	}



	/// <summary> Читает конкретный файл и возвращает его содержимое (текст). </summary>
	public string ReadFile(string fileName)
	{
		return File.ReadAllText(fileName);

		// TODO: убрать заглушку из чтения файла

		/*

		return @"
                using System;

                class Writer
                {
                    public static string Main(string message)
                    {
                        return message + "" готовый!!11!!!!"";
                    }
                }

				class SuperWriter
                {
                    public static string Main(string message)
                    {
                        return message + message + message + "" готовый!!11!!!!"";
                    }
                }

				class Something
                {
                }

				class SomethingNew
                {
					public static string Main(int a)
                    {
                        return a + a + a + "" готовый!!11!!!!"";
                    }
                }

				class Amdhasd
                {
					public static int Main(string message)
                    {
                        return 5;
                    }
                }";

		*/
	}
}
