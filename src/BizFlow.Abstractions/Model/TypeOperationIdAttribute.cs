namespace BizFlow.Abstractions.Model
{
    public class TypeOperationIdAttribute : Attribute
    {
        public string TypeOperationId { get; set; } = string.Empty;

        public TypeOperationIdAttribute(string typeOperationId)
        {
            TypeOperationId = typeOperationId;
        }
    }
}
