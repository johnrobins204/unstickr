using System.Diagnostics;
using System.Threading.Tasks;

namespace StoryFort.Ingest.Services
{
    /// <summary>
    /// Implementation of IGutenbergMirrorService using rsync (audit compliant)
    /// </summary>
    public class GutenbergMirrorService : IGutenbergMirrorService
    {
        public async Task<string> SyncFromMirrorAsync(int gutenbergId, string outputPath)
        {
            // Example rsync command for Project Gutenberg mirror
            string mirror = "aleph.gutenberg.org::gutenberg";
            string bookPath = $"/ebooks/{gutenbergId}";
            string args = $"-av --include='*/' --include='*.epub' --exclude='*' {mirror}{bookPath} {outputPath}";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "rsync",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new System.Exception($"Rsync failed: {error}");
            }

            return outputPath;
        }

        public int? ExtractGutenbergId(string url)
        {
            // Example: https://www.gutenberg.org/ebooks/11
            var parts = url.Split('/');
            if (parts.Length > 0 && int.TryParse(parts[^1], out int id))
                return id;
            return null;
        }
    }
}