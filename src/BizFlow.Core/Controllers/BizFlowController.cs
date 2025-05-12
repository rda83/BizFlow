using BizFlow.Core.Internal.Features.AddPipeline;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BizFlow.Core.Controllers
{
    [ApiController]
    [Route("bizFlow")]
    public class BizFlowController : ControllerBase
    {
        private readonly IAddPipelineHandler addPipelineHandler;

        public BizFlowController(IAddPipelineHandler addPipelineHandler) 
        { 
            this.addPipelineHandler = addPipelineHandler;
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

            var result = await addPipelineHandler.AddPipelineAsync(command);

        //private IActionResult GetRoutineOpsChangingResult(RoutineOpsChangingResult routineOpsChangingResult)
        //{
        //    if (routineOpsChangingResult.Success)
        //    {
        //        return Ok(routineOpsChangingResult);
        //    }
        //    else
        //    {
        //        return BadRequest(routineOpsChangingResult);
        //    }
        //}

            return Ok(result);
        }
        





        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        public IActionResult GetOperations()
        {
            return Ok("Biz flow begin ...");
        }


    }
}
