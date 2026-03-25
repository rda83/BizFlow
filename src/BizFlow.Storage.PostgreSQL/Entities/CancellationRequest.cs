
namespace BizFlow.Storage.PostgreSQL.Entities
{
    class CancellationRequest
    {
        public long Id { get; set; }
        public string PipelineName { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
        public string? Description { get; set; } = string.Empty;
        public bool ClosingByExpirationTimeOnly { get; set; }
        public DateTime Created { get; set; }
        public bool Executed { get; set; }
        public DateTime ClosingTime { get; set; }
        public bool ClosedAfterExpirationDate { get; set; }

        public CancellationRequest() { }

        public CancellationRequest(Core.Model.CancellationRequest cancellationRequest) 
        {
            PipelineName = cancellationRequest.PipelineName;
            ExpirationTime = cancellationRequest.ExpirationTime;
            Description = cancellationRequest.Description;
            ClosingByExpirationTimeOnly = cancellationRequest.ClosingByExpirationTimeOnly;
            Created = cancellationRequest.Created;
            Executed = cancellationRequest.Executed;
            ClosingTime = cancellationRequest.ClosingTime;
            ClosedAfterExpirationDate = cancellationRequest.ClosedAfterExpirationDate;
        }

        public Core.Model.CancellationRequest ToCoreModel()
        {
            var result = new Core.Model.CancellationRequest()
            {
                PipelineName = PipelineName,
                ExpirationTime = ExpirationTime,
                Description = Description,
                ClosingByExpirationTimeOnly = ClosingByExpirationTimeOnly,
                Created = Created,
                Executed = Executed,
                ClosingTime = ClosingTime,
                ClosedAfterExpirationDate = ClosedAfterExpirationDate,
            };
            return result;
        }
    }
}
