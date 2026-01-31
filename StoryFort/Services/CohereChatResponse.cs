using System.Collections.Generic;

namespace StoryFort.Services;

public class CohereChatResponse
{
    public CohereMessage? Message { get; set; }
}

public class CohereMessage
{
    public List<CohereContentItem>? Content { get; set; }
}

public class CohereContentItem
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

public class CohereGenerateResponse
{
    public List<CohereGeneration>? Generations { get; set; }
}

public class CohereGeneration
{
    public string? Text { get; set; }
}
