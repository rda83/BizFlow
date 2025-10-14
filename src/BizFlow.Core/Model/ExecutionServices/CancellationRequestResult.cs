namespace BizFlow.Core.Model.ExecutionServices
{
    public class CancellationRequestResult
    {
        public bool ClosingByExpirationTimeOnly { get; set; }
        public string Description { get; set; } = string.Empty;
        public long CancellationRequestId { get; set; }

        internal static CancellationRequestResult? FromRequest(CancelPipelineRequest cancellationRequest)
        {
            var result = new CancellationRequestResult()
            {
                CancellationRequestId = cancellationRequest.Id,
                ClosingByExpirationTimeOnly = cancellationRequest.ClosingByExpirationTimeOnly,
                Description = cancellationRequest.Description ?? string.Empty,
            };
            return result;
        }
    }
}
