# ADR-0003: Rules engine boundary and roll pipeline

- **Status:** Accepted
- **Date:** 2026-11-02
- **Decider:** PatrykLazuk (tech lead)
- **Issue:** #7
- **Spec:** [§18 Proposed first ADRs](../../SPEC.md#18-proposed-first-adrs), [D2 d20 resolution, D3 Automatic failures, D4 Damage rolls, D5 Manual dice, D7 Server dice](../../SPEC.md#6-user-stories-and-acceptance-criteria), [§7.7 Golden scenarios](../../SPEC.md#77-golden-scenarios), [§10 Live sync behavior](../../SPEC.md#10-live-sync-behavior) (S-4, S-6), [§13 Non-functional requirements](../../SPEC.md#13-non-functional-requirements) (NFR-25)

## Context

All game math lives in `QuestMaster.Rules`, and that project stays pure: no ASP.NET, EF Core or I/O (NFR-25, ADR-0001). Rolling dice is the one place where the game needs something the pure domain does not have: randomness. This ADR decides how dice values reach the domain.

The requirements that shape the answer:

- **Server dice (D7).** They come from a cryptographically secure generator (`RandomNumberGenerator.GetInt32(1, sides + 1)`). A roll is stored before anyone sees it, and retrying the same command returns the same roll instead of rolling again (D7.3, S-4, S-6). All dice happen on the server (ADR-0005).
- **Manual dice (D5).** A player types in what they rolled on physical dice. For a d20 roll that is one or two faces (1–20 each). For damage it is **the total of the dice**, checked against the possible range (2d6: 2–12). For a death save it is one d20, and for a Hit Point Die one value. The app then computes the kept die, the total and crit/miss **exactly as with server dice** (D5.2). The GM can switch manual dice off, and the server then rejects them (D5.4).
- **Dice in tests.** Domain and integration tests use deterministic dice. The production generator is used only by the uniformity test and a smoke test (ADR-0013, Decisions 4 and 5). ADR-0013 left the placement of the dice source to this ADR.
- **The roll record** keeps every die face and which one was kept (D2.4).
- **Some rolls need no dice at all:** an automatic failure (D3, GS-25) and Unarmed Strike damage, where a critical hit has no dice to double (D4.4, GS-17).

## Options considered

In plain words, the rules engine is the scorekeeper. The question is where the scorekeeper gets the dice values from.

- **A. Plan, then dice, then resolve.** The domain says what a roll needs ("two d20s, because of advantage"). Someone outside the domain provides the values: the server's generator or the player's typed-in faces. The domain then computes the result from those values.
- **B. Injected random source.** The domain receives a random-number interface and rolls by itself. Tests pass a fake that returns fixed values.
- **C. Modifiers only.** The domain returns the modifier and the advantage mode. The application layer rolls, picks the kept die and adds the numbers.

Sketches, for illustration only (names are settled in issue #9):

```csharp
// A
var plan = D20Roll.Plan(RollMode.Advantage);                    // needs 2 × d20
var result = D20Roll.Resolve(plan, modifier, faces: [7, 17]);  // keeps 17

// B
var result = D20Roll.Roll(RollMode.Advantage, modifier, dice); // dice: IDice

// C
var spec = D20Roll.Spec(modifier, RollMode.Advantage);          // the server rolls, keeps and adds
```

### Where dice come in

| | **A. Plan → dice → resolve** | **B. Random source in the domain** | **C. Modifiers only** |
|---|---|---|---|
| Server dice (D7) | the server rolls the dice the plan asks for and passes the values to resolve | a cryptographic implementation of the interface is injected into the domain | the server rolls and computes the result |
| Manual dice (D5) | the values from the command go into **the same** resolve | a fake random source that returns the typed-in values, which depends on the order in which the domain asks for dice; a manual damage total does not fit a source of single dice, so it needs a second path | the server computes the result |
| Test dice | the values are written in the test as data: `[17]` | a fake random source, again coupled to the order of calls | game math is tested in the server's tests |
| Cryptographic generator | `QuestMaster.Server` | `QuestMaster.Server`, implementing an interface declared in `QuestMaster.Rules` | `QuestMaster.Server` |
| Storing the faces for retries (D7.3) | the faces are plain input data, so they are stored with the roll | needs a wrapper around the source that records what was drawn | the server stores them |
| Same input, same output in the domain | yes | no | yes, but the domain no longer does the roll math |

### Trade-offs

- **A** gives one resolution path for server and manual dice, so D5.2 holds by construction. Tests and golden scenarios are plain data, and the domain needs no interface. The cost is two calls instead of one, and the server must provide exactly the dice the plan asks for. Rerolls would need a second round. They are in the backlog (SPEC §16: Heroic Inspiration p.8, Halfling Luck p.86).
- **B** is a single call and a well-known pattern. But manual dice have to pretend to be a random source, and manual damage (a total, not single dice) does not fit, which creates a second path where server and manual results could drift apart. The domain also stops being deterministic, and tests depend on the order in which dice are drawn.
- **C** keeps the domain smallest, but moves game math out of `QuestMaster.Rules`: picking the kept die for advantage and disadvantage, critical hits and misses, doubling damage dice on a crit, the Exhaustion penalty and the damage minimum of 0. That breaks the rule that all game math lives in the domain, and golden scenarios such as GS-17 and GS-18 would be tested in the server.

## Decision

**Option A: plan, then dice, then resolve.**

1. **The domain plans.** For each roll the domain describes which dice are needed (how many and how many sides, possibly none) together with what it needs to compute the result (modifier, mode and roll type).
2. **The domain resolves.** Given a plan and the dice values, the domain computes the result: every die face, which one is kept, the total and the flags (critical hit, miss). The same function is used for server dice and manual dice. The dice source (`server` or `manual`) is recorded in the result as data and does not change the math.
3. **Resolve validates the values.** It rejects the wrong number of dice and values out of range: a d20 face outside 1–20, or a manual damage total outside the range of the plan's dice. This is also the validation of manual dice. The form of the rejection is chosen in issue #9 and aligned with ADR-0007 (results or exceptions for rejected commands).
4. **Dice values have two forms.** Single faces, used for d20 rolls and all server dice, and a total, used only for manual damage (D5.1). A plan that needs no dice is resolved without values (D3, GS-17).
5. **`QuestMaster.Rules` contains no randomness:** no `System.Random`, no `RandomNumberGenerator` and no random-source interface. An architecture test enforces this.
6. **The server provides the values.** The Rolls feature in `QuestMaster.Server` asks the domain for the plan, then takes the values either from the command (manual dice, only when the campaign allows them, D5.4) or from the server dice roller (D7). It then asks the domain to resolve, and stores the roll before acknowledging the command (D7.3, S-4).
7. **The server dice roller** is one interface in the server's Rolls feature with two implementations: the production one using `RandomNumberGenerator.GetInt32(1, sides + 1)`, and a deterministic one registered by the integration tests. Two real implementations justify the interface (ADR-0014). Manual dice do not go through it, since they are values from the command. The names are settled in issue #9.
8. **Where the dice tests live.** Domain tests pass dice values as data and need no fake. The uniformity test (ADR-0013, Decision 5) and the smoke test of the production roller live in `tests/QuestMaster.Server.Tests`. Issue #9 creates that project, without a database. Testcontainers is added later in issue #14.

How one roll flows through the layers:

```
roll.d20 command → QuestMaster.Server, Rolls feature
  1. plan    = Rules: plan the roll                 (normal mode → 1 × d20)
  2. values  = manual values from the command       (D5, when allowed)
               or the server dice roller            (D7, cryptographic)
  3. result  = Rules: resolve the plan with values  (validates, computes)
  4. store the roll and its event in one transaction, then acknowledge (S-4)
```

## Consequences

**Positive**

- Server and manual dice share one resolution path, so they cannot compute differently (D5.2).
- The domain is deterministic: the same plan and the same values always give the same result. Golden scenarios read as data, for example GS-18 "die shows 1".
- The faces are ordinary data. They are stored with the roll, a retry returns the stored roll (D7.3, S-6), and the record shows every die (D2.4).
- No interface in the domain. The only interface is in the server, where it has two real implementations.
- Rolls without dice (automatic failures, Unarmed Strike damage) are plans that need no dice, not a special path.

**Negative**

- A roll takes two calls into the domain instead of one, and the server must provide exactly the dice the plan asks for. Resolve rejects anything else, so a mistake fails loudly instead of producing a wrong number.
- Dice values come in two forms (faces or a total). A manual damage roll records only the total, not single dice.
- Rerolls (backlog) do not fit a single plan and resolve. They would need a second round, for example a result that asks for one more die.
- `tests/QuestMaster.Server.Tests` is created earlier than planned (issue #9 instead of #14).

**Revisit when**

A rule enters the scope in which the number of dice depends on dice already rolled (rerolls, exploding dice), and a second round of plan and resolve becomes hard to read. Then compare it with option B for those roll types only. The server keeps the cryptographic generator in any case (ADR-0005).

**Follow-ups**

- Issue #9: plan and resolve for a normal d20 check; the production server dice roller; `tests/QuestMaster.Server.Tests` with the uniformity and smoke tests; an architecture test that forbids randomness in `QuestMaster.Rules`. Done; see the decisions below.
- Issue #24: the `roll.d20` command uses this flow. In M0 commands carry no dice values (D7.2). It also adds the server dice roller interface and its deterministic implementation (moved from #9, see below).
- M2: advantage and disadvantage (two d20s), damage plans with critical hits, and manual dice (D5), including the manual damage total.

## Decisions made during implementation

Each task starts without the discussion that led to it, so decisions that settle, defer or change a point of this ADR are recorded here.

### Issue #9 (2026-11-02)

- **Shape of the code (Decisions 1 and 2).** `new D20RollPlan(modifier)` holds everything the result needs. `plan.DiceNeeded` tells the server which dice to roll, for example `DiceNeeded(Count: 1, Sides: 20)`. `plan.Resolve(faces)` returns a `D20RollResult` with every `RolledDie`, the kept die, the modifier and the total. This replaces the static `D20Roll.Plan(RollMode…)` sketch above: the modifier belongs in the plan, as Decision 1 says, and a `RollMode` with only `Normal` would be speculative (ADR-0014). M2 adds the mode as a constructor parameter that defaults to normal, and the roll type when critical hits need it (D2.3).
- **Form of the rejection (Decision 3).** `Resolve` throws: `ArgumentException` for the wrong number of faces and `ArgumentOutOfRangeException` for a face outside the die's range. In M0 faces come only from the server dice roller, so an invalid face is a bug, as in `AbilityScore`. Revisit together with ADR-0007 when manual dice (M2) bring faces typed in by a player.
- **Dice source in the result (Decision 2)** is not added yet: with server dice only it would have a single value. It comes with the roll record in #24 or with manual dice in M2, whichever is first.
- **Server dice roller (Decision 7).** #9 adds only the concrete production class `DiceRoller` in `QuestMaster.Server/Rolls`. The interface and the deterministic implementation move to issue #24, where integration tests first use them. In #9 nothing would use the interface, which is the speculative abstraction ADR-0014 rules out.
