using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WillRobot.Dialogs
{
    public class ReviewSelectionDialog : ComponentDialog
    {
        private const string DoneOption = "Finalizar";

        private const string CompaniesSelected = "value-companiesSelected";

        private readonly string[] _companyOptions = new string[]
        {
            "Google", "Amazon", "Facebook", "Instagram",
        };

        public ReviewSelectionDialog()
            : base(nameof(ReviewSelectionDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    SelectionStepAsync,
                    LoopStepAsync,
                }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var list = stepContext.Options as List<string> ?? new List<string>();
            stepContext.Values[CompaniesSelected] = list;

            string message;
            if (list.Count is 0)
            {
                message = $"Por favor selecione uma empresa para avalisar, ou `{DoneOption}` para encerrar.";
            }
            else
            {
                message = $"Você selecionou **{list[0]}**. Você pode avaliar empresas adicionais, " +
                    $"ou selecionar `{DoneOption}` para encerrar.";
            }

            var options = _companyOptions.ToList();
            options.Add(DoneOption);
            if (list.Count > 0)
            {
                options.Remove(list[0]);
            }

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(message),
                RetryPrompt = MessageFactory.Text("Por favor, selecione uma opção da lista."),
                Choices = ChoiceFactory.ToChoices(options),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var list = stepContext.Values[CompaniesSelected] as List<string>;
            var choice = (FoundChoice)stepContext.Result;
            var done = choice.Value == DoneOption;

            if (!done)
            {
                list.Add(choice.Value);
            }

            if (done || list.Count >= 2)
            {
                return await stepContext.EndDialogAsync(list, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(ReviewSelectionDialog), list, cancellationToken);
            }
        }
    }
}
