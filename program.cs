using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime;
//using System.Linq;
using System.Text;

namespace B4XCustomActions
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				//string ThisLocation = Directory.GetCurrentDirectory();

				//Console.WriteLine(ThisLocation);
				//Console.WriteLine(args.ToString());

				int c = args.GetUpperBound(0);

				string TAction = "";
				string TDirectory = "";
				string TDateFormat = "yyyy-MM-dd";
				string TTimeFormat = "HH:mm:ss";
				string TSource = "";
				string TDestination = "";

				// Loop through arguments
				for (int n = 0; n < c; n++)
				{
					string thisKey = args[n].ToLower();
					string thisVal = args[n + 1].TrimEnd().TrimStart();

					//Console.WriteLine(thisKey);
					//Console.WriteLine(thisVal);

					// eval the key or slash-switch option ("/key")
					switch (thisKey)
					{
						case "-action":
							TAction = thisVal;
							break;
						case "-directory":
							TDirectory = thisVal;
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

				Console.WriteLine("#####################");
				Console.WriteLine("    B4X Custom Action");
				Console.WriteLine("#####################");

				switch (TAction)
				{
					case "compileonly":
						Console.WriteLine("Compile Only");
						Environment.Exit(1);
						break;
					case "copyjar":
						Console.WriteLine("Copy Jar");
						CopyJar(TDirectory);
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

		private static void CopyJar(string TargetDirectory)
		{
			try
			{
				String jar = FindJar();
				if (jar != null)
				{
					File.Copy(Path.Combine(GetObjectsFolder(), jar), Path.Combine(TargetDirectory, jar), overwrite: true);
				}
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Environment.Exit(1);
			}
		}

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
				Environment.Exit(1);
			}
		}

		private static void UpdateVersion()
		{
			try
			{
				string TargetFile = Path.Combine(GetFilesFolder(), "version.txt");

				// Default version
				string version = "0.0.1";

				if (File.Exists(TargetFile))
				{
					// Read the current version from the file
					version = File.ReadAllText(TargetFile).Trim();

					// Split the version into major, minor, and build
					var versionParts = version.Split('.');

					int major = int.Parse(versionParts[0]);
					int minor = int.Parse(versionParts[1]);
					int build = int.Parse(versionParts[2]);

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
				}

				// Write the updated version back to the file
				File.WriteAllText(TargetFile, version);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Environment.Exit(1);
			}
		}

		public static void CopyPath(string sourcePath, string destinationPath)
		{
			try
			{
				if (sourcePath.StartsWith("Files")) sourcePath = Path.Combine(GetFilesFolder(),sourcePath.Substring(5));
				if (destinationPath.StartsWith("Files")) destinationPath = Path.Combine(GetFilesFolder(),destinationPath.Substring(5));

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
				Environment.Exit(1);
			}
		}

		private static void Zip(string sourcePath, string destinationPath)
		{
			try
			{
				if (sourcePath.StartsWith("Files")) sourcePath = Path.Combine(GetFilesFolder(),sourcePath.Substring(5));
				if (destinationPath.StartsWith("Files")) destinationPath = Path.Combine(GetFilesFolder(),destinationPath.Substring(5));

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
						Console.WriteLine("JHello");
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
				Environment.Exit(1);
			}
		}

		private static String FindJar()
		{
			// Search for .jar files in the directory
			string[] jarFiles = Directory.GetFiles(GetObjectsFolder(), "*.jar");

			// Return the first .jar filename if found, otherwise return null
			return jarFiles.Length > 0 ? Path.GetFileName(jarFiles[0]) : null;
		}

		private static string GetFilesFolder()
		{
			string Files = Path.GetDirectoryName(GetObjectsFolder());
			return Path.Combine(Files, "Files");
		}

		private static string GetObjectsFolder()
		{
			return Directory.GetCurrentDirectory();
		}

		private static void CopyFile(string sourceFile, string destinationFile)
		{
			string destinationDir = Path.GetDirectoryName(destinationFile);

			if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
			{
				Directory.CreateDirectory(destinationDir);
			}

			File.Copy(sourceFile, destinationFile, overwrite: true);
		}

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


