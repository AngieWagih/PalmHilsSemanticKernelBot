using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;

namespace PalmHilsSemanticKernelBot.BusinessLogic.Bot
{
    public class SemanticKernelWithBot : ActivityHandler
    {
        private readonly ConversationState ConversationState;
        private readonly Kernel Kernel;
        private readonly IChatCompletionService ChatService;
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment WebHostEnvironment;
        private readonly IStatePropertyAccessor<ChatHistory> ChatHistoryAccessor;
        private readonly ILogger<SemanticKernelWithBot> _logger;

        // Backup storage for chat history
        private static readonly Dictionary<string, ChatHistory> _chatHistoryBackup = new();

        public SemanticKernelWithBot(
            ConversationState conversationState,
            Kernel kernel,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            ILogger<SemanticKernelWithBot> logger)
        {
            ConversationState = conversationState;
            Kernel = kernel;
            ChatService = Kernel.GetRequiredService<IChatCompletionService>();
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            ChatHistoryAccessor = ConversationState.CreateProperty<ChatHistory>("ChatHistory");
            _logger = logger;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userMessage = turnContext.Activity.Text;
            var conversationId = turnContext.Activity.Conversation.Id;

            try
            {
                _logger.LogInformation("=== OnMessageActivityAsync started ===");
                _logger.LogInformation($"User message: {userMessage}");
                _logger.LogInformation($"Conversation ID: {conversationId}");

                var basePath = WebHostEnvironment.WebRootPath;
                var schemaPath = Configuration.GetValue<string>("SystemPromptsPaths:SystemPrompt");
                var systemPrompt = System.IO.File.ReadAllText(basePath + schemaPath);

                ChatHistory chatHistory = await GetChatHistoryWithFallback(turnContext, conversationId, cancellationToken);

                // Only add system message if this is the first message
                if (chatHistory.Count == 0)
                {
                    chatHistory.AddSystemMessage(systemPrompt);
                    _logger.LogInformation("Added system message to new chat history");
                }

                // Add the current user message
                chatHistory.AddUserMessage(userMessage);
                _logger.LogInformation($"Added user message. Chat history now has {chatHistory.Count} messages");

                // Save state BEFORE calling Semantic Kernel
                await SaveChatHistoryWithBackup(turnContext, conversationId, chatHistory, cancellationToken);

                // Get response from Semantic Kernel with function calling
                _logger.LogInformation("Calling Semantic Kernel...");
                var response = await ChatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    },
                    kernel: Kernel,
                    cancellationToken: cancellationToken);

                _logger.LogInformation($"Got response from SK: {response.Content}");

                // Add the assistant's response to the chat history
                chatHistory.AddAssistantMessage(response.Content);
                _logger.LogInformation($"Added assistant message. Chat history now has {chatHistory.Count} messages");

                // Save the updated conversation state
                await SaveChatHistoryWithBackup(turnContext, conversationId, chatHistory, cancellationToken);

                // Send response back through Bot Framework
                await turnContext.SendActivityAsync(MessageFactory.Text(response.Content), cancellationToken);

                _logger.LogInformation("=== OnMessageActivityAsync completed successfully ===");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnMessageActivityAsync: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                // Try to recover chat history from backup
                await RecoverChatHistoryFromBackup(turnContext, conversationId, cancellationToken);

