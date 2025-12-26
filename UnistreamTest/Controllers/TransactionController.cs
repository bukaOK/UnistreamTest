using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UnistreamTest.Models.TransactionApi;
using UnistreamTest.RequestHandlers.Abstract;
using UnistreamTest.Extensions;

namespace UnistreamTest.Controllers
{
    [ApiController]
    [Route("api/v1/transaction")]
    public class TransactionController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<Transaction>(200)]
        public async Task<IResult> Get(
            [FromQuery] Guid id,
            [FromServices] IGetPaymentTransactionHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.MapToHttpResult();
        }

        [HttpPost]
        [ProducesResponseType<CreateTransactionResponse>(200)]
        public async Task<IResult> Add(
            [FromBody] Transaction request,
            [FromServices] ICreatePaymentTransactionHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.MapToHttpResult();
        }
    }
}
