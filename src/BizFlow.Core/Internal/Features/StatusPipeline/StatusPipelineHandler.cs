using BizFlow.Core.Contracts;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.StatusPipeline
{
    public class StatusPipelineHandler : IStatusPipelineHandler
    {
        private readonly IPipelineService _pipelineService;
        private readonly IBizFlowJournal _bizFlowJournal;

        private static readonly HashSet<TypeBizFlowJournaAction> _startStatuses = new()
        {
            TypeBizFlowJournaAction.Start
        };

        private static readonly HashSet<TypeBizFlowJournaAction> _finishedStatuses = new()
        {
            TypeBizFlowJournaAction.Success,
            TypeBizFlowJournaAction.Error
        };

        private static readonly HashSet<TypeBizFlowJournaAction> _successStatuses = new()
        {
            TypeBizFlowJournaAction.Success
        };

        public StatusPipelineHandler(IPipelineService pipelineService, IBizFlowJournal bizFlowJournal)
        {
            _pipelineService = pipelineService;
            _bizFlowJournal = bizFlowJournal;
        }
        public async Task<StatusPipelineResult> StatusPipeline(StatusPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentException.ThrowIfNullOrWhiteSpace(command.Name);

            var pipeLine = await _pipelineService.GetPipelineAsync(command.Name, cancellationToken);
            if (pipeLine == null)
                throw new InvalidOperationException($"Пайплайн '{command.Name}' не найден"); // TODO:i18n

            var pipelineItems = pipeLine.PipelineItems
                .OrderBy(i => i.SortOrder)
                .ToList();

            IEnumerable<BizFlowJournalRecord> journalRecords = [];
            var launchId = await _bizFlowJournal.GetLastLaunchId(command.Name);
            if (!string.IsNullOrEmpty(launchId))
            {
                journalRecords = await _bizFlowJournal.GetJournalRecordByLaunchId(launchId);
            }
            journalRecords ??= [];
            
            var result = new StatusPipelineResult()
            {
                PipelineName = pipeLine.Name,
                Description = pipeLine.Description,
                CronExpression = pipeLine.CronExpression,
            };

            var firstAction = journalRecords.FirstOrDefault();
            if (firstAction != null)
            {
                result.IsStartNowPipeline = firstAction.IsStartNowPipeline;
            }

            foreach (var item in pipelineItems)
            {
                var itemActions = journalRecords.Where(i => i.ItemId == item.Id).ToList();
                var startAction = itemActions.Where(action => _startStatuses.Contains(action.TypeAction)).FirstOrDefault();
                var finishedAction = itemActions.Where(action => _finishedStatuses.Contains(action.TypeAction)).FirstOrDefault();
                var lastAction = itemActions.OrderByDescending(action => action.Period).FirstOrDefault();

                var existStartStatus = startAction != null;
                var existFinishedStatus = finishedAction != null;
                var isSuccessFinishedStatus = finishedAction != null && _successStatuses.Contains(finishedAction.TypeAction);

                var statusItem = new StatusPipelineResultItem
                {
                    PipelineItemDescription = item.Description,
                    SortOrder = item.SortOrder,
                    TypeOperationId = item.TypeOperationId,
                    Started = startAction?.Period,
                    Finished = finishedAction?.Period,
                    LastOperationAction = lastAction?.TypeAction.ToString(),
                    Complete = existStartStatus && existFinishedStatus,
                    Success = existStartStatus && isSuccessFinishedStatus,
                    Messages = itemActions.OrderBy(action => action.Period)
                        .Select(action => $"{action.TypeAction}: {action.Message}")
                        .ToList(),
                };

                result.Items.Add(statusItem);
            }

            if (result.Items.Count == pipelineItems.Count && result.Items.All(i => i.Complete))
            {
                result.Complete = true;
            }

            if (result.Items.Count == pipelineItems.Count && result.Items.All(i => i.Success))
            {
                result.Success = true;
            }
            return result;
        }
    }
}
