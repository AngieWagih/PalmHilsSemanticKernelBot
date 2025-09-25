using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace PalmHilsSemanticKernelBot.BusinessLogic.Bot
{
    public class SemanticKernelWithBot: ActivityHandler
    {

        private readonly ConversationState _conversationState;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public SemanticKernelWithBot(ConversationState conversationState, Kernel kernel, IChatCompletionService chatService)
        {
            _conversationState = conversationState;
            _kernel = kernel;
            _chatService = chatService;
        }


        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userMessage = turnContext.Activity.Text;

            try
            {
                var systemPrompt = @"You are a helpful assistant that can ONLY answer questions using the available functions/plugins. 

                IMPORTANT RULES:
                1. You can ONLY use the functions that are available to you
                2. If a user asks something that cannot be answered with your available functions, politely explain that you can only help with specific capabilities
                3. Do NOT provide answers based on your general knowledge if there's no appropriate function
                4. Always try to use the most appropriate function for the user's request
                5. If no function matches the request, list what you CAN help with

                Available capabilities:
                - Mathematical calculations and operations
                - WSeather conditions and forecasts for specific locations

                If you cannot handle a request with your available functions, respond with: 'I can only help with [list your available capabilities]. Please ask me something within these areas.'";


                // Create chat history for context
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(userMessage);

                // Get response from Semantic Kernel with function calling
                var response = await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    },
                    kernel: _kernel,
                    cancellationToken: cancellationToken);

                // Send response back through Bot Framework
                await turnContext.SendActivityAsync(MessageFactory.Text(response.Content), cancellationToken);

            }
            catch (Exception ex)
            {

                await turnContext.SendActivityAsync(MessageFactory.Text($"Sorry, I encountered an error: {ex.Message}"), cancellationToken);
            }

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello! I'm an intelligent assistant powered by Semantic Kernel. I can help you with weather information and math calculations. Try asking me something like 'What's the weather in Seattle?' or 'What's 15 * 23?'";

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
