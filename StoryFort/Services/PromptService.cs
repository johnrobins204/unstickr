using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StoryFort.Services
{
    public class PromptService : IPromptService
    {
        private readonly PromptRepository _repo;

        public PromptService(PromptRepository repo)
        {
            _repo = repo;
        }

        public string? GetTemplate(string key)
        {
            return _repo.GetLatestPrompt(key);
        }

        public string? FormatTemplate(string key, IDictionary<string, string?> variables)
        {
            var template = GetTemplate(key);
            if (string.IsNullOrEmpty(template)) return null;

            // Simple placeholder replacement: {{Key}}
            foreach (var kv in variables)
            {
                var token = $"{{{{{kv.Key}}}}}"; // {{Key}}
                var value = kv.Value ?? string.Empty;
                template = template.Replace(token, value, StringComparison.OrdinalIgnoreCase);
            }

            // Remove any unreplaced placeholders to avoid leaking templates
            template = Regex.Replace(template, @"{{\s*[^}]+\s*}}", string.Empty);
            return template;
        }
    }
}
