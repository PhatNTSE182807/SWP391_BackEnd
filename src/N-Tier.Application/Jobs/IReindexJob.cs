namespace N_Tier.Application.Jobs;

public interface IReindexJob
{
    Task ExecuteAsync();
}
