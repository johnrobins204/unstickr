using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace StoryFort.Services
{
    /// <summary>
    /// Loads prompt templates from the `Prompts/` folder in the application content root.
    /// Naming conventions supported:
    /// - Prompts/spark.txt (single file)
    /// - Prompts/spark_v1.txt, spark_v2.txt (versioned) -> latest selected
    /// </summary>
    public class PromptRepository
    {
        private readonly string _promptsPath;

        public PromptRepository(IHostEnvironment env)
        {
            _promptsPath = Path.Combine(env.ContentRootPath ?? string.Empty, "Prompts");
        }

        public string? GetLatestPrompt(string key)
        {
            try
            {
                if (!Directory.Exists(_promptsPath)) return null;

                // Look for versioned files like key_v1.txt
                var files = Directory.GetFiles(_promptsPath, $"{key}*_v*.txt")
                    .Concat(Directory.GetFiles(_promptsPath, $"{key}.txt"))
                    .ToList();

                if (!files.Any()) return null;

                // Prefer versioned files by picking highest numeric suffix if present
                var versioned = files
                    .Select(f => new { Path = f, Name = Path.GetFileName(f) })
                    .Where(x => x.Name.Contains("_v"))
                    .Select(x => new { x.Path, Version = ParseVersion(x.Name) })
                    .Where(x => x.Version.HasValue)
                    .OrderByDescending(x => x.Version.Value)
                    .ToList();

                string selected = versioned.Any() ? versioned.First().Path : files.First();
                return File.ReadAllText(selected);
            }
            catch
            {
                return null;
            }
        }

        private int? ParseVersion(string fileName)
        {
            var idx = fileName.LastIndexOf("_v", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            var dot = fileName.IndexOf('.', idx);
            var substr = dot > idx ? fileName.Substring(idx + 2, dot - (idx + 2)) : fileName.Substring(idx + 2);
            if (int.TryParse(substr, out var v)) return v;
            return null;
        }
    }
}
