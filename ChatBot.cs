// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Dialogs;
using ChatBot.Dialogs.ArticleDialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace ChatBot
{
    public class EmptyBot : ActivityHandler
    {
        private readonly DialogSet dialogs;
        public EmptyBot(BotAccessors botAccessors)
        {
            var dialogState = botAccessors.DialogStateAccessor;
            // compose dialogs
            dialogs = new DialogSet(dialogState);
            dialogs.Add(MainDialog.Instance);
            dialogs.Add(MainArticleDialog.Instance);
            dialogs.Add(new ChoicePrompt("choicePrompt"));
            dialogs.Add(new TextPrompt("textPrompt"));
            dialogs.Add(new NumberPrompt<int>("numberPrompt"));
            BotAccessors = botAccessors;
        }

        public BotAccessors BotAccessors { get; }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // initialize state if necessary
                var state = await BotAccessors.ChatBotStateStateAccessor.GetAsync(turnContext, () => new ChatBotState(), cancellationToken);

                turnContext.TurnState.Add("BotAccessors", BotAccessors);

                var dialogCtx = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                if (dialogCtx.ActiveDialog == null)
                {
                    await dialogCtx.BeginDialogAsync(MainDialog.Id);
                }
                else
                {
                    await dialogCtx.ContinueDialogAsync(cancellationToken);
                }

                await BotAccessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("Hi there, my name is Mark-1, How can I help you today");
            }
        }
    }
}
