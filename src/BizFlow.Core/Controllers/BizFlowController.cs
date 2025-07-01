using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Features.AddPipeline;
using BizFlow.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BizFlow.Core.Controllers
{
    [ApiController]
    [Route("bizFlow")]
    public class BizFlowController : ControllerBase
    {
        private readonly IAddPipelineHandler _addPipelineHandler;
        private readonly IBizFlowJournal _journal;

        public BizFlowController(IAddPipelineHandler addPipelineHandler, IBizFlowJournal journal) 
        { 
            _addPipelineHandler = addPipelineHandler;
            _journal = journal;
        }

        /// <summary>
        /// Добавление нового пайплайна
        /// </summary>
        /// <returns>Результат выполнения операции</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BizFlowChangingResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost("pipeline")]
        public async Task<IActionResult> CreateOperationAsync(AddPipelineCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _addPipelineHandler.AddPipelineAsync(command);

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
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest("Номер страницы и размер страницы должны быть больше 0");

                var records = await _journal.GetPagedAsync(pageNumber, pageSize);

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
    }
}
