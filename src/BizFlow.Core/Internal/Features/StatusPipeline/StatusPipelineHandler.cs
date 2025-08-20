using BizFlow.Core.Contracts;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.StatusPipeline
{
    public class StatusPipelineHandler : IStatusPipelineHandler
    {
        private readonly IPipelineService _pipelineService;
        private readonly IBizFlowJournal _bizFlowJournal;

        public StatusPipelineHandler(IPipelineService pipelineService, IBizFlowJournal bizFlowJournal)
        {
            _pipelineService = pipelineService;
            _bizFlowJournal = bizFlowJournal;
        }


        public async Task<StatusPipelineResult> StatusPipeline(StatusPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var startStatuses = new List<TypeBizFlowJournaAction>()
            {
                TypeBizFlowJournaAction.Start,
            };
            var finishedStatuses = new List<TypeBizFlowJournaAction>()
            {
                TypeBizFlowJournaAction.Success,
                TypeBizFlowJournaAction.Error,
            };
            var successStatuses = new List<TypeBizFlowJournaAction>()
            {
                TypeBizFlowJournaAction.Success,
            };

            // TODO: command.Name - проверить на заполненность
            var pipeLine = await _pipelineService.GetPipelineAsync(command.Name, cancellationToken);
            //  TODO:  pipeLine - проверить на null

            var pipelineItems = pipeLine.PipelineItems
                .OrderBy(i => i.SortOrder)
                .ToList();
           
            var launchId = await _bizFlowJournal.GetLastLaunchId(command.Name);
            // TODO: launchId - проверить на заполненность

            var journalRecords = await _bizFlowJournal.GetJournalRecordByLaunchId(launchId);

            var result = new StatusPipelineResult();
            result.PipelineName = pipeLine.Name;
            result.Description = pipeLine.Description;

            foreach (var item in pipelineItems)
            {
                bool existStartStatus = false;
                bool existfinishedStatus = false;
                bool IsSuccessFinishedStatus = false;

                var statusItem = new StatusPipelineResultItem();

                statusItem.PipelineItemDescription = item.Description;
                statusItem.SortOrder = item.SortOrder;
                statusItem.TypeOperationId = item.TypeOperationId;

                var itemActions = journalRecords.Where(i => i.ItemId == item.Id)
                     .OrderBy(i => i.Period)
                     .ToList();

                var journalRecordCount = itemActions.Count();

                List<string> messages = new List<string>();
                


                // Цикл по записям лога
                for (int i = 0; i < journalRecordCount; i++)
                {
                    var journalRecord = itemActions[i];
                    //                    if (startStatuses.Contains(itemAction.TypeOperationAction))
                    //                    {
                    //                        statusItem.Started = itemAction.Period;
                    //                        statusItem.TriggerName = itemAction.TriggerName;
                    //                        existStartStatus = true;
                    //                    }
                    //                    if (i == itemActionsCount - 1)
                    //                    {
                    //                        if (finishedStatuses.Contains(itemAction.TypeOperationAction))
                    //                        {
                    //                            existfinishedStatus = true;
                    //                            statusItem.Finished = itemAction.Period;
                    //                        }




                    //if (successStatuses.Contains(itemAction.TypeOperationAction))
                    //{
                    //    IsSuccessFinishedStatus = true;
                    //    statusItem.Finished = itemAction.Period;
                    //}

                    //statusItem.LastOperationAction = itemAction.TypeOperationAction;   
                    //                    }

                    //                    if (!string.IsNullOrEmpty(itemAction.Message))
                    //{
                    //    messages.Add(itemAction.Message);
                    //}
                }

                //statusItem.Messages = messages;

                //var complete = existStartStatus && existfinishedStatus;
                //var success = existStartStatus && IsSuccessFinishedStatus;
                //statusItem.Complete = complete;
                //statusItem.Success = success;

                //result.Items.Add(statusItem);
            }

            //if (result.Items.Count == operationItems.Count
            //  && result.Items.Where(i => i.Complete == true).Count() == result.Items.Count)
            //{
            //    result.Complete = true;
            //}
            //if (result.Items.Count == operationItems.Count
            //    && result.Items.Where(i => i.Success == true).Count() == result.Items.Count)
            //{
            //    result.Success = true;
            //}
            //return result;

            throw new NotImplementedException();
        }
    }
}
