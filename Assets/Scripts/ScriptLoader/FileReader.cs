﻿using System.IO;
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

	public string[] GetFileNamesInFolder(string folderPath, string searchPattern)
	{
		if (!Directory.Exists(folderPath))
			return new string[0];

		return Directory.GetFiles(folderPath, searchPattern);
	}



	/// <summary> Читает конкретный файл и возвращает его содержимое (текст). </summary>
	public string ReadFile(string fileName)
	{
		using (var reader = new StreamReader(fileName, detectEncodingFromByteOrderMarks: true))
			return File.ReadAllText(fileName, reader.CurrentEncoding);
	}
}