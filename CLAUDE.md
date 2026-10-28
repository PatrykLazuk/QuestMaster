<!--
Starter CLAUDE.md for QuestMaster. Keep it under ~100 lines.
Add a rule only when Claude repeats a mistake; delete rules Claude follows anyway.
Block-level HTML comments like this one are stripped before Claude reads the file.
-->

# QuestMaster

Web app for D&D 5e game masters and players: character sheets that compute stats and
modifiers automatically, dice rolls with advantage/disadvantage, live session sync across devices.
It is also my portfolio project and my practice in leading an AI coding agent: the collaboration rules below matter more than speed.

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

## Collaboration rules (I lead, you implement)

- When I ask a question in Polish, always respond in Polish; otherwise default to English.
- Always explain topics in plain, easy-to-understand language first, including the important and complicated ones.
- IMPORTANT: I am the tech lead. I decide architecture, patterns and scope, and I tell you concretely what to implement.
  You implement it, the domain included, and I review the result.
- If a task lacks scope or acceptance criteria, ask or propose them instead of guessing.
- For non-trivial changes, propose a plan first and wait for my approval.
- Stick to the agreed scope and keep diffs small. Report other issues you notice instead of fixing them.
- After each task, summarize what changed and why, deviations from the plan, and open questions.
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

- GitHub Flow: `main` is always releasable; all work happens on short-lived branches (1-3 days) cut from fresh `main`.
- One issue = one branch = one PR. Branches: `feat/<issue>-<slug>`, `fix/<issue>-<slug>`.
  Small docs/config changes need no issue: `docs/<slug>`, `chore/<slug>`.
- Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`).
- Never commit to `main` directly. Never force-push.
- Merge with squash only (the repo allows nothing else). The PR title becomes the commit on `main`,
  so it must be a valid Conventional Commit. Branches are deleted automatically after merge.
- I review and click "Squash and merge" myself. You open the PR (`gh pr create`) and write its description
  (summary, changes, verification, notes), but never merge it.
- Do not add a "Generated with Claude Code" footer to PR descriptions; AI involvement is documented
  in the README ("How this project is built") and in the `Co-Authored-By` commit trailer.

## Docs

- ADRs: `docs/adr/NNNN-title.md` (Context / Decision / Consequences).

## Compact instructions

When compacting, preserve: the current task and its issue number, modified files,
test commands, and open decisions.
