using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;

namespace B4XCustomActions
{
	class Program
	{
		/// <summary>
		/// Main entry point for the B4XCustomActions library.
		/// This library is intended to be called from the command line as a custom step in a B4X project.
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
				string TFind = "";
				string TReplace = "";
				string TMessage = "";

				// Loop through arguments
				for (int n = 0; n < c; n++)
				{
					string thisKey = args[n].ToLower().TrimEnd().TrimStart();
					string thisVal = args[n + 1].TrimEnd().TrimStart();

					if (thisVal.Contains("%")) thisVal = Utils.ResolveVariables(thisVal);

					//Console.WriteLine(thisKey);
					//Console.WriteLine(thisVal);

					switch (thisKey)
					{
						case "-echo":
							TAction = thisVal;
							break;	
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
						case "-find":
							TFind = thisVal;
							break;
						case "-replace":
							TReplace = thisVal;
							break;
						case "-message":
							TMessage = thisVal;
							break;
					}
				}

				Console.WriteLine(" ");
				Console.WriteLine("###############################################################");
				Console.WriteLine("B4X Custom Action - " + Utils.GetProjectName());
				Console.WriteLine("###############################################################");

				switch (TAction)
				{
					case "echo":
						if (TMessage.Length > 0) Console.WriteLine(TMessage);
						Environment.Exit(0);
						break;
					case "compileonly":
						Console.WriteLine("Compile Only");
						Console.WriteLine(" ");
						if (TMessage.Length > 0) Console.WriteLine(TMessage);
						Console.WriteLine("NOTE: this isn't actually an error!");
						Console.WriteLine(" ");
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
					case "replacetext":
						Console.WriteLine("Find and Replace Text");
						FindAndReplaceText(TSource, TFind, TReplace);
						break;
					case "githubpush":
						Console.WriteLine("Github Push");
						GithubPush();
						break;
					case "jarchecksum":
						Console.WriteLine("SHA256");
						WriteJarChecksum(TDestination);
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
				String jar = Utils.FindJar();

				if (jar != null)
				{
					string sourcePath = Path.Combine(Utils.GetObjectsFolder(), jar);
					CopyPath(sourcePath, destinationPath);
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
				string TargetFile = Path.Combine(Utils.GetFilesFolder(), "build.txt");
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
			string TargetFile = Path.Combine(Utils.GetFilesFolder(), "version.txt");

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
					foreach (char c in version) { if (c == '.') dotCount++; }

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
		/// Copies a file or directory from a specified source path to a destination path.
		/// </summary>
		/// <param name="sourcePath">The source path, which can be a file or a directory. 
		/// If the path is relative, it will be resolved relative to the project folder.</param>
		/// <param name="destinationPath">The destination path, which can be a directory or a file path. 
		/// If the path is relative, it will be resolved relative to the project folder.</param>
		/// <remarks>
		/// If the source path is a file and the destination path is a directory, the file will be copied into the directory.
		/// If the source path is a directory and the destination path is a directory, the entire directory will be copied.
		/// The method will exit the program with a status code of 0 if the copy operation is successful, or 1 if an error occurs.
		/// </remarks>
		public static void CopyPath(string sourcePath, string destinationPath)
		{
			try
			{
				sourcePath = Utils.ResolvePath(sourcePath);
				destinationPath = Utils.ResolvePath(destinationPath);

				bool sourceIsDirectory = Utils.IsDirectoryPath(sourcePath);
				bool sourceIsFile = Utils.IsFilePath(sourcePath);

				bool desitinationIsDirectory = Utils.IsDirectoryPath(destinationPath);
				bool desitinationIsFile = Utils.IsFilePath(destinationPath);

				if (sourceIsFile && !File.Exists(sourcePath))
				{
					Console.WriteLine("source file not found - " + sourcePath);
					Environment.Exit(1);
				}

				if (sourceIsDirectory && !Directory.Exists(sourcePath))
				{
					Console.WriteLine("source directory not found - " + sourcePath);
					Environment.Exit(1);
				}

				if (sourceIsFile && desitinationIsFile)
				{
					Console.WriteLine("Copy File " + sourcePath + " to " + destinationPath);
					Utils.CopyFile(sourcePath, destinationPath);
					Environment.Exit(0);
				}

				if (sourceIsFile && desitinationIsDirectory)
				{
					string FileName = Path.GetFileName(sourcePath);
					destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
					Console.WriteLine("Copy File " + sourcePath + " to " + destinationPath);
					Utils.CopyFile(sourcePath, destinationPath);
					Environment.Exit(0);
				}

				if (sourceIsDirectory && desitinationIsDirectory)
				{
					Console.WriteLine("Copy Directory " + sourcePath + " to " + destinationPath);
					Utils.CopyDirectory(sourcePath, destinationPath);
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
				sourcePath = Utils.ResolvePath(sourcePath);
				destinationPath = Utils.ResolvePath(destinationPath);

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
        /// Pushes the current project to GitHub.
        /// This method writes out .gitattribute and .gitignore files if they do not exist,
        /// and then pushes new or updated files to GitHub.
        /// </summary>
		public static void GithubPush()
		{
			string projectPath = Utils.GetProjectFolder();

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

			string githubAPIKey = Utils.GetGitHubAPIKeyFromInstallFolder();
			string[] githubIgnore = Utils.GetGitHubIgnoreArray();

			string githubRepoName = Utils.GetProjectConfigValue("github_repository_name");
			string githubRepoOwner = Utils.GetProjectConfigValue("github_repository_owner");
			string githubBranchName = Utils.GetProjectConfigValue("github_branch");			

			Console.WriteLine(" ");
			Console.WriteLine(" - Repository Name: " + githubRepoName);
			Console.WriteLine(" - Repository Owner: " + githubRepoOwner);
			Console.WriteLine(" - Branch: " + githubBranchName);

			bool result = GitHub.CheckIfRepositoryExists(githubAPIKey, githubRepoOwner, githubRepoName);

			if (!result)
			{
				Console.WriteLine(" ");
				Console.WriteLine("ERROR: GitHub Repo does not exist or credentials are invalid.");
				Console.WriteLine(" ");
				Environment.Exit(1);
			}

			Dictionary<string, string> filesGitHub = GitHub.GetFileList(githubAPIKey, githubRepoOwner, githubRepoName, githubBranchName);			

			string[] files = Utils.GetFilesRecursively(Utils.GetProjectFolder());

			foreach (string file in files)
			{
				
				if (Utils.ShouldExclude(file, githubIgnore))
				{					
					//Console.WriteLine("Ignoring: " + file);
					continue;
				}				

				Console.WriteLine(" ");
				Console.WriteLine("Processing: " + file);

				string sha = GitHub.CalculateLocalFileSHA256(Path.Combine(Utils.GetProjectFolder(), file));

				string existing_sha = null;
				if (filesGitHub.ContainsKey(file)) existing_sha = filesGitHub[file];

				if (existing_sha == null)
				{
					Console.WriteLine(" - Not in GitHub.");
					Console.WriteLine(" - Uploading to GitHub.");
					bool result1 = GitHub.UploadFile(githubAPIKey, githubRepoOwner, githubRepoName, githubBranchName, file, "Initial commit", null);
					if (result1 == true)
					{
						Console.WriteLine(" - OK");
					}
					else
					{
						Console.WriteLine(" - Upload failed.");
						continue;
					}
				}
				else
				{
					if (existing_sha != sha)
					{
						Console.WriteLine(" - Exists in GitHub.");
						Console.WriteLine(" - Updating GitHub.");
						bool result2 = GitHub.UploadFile(githubAPIKey, githubRepoOwner, githubRepoName, githubBranchName, file, "Update", existing_sha);
						if (result2 == true)
						{
							Console.WriteLine(" - OK");
						}
						else
						{
							Console.WriteLine(" - Upload failed.");
							continue;
						}
					}
					else
					{
						Console.WriteLine(" - Exists in GitHub.");
						Console.WriteLine(" - No changes (Local = GitHub).");
						Console.WriteLine(" - OK");
					}
				}
			}

			//Delete any files that are no longer in the project
			foreach (string file in filesGitHub.Keys)
			{
				if (!files.Contains(file) && file != "README.md")
				{
					Console.WriteLine(" ");
					Console.WriteLine("Processing: " + file);
					Console.WriteLine(" - Not in project.");
					Console.WriteLine(" - Deleting from GitHUb.");
					bool result3 = GitHub.DeleteFile(githubAPIKey, githubRepoOwner, githubRepoName, githubBranchName, file, filesGitHub[file]);
					if (result3 == true)
					{
						Console.WriteLine(" - OK");
					}
					else
					{
						Console.WriteLine(" - Delete failed.");
						continue;
					}
				}
			}

			Console.WriteLine(" ");

			Environment.Exit(0);
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
				string sourceDir = Path.Combine(Utils.GetProjectFolder(), "AutoBackups");

				destinationDir = Path.Combine(destinationDir, Utils.GetProjectName());

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
        /// Replaces all occurrences of a specified string in a file with another string.
        /// </summary>
        /// <param name="sourceFile">The file to search and replace in. If the file path is relative, it will be resolved relative to the project folder.</param>
        /// <param name="find">The string to search for and replace.</param>
        /// <param name="replace">The string to replace the search string with. If the string is a file path, the contents of the file will be used.</param>
        /// <remarks>
        /// The method will exit the program with a status code of 0 if no errors occur, or 1 if an error occurs.
        /// </remarks>
		private static void FindAndReplaceText(string sourceFile, string find, string replace)
		{
			sourceFile = Utils.ResolvePath(sourceFile);

			Console.WriteLine(sourceFile);
			Console.WriteLine(find);
			Console.WriteLine(replace);

			if (!File.Exists(sourceFile))
			{
				Console.WriteLine("Source file does not exist (" + sourceFile + ")");
				Environment.Exit(1);
			}

			string input = File.ReadAllText(sourceFile);

			if (File.Exists(replace)) replace = File.ReadAllText(replace);

			//temp_text = temp_text.Replace(find,replace);

			string temp_text = string.Join("\n", input.Split('\n').Select(line =>
			{
				if (line.Trim().StartsWith("#CustomBuildAction"))
				{
					return line; // Keep the line unchanged
				}
				return line.Replace(find, replace); // Replace
			}));

			File.WriteAllText(sourceFile, temp_text);

			Environment.Exit(0);
		}


		static void WriteJarChecksum(string destinationPath)
		{
			try
			{
				String jar = Utils.FindJar();

				if (destinationPath == "") destinationPath = Utils.GetObjectsFolder();

				Console.WriteLine("Writing checksum for  " + jar + " to " + destinationPath);

				if (jar != null)
				{
					string CheckSum = Utils.GetSHA256Hash(jar);

					Console.WriteLine(CheckSum);

					if (Directory.Exists(destinationPath))
					{
						destinationPath = Path.Combine(destinationPath, jar.Replace(".jar", "") + "_checksum.txt");
					}
					File.WriteAllText(destinationPath, CheckSum);

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
	}
}