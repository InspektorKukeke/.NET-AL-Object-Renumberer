using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ALObjectRenumberer
{
    class Program
    {

        static void Main(string[] args)
        {

            List <FileInfo> filesInfo = new List<FileInfo>();

            Console.WriteLine("Enter path to the project root folder(example: C:\\BC365\\ISV\\EXTENSION):");
            string[] sFolderPath = new string[1] { Console.ReadLine() };
            string[] folderName = sFolderPath[0].Split("\\");
       
            Console.WriteLine("Enter starting number of the new object range(example: 100000):");
            string sStartRange = Console.ReadLine();
            long StartRange = Convert.ToInt64(sStartRange);
            long EndRange = 0;
            string[] applines;
            string appText = "";

            foreach (string path in sFolderPath)
            {
                if (File.Exists(path))
                {
                    // This path is a file
                    GetFileRange(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path, StartRange, filesInfo);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }
                        
            CreateFiles(filesInfo);

            EndRange = filesInfo.Max(m => m.objectID);

            string fileName = Path.Combine(sFolderPath[0], "app.json");
            applines = File.ReadAllLines(fileName);
            foreach (string line in applines)
            {

                if (line.Contains("\"from\":"))
                {
                    appText +=  "\"from\":" + sStartRange + ",";
                }
                else if (line.Contains("\"to\":"))
                {
                    appText += "\"to\":" + EndRange.ToString();

                }else
                {
                    appText += line;
                }

                appText += "\n";
            }

            if(appText != "")
            {
                
                File.Delete(fileName);
                File.WriteAllText(fileName, appText);
            }

        }

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory, long StartRange, List<FileInfo> filesInfo)
        {
            long currentRange;
            long incrementedRange = 0;
            string currentFolder;
            string currentDir = "";
            
            List<string> dirs = new List<string>();
            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
           
            foreach (string subdirectory in subdirectoryEntries)
            {
                currentDir = subdirectory;
                dirs = subdirectory.Split("\\").ToList();
                currentFolder = dirs.Last();

                if (currentFolder.Contains("Codeunit") || currentFolder.Contains("Page") || currentFolder.Contains("Table")
                    || currentFolder.Contains("dotnet") || currentFolder.Contains("XMLPort") || currentFolder.Contains("Report")
                    || currentFolder.Contains("Query") || currentFolder.Contains("Profile") || currentFolder.Contains("XMLport")
                    || currentFolder.Contains("Layout"))
                {
                    ProcessDirectory(subdirectory, StartRange, filesInfo);
                }
            }
            currentDir = targetDirectory;
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                string previousDir = "";
                if (filesInfo.Count > 0) { 
                    previousDir = filesInfo.Last().path;
                }

                if (currentDir != previousDir)
                {
                    incrementedRange = StartRange;
                }
                else
                {
                    incrementedRange += 1;
                }

                currentRange = GetFileRange(fileName);

                if (currentRange != 0)
                {
                    filesInfo.Add(RenumberFile(fileName, currentRange, incrementedRange));
                }

               
            }

        }

        // Insert logic for processing found files here.
        public static long GetFileRange(string path)
        {
            Console.WriteLine("Processed file '{0}'.", path);
            string fileName = Path.GetFileName(path);
            string chars = new string("");
            char[] digits = fileName.ToCharArray();
            foreach (char ch in digits)
            {
                if (char.IsDigit(ch))
                {
                    chars += ch;
                }
                else if (chars != "")
                {
                    return Convert.ToInt64(chars);
                }
                else
                {
                    return 0;
                }
              

            }

            return 0;
        }
        public static FileInfo RenumberFile(string path,long currentRange, long incrementedRange)
        {
            string fileText = File.ReadAllText(path).Replace(currentRange.ToString(), incrementedRange.ToString());
            string fileName = Path.GetFileName(path).Replace(currentRange.ToString(), incrementedRange.ToString());
            string dir = Path.GetDirectoryName(path);

            File.Delete(path);

            FileInfo fileInfo = new FileInfo(dir, fileName, fileText, incrementedRange);

            return fileInfo;
            
        }

        public static void CreateFiles(List<FileInfo> filesInfo)
        {
            foreach(FileInfo fileInfo in filesInfo)
            {
                File.WriteAllText(Path.Combine(fileInfo.path, fileInfo.filename), fileInfo.filecontent);

            }
        }
    }
}

