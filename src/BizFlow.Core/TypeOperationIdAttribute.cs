
namespace BizFlow.Core
{
    public class TypeOperationIdAttribute : Attribute
    {
        public string TypeOperationId { get; set; } = string.Empty;

        public TypeOperationIdAttribute(string typeOperationId)
        {
            this.TypeOperationId = typeOperationId;
        }
    }
}
