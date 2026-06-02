using System.Threading.Tasks;
using N_Tier.Application.Models.SemanticScholar;

namespace N_Tier.Application.Services;

public interface ISemanticScholarService
{
    Task<SemanticScholarSearchResponseModel> SearchWorksAsync(SemanticScholarSearchRequestModel request);
}
