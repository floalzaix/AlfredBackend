using System.Security.Cryptography;
using Backend.BLL.Interfaces;
using Backend.DAL.Interfaces;
using Backend.Models;

namespace Backend.BLL.Services;

public class HouseholdService(IHouseholdRepository repository) : IHouseholdService
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 16;
    private const int MaximumCodeAttempts = 10;

    public async Task<Household> CreateAsync(string name, string userId, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < MaximumCodeAttempts; attempt++)
        {
            var code = new char[CodeLength];
            for (var i = 0; i < code.Length; i++)
                code[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];

            var household = new Household
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                InvitationCode = new string(code)
            };

            if (await repository.CreateAsync(household, userId.Trim(), cancellationToken))
                return household;
        }

        throw new InvalidOperationException("Could not generate a unique household invitation code.");
    }

    public Task<JoinHouseholdResult> JoinAsync(string code, string userId, CancellationToken cancellationToken = default) =>
        repository.JoinAsync(code.Trim().ToUpperInvariant(), userId.Trim(), cancellationToken);
}
