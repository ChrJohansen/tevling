using Spur.Model;

namespace Spur.Services;

public interface IActivityService
{
    Task<Activity> CreateActivityAsync(long stravaAthleteId, long stravaActivityId,
        CancellationToken ct = default);

    Task<Activity> UpdateActivityAsync(long stravaAthleteId, long stravaActivityId,
        CancellationToken ct = default);

    Task DeleteActivityAsync(long stravaAthleteId, long stravaActivityId,
        CancellationToken ct = default);

    Task<Activity[]> GetActivitiesForAthleteAsync(int athleteId, int pageSize, int page = 0,
        CancellationToken ct = default);

    IObservable<FeedUpdate<Activity>> GetActivityFeedForAthlete(int athleteId);

    Task ImportActivitiesForAthlete(int athleteId, CancellationToken ct = default);
}
