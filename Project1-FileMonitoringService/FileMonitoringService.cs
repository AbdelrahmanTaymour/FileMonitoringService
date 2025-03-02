using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Project1_FileMonitoringService
{
    public partial class FileMonitoringService : ServiceBase
    {
        private FileSystemWatcher fileWatcher;
        private string sourceFolder;
        private string destinationFolder;
        private string logFolder;

        public FileMonitoringService()
        {
            InitializeComponent();

            sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            destinationFolder = ConfigurationManager.AppSettings["DestinationFolder"];
            logFolder = ConfigurationManager.AppSettings["LogFolder"];

            if (string.IsNullOrWhiteSpace(sourceFolder))
            {
                sourceFolder = @"C:\FileMonitoring\Source";
                Log($"SourceFolder is missing in App.Config. Using default: {sourceFolder}");
            }

            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                destinationFolder = @"C:\FileMonitoring\Destination";
                Log($"Destination Folder is missing in App.Config. Using default: {destinationFolder}");
            }

            if (string.IsNullOrWhiteSpace(logFolder))
            {
                logFolder = @"C:\FileMonitoring\Logs";
                Log($"Log Folder is missing in App.Config. Using default: {logFolder}");
            }
            Directory.CreateDirectory(sourceFolder);
            Directory.CreateDirectory(destinationFolder);
            Directory.CreateDirectory(logFolder);

        }

        protected override void OnStart(string[] args)
        {
            Log("Service is started");

            fileWatcher = new FileSystemWatcher
            {
                Path = sourceFolder,
                Filter = "*.*",
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            fileWatcher.Created += OnFileCreated;

            Log("File monitoring started on folder: " + sourceFolder);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                Log($"File detected: {e.FullPath}");
                
                string newFileName = $"{Guid.NewGuid()}{Path.GetExtension(e.Name)}";
                string destinationFile = Path.Combine(destinationFolder, newFileName);

                File.Move(e.FullPath, destinationFile);

                Log($"File moved: {e.FullPath} -> {destinationFile}");

            }
            catch (Exception ex) 
            {
                Log($"Error processing file: {e.FullPath}. Exception: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
            Log("Service Stopped.");
        }

        private void Log(string message)
        {
            string logFilePath = Path.Combine(sourceFolder, "ServiceLog.txt");
            string logMessage = $"[{DateTime.Now:yyyy:MM:dd HH:mm:ss}]  {message}";

            File.AppendAllText(logFilePath, logMessage);

            if (Environment.UserInteractive)
            {
                Console.WriteLine(logMessage);
            }
        }

        public void StartInConsole()
        {
            OnStart(null);
            Console.WriteLine("Press Enter to stop the service...");
            Console.ReadLine();
            OnStop();
            Console.ReadKey();
        }
    }
}
