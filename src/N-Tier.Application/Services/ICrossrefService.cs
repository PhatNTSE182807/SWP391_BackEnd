using System.Threading.Tasks;
using N_Tier.Application.Models.Crossref;

namespace N_Tier.Application.Services;

public interface ICrossrefService
{
    Task<CrossrefSearchResponseModel> SearchWorksAsync(CrossrefSearchRequestModel request);
}
