using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BizFlow.Core.Controllers
{
    [ApiController]
    [Route("bizFlow")]
    public class BizFlowController : ControllerBase
    {
        public BizFlowController() { }

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
