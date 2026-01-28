using System.Threading.Tasks;
using StoryFort.Models;

namespace StoryFort.Services;

public interface IPromptStrategy
{
    Task<string> BuildPromptAsync(StoryState state, string modelName);
}

