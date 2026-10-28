# ADR-0005: Server-authoritative math and dice

- **Status:** Accepted
- **Date:** 2026-10-29
- **Decider:** PatrykLazuk (tech lead)
- **Issue:** #4
- **Spec:** [Appendix A, decision 18](../../SPEC.md#appendix-a-interview-decision-log), [D7 Server dice](../../SPEC.md#6-user-stories-and-acceptance-criteria), [F2 Sheet sync](../../SPEC.md#6-user-stories-and-acceptance-criteria), [§13 Non-functional requirements](../../SPEC.md#13-non-functional-requirements) (NFR-02), [§14 What can go wrong](../../SPEC.md#14-what-can-go-wrong) (RISK-05, RISK-11)

## Context

Two goals pull on where calculation and dice happen:

- **G-1 Correct math** and **G-4 Trustworthy dice.** Every derived number follows the SRD and the rulings, and app dice cannot be forged or re-rolled by a client (SPEC §2).
- **A live table on flaky phone Wi-Fi.** Editing a sheet should feel immediate (NFR-02: an updated sheet on the author's device in at most 500 ms, p95).

The rules engine (`QuestMaster.Rules`) is written in C# and tested there. The question is whether it is the only place where numbers are computed, and whether anything dice-related is ever decided on a device.

This decision was already taken in the spec interview (Appendix A, decision 18: "On the server; computed sheets are pushed to clients"). This ADR records the options and the consequences.

## Options considered

| | **A. Server only** | **B. Rules engine compiled to WebAssembly** | **C. TypeScript duplicate of the formulas** |
|---|---|---|---|
| Where numbers are computed | the server | the server, and the browser runs the same engine | the server, and the browser runs a second implementation |
| Instant feedback on edit | no, it waits for the server (pending indicator) | yes | yes |
| Risk of two versions of the truth | none | low (same code), but the client copy must stay in sync with the server version | high: two implementations of the same rules drift apart (RISK-05) and every rule is tested twice |
| Cost | lowest | the .NET runtime is added to the client download (size not measured), and the Angular-to-WebAssembly interop is extra work | the whole rules engine is written and maintained twice |
| Dice | server | server (the browser is not trusted for dice either way) | server |

## Decision

**Option A: all calculation and all dice happen on the server.**

- Derived values are computed only by `QuestMaster.Rules` on the server and pushed to clients as a full computed sheet with a `version` (SPEC §10.3, `CharacterSheetChanged`). The client contains no game math.
- Server dice come from a cryptographically secure generator. A client never sends dice values, with one exception: manual (physical) dice (D5). Manual values are validated on the server, stored with `diceSource: manual` and shown as such everywhere. They are a matter of trust at the table: the GM can switch them off (A3).
- The device that edits a sheet shows a small pending indicator until the updated sheet arrives (F2.3). There is no optimistic or predicted math on the client.
- A sheet is recomputed from its inputs and state on each change. Derived values are not stored (SPEC §11).

## Consequences

**Positive**

- One implementation of the rules, tested once, in one language.
- A client cannot forge or re-roll a result. A roll is stored before anyone sees it (D7.3).
- Clients are thin: they render what the server sends.

**Negative**

- Every input edit and every roll needs a round trip. On weak Wi-Fi this is visible (RISK-11). The pending indicator and the "not Live, so actions are disabled" rule (F6.2) make it understandable instead of hiding it.
- Offline the app is read-only, using the last snapshot (F9).
- The server does the work for all clients. The load is small (at most 10 members per campaign, NFR-05), and this is measured in the load test planned for M5.

**Revisit when**

Measured p95 for command to updated sheet exceeds 500 ms in a real session and the pending indicator is judged unacceptable. Then consider option B for a non-authoritative live preview only. Dice stay on the server in any case.
