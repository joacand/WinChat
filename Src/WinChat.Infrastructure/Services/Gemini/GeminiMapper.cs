using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.Json;
using WinChat.Infrastructure.Services.Gemini.Contracts;

namespace WinChat.Infrastructure.Services.Gemini
{
    public class GeminiMessage
    {
        public string Role { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
    public class ToolChatMessage : GeminiMessage
    {
        public ToolChatMessage(string callId, string result)
        {
            ToolCallId = callId;
            Text = result;
        }
        public string ToolCallId { get; set; } = string.Empty;
    }
    public class AssistantMessage : GeminiMessage
    {
        public IList<ChatToolCall> ToolCalls { get; } = new List<ChatToolCall>();
    }
    public class ChatToolCall
    {
        public ChatToolCall(string id, string fName, string fArgs)
        {
            FunctionCallId = id;
            FunctionName = fName;
            FunctionArgument = fArgs;
        }
        public string FunctionCallId { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public string FunctionArgument { get; set; } = string.Empty;
    }
    internal static class GeminiMapper
    {
        public static TextGenerationRequest ToGeminiMessages(IList<Microsoft.Extensions.AI.ChatMessage> chatMessages, List<GeminiTool> tools, JsonSerializerOptions toolCallJsonSerializerOptions)
        {
            var systemInstructions = new SystemInstruction();
            //var content = new Contents();

            var systemCollection = new List<string>();
            var contentCollection = new List<Contents>();

            foreach (var input in chatMessages)
            {
                if (input.Role == ChatRole.System)
                {
                    var parts = ToChatContent(input.Contents);
                    //var gMsg = new GeminiMessage
                    //{
                    //    Role = input.Role.ToString(),
                    //    ParticipantName = input.AuthorName,
                    //    Text = string.Join(" ", parts)
                    //};
                    systemCollection.Add(string.Join(" ", parts));
                }
                else if (input.Role == ChatRole.User)
                {
                    var parts = ToChatContent(input.Contents);
                    //var gMsg = new GeminiMessage
                    //{
                    //    Role = input.Role.ToString(),
                    //    ParticipantName = input.AuthorName,
                    //    Text = string.Join(" ", parts)
                    //};
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


                            //   contentCollection.Add(JsonSerializer.Serialize(new ToolChatMessage(resultContent.CallId, result ?? string.Empty)));
                        }
                    }
                }
                else if (input.Role == ChatRole.Assistant)
                {
                    var parts = ToChatContent(input.Contents);
                    //var message = new AssistantMessage
                    //{
                    //    Role = input.Role.ToString(),
                    //    ParticipantName = input.AuthorName,
                    //    Text = string.Join(" ", parts)
                    //};
                    contentCollection.Add(new Contents
                    {
                        Role = input.Role.ToString(),
                        Parts = new() { Text = string.Join(" ", parts) }
                    });

                    foreach (var aicontent in input.Contents)
                    {
                        if (aicontent is FunctionCallContent callRequest)
                        {
                            //message.ToolCalls.Add(
                            //    new ChatToolCall(
                            //        callRequest.CallId,
                            //        callRequest.Name,
                            //       string.Join(',', callRequest.Arguments.Keys)));
                        }
                    }

                    // contentCollection.Add(JsonSerializer.Serialize(message, toolCallJsonSerializerOptions));
                }
            }

            return new TextGenerationRequest
            {
                SystemInstruction = new SystemInstruction { Parts = new() { Text = string.Join(",", systemCollection) } },
                Contents = new Contents { Parts = new() { Text = string.Join(",", contentCollection.Select(x => JsonSerializer.Serialize(x, Constants.DefaultSerializerOptions))) } },
                Tools = GenerateTools(tools)
            };
        }

        private static List<GeminiTool> GenerateTools(List<GeminiTool> tools)
        {
            return tools;
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
            if (candidate == null) { return null; }

            // Create the return message.
            ChatMessage returnMessage = new()
            {
                RawRepresentation = geminiResponse,
                Role = FromRole(candidate.Content?.Role),
            };

            // Populate its content from those in the OpenAI response content.
            foreach (var contentPart in candidate.Content.Parts)
            {
                if (ToAIContent(contentPart) is AIContent aiContent)
                {
                    returnMessage.Contents.Add(aiContent);
                }
            }

            // Also manufacture function calling content items from any tool calls in the response.
            if (options?.Tools is { Count: > 0 })
            {
                foreach (var functionPart in candidate.Content.Parts.Where(x => x.FunctionCall != null))
                {
                    var toolCall = functionPart.FunctionCall;
                    if (!string.IsNullOrWhiteSpace(toolCall.Name))
                    {
                        var callContent = ParseCallContent(toolCall.Args, toolCall.Name);
                        callContent.RawRepresentation = toolCall;

                        returnMessage.Contents.Add(callContent);
                    }
                }
            }

            // Wrap the content in a ChatResponse to return.
            var response = new ChatResponse([returnMessage])
            {
                CreatedAt = DateTimeOffset.UtcNow,
                FinishReason = ToFinishReason(candidate.FinishReason),
                ModelId = geminiResponse.ModelVersion,
                RawRepresentation = geminiResponse,
                ResponseId = Guid.NewGuid().ToString(),
            };

            //if (geminiResponse.Usage is ChatTokenUsage tokenUsage)
            //{
            //    response.Usage = FromOpenAIUsage(tokenUsage);
            //}

            return response;
        }

        private static ChatFinishReason? ToFinishReason(string? finishReason)
        {
            return ChatFinishReason.Stop;
        }

        private static AIContent ToAIContent(Part contentPart)
        {
            return new TextContent(contentPart.Text);
        }

        private static ChatRole FromRole(string? role)
        {
            return role switch
            {
                "System" => ChatRole.System,
                "User" => ChatRole.User,
                "Assistant" => ChatRole.Assistant,
                "Tool" => ChatRole.Tool,
                _ => new ChatRole(role.ToString()),
            };
        }

        private static FunctionCallContent ParseCallContent(Dictionary<string, object?> args, string name)
        {
            return FunctionCallContent.CreateFromParsedArguments<string>(name, name, name, json => args);
        }

        public static List<GeminiTool> ToGeminiOptions(ChatOptions? options)
        {
            var tools = new List<GeminiTool>();

            foreach (var t in options.Tools)
            {
                var tool = new GeminiTool
                {
                    FunctionDecleration =
                    [
                        new GeminiFunctionDecleration
                        {
                            Name = t.Name,
                            Description = t.Description,
                            Parameters = new GeminiParameters
                            {
                                Properties = t.AdditionalProperties.ToDictionary(k => k.Key, v => new InternalParameter
                                {
                                   Description = (string)v.Value
                                }),
                                Required = t.AdditionalProperties.Keys.ToList()
                            }
                        }
                    ]
                };
                tools.Add(tool);
            }

            return tools;
        }
    }
}