                // Send a user-friendly error message
                await turnContext.SendActivityAsync(MessageFactory.Text("I encountered an issue processing your request, but I've preserved our conversation history. Let's try again. What can I help you with?"));
            }
        }

        private async Task<ChatHistory> GetChatHistoryWithFallback(ITurnContext turnContext, string conversationId, CancellationToken cancellationToken)
        {
            ChatHistory chatHistory;

            try
            {
                // Try to get existing chat history from state
                _logger.LogInformation("Attempting to retrieve chat history from state...");
                chatHistory = await ChatHistoryAccessor.GetAsync(turnContext, () => new ChatHistory(), cancellationToken);
                _logger.LogInformation($"Successfully retrieved chat history with {chatHistory.Count} messages");

                // Update backup with successful retrieval
                _chatHistoryBackup[conversationId] = CloneChatHistory(chatHistory);

                return chatHistory;
            }
            catch (Exception ex) when (ex is JsonSerializationException ||
                                       ex.InnerException is JsonSerializationException ||
                                       ex.Message.Contains("ContentTokenLogProbabilities"))
            {
                _logger.LogWarning($"Serialization error in conversation state: {ex.Message}");

                // Try to recover from backup first
                if (_chatHistoryBackup.ContainsKey(conversationId))
                {
                    _logger.LogInformation("Recovering chat history from backup...");
                    chatHistory = CloneChatHistory(_chatHistoryBackup[conversationId]);

                    // Clear the corrupted state and save the recovered history
                    try
                    {
                        await ConversationState.ClearStateAsync(turnContext, cancellationToken);
                        await ChatHistoryAccessor.SetAsync(turnContext, chatHistory, cancellationToken);
                        await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                        _logger.LogInformation("Successfully recovered and saved chat history from backup");
                        return chatHistory;
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError($"Failed to save recovered chat history: {saveEx.Message}");
                    }
                }

                // If backup recovery fails, clear state and start fresh
                _logger.LogInformation("Starting with fresh chat history");
                await ConversationState.ClearStateAsync(turnContext, cancellationToken);
                chatHistory = new ChatHistory();
                return chatHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error retrieving chat history: {ex.Message}");

                // Try backup recovery as last resort
                if (_chatHistoryBackup.ContainsKey(conversationId))
                {
                    _logger.LogInformation("Attempting backup recovery for unexpected error...");
                    return CloneChatHistory(_chatHistoryBackup[conversationId]);
                }

                return new ChatHistory();
            }
        }

        private async Task SaveChatHistoryWithBackup(ITurnContext turnContext, string conversationId, ChatHistory chatHistory, CancellationToken cancellationToken)
        {
            try
            {
                // Create backup before saving
                _chatHistoryBackup[conversationId] = CloneChatHistory(chatHistory);
                _logger.LogInformation($"Created backup of chat history with {chatHistory.Count} messages");

                // Test serialization first
                var testSerialization = JsonConvert.SerializeObject(chatHistory);
                _logger.LogInformation("Chat history serialization test passed");

                // Save to state
                await ChatHistoryAccessor.SetAsync(turnContext, chatHistory, cancellationToken);
                await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                _logger.LogInformation("Successfully saved chat history to state");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving chat history: {ex.Message}");

                // Try to save a clean version
                try
                {
                    var cleanHistory = CreateCleanChatHistory(chatHistory);
                    await ChatHistoryAccessor.SetAsync(turnContext, cleanHistory, cancellationToken);
                    await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                    _logger.LogInformation("Successfully saved clean chat history");

                    // Update backup with clean version
                    _chatHistoryBackup[conversationId] = cleanHistory;
                }
                catch (Exception cleanEx)
                {
                    _logger.LogError($"Failed to save even clean chat history: {cleanEx.Message}");
                    // Don't throw - we have backup
                }
            }
        }

        private async Task RecoverChatHistoryFromBackup(ITurnContext turnContext, string conversationId, CancellationToken cancellationToken)
        {
            try
            {
                if (_chatHistoryBackup.ContainsKey(conversationId))
                {
                    _logger.LogInformation("Attempting to recover chat history from backup after error...");
                    var backupHistory = _chatHistoryBackup[conversationId];

                    await ConversationState.ClearStateAsync(turnContext, cancellationToken);
                    await ChatHistoryAccessor.SetAsync(turnContext, backupHistory, cancellationToken);
                    await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                    _logger.LogInformation("Successfully recovered chat history from backup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to recover from backup: {ex.Message}");
            }
        }

        private ChatHistory CloneChatHistory(ChatHistory original)
        {
            var clone = new ChatHistory();
            foreach (var message in original)
            {
                // Only copy essential properties to avoid serialization issues
                switch (message.Role.Label.ToLower())
                {
                    case "system":
                        clone.AddSystemMessage(message.Content ?? string.Empty);
                        break;
                    case "user":
                        clone.AddUserMessage(message.Content ?? string.Empty);
                        break;
                    case "assistant":
                        clone.AddAssistantMessage(message.Content ?? string.Empty);
                        break;
                    default:
                        clone.AddMessage(message.Role, message.Content ?? string.Empty);
                        break;
                }
            }
            return clone;
        }

        private ChatHistory CreateCleanChatHistory(ChatHistory original)
        {
            var clean = new ChatHistory();
            foreach (var message in original)
            {
                try
                {
                    // Create new message with only content to avoid serialization issues
                    clean.AddMessage(message.Role, message.Content ?? string.Empty);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Skipping message due to error: {ex.Message}");
                    // Skip problematic messages
                }
            }
            return clean;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var basePath = WebHostEnvironment.WebRootPath;
            var schemaPath = Configuration.GetValue<string>("SystemPromptsPaths:WelcomePrompt");
            var welcomeText = System.IO.File.ReadAllText(basePath + schemaPath);

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText), cancellationToken);
                }
            }
        }
    }
}