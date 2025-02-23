using Microsoft.Extensions.AI;
using System.Text.Json;
using WinChat.Infrastructure.Services.Gemini.Contracts;

namespace WinChat.Infrastructure.Services.Gemini;

internal static class GeminiMapper
{
    public static TextGenerationRequest ToGeminiMessages(IList<ChatMessage> chatMessages, List<GeminiTool> tools, JsonSerializerOptions toolCallJsonSerializerOptions)
    {
        var systemInstructions = new SystemInstruction();

        var systemCollection = new List<string>();
        var contentCollection = new List<Contents>();

        foreach (var input in chatMessages)
        {
            if (input.Role == ChatRole.System)
            {
                var parts = ToChatContent(input.Contents);
                systemCollection.Add(string.Join(" ", parts));
            }
            else if (input.Role == ChatRole.User)
            {
                var parts = ToChatContent(input.Contents);
                contentCollection.Add(new Contents
                {
                    Role = input.Role.ToString(),
                    Parts = new() { Text = string.Join(" ", parts) }
                });
            }
            else if (input.Role == ChatRole.Tool)
            {
                foreach (AIContent item in input.Contents)
                {
                    if (item is FunctionResultContent resultContent)
                    {
                        string? result = resultContent.Result as string;
                        if (result is null && resultContent.Result is not null)
                        {
                            try
                            {
                                result = JsonSerializer.Serialize(resultContent.Result);
                            }
                            catch (NotSupportedException)
                            {
                                // If the type can't be serialized, skip it.
                            }
                        }
                    }
                }
            }
            else if (input.Role == ChatRole.Assistant)
            {
                var parts = ToChatContent(input.Contents);
                contentCollection.Add(new Contents
                {
                    Role = input.Role.ToString(),
                    Parts = new() { Text = string.Join(" ", parts) }
                });
            }
        }

        return new()
        {
            SystemInstruction = new SystemInstruction { Parts = new() { Text = string.Join(",", systemCollection) } },
            Contents = new Contents
            {
                Parts = new()
                {
                    Text = string.Join(",",
                    contentCollection.Select(x => JsonSerializer.Serialize(x, Constants.DefaultSerializerOptions)))
                }
            },
            Tools = tools
        };
    }

    private static List<string> ToChatContent(IList<AIContent> contents)
    {
        List<string> parts = [];
        foreach (var content in contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    parts.Add(textContent.Text);
                    break;
            }
        }

        if (parts.Count == 0)
        {
            parts.Add(string.Empty);
        }

        return parts;
    }

    public static ChatResponse FromGeminiResponse(TextGenerationResponse geminiResponse, ChatOptions? options, object geminiAIOptions)
    {
        var candidate = geminiResponse.Candidates.FirstOrDefault();
        if (candidate == null) { return new(new ChatMessage()); }

        ChatMessage returnMessage = new()
        {
            RawRepresentation = geminiResponse,
            Role = FromRole(candidate.Content?.Role),
        };

        foreach (var contentPart in candidate.Content?.Parts ?? [])
        {
            if (ToTextContent(contentPart) is AIContent aiContent)
            {
                returnMessage.Contents.Add(aiContent);
            }
        }

        if (options?.Tools is { Count: > 0 })
        {
            foreach (var functionPart in (candidate.Content?.Parts ?? [])
                .Where(x => x.FunctionCall != null))
            {
                var toolCall = functionPart.FunctionCall;
                if (!string.IsNullOrWhiteSpace(toolCall?.Name))
                {
                    var callContent = ParseCallContent(toolCall.Args, toolCall.Name);
                    callContent.RawRepresentation = toolCall;

                    returnMessage.Contents.Add(callContent);
                }
            }
        }

        var response = new ChatResponse([returnMessage])
        {
            CreatedAt = DateTimeOffset.UtcNow,
            FinishReason = ToFinishReason(candidate.FinishReason),
            ModelId = geminiResponse.ModelVersion,
            RawRepresentation = geminiResponse,
            ResponseId = Guid.NewGuid().ToString(),
        };

        return response;
    }

    private static ChatFinishReason? ToFinishReason(string? finishReason) =>
        finishReason switch
        {
            "Stop" => ChatFinishReason.Stop,
            "ToolCalls" => ChatFinishReason.ToolCalls,
            "Length" => ChatFinishReason.Length,
            "ContentFilter" => ChatFinishReason.ContentFilter,
            _ => ChatFinishReason.Stop
        };

    private static TextContent ToTextContent(Part contentPart) => new(contentPart.Text);

    private static ChatRole FromRole(string? role)
    {
        return role switch
        {
            "System" => ChatRole.System,
            "User" => ChatRole.User,
            "Assistant" => ChatRole.Assistant,
            "Tool" => ChatRole.Tool,
            _ => new ChatRole(role?.ToString() ?? string.Empty),
        };
    }

    private static FunctionCallContent ParseCallContent(Dictionary<string, object?> args, string name) =>
        FunctionCallContent.CreateFromParsedArguments<string>(name, name, name, json => args);

    public static List<GeminiTool> ToGeminiOptions(ChatOptions? options)
    {
        var tools = new List<GeminiTool>();

        foreach (var optionTool in options?.Tools ?? [])
        {
            var tool = new GeminiTool
            {
                FunctionDecleration =
                [
                    new GeminiFunctionDecleration
                    {
                        Name = optionTool.Name,
                        Description = optionTool.Description,
                        Parameters = new GeminiParameters
                        {
                            Properties = optionTool.AdditionalProperties.ToDictionary(k => k.Key, v => new InternalParameter
                            {
                               Description = v.Value?.ToString() ?? string.Empty
                            }),
                            Required = [.. optionTool.AdditionalProperties.Keys]
                        }
                    }
                ]
            };
            tools.Add(tool);
        }

        return tools;
    }
}
