using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace CrizGames.Vour.Editor
{
    using ProgressAction = Action<string, float>;
    
    public static class FFmpegInstaller
    {
        
        private const string FFmpegDownloadUrlWindows = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip";
        private const string FFmpegDownloadUrlOSX = "https://evermeet.cx/ffmpeg/getrelease/zip";
        private const string FFprobeDownloadUrlOSX = "https://evermeet.cx/ffmpeg/getrelease/ffprobe/zip";
        
        private static readonly PackageIdUrl FFMpegCorePackage = new() { ID = "com.crizgames.ffmpegcore", VersionOrURL = "https://github.com/CrizGames/FFMpegCoreUnity.git" };
        
        private static readonly bool IsWindows = Application.platform == RuntimePlatform.WindowsEditor;
        private static readonly bool IsOSX = Application.platform == RuntimePlatform.OSXEditor;
        
        public static readonly bool IsSupported = IsWindows || IsOSX;
        
        private static readonly string FFmpegBinName = IsWindows ? "ffmpeg.exe" : "ffmpeg";
        private static string ProjectTempFolder => Path.Combine(Application.dataPath, "../Temp");
        private static string LibraryVourFolder => Path.Combine(Application.dataPath, "../Library/Vour");
        private static string FFmpegInstallFolder => Path.Combine(LibraryVourFolder, "FFmpeg");
        public static string FFmpegBinaryFolder => Path.Combine(FFmpegInstallFolder, IsWindows ? "bin" : "");
        private static string FFmpegBinPath => Path.Combine(FFmpegBinaryFolder, FFmpegBinName);
        private static bool IsFFmpegBinariesInstalled => File.Exists(FFmpegBinPath);
        private static bool IsFFmpegCorePackageInstalled => VourPackageManager.IsPackageInstalled(FFMpegCorePackage);
        public static bool IsFFmpegInstalled => IsFFmpegBinariesInstalled && IsFFmpegCorePackageInstalled;
        public static bool OnlyBinariesMissing => !IsFFmpegBinariesInstalled && IsFFmpegCorePackageInstalled;

        public static void InstallFFmpeg()
        {
            const string progressWindowTitle = "Installing FFmpeg for this project";
            const string successText = "FFmpeg successfully installed!";
            
            DoInstallOrUninstall(InstallBinaries, InstallPackage, progressWindowTitle, successText);
        }

        public static void UninstallFFmpeg()
        {
            const string progressWindowTitle = "Uninstalling FFmpeg for this project";
            const string successText = "FFmpeg successfully uninstalled!";
            
            DoInstallOrUninstall(UninstallBinaries, UninstallPackage, progressWindowTitle, successText);
        }

        private static void DoInstallOrUninstall(Func<ProgressAction, bool> binaryAction, Func<ProgressAction, bool> packageAction, string title, string success)
        {
            if (binaryAction((info, progress) => DisplayProgress(info, progress * 0.5f)) 
                && packageAction((info, progress) => DisplayProgress(info, 0.5f + progress * 0.5f)))
            {
                DisplayProgress(success, 1f);
                Thread.Sleep(1000);
            }
            EditorUtility.ClearProgressBar();
            return;

            void DisplayProgress(string info, float progress)
            {
                EditorUtility.DisplayProgressBar(title, info, progress);
            }
        }
        
        /// <summary>
        /// Install FFMpegCore package.
        /// </summary>
        private static bool InstallPackage(ProgressAction progressCallback)
        {
            if (IsFFmpegCorePackageInstalled)
            {
                progressCallback("FFMpegCore is already installed!", 1f);
                return true;
            }
            
            progressCallback("Installing FFMpegCore...", 0.5f);
            
            var request = VourPackageManager.InstallPackages(FFMpegCorePackage);
            var success = WaitForAddAndRemoveRequest(request);
            
            progressCallback("Installed FFMpegCore", 1f);

            if (success) 
                return true;
            
            Debug.LogError($"Error installing FFMpegCore package: Error code: {request.Error.errorCode}; {request.Error.message}");
            EditorUtility.DisplayDialog("Error", "Error while installing FFMpegCore package. See console for details.", "Ok");
            return false;
        }
        
        /// <summary>
        /// Install FFMpegCore package.
        /// </summary>
        private static bool UninstallPackage(ProgressAction progressCallback)
        {
            if (!IsFFmpegCorePackageInstalled)
            {
                progressCallback("FFMpegCore is not installed.", 1f);
                return true;
            }
            
            progressCallback("Uninstalling FFMpegCore...", 0.5f);
            
            var request = VourPackageManager.UninstallPackages(FFMpegCorePackage);
            var success = WaitForAddAndRemoveRequest(request);
            
            progressCallback("Uninstalled FFMpegCore", 1f);

            if (success)
                return true;
            
            Debug.LogError($"Error uninstalling FFMpegCore package: Error code: {request.Error.errorCode}; {request.Error.message}");
            EditorUtility.DisplayDialog("Error", "Error while uninstalling FFMpegCore package. See console for details.", "Ok");
            return false;
        }

        private static bool WaitForAddAndRemoveRequest(AddAndRemoveRequest request)
        {
            while (!request.IsCompleted && request.Error == null && request.Status != StatusCode.Failure)
                Thread.Sleep(100);

            return request.Error == null;
        }

        /// <summary>
        /// Download FFmpeg to Library folder.
        /// </summary>
        private static bool InstallBinaries(ProgressAction progressCallback)
        {
            if (IsFFmpegBinariesInstalled)
            {
                progressCallback("FFmpeg binaries are already installed!", 1);
                return true;
            }

            try
            {
                progressCallback("Downloading FFmpeg...", 0);
                
                if (IsWindows)
                {
                    var zipPath = Path.Combine(ProjectTempFolder, "ffmpeg.zip");
                
                    if (!DownloadZip(FFmpegDownloadUrlWindows, zipPath, progressCallback))
                        return false;
                    
                    progressCallback("Extracting FFmpeg...", 0.9f);
                    
                    Directory.CreateDirectory(LibraryVourFolder);

                    // Not using Application.temporaryCachePath because Directory.Move() will
                    // throw an exception if the two folders are not on the same volume
                    var tempFolder = Path.Combine(ProjectTempFolder, "ffmpeg");
                    // Extract zip file into temp folder
                    ZipFile.ExtractToDirectory(zipPath, tempFolder, true);
                    
                    progressCallback("Moving files...", 0.95f);

                    // Get inner folder
                    var innerFolder = Directory.GetDirectories(tempFolder).First();
                
                    // Move inner folder contents to target folder
                    Directory.Move(innerFolder, FFmpegInstallFolder);
                    
                    // Cleanup
                    Directory.Delete(tempFolder);
                    File.Delete(zipPath);

                    progressCallback("Installed FFmpeg successfully!", 1f);
                }
                else // OSX
                {
                    var ffmpegZipPath = Path.Combine(ProjectTempFolder, "ffmpeg.zip");
                    var ffprobeZipPath = Path.Combine(ProjectTempFolder, "ffprobe.zip");
                
                    if (!DownloadZip(FFmpegDownloadUrlOSX, ffmpegZipPath, (text, progress) => progressCallback(text, progress * 0.5f)))
                        return false;
                
                    if (!DownloadZip(FFprobeDownloadUrlOSX, ffprobeZipPath, (text, progress) => progressCallback(text, 0.5f + progress * 0.4f)))
                        return false;
                    
                    progressCallback("Extracting FFmpeg...", 0.9f);
                    
                    Directory.CreateDirectory(LibraryVourFolder);

                    // Not using Application.temporaryCachePath because Directory.Move() will
                    // throw an exception if the two folders are not on the same volume
                    var tempFolder = Path.Combine(ProjectTempFolder, "ffmpeg");
                    // Extract zip files into temp folder
                    ZipFile.ExtractToDirectory(ffmpegZipPath, tempFolder, true);
                    ZipFile.ExtractToDirectory(ffprobeZipPath, tempFolder, true);
                    
                    progressCallback("Moving files...", 0.95f);
                    
                    // Move binaries to target folder
                    Directory.Move(tempFolder, FFmpegBinaryFolder);
                    
                    // Set execute permissions
                    foreach (var file in Directory.GetFiles(FFmpegBinaryFolder))
                        SetExecutePermissionOSX(file);
                    
                    // Cleanup
                    File.Delete(ffmpegZipPath);
                    File.Delete(ffprobeZipPath);

                    progressCallback("Installed FFmpeg successfully!", 1f);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Delete folder when there was an error while extracting
                if (Directory.Exists(FFmpegInstallFolder))
                    Directory.Delete(FFmpegInstallFolder, true);
                
                Debug.LogError($"Error extracting files: {ex}");
                EditorUtility.DisplayDialog("Error", "Error while installing FFmpeg. See console for details.", "Ok");
                
                return false;
            }
        }

        private static bool UninstallBinaries(ProgressAction progressCallback)
        {
            if (!IsFFmpegBinariesInstalled)
            {
                progressCallback("FFmpeg is already uninstalled.", 1);
                return true;
            }
            
            if (Directory.Exists(FFmpegInstallFolder))
                Directory.Delete(FFmpegInstallFolder, true);
            
            return true;
        }

        private static bool DownloadZip(string url, string destPath, ProgressAction progressCallback)
        {
            using var request = GetRequestWithHandler(url, new DownloadHandlerFile(destPath));
            request.SendWebRequest();
            
            // Update progress while extracting
            while (!request.isDone)
            {
                progressCallback("Downloading FFmpeg...", request.downloadProgress * 0.9f);
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                progressCallback("Error while downloading FFmpeg binaries. See console for details.", 1f);
                Debug.LogError($"An error occured while downloading FFmpeg: {request.result}, Code: {request.responseCode}, {request.error}");
                
                // Delete downloaded zip file
                if(File.Exists(destPath))
                    File.Delete(destPath);
                
                return false;
            }

            return true;
        }
        
        private static UnityWebRequest GetRequestWithHandler(string url, DownloadHandler downloadHandler)
            => new(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);
        
        private static void SetExecutePermissionOSX(string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{filePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"Failed to set execute permission for {filePath}: {error}");
            }
        }
    }
}