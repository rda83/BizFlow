using System.Text.Json;

namespace BizFlow.Storage.PostgreSQL.Entities
{
    class PipelineItem
    {
        public long Id { get; set; }
        public long PipelineId { get; set; }
        public string? TypeOperationId { get; set; }
        public int SortOrder { get; set; }
        public string? Description { get; set; }
        public bool Blocked { get; set; }
        public JsonElement Options { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PipelineItem() { }

        public PipelineItem(long pipelineId, Core.Model.PipelineItem pipelineItemDto)
        {
            PipelineId = pipelineId;

            Id = pipelineItemDto.Id;
            TypeOperationId = pipelineItemDto.TypeOperationId;
            SortOrder = pipelineItemDto.SortOrder;
            Description = pipelineItemDto.Description;
            Blocked = pipelineItemDto.Blocked;
            Options = pipelineItemDto.Options;
        }
    }
}
