# Copilot instructions for IpcServiceFramework

Purpose: Provide targeted, actionable guidance so an AI coding agent can be immediately productive in this repo.

## Quick summary (big picture)
- This repo implements a small .NET Core IPC framework that exposes: client libraries, server hosting, and protocol-specific transports (Named Pipe, TCP).
- Key components:
  - `src/JKang.IpcServiceFramework.Core` — core message types (`IpcRequest`, `IpcResponse`, `IpcStatus`), serializer interface (`IIpcMessageSerializer`), IO abstractions (`IpcReader`/`IpcWriter`).
  - `src/*Client*` — client SDKs + DI registration helpers (e.g., `AddNamedPipeIpcClient<T>`).
  - `src/*Hosting*` — host-side endpoint abstractions and `ConfigureIpcHost`/`IIpcHostBuilder` extensions (e.g., `AddNamedPipeEndpoint<T>`).
  - `samples/` — runnable examples (server and client) demonstrating DI registration and usage (`samples/*Program.cs`).

## When you edit code
- Preserve public API shapes: many projects are shipped as NuGet packages; avoid breaking changes without a version bump in `build/version.yml`.
- Add tests for behavior changes; integration tests typically use `IpcApplicationFactory<T>` (see `src/JKang.IpcServiceFramework.Testing`) to spin up host+client in-proc.

## Build / Test / CI
- Local build: `dotnet build src/*.sln --configuration Release` ✅
- Run tests: `dotnet test src/*Tests/*.csproj --configuration Release` ✅
- CI: Azure Pipelines is used (`build/azure-pipelines-ci.yml`); build/template runs `dotnet build` then `dotnet test` and packages artifacts.
- Versioning: package versions are controlled by `build/version.yml` — update that file when preparing a new release.
- Signing: assemblies are strong-named (`SignAssembly` true in `src/Directory.Build.props`) using `IpcServiceFramework.snk`. If missing, builds may fail.

## Important conventions & patterns (project-specific)
- DI registration:
  - Server: `Host.CreateDefaultBuilder(...).ConfigureIpcHost(builder => builder.AddNamedPipeEndpoint<TContract>(...))` (see `samples/` and `NamedPipeIpcHostBuilderExtensions.cs`).
  - Client: use `IServiceCollection.AddNamedPipeIpcClient<TContract>("name", pipeName)` then `IIpcClientFactory<T>.CreateClient("name")`.
- Request generation:
  - Clients can invoke via expression proxies (DispatchProxy) `client.InvokeAsync(x => x.Method(...))` or by building `IpcRequest` directly (`TestHelpers.CreateIpcRequest(...)`). Use the proxy when allowed; dynamic codegen can be disabled via the `DisableDynamicCodeGeneration` build define.
- Serialization:
  - Default serializer: `DefaultIpcMessageSerializer` uses `Newtonsoft.Json` with `TypeNameHandling.Objects`. Be careful: this affects type resolution and has security implications. The client option `UseSimpleTypeNameAssemblyFormatHandling` toggles how type assembly names are handled (see `IpcClientOptions.UseSimpleTypeNameAssemblyFormatHandling`).
- Error handling:
  - Server maps exceptions to `IpcResponse` with `IpcStatus` and the client throws `IpcFaultException` for non-OK statuses. Use `IpcEndpointOptions.IncludeFailureDetailsInResponse` to include exception messages in responses (useful for tests).
- Streams & translators:
  - You can wrap streams for custom handshakes/logging with `IpcClientOptions.StreamTranslator` (see `doc/stream-translator.md` and `samples` examples).

## Testing notes & patterns
- Tests use xUnit, Moq, AutoFixture and the `IpcApplicationFactory<T>` pattern to create host+client pairs.
- Many tests verify both dynamic proxy invocation and static `IpcRequest` invocation — ensure you add test coverage for both when relevant.
- The test projects target `netcoreapp3.1`; library projects target `netstandard2.0`.
- To test environments where dynamic generation is unavailable, set `DisableDynamicCodeGeneration=true` (either in a build property or CI matrix).

## Security & operational notes
- TCP over untrusted networks: prefer SSL/TLS (see `doc/tcp/security.md`) and do not use example certs in production.
- Serializer uses `TypeNameHandling.Objects`: when changing serializer behavior, ensure tests cover scenarios with mismatched assemblies and type name handling.
- Be cautious when enabling `IncludeFailureDetailsInResponse` in production — it can leak server exception details.

## Good first tasks for an AI agent
- Small bugfixes: add unit tests under `src/*Tests`, follow existing test patterns, use `IpcApplicationFactory` for integration tests.
- Feature work: register DI helpers (follow `*ServiceCollectionExtensions.cs` structure) and add corresponding tests (contract tests and error cases).
- Refactors: keep backward compatibility; update `build/version.yml` when the public API changes.

---

If anything above is unclear or you want more detail for any section (examples, tests, or CI matrix samples), tell me which parts to expand and I will iterate. ✅