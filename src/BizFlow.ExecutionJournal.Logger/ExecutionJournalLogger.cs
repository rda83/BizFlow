using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using BizFlow.Core.Model;
using Microsoft.Extensions.Logging;

namespace BizFlow.ExecutionJournal.Logger
{
    public class ExecutionJournalLogger : IExecutionJournal
    {
        private readonly ILogger<ExecutionJournalLogger> _logger;

        public ExecutionJournalLogger(ILogger<ExecutionJournalLogger> logger)
        {
            _logger = logger;
        }

        public Task AddBlockedPipelineAsync(string launchId, bool isStartNowPipeline, string pipelineName)
        {
            var msg = """
                Pipeline blocked: {pipelineName}
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;
            _logger.LogInformation(msg,
                pipelineName,
                DateTime.Now,   // Period
                pipelineName,   // pipelineName
                string.Empty,   // ItemDescription
                0,              // ItemSortOrder
                0,              // ItemId
                TypeBizFlowJournalAction.BlockedPipeline, // TypeAction
                string.Empty, // TypeOperationId
                launchId,  //LaunchId
                string.Empty, //Message
                string.Empty, //Trigger
                isStartNowPipeline); //IsStartNow

            return Task.CompletedTask;
        }

        public Task AddBlockedPipelineItemAsync(string launchId, Pipeline pipeline, PipelineItem pipelineItem)
        {
            var msg = """
                Pipeline blocked item: {StepDescription}
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;
            _logger.LogInformation(msg,
                pipelineItem.Description,           // StepDescription
                DateTime.Now,                       // Period    
                pipeline.Name,                      // PipelineName
                pipelineItem.Description,           // ItemDescription 
                pipelineItem.SortOrder,             // ItemSortOrder
                pipelineItem.Id,                    // ItemId
                TypeBizFlowJournalAction.BlockedPipelineItem,     // TypeAction
                pipelineItem.TypeOperationId,       // TypeOperationId
                launchId,                           // LaunchId
                string.Empty,                       // Message
                string.Empty,                       // Trigger
                false);                             // IsStartNow

            return Task.CompletedTask;
        }

        public Task AddCancelledAsync(CancelOperationArgs args)
        {
            var msg = """
                Pipeline cancelled: {StepDescription}
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;
            _logger.LogInformation(msg,
                args.PipelineName,                                                              // StepDescription 
                DateTime.Now,                                                                   // Period
                args.PipelineName,                                                              // PipelineName
                args.ItemDescription,                                                           // ItemDescription
                args.ItemSortOrder,                                                             // ItemSortOrder
                args.ItemId,                                                                    // ItemId
                TypeBizFlowJournalAction.Cancelled,                                              // TypeAction
                args.TypeOperationId,                                                           // TypeOperationId
                args.LaunchId,                                                                  // LaunchId
                $"Operation cancelled. Cancellation request ID: {args.CancellationRequestId}",  // Message
                args.Trigger,                                                                   // Trigger
                args.IsStartNowPipeline);                                                       // IsStartNow

            return Task.CompletedTask;
        }

        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, string msg)
        {
            var msgText = """
                Pipeline error:
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;

            _logger.LogInformation(msgText,
                DateTime.Now,                   // Period
                string.Empty,                   // PipelineName
                string.Empty,                   // ItemDescription
                0,                              // ItemSortOrder
                0,                              // ItemId
                TypeBizFlowJournalAction.Error, // TypeAction
                string.Empty,                   // TypeOperationId
                launchId,                       // LaunchIdLaunchId
                msg,                            // Message
                string.Empty,                   // Trigger
                isStartNowPipeline              // IsStartNow
                );

            return Task.CompletedTask;
        }

        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            var msg = """
                Pipeline error: {StepDescription}
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;
            _logger.LogInformation(msg,
                pipelineItem.Description,           // StepDescription
                DateTime.Now,                       // Period    
                pipeline.Name,                      // PipelineName
                pipelineItem.Description,           // ItemDescription 
                pipelineItem.SortOrder,             // ItemSortOrder
                pipelineItem.Id,                    // ItemId
                TypeBizFlowJournalAction.Error,     // TypeAction
                pipelineItem.TypeOperationId,       // TypeOperationId
                launchId,                           // LaunchId
                string.Empty,                       // Message
                string.Empty,                       // Trigger
                false);                             // IsStartNow

            return Task.CompletedTask;
        }

        public Task AddStartAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            var msg = """
                Pipeline step started: {StepDescription}
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;
            _logger.LogInformation(msg,
                pipelineItem.Description,
                DateTime.Now,
                pipeline.Name,
                pipelineItem.Description,
                pipelineItem.SortOrder,
                pipelineItem.Id,
                TypeBizFlowJournalAction.Start,
                pipelineItem.TypeOperationId,
                launchId,
                string.Empty,
                string.Empty,
                isStartNowPipeline);

            return Task.CompletedTask;
        }

        public Task AddSuccessAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            var msg = """
                Pipeline success: {StepDescription}
                (Period: {Period}, PipelineName: {PipelineName}, ItemDescription: {ItemDescription}, ItemSortOrder: {ItemSortOrder},
                ItemId: {ItemId}, TypeAction: {TypeAction}, TypeOperationId: {TypeOperationId}, LaunchId: {LaunchId}, Message: {Message},
                Trigger: {Trigger}, IsStartNow: {IsStartNow})
                """;
            _logger.LogInformation(msg,
                pipelineItem.Description,           // StepDescription
                DateTime.Now,                       // Period    
                pipeline.Name,                      // PipelineName
                pipelineItem.Description,           // ItemDescription 
                pipelineItem.SortOrder,             // ItemSortOrder
                pipelineItem.Id,                    // ItemId
                TypeBizFlowJournalAction.Success,     // TypeAction
                pipelineItem.TypeOperationId,       // TypeOperationId
                launchId,                           // LaunchId
                string.Empty,                       // Message
                string.Empty,                       // Trigger
                false);                             // IsStartNow

            return Task.CompletedTask;
        }
    }
}
