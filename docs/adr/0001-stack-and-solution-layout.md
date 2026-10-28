# ADR-0001: Stack confirmation and solution layout

- **Status:** Accepted
- **Date:** 2026-10-29
- **Decider:** PatrykLazuk (tech lead)
- **Issue:** #4
- **Spec:** [§18 Proposed first ADRs](../../SPEC.md#18-proposed-first-adrs), [§13 Non-functional requirements](../../SPEC.md#13-non-functional-requirements) (NFR-16, NFR-25, NFR-26), [§14 What can go wrong](../../SPEC.md#14-what-can-go-wrong) (RISK-14)

## Context

QuestMaster is built by one person leading an AI coding agent. The MVP has about 45 stories (SPEC §6) and the whole thing has to reach a real table, so the cost of ceremony matters (RISK-14).

Three facts shape the layout:

1. **The rules engine must stay pure.** `QuestMaster.Rules` has no dependency on ASP.NET, EF Core or I/O, enforced by an architecture test (NFR-25).
2. **Correctness of the live table depends on database behavior.** A command's result, its roll or log entry, its event and the campaign's next `seq` are stored in one transaction under the campaign lock (SPEC §10, S-4 and S-5). Hiding that behind an abstraction would hide the thing that must be right.
3. **Integration tests run against a real PostgreSQL** (Testcontainers, NFR-26), not against a substitute.

The stack was proposed in `CLAUDE.md`. This ADR confirms it as a whole and decides how the .NET solution is cut into projects. Sub-decisions that have their own ADR stay there.

## Decision 1: Confirm the stack

| Part | Choice | Details decided in |
|---|---|---|
| Language and runtime | C# on .NET 10 | this ADR |
| Server | ASP.NET Core | this ADR |
| Live connection | SignalR | ADR-0006 |
| Persistence | EF Core + PostgreSQL (Npgsql provider) | ADR-0008 (JSONB document or tables) |
| Local orchestration | Aspire (AppHost + ServiceDefaults) | this ADR; hosting in ADR-0010 |
| Authentication | cookie session, Google/Discord OAuth | ADR-0009 |
| Frontend | Angular, shipped as a PWA | ADR-0012 (state, i18n, UI kit, Node version), ADR-0011 (PWA, M5) |
| Tests | xUnit v3, Testcontainers, Playwright for .NET (all test code is C#; TypeScript is only the Angular app) | ADR-0013 |
| CI | GitHub Actions | issue #6 |

Versions verified on 2026-10-28 (a snapshot, the pins live in `global.json`, `Directory.Packages.props` and `package-lock.json`): .NET SDK 10.0.403 (.NET 10 is supported until 2028-11-14), Aspire 13.6.4, Angular 22.2 (its CLI requires Node `^22.22.3 || ^24.15.0 || >=26`), Playwright 1.64.1, xUnit v3 4.0.2, Testcontainers 4.16.0.

## Decision 2: Solution layout

### Options considered

| | **A. Pure domain + one server project** | **B. Layered projects** | **C. One project per module** |
|---|---|---|---|
| Projects | `Rules`, `Server`, `AppHost`, `ServiceDefaults`, test projects | `Rules`, `Application`, `Infrastructure`, `Api`, `AppHost`, `ServiceDefaults`, test projects | `Rules` plus a project per module (Campaigns, Characters, Rolls, Sync...) |
| Boundary between server layers | folders, guarded by architecture tests and review | separate projects, guarded by the compiler | separate projects per module |
| Cost for a solo developer | lowest: one feature lives in one folder | higher: one feature touches 3-4 projects, with mapping between layers and interfaces that have a single implementation | high, and the module boundaries are not known yet |
| Fit with this system | keeps the transaction and lock logic visible | the persistence interface would hide the transaction semantics that S-4 and S-5 depend on, and the SPEC chose real-database tests over mocks, so the usual payoff of the extra layer (testing with a fake repository) does not apply | boundaries guessed too early |
| Reversibility | splitting a folder into a project later is cheap if the folder boundaries are clean | merging projects back is possible but pointless | merging is costly |

Option B's real benefits are replacing the database, supporting a second entry point to the same use cases (a background worker or a CLI), and compiler-enforced boundaries for a larger team. None of these applies today.

### Decision

**Option A.** The one boundary that matters most, the pure domain, is a project of its own, so the compiler enforces it. The server is one project organized in feature folders.

```
src/
  QuestMaster.Rules/             pure domain: game math, no ASP.NET, EF Core or I/O
  QuestMaster.Server/            ASP.NET Core: API, command pipeline, SignalR, persistence; feature folders
  QuestMaster.AppHost/           Aspire: starts the server, PostgreSQL and the web app locally
  QuestMaster.ServiceDefaults/   shared telemetry, health checks and resilience defaults
tests/
  QuestMaster.Rules.Tests/         unit tests and golden scenarios
  QuestMaster.Server.Tests/        integration tests (Testcontainers)
  QuestMaster.Architecture.Tests/  dependency rules
  QuestMaster.E2E.Tests/           browser tests (Playwright for .NET)
```

The Angular app is placed by ADR-0012.

Project references:

```
Rules               -> (nothing)
Server              -> Rules, ServiceDefaults
AppHost             -> Server (as an orchestrated resource)
Rules.Tests         -> Rules
Server.Tests        -> Server, Rules
Architecture.Tests  -> Rules, Server (inspects their assemblies)
E2E.Tests           -> AppHost (starts the app through Aspire.Hosting.Testing, see ADR-0013)
```

### Enforcement

- `QuestMaster.Rules` has no project references and no references to ASP.NET Core, EF Core, Npgsql, SignalR or I/O APIs. An architecture test fails the build otherwise (NFR-25). It is introduced in issue #5 and shown failing once before it passes.
- Boundaries between feature folders inside `Server` are written as architecture tests when the second feature exists. Until then they are a review rule: a feature does not use another feature's internal types.

### When to revisit

Move to option B (or split `Server` further) through a new ADR when any of these is true:

1. a second entry point, such as a background worker or a CLI, needs the same use cases;
2. a second developer joins and needs enforced ownership boundaries;
3. we must abstract or replace persistence;
4. the architecture tests keep being bypassed or disabled, which would show that discipline is not enough.

## Decision 3: Build setup and dependency policy

Build setup, created in issue #5:

- **Central Package Management:** all package versions live in `Directory.Packages.props`.
- **NuGet lock files** (`packages.lock.json`) and `dotnet restore --locked-mode` in CI, so CI installs exactly the reviewed versions. Not verified yet: whether Dependabot refreshes lock files. If it does not, issue #6 drops `--locked-mode` and records why.
- **`global.json`** pins the .NET SDK to 10.0.403 with `rollForward: latestPatch`.
- **`Directory.Build.props`:** nullable reference types on, warnings as errors; `.editorconfig` is followed (see `CLAUDE.md`).
- **Node** is pinned for the web client. The pinning mechanism is decided with the Angular shell (issue #11) and the major version in ADR-0012.

Dependency policy: **external packages and tools only where they earn their place.** A dependency is judged case by case on:

1. **License:** OSI-approved, and no history or risk of turning commercial.
2. **Maintenance:** a recent release or push, and an active issue tracker.
3. **Size of the problem solved:** a small dependency for a small problem is a liability, and we prefer to write and own it.
4. **Exit cost:** how hard it is to remove later.
5. **Layer:** `QuestMaster.Rules` takes no third-party dependencies.

This is a balance, not a ban. We do not hand-write security-critical or large, well-solved infrastructure (cryptography, OAuth handling, the ORM, SignalR). Each case is decided as it comes up and recorded in the pull request, or in an ADR when it is architectural.

Case that motivated this policy: FluentAssertions 8 and later is free for open-source and non-commercial use, but commercial use requires a paid license (per its README). We use the plain xUnit `Assert` instead (ADR-0013). Another case to be decided in issue #5: `NetArchTest` (last repository change in July 2024) versus `ArchUnitNET` (active) versus a hand-written reflection test.

## Consequences

**Positive**

- Fewer projects and less mapping code, so features are cheaper to build and review.
- A feature's code sits in one place, and the transaction and lock logic stays visible.
- The decision is reversible, and the conditions for reversing it are written down.

**Negative**

- The compiler does not enforce boundaries inside `Server`. We rely on architecture tests and review, and `Server` can degrade into a tangle if they are neglected.
- A reader who expects a layered structure will find this one unfamiliar. This ADR is the explanation.

**Follow-ups**

- Issue #5 creates the solution and the first architecture test.
- Issue #6 sets up CI and checks the Dependabot and lock-file question.
- Issue #5 compares architecture-test libraries.
