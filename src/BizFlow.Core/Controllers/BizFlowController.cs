using BizFlow.Abstractions;
using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Internal.Features.AddPipeline;
using BizFlow.Core.Internal.Features.CancelPipeline;
using BizFlow.Core.Internal.Features.DeletePipeline;
using BizFlow.Core.Internal.Features.StartNowPipeline;
using BizFlow.Core.Internal.Features.StatusPipeline;
using BizFlow.Core.Model;
using BizFlow.Schedules.Interval;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BizFlow.Core.Controllers
{
    [ApiController]
    [Route("bizFlow")]
    public class BizFlowController : ControllerBase
    {
        public BizFlowController() { }

        /// <summary>
        /// Добавление нового пайплайна
        /// </summary>
        /// <returns>Результат выполнения операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline")]
        public async Task<IActionResult> CreatePipelineAsync(AddPipelineCommand command,
            [FromServices] IAddPipelineHandler addPipelineHandler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await addPipelineHandler.AddPipelineAsync(command);

            return Ok(result);
        }

        /// <summary>
        /// Удаление пайплайна
        /// </summary>
        /// <returns>Результат выполнения операции</returns>
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Consumes("application/json")]
        //[HttpDelete("pipeline/{pipelineName}")]
        //public async Task<IActionResult> DeletePipeline([FromRoute] string pipelineName,
        //    [FromServices] IDeletePipelineHandler deletePipelineHandler)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var result = await deletePipelineHandler.DeletePipelineAsync(new DeletePipelineCommand()
        //    {
        //        Name = pipelineName,
        //    });

        //    return Ok(result);
        //}

        /// <summary>
        /// Получает постраничный список записей журнала выполнения пайплайнов
        /// </summary>
        /// <param name="lastId"></param>
        /// <param name="limit"></param>
        /// <returns>Список записей и общее количество</returns>
        [HttpGet("journal-records")]
        [ProducesResponseType(typeof(PagedResponse<JournalRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetJournalRecordsPaged(
            [FromServices] IBizFlowStorage bizFlowStorage,
            [FromQuery] long lastId = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var records = await bizFlowStorage.GetJournalRecordsAsync(lastId, limit);

                var response = new PagedResponse<JournalRecord>()
                {
                    Data = records.Pipelines,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Немедленный запуск пайплайна
        /// </summary>
        /// <returns>Результат выполнения операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline/{pipelineName}/startNow")]
        public async Task<IActionResult> StartNowPipeline([FromRoute] string pipelineName,
            [FromServices] IStartNowPipelineHandler handler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await handler.StartNowPipelineAsync(new StartNowPipelineCommand() { Name = pipelineName } );
            return Ok(result);
        }

        /// <summary>
        /// Получение статуса выполнения пайплайна
        /// </summary>
        /// <returns>Статус выполнения пайплайна</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatusPipelineResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline/{pipelineName}/statusPipeline")]
        public async Task<IActionResult> StatusPipeline([FromRoute] string pipelineName,
            [FromServices] IStatusPipelineHandler handler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await handler.StatusPipeline(new StatusPipelineCommand() { Name = pipelineName });
            return Ok(result);
        }

        /// <summary>
        /// Отправка запроса для завершения пайплайна
        /// </summary>
        /// <returns>Результат операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline/cancelPipeline")]
        public async Task<IActionResult> CancelPipeline([FromBody] CancelPipelineCommand cancelPipelineCommand,
            [FromServices] ICancelPipelineHandler handler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await handler.CancelPipeline(cancelPipelineCommand);
            return Ok(result);
        }

        /// <summary>
        /// Закрыть запрос на завершение пайплайна
        /// </summary>
        /// <returns>Результат операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline/closeCancellationRequest")]
        public async Task<IActionResult> CloseCancellationRequest([FromBody] CloseCancelPipelineCommand command,
            [FromServices] ICancelPipelineHandler handler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await handler.CloseCancellationRequest(command);
            return Ok(result);
        }


        // DEBUG <


        /// <summary>
        /// Отладка механизма отмены выполнения пайплайна
        /// </summary>
        /// <returns>Результат операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline/cancelPipelineTest")]
        public async Task<IActionResult> CancelPipelineTest([FromBody] CancelPipelineCommand command,
            [FromServices] ICancellationService cancellationService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            cancellationService.Cancel(command.PipelineName);
            return Ok();
        }


        /// <summary>
        /// Отладка механизма создания пайплайна
        /// </summary>
        /// <returns>Результат операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline/createPipelineTest")]
        public async Task<IActionResult> CreatelPipelineTest([FromBody] AddPipelineCommand command,
            [FromServices] IPipelineRegistry pipelineRegistry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var newPipeline = new BizFlow.Abstractions.Model.Pipeline();

            newPipeline.Name = command.Name;
            newPipeline.Description = command.Description;
            newPipeline.Schedule = new IntervalSchedule(TimeSpan.FromSeconds(30)); // Только на время отладки

            foreach (var item in command.PipelineItems)
            {
                var newItem = new BizFlow.Abstractions.Model.PipelineItem();
                newItem.TypeOperationId = item.TypeOperationId;
                newItem.Description = item.Description;
                newItem.SortOrder = item.SortOrder;

                newPipeline.PipelineItems.Add(newItem);
            }

            pipelineRegistry.Create(newPipeline);

            return Ok();
        }


        /// <summary>
        /// Отладка механизма удаления пайплайна
        /// </summary>
        /// <returns>Результат выполнения операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpDelete("pipeline/remove/{pipelineName}")]
        public async Task<IActionResult> DeletePipelineTest([FromRoute] string pipelineName,
            [FromServices] IPipelineRegistry pipelineRegistry)
        {
            var result = pipelineRegistry.Remove(pipelineName);
            return Ok(result);
        }

        // DEBUG >
    }
}
