using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Features.AddPipeline;
using BizFlow.Core.Internal.Features.DeletePipeline;
using BizFlow.Core.Internal.Features.StartNowPipeline;
using BizFlow.Core.Internal.Features.StatusPipeline;
using BizFlow.Core.Model;
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpDelete("pipeline/{pipelineName}")]
        public async Task<IActionResult> DeletePipeline([FromRoute] string pipelineName,
            [FromServices] IDeletePipelineHandler deletePipelineHandler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await deletePipelineHandler.DeletePipelineAsync(new DeletePipelineCommand()
            {
                Name = pipelineName,
            });

            return Ok(result);
        }

        /// <summary>
        /// Получает постраничный список записей журнала выполнения пайплайнов
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начиная с 1)</param>
        /// <param name="pageSize">Количество записей на странице</param>
        /// <returns>Список записей и общее количество</returns>
        [HttpGet("journal-records")]
        [ProducesResponseType(typeof(PagedResponse<BizFlowJournalRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetJournalRecordsPaged(
            [FromServices] IBizFlowJournal journal,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest("Номер страницы и размер страницы должны быть больше 0");

                var records = await journal.GetPagedAsync(pageNumber, pageSize);

                var response = new PagedResponse<BizFlowJournalRecord>()
                {
                    Data = records,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
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
    }
}
