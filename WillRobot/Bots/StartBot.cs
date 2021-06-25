using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WillRobot.Dialogs;

namespace WillRobot.Bots
{
    public class StartBot : DialogBot<MainDialog>
    {
        public StartBot(ConversationState conversationState, UserState userState, MainDialog dialog, ILogger<DialogBot<MainDialog>> logger)
            : base(conversationState, userState, dialog, logger)
        { }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to CardBot."
                        + " This bot will show you different types of Rich Cards."
                        + " Please type anything to get started.");

                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }
    }
}
