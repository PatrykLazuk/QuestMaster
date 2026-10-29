# QuestMaster

A phone-first web app for 5E tabletop sessions played around a real table. It's a smart character sheet that calculates every number for you, dice rolls with advantage and disadvantage, and a live table where everyone's rolls and HP changes show up on every phone within a second.

> **Status:** early stage. The MVP spec is done; implementation hasn't started yet.

## What it will do (MVP)

- **Smart character sheet:** enter your ability scores, class, level and gear. The app computes modifiers, saves, skills, AC, HP and attack bonuses, and can explain every number line by line.
- **Rolls:** checks, saves, initiative, attacks and damage, with advantage/disadvantage applied by the rules. Use the app's dice or type in what your physical dice show.
- **Live table:** a shared roll feed, a GM dashboard with the whole party, and GM actions like damage and conditions. Phones that go to sleep catch up automatically.

## How this project is built

QuestMaster is built with an AI coding agent (Claude Code) under human technical leadership.
The tech lead decides architecture, scope and acceptance criteria; the agent implements;
every change lands through a reviewed pull request. This is agentic engineering, not vibe coding:

- Plan first for non-trivial changes, then small, reviewable steps.
- Red/green TDD for the domain; tests are never weakened to make them pass.
- One issue = one branch = one PR, squash-merged after human review.
- Decisions are recorded in ADRs (`docs/adr/`).
- Commits where the agent contributed carry a `Co-Authored-By` trailer.

The working rules for the agent are in [CLAUDE.md](CLAUDE.md).

## Docs

- [SPEC.md](SPEC.md): the MVP specification (user stories, rules with SRD page references, sync design, milestones)
- [docs/adr/](docs/adr/): architecture decision records
- [CLAUDE.md](CLAUDE.md): how I work with Claude Code on this project (I lead and review, Claude implements)

## Getting started

Prerequisites:

- [.NET SDK 10.0.403](https://dotnet.microsoft.com/download/dotnet/10.0) or a later 10.0.4xx patch (pinned in `global.json`)
- [Aspire CLI](https://aspire.dev) 13.6

Commands, run from the repository root:

| What | Command |
|---|---|
| Build | `dotnet build` |
| Run the tests | `dotnet test` |
| Check formatting | `dotnet format --verify-no-changes` |
| Run the app locally | `aspire run` |

`aspire run` starts the API and prints a link to the Aspire dashboard (logs, traces and health of every part).
The API answers its health check at http://localhost:5118/health.

## Tech stack (planned)

.NET 10 · ASP.NET Core · SignalR · EF Core + PostgreSQL · Aspire · Angular (PWA) · xUnit · Testcontainers · Playwright · GitHub Actions

## License

Code: [MIT](LICENSE). Game content from the SRD stays under CC-BY-4.0 (see below).

## Legal

QuestMaster is 5E compatible and uses rules only from the System Reference Document 5.2.1.

This work includes material from the System Reference Document 5.2.1 (“SRD 5.2.1”) by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2.1 is licensed under the Creative Commons Attribution 4.0 International License, available at https://creativecommons.org/licenses/by/4.0/legalcode.
