using System.Diagnostics;
using Tevling.Shared;
using Tevling.Strava;
using Athlete = Tevling.Model.Athlete;

namespace Tevling.Components;

public partial class ChallengeForm : ComponentBase
{
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] private IAthleteService AthleteService { get; set; } = null!;
    [Inject] private IChallengeService ChallengeService { get; set; } = null!;

    [Parameter] public string SubmitLabel { get; set; } = "Submit";
    [Parameter] public string CancelLabel { get; set; } = "Cancel";
    [Parameter] public Action<ChallengeFormModel>? OnSubmit { get; set; }
    [Parameter] public Func<ChallengeFormModel, Task>? OnSubmitAsync { get; set; }
    [Parameter] public Action? OnCancel { get; set; }
    [Parameter] public Func<Task>? OnCancelAsync { get; set; }
    [Parameter] public Challenge? EditChallenge { get; set; }

    [SupplyParameterFromForm] public ChallengeFormModel Challenge { get; set; } = new();

    private List<ChallengeTemplate> _templates = [];
    private Dictionary<int, bool> _templatesSelectedForDeletion = [];
    private const int MaximumSuggestions = 10;
    private DropdownSearch<ActivityType>? _dropdownSearchRefActivityTypes;
    private DropdownSearch<Athlete>? _dropdownSearchRefAthletes;
    private Athlete Athlete { get; set; } = default!;
    private static IEnumerable<ActivityType> ActivityTypes => Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>();
    private static Func<Athlete, string> AthletesDisplayFunc => athlete => athlete.Name;

    protected override async Task OnInitializedAsync()
    {
        Athlete = await AuthenticationService.GetCurrentAthleteAsync();
        var templates = await ChallengeService.GetChallengeTemplatesAsync(Athlete.Id);
        _templates = [.. await ChallengeService.GetChallengeTemplatesAsync(Athlete.Id)];
        foreach (ChallengeTemplate challengeTemplate in templates)
        {
            _templatesSelectedForDeletion[challengeTemplate.Id] = false;
        }
    }


    private void ToggleSelection(int id)
    {
        _templatesSelectedForDeletion[id] = !_templatesSelectedForDeletion[id];
    }

    private void DeleteTemplates()
    {
        var keysToDelete = _templatesSelectedForDeletion.Where(t => t.Value).Select(t => t.Key).ToArray();
        foreach (int key in keysToDelete)
        {
            _ = DeleteTemplate(key);
            _templatesSelectedForDeletion.Remove(key);
        }
    }

    private async Task DeleteTemplate(int templateId)
    {
        await ChallengeService.DeleteChallengeTemplateAsync(templateId);
        _templates = [.. await ChallengeService.GetChallengeTemplatesAsync(Athlete.Id)];
        var templates = await ChallengeService.GetChallengeTemplatesAsync(Athlete.Id);

        foreach (ChallengeTemplate challengeTemplate in templates)
        {
            _templatesSelectedForDeletion[challengeTemplate.Id] = false;
        }
    }

    private void LoadSelectedTemplate(object? value)
    {
        if (value is not null && int.TryParse(value.ToString(), out int index) && index < _templates.Count)
        {
            LoadTemplate(_templates[index]);
        }
    }

    private void LoadTemplate(ChallengeTemplate template)
    {
        Challenge = new ChallengeFormModel
        {
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddMonths(1),
            CreatedBy = Athlete.Id,
            Title = template.Title,
            Description = template.Description,
            Measurement = template.Measurement,
            ActivityTypes = template.ActivityTypes,
            IsPrivate = template.IsPrivate,
            InvitedAthletes = [],
        };
    }

    private async Task CreateChallengeTemplate()
    {
        ChallengeTemplate newChallengeTemplate = new()
        {
            Title = Challenge.Title,
            Description = Challenge.Description,
            Measurement = Challenge.Measurement,
            ActivityTypes = [.. Challenge.ActivityTypes],
            IsPrivate = Challenge.IsPrivate,
            Created = DateTimeOffset.Now,
            CreatedById = Challenge.CreatedBy,
        };
        await ChallengeService.CreateChallengeTemplateAsync(newChallengeTemplate);
        _templates = [.. await ChallengeService.GetChallengeTemplatesAsync(Athlete.Id)];
        var templates = await ChallengeService.GetChallengeTemplatesAsync(Athlete.Id);

        foreach (ChallengeTemplate challengeTemplate in templates)
        {
            _templatesSelectedForDeletion[challengeTemplate.Id] = false;
        }
    }
    
    protected override void OnParametersSet()
    {
        if (EditChallenge != null)
        {
            if (EditChallenge.Athletes is null) throw new Exception("Athletes not initialized");

            Challenge.Title = EditChallenge.Title;
            Challenge.Description = EditChallenge.Description;
            Challenge.Start = EditChallenge.Start;
            Challenge.End = EditChallenge.End;
            Challenge.Measurement = EditChallenge.Measurement;
            Challenge.ActivityTypes = EditChallenge.ActivityTypes.ToList();
            Challenge.IsPrivate = EditChallenge.IsPrivate;
            Challenge.CreatedBy = EditChallenge.CreatedById;
            Challenge.InvitedAthletes = EditChallenge.InvitedAthletes?.ToList() ?? []
                ;
        }
        else
        {
            Challenge = new ChallengeFormModel
            {
                Start = DateTimeOffset.Now,
                End = DateTimeOffset.Now.AddMonths(1),
                CreatedBy = Athlete.Id,
            };
        }
    }

    private async Task<IEnumerable<Athlete>> SearchAthletes(string searchText)
    {
        AthleteFilter filter = new()
        {
            SearchText = searchText,
            NotIn = Challenge.InvitedAthletes.Select(a => a.Id).Append(Athlete.Id),
        };

        Athlete[] result = await AthleteService.GetAthletesAsync(filter, new Paging(MaximumSuggestions));

        return result;
    }

    private async Task DeselectActivityType(ActivityType item)
    {
        if (_dropdownSearchRefActivityTypes is null) return;

        await _dropdownSearchRefActivityTypes.DeselectItemAsync(item);
    }


    private async Task DeselectAthlete(Athlete item)
    {
        if (_dropdownSearchRefAthletes is null) return;

        await _dropdownSearchRefAthletes.DeselectItemAsync(item);
    }

    private async Task SubmitForm()
    {
        OnSubmit?.Invoke(Challenge);

        if (OnSubmitAsync != null) await OnSubmitAsync(Challenge);
    }

    private async Task Cancel()
    {
        OnCancel?.Invoke();

        if (OnCancelAsync != null) await OnCancelAsync();
    }
}
