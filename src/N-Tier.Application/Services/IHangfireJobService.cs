using System.Threading.Tasks;

namespace N_Tier.Application.Services;

public interface IHangfireJobService
{
    void ScheduleRecurringJobs();
    Task ReindexPapersAsync();
    string EnqueueReindexJob();
    string EnqueueRecreateIndexJob();
}
