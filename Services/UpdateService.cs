using System.Diagnostics;
using System.Security.Cryptography;

namespace DisplayedAppSwitcher.Services;

public class UpdateProgress {
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
}

public static class UpdateService {
    private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "DisplayedAppSwitcher_Update");

    public static async Task<(string installerPath, bool success, string errorMessage)> DownloadAndVerifyInstallerAsync(
        string version,
        IProgress<UpdateProgress>? progress,
        CancellationToken cancellationToken) {

        try {
            Directory.CreateDirectory(TempDir);

            // Strip leading 'v' if present for filename, keep for tag
            var versionClean = version.TrimStart('v');
            var tag = version.StartsWith('v') ? version : $"v{version}";
            var installerFileName = $"DisplayedAppSwitcher_{versionClean}_Setup.exe";
            var checksumFileName = $"{installerFileName}.sha256";

            var checksumUrl = $"https://github.com/viaart/DisplayedAppSwitcher/releases/download/{tag}/{checksumFileName}";
            var installerUrl = $"https://github.com/viaart/DisplayedAppSwitcher/releases/download/{tag}/{installerFileName}";

            var installerPath = Path.Combine(TempDir, installerFileName);
            var checksumPath = Path.Combine(TempDir, checksumFileName);

            // Download checksum file
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("DisplayedAppSwitcher-Updater");

            string checksumContent;
            try {
                checksumContent = await client.GetStringAsync(checksumUrl, cancellationToken);
            } catch (HttpRequestException) {
                return ("", false, "Failed to download checksum file. This release may not support auto-update.");
            }

            // Parse expected hash — format: "<hash>  <filename>"
            var expectedHash = checksumContent.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            if (expectedHash.Length != 64) {
                return ("", false, "Invalid checksum file format.");
            }

            // Download installer with progress
            using var response = await client.GetAsync(installerUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            long bytesDownloaded = 0;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0) {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                bytesDownloaded += bytesRead;
                progress?.Report(new UpdateProgress {
                    BytesDownloaded = bytesDownloaded,
                    TotalBytes = totalBytes
                });
            }

            fileStream.Close();

            // Verify SHA256
            var actualHash = await ComputeSha256Async(installerPath, cancellationToken);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase)) {
                try { File.Delete(installerPath); } catch { }
                return ("", false, $"Checksum verification failed.\nExpected: {expectedHash}\nActual: {actualHash}");
            }

            return (installerPath, true, "");
        } catch (OperationCanceledException) {
            return ("", false, "Download was cancelled.");
        } catch (Exception ex) {
            return ("", false, $"Download failed: {ex.Message}");
        }
    }

    public static void LaunchUpdaterScript(string installerPath) {
        var appExePath = Application.ExecutablePath;
        var scriptPath = Path.Combine(TempDir, "update_DisplayedAppSwitcher.cmd");

        var script = $"""
            @echo off
            setlocal
            set RETRY=0
            :waitloop
            tasklist /FI "IMAGENAME eq DisplayedAppSwitcher.exe" 2>NUL | find /I "DisplayedAppSwitcher.exe" >NUL
            if errorlevel 1 goto install
            set /A RETRY+=1
            if %RETRY% GEQ 10 goto install
            timeout /t 1 /nobreak >NUL
            goto waitloop

            :install
            timeout /t 2 /nobreak >NUL
            "{installerPath}" /SILENT /NORESTART
            if errorlevel 1 goto cleanup

            :relaunch
            start "" "{appExePath}"

            :cleanup
            timeout /t 2 /nobreak >NUL
            del /q "{installerPath}" 2>NUL
            del /q "{TempDir}\*.sha256" 2>NUL
            (goto) 2>nul & del /q "%~f0" & rmdir /q "{TempDir}" 2>NUL
            """;

        File.WriteAllText(scriptPath, script);

        Process.Start(new ProcessStartInfo {
            FileName = "cmd.exe",
            Arguments = $"/c \"{scriptPath}\"",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken) {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
