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

namespace B4XCustomActions
{
    public static class GitHub
    {
        /// <summary>
        /// Checks if a GitHub repository exists with the given API key, owner and repository name.
        /// </summary>
        /// <param name="apiKey">The GitHub API key.</param>
        /// <param name="owner">The owner of the repository.</param>
        /// <param name="repo">The name of the repository.</param>
        /// <returns>True if the repository exists, otherwise false.</returns>
        public static bool CheckIfRepositoryExists(string apiKey, string owner, string repo)
        {
            string repoUrl = "https://api.github.com/repos/" + owner + "/" + repo;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(repoUrl);
            request.Method = "GET";

            // Set up the authorization header
            request.Headers.Add("Authorization", "token " + apiKey);
            request.UserAgent = "b4x_cba";

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch
            {
                //Fail silently
                return false;
            }

            // Return true if the response status is 200 (OK), otherwise false
            return (200 == (int)response.StatusCode);
        }

        /// <summary>
        /// Retrieves a list of files from a GitHub repository.
        /// </summary>
        /// <param name="apiKey">The GitHub API key.</param>
        /// <param name="owner">The owner of the repository.</param>
        /// <param name="repo">The name of the repository.</param>
        /// <param name="branch">The branch of the repository.</param>
        /// <param name="directory">The directory to retrieve files from. If null, will retrieve files from the root of the repository.</param>
        /// <returns>A dictionary of file names and their corresponding SHA values.</returns>
        public static Dictionary<string, string> GetFileList(string apiKey, string owner, string repo, string branch, string directory = null)
        {
            var filesDictionary = new Dictionary<string, string>();

            string fileUrl = "https://api.github.com/repos/" + owner + "/" + repo + "/contents?ref=" + branch;

            if (directory != null)
            {
                fileUrl = "https://api.github.com/repos/" + owner + "/" + repo + "/contents/" + directory + "?ref=" + branch;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileUrl);
            request.Method = "GET";

            // Set up the authorization header	
            request.Headers.Add("Authorization", "token " + apiKey);
            request.UserAgent = "b4x_cba";
            request.Timeout = 11000;

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();

                if ((int)response.StatusCode == 200)
                {
                    System.IO.Stream responseStream = response.GetResponseStream();
                    System.IO.StreamReader readStream = new System.IO.StreamReader(responseStream);
                    string json = readStream.ReadToEnd();
                    readStream.Close();
                    responseStream.Close();
                    response.Close();

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var files = serializer.Deserialize<List<GitHubFile>>(json);

                    // Populate dictionary with file name and sha
                    foreach (var file in files)
                    {
                        if (file.type == "dir")
                        {
                            string dir = file.name;
                            Dictionary<string, string> filesDictionarySub = GetFileList(apiKey, owner, repo, branch, dir);
                            foreach (KeyValuePair<string, string> kvp in filesDictionarySub) { filesDictionary[Path.Combine(dir, kvp.Key)] = kvp.Value; }
                        }
                        else
                        {
                            filesDictionary[file.name] = file.sha;
                        }
                    }

                    return filesDictionary;
                }
                else
                {
                    Console.WriteLine("GetFileList HttpWebResponse StatusCode: " + response.StatusCode + " for " + fileUrl);
                    return filesDictionary;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return filesDictionary;
            }
        }

        public class GitHubFile
        {
            public string name { get; set; }
            public string sha { get; set; }
            public string type { get; set; }
        }

        /// <summary>
        /// Deletes the specified file from the specified branch of the specified GitHub repository.
        /// </summary>
        /// <param name="apiKey">The GitHub API key.</param>
        /// <param name="owner">The owner of the repository.</param>
        /// <param name="repo">The name of the repository.</param>
        /// <param name="branch">The branch of the repository.</param>
        /// <param name="filePath">The file path of the file to delete.</param>
        /// <param name="sha">The SHA of the file to delete.</param>
        /// <returns>True if the file was successfully deleted, otherwise false.</returns>
        public static bool DeleteFile(string apiKey, string owner, string repo, string branch, string filePath, string sha)
        {
            string fileUrl = "https://api.github.com/repos/" + owner + "/" + repo + "/contents/" + filePath + "?ref=" + branch;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileUrl);
            request.Method = "DELETE";

            // Set up the authorization header	
            request.Headers.Add("Authorization", "token " + apiKey);
            request.UserAgent = "b4x_cba";
            request.ContentType = "application/json";

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{";
                json = json + "\"message\":\"Deleted file\"";
                json = json + ",\"branch\":\"" + branch + "\"";
                json = json + ",\"sha\":\"" + sha + "\"";
                json = json + "}";
                streamWriter.Write(json);
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }

            // Return true if the response status is 200 (OK), otherwise false	
            return (200 == (int)response.StatusCode);
        }


        /// <summary>
        /// Uploads a file to the specified branch of the specified GitHub repository.
        /// </summary>
        /// <param name="apiKey">The GitHub API key.</param>
        /// <param name="owner">The owner of the repository.</param>
        /// <param name="repo">The name of the repository.</param>
        /// <param name="branch">The branch of the repository.</param>
        /// <param name="filePath">The file path of the file to upload.</param>
        /// <param name="message">The commit message.</param>
        /// <param name="sha">The SHA of the file to update. If null, a new file will be created.</param>
        /// <returns>True if the file was successfully uploaded, otherwise false.</returns>
        public static bool UploadFile(string apiKey, string owner, string repo, string branch, string filePath, string message, string sha)
        {
            byte[] fileContent = File.ReadAllBytes(Path.Combine(Utils.GetProjectFolder(), filePath));
            string encodedContent = Convert.ToBase64String(fileContent);

            string fileUrl = "https://api.github.com/repos/" + owner + "/" + repo + "/contents/" + filePath;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileUrl);
            request.Method = "PUT";

            // Set up the authorization header	
            request.Headers.Add("Authorization", "token " + apiKey);
            request.UserAgent = "b4x_cba";
            request.ContentType = "application/json";

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{";
                json = json + "\"message\":\"" + message + "\"";
                json = json + ",\"branch\":\"" + branch + "\"";
                json = json + ",\"content\":\"" + encodedContent + "\"";
                if (sha != null) json = json + ",\"sha\":\"" + sha + "\"";
                json = json + "}";
                streamWriter.Write(json);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                int statusCode = (int)response.StatusCode;
                //Console.WriteLine(statusCode.ToString());
                response.Close();
                return (200 == statusCode || 201 == statusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Calculates the SHA256 hash of a local file to compare with GitHub.
        /// </summary>
        /// <param name="filePath">The path to the file to hash.</param>
        /// <returns>The SHA256 hash as a string.</returns>
        public static string CalculateLocalFileSHA256(string filePath)
        {
            byte[] originalArray = File.ReadAllBytes(filePath);
            byte[] blob = AppendSizeAndNullByte(originalArray);

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(blob);
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashString.AppendFormat("{0:x2}", b);
                }
                return hashString.ToString();
            }

        }

        /// <summary>
        /// Prepends a "blob {length}\0" prefix to the input, as required by the GitHub API
        /// </summary>
        /// <param name="input">The input byte array</param>
        /// <returns>The modified byte array</returns>
        public static byte[] AppendSizeAndNullByte(byte[] input)
        {
            byte[] prefix = Encoding.ASCII.GetBytes("blob " + input.Length.ToString() + "\0");
            byte[] result = new byte[prefix.Length + input.Length];
            Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
            Buffer.BlockCopy(input, 0, result, prefix.Length, input.Length);
            return result;
        }
    }

}