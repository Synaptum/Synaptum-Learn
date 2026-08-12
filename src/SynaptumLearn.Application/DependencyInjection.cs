using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SynaptumLearn.Application.Common.Behaviors;
using SynaptumLearn.Application.Common.Events;
using SynaptumLearn.Application.Common.Interfaces;
using System.Reflection;

namespace SynaptumLearn.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        return services;
    }
}