using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WillRobot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;

        public MainDialog(ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ChoiceCardStepAsync,
                ShowCardStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChoiceCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ChoiceCardStepAsync");

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Selecione uma opção do Menu:"),
                RetryPrompt = MessageFactory.Text("Opção informada invalida, por favor informe uma opção invalida."),
                Choices = GetChoices(),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private IList<Choice> GetChoices()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Adaptive Card", Synonyms = new List<string>() { "adaptive" } },
                new Choice() { Value = "Animation Card", Synonyms = new List<string>() { "animation" } },
                new Choice() { Value = "Audio Card", Synonyms = new List<string>() { "audio" } },
                new Choice() { Value = "Hero Card", Synonyms = new List<string>() { "hero" } },
                new Choice() { Value = "OAuth Card", Synonyms = new List<string>() { "oauth" } },
                new Choice() { Value = "Receipt Card", Synonyms = new List<string>() { "receipt" } },
                new Choice() { Value = "Signin Card", Synonyms = new List<string>() { "signin" } },
                new Choice() { Value = "Thumbnail Card", Synonyms = new List<string>() { "thumbnail", "thumb" } },
                new Choice() { Value = "Video Card", Synonyms = new List<string>() { "video" } },
                new Choice() { Value = "All cards", Synonyms = new List<string>() { "all" } },
            };

            return cardOptions;
        }

        private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ShowCardStepAsync");

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Decide which type of card(s) we are going to show the user
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Adaptive Card":
                    // Display an Adaptive Card
                    reply.Attachments.Add(Cards.CreateAdaptiveCardAttachment());
                    break;
                case "Animation Card":
                    // Display an AnimationCard.
                    reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                    break;
                case "Audio Card":
                    // Display an AudioCard
                    reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                    break;
                case "Hero Card":
                    // Display a HeroCard.
                    reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                    break;
                case "OAuth Card":
                    // Display an OAuthCard
                    reply.Attachments.Add(Cards.GetOAuthCard().ToAttachment());
                    break;
                case "Receipt Card":
                    // Display a ReceiptCard.
                    reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                    break;
                case "Signin Card":
                    // Display a SignInCard.
                    reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                    break;
                case "Thumbnail Card":
                    // Display a ThumbnailCard.
                    reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                    break;
                case "Video Card":
                    // Display a VideoCard
                    reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                    break;
                default:
                    // Display a carousel of all the rich card types.
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments.Add(Cards.CreateAdaptiveCardAttachment());
                    reply.Attachments.Add(Cards.GetAnimationCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetAudioCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetHeroCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetOAuthCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetReceiptCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetSigninCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetThumbnailCard().ToAttachment());
                    reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
                    break;
            }

            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            // Give the user instructions about what to do next
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Digite algo para continuar e selecionar outra opção do Menu."), cancellationToken);

            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You are now logged in."), cancellationToken);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to view your token?") }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhaseOneAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);

            var result = (bool)stepContext.Result;

            if (result)
            {
                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayTokenPhaseTwoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;

            if (tokenResponse != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is your token {tokenResponse.Token}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
