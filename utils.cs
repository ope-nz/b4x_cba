using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace B4XCustomActions
{
    public static class Utils
    {
        /// <summary>
        /// Reads a GitHub API key from a file in the same folder as the B4X installer.
        /// If the file does not exist, it is created with a value entered by the user.
        /// </summary>
        /// <returns>The GitHub API key as a string.</returns>
        public static string GetGitHubAPIKeyFromInstallFolder()
        {
            string installer_folder = GetInstallFolder();
            string path = Path.Combine(installer_folder, "github_api_key.txt");
            if (!File.Exists(path))
            {
                string input = InputBox.Show("B4X Custom Build Actions", "Please enter your GitHub API Key:");

                if (!string.IsNullOrWhiteSpace(input))
                {
                    File.WriteAllText(path, input);
                }
                else
                {
                    Console.WriteLine("No API Key entered.");
                    Environment.Exit(1);
                }
            }

            if (File.ReadAllText(path).Trim() == "")
            {
                Console.WriteLine("API Key is empty.");
                Environment.Exit(1);
            }

            return File.ReadAllText(path).Trim();
        }

        /// <summary>
        /// Creates the .gitattribute file in the project folder if it doesn't exist.
        /// This file is used by GitHub to determine the language of source files.
        /// </summary>
        public static void CreateGitAttributeFileIfNotExist()
        {
            string gitattribute = Path.Combine(GetProjectFolder(), ".gitattribute");
            if (!File.Exists(gitattribute))
            {
                // Write out .gitattribute if it doesnt exist
                StringBuilder sb = new StringBuilder();
                sb.Append("# Auto detect text files and perform LF normalization").Append(Environment.NewLine);
                sb.Append("* text=auto").Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("*.b4a linguist-language=B4X").Append(Environment.NewLine);
                sb.Append("*.b4i linguist-language=B4X").Append(Environment.NewLine);
                sb.Append("*.b4j linguist-language=B4X").Append(Environment.NewLine);
                sb.Append("*.bas linguist-language=B4X").Append(Environment.NewLine);
                sb.Append("*.b4a linguist-detectable=true").Append(Environment.NewLine);
                sb.Append("*.b4i linguist-detectable=true").Append(Environment.NewLine);
                sb.Append("*.b4j linguist-detectable=true").Append(Environment.NewLine);
                sb.Append("*.bas linguist-detectable=true").Append(Environment.NewLine);
                File.WriteAllText(gitattribute, sb.ToString());
            }
        }

        /// <summary>
        /// Reads the content of the .gitignore file in the project folder.
        /// If the file does not exist, it is created with default values.
        /// The content is then parsed and returned as an array of strings
        /// representing the patterns to ignore.
        /// </summary>
        /// <returns>An array of ignore patterns, or an empty array if the file does not exist.</returns>
        public static string[] GetGitHubIgnoreArray()
        {
            // Write out .gitignore if it doesnt exist
            string gitignore = Path.Combine(GetProjectFolder(), ".gitignore");
            if (!File.Exists(gitignore))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("# ignore files").Append(Environment.NewLine);
                sb.Append("*.meta").Append(Environment.NewLine);
                sb.Append("*.zip").Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("# ignore ALL files in these directories").Append(Environment.NewLine);
                sb.Append("AutoBackups/").Append(Environment.NewLine);
                sb.Append("Objects/").Append(Environment.NewLine);
                File.WriteAllText(gitignore, sb.ToString());
            }

            List<string> ignorePatterns = new List<string>();

            foreach (string line in File.ReadAllLines(gitignore))
            {
                string tline = line.Trim();
                if (string.IsNullOrWhiteSpace(tline) || tline.StartsWith("#")) continue; // Skip empty lines and comments
                ignorePatterns.Add(tline);
            }

            return ignorePatterns.ToArray();
        }

        /// <summary>
        /// Reads the value of the given key from the project configuration file.
        /// </summary>
        /// <param name="key">The key to look up in the project configuration file.</param>
        /// <returns>The value associated with the key, or <c>null</c> if the key is not found.</returns>
        public static string GetProjectConfigValue(string key)
        {
            string project = GetProjectPath();
            string projectContent = File.ReadAllText(project).Trim();
            string find = key + "=";

            int idx1 = projectContent.IndexOf(find);

            if (idx1 > 0)
            {
                idx1 = idx1 + find.Length;
                int idx2 = projectContent.IndexOf("\n", idx1);

                if (idx2 > 0)
                {
                    string thisVal = projectContent.Substring(idx1, idx2 - idx1).Trim();
                    if (thisVal.Contains("%")) thisVal = ResolveVariables(thisVal);
                    return thisVal;
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates the SHA256 hash of a file.
        /// </summary>
        /// <param name="filePath">The path of the file to hash.</param>
        /// <returns>The SHA256 hash of the file as a hexadecimal string.</returns>
        public static string GetSHA256Hash(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(fs);
                StringBuilder hexString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hexString.Append(b.ToString("x2"));
                }
                return hexString.ToString();
            }
        }

        /// <summary>
        /// Determines if a given string pattern contains wildcard characters.
        /// </summary>
        /// <param name="pattern">The string pattern to check for wildcards.</param>
        /// <returns>True if the pattern contains '*' or '?', otherwise false.</returns>

        static bool IsWildcardPattern(string pattern)
        {
            return pattern.Contains("*") || pattern.Contains("?");
        }

        /// <summary>
        /// Converts a wildcard pattern into a regular expression.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to convert.</param>
        /// <returns>The regular expression equivalent of the wildcard pattern.</returns>
        /// <remarks>
        /// This function escapes all characters in the pattern, then replaces '*' with ".*" and '?' with ".".
        /// The resulting regular expression will match strings that match the wildcard pattern.
        /// </remarks>
        static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace("\\*", ".*")
                              .Replace("\\?", ".") + "$";
        }

        // Normalize paths for consistent comparison
        static string NormalizePath(string path)
        {
            return path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // Check if a path should be excluded
        public static bool ShouldExclude(string path, string[] ignore)
        {
            List<Regex> regexList = new List<Regex>();
            foreach (var pattern in ignore)
            {
                if (path.ToLower().EndsWith(pattern.ToLower())) return true; //Should match file names
                if (path.Replace("\\", "/").ToLower().StartsWith(pattern.ToLower())) return true; //Should match folder names

                string regexPattern = Utils.WildcardToRegex(pattern);
                regexList.Add(new Regex(regexPattern, RegexOptions.IgnoreCase));
            }

            foreach (var regex in regexList)
            {
                if (regex.IsMatch(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Recursively retrieves all files in the specified folder and its subdirectories.
        /// </summary>
        /// <param name="folderPath">The path of the folder to search for files.</param>
        /// <returns>An array of file paths relative to the provided folder path.</returns>

        public static string[] GetFilesRecursively(string folderPath)
        {
            List<string> fileList = new List<string>();
            ProcessDirectory(folderPath, folderPath, fileList);
            return fileList.ToArray();
        }

        /// <summary>
        /// Recursively adds all files in the specified directory and its subdirectories to the fileList.
        /// </summary>
        /// <param name="currentFolder">The path of the current folder being processed.</param>
        /// <param name="baseFolder">The base folder path to make the file paths relative to.</param>
        /// <param name="fileList">A list of file paths relative to the base folder.</param>
        private static void ProcessDirectory(string currentFolder, string baseFolder, List<string> fileList)
        {
            // Add files with relative paths
            foreach (string file in Directory.GetFiles(currentFolder))
            {
                string relativePath = file.Replace(baseFolder + Path.DirectorySeparatorChar, "");
                fileList.Add(relativePath);
            }

            // Recursively process subdirectories
            foreach (string subdirectory in Directory.GetDirectories(currentFolder))
            {
                ProcessDirectory(subdirectory, baseFolder, fileList);
            }
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
		public static void CopyFile(string sourceFile, string destinationFile)
        {
            string destinationDir = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            File.Copy(sourceFile, destinationFile, overwrite: true);
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
		public static void CopyDirectory(string sourceDir, string destinationDir)
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


        /// <summary>
        /// Search for .jar files in the directory
        /// and return the first .jar filename if found, otherwise return null
        /// </summary>
        /// <returns>The first .jar filename if found, otherwise null</returns>
        public static String FindJar()
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
        public static string GetFilesFolder()
        {
            string Files = Path.GetDirectoryName(GetObjectsFolder());
            return Path.Combine(Files, "Files");
        }

        /// <summary>
        /// Gets the current working directory of the application.
        /// </summary>
        /// <returns>The path of the current working directory.</returns>
        public static string GetObjectsFolder()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Gets the path of the folder where the application's executable file is located.
        /// </summary>
        /// <returns>The path of the folder where the application's executable file is located.</returns>
        public static string GetInstallFolder()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(exePath);
        }

        /// <summary>
        /// Gets the path of the project folder.
        /// </summary>
        /// <returns>The path of the project folder.</returns>
        public static string GetProjectFolder()
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
        public static string GetProjectName()
        {
            string projectPath = GetProjectPath();
            string project = Path.GetFileName(projectPath);
            if (project != null && project.Length > 0) return project.Substring(0, project.LastIndexOf("."));
            return "unknown";
        }

        /// <summary>
        /// Retrieves the path of the project file in the project folder.
        /// </summary>
        /// <returns>The path of the project file, or <c>null</c> if no project file is found.</returns>
        /// <remarks>
        /// This method searches for files with the ".b4?" extension in the project folder and returns the path of the first match found.
        /// If no such files are present, it defaults to returning <c>null</c>.
        /// </remarks>
        public static string GetProjectPath()
        {
            string[] projectFiles = Directory.GetFiles(GetProjectFolder(), "*.b4?");
            if (projectFiles.Length == 0) return null;
            return projectFiles[0];
        }

        /// <summary>
        /// Resolves a path relative to the Objects folder or Files folder.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>The fully qualified path.</returns>
        /// <remarks>
        /// If the path starts with "Files", it is assumed to be relative to the Files folder.
        /// If the path starts with "Objects", it is assumed to be relative to the Objects folder.
        /// If the path does not contain a drive letter, it is assumed to be a relative path and is appended to the Objects folder.
        /// </remarks>
        public static string ResolvePath(string path)
        {
            if (path.StartsWith("Files")) return Path.Combine(GetFilesFolder(), path.Substring(5));
            if (path.StartsWith("Objects")) return Path.Combine(GetObjectsFolder(), path.Substring(7));

            // If no drive then assume its a relative path, append base directory
            if (!path.Contains(":")) path = Path.Combine(GetObjectsFolder(), path);

            return path;
        }

        /// <summary>
        /// Replaces placeholders in the input string with corresponding values.
        /// </summary>
        /// <param name="content">The input string containing placeholders.</param>
        /// <returns>The string with placeholders replaced by their respective values.</returns>
        /// <remarks>
        /// This method recognizes the following placeholders and replaces them:
        /// - %VERSION%: Replaced with the current version retrieved by GetVersion().
        /// - %DATE%: Replaced with the current date in "yyyyMMdd" format.
        /// - %TIME%: Replaced with the current time in "HHmmss" format.
        /// - %PROJECT_NAME%: Replaced with the project name retrieved by GetProjectName().
        /// - %JAR%: Replaced with the name of the first .jar file found.
        /// - %JAR_NAME%: Replaced with the name of the first .jar file found, without the ".jar" extension.
        /// </remarks>
        public static string ResolveVariables(string content)
        {
            if (content.Contains("%VERSION%")) content = content.Replace("%VERSION%", GetVersion());
            if (content.Contains("%DATE%")) content = content.Replace("%DATE%", DateTime.Now.ToString("yyyyMMdd"));
            if (content.Contains("%TIME%")) content = content.Replace("%TIME%", DateTime.Now.ToString("HHmmss"));
            if (content.Contains("%PROJECT_NAME%")) content = content.Replace("%PROJECT_NAME%", GetProjectName());
            if (content.Contains("%JAR%")) content = content.Replace("%JAR%", FindJar());
            if (content.Contains("%JAR_NAME%")) content = content.Replace("%JAR_NAME%", FindJar().Replace(".jar", ""));

            return content;
        }

        /// <summary>
        /// Retrieves the current version number from the "version.txt" file in the Files folder.
        /// </summary>
        /// <returns>The version number as a string, or an empty string if the file does not exist.</returns>
        private static string GetVersion()
        {
            string TargetFile = Path.Combine(GetFilesFolder(), "version.txt");
            if (File.Exists(TargetFile)) return File.ReadAllText(TargetFile).Trim();
            return "";
        }

        /// <summary>
        /// Checks whether a given path is a file path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path is a file path, <c>false</c> if it is a directory path.</returns>
        /// <remarks>
        /// This method checks whether the given path is a file path by checking if it already exists as a file, or if it has a file extension.
        /// </remarks>
        public static bool IsFilePath(string path)
        {
            // Check if it already exists         
            if (File.Exists(path)) return true;

            // Get the file extension (returns empty string if none)
            string extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension);
        }

        /// <summary>
        /// Determines whether the specified path is a directory path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path is a directory path; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This method returns <c>true</c> if the path exists as a directory or if the path does not have a file extension, 
        /// indicating it is likely a directory path.
        /// </remarks>
        public static bool IsDirectoryPath(string path)
        {
            // Check if it already exists
            if (Directory.Exists(path)) return true;

            // If no extension, it's likely a directory
            return string.IsNullOrEmpty(Path.GetExtension(path));
        }

    }
}

public class InputBox
{
    /// <summary>
    /// Displays a modal input dialog box with a specified title and prompt text.
    /// </summary>
    /// <param name="title">The title of the input dialog box.</param>
    /// <param name="promptText">The text to display as a prompt in the dialog box.</param>
    /// <returns>The text entered by the user if the OK button is pressed; otherwise, an empty string.</returns>
    /// <remarks>
    /// The dialog contains an input TextBox, an OK button, and a Cancel button. 
    /// The OK button confirms the input, while the Cancel button dismisses the dialog without input.
    /// </remarks>

    public static string Show(string title, string promptText)
    {
        Form form = new Form();
        Label label = new Label();
        TextBox textBox = new TextBox();
        Button buttonOk = new Button();
        Button buttonCancel = new Button();
        string input = string.Empty;

        form.Text = title;
        label.Text = promptText;
        textBox.Text = "";

        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;

        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 372, 20);
        buttonOk.SetBounds(228, 72, 75, 23);
        buttonCancel.SetBounds(309, 72, 75, 23);

        label.AutoSize = true;
        textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.ClientSize = new System.Drawing.Size(396, 107);
        form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
        form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        if (form.ShowDialog() == DialogResult.OK)
        {
            input = textBox.Text;
        }
        return input;
    }
}