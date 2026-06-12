using System.Threading.Tasks;

namespace N_Tier.Application.Services;

public interface IHangfireJobService
{
    void ScheduleRecurringJobs();
    Task ReindexPapersAsync();
    string EnqueueReindexJob();
    string EnqueueRecreateIndexJob();

    // Author background jobs
    Task ReindexAuthorsAsync();
    string EnqueueReindexAuthorsJob();
    string EnqueueRecreateAuthorsIndexJob();
}
