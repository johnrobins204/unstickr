using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoryFort.Services
{
    public interface IPromptService
    {
        /// <summary>
        /// Load the raw template for a given key (e.g. "spark").
        /// </summary>
        string? GetTemplate(string key);

        /// <summary>
        /// Format a template by replacing simple placeholders using the provided variables.
        /// Placeholders are expressed as {{Key}} inside the template.
        /// </summary>
        string? FormatTemplate(string key, IDictionary<string, string?> variables);
    }
}
