using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PalmHilsSemanticKernelBot.BusinessLogic.Bot
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        private readonly ConversationState ConversationState;
        private readonly ILogger<AdapterWithErrorHandler> Logger;

       
        public AdapterWithErrorHandler(
            BotFrameworkAuthentication auth,
            ConversationState conversationState,
            ILogger<AdapterWithErrorHandler> logger)
            : base(auth, logger)
        {
            // Add null checks for dependencies to ensure the service starts correctly.
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            OnTurnError = async (turnContext, exception) =>
            {
                
                Logger.LogError(exception, "[OnTurnError] Unhandled exception caught: {ExceptionMessage}", exception.Message);

                
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.ToString(), "https://www.botframework.com/schemas/error", "TurnError");

                string userMessage;

                
                if (exception.Message.Contains("ContentTokenLogProbabilities"))
                {
                    userMessage = "I seem to have run into a technical issue with that request. Let's try that again. What would you like to do?";
                    // Don't clear state for serialization errors - just continue with the conversation
                    await turnContext.SendActivityAsync(MessageFactory.Text(userMessage));
                    return; // Exit early without clearing state
                }
                else
                {
                    
                    userMessage = "I'm sorry, the bot encountered an unexpected error and needs to restart the conversation.";
                }

                await turnContext.SendActivityAsync(MessageFactory.Text(userMessage));

                try
                {
                    // Only clear state for non-serialization errors
                    await ConversationState.ClearStateAsync(turnContext, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to clear conversation state in OnTurnError: {ExceptionMessage}", ex.Message);
                }
            };
        }
    }
}