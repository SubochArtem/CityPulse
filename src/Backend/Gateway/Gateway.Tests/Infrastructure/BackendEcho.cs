namespace Gateway.Tests.Infrastructure;

public sealed record BackendEcho(
    string Service,
    string Method,
    string Path,
    string Query,
    string Authorization,
    string Body);
