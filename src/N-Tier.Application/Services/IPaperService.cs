using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Paper;

namespace N_Tier.Application.Services
{
    public interface IPaperService
    {
        Task<List<PaperResponseModel>> GetAllPapersAsync();
        Task<PaperResponseModel> GetPaperByIdAsync(Guid id);
        Task<List<PaperResponseModel>> GetPaperbyAuthorId (Guid authorId);
        Task<PagedResponse<PaperResponseModel>> GetPaginatedPapersAsync(PagedRequest request);
    }
}
