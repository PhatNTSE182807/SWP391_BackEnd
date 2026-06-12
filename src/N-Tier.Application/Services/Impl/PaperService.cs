using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using N_Tier.Application.Models;
using N_Tier.Application.Models.Paper;
using N_Tier.DataAccess.Repositories;

namespace N_Tier.Application.Services.Impl
{
    public class PaperService : IPaperService
    {
        private readonly IPaperRepository _paperRepository;

        public PaperService(IPaperRepository paperRepository)
        {
            _paperRepository = paperRepository;
        }

        public async Task<List<PaperResponseModel>> GetAllPapersAsync()
        {
            var paper = await _paperRepository.GetAllAsync(p => true);
            return paper.Adapt<List<PaperResponseModel>>();
        }

        public async Task<PagedResponse<PaperResponseModel>> GetPaginatedPapersAsync(PagedRequest request)
        {
            var (results, total) = await _paperRepository.GetPaginatedAsync(request.Page, request.Size);
            var mappedResults = results.Adapt<List<PaperResponseModel>>();
            return new PagedResponse<PaperResponseModel>(mappedResults, total, request.Page, request.Size);
        }

        public async Task<List<PaperResponseModel>> GetPaperbyAuthorId(Guid authorId)
        {
            var paper = await _paperRepository.GetPaperbyAuthorIdAsync(authorId);
            return paper.Adapt<List<PaperResponseModel>>();
        }

        public Task<PaperResponseModel> GetPaperByIdAsync(Guid id)
        {
            return GetPaperDetailAsync(id);
        }

        private async Task<PaperResponseModel> GetPaperDetailAsync(Guid id)
        {
            var paper = await _paperRepository.GetByIdAsync(id);
            return paper.Adapt<PaperResponseModel>();
        }
    }
}
