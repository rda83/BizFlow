using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.AddPipeline
{
    // Если удалить интерфейсы:
    // Чтобы использовать AddPipelineHandler, клиентский проект обязан сослаться на сборку BizFlow.Core.Это
    // нарушает принцип чистой архитектуры, о котором мы говорили.
    // Клиент потянет за собой весь планировщик, реестр и т.д. - но это справедливо если мы интерфейсы перенесем в BizFlow.Abstractions
    
    // по итогу: пробуем перетащить интерфеейсы в BizFlow.Abstractions, и возможно получтиься отказаться от ссылки на BizFlow.Core у клиента


    public interface IAddPipelineHandler
    {
        //Task<BizFlowChangingResult> AddPipelineAsync(AddPipelineCommand command, 
        //    CancellationToken cancellationToken = default);
    }
}
