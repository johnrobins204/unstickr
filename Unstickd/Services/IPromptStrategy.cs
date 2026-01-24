using System.Threading.Tasks;
using Unstickd.Models;

namespace Unstickd.Services;

public interface IPromptStrategy
{
    Task<string> BuildPromptAsync(StoryState state, string modelName);
}
