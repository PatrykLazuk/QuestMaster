# ADR-0013: Testing strategy

- **Status:** Accepted
- **Date:** 2026-10-29
- **Decider:** PatrykLazuk (tech lead)
- **Issue:** #4
- **Spec:** [§7.7 Golden scenarios](../../SPEC.md#77-golden-scenarios), [D7 Server dice](../../SPEC.md#6-user-stories-and-acceptance-criteria), [§13 Non-functional requirements](../../SPEC.md#13-non-functional-requirements) (NFR-24, NFR-25, NFR-26), [§14 What can go wrong](../../SPEC.md#14-what-can-go-wrong) (RISK-05, RISK-15)

## Context

The product's value is correct numbers (G-1) and a live table that does not lose or duplicate actions (G-2, G-3). The project rules require red/green TDD for domain code, tests that are never weakened, and proof (a command and its result) for every claim that something works.

The SPEC already fixes some requirements: every rule row in §7.1 has at least one test that names its SRD page, and every golden scenario is an automated test (NFR-24); CI runs build, format check, unit, integration (Testcontainers) and E2E (Playwright) tests on every pull request (NFR-26); real-time tests must not be flaky (RISK-15).

This ADR decides the layers, the tools, the conventions and the policy for the one probabilistic test (dice uniformity).

## Decision 1: Test layers

| Layer | Project or location | What it proves | How many |
|---|---|---|---|
| Unit | `tests/QuestMaster.Rules.Tests` | rules, golden scenarios, edge cases, pure and fast | the majority |
| Architecture | `tests/QuestMaster.Architecture.Tests` | dependency rules, for example `Rules` stays pure (NFR-25) | few, cheap |
| Integration | `tests/QuestMaster.Server.Tests` | the command pipeline, transactions, `seq`, idempotency, authorization, the visibility filter, against a real PostgreSQL | a focused set |
| End-to-end | `tests/QuestMaster.E2E.Tests` (Playwright for .NET on xUnit v3) | whole flows across browser, server, database and SignalR | very few |
| Web unit | inside the Angular app (TypeScript, runner chosen in ADR-0012) | components and services of the frontend, no hard-coded UI text | per component |

All test code outside the Angular app is C#. TypeScript is used only for the Angular frontend, including that frontend's own unit tests.

End-to-end tests are reserved for scenarios that cross all the layers, such as the M0 exit scenario, the GM-only leak check (M2) and the offline catch-up (F6.5). They are slow and more fragile than the other layers, so a rule that can be tested lower is tested lower. Load and soak tests (M5) are out of scope here and will be decided in M5.

Small input ranges are tested exhaustively instead of with random data. For example, GS-01 loops over every score from 1 to 30.

## Decision 2: Unit and integration tooling

### Options considered

| Decision | Options | Chosen |
|---|---|---|
| Test framework | **xUnit v3** 4.0.2, Apache-2.0, named in project rules; **TUnit** 1.76.x, MIT, newer; **NUnit** 5.0.0 | **xUnit v3**, for every .NET test project including the end-to-end tests |
| Assertions | **plain xUnit `Assert`**; **Shouldly** 4.3.0, BSD-3-Clause; **AwesomeAssertions** 9.6.0, Apache-2.0, a fork of FluentAssertions; **FluentAssertions** 8.11.0, free for open source and non-commercial use, commercial use needs a paid license (per its README) | **plain `Assert`** (no extra dependency, see the dependency policy in ADR-0001; add Shouldly later if it clearly pays off) |
| Database in integration tests | **Testcontainers** for PostgreSQL 4.16.0, MIT; **the full AppHost through `Aspire.Hosting.Testing`** 13.6.4; **an in-memory or SQLite substitute** | **Testcontainers.** A substitute is rejected because it has different transaction and locking semantics and would hide exactly the bugs RISK-03 describes |

The test runner mode (VSTest or Microsoft.Testing.Platform) is chosen in issue #5. In the .NET 10 SDK, `dotnet test` supports the Microsoft.Testing.Platform mode through an opt-in in `global.json`, and then all test projects must use it.

Integration tests start one PostgreSQL container per test run. The PostgreSQL major version is aligned with production (ADR-0010). Isolation per test class and the host setup are settled in issue #14.

## Decision 3: Naming and traceability

- Test names describe the behavior.
- A test for a golden scenario carries `[Trait("GS", "GS-01")]`, and every test that implements a rule carries `[Trait("SRD", "p.6, p.187")]`. This makes NFR-24 searchable and filterable without custom tooling.
- A check that every golden scenario already due in the current milestone has a test is a candidate for M1 and is not part of M0.
- Red/green TDD: for domain code the pull request history shows the failing test before the implementation.
- A test is never weakened, skipped or deleted, and an expected value is never changed, to make it pass. A test that looks wrong stops the work and is discussed first.

## Decision 4: Dice in tests

All domain and integration tests use a deterministic dice source. The production generator (`RandomNumberGenerator.GetInt32(1, sides + 1)`, per D7.1) is used only by the uniformity test below and by a smoke test. Where the dice source sits relative to the domain is decided in ADR-0003.

## Decision 5: Dice uniformity test

A chi-square goodness-of-fit test on the production d20 generator.

- **Sample:** 200,000 rolls. The expected count per face is 10,000.
- **Statistic:** chi-square with 19 degrees of freedom. The test fails if it is above **69.78**.
- **False-failure rate:** for a fair die this threshold is exceeded with probability 10⁻⁷ per run. The SPEC requires less than 10⁻⁶, which would be a threshold of 63.68, so this is ten times stricter at no real cost. At 100 CI runs a day that is one false failure in about 270 years.
- **Sensitivity:** a die whose "20" comes up 6% of the time instead of 5% is detected practically always (non-centrality about 421). One at 5.1% is not (about 4.2). The test is meant to catch gross mistakes such as a wrong range or mapping. Fine bias is excluded by relying on the .NET generator method, not by this test.
- **Policy:** a failure is a signal to investigate, not to re-run. The test has no automatic retry.
- **Provenance:** the critical values were computed on 2026-10-28 with a hand-written incomplete-gamma implementation and cross-checked by numerical integration of the chi-square density: P(chi-square > 63.68) = 9.99 x 10⁻⁷ and P(chi-square > 69.78) = 1.00 x 10⁻⁷ with 19 degrees of freedom. The sensitivity figure for a "20" at 5.1% (detected with probability about 7 x 10⁻⁶) comes from the same check. The implementing issue (#9) keeps the derivation next to the test.

## Decision 6: Property-based tests

Deferred to M1. They become useful when the invariant "the active breakdown lines add up to the shown value" (B11.3) must hold for many generated characters. Candidates then: FsCheck 3.6.0 and CsCheck 5.0.0, chosen under the dependency policy of ADR-0001.

## Decision 7: End-to-end tests

### Options considered

| Decision | Options | Chosen |
|---|---|---|
| Language and runner | **.NET:** Playwright for .NET with the base classes Microsoft provides for xUnit v3 (`Microsoft.Playwright.Xunit.v3` 1.64.1, MIT); **TypeScript:** `@playwright/test`, Playwright's native runner | **.NET.** Backend, integration and end-to-end tests are all C# on one runner (`dotnet test`), and seeded users, helpers and contract types can be shared between the layers |
| Starting the app | **`Aspire.Hosting.Testing`:** a test starts the AppHost in-process (a .NET library, so only reachable from C# tests); **an external process** started by a script or a CI step; **docker compose** | **`Aspire.Hosting.Testing`.** The test controls the lifecycle of the app and runs it the way it runs locally |

Why not TypeScript: Playwright's native runner is the best-documented path and is likely to have more conveniences (UI mode, built-in retry, sharding and project configuration; from general knowledge, not compared in detail). Against that, it adds a second language to the test stack and cannot use `Aspire.Hosting.Testing`.

Conventions:

- Each user or device is its own browser context, so one test can drive a GM and several players, and can take one context offline. The Microsoft base classes give each test its own page and context.
- The default context uses a phone viewport of 360 x 640.
- The app runs in the Test environment (the dev login exists there, A1.5) with a fresh database for each run.
- Default: one app instance per test run, shared by the end-to-end tests through a fixture, and every test creates its own campaign and users. The exact setup is settled in issue #20.
- Locators are role- or test-id-based, and waits use Playwright's auto-waiting assertions, never fixed delays.
- xUnit runs test classes in parallel and the tests inside a class one after another. The degree of parallelism for this layer is set in issue #20.
- No automatic test retries (RISK-15): no retry mechanism is added. A flaky test is a bug and is fixed, or removed from the required set with an explanation.
- Tracing is on. On failure the Playwright trace is saved and uploaded as a CI artifact.
- CI installs the Playwright browsers before the job runs the tests.

Open points to confirm in issue #20, none of which I have verified yet:

- `Microsoft.Playwright.Xunit.v3` depends on `xunit.v3.extensibility.core` version 1.0.1 or newer, and we use xUnit v3 4.0.2. Compatibility must be confirmed.
- I found no documented example of `Aspire.Hosting.Testing` combined with Playwright. It should work because the app is plain HTTP, and the fallback is an external process.
- Whether the browser tests run against the Angular dev server or a production build served from the same origin as the API depends on ADR-0012 and issue #11.
- The M5 scenario "after a server restart all clients catch up" (F6.6) means stopping and starting the app during a test. Controlling the app from the test process should make this easier than from another language, to be confirmed in M5.

## Decision 8: What CI runs

| Job | Added by | Runs |
|---|---|---|
| `build-and-test` | issue #6 | restore, build, format check, unit and architecture tests |
| `web` | issue #11 | `npm ci`, lint, web unit tests, production build |
| `integration` | issue #14 | integration tests with Testcontainers |
| `e2e` | issue #20 | Playwright for .NET tests, after installing the browsers |

Each layer is added to CI by the issue that introduces it, and the required checks in the `protect-main` ruleset follow.

## Consequences

**Positive**

- Fast feedback where most rules live, and few slow tests.
- Traceability from SRD page and golden scenario to test, without custom tooling.
- The only probabilistic test has a documented, very low false-failure rate and a clear failure policy.
- Few dependencies in the test stack.
- One language and one runner for everything except the Angular frontend's own tests.

**Negative**

- Plain `Assert` is less expressive than an assertion library, which may make long assertions noisy.
- Playwright's TypeScript runner and its community examples are not available for the end-to-end layer, and the .NET path has fewer examples (see Decision 7).
- Integration and end-to-end tests need Docker locally and in CI, and the end-to-end tests also need the Playwright browsers.
- Without retries, a genuinely flaky end-to-end test blocks work until it is fixed.

**Follow-ups**

- Issue #5: test projects, runner mode and the first architecture test.
- Issue #9: uniformity test, with the derivation next to it.
- Issue #14: Testcontainers setup.
- Issue #20: the `tests/QuestMaster.E2E.Tests` project, how it starts the app through `Aspire.Hosting.Testing`, and the open points above.
