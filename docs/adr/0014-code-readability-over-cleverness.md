# ADR-0014: Code readability over cleverness

- **Status:** Accepted
- **Date:** 2026-10-29
- **Decider:** PatrykLazuk (tech lead)
- **Issue:** #4 (added at the tech lead's request while ADR-0001, ADR-0005 and ADR-0013 were in review)
- **Spec:** [§2 Goals](../../SPEC.md#2-goals) (G-6), [§14 What can go wrong](../../SPEC.md#14-what-can-go-wrong) (RISK-06, RISK-14)

## Context

Code outlives the memory of its author. In a large project it gets hard, a year later, to tell what the author meant, and every fix or extension starts with reverse engineering.

Two things make this matter here:

- **A human reviews and maintains everything.** The tech lead reads every change, fixes bugs and extends features. Code the human cannot follow quickly is a cost on every future task.
- **An AI agent writes most of the code.** An agent can produce dense, over-general or "impressive" code very fast. Without a written principle the default drifts toward cleverness and speculative abstraction. RISK-06 (the effect engine growing without limit) and RISK-14 (solo-developer time) are the same pressure seen from the design side.

`CLAUDE.md` currently covers formatting, nullable reference types, warnings as errors and value objects. It says nothing about readability itself.

## Options considered

| | **A. Written principle with concrete rules and a review test** | **B. No written principle: `.editorconfig`, analyzers and review only** | **C. Mechanical limits as the main tool (maximum method length, cyclomatic complexity)** |
|---|---|---|---|
| What it gives | a shared definition of "readable", usable by the reviewer and by the agent | zero effort | an objective number that CI can check |
| Weakness | needs judgement in review | the standard stays implicit, and cleverness and speculative abstraction are not caught by a formatter | metrics are gamed (a long method becomes five short ones that are harder to follow) and do not measure clarity |
| Fit | covers the real problem: understanding, fixing and extending | misses the problem | catches only the crudest form of it |

## Decision

**Option A.** Mechanical limits (option C) may be added later as hints, never as the definition of readability.

**The principle.** Code is written first for the next human who has to read, fix and extend it, including its author in a year. When readability conflicts with brevity, cleverness, performance tricks or generality, readability wins. Correctness comes before all of them, because a readable bug is still a bug.

**The rules.**

1. **Explicit over clever.** Plain statements and straightforward control flow beat dense one-liners. A LINQ chain, a nested ternary or a pattern-matching expression that has to be read twice is rewritten as simple steps with named intermediate values. No reliance on obscure language features, operator precedence or implicit conversions.
2. **Names carry meaning.** Full words, in the vocabulary of the SPEC glossary and the SRD (Campaign, Command, Breakdown, Proficiency Bonus), so a reader can look a term up. Abbreviations only when they are established in the domain (HP, AC, DC, SRD).
3. **No generalization before it is needed.** No interface with a single implementation, no generic base class and no configuration option "for later". Generalize when the second real use appears, and prefer a little duplication over the wrong abstraction. Exceptions are seams decided in an ADR, for example the dice source in ADR-0003.
4. **Patterns only for a problem you can name.** A design pattern is used when it removes a concrete difficulty and a requirement and a test justify it. It is not used for its own sake or "in case".
5. **A request can be followed with Go to Definition.** A reader can trace a request from its entry point to the database by jumping to definitions in the IDE. Hidden wiring (reflection, convention-based discovery, behaviors that run "somewhere") is used only when its benefit is clear and is recorded in the ADR that chose it. ADR-0007 (a mediator or plain handlers) applies this rule.
6. **Small units with one job.** A method or class does one thing that can be named. One that does not fit on a screen, or needs "and" to describe, is a reason to take a look, not an automatic failure.
7. **Comments explain why, not what.** They give the rule and its source (SRD page, ruling R-n), a trade-off or a non-obvious constraint. A comment that explains what the code does is a sign to rename or restructure.
8. **Performance tricks need a measurement.** A less readable optimization is allowed only with a measured reason, written in a comment next to it.
9. **Tests read like documentation.** Test names describe behavior, and golden-scenario tests read like the SRD example they come from (ADR-0013).

**How it is checked.**

- **Cold read.** The tech lead must be able to understand each changed unit by reading it, without the author's explanation. If an explanation is needed, the code (names, structure, comments) is changed. The explanation does not live only in the pull request text.
- **Go to Definition.** For any new flow, the reviewer can follow it from entry point to database by jumping to definitions.
- **Mechanical part.** Naming, formatting, nullable and warnings as errors are enforced by `.editorconfig` and the compiler (see `CLAUDE.md`). The judgement part is reviewed by a human, and no metric replaces that.
- **Agent.** The rule is written into `CLAUDE.md`, so the coding agent writes for the reviewer and treats "the tech lead had to ask what this does" as a defect.

## Consequences

**Positive**

- Reviews are faster, bugs are easier to find, and extending a feature does not begin with deciphering it.
- The code stays understandable to a human even though most of it is written by an agent.
- The rule gives a clear answer when a clever or general solution is tempting.

**Negative**

- More lines and some duplication.
- The code may look less "advanced" to a reader who expects patterns and layers. This ADR is the explanation, and patterns still appear wherever they remove a real problem.
- The rule depends on review discipline. The cold read and Go to Definition checks are concrete enough to apply, but nothing automatic guards them.

**Revisit when**

- the same logic has been copied into several places (a sign that it is time to generalize), or
- reviews turn into repeated disputes about what "readable" means (then consider analyzer thresholds as hints).

**Follow-ups**

- Add a short pointer to this rule in the "Code style" section of `CLAUDE.md`, as a separate small docs pull request.
- Issue #6: add a readability item to the Definition of Done in the pull request template.
- ADR-0007: apply rule 5 when comparing a mediator with plain handlers.
