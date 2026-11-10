using Backend.BLL.Services;
using Backend.DAL.Interfaces;
using Backend.Models;

// Dependency-free service checks. Run with: dotnet run --project backend.tests
await CreationRetriesCollisions();
await CreationStopsAfterRepeatedCollisions();
await JoinNormalizesAndForwardsResults();
Console.WriteLine("Household service checks passed.");

static async Task CreationRetriesCollisions()
{
    var repository = new FakeRepository { CollisionsRemaining = 1 };
    var service = new HouseholdService(repository);
    using var source = new CancellationTokenSource();
    var household = await service.CreateAsync("  Maison  ", "  creator  ", source.Token);

    Assert(repository.Created.Count == 2, "A code collision must trigger a retry.");
    Assert(repository.Created[0].InvitationCode != household.InvitationCode, "A retry must generate a new code.");
    Assert(household.Id != Guid.Empty && household.Name == "Maison", "The created household must have an ID and trimmed name.");
    Assert(household.InvitationCode.Length == 16 && household.InvitationCode.All("ABCDEFGHJKLMNPQRSTUVWXYZ23456789".Contains), "The invitation code has an invalid format.");
    Assert(repository.LastUserId == "creator" && repository.LastToken == source.Token, "Creator and cancellation token must reach the repository.");
}

static async Task CreationStopsAfterRepeatedCollisions()
{
    var repository = new FakeRepository { CollisionsRemaining = int.MaxValue };
    try
    {
        await new HouseholdService(repository).CreateAsync("Maison", "creator");
        throw new Exception("Repeated collisions must fail instead of returning an unpersisted household.");
    }
    catch (InvalidOperationException)
    {
        Assert(repository.Created.Count > 1 && repository.Created.Count <= 10, "Collision retries must be bounded.");
    }
}

static async Task JoinNormalizesAndForwardsResults()
{
    var household = new Household { Id = Guid.NewGuid(), Name = "Maison", InvitationCode = "ABCDEFGH23456789" };
    var repository = new FakeRepository { Result = new(JoinHouseholdStatus.Joined, household) };
    var service = new HouseholdService(repository);
    using var source = new CancellationTokenSource();
    var result = await service.JoinAsync("  abcdefgh23456789  ", " member ", source.Token);
    Assert(repository.LastCode == household.InvitationCode && repository.LastUserId == "member", "Join must normalize the code and member ID.");
    Assert(repository.LastToken == source.Token && result == repository.Result, "Join must forward the token and repository result.");

    foreach (var status in new[] { JoinHouseholdStatus.NotFound, JoinHouseholdStatus.AlreadyMember })
    {
        repository.Result = new(status, null);
        Assert((await service.JoinAsync("missing", "member")).Status == status, "Join must preserve failure statuses.");
    }
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

sealed class FakeRepository : IHouseholdRepository
{
    public int CollisionsRemaining { get; set; }
    public List<Household> Created { get; } = [];
    public string? LastCode { get; private set; }
    public string? LastUserId { get; private set; }
    public CancellationToken LastToken { get; private set; }
    public JoinHouseholdResult Result { get; set; } = new(JoinHouseholdStatus.NotFound, null);

    public Task<bool> CreateAsync(Household household, string creatorId, CancellationToken cancellationToken = default)
    {
        Created.Add(household);
        LastUserId = creatorId;
        LastToken = cancellationToken;
        return Task.FromResult(CollisionsRemaining-- <= 0);
    }

    public Task<JoinHouseholdResult> JoinAsync(string invitationCode, string userId, CancellationToken cancellationToken = default)
    {
        LastCode = invitationCode;
        LastUserId = userId;
        LastToken = cancellationToken;
        return Task.FromResult(Result);
    }
}
