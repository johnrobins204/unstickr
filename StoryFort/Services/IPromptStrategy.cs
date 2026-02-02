using System.Threading.Tasks;
using StoryFort.Models;

namespace StoryFort.Services;

public interface IPromptStrategy
{
    Task<string> BuildPromptAsync(StoryContext context, string modelName);
}

