using System.Reflection;
using Electium.Application.Interfaces;
using Electium.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Electium.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<ICitizenService, CitizenService>();
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IElectivePositionService, ElectivePositionService>();
        services.AddScoped<IElectionService, ElectionService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IVotingService, VotingService>();

        return services;
    }
}
