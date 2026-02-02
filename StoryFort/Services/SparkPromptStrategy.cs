using System;
using System.Linq;
using System.Threading.Tasks;
using StoryFort.Models;

namespace StoryFort.Services;

public class SparkPromptStrategy : IPromptStrategy
{
    private readonly ICohereTutorService _cohereTutorService;
    private readonly SparkPromptOptions _options;
    private readonly TutorSessionService _tutorSessionService;
    private readonly PromptRepository _promptRepository;
    private readonly IPromptService _promptService;
    public SparkPromptStrategy(ICohereTutorService cohereTutorService, Microsoft.Extensions.Options.IOptionsSnapshot<SparkPromptOptions> options, TutorSessionService tutorSessionService, PromptRepository promptRepository, IPromptService promptService)
    {
        _cohereTutorService = cohereTutorService;
        _options = options.Value;
        _tutorSessionService = tutorSessionService;
        _promptRepository = promptRepository;
        _promptService = promptService;
    }

    public async Task<string> BuildPromptAsync(StoryContext context, string modelName)
    {
        // Step 1: Sensory Question
        if (_tutorSessionService.HistoryCount == 0)
        {
            var prompt = "Ask the user a sensory question to start their story. Only ask a question, do not generate prose.";
            return await SendPromptAsync(prompt, context, modelName);
        }
        // Step 2: Attribute Narrowing
        else if (_tutorSessionService.HistoryCount == 1)
        {
            var prompt = "Based on the user's answer, ask an attribute narrowing question. Only ask a question.";
            return await SendPromptAsync(prompt, context, modelName);
        }
        // Step 3: Scenario Prompt
        else if (_tutorSessionService.HistoryCount == 2)
        {
            var prompt = "Based on previous answers, ask a scenario question to help the user imagine a situation. Only ask a question.";
            return await SendPromptAsync(prompt, context, modelName);
        }
        // Step 4: Output READY_TO_WRITE
        else
        {
            var prompt = $"If the user is ready, output JSON: {_options.ReadyToWriteJson}. Otherwise, ask a final clarifying question.";
            var response = await SendPromptAsync(prompt, context, modelName);
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(response);
                if (json.RootElement.TryGetProperty("status", out var status) && status.GetString() == "READY_TO_WRITE")
                {
                    _tutorSessionService.LastJsonStatus = "READY_TO_WRITE";
                }
            }
            catch { /* Not JSON, ignore */ }
            return response;
        }
    }

    private async Task<string> SendPromptAsync(string prompt, StoryContext context, string modelName)
    {
        var account = context.Account ?? new Account();
        var age = context.Age ?? "Unknown";
        var genre = context.Genre ?? "General";
        var archetype = context.Archetype ?? "Unknown";

        // Prefer a file-based prompt if present in Prompts/spark*.txt
        // Prefer templated prompt via PromptService (supports placeholders like {{Genre}})
        var variables = new Dictionary<string, string?>
        {
            ["Genre"] = genre,
            ["Archetype"] = archetype,
            ["Age"] = age
        };

        var tutorPrompt = _promptService.FormatTemplate("spark", variables);
        if (string.IsNullOrEmpty(tutorPrompt))
        {
            var filePrompt = _promptRepository.GetLatestPrompt("spark");
            var baseSystem = !string.IsNullOrEmpty(filePrompt) ? filePrompt :
                (string.IsNullOrEmpty(_options.SystemPrompt) ?
                    "Role: You are a Socratic tutor supporting the creative writing development of students. Only ask a single leading question; do not generate story prose." :
                    _options.SystemPrompt);

            tutorPrompt = $@"{baseSystem}
            Story_Metadata: Genre: {genre}, Story Archetype: {archetype}, Target Age: {age}
            Input: You will receive a two part <user_prompt>:
            * <User_Story>The story segment that immediately preceded the stuck moment in the students own typing
            * <Previous_Nudge>The last nudge you gave to the student. If null or Null, then discard this data
            Expectations: Output a single Socratic leading question per the above.";
        }

        int nSentences = 4;
        string storyContent = context.Content ?? string.Empty;
        string[] sentences = storyContent.Split(new[] {'.', '!', '?'}, StringSplitOptions.RemoveEmptyEntries);
        string lastSegment = string.Join(". ", sentences.Skip(Math.Max(0, sentences.Length - nSentences)).Select(s => s.Trim())) + (sentences.Length > 0 ? "." : "");
        string previousNudge = _tutorSessionService.HistoryCount > 0 ? _tutorSessionService.Session.History.Last() : "Null";
        var userPrompt = $"<User_Story>{lastSegment}</User_Story>\n<Previous_Nudge>{previousNudge}</Previous_Nudge>";
        var fullPrompt = $"{tutorPrompt}\n\nInstruction: {prompt}\n\nUser_Prompt: {userPrompt}";
        var response = await _cohereTutorService.GetSocraticPromptAsync(fullPrompt, account, account.UseReasoningModel);
        _tutorSessionService.AddHistory(response);
        return response;
    }
}

