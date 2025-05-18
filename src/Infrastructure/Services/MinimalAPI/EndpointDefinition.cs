using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MinimalAPI;

public sealed record EndpointDefinition(Type EndpointType,Type RequestType, Type ResponseType)
{
    public string? RouteGroupPath { get; internal set; }
    internal IEndpointFilter[] EndpointFilters { get; set; } = [];
    public VersioningOptions Version { get; internal set; }
    public string[] Verbs { internal get; set; } = [];
    public object[] EndpointAttributes { get; internal set; } = [];

    public bool EnabledAuthorization { get; internal set; }

    public bool AllowAnonymous;

    [StringSyntax("Route")]
    public string? RouteTemplate { get; set; }
    public List<string>? PreBuiltUserPolicies { get; private set; }
    public Action<AuthorizationPolicyBuilder>? PolicyBuilder { get; private set; }

    public void Policy(Action<AuthorizationPolicyBuilder> policy)
        => PolicyBuilder = policy + PolicyBuilder;

    public void Policies(params string[] policyNames)
    {
        PreBuiltUserPolicies?.AddRange(policyNames);
        PreBuiltUserPolicies ??= [.. policyNames];
    }

    public List<string>? AuthSchemeNames { get; private set; }

    public List<string>? AllowedRoles { get; private set; }

    public void Roles(params string[] rolesNames)
    {
        AllowedRoles?.AddRange(rolesNames);
        AllowedRoles ??= [.. rolesNames];
    }

    public void AuthSchemes(params string[] rolesNames)
    {
        AuthSchemeNames?.AddRange(rolesNames);
        AuthSchemeNames ??= [.. rolesNames];
    }

    internal readonly bool Disposable = EndpointType.IsAssignableTo(typeof(IDisposable));
    internal readonly bool DisposableAsync = EndpointType.IsAssignableTo(typeof(IAsyncDisposable));

    internal object? RequestBinder;
    internal JsonSerializerContext? SerializerContext;
}
