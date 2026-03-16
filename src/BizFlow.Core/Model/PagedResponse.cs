
namespace BizFlow.Core.Model
{
    public class PagedResponse<T>
    {
        public IReadOnlyCollection<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
