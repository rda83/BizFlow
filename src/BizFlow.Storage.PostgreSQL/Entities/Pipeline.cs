
namespace BizFlow.Storage.PostgreSQL.Entities
{
    class Pipeline
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Blocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public Pipeline() { }
        public Pipeline(Core.Model.Pipeline pipelineDto)
        {
            Name = pipelineDto.Name;
            CronExpression = pipelineDto.CronExpression;
            Description = pipelineDto.Description;
            Blocked = pipelineDto.Blocked;
        }
    }
}
