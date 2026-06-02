using System.Threading.Tasks;
using N_Tier.Application.Models.OpenAlex;

namespace N_Tier.Application.Services;

public interface IOpenAlexService
{
    Task<SearchWorksResponseModel> SearchWorksAsync(SearchWorksRequestModel request);
}
