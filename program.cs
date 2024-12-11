using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Linq;
using System.Text;

namespace B4XCustomActions
{
    class Program
    {
        static void Main(string[] args)
        {		
			try
			{
				string ThisLocation = Directory.GetCurrentDirectory();
				
				//Console.WriteLine(ThisLocation);
				//Console.WriteLine(args.ToString());

				int c = args.GetUpperBound(0);

				string TAction = "";
				string TDirectory = "";
				string TDateFormat = "yyyy-MM-dd";
				string TTimeFormat = "HH:mm:ss";

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
						case "-dateformat":
							TDateFormat = thisVal;
							break;
						case "-timeformat":
							TTimeFormat = thisVal;
							break;
					}
				}

				Console.WriteLine("#####################");				
				Console.WriteLine("# B4X Custom Action #");
				Console.WriteLine("#####################");				

				switch (TAction)
				{
					case "compileonly":
						CompileOnly();
						break;				
					case "copyjar":
						CopyJar(ThisLocation,TDirectory);						
						break;	
					case "buildtime":
						BuildTime(ThisLocation,TDateFormat,TTimeFormat);
						break;		
					case "updateversion":
						UpdateVersion(ThisLocation);
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
		
		private static void CompileOnly()
		{
			Console.WriteLine("Compile Only");					
			Environment.Exit(1);
		}
		
		private static void CopyJar(string BaseDirectory,string TargetDirectory)
		{			
			try{				
				String jar = FindJar(BaseDirectory);
				if (jar != null){
					File.Copy(Path.Combine(BaseDirectory,jar), Path.Combine(TargetDirectory,jar), overwrite: true);
				}
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Environment.Exit(1);
			}	
		}

		private static void BuildTime(string BaseDirectory, string DateFormat, string TimeFormat)
		{
			// Define the desired format for the DateTime string
			string dateFormat = DateFormat+" "+TimeFormat;
        
			// Get the current DateTime and format it
			string dateTimeString = DateTime.Now.ToString(dateFormat);
			
			try
			{
				string TargetFile = Path.GetDirectoryName(BaseDirectory);
				TargetFile = Path.Combine(TargetFile,"Files\\build.txt");
				File.WriteAllText(TargetFile, dateTimeString);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred: " + ex.Message);
				Environment.Exit(1);
			}
		}
		
		private static void UpdateVersion(string BaseDirectory)
		{
			try{
				string TargetFile = Path.GetDirectoryName(BaseDirectory);
				TargetFile = Path.Combine(TargetFile,"Files\\version.txt");

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
					version = major+"."+minor+"."+build;
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
		
		private static String FindJar(string BaseDirectory)
		{		
			// Search for .jar files in the directory
			string[] jarFiles = Directory.GetFiles(BaseDirectory, "*.jar");

			// Return the first .jar file if found, otherwise return null
			return jarFiles.Length > 0 ? Path.GetFileName(jarFiles[0]) : null;
		}
    }
}


