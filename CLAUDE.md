<!--
Starter CLAUDE.md for QuestMaster. Keep it under ~100 lines.
Add a rule only when Claude repeats a mistake; delete rules Claude follows anyway.
Block-level HTML comments like this one are stripped before Claude reads the file.
-->

# QuestMaster

Web app for D&D 5e game masters and players: character sheets that compute stats and
modifiers automatically, dice rolls with advantage/disadvantage, live session sync across devices.
It is also my learning and portfolio project: the collaboration rules below matter more than speed.

## Status

Early stage. Decisions live in `docs/adr/`. If a command or folder below doesn't exist yet,
ask me instead of inventing one.

## Stack (proposed, confirm in ADR-0001)

- .NET 10, ASP.NET Core, SignalR, EF Core + PostgreSQL, Aspire for local orchestration
- Tests: xUnit, Testcontainers (integration), Playwright (E2E)
- Frontend: Angular, shipped as a PWA
- CI: GitHub Actions

## Commands

- Build: `dotnet build`
- Tests: `dotnet test`
- Run locally: `aspire run`
- Format: `dotnet format`
<!-- TODO: add the command that runs only fast unit tests once test projects exist. -->

## Architecture

- Modular monolith. No microservices unless an ADR says so.
- `src/QuestMaster.Rules` is the pure domain (rules engine): no ASP.NET, EF or I/O.
  All game math (ability modifiers, proficiency, advantage/disadvantage, item effects) lives there.
- Game content comes from SRD 5.2 (CC-BY-4.0) only, with attribution.
  Never paste text from other D&D books.

## Collaboration rules (learning mode)

- When I ask question in Polish you always respond in Polish. If not aways default to English.
- Always explain topics in very easy to understand language. Will disscuss very important and complicate topics bu you will always first explain them in easy language.
- IMPORTANT: In `src/QuestMaster.Rules` I write the production code. You may explain,
  propose 2-3 approaches with trade-offs, write failing tests when I ask, and review my code.
  Implement domain logic only when I explicitly say "implement it".
- Outside the domain you may implement, but keep diffs small and explain non-obvious choices.
- For non-trivial changes, propose a plan first and wait for my approval.
- When I ask "why", give trade-offs and alternatives, not just the chosen option.
- If you think I'm wrong, say so and name the risk. If something is ambiguous, ask.

## Testing rules

- Red/green TDD for domain code: write the test, confirm it fails, then implement.
- IMPORTANT: Never weaken, skip or delete a test, or change an expected value, to make it pass.
  If you believe a test is wrong, stop and explain why.
- Never hard-code values just to satisfy tests.
- When you claim something works, show the command you ran and its result summary.

## Code style

- Follow `.editorconfig`; run `dotnet format` before committing.
- Nullable reference types on; warnings as errors.
- In the domain, prefer small value objects (e.g. `AbilityScore`, `Modifier`) over primitives.
- Code, comments, commit messages and docs in English.

## Git

- One issue = one branch = one PR. Branches: `feat/<issue>-<slug>`, `fix/<issue>-<slug>`.
- Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`).
- Never commit to `main` directly. Never force-push.

## Docs

- ADRs: `docs/adr/NNNN-title.md` (Context / Decision / Consequences).
- `docs/learning-log.md` is my personal learning log. Don't edit it unless I ask.

## Compact instructions

When compacting, preserve: the current task and its issue number, modified files,
test commands, and open decisions.
