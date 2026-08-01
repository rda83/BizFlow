using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core
{
    /// <summary>
    /// Executes a <see cref="Pipeline"/> by resolving and invoking registered <see cref="IWorker"/> 
    /// implementations for each pipeline item, using dependency injection scopes.
    /// All execution events are recorded in an <see cref="IExecutionJournal"/>.
    /// </summary>
    /// <remarks>
    /// The executor creates a new DI scope per pipeline item via <see cref="IServiceScopeFactory"/>,
    /// resolving a keyed service of type <see cref="IWorker"/> based on <c>TypeOperationId</c>.
    /// 
    /// <para>Cancellation is supported: if the token is cancelled, the current item is logged as cancelled
    /// and an <see cref="OperationCanceledException"/> is thrown.</para>
    /// 
    /// <para>If a pipeline item throws an exception (other than cancellation), it is logged and rethrown,
    /// causing the entire pipeline execution to stop.</para>
    /// </remarks>
    public class PipelineExecutor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IExecutionJournal _journal;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineExecutor"/> class.
        /// </summary>
        /// <param name="scopeFactory">
        /// The factory used to create scopes for resolving per-item <see cref="IWorker"/> services.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <param name="journal">
        /// The journal where all execution events (start, success, error, cancellation, blocked) are recorded.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="scopeFactory"/> or <paramref name="journal"/> is <see langword="null"/>.
        /// </exception>
        public PipelineExecutor(IServiceScopeFactory scopeFactory, IExecutionJournal journal)
        {
            _scopeFactory = scopeFactory;
            _journal = journal;
        }

        public async Task Execute(Pipeline pipeline, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var launchId = Guid.NewGuid().ToString();
                      
            var isStartNowPipeline = false; // DEBUG

            if (pipeline == null)
            {
                await _journal.AddErrorAsync(launchId, isStartNowPipeline, "The pipeline parameter was null. Execution cannot proceed.");
                return;
            }

            if (pipeline.Blocked)
            {
                await _journal.AddBlockedPipelineAsync(launchId, isStartNowPipeline, pipeline.Name);
                return;
            }

            await ExecuteAllItems(pipeline, launchId, isStartNowPipeline, ct);   
        }

        /// <summary>
        /// Starts execution of the specified <paramref name="pipeline"/>.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline to execute. If <see langword="null"/>, an error is logged and execution returns immediately.
        /// </param>
        /// <param name="ct">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// If cancellation is requested before execution begins, the method will log the cancellation
        /// and throw an <see cref="OperationCanceledException"/> for each remaining item.
        /// </param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>Each pipeline item is executed sequentially in order of <see cref="PipelineItem.SortOrder"/>.</para>
        /// 
        /// <para>If the pipeline or an individual item is marked as blocked, execution is skipped and the 
        /// fact is recorded in the journal without throwing an exception.</para>
        /// 
        /// <para>When an item fails with an exception other than <see cref="OperationCanceledException"/>,
        /// the error is logged and immediately rethrown, stopping further item execution.</para>
        /// 
        /// <para>This method does not catch <see cref="OperationCanceledException"/>; it propagates to the caller
        /// after logging the cancellation of the current item.</para>
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the cancellation token is cancelled during execution.
        /// </exception>
        /// <seealso cref="IWorker"/>
        /// <seealso cref="IExecutionJournal"/>
        private async Task ExecuteAllItems(Pipeline pipeline, string launchId, bool isStartNowPipeline,  CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (pipeline.PipelineItems == null)
            {
                await _journal.AddErrorAsync(launchId, isStartNowPipeline, "Pipeline has no items collection.");
                return;
            }

            foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
            {
                if (!ct.IsCancellationRequested)
                {
                    await _journal.AddStartAsync(launchId, isStartNowPipeline, pipeline, pipelineItem);

                    if (pipelineItem.Blocked)
                    {
                        await _journal.AddBlockedPipelineItemAsync(launchId, pipeline, pipelineItem);
                        continue;
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(pipelineItem.TypeOperationId))
                        {                            
                            throw new InvalidOperationException(
                                $"Pipeline item '{pipelineItem.Description}' (Id: {pipelineItem.Id}) has empty TypeOperationId.");
                        }

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var worker = scope.ServiceProvider
                                .GetRequiredKeyedService<IWorker>(pipelineItem.TypeOperationId);

                            var workerContext = new WorkerContext();
                            workerContext.LaunchId = launchId;
                            workerContext.TypeOperationId = pipelineItem.TypeOperationId ?? string.Empty;
                            workerContext.PipelineName = pipeline.Name;
                            workerContext.Options = pipelineItem.Options;
                            workerContext.IsStartNowPipeline = isStartNowPipeline;

                            await worker.ExecuteAsync(workerContext, ct);
                        }
                        await _journal.AddSuccessAsync(launchId, isStartNowPipeline, pipeline, pipelineItem);
                    }
                    catch (OperationCanceledException)
                    {
                        await _journal.AddCancelledPipelineItemAsync(launchId, isStartNowPipeline, pipeline, pipelineItem);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await _journal.AddErrorAsync(launchId, isStartNowPipeline, pipeline, pipelineItem, ex.ToString());
                        throw; // TODO: необходимо дать право выбора, продолжать ли выполнение пайплайна при падении одного из элементов.
                    }
                }
                else
                {
                    await _journal.AddCancelledPipelineItemAsync(launchId, isStartNowPipeline, pipeline, pipelineItem);
                    ct.ThrowIfCancellationRequested();
                }           
            }
        }
    }
}
