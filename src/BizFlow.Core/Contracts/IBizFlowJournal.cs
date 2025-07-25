﻿using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IBizFlowJournal
    {
        Task AddRecordAsync(BizFlowJournalRecord record, CancellationToken cancellationToken = default);
        Task<IEnumerable<BizFlowJournalRecord>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}
