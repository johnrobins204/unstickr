using System;
using System.Linq;
using System.Threading.Tasks;
using Unstickd.Models;

namespace Unstickd.Services;

public class SparkPromptStrategy : IPromptStrategy
{
    private readonly ICohereTutorService _cohereTutorService;
    public SparkPromptStrategy(ICohereTutorService cohereTutorService)
    {
        _cohereTutorService = cohereTutorService;
    }

    public async Task<string> BuildPromptAsync(StoryState state, string modelName)
    {
        var session = state.TutorSession;
        // Step 1: Sensory Question
        if (session.History.Count == 0)
        {
            var prompt = "Ask the user a sensory question to start their story. Only ask a question, do not generate prose.";
            return await SendPromptAsync(prompt, state, modelName);
        }
        // Step 2: Attribute Narrowing
        else if (session.History.Count == 1)
        {
            var prompt = "Based on the user's answer, ask an attribute narrowing question. Only ask a question.";
            return await SendPromptAsync(prompt, state, modelName);
        }
        // Step 3: Scenario Prompt
        else if (session.History.Count == 2)
        {
            var prompt = "Based on previous answers, ask a scenario question to help the user imagine a situation. Only ask a question.";
            return await SendPromptAsync(prompt, state, modelName);
        }
        // Step 4: Output READY_TO_WRITE
        else
        {
            var prompt = "If the user is ready, output JSON: {\"status\": \"READY_TO_WRITE\"}. Otherwise, ask a final clarifying question.";
            var response = await SendPromptAsync(prompt, state, modelName);
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(response);
                if (json.RootElement.TryGetProperty("status", out var status) && status.GetString() == "READY_TO_WRITE")
                {
                    session.LastJsonStatus = "READY_TO_WRITE";
                }
            }
            catch { /* Not JSON, ignore */ }
            return response;
        }
    }

    private async Task<string> SendPromptAsync(string prompt, StoryState state, string modelName)
    {
        var account = state.Account ?? new Account();
        var age = state.Age ?? "Unknown";
        var genre = state.Genre ?? "General";
        var archetype = state.Archetype ?? "Unknown";

        var tutorPrompt = $@"Role: You are a Socratic tutor supporting the creative writing development of students in Manitoba using official curriculum as a guide. When you receive a <user_prompt> it means a student has become stuck in a creative writing assignment and needs a nudge to become unstuck. You will use available Public Domain literature as a template to help unstick this student through a leading Socratic question.
            You are expert in the following categories: <Story_Metadata> Genre: {genre}, Story Archetype: {archetype}, and Target Age: {age} </story_metadata>
            Input: You will receive a two part <user_prompt>:
            * <User_Story>The story segment that immediately preceded the stuck moment in the students own typing, possibly containing spelling errors of phonetic spelling
            * <Previous_Nudge>The last nudge you gave to the student. If null or Null, then discard this data
            Steps:
            1. You will read and understand the <student_story> in relation to <story_metadata>.
            2. You will select an exemplar public literature story <exemplar> that is in the same class as the student story based on <story_metadata>
            3. You will locate a section of <exemplar> that matches the approximate structure of the student's story.
            4. You will prepare an extract that is of similar length and pattern as <student_story> from <exemplar> to create <public_story> which is a story segment similar to the students, but extending one sentence beyond the student's stuck point.
            5. You will formulate one leading Socratic question to prompt the student to continue writing, using the public literature as an example for the student to emulate.
            Expectations:
            * Output a single Socratic leading question per your Steps.
            * Output <public_story> for my debugging purposes.
            * Output the name of the public domain literature that you assessed classified with the student story";

        int nSentences = 4;
        string storyContent = state.Content ?? string.Empty;
        string[] sentences = storyContent.Split(new[] {'.', '!', '?'}, StringSplitOptions.RemoveEmptyEntries);
        string lastSegment = string.Join(". ", sentences.Skip(Math.Max(0, sentences.Length - nSentences)).Select(s => s.Trim())) + (sentences.Length > 0 ? "." : "");
        string previousNudge = state.TutorSession.History.Count > 0 ? state.TutorSession.History.Last() : "Null";
        var userPrompt = $"<User_Story>{lastSegment}</User_Story>\n<Previous_Nudge>{previousNudge}</Previous_Nudge>";
        var fullPrompt = $"{tutorPrompt}\nUser_Prompt: {userPrompt}";
        var response = await _cohereTutorService.GetSocraticPromptAsync(fullPrompt, account, account.UseReasoningModel);
        state.TutorSession.History.Add(response);
        return response;
    }
}
