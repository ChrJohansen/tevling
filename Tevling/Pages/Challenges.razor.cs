using System.Diagnostics;
using System.Reactive.Subjects;

namespace Tevling.Pages;

public partial class Challenges : ComponentBase, IDisposable
{
    [Inject] private ILogger<Challenges> Logger { get; set; } = null!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] private IChallengeService ChallengeService { get; set; } = null!;

    private readonly Subject<string> _filterTextSubject = new();
    private readonly TimeSpan _filterTextThrottle = TimeSpan.FromMilliseconds(300);

    private readonly int _pageSize = 10;
    private IDisposable? _challengeFeedSubscription;
    private List<Challenge> _challenges = [];

    private string _filterText = string.Empty;
    private IDisposable? _filterTextSubscription;
    private int _page;

    private bool _showAllChallenges = true;

    private bool _showOutdatedChallenges;
    
    private bool _showTimeBasedChallenges;
    private bool _showMyChallenges;
    private bool _showChallengesByMe;
    private bool _showDistanceBasedChallenges;
    private bool _showElevationBasedChallenges;
    private int AthleteId { get; set; }
    private bool HasMore { get; set; } = true;
    private Challenge[] ChallengeList { get; set; } = [];

    private bool ShowMyChallenges
    {
        get => _showMyChallenges;
        set
        {
            _showMyChallenges = value;
            OnFilterChange();
        }
    }
    private bool ShowChallengesByMe
    {
        get => _showChallengesByMe;
        set
        {
            _showChallengesByMe = value;
            OnFilterChange();
        }
    }

    private bool ShowOutdatedChallenges
    {
        get => _showOutdatedChallenges;
        set
        {
            _showOutdatedChallenges = value;
            OnFilterChange();
        }
    }
    
    private bool ShowTimeBasedChallenges
    {
        get => _showTimeBasedChallenges;
        set
        {
            _showTimeBasedChallenges = value;
            OnFilterChange();
        }
    }
    private bool ShowDistanceBasedChallenges
    {
        get => _showDistanceBasedChallenges;
        set
        {
            _showDistanceBasedChallenges = value;
            OnFilterChange();
        }
    }
    private bool ShowElevationBasedChallenges
    {
        get => _showElevationBasedChallenges;
        set
        {
            _showElevationBasedChallenges = value;
            OnFilterChange();
        }
    }

    private string FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value;
            OnFilterChange();
        }
    }

    public void Dispose()
    {
        _filterTextSubscription?.Dispose();
        _challengeFeedSubscription?.Dispose();
    }

    protected override async Task OnInitializedAsync()
    {
        Athlete athlete = await AuthenticationService.GetCurrentAthleteAsync();

        AthleteId = athlete.Id;
        await FetchChallenges();
        SubscribeToChallengeFeed();

        _filterTextSubscription = _filterTextSubject
            .Throttle(_filterTextThrottle)
            .Subscribe(
                s =>
                {
                    FilterText = s;
                    InvokeAsync(StateHasChanged);
                });
    }

    private void SetFilterTextDebounced(ChangeEventArgs e)
    {
        _filterTextSubject.OnNext(e.Value!.ToString()!);
    }

    private void OnFilterChange()
    {
        _challenges = [];
        _page = -1;
        HasMore = true;
        UpdateChallenges();
    }

    private async Task LoadMore(CancellationToken ct)
    {
        int prevCount = _challenges.Count;
        _page++;
        await FetchChallenges(ct);
        HasMore = _challenges.Count > prevCount;
        StateHasChanged();
    }

    private async Task FetchChallenges(CancellationToken ct = default)
    {
        ChallengeFilter filter = new(
            _filterText,
            _showAllChallenges ? null : AthleteId,
            _showOutdatedChallenges);
        Challenge[] challenges =
            await ChallengeService.GetChallengesAsync(AthleteId, filter, new Paging(_pageSize, _page), ct);
        AddChallenges(challenges);
    }

    private void SubscribeToChallengeFeed()
    {
        _challengeFeedSubscription = ChallengeService.GetChallengeFeed()
            .Catch<FeedUpdate<Challenge>, Exception>(
                err =>
                {
                    Logger.LogError(err, "Error in challenge feed");
                    return Observable.Throw<FeedUpdate<Challenge>>(err).Delay(TimeSpan.FromSeconds(1));
                })
            .Retry()
            .Subscribe(
                feed =>
                {
                    switch (feed.Action)
                    {
                        case FeedAction.Create:
                            AddChallenges(feed.Item);
                            break;
                        case FeedAction.Update:
                            ReplaceChallenge(feed.Item);
                            break;
                        case FeedAction.Delete:
                            RemoveChallenge(feed.Item);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unknown challenge feed action: " + feed.Action);
                    }
                });
    }

    private void AddChallenges(params Challenge[] challenges)
    {
        _challenges.AddRange(challenges);

        UpdateChallenges();
    }

    private void ReplaceChallenge(Challenge challenge)
    {
        _challenges.RemoveAll(c => c.Id == challenge.Id);
        _challenges.Add(challenge);

        UpdateChallenges();
    }

    private void RemoveChallenge(Challenge challenge)
    {
        _challenges.RemoveAll(c => c.Id == challenge.Id);

        UpdateChallenges();
    }

    private void UpdateChallenges()
    {
        ChallengeList = _challenges
            .Where(c => !_showChallengesByMe || c.CreatedById == AthleteId)
            .Where(c => !_showMyChallenges || c.Athletes?.Any(athlete => athlete.Id == AthleteId) == true)
            .Where(c => !_showTimeBasedChallenges || c.Measurement == ChallengeMeasurement.Time)
            .Where(c => !_showDistanceBasedChallenges || c.Measurement == ChallengeMeasurement.Distance)
            .Where(c => !_showElevationBasedChallenges || c.Measurement == ChallengeMeasurement.Elevation)
            .Where(c => _showOutdatedChallenges || c.End.UtcDateTime.Date >= DateTimeOffset.UtcNow.Date)
            .Where(
                c => string.IsNullOrWhiteSpace(_filterText) ||
                    c.Title.Contains(_filterText, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(c => c.Start)
            .ThenBy(c => c.Title)
            .ToArray();

        InvokeAsync(StateHasChanged);
    }
}
