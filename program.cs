using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime;
using System.Text;

namespace B4XCustomActions
{
	class Program
	{
		/// <summary>
		/// Main entry point for the B4XCustomActions library.  This library is intended to be called from the command line
		/// as a custom step in a B4X project.
		/// </summary>
		static void Main(string[] args)
		{
			try
			{
				// If the argument is a string, split it into an array of strings
				if (args.Length == 1 && args[0].Contains(" ")) args = args[0].Split(' ');

				int c = args.GetUpperBound(0);

				string TAction = "";
				string TDateFormat = "yyyy-MM-dd";
				string TTimeFormat = "HH:mm:ss";
				string TSource = "";
				string TDestination = "";

				// Loop through arguments
				for (int n = 0; n < c; n++)
				{
					string thisKey = args[n].ToLower().TrimEnd().TrimStart();
					string thisVal = args[n + 1].TrimEnd().TrimStart();

					//Console.WriteLine(thisKey);
					//Console.WriteLine(thisVal);

					// eval the key or slash-switch option ("/key")
					switch (thisKey)
					{
						case "-action":
							TAction = thisVal;
							break;
						case "-source":
							TSource = thisVal;
							break;
						case "-destination":
							TDestination = thisVal;
							break;
						case "-dateformat":
							TDateFormat = thisVal;
							break;
						case "-timeformat":
							TTimeFormat = thisVal;
							break;
					}
				}

				Console.WriteLine("###############################################################");
				Console.WriteLine("B4X Custom Action - " + GetProjectName());
				Console.WriteLine("###############################################################");

				switch (TAction)
				{
					case "compileonly":
						Console.WriteLine("Compile Only");
						Environment.Exit(1);
						break;
					case "copyjar":
						Console.WriteLine("Copy Jar");
						CopyJar(TDestination);
						break;
					case "copy":
						Console.WriteLine("Copy");
						CopyPath(TSource, TDestination);
						break;
					case "buildtime":
						Console.WriteLine("Build Time");
						BuildTime(TDateFormat, TTimeFormat);
						break;
					case "updateversion":
						Console.WriteLine("Update Version");
						UpdateVersion();
						break;
					case "zip":
						Console.WriteLine("Zip");
						Zip(TSource, TDestination);
						break;
					case "moveautobackups":
						Console.WriteLine("Move AutoBackups");
						MoveAutoBackups(TDestination);
						break;
					default:
						Console.WriteLine("No action supplied");
						break;
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
			}
		}

		/// <summary>
		/// Copies a JAR file from the objects folder to the specified destination path.
		/// </summary>
		/// <param name="destinationPath">The target directory path where the JAR file will be copied.</param>
		/// <remarks>If a JAR file is found, it is copied to the destination with overwriting enabled.
		/// In case of an error during the process, an error message is printed, and the application exits with a status code of 1.</remarks>
		private static void CopyJar(string destinationPath)
		{
			try
			{
				String jar = FindJar();

				Console.WriteLine("Copying  " + jar + " to " + destinationPath);

				if (jar != null)
				{
					File.Copy(Path.Combine(GetObjectsFolder(), jar), Path.Combine(destinationPath, jar), overwrite: true);
				}
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Console.WriteLine("destinationPath: " + destinationPath);
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Writes the current date and time in the specified format to a file named "build.txt" in the Files folder.
		/// </summary>
		/// <param name="DateFormat">The format string for the date part of the DateTime object.</param>
		/// <param name="TimeFormat">The format string for the time part of the DateTime object.</param>
		/// <remarks>If a file with the same name already exists, it will be overwritten.</remarks>
		private static void BuildTime(string DateFormat, string TimeFormat)
		{
			// Define the desired format for the DateTime string
			string dateFormat = DateFormat + " " + TimeFormat;

			// Get the current DateTime and format it
			string dateTimeString = DateTime.Now.ToString(dateFormat);

			try
			{
				string TargetFile = Path.Combine(GetFilesFolder(), "build.txt");
				File.WriteAllText(TargetFile, dateTimeString);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Console.WriteLine("DateFormat: " + DateFormat);
				Console.WriteLine("TimeFormat: " + TimeFormat);
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Updates the version number in a file named "version.txt" in the Files folder.
		/// </summary>
		/// <remarks>
		/// The version number is incremented by one. If the build number exceeds 9, the minor version is incremented, and if the minor version exceeds 9, the major version is incremented.
		/// If a file with the same name already exists, it will be overwritten.
		/// </remarks>
		private static void UpdateVersion()
		{
			// Default version
			string version = "0.0.0";
			string TargetFile = Path.Combine(GetFilesFolder(), "version.txt");

			int major = 0;
			int minor = 0;
			int build = 0;

			try
			{
				if (File.Exists(TargetFile))
				{
					// Read the current version from the file
					version = File.ReadAllText(TargetFile).Trim();

					int dotCount = 0;
					foreach (char c in version){if (c == '.') dotCount++;}

					if (version.Length == 0 || dotCount != 2) version = "0.0.0";

					// Split the version into major, minor, and build
					var versionParts = version.Split('.');

					major = int.Parse(versionParts[0]);
					minor = int.Parse(versionParts[1]);
					build = int.Parse(versionParts[2]);
				}

				// Increment the build, and check if it exceeds 9
				build++;
				if (build > 9)
				{
					build = 0;
					minor++;
					if (minor > 9)
					{
						minor = 0;
						major++;
					}
				}

				// Construct the new version
				version = major + "." + minor + "." + build;

				Console.WriteLine("Incrementing version to " + version);

				// Write the updated version back to the file
				File.WriteAllText(TargetFile, version);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Console.WriteLine("version: " + version);

				if (File.Exists(TargetFile))
				{
					// Read the current version from the file
					Console.WriteLine("version.txt: " + File.ReadAllText(TargetFile).Trim());
				}

				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Copies the specified source path to the specified destination path.
		/// </summary>
		/// <param name="sourcePath">The source path to copy. Can be a file or a directory. If it is a relative path, it will be resolved relative to the objects folder.</param>
		/// <param name="destinationPath">The destination path to copy to. If it is a relative path, it will be resolved relative to the files folder.</param>
		/// <remarks>If a file with the same name already exists in the destination path, it will be overwritten.</remarks>
		public static void CopyPath(string sourcePath, string destinationPath)
		{
			try
			{
				if (sourcePath.StartsWith("Files")) sourcePath = Path.Combine(GetFilesFolder(), sourcePath.Substring(5));
				if (destinationPath.StartsWith("Files")) destinationPath = Path.Combine(GetFilesFolder(), destinationPath.Substring(5));

				if (!sourcePath.Contains(":"))
				{
					sourcePath = Path.Combine(GetObjectsFolder(), sourcePath);
				}

				bool isDirectory = Directory.Exists(sourcePath);
				bool isFile = File.Exists(sourcePath);

				if (isFile)
				{
					string FileName = Path.GetFileName(sourcePath);
					if (!destinationPath.Contains(FileName)) destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
				}

				if (isDirectory)
				{
					if (!destinationPath.Contains(Path.GetFileName(sourcePath))) destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
				}

				if (File.Exists(sourcePath))
				{
					Console.WriteLine("Copy File " + sourcePath + " to " + destinationPath);
					CopyFile(sourcePath, destinationPath);
					Environment.Exit(0);
				}

				if (Directory.Exists(sourcePath))
				{
					Console.WriteLine("Copy Directory " + sourcePath + " to " + destinationPath);
					CopyDirectory(sourcePath, destinationPath);
					Environment.Exit(0);
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Console.WriteLine("sourcePath: " + sourcePath);
				Console.WriteLine("destinationPath: " + destinationPath);
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Zips the specified source path to the specified destination path.
		/// </summary>
		/// <param name="sourcePath">The source path to zip. Can be a file or a directory. If it is a relative path, it will be resolved relative to the objects folder.</param>
		/// <param name="destinationPath">The destination path to zip to. If it is a relative path, it will be resolved relative to the files folder.</param>
		/// <remarks>If a file with the same name already exists in the destination path, it will be overwritten. If destinationPath does not end with .zip, the zip file name will be derived from the source name.</remarks>
		private static void Zip(string sourcePath, string destinationPath)
		{
			try
			{
				if (sourcePath.StartsWith("Files")) sourcePath = Path.Combine(GetFilesFolder(), sourcePath.Substring(5));
				if (destinationPath.StartsWith("Files")) destinationPath = Path.Combine(GetFilesFolder(), destinationPath.Substring(5));

				// If no drive then assume its a relative path, append base directory
				if (!sourcePath.Contains(":")) sourcePath = Path.Combine(GetObjectsFolder(), sourcePath);

				string zipFilePath = "";

				if (destinationPath.EndsWith(".zip"))
				{
					Console.WriteLine("Using specified zip file: " + destinationPath);
					zipFilePath = destinationPath;
				}
				else
				{
					// Extract the source name (could be filename or folder name)
					string sourceName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

					// If it contains a . then assume its a file and remove extension
					if (sourceName.Contains(".")) sourceName = sourceName.Substring(0, sourceName.LastIndexOf("."));

					zipFilePath = Path.Combine(destinationPath, sourceName + ".zip");
					Console.WriteLine("Using derived zip file: " + zipFilePath);
				}

				if (File.Exists(zipFilePath)) File.Delete(zipFilePath);

				Console.WriteLine("Zipping " + sourcePath + " to " + zipFilePath);

				if (File.Exists(sourcePath))
				{
					// Source is a file
					using (ZipStorer zip = ZipStorer.Create(zipFilePath))
					{
						zip.AddFile(ZipStorer.Compression.Deflate, sourcePath, Path.GetFileName(sourcePath));
					}

					Environment.Exit(0);
				}

				if (Directory.Exists(sourcePath))
				{
					// Source is a directory
					using (ZipStorer zip = ZipStorer.Create(zipFilePath))
					{
						zip.AddDirectory(ZipStorer.Compression.Deflate, sourcePath, null);
					}
					Environment.Exit(0);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Console.WriteLine("sourcePath: " + sourcePath);
				Console.WriteLine("destinationPath: " + destinationPath);
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Search for .jar files in the directory
		/// and return the first .jar filename if found, otherwise return null
		/// </summary>
		/// <returns>The first .jar filename if found, otherwise null</returns>
		private static String FindJar()
		{
			// Search for .jar files in the directory
			string[] jarFiles = Directory.GetFiles(GetObjectsFolder(), "*.jar");

			// Return the first .jar filename if found, otherwise return null
			return jarFiles.Length > 0 ? Path.GetFileName(jarFiles[0]) : null;
		}

		/// <summary>
		/// Gets the path of the Files folder.
		/// </summary>
		/// <returns>The path of the Files folder.</returns>
		/// <remarks>
		/// This method returns the path of the Files folder, which is located in the same directory as the Objects folder.
		/// </remarks>
		private static string GetFilesFolder()
		{
			string Files = Path.GetDirectoryName(GetObjectsFolder());
			return Path.Combine(Files, "Files");
		}

		/// <summary>
		/// Gets the current working directory of the application.
		/// </summary>
		/// <returns>The path of the current working directory.</returns>
		private static string GetObjectsFolder()
		{
			return Directory.GetCurrentDirectory();
		}

		/// <summary>
		/// Gets the path of the project folder.
		/// </summary>
		/// <returns>The path of the project folder.</returns>
		private static string GetProjectFolder()
		{
			return Path.GetDirectoryName(GetObjectsFolder());
		}

		/// <summary>
		/// Retrieves the name of the project file in the project folder.
		/// </summary>
		/// <returns>The project name without the file extension, or "unknown" if no project file is found.</returns>
		/// <remarks>
		/// This method searches for files with the ".b4?" extension in the project folder and returns the name of the first match found.
		/// If no such files are present, it defaults to returning "unknown".
		/// </remarks>
		private static string GetProjectName()
		{
			string[] projectFiles = Directory.GetFiles(GetProjectFolder(), "*.b4?");
			string project = null;
			if (projectFiles.Length > 0) project = Path.GetFileName(projectFiles[0]);
			if (project != null) return project.Substring(0, project.LastIndexOf("."));
			return "unknown";
		}

		/// <summary>
		/// Copies a file to a destination file.
		/// </summary>
		/// <param name="sourceFile">The path of the file to copy.</param>
		/// <param name="destinationFile">The path of the file to copy to.</param>
		/// <remarks>
		/// If the destination directory does not exist, it is created.
		/// If a file with the same name already exists at the destination, it is overwritten.
		/// </remarks>
		private static void CopyFile(string sourceFile, string destinationFile)
		{
			string destinationDir = Path.GetDirectoryName(destinationFile);

			if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
			{
				Directory.CreateDirectory(destinationDir);
			}

			File.Copy(sourceFile, destinationFile, overwrite: true);
		}

		/// <summary>
		/// Moves all B4A AutoBackup zip files to a specified destination directory.
		/// </summary>
		/// <param name="destinationDir">The destination directory to move the zip files to.</param>
		/// <remarks>
		/// This method moves all zip files from the AutoBackups folder in the project directory
		/// to the destination directory and renames them to include the project name.
		/// The destination directory is created if it does not already exist.
		/// The method will exit the program with a status code of 0 if no errors occur, or 1 if an error occurs.
		/// </remarks>
		private static void MoveAutoBackups(string destinationDir)
		{
			try
			{
				string sourceDir = Path.Combine(GetProjectFolder(), "AutoBackups");

				destinationDir = Path.Combine(destinationDir, GetProjectName());

				if (!Directory.Exists(destinationDir))
				{
					Directory.CreateDirectory(destinationDir);
				}

				if (Directory.Exists(sourceDir))
				{
					string[] zipFiles = Directory.GetFiles(sourceDir, "*.zip");

					// Copy each .zip file to the destination directory
					foreach (string file in zipFiles)
					{
						string fileName = Path.GetFileName(file);
						string destinationPath = Path.Combine(destinationDir, fileName);

						// Copy file and overwrite if it already exists
						File.Move(file, destinationPath);
						Console.WriteLine("Copied: " + fileName);
					}
				}

				Environment.Exit(0);
			}

			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Environment.Exit(1);
			}

		}

		/// <summary>
		/// Copy all files and subdirectories from the source directory to the destination directory.
		/// If the destination directory does not exist, it will be created.
		/// All files in the source directory will be copied to the destination directory.
		/// All subdirectories in the source directory will be copied to the destination directory
		/// using a recursive call to this method.
		/// </summary>
		/// <param name="sourceDir">The source directory to copy from.</param>
		/// <param name="destinationDir">The destination directory to copy to.</param>
		private static void CopyDirectory(string sourceDir, string destinationDir)
		{
			// Create destination directory if it does not exist
			if (!Directory.Exists(destinationDir))
			{
				Directory.CreateDirectory(destinationDir);
			}

			// Copy all files in the source directory
			foreach (string file in Directory.GetFiles(sourceDir))
			{
				string destinationFile = Path.Combine(destinationDir, Path.GetFileName(file));
				File.Copy(file, destinationFile, overwrite: true);
			}

			// Recursively copy all subdirectories
			foreach (string directory in Directory.GetDirectories(sourceDir))
			{
				string destinationSubDir = Path.Combine(destinationDir, Path.GetFileName(directory));
				CopyDirectory(directory, destinationSubDir);
			}
		}



	}
}


