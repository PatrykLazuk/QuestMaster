# QuestMaster — MVP Specification

| | |
|---|---|
| **Status** | Draft v0.1, for review |
| **Date** | 2026-10-18 |
| **Owner** | @PatrykLazuk |
| **Rules baseline** | System Reference Document **5.2.1** (2024 rules), CC-BY-4.0. Attribution in [§19](#19-attribution) |
| **Scope** | MVP only. Everything else is in [§16 Out of scope / backlog](#16-out-of-scope--backlog) |

**How to read this document**

- **Must** means the MVP doesn't ship without it. **Should** means it's planned for the MVP but is the first to go if time runs short (see the cut line in [§15](#15-milestones-and-cut-line)).
- **SRD p.N** is the printed page number in the official SRD 5.2.1 PDF (<https://www.dndbeyond.com/srd>). Every page reference was checked against that PDF on 2026-10-18.
- **R-n** is a *ruling*: our documented interpretation where the SRD is silent or leaves a choice ([§8](#8-rulings-register)). **OQ-n** is an open question ([§17](#17-open-questions)). **GS-n** is a *golden scenario*, a concrete test case with exact numbers ([§7.7](#77-golden-scenarios)).
- Complex sections start with a short **In plain words** paragraph.

## Contents

1. [Summary in plain words](#1-summary-in-plain-words)
2. [Goals](#2-goals)
3. [Non-goals](#3-non-goals)
4. [Roles and permissions](#4-roles-and-permissions)
5. [Glossary](#5-glossary)
6. [User stories and acceptance criteria](#6-user-stories-and-acceptance-criteria)
7. [Rules to implement](#7-rules-to-implement)
8. [Rulings register](#8-rulings-register)
9. [Effect model](#9-effect-model)
10. [Live sync behavior](#10-live-sync-behavior)
11. [Data model (conceptual)](#11-data-model-conceptual)
12. [Phone UX principles](#12-phone-ux-principles)
13. [Non-functional requirements](#13-non-functional-requirements)
14. [What can go wrong](#14-what-can-go-wrong)
15. [Milestones and cut line](#15-milestones-and-cut-line)
16. [Out of scope / backlog](#16-out-of-scope--backlog)
17. [Open questions](#17-open-questions)
18. [Proposed first ADRs](#18-proposed-first-adrs)
19. [Attribution](#19-attribution)
- [Appendix A. Interview decision log](#appendix-a-interview-decision-log)

---

## 1. Summary in plain words

QuestMaster is a phone-first web app for a fifth-edition (5E) tabletop group that plays **in person, around a real table, with phones**. It has three parts:

1. **A smart character sheet.** You type in the basic facts: ability scores, class and level, proficiencies and gear. The app works out every other number (modifiers, saves, skills, AC, HP, attack bonuses and more) and can explain each one, line by line.
2. **Rolls.** Tap a skill, save or weapon. The app rolls for you, or takes the number from your physical die, and applies advantage and disadvantage the way the rules say. It shows how the total was built.
3. **A live table.** Every roll and every change (damage, conditions…) shows up on the other phones within a second. The GM sees everyone's character at a glance and can apply damage or conditions. A phone that went to sleep catches up by itself when it wakes.

The rules come only from SRD 5.2.1. For anything outside the SRD (a subclass or magic item from another book), players create **custom entries** with real math. The app never ships that content itself.

## 2. Goals

| ID | Goal | How we know |
|---|---|---|
| G-1 | **Correct math.** Every derived number follows SRD 5.2.1 (§7) and the rulings (§8). | All golden scenarios (§7.7) pass as automated tests. Every number has a breakdown (B11). |
| G-2 | **Usable at a real table.** | One full in-person session (~4 h, 1 GM + 4–6 players on phones) with zero lost or duplicated actions and no unresolved "this number is wrong" report. |
| G-3 | **Live.** | A roll appears on every permitted device in ≤ 1 s (p95). A phone waking from sleep shows the current state in ≤ 3 s (p95). |
| G-4 | **Trustworthy dice.** | App dice can't be forged or re-rolled by a client. Manual dice are always labeled. |
| G-5 | **Legally clean.** | Only SRD 5.2.1 content is shipped, with the required attribution (§19). |
| G-6 | **Learning and portfolio value.** | A pure rules engine with SRD-referenced tests (TDD), decisions recorded as ADRs (§18). |

## 3. Non-goals

- **Not a virtual tabletop.** No maps, tokens, distances or line of sight.
- **Not a character builder.** No guided creation or level-up automation. No species, background or feat automation.
- **No monsters or NPCs**, no initiative order or turn tracker, no encounters.
- **Spells stop at the spell save DC and spell attack.** No spell lists, slots or concentration.
- **No content beyond SRD 5.2.1** shipped by the app, and no sharing of user-created content outside its campaign.
- **No 2014 rules**, and no house-rule toggles beyond the rulings in §8.
- **No native apps and no offline actions.** Offline is read-only.
- **No scale-out** beyond a single server instance.

## 4. Roles and permissions

**In plain words:** each campaign has exactly one GM (the person who created it) and up to nine players. The same person can be the GM in one campaign and a player in another. Players fully control their own character. The GM can see every character and apply "table actions" such as damage or conditions, but can't rewrite a player's sheet.

| Action | GM | Owner (player) | Other player |
|---|---|---|---|
| See public rolls in the feed | ✓ | ✓ | ✓ |
| See a "to GM only" roll | ✓ | ✓ (own rolls) | ✗ |
| View a full sheet (inputs, breakdowns, custom entries) | ✓ (all) | ✓ (own) | ✗ |
| View the party summary (HP, temp HP, status, conditions) | ✓ | ✓ | ✓ if Party view is on |
| Create a character | ✗ in own campaign | ✓ (one per campaign) | n/a |
| Edit sheet inputs (scores, classes, items, custom entries, adjustments, overrides) | ✗ (OQ-4) | ✓ | ✗ |
| Roll for a character | ✗ (OQ-5) | ✓ | ✗ |
| Table actions: damage, heal, temp HP, conditions, Exhaustion, Mark Stable, rests | ✓ (any) | ✓ (own) | ✗ |
| Revive a dead character | ✓ | ✗ | ✗ |
| Heroic Inspiration | grant, remove | mark gained, spend | ✗ |
| Undo the last table action on a character | ✓ | ✓ if they did it | ✗ |
| View a character's change log | ✓ | ✓ (own) | ✗ |
| Campaign settings, regenerate invite, remove members, delete campaign | ✓ | ✗ | ✗ |
| Delete a character | ✓ | ✓ (own) | ✗ |
| Leave the campaign | ✗ (GM transfer is backlog) | ✓ | ✓ |

## 5. Glossary

| Term | Meaning |
|---|---|
| **Campaign** | A persistent group (1 GM + players) with its characters, roll feed and settings. |
| **Table** | The live view of a campaign. A device is "at the table" while it is connected. |
| **Inputs** | Facts the player enters: base ability scores, class levels, skill proficiencies, equipment, custom entries, adjustments, overrides. |
| **State** | Things that change during play: current HP, temporary HP, spent Hit Point Dice, death saves, life status, conditions, Exhaustion level, Heroic Inspiration. |
| **Derived values** | Everything the rules engine computes from inputs + state (modifiers, bonuses, AC, passive Perception…). Never stored; always recomputed. |
| **Breakdown** | The list of contributions that explains a derived value, e.g. "+3 Dex, +2 proficiency, +1 Stone of Good Luck". |
| **Effect** | A small typed instruction attached to a source, e.g. "+1 to AC" ([§9](#9-effect-model)). |
| **Custom entry** | A user-created feature or item (name, note, effects) for content outside the SRD. Private to its campaign. |
| **Adjustment** | A labeled +/− the player adds to a derived value. It keeps updating with the sheet. |
| **Override** | A hard replacement of a derived value, always shown with ⚠. |
| **Table action** | A state change during play: damage, healing, temp HP, condition, Exhaustion, Heroic Inspiration, Mark Stable, rest, revive. |
| **Command** | A request from a device to change something (an input edit, a table action or a roll). Carries a unique CommandId. |
| **Event** | The server's record that something happened, numbered with the campaign's `seq` and sent to the devices allowed to see it. |
| **seq** | A per-campaign, strictly increasing number the server gives each event. |
| **D20 Test** | An ability check, saving throw or attack roll (SRD p.6). |
| **Fixed vs situational** | *Fixed* means the app always knows the effect applies (Poisoned → disadvantage on attack rolls). *Situational* means it depends on the scene (Frightened → only while the source is in sight), so the player confirms it on the roll sheet. |
| **RAW** | Rules as written. |
| **Ruling** | Our documented interpretation where the SRD is silent or leaves a choice ([§8](#8-rulings-register)). |

---

## 6. User stories and acceptance criteria

### 6.0 Story index

| ID | Story | Priority | Milestone |
|---|---|---|---|
| A1 | Sign in with Google or Discord | Must | M0 (1 provider) / M5 (both) |
| A2 | Create a campaign and invite players | Must | M0 (link) / M1 (QR) |
| A3 | Campaign settings | Should | M5 |
| B1 | Create and edit a character | Must | M0 (minimal) / M1 |
| B2 | Ability modifiers and proficiency bonus | Must | M0 / M1 |
| B3 | Saving throws | Must | M1 |
| B4 | Skills, Expertise and passive Perception | Must | M1 |
| B5 | Initiative | Must | M1 |
| B6 | Armor Class | Must | M1 / M3 |
| B7 | Speed | Should | M4 |
| B8 | Hit Point maximum and Hit Point Dice | Must | M1 |
| B9 | Weapon attacks and Unarmed Strike | Must | M2 |
| B10 | Spellcasting numbers | Must | M1 |
| B11 | Breakdown for every number | Must | M1 |
| B12 | Adjustments and overrides | Must | M1 |
| C1 | SRD armor, shields and weapons | Must | M1 |
| C2 | Curated SRD magic items and attunement | Must | M3 |
| C3 | Custom entries with effects | Must | M3 |
| C4 | Stacking rules | Must | M3 |
| C5 | Gated effects | Must | M3 |
| D1 | Roll sheet | Must | M2 |
| D2 | d20 resolution | Must | M0 (normal) / M2 |
| D3 | Automatic failures | Must | M4 |
| D4 | Damage rolls and critical hits | Must | M2 |
| D5 | Manual (physical) dice | Must | M2 |
| D6 | Roll visibility | Must | M2 |
| D7 | Server dice | Must | M0 |
| E1 | Damage and healing | Must | M4 |
| E2 | Temporary HP | Must | M4 |
| E3 | 0 HP, death saves and instant death | Must | M4 |
| E4 | Conditions and Exhaustion | Must | M4 |
| E5 | Heroic Inspiration | Should | M4 |
| E6 | Short and Long Rests | Should | M4 |
| F1 | Live roll feed | Must | M0 / M2 |
| F2 | Sheet sync across devices | Must | M1 |
| F3 | GM dashboard (presence: Should) | Must | M5 |
| F4 | GM table actions | Must | M4 |
| F5 | Party view | Should | M5 |
| F6 | Reconnect and catch-up | Must | M0 (v0) / M5 |
| F7 | Idempotent, ordered commands | Must | M0 |
| F8 | Change log and undo | Should | M5 |
| F9 | PWA, read-only offline | Should | M5 |
| G1 | SRD attribution and licensing | Must | M0 |
| G2 | i18n-ready | Must | M0 |
| G3 | Phone ergonomics and accessibility | Must | every milestone |

### A. Accounts and campaigns

#### A1 · Sign in with Google or Discord — Must · M0 / M5
*As a player at the table, I want to sign in with one tap, so I don't need yet another password.*

1. **Given** I'm signed out, **when** I choose "Continue with Google" or "Continue with Discord" and approve, **then** I'm signed in and see my campaigns.
2. **Given** it's my first sign-in, **then** my display name is prefilled from the provider and I can change it (1–30 characters).
3. **Given** I used the app on this device within the last 30 days, **when** I open it, **then** I'm still signed in (sliding 30-day session).
4. **When** I sign out, **then** this device's session ends, its live connection closes and its offline cache is cleared (F9). My other devices stay signed in.
5. **Given** the Development or Test environment, **then** a "Dev login" lets tests sign in as seeded users. **Given** Production, **then** the dev login endpoint doesn't exist (an automated test asserts 404).
6. The app stores only the provider name, provider user id and display name. No email address, and no provider tokens after sign-in.

#### A2 · Create a campaign and invite players — Must · M0 (link) / M1 (QR)
*As a GM, I want to create a campaign and let my players join by scanning a code, so setup at the table takes a minute.*

1. **When** I create a campaign with a name (1–60 characters), **then** I'm its GM and I see an invite link plus a QR code of that link.
2. **Given** a valid invite link, **when** a signed-in user opens it, **then** they join as a Player and land on "Create your character". **Given** a signed-out user, **then** they sign in first and are joined automatically.
3. **Given** I'm already a member, **when** I open the invite link again, **then** the campaign opens and no duplicate membership is created.
4. **When** the GM regenerates the invite, **then** the old link stops working immediately and shows "This invite is no longer valid. Ask your GM for a new one."
5. Invite tokens contain ≥ 128 bits of randomness, are stored hashed, and aren't derived from any id.
6. **Given** the campaign already has 10 members (GM + 9 players), **when** someone tries to join, **then** it fails with "This campaign is full."
7. **When** the GM removes a player, **then** that player's devices leave the table within 5 s, their further requests for the campaign are rejected, and their character is archived (OQ-2).
8. A player can leave a campaign. The effect is the same as being removed.

#### A3 · Campaign settings — Should · M5
1. The GM can switch **Party view** (default on) and **Manual dice** (default on).
2. **Given** a setting changes, **then** every connected device reflects it within 1 s.
3. **Given** Manual dice is off, **then** "I rolled…" is hidden everywhere, and the server rejects manual roll commands with a clear reason.

### B. Smart character sheet

#### B1 · Create and edit a character — Must · M0 (name, one class, scores) / M1 (all inputs)
*As a player, I want to enter my character's basic facts once, so the app can work out everything else.*

Inputs: name; class levels (one or more SRD classes, the first one is the **initial class**, SRD p.25); six base ability scores; proficiency level per skill (none, proficient, Expertise); base speed; equipment (C1, C2); custom entries (C3).

1. **Given** I'm a player without a character in this campaign, **when** I save a name (1–40 characters), at least one class with a level, and six base scores, **then** the character exists and appears on the GM's devices within 1 s.
2. Total character level must be 1–20, each class level ≥ 1, no class listed twice. Scores must be integers 1–30. Invalid input is rejected with a message next to the field, and nothing is saved.
3. **Given** a base score above 20, **then** the sheet saves it and shows a warning: "Characters normally max out at 20 (SRD p.5)."
4. Expertise can only be set on a skill the character is proficient in: "Expertise requires proficiency (SRD p.182)."
5. Class-derived facts (hit die, saving throw proficiencies, armor training, weapon proficiencies, spellcasting ability) are read-only, come from §7.3, and show their SRD page.
6. A player can't create a second character in the same campaign, and the GM can't create one in their own campaign.
7. Base speed defaults to 30 ft and must be a multiple of 5 from 0 to 120.

#### B2 · Ability modifiers and proficiency bonus — Must · M0 (modifiers) / M1
1. For every score 1–30 the modifier matches the SRD table (p.6) and the Round Down rule (p.187): **GS-01**.
2. The proficiency bonus (PB) comes from **total** character level: 1–4 → +2, 5–8 → +3, 9–12 → +4, 13–16 → +5, 17–20 → +6 (p.8, p.25): **GS-02**.
3. Effective scores = base scores, then "set score if higher" effects. The highest such effect wins, and it has no effect if the score without it is already ≥ its value: **GS-11**.
4. Every score and modifier opens its breakdown (B11).

#### B3 · Saving throws — Must · M1
1. Save bonus = ability modifier + PB (if proficient) + active bonuses to saving throws (all saves, or that ability).
2. Save proficiencies come only from the initial class (p.25): **GS-14**.
3. A save proficiency granted twice (class and custom entry) adds PB once (p.8).
4. Bonuses with different names stack (**GS-28**); same-named ones don't (R-1).
5. Bonuses to all saving throws also apply to Death Saving Throws (R-7).

#### B4 · Skills, Expertise and passive Perception — Must · M1
1. The sheet lists the 18 skills with their default abilities (§7.2).
2. Skill bonus = modifier of the ability used + PB (proficient) or 2 × PB (Expertise, p.182) + active bonuses to that skill, to checks with that ability, and to all ability checks: **GS-03**, **GS-32**.
3. Passive Perception = 10 + the Wisdom (Perception) check bonus. Add 5 with fixed advantage on Perception checks, subtract 5 with fixed disadvantage, and change nothing if both apply (p.186, p.8): **GS-04**.
4. Situational effects and Exhaustion don't change passive Perception (R-6, OQ-1).
5. On the roll sheet the player can pick a different ability for a skill check, e.g. Strength (Intimidation), because the Skills table lists only the ability a skill "most often" uses (p.9). PB still applies if proficient in the skill, and ability-specific bonuses follow the chosen ability.

#### B5 · Initiative — Must · M1
1. Initiative is a Dexterity check (p.13). Bonus = Dex modifier + bonuses to ability checks, Dex checks and initiative + PB if an effect grants initiative proficiency (e.g. a custom entry for the SRD Alert feat, p.87).
2. Fixed sources apply automatically: Poisoned (disadvantage, p.186), Invisible (advantage, p.184), Incapacitated (disadvantage, p.184), armor without training (disadvantage, p.92). Surprise is a situational question, "Were you surprised?" → disadvantage (p.13): **GS-05**.
3. Exhaustion reduces the roll (p.181).
4. The MVP doesn't track initiative order (backlog).

#### B6 · Armor Class — Must · M1 (armor, Shield, Unarmored Defense) / M3 (magic, custom)
1. The engine builds the candidate base ACs:
   - unarmored: 10 + Dex (p.7);
   - worn armor, per the Armor table (p.92): Light = base + Dex, Medium = base + Dex (max +2), Heavy = base;
   - Barbarian Unarmored Defense: 10 + Dex + Con while wearing no armor, Shield allowed (p.29);
   - Monk Unarmored Defense: 10 + Dex + Wis while wearing no armor and wielding no Shield (p.50);
   - custom base-AC effects (§9).
2. Only one base applies (p.7, p.177, p.25). The engine picks the **highest eligible** one (R-2), and the breakdown lists the others as "not used".
3. Then it adds: +2 for a Shield if trained with Shields (p.92); the magic bonus of armor or Shield (p.210, p.243); active AC bonuses (Ring or Cloak of Protection; Bracers of Defense only with no armor and no Shield, p.214).
4. An untrained Shield gives no AC at all, including its magic bonus (R-9).
5. Armor worn without training still gives its AC. The sheet shows a warning about disadvantage and spellcasting (p.92).
6. Golden scenarios **GS-06** to **GS-10**, **GS-26** and **GS-27** pass.

#### B7 · Speed — Should · M4
1. Speed = base speed, −10 ft if wearing armor whose Strength requirement is above effective Strength (p.92), −5 ft × Exhaustion level (p.181). Minimum 0.
2. Speed is 0 while Grappled, Restrained, Paralyzed, Petrified or Unconscious (p.182, p.187, p.186, p.191).
3. Example: base 30, Chain Mail (Str 13), Str 12 → 20. The same character with Gauntlets of Ogre Power (Str 19) → 30.

#### B8 · Hit Point maximum and Hit Point Dice — Must · M1
1. Level 1 (initial class only): the hit die's maximum + Con modifier (p.22, p.25).
2. Every later level: that class's fixed value + Con modifier, minimum 1 per level (p.23, R-5).
3. The Con modifier uses **effective** Con, so a Con change updates HP max for every level: **GS-12**.
4. HP-max bonuses from effects apply, including "+N per character level" (e.g. a custom entry for the Dwarf's Dwarven Toughness, p.84).
5. The Hit Point Dice pool is one die per class level, using that class's die (p.25): **GS-13**.
6. Rolled HP per level is backlog. Players who rolled use a labeled HP adjustment (B12).

#### B9 · Weapon attacks and Unarmed Strike — Must · M2
1. Each equipped weapon shows its attack bonus and damage formula. Unarmed Strike is always listed (p.190).
2. Ability used: melee → Str; ranged → Dex; Finesse → the higher of Str and Dex, for both attack and damage (p.89, R-4). A thrown melee weapon uses its melee ability (p.90).
3. PB is added only with weapon proficiency, from class data by category or property (e.g. Rogue: "Martial weapons that have the Finesse or Light property", p.61). Otherwise the attack shows a "not proficient" chip.
4. A magic weapon adds +N to attack and damage (p.253). Active bonuses to attack and damage rolls apply.
5. A Heavy weapon with Str < 13 (melee) or Dex < 13 (ranged) gets fixed disadvantage (p.89): **GS-15**.
6. Versatile weapons default to two-handed damage when no Shield is equipped, one-handed otherwise. The player can switch per roll (p.90, R-8): **GS-30**.
7. Ranged attacks get situational questions: "Within 5 ft of an enemy who can see you and isn't Incapacitated?" (p.15) and "Target beyond normal range?" (p.90). Each "yes" means disadvantage.
8. Unarmed Strike: attack = Str modifier + PB, damage = 1 + Str modifier, Bludgeoning (p.190): **GS-17**.
9. Finesse example: **GS-16**.

#### B10 · Spellcasting numbers — Must · M1
1. For each spellcasting ability among the character's classes (§7.3), the sheet shows spell save DC = 8 + modifier + PB and spell attack bonus = modifier + PB (p.23, p.106): **GS-29**.
2. Classes that share an ability show one line.
3. Bonuses that target spell save DC or spell attack apply.
4. "Spell attack" can be rolled from the roll sheet. No spell list, no spell damage.
5. While the character wears armor without training, the block shows "Can't cast spells: untrained armor (SRD p.92)."

#### B11 · Breakdown for every number — Must · M1
1. Tapping any derived number opens its breakdown: one line per contribution, with its value and source (class + page, SRD item + page, condition, custom entry, adjustment, override).
2. Inactive or ignored effects are listed greyed out, with the reason. Examples: "Bracers of Defense: inactive, Shield equipped"; "Ring of Protection (2nd): same name, not applied (R-1)"; "Gauntlets of Ogre Power: no effect, Strength already 19+".
3. The active lines always add up to the shown value. An automated invariant test checks this for every derived value in every golden scenario.
4. Each roll stores a snapshot of its breakdown, so later sheet edits never change past rolls.

#### B12 · Adjustments and overrides — Must · M1
1. The player can add an **adjustment** (label 1–40 characters, value −20…+20) to anything a Bonus effect can target (§9). It shows in the breakdown and keeps updating.
2. The player can **override** a final value: AC, initiative bonus, a save bonus, a skill bonus, passive Perception, speed, HP max, spell save DC or spell attack, a weapon's attack or damage bonus. Ability scores and PB can't be overridden.
3. An overridden value shows ⚠ on the sheet and on the GM dashboard. Its breakdown shows the computed value struck through, plus "Override N, set by <name> on <date>".
4. One tap resets an override.
5. Rolls use the overridden modifier. Advantage/disadvantage logic ignores overrides.

### C. Items and effects

#### C1 · SRD armor, shields and weapons — Must · M1
1. The catalog contains all armor and the Shield from the Armor table (p.92) and all weapons from the Weapons table (p.91), with damage, damage type and properties. Weapon mastery is stored but not used (backlog).
2. A character can wear one suit of armor and wield one Shield (p.92), and wear at most one cloak, one pair of bracers, one pair of gloves or gauntlets, one pair of footwear and one headwear item (p.103). Equipping into an occupied slot asks to replace the current item.
3. Armor with a Strength requirement lowers speed by 10 ft when effective Strength is below it (p.92, B7).
4. Armor marked "Disadvantage" for Stealth gives fixed disadvantage on Dexterity (Stealth) checks (p.92).
5. Armor worn without training gives fixed disadvantage on every D20 Test that involves Strength or Dexterity, and blocks spellcasting. A Shield without training gives no AC (p.92, p.177).
6. Any number of weapons can be carried. Only equipped weapons appear in the attack list.

#### C2 · Curated SRD magic items and attunement — Must · M3
Ready-made items. Each one is pure data with an SRD page, built from the effect types in §9:

| Item | SRD p. | Attunement | Effects modeled |
|---|---|---|---|
| Amulet of Health | 209 | yes | Con becomes 19 if lower |
| Armor, +1, +2, or +3 | 210 | no | Chosen armor + N to AC |
| Belt of Giant Strength (Hill 21, Frost or Stone 23, Fire 25, Cloud 27, Storm 29) | 213 | yes | Str becomes N if lower |
| Bracers of Defense | 214 | yes | +2 AC while wearing no armor and using no Shield |
| Cloak of Protection | 216 | yes | +1 AC, +1 saving throws |
| Cloak of the Bat | 216 | yes | Advantage on Dexterity (Stealth) checks (other properties are text only) |
| Gauntlets of Ogre Power | 223 | yes | Str becomes 19 if lower |
| Headband of Intellect | 225 | yes | Int becomes 19 if lower |
| Ring of Protection | 238 | yes | +1 AC, +1 saving throws |
| Shield, +1, +2, or +3 | 243 | no | Shield + N to AC (on top of the normal +2) |
| Stone of Good Luck (Luckstone) | 248 | yes | +1 ability checks, +1 saving throws, while carried |
| Weapon, +1, +2, or +3 | 253 | no | Chosen weapon + N to attack and damage rolls |

1. Each item can be picked from the catalog and shows its SRD page and a one-line summary, with no long rule text.
2. Effects of an item that requires attunement apply only while it is attuned **and** worn or carried as the item says (p.102).
3. A character can be attuned to at most 3 items (p.177). A 4th attempt is rejected with "Already attuned to 3 items."
4. Unequipping an item or ending attunement removes its effects right away, on all devices.
5. Attunement timing (during a Short Rest, p.102) isn't enforced; the table handles it on the honor system.
6. "+N" items record the base item they enhance (e.g. Longsword +1, Chain Mail +2).

#### C3 · Custom entries with effects — Must · M3
*As a player, I want to add my non-SRD subclass feature or magic item with its real numbers, so my sheet stays correct.*

1. The player can create a custom entry: kind (feature or item), name (1–60 characters), plain-text note (≤ 1,000 characters). Items also get a slot (armor, shield, cloak, bracers, gloves, footwear, headwear, ring, other) and a "requires attunement" flag.
2. An entry holds 0–10 effects of the user-creatable types in §9. Each effect is validated (e.g. set-score value 1–30, bonus −20…+20).
3. Custom entries are visible only to the owner and the GM, and are labeled "Custom" everywhere they appear.
4. Notes render as plain text (no HTML, no Markdown).
5. An entry can be switched off without being deleted.
6. Custom entries never leave their campaign (no sharing or export in the MVP).
7. Limit: 50 custom entries per character.

#### C4 · Stacking rules — Must · M3
1. Same-name effects: only the most potent applies (R-1). Two Rings of Protection give +1 AC, not +2 (**GS-06**).
2. Effects with different names stack: **GS-28**.
3. Proficiency is added once, however many sources grant it (p.8). Expertise doubles PB once (p.182): **GS-32**.
4. Advantage and disadvantage never stack, and any advantage plus any disadvantage means a single d20 (p.8): **GS-24**.
5. Set-score effects: the highest applies, and none applies if the score is already ≥ its value: **GS-11**.
6. A condition is either on or off (p.179). Exhaustion levels add up, to a maximum of 6 (p.181).

#### C5 · Gated effects — Must · M3
1. Supported gates: requires attunement; while equipped (worn or wielded); while carried; while wearing no armor; while not using a Shield; while the entry is switched on; while a given condition is active.
2. Gates are re-evaluated after every change, and the updated sheet reaches all devices. Example: equipping a Shield makes Bracers of Defense inactive (**GS-10**).

### D. Rolling

#### D1 · Roll sheet — Must · M2
*As a player, I want to see why a roll has advantage or disadvantage before I roll, so I trust the result.*

1. Tapping a check, save, skill, initiative, attack, spell attack or death save opens the roll sheet in ≤ 100 ms (sketch in §12).
2. It shows the modifier, the **fixed** advantage/disadvantage sources (checked, with reasons), and the **situational** questions (unchecked; checking one adds that source).
3. The DIS / NORMAL / ADV control is pre-selected from the checked sources (p.8). The player can change it, since the GM may grant advantage or impose disadvantage (p.8). The record then stores "set manually".
4. Skill checks have an ability picker (B4). Versatile weapons have a grip picker (B9).
5. Visibility: **Public** (default, and it resets to Public after every roll) or **To GM only**.
6. "Roll" (server dice) and "I rolled…" (manual dice, if allowed) sit in the bottom third of the screen.
7. If the roll is an automatic failure (D3), the main button reads "Record automatic failure".
8. When the device isn't Live (F6), the buttons are disabled, with the reason shown.

#### D2 · d20 resolution — Must · M0 (normal) / M2
1. Normal: one d20. Advantage: two d20s, keep the higher. Disadvantage: two d20s, keep the lower (p.8). The result card shows both dice, with the dropped one greyed out.
2. Total = kept die + modifier − 2 × Exhaustion level (p.181).
3. **Attack rolls only:** a kept die of 20 shows "Critical Hit", and a kept die of 1 shows "Miss" whatever the total (p.7). Checks and saves show no success or failure label, because the app doesn't know the DC or the target's AC.
4. The roll record stores: character, author, roll type, label, ability, mode (with sources, and whether it was set manually), every die face and which one was kept, the breakdown snapshot, the Exhaustion penalty, total, flags, dice source, visibility, time and seq.

#### D3 · Automatic failures — Must · M4
1. Strength and Dexterity saves fail automatically while Paralyzed, Petrified, Stunned or Unconscious (p.186, p.189, p.191). No dice are rolled, and the feed shows e.g. "Dexterity save: automatic failure (Paralyzed)": **GS-25**.
2. For an ability check while Blinded or Deafened, the roll sheet asks "Does this check need sight?" or "…hearing?". "Yes" records an automatic failure (p.177, p.181).
3. An automatic failure can't be bypassed except by removing the condition.

#### D4 · Damage rolls and critical hits — Must · M2
1. After an attack roll, the result card offers "Roll damage": the weapon's dice + the same ability modifier as the attack (p.16) + magic and other damage bonuses.
2. If the attack's kept die was a 20, damage defaults to **Critical**: the dice are rolled twice and the modifiers added once (p.16). The player can switch Critical on or off, e.g. when the GM rules that a hit on a Paralyzed creature within 5 ft is a crit (p.186).
3. Total damage is never below 0 (p.16): **GS-18**.
4. Unarmed Strike damage is 1 + Str modifier, and a crit doesn't change it because there are no dice to double: **GS-17**.
5. The card shows the damage type. Resistances aren't applied (backlog), and damage isn't applied to anyone automatically (there are no targets in the MVP).
6. A damage roll links to its attack roll and inherits its visibility.

#### D5 · Manual (physical) dice — Must · M2
*As a player who loves my own dice, I want to type in what I rolled and let the app do the math.*

1. "I rolled…" asks for:
   - one d20 for a normal roll, or two for advantage or disadvantage, each 1–20;
   - for damage, the total of the dice, checked against the dice range (2d6: 2–12; a crit with 4d6: 4–24);
   - for a death save, one d20;
   - for a Hit Point Die, one die value.
2. The app works out the kept die, the total and crit/miss exactly as with server dice.
3. Manual rolls are labeled "manual" in the feed and in the record, with an icon and text, not color alone.
4. When the GM turns Manual dice off (A3), the option disappears and the server rejects manual roll commands.

#### D6 · Roll visibility — Must · M2
1. Public rolls go to every member.
2. "To GM only" rolls go only to the roller's devices and the GM's devices. Other players receive nothing about them: not live, not through catch-up, not in snapshots. An automated E2E test inspects a third player's received messages to prove it.
3. Death saves follow the roll's visibility. Death-save counts are visible to the owner and the GM only (F5).

#### D7 · Server dice — Must · M0
1. Server dice come from a cryptographically secure generator with uniform results (e.g. `RandomNumberGenerator.GetInt32(1, sides + 1)`).
2. Clients never send dice values, except in manual mode.
3. A roll is stored before anyone sees its result. Retrying the same command returns the same result (F7), so a retry can't re-roll.
4. There is a statistical uniformity test with a documented false-failure rate below 1 in 10⁶ runs (ADR-0013).

### E. Combat state

#### E1 · Damage and healing — Must · M4
1. "Take N damage" (N ≥ 0): temporary HP absorb it first, the rest lowers HP, and HP never goes below 0 (p.18).
2. "Heal N": HP rises up to HP max. Temporary HP don't change (p.18).
3. Healing a character at 0 HP raises HP, resets death saves and ends Unconscious, but they stay Prone (p.17, p.191): **GS-31**.
4. The owner and the GM can both do this. The toast and the log show who did it and how much.
5. A Dead character can't be damaged or healed. The GM can "Revive" them with an HP value (default 1), which resets death saves and is logged.

#### E2 · Temporary HP — Must · M4
1. "Gain N temporary HP": if the current temporary HP are ≥ N, nothing changes and the app says it kept the higher value. Otherwise they become N. They never add up (p.18, R-3): **GS-19**.
2. Healing never restores temporary HP, and gaining them isn't healing (p.18).
3. A Long Rest removes them (p.18).
4. The HP display shows them separately, e.g. "24 / 31 +5".

#### E3 · 0 HP, death saves and instant death — Must · M4
1. When damage drops HP to 0: if the leftover damage ≥ HP max, the character is Dead (massive damage, p.17). Otherwise they gain Unconscious and their status becomes **Dying** (p.17): **GS-20**.
2. At 0 HP, any damage adds 1 death-save failure, a Critical Hit adds 2, and damage ≥ HP max kills (p.18): **GS-21**. The damage action has a "Critical Hit" checkbox.
3. Death save = d20 + bonuses to saving throws − 2 × Exhaustion. A total ≥ 10 is a success, anything lower a failure. A natural 1 counts as two failures; a natural 20 means the character regains 1 HP (pp.17–18, R-7): **GS-22**.
4. 3 successes → **Stable** (still at 0 HP and Unconscious, no more death saves). 3 failures → **Dead**. Both counts reset when the character regains HP or becomes Stable (p.17).
5. Death saves can only be rolled while Dying. The app doesn't enforce turn timing.
6. The owner or the GM can "Mark Stable", e.g. after a successful Help action (p.18).

#### E4 · Conditions and Exhaustion — Must · M4
1. The owner or the GM can switch any of the 15 conditions on or off (p.179). Exhaustion has levels 0–6 (p.181).
2. Implied conditions are added automatically and shown as "from <condition>": Paralyzed, Petrified and Stunned → Incapacitated; Unconscious → Incapacitated + Prone. Prone stays after Unconscious ends (p.186, p.189, p.191). An implied condition can't be removed while its source is active.
3. Effects follow the conditions matrix (§7.5). Fixed effects apply automatically to the character's own rolls; situational ones appear as questions on the roll sheet.
4. Effects on attacks *against* the character (e.g. "attacks vs you have Advantage") show as info chips on the GM dashboard and in the party view.
5. Exhaustion: every D20 Test −2 × level, speed −5 ft × level, death at level 6, and a Long Rest removes one level (p.181, p.185): **GS-23**.
6. Petrified grants immunity to Poisoned, so adding Poisoned shows "Immune (Petrified)" (p.186).
7. Condition durations aren't tracked (backlog).

#### E5 · Heroic Inspiration — Should · M4
1. The GM can grant or remove it. The owner can mark it gained, e.g. through the Human's Resourceful trait after a Long Rest (p.86), or spent.
2. A character has at most one (p.8). Granting it again changes nothing and shows "Already has Heroic Inspiration. They could give it to another character (p.8)."
3. Spending it only marks it spent. The reroll itself is backlog.

#### E6 · Short and Long Rests — Should · M4
1. **Short Rest** (needs HP ≥ 1, p.187): the player spends Hit Point Dice one at a time, picking the die type if multiclassed. Each die = roll (server or manual) + Con modifier, minimum 1 HP. HP can't exceed the maximum, and spent dice are tracked.
2. **Long Rest** (needs HP ≥ 1, p.185): HP to maximum, all Hit Point Dice restored, Exhaustion −1, temporary HP removed (p.18).
3. The GM can start a Long Rest for one character or the whole party. Characters at 0 HP are skipped, with a message.
4. Rest durations and interruptions aren't enforced.

### F. Live table

#### F1 · Live roll feed — Must · M0 / M2
1. A stored roll appears on every permitted device within 1 s (p95) (NFR-01).
2. The feed is ordered by the server's seq, never by device clocks.
3. Each entry shows the character, the label, a big total, the dice (kept and dropped), the mode with reason chips, manual and GM-only badges, and the time. Tapping it opens the breakdown.
4. The feed shows the latest 100 visible entries. "Load older" pages back 100 at a time.

#### F2 · Sheet sync across devices — Must · M1
1. Any change to a character (input edit or table action) reaches the owner's other devices and the GM's devices within 1 s (p95).
2. Field edits are "set" commands, and the last one processed wins. All devices end up showing the same value.
3. All math runs on the server. The editing device shows a small pending indicator until the updated sheet arrives; there is no client-side math.

#### F3 · GM dashboard — Must (presence: Should) · M5
*As a GM, I want to see the whole party at a glance during combat.*

1. One card per character: name, player, HP / max / temporary HP, status (OK, **Bloodied** at half HP or fewer (p.177), Dying, Stable, Dead), AC, passive Perception, initiative bonus, spell save DC(s), conditions and Exhaustion, Heroic Inspiration, death saves, ⚠ overrides, last roll.
2. Cards update live (≤ 1 s p95).
3. Presence (Should): Online (≥ 1 device connected), Reconnecting (lost less than 60 s ago), Offline.
4. On a phone the cards form a vertical list. On wider screens they form a grid.
5. Tapping a card opens the full sheet (inputs read-only for the GM) with the table-action panel (F4).

#### F4 · GM table actions — Must · M4
1. The GM can apply these to any character: damage (with "Critical Hit"), healing, temporary HP, conditions, Exhaustion ±1, Heroic Inspiration, Mark Stable, Revive, Long Rest.
2. The player's devices update within 1 s and show a toast naming the GM and the change, e.g. "GM: −7 HP (Critical Hit)".
3. (Should) The GM can select several characters and apply the same damage, healing or condition in one go. This sends one command per character, sharing a batch id.

#### F5 · Party view — Should · M5
1. With Party view on, each player sees the other characters' name, HP / max, temporary HP, status, conditions and Exhaustion. They don't see death-save counts, AC, breakdowns, inputs or custom entries.
2. With Party view off, players see only their own character and the feed.
3. Switching it updates all devices within 1 s.

#### F6 · Reconnect and catch-up — Must · M0 (v0) / M5 (complete)
*As a player whose phone keeps going to sleep, I want the app to catch up on its own, so I never act on stale numbers.*

1. The connection state is always visible: **Live** (no banner), **Reconnecting…** (amber banner), **Offline: read-only, last updated hh:mm** (grey banner).
2. When the device isn't Live, every action is disabled with the reason shown. Nothing is queued.
3. The client reconnects automatically with backoff and jitter (immediately, then after 2 s, 5 s, 10 s, then every 15 s). It also reconnects right away when the page becomes visible again or the network returns.
4. On every connect or reconnect, on app resume, and every 60 s while Live, the client asks for events after its last seq. The server returns the visible ones in order (up to 500), or "snapshot required", in which case the client loads a fresh snapshot (§10, S-9 and S-10).
5. After catch-up the client shows exactly the server's state. An E2E test takes one player offline for 10 minutes while 20 actions happen, then compares that player's sheet and feed with a freshly loaded client's.
6. After a server restart, all clients reconnect and catch up, and no acknowledged command is lost.

#### F7 · Idempotent, ordered commands — Must · M0
1. Every command carries a client-generated CommandId (UUID). The server keeps processed ids for ≥ 24 hours. A repeated id returns the original outcome and creates no new event or log entry.
2. A campaign's commands are processed one at a time, in arrival order. So the GM's "take 7 damage" and the player's "heal 3", sent in the same second, both apply.
3. A command is acknowledged only after its result is stored.
4. Test: sending the same "take 7 damage" command three times lowers HP by 7 once, with one event and one log entry.
5. More than 10 commands per second from one user are rejected with "Slow down" (NFR-15).

#### F8 · Change log and undo — Should · M5
1. Every accepted command that changes a character creates a log entry: time, author, device, action, and before → after for each changed field.
2. The owner and the GM can open a character's log (latest 200 entries).
3. "Undo" is offered only for the **most recent table action** on that character, to its author or the GM. Undo restores exactly the fields that action changed, and is itself logged, e.g. "Undid −17 HP (by GM)".
4. Once a newer table action exists for that character, older actions can't be undone.
5. Rolls are never deleted. If an undone action came from a roll (e.g. a death save), the roll in the feed is marked "reverted".
6. Input edits are logged but can't be undone (edit again instead).

#### F9 · PWA, read-only offline — Should · M5
1. The app is installable (web manifest, icons, service worker).
2. The device keeps the latest snapshot of the user's character (for the GM, the dashboard). Opened offline, the app shows it with "Offline: read-only, last updated hh:mm".
3. Offline, nothing can be changed and nothing is queued.
4. Signing out deletes the cached data from the device.
5. The cache is best effort, because browsers (especially iOS Safari) may evict it. The server stays the source of truth.

### G. Cross-cutting

#### G1 · SRD attribution and licensing — Must · M0
1. An "About & legal" page shows the SRD 5.2.1 attribution statement word for word (§19), and can be reached from every screen in ≤ 2 taps.
2. Product branding (name, logo, store and landing pages, app UI) doesn't use Wizards of the Coast trademarks. The only compatibility claim is "5E compatible" or "compatible with fifth edition" (SRD p.1).
3. Every SRD data record in the repo carries `srdVersion: "5.2.1"` and its `srdPage`.
4. No non-SRD rules text or game content is committed. The PR template has an "SRD-only content" checkbox.
5. User-created content (custom entries) is never shown outside its campaign.

#### G2 · i18n-ready — Must · M0
1. Every user-visible string in the frontend comes from a translation key. A lint rule or test fails the build on hard-coded template text.
2. The MVP ships English only.
3. SRD names (skills, conditions, items) also go through keys, so a translation is possible later. CC-BY then requires noting the changes.
4. Server errors are codes with parameters, translated on the client.
5. Numbers and dates use locale-aware formatting.

#### G3 · Phone ergonomics and accessibility — Must · every milestone
1. Works from 360 × 640 px up, with no horizontal scrolling.
2. Touch targets are ≥ 44 × 44 px. Roll buttons and table actions sit in the bottom third of the screen.
3. Light and dark themes follow the system setting, with a manual toggle. Contrast meets WCAG 2.2 AA.
4. Roll results are announced to screen readers (ARIA live region) with the total and the kept die.
5. Advantage, disadvantage, manual dice and GM-only are shown with text or icons, never color alone.
6. Text can scale to 200% without hiding any action.

---

## 7. Rules to implement

All page numbers refer to SRD 5.2.1. Rule summaries are paraphrased; read the SRD page for the full text.

### 7.1 Core rules

| Rule | SRD p. | Engine notes and edge cases | Stories |
|---|---|---|---|
| Ability scores normally 1–20 (monsters up to 30) | 5 | Accept 1–30 as input; warn above 20 | B1 |
| Ability modifier table | 6 | `floor((score − 10) / 2)`. **C# `/` truncates toward zero, so `(9 − 10) / 2` gives 0, not −1.** Test every score 1–30 against the table | B2 |
| Round Down | 187 | Every division in the game rounds down | all |
| D20 Test steps | 6 | Roll → add ability modifier, PB if relevant, bonuses and penalties → compare to a target the app doesn't know | D2 |
| Advantage/Disadvantage: two d20s; they don't stack; both → neither; a reroll affects only one die | 7–8, 181 | Mode resolution is a pure function of the sources; the player can set it manually (GM's call) | D1, D2 |
| Natural 20 / natural 1 matter for attack rolls only | 7 | Crit or Miss labels on attacks only | D2 |
| Proficiency Bonus table; added once; multiplied or divided once | 8 | Total character level when multiclassed (p.25) | B2–B5, B9, B10 |
| Skills and their usual abilities | 9 | The usual ability is "most often" used, so allow another ability per roll | B4 |
| Saving throw proficiencies; initial class only | 9, 25 | Class tables in §7.3 | B3 |
| Expertise | 182 | 2 × PB on that skill unless already doubled; needs proficiency | B4 |
| Passive Perception | 186 | 10 + check bonus, ±5 for advantage/disadvantage | B4 |
| Initiative is a Dexterity check; surprise | 13 | Effects on ability checks and Dex checks apply | B5 |
| Invisible / Incapacitated when rolling initiative | 184 | Advantage / disadvantage | B5 |
| Attack roll abilities | 7 | Str melee, Dex ranged, spellcasting ability for spells | B9, B10 |
| Weapon proficiency | 89 | PB only with proficiency | B9 |
| Finesse, Heavy, Light | 89 | Finesse uses the same ability for attack and damage (R-4); Heavy → disadvantage below 13 | B9 |
| Range, Thrown, Versatile | 90 | Beyond normal range → disadvantage (situational) | B9 |
| Weapons table | 91 | Catalog data | C1 |
| Ranged attacks in close combat | 15 | Situational disadvantage | B9 |
| Unarmed Strike | 190 | Str + PB; damage 1 + Str | B9, D4 |
| Damage rolls: same ability modifier; minimum 0 | 16 | | D4 |
| Critical Hits: roll damage dice twice | 16 | Modifiers once; no dice → no extra damage | D4 |
| Base AC 10 + Dex; only one base calculation | 7, 177, 25 | Engine picks the highest eligible (R-2) | B6 |
| Armor table; Strength requirement; Stealth; one armor + one Shield | 92 | Catalog data | B6, B7, C1 |
| Armor training; Shield training | 92, 177 | Disadvantage on Str/Dex D20 Tests and no spells; untrained Shield → no AC (R-9) | B6, C1 |
| Unarmored Defense: Barbarian, Monk | 29, 50 | Shield allowed only for the Barbarian | B6 |
| Level 1 HP by class | 22 | Initial class only (p.25) | B8 |
| Fixed HP per level, minimum 1 | 23 | R-5 | B8 |
| Multiclassing: PB, proficiencies, HP, Hit Point Dice, AC | 25 | Each class page has an "As a Multiclass Character" box | B1–B8 |
| Temporary HP | 18, 190 | Don't stack (R-3); not healing; end on a Long Rest | E2 |
| Dropping to 0 HP, Unconscious, death saves, massive damage, damage at 0 HP, stabilizing | 17–18 | | E3 |
| Conditions (list, don't stack with themselves) | 179 | Matrix in §7.5 | E4, D1, D3 |
| Exhaustion | 181 | −2 × level on D20 Tests; speed −5 × level; death at 6 | E4, D2, B7 |
| Heroic Inspiration | 8 | A flag only in the MVP | E5 |
| Short Rest; Hit Point Dice | 187 | Minimum 1 HP per die | E6 |
| Long Rest | 185 | Needs ≥ 1 HP | E6 |
| Spell save DC and spell attack | 23, 106 | 8 + mod + PB; mod + PB | B10 |
| Combining spell effects | 106 | Basis for R-1 | C4 |
| Attunement: max 3; without it, only nonmagical benefits | 177, 102 | | C2 |
| Wearing items; multiple items of the same kind; paired items | 103 | Pairs are modeled as one item | C1, C2 |
| Bloodied | 177 | Status chip at half HP or fewer | F3 |
| Choosing to fail a save | 7 | Backlog | n/a |
| Tool + skill proficiency → advantage | 9 | Backlog (tools aren't modeled) | n/a |

### 7.2 Skills (p.9)

| Ability | Skills |
|---|---|
| Strength | Athletics |
| Dexterity | Acrobatics, Sleight of Hand, Stealth |
| Intelligence | Arcana, History, Investigation, Nature, Religion |
| Wisdom | Animal Handling, Insight, Medicine, Perception, Survival |
| Charisma | Deception, Intimidation, Performance, Persuasion |

### 7.3 SRD class data (MVP subset)

This is the only class data the MVP needs. Class features are not automated, except Unarmored Defense.

| Class | p. | Hit die | Level-1 HP / later levels | Saving throws | Armor training | Weapon proficiencies | Spellcasting ability |
|---|---|---|---|---|---|---|---|
| Barbarian | 28 | d12 | 12 / 7 + Con | Str, Con | Light, Medium, Shields | Simple, Martial | n/a |
| Bard | 31 | d8 | 8 / 5 + Con | Dex, Cha | Light | Simple | Cha |
| Cleric | 36 | d8 | 8 / 5 + Con | Wis, Cha | Light, Medium, Shields | Simple | Wis |
| Druid | 41 | d8 | 8 / 5 + Con | Int, Wis | Light, Shields | Simple | Wis |
| Fighter | 47 | d10 | 10 / 6 + Con | Str, Con | Light, Medium, Heavy, Shields | Simple, Martial | n/a |
| Monk | 49 | d8 | 8 / 5 + Con | Str, Dex | none | Simple; Martial with the Light property | n/a |
| Paladin | 53 | d10 | 10 / 6 + Con | Wis, Cha | Light, Medium, Heavy, Shields | Simple, Martial | Cha |
| Ranger | 57 | d10 | 10 / 6 + Con | Str, Dex | Light, Medium, Shields | Simple, Martial | Wis |
| Rogue | 61 | d8 | 8 / 5 + Con | Dex, Int | Light | Simple; Martial with Finesse or Light | n/a |
| Sorcerer | 64 | d6 | 6 / 4 + Con | Con, Cha | none | Simple | Cha |
| Warlock | 70 | d8 | 8 / 5 + Con | Wis, Cha | Light | Simple | Cha |
| Wizard | 77 | d6 | 6 / 4 + Con | Int, Wis | none | Simple | Int |

Level-1 HP: p.22. Later levels: p.23. Multiclass proficiency gains come from each class's "As a Multiclass Character" box (p.25); they are entered as data with page references during M1.

### 7.4 Other advantage/disadvantage sources

| Source | SRD p. | Kind | Affects |
|---|---|---|---|
| Armor worn without training | 92, 177 | fixed | Disadvantage on D20 Tests involving Str or Dex (checks, saves, attacks, initiative) |
| Armor marked "Disadvantage" for Stealth | 92 | fixed | Disadvantage on Dexterity (Stealth) checks |
| Heavy weapon below Str 13 (melee) / Dex 13 (ranged) | 89 | fixed | Disadvantage on attacks with that weapon |
| Cloak of the Bat (attuned) | 216 | fixed | Advantage on Dexterity (Stealth) checks |
| Surprised at the start of combat | 13 | situational | Disadvantage on Initiative |
| Ranged attack within 5 ft of an enemy who can see you and isn't Incapacitated | 15 | situational | Disadvantage on that attack |
| Target beyond normal range | 90 | situational | Disadvantage on that attack |
| GM decides | 8 | manual | Player sets the mode on the roll sheet ("set manually") |
| Custom entries | n/a | fixed or situational | As defined by the entry (§9) |

### 7.5 Conditions matrix

"Own rolls" means rolls the character makes. "Vs you" effects change other creatures' rolls, so they are shown as info chips only.

| Condition | SRD p. | Own rolls: fixed (automatic) | Own rolls: situational (question) | Other effects shown | Implies |
|---|---|---|---|---|---|
| Blinded | 177 | Disadvantage on attack rolls | "Does this check need sight?" → automatic failure | Attacks vs you: Advantage | n/a |
| Charmed | 178 | n/a | n/a | Can't attack the charmer; the charmer has Advantage on social checks vs you | n/a |
| Deafened | 181 | n/a | "Does this check need hearing?" → automatic failure | n/a | n/a |
| Exhaustion (1–6) | 181 | −2 × level on every D20 Test | n/a | Speed −5 ft × level; death at level 6; Long Rest −1 level | n/a |
| Frightened | 182 | n/a | Disadvantage on ability checks and attack rolls while the source is in line of sight | Can't willingly move closer to the source | n/a |
| Grappled | 182 | n/a | Disadvantage on attacks against anyone other than the grappler | Speed 0 | n/a |
| Incapacitated | 184 | Disadvantage on Initiative | n/a | No actions, Bonus Actions or Reactions; Concentration broken; can't speak | n/a |
| Invisible | 184 | Advantage on Initiative | Advantage on attack rolls ("Can the target see you?" → no advantage) | Attacks vs you: Disadvantage (unless the attacker can see you) | n/a |
| Paralyzed | 186 | Str and Dex saves fail automatically | n/a | Speed 0; attacks vs you: Advantage; hits from within 5 ft are Critical Hits | Incapacitated |
| Petrified | 186 | Str and Dex saves fail automatically | n/a | Speed 0; attacks vs you: Advantage; Resistance to all damage; immune to Poisoned | Incapacitated |
| Poisoned | 186 | Disadvantage on attack rolls and ability checks (so also initiative and passive Perception) | n/a | n/a | n/a |
| Prone | 186 | Disadvantage on attack rolls | n/a | Attacks vs you: Advantage within 5 ft, otherwise Disadvantage | n/a |
| Restrained | 187 | Disadvantage on attack rolls and Dex saves | n/a | Speed 0; attacks vs you: Advantage | n/a |
| Stunned | 189 | Str and Dex saves fail automatically | n/a | Attacks vs you: Advantage | Incapacitated |
| Unconscious | 191 | Str and Dex saves fail automatically | n/a | Speed 0; attacks vs you: Advantage; hits from within 5 ft are Critical Hits; drops held items | Incapacitated, Prone (Prone remains when it ends) |

### 7.6 Computation order

See the evaluation pipeline in [§9.5](#95-evaluation-pipeline).

### 7.7 Golden scenarios

Each scenario becomes an automated test named after its ID and SRD pages. "Milestone" is when the scenario (or the part of it after the slash) becomes testable.

| ID | Setup | Expected | Basis | Milestone |
|---|---|---|---|---|
| GS-01 | Scores 1, 8, 9, 10, 11, 15, 20, 30 | Modifiers −5, −1, −1, +0, +0, +2, +5, +10 | p.6, p.187 | M0 |
| GS-02 | Fighter 3 / Rogue 2 | PB +3 | p.8, p.25 | M1 |
| GS-03 | Rogue 5, Dex 16, Expertise in Stealth / + Stone of Good Luck attuned | Stealth +9 / +10 | p.182, p.248 | M1 / M3 |
| GS-04 | Ranger 1, Wis 14, proficient in Perception / + Poisoned | Perception +4, passive 14 / passive 9 | p.186 | M1 / M4 |
| GS-05 | Dex 14, Stone of Good Luck attuned; then Poisoned; then also Invisible; then Exhaustion 2 | Initiative +3 → disadvantage → advantage and disadvantage cancel, one d20 → the roll's total −4 | p.13, p.184, p.186, p.8, p.181 | M4 |
| GS-06 | Fighter, Dex 18, Chain Mail; then Shield; then Shield +1 instead; then Ring of Protection; then Cloak of Protection; then a second Ring of Protection | AC 16 → 18 → 19 / → 20 → 21 → 21 | p.92, p.243, p.238, p.216, R-1 | M1 / M3 |
| GS-07 | Breastplate with Dex 18; Half Plate Armor with Dex 12 | AC 16; AC 16 | p.92 | M1 |
| GS-08 | Barbarian 1, Dex 14, Con 16, no armor; then Shield; then Leather Armor instead of no armor (no Shield) | AC 15 → 17 → 13 (Unarmored Defense not eligible in armor) | p.29, p.92, R-2 | M1 |
| GS-09 | Monk 1, Dex 16, Wis 16, no armor; then picks up a Shield | AC 16 → 13 (no Shield training, so no Shield AC, and Monk Unarmored Defense needs no Shield) | p.50, p.92 | M1 |
| GS-10 | Wizard 1, Dex 14, Bracers of Defense attuned; then a custom base AC "13 + Dex, no armor"; then a Shield | AC 14 → 17 → 15 (untrained Shield: no AC; Bracers inactive) | p.214, p.92, R-2, R-9 | M3 |
| GS-11 | Str 15 + Gauntlets of Ogre Power; Str 20 + Gauntlets; Str 15 + Gauntlets + Belt of Giant Strength (Hill) | Str 19; Str 20 (no effect); Str 21 | p.223, p.213 | M3 |
| GS-12 | Fighter 5, Con 10 / then attune Amulet of Health | HP max 34 / 54 | p.22, p.23, p.209 | M1 / M3 |
| GS-13 | Con 14: Fighter 1 / Wizard 1 (Fighter first) vs Wizard 1 / Fighter 1 (Wizard first) | HP max 18 vs 16; Hit Point Dice 1d10 + 1d6 in both | p.22, p.23, p.25 | M1 |
| GS-14 | Wizard 1 / Fighter 1 (Wizard first) | Proficient saves: Int and Wis only | p.25, p.77 | M1 |
| GS-15 | Greatsword with Str 12; with Str 13; Longbow with Dex 12 | Fixed disadvantage; none; fixed disadvantage | p.89, p.91 | M2 |
| GS-16 | Level 1, Str 10, Dex 16, proficient, Rapier | Attack +5, damage 1d8 + 3 | p.89, R-4 | M2 |
| GS-17 | Str 16, Longsword (one-handed) hit with a natural 20; Unarmed Strike crit with Str 16 | Damage 2d8 + 3; Unarmed Strike damage 4 (no dice to double) | p.16, p.190 | M2 |
| GS-18 | Club (1d4), Str 6, die shows 1 | Damage 0, not −1 | p.16 | M2 |
| GS-19 | HP 20/31 with 5 temp HP; gain 4 temp HP; take 8 damage; heal 20 | Temp stays 5 → temp 0, HP 17 → HP 31, temp 0 | p.18, R-3 | M4 |
| GS-20 | HP max 12, HP 6; takes 18 / instead takes 17 | Dead (12 left over ≥ 12) / Dying, Unconscious (11 left over) | p.17 | M4 |
| GS-21 | Dying with 1 failure; takes 3 damage; then takes a Critical Hit | 2 failures → 4 failures → Dead | p.18 | M4 |
| GS-22 | Dying; death save d20 = 1; d20 = 20; d20 = 9 with Ring of Protection; d20 = 11 with Exhaustion 1 | +2 failures; HP 1, Unconscious ends, still Prone, counts reset; success (10); failure (9) | pp.17–18, p.191, p.181, R-7 | M4 |
| GS-23 | Exhaustion 3, base speed 30; then level 6; long rest at level 3 | Every D20 Test −6, speed 15; Dead at 6; level 2 after the Long Rest | p.181, p.185 | M4 |
| GS-24 | Attack while Poisoned and Prone, and the player confirms Invisible (target can't see them) | Advantage + disadvantage → one d20 | p.8, p.186, p.184 | M4 |
| GS-25 | Paralyzed: Dex save; Wis save | Dex: automatic failure, no dice. Wis: rolled normally | p.186 | M4 |
| GS-26 | Wizard 1 (no armor training), Str 8, Chain Mail | AC 16; disadvantage on Str/Dex checks, saves, attacks and initiative; "can't cast spells" warning / speed −10 ft | p.92, p.177 | M1 / M4 |
| GS-27 | Wizard 1 with a Shield / with a Shield +1 | No AC from the Shield / still none (R-9) | p.92, p.177, R-9 | M1 / M3 |
| GS-28 | Dex save +2 without items; Ring + Cloak of Protection | Dex save +4 | p.238, p.216 | M3 |
| GS-29 | Cleric 5, Wis 18; Paladin 2 / Sorcerer 3, Cha 16 | DC 15, spell attack +7; one line: DC 14, +6 | p.23, p.106 | M1 |
| GS-30 | Longsword, no Shield; with a Shield | Default damage 1d10; default 1d8 (both switchable) | p.90, R-8 | M2 |
| GS-31 | At 0 HP with 2 failures, healed for 5 | HP 5, death saves reset, Unconscious ends, still Prone | p.17, p.191 | M4 |
| GS-32 | Rogue 1, Stealth proficient from the class and also from a custom entry, plus Expertise | PB counted once, doubled once (+2 × PB) | p.8, p.182 | M3 |

---

## 8. Rulings register

**In plain words:** sometimes the SRD lets the player choose, or simply says nothing. Then we pick one behavior, write it down here, and test it like any other rule. A ruling is never hidden: the breakdown names it.

| ID | Ruling | SRD basis | Why | Escape hatch |
|---|---|---|---|---|
| R-1 | **Same-name effects don't combine; only the most potent applies.** Matching key: catalog id for SRD items, normalized name (trimmed, case-insensitive) for custom entries, plus the same effect type and target. "Most potent" = largest benefit (or largest penalty, for penalties). | p.106 says this for the same *spell*; it is silent on items | Matches what most tables expect, and stops duplicated custom entries from farming bonuses | GM can ask the player to rename a custom entry |
| R-2 | **The engine picks the highest eligible base AC.** | p.7, p.177: "you choose which calculation to use" | A player would always choose the highest; one less decision | Override (B12) |
| R-3 | **New temporary HP replace current ones only if higher.** | p.18: the creature decides | The higher value is the sensible choice in nearly all cases | none in MVP |
| R-4 | **Finesse uses the higher of Str and Dex, for attack and damage.** | p.89: "your choice", same modifier for both | Always the better choice | Override the weapon's bonus |
| R-5 | **The minimum of 1 HP per level also applies to fixed values.** | p.23 states the minimum for rolled values | Consistent; prevents 0 or negative gains with very low Con | Adjustment |
| R-6 | **Situational effects apply only when confirmed on a roll; passive Perception uses fixed effects only.** | p.186, p.8 | The app can't see the scene | Adjustment or override |
| R-7 | **Death Saving Throws are saving throws: bonuses to all saving throws and Exhaustion apply; ability-specific ones don't.** | p.17 ("unlike other saving throws… not tied to an ability score"), p.181 | A RAW reading, made explicit | n/a |
| R-8 | **Versatile default grip:** two-handed when no Shield is equipped, otherwise one-handed. | p.90 | Product default | Switch per roll |
| R-9 | **A Shield without training gives no AC at all, including its magic bonus.** | p.92, p.177 ("you don't gain its AC bonus") | The magic bonus is part of the Shield's AC benefit | Override |

---

## 9. Effect model

**In plain words:** an effect is a tiny instruction, like "+1 to AC" or "Strength becomes 19 unless it's already higher". Everything that changes numbers is a bundle of effects: SRD items, the few modeled class features, conditions, custom entries and adjustments. The engine knows only a short, fixed list of effect types. That keeps the math testable. Anything the list can't express is handled with an override.

**Rule:** adding an effect type, target or gate requires an ADR and tests.

### 9.1 User-creatable effect types (custom entries and adjustments)

| Type | Parameters | Example |
|---|---|---|
| **Bonus** | target; value −20…+20; scaling: none, × character level, or × levels in class X | Ring of Protection: +1 AC and +1 to all saving throws. Dwarven Toughness: +1 HP max × character level |
| **SetAbilityScore** | ability; value 1–30. Applies only if the score without it is lower; the highest one wins | Gauntlets of Ogre Power: Strength 19 |
| **BaseArmorClass** | base number; abilities added, each with an optional cap; usually gated | A Mage Armor-like effect: 13 + Dex, while wearing no armor |
| **RollMode** | Advantage or Disadvantage; roll scope (a target from §9.3); fixed, or situational with a question (≤ 80 characters) | Cloak of the Bat: Advantage on Dexterity (Stealth) checks, fixed |
| **Proficiency** | Proficient or Expertise; target: a skill, a saving throw, initiative, a weapon category or weapon, an armor category or Shield | Alert-like feat: proficiency in initiative |

### 9.2 Internal effect types (SRD rules and conditions only; not user-creatable in the MVP)

| Type | Parameters | Used by |
|---|---|---|
| **AutoFail** | roll scope; fixed or situational | Paralyzed, Petrified, Stunned, Unconscious (Str and Dex saves); Blinded and Deafened (checks needing sight or hearing) |
| **D20Penalty** | value per level | Exhaustion: −2 × level |
| **SpeedChange** | set to 0, or −N ft | Grappled, Restrained, Paralyzed, Petrified, Unconscious; Exhaustion; heavy armor below its Strength requirement |

### 9.3 Targets

| Target | Includes |
|---|---|
| ArmorClass | Final AC, after the base is chosen |
| SavingThrow(all, or one ability) | "All" includes Death Saving Throws (R-7) |
| AbilityCheck(all, or one ability) | Includes skill checks with that ability, and initiative (Dex) |
| SkillCheck(skill) | |
| Initiative | |
| AttackRoll(all, melee, ranged, spell, or one weapon) | |
| DamageRoll(all weapons, or one weapon) | |
| HitPointMaximum | |
| Speed | |
| SpellSaveDC, SpellAttack | |
| PassivePerception | A direct bonus (rare); Perception check bonuses already flow in |

### 9.4 Gates

All of an effect's gates must hold for it to be active: **RequiresAttunement**, **WhileEquipped** (worn or wielded), **WhileCarried**, **WhileNoArmor**, **WhileNoShield**, **WhileEnabled** (custom entry switched on), **WhileCondition(X)**.

Gates depend only on equipment, attunement, toggles and conditions, never on computed numbers, so evaluation can't loop.

### 9.5 Evaluation pipeline

Order matters. Every step writes breakdown lines.

1. **Collect sources:** class data, equipped, carried and attuned items, enabled custom entries, adjustments, active conditions (including implied ones) and Exhaustion.
2. **Check gates.**
3. **Apply R-1** (same name → only the most potent).
4. **Effective ability scores:** base scores, then SetAbilityScore (the highest, only if higher).
5. **Ability modifiers;** PB from total level.
6. **Proficiency level per target:** the highest of none, proficient and Expertise. Never added twice (p.8, p.182).
7. **Numeric value per target:** ability modifier + PB part + sum of bonuses, then the override if there is one.
8. **AC:** candidate bases → highest eligible (R-2) → + Shield → + magic → + AC bonuses → override.
9. **Per roll:** fixed sources + confirmed situational sources → mode (p.8); check for automatic failure.
10. **At roll time:** dice → kept die → + modifier − Exhaustion penalty → flags (Critical Hit or Miss, attacks only).

---

## 10. Live sync behavior

**In plain words:** think of the server as the one scorekeeper at the table. Phones never change numbers themselves. They *ask* the scorekeeper ("Alice takes 7 damage"). The scorekeeper writes it in the book under the next line number and reads it out to everyone allowed to hear it. A phone that dozed off asks "what happened after line 41?" and gets the lines it missed. If a phone asks twice because it didn't hear the answer, the scorekeeper recognizes the request and doesn't write it down again.

### 10.1 Rules

| ID | Rule |
|---|---|
| S-1 | Every change and every roll is a **command**: `{commandId (UUID from the client), campaignId, type, payload}`. Read-only queries aren't commands. |
| S-2 | The server **validates and authorizes** every command: membership, role, ownership and state preconditions (e.g. "no Long Rest at 0 HP"). |
| S-3 | A campaign's commands are **processed one at a time**, in arrival order. At ≤ 10 people per campaign this is the simplest correct choice; ADR-0007 may relax it to per character. |
| S-4 | The result (state change, roll, log entry, events) is **stored in one transaction before the acknowledgement**. |
| S-5 | Each stored event gets the campaign's **next seq inside that same transaction, under the campaign lock**, so seq order equals commit order. Otherwise catch-up could skip an event that commits late. |
| S-6 | **Idempotency:** commandIds are stored; a repeat returns the stored outcome (the same roll), with no new event. |
| S-7 | **Fan-out after commit**, only to permitted users: public events → the campaign group; GM-only rolls → the GM and the roller; the full computed sheet → owner and GM; the party summary → the other players (if Party view is on). |
| S-8 | **Clients:** order the feed by seq; ignore character snapshots whose `version` is older than the one they hold (robust to reordering). Seq gaps are normal, because filtering hides other people's GM-only events, so clients don't treat them as errors. |
| S-9 | **Catch-up:** on connect, reconnect, app resume, and every 60 s while Live, the client asks for events after its last seq. The server returns the visible ones (max 500) or `snapshotRequired`. |
| S-10 | **Snapshot:** one call returns the current visible state plus the current seq. Used on first load, after `snapshotRequired`, and after sign-in. |
| S-11 | **Connection state machine:** Live ⇄ Reconnecting → Offline. Actions only in Live. |
| S-12 | No in-memory state is needed for correctness. Presence is best effort. |
| S-13 | SignalR's stateful reconnect may be enabled to smooth over short blips, but correctness relies only on S-9 and S-10. |
| S-14 | When a member is removed, their connections are closed, and their later catch-up and snapshot calls are rejected. |

### 10.2 Commands (MVP catalog)

| Group | Commands |
|---|---|
| Campaign | create, rename, settings.update, invite.regenerate, member.remove, member.leave |
| Character inputs | create, setName, setClassLevels, setBaseScore, setSkillProficiency, setBaseSpeed, item.add / remove / equip / unequip / attune / unattune, customEntry.upsert / delete / toggle, adjustment.upsert / delete, override.set / clear, archive, delete |
| Rolls | roll.d20 (kind: check, save, skill, initiative, attack, spellAttack, deathSave; ability; mode; grip; visibility; confirmed situational sources; optional manual dice), roll.damage (linked attack, critical, grip, optional manual total), roll.hitDie (die, optional manual value) |
| Table actions | damage(amount, critical), heal(amount), gainTempHp(amount), condition.set / clear, exhaustion.change(±1), inspiration.grant / remove / gain / spend, markStable, revive(hp), longRest(characterIds), undo(commandId) |

### 10.3 Server → client messages

| Message | Audience | Content |
|---|---|---|
| RollRecorded | public: everyone; GM-only: GM + roller | Roll record, including the breakdown snapshot |
| CharacterSheetChanged | owner + GM | Full computed sheet + version |
| PartySummaryChanged | other players (if Party view is on) | Summary + version |
| CampaignChanged | everyone | Settings, members |
| MemberRemoved | everyone (forced disconnect for the removed user) | Member id |
| Presence (no seq) | GM | Online / Reconnecting / Offline per member |

---

## 11. Data model (conceptual)

**In plain words:** we store what people *typed* (inputs), what *happened* (state, rolls, the command log), and nothing that can be recomputed. The numbers on the sheet are always calculated fresh by the rules engine. The exceptions are history: a roll keeps the breakdown it had at roll time, and the change log keeps before/after values.

| Entity | Key fields | Notes |
|---|---|---|
| User | id, displayName, createdAt | |
| ExternalLogin | provider, providerUserId, userId | Unique on (provider, providerUserId) |
| Device | id, userId, label, lastSeenAt | Random id per browser install; for presence and logs |
| Campaign | id, name, gmUserId, inviteTokenHash, settings {partyView, manualDice}, lastSeq | lastSeq advances under the campaign lock (S-5) |
| Membership | campaignId, userId, role (GM, Player), status (Active, Removed, Left), joinedAt | |
| Character | id, campaignId, ownerUserId, status (Active, Archived), version, schemaVersion, inputs, state | `version` increments on every change |
| ↳ inputs | name, classLevels [classId, level] (first = initial class), baseScores, skills {skill: none, proficient, expertise}, baseSpeed, inventory [ItemInstance], customEntries [CustomEntry], adjustments, overrides | |
| ↳ state | currentHp, tempHp, spentHitDice {die: count}, deathSaves {successes, failures}, life (Alive, Dying, Stable, Dead), conditions, exhaustion, heroicInspiration | |
| ItemInstance (in inputs) | id, catalogEntryId or customEntryId, options (base armor or weapon, variant), equipped, attuned | |
| CustomEntry (in inputs) | id, kind, name, note, slot, requiresAttunement, enabled, effects [Effect] | |
| Effect | type, target, parameters, gates, fixed or situational (+ question) | §9 |
| CatalogEntry (SRD, read-only) | id (stable slug), kind (class, armor, weapon, magicItem, condition, skill), data, srdVersion, srdPage | Seeded from code-reviewed JSON in the repo |
| CommandRecord | commandId (unique), campaignId, characterId?, actorUserId, deviceId, type, payload, outcome, changes (before/after), undoOf?, undoneBy?, createdAt | Idempotency + change log + undo |
| RollRecord | id, campaignId, characterId, actorUserId, commandId, kind, label, ability, mode, modeSources, modeSetManually, dice [sides, value, kept], breakdown snapshot, exhaustionPenalty, total, flags, diceSource (server, manual), visibility, linkedRollId, reverted, seq, createdAt | Never deleted |
| CampaignEvent | campaignId, seq, type, audience (everyone, GM + user, owner + GM, players), refs, createdAt | Read by catch-up |

```
User 1──* Membership *──1 Campaign 1──* Character (owner: User)
Campaign 1──* CampaignEvent / RollRecord / CommandRecord
Character 1──* RollRecord / CommandRecord
Character contains inputs { ItemInstance*, CustomEntry* { Effect* } } and state
ItemInstance and class levels ──> CatalogEntry (SRD 5.2.1, read-only)
```

How inputs and state are physically stored (one JSONB document per character, or normalized tables) is decided in ADR-0008.

---

## 12. Phone UX principles

1. **One hand, bottom third.** Rolling and table actions sit where a thumb reaches. Reading goes at the top.
2. **Every number is explainable.** Tap it to see the breakdown (B11).
3. **No silent automation.** Every automatic advantage, disadvantage or failure shows its source.
4. **The connection state is always visible** (F6). The app never lets you act on stale data.
5. **Big, glanceable results.** The total is the largest text on the card; the kept die is highlighted and the dropped one greyed.
6. **Dim rooms.** Dark theme, readable contrast, no tiny text.

**Play screen (player, portrait)**

```
┌──────────────────────────────────┐
│ ● Live              Mira · Rogue 5│  connection state + character
│ HP ███████████░░░  27/38   +5     │  temp HP shown separately
│ AC 15 · Init +4 · Speed 30 · PP 14│
│ [Prone] [Exhaustion 1]  ★ Inspired│  condition chips (tap = details)
├──────────────────────────────────┤
│   Rolls  │  Sheet  │ Feed │ Party │
├──────────────────────────────────┤
│ SAVES   Str +0   Dex +7   Con +2  │
│         Int +5   Wis +1   Cha +2  │
│ SKILLS  Acrobatics +4             │
│         Stealth   +10             │
│ ATTACKS Rapier    +7  1d8+4    ⓓ  │  ⓓ = fixed disadvantage now (Prone)
│         Shortbow  +7  1d6+4    ⓓ  │
├──────────────────────────────────┤
│   [ − Take damage ]  [ + Heal ]   │  bottom third: table actions
└──────────────────────────────────┘
```

**Roll sheet** (D1)

```
┌──────────────────────────────────┐
│ Bram · Stealth check (DEX)    +1  │
│ Ability: [ DEX ▾ ]                │
│ Suggested: DISADVANTAGE           │
│  [x] Chain Mail (fixed)           │
│  [ ] Frightened: is the source    │
│      of fear in sight?            │
│                                   │
│   [*DIS*]   [ NORMAL ]   [ ADV ]  │  pre-selected from the sources
│                                   │
│   Visibility: Public ▾            │
│   [    ROLL    ]  [ I rolled… ]   │
└──────────────────────────────────┘
```

**Result card**

```
┌──────────────────────────────────┐
│ Bram · Stealth · DISADVANTAGE     │
│                                   │
│                12                 │  total, largest text
│                                   │
│ d20:  11 ✓     17 (dropped)       │
│ +1 Dex                 [details]  │
│ Why DIS: Chain Mail (fixed)       │
│ Public · server dice              │
└──────────────────────────────────┘
```

On an attack, the result card also has **[ Roll damage ]**, which pre-selects Critical after a natural 20 (D4).

---

## 13. Non-functional requirements

| ID | Category | Requirement |
|---|---|---|
| NFR-01 | Performance | Stored roll → visible on other connected devices ≤ 1 s p95 (same region, normal Wi-Fi or 4G). |
| NFR-02 | Performance | Command sent → updated sheet on the author's device ≤ 500 ms p95. |
| NFR-03 | Performance | After a reconnect, state is current within ≤ 3 s p95, for a 4-hour session. |
| NFR-04 | Performance | Snapshot load ≤ 1 s p95 (10 members, 100 feed entries). Roll sheet opens in ≤ 100 ms. |
| NFR-05 | Capacity | ≤ 10 members per campaign; ≤ 3 connected devices per user per campaign; ≤ 50 custom entries and ≤ 200 inventory items per character. |
| NFR-06 | Capacity | One instance handles ≥ 50 concurrent campaigns at normal table activity (load test before M5 ends). |
| NFR-07 | Reliability | No acknowledged command is lost on a restart or crash (stored before the acknowledgement). |
| NFR-08 | Reliability | Deploys happen outside announced game nights. Clients survive a restart through catch-up. |
| NFR-09 | Reliability | Daily database backups with 7-day retention. A restore is tested once before the first real session. |
| NFR-10 | Security | Authorization is checked on every API call and every hub method call, not only on connect: membership, role, ownership. |
| NFR-11 | Security | Cookie session: HttpOnly, Secure, SameSite=Lax. Antiforgery on state-changing HTTP endpoints. Same-origin SPA (ADR-0009). |
| NFR-12 | Security | Visibility filtering lives in one place, used by live push, catch-up and snapshot. |
| NFR-13 | Security | Invite tokens: ≥ 128-bit random, stored hashed, revocable. |
| NFR-14 | Security | User text (names, notes) is rendered as plain text. A strict Content Security Policy is set. |
| NFR-15 | Security | Rate limits: more than 10 commands per second per user are rejected; join attempts are limited per IP. |
| NFR-16 | Security | Secrets never go in the repo (user-secrets locally, platform secrets in production). Dependency updates are automated. |
| NFR-17 | Privacy | Logs never contain custom entry notes, tokens or cookies. |
| NFR-18 | Privacy | Only provider, provider user id and display name are stored about users. |
| NFR-19 | Privacy | Account deletion and a privacy notice exist before anyone outside your own group is invited (details in OQ-3). |
| NFR-20 | Privacy | (Should) Data is hosted in an EU region. |
| NFR-21 | Accessibility | WCAG 2.2 AA: contrast, target size, focus, labels (see G3). |
| NFR-22 | Devices | Supported: the last two major versions of iOS Safari and Android Chrome; current desktop Chrome, Edge, Firefox and Safari. |
| NFR-23 | Observability | OpenTelemetry traces, metrics and logs (Aspire service defaults). commandId, campaignId and seq appear on every log line about a command. Metrics: command latency, fan-out latency, reconnects per session, catch-up size, rejected commands by reason. |
| NFR-24 | Quality | Every rule row in §7.1 has ≥ 1 automated test naming its SRD page. Every golden scenario (§7.7) is an automated test. |
| NFR-25 | Quality | `QuestMaster.Rules` has no dependency on ASP.NET, EF Core or I/O, enforced by an architecture test. |
| NFR-26 | Quality | CI runs build, format check, unit, integration (Testcontainers) and E2E (Playwright) tests on every PR. |

---

## 14. What can go wrong

| ID | Risk | What it would look like | Mitigation | Covered by |
|---|---|---|---|---|
| RISK-01 | Phones drop the live connection all the time (lock screen, background, Wi-Fi) | Stale numbers, missed rolls, "it didn't update" | Catch-up on resume, reconnect and every 60 s; visible connection state; actions disabled unless Live | F6, S-9 |
| RISK-02 | Retried commands apply twice | Double damage, duplicate rolls, re-rolling a bad result | CommandId idempotency; a roll is stored before its result is shown | F7, D7 |
| RISK-03 | Catch-up silently skips an event | One phone permanently out of sync | Seq assigned under the campaign lock inside the commit; integration test with concurrent commands | S-5 |
| RISK-04 | A GM-only roll leaks | Other players see a secret result through push, catch-up, snapshot, logs or error messages | One visibility filter for every path; E2E test inspects another player's messages | D6, NFR-12 |
| RISK-05 | Wrong math | Players stop trusting the app | TDD with SRD page references; golden scenarios; breakdown on every number; overrides as the escape hatch; "breakdown adds up" invariant | B11, §7.7 |
| RISK-06 | The effect engine grows without limit | Every new feature wants a new effect type | Closed type list; a new type needs an ADR; custom entries + overrides cover the rest | §9 |
| RISK-07 | Licensing or trademark mistakes | Non-SRD text in the repo, WotC trademarks in branding, missing attribution | SRD-only data with page refs; "5E compatible" wording only; attribution page; PR checklist. (Descriptive mentions in README/CLAUDE.md are fine, but keep trademarks out of the product name and UI.) | G1 |
| RISK-08 | Concurrent edits surprise people | GM and player change HP at once and someone thinks the app is broken | Intent commands + serialized processing; toasts saying who changed what; change log | F4, F7, F8 |
| RISK-09 | Undo restores stale values over newer changes | HP jumps back to an old number | Only the last table action per character can be undone | F8 |
| RISK-10 | Deploy or restart during game night | All phones reconnect at once, short outage | Deploy outside sessions; reconnect jitter; correctness never depends on memory | NFR-08, S-12 |
| RISK-11 | Latency on table Wi-Fi | Rolls feel broken | Small server work per command; "rolling…" feedback; measure p95 in a real session; host in a nearby region | NFR-01 |
| RISK-12 | iOS/Safari evicts PWA storage or suspends the app | Empty offline view, slow resume | Offline cache is best effort; snapshot on resume; server is the source of truth | F9 |
| RISK-13 | OAuth outage, or someone can't log in at game night | A player stuck outside the table | 30-day sliding sessions; two providers; GM-issued one-time join codes in the backlog | A1 |
| RISK-14 | Solo-dev time on a ~45-story MVP with a hand-written domain | MVP never reaches the table | Walking skeleton first; Must/Should; a cut line; every milestone usable on its own | §15 |
| RISK-15 | Flaky real-time tests | CI noise, ignored failures | Deterministic dice in tests; Playwright with several browser contexts and simulated network drops; uniformity test with a documented false-failure rate | ADR-0013 |
| RISK-16 | SRD errata (5.2.x) | Data or rules drift from the official text | `srdVersion` on every record; tests reference pages; a checklist for new SRD releases | G1 |
| RISK-17 | Privacy obligations (EU users) | Personal data kept longer than needed | Minimal data; no email; deletion flow; EU hosting | NFR-17 to NFR-20 |
| RISK-18 | Abuse or flooding | Roll spam, unwanted joiners | Per-user rate limits; GM can remove members and regenerate the invite | NFR-15, A2 |

---

## 15. Milestones and cut line

Every milestone ends with something usable at a real table. Must stories are listed under the milestone that completes them.

| Milestone | Stories | Exit criteria |
|---|---|---|
| **M0 · Walking skeleton** | A1 (one provider + dev login), A2 (link), B1 (minimal), B2 (modifiers), D2 (normal rolls), D7, F1 (basic), F6 (v0), F7, G1, G2, G3 baseline, CI/CD, HTTPS deploy | On the deployed HTTPS URL with real phones: a GM creates a campaign and two players sign in and join by link; player A creates a minimal character and rolls a Dexterity check with server dice; it appears on player B's phone in ≤ 1 s. B locks their phone while A rolls 3 times; on unlock all 3 appear. The duplicate-command test passes. ADRs 0001–0003, 0005–0010, 0012 and 0013 accepted. |
| **M1 · Sheet math** | A2 (QR), B1–B6 (armor, Shield, Unarmored Defense), B8, B10, B11, B12, C1, F2 | All golden scenarios tagged M1 pass. A level-5 multiclass character with armor, Shield and Expertise shows correct numbers, every number has a breakdown, and edits reach a second device in ≤ 1 s. |
| **M2 · Rolling** | B9, D1, D2, D4, D5, D6, F1 | All M2 golden scenarios pass. Every roll type works with server and manual dice from the roll sheet on a phone. The GM-only leak test passes. |
| **M3 · Items and effects** | B6 (magic, custom), C2, C3, C4, C5 | All M3 golden scenarios pass. The attunement limit is enforced. A non-SRD feature built as a custom entry shows correct numbers. |
| **M4 · Combat state and GM actions** | B7, D3, E1–E6, F4 | All M4 golden scenarios pass. Conditions behave as in §7.5. The GM can apply every table action from the character view. |
| **M5 · Live table complete and hardening** | A1 (second provider), A3, F3, F5, F6 (complete), F8, F9, G3 audit, NFR load and soak tests | A 3-hour soak test (1 GM + 6 simulated players, random disconnects) ends with zero divergence between clients and server. One real-table dry run with no lost or duplicated action. |

**Cut line.** If time runs short, drop in this order: (1) F5 Party view → (2) F3 presence → (3) E6 Short Rest (keep the Long Rest) → (4) F8 undo (keep the log) → (5) F9 offline cache.

**Never cut:** F7 idempotency, F6 catch-up, authorization (NFR-10 to NFR-12), G1 attribution.

---

## 16. Out of scope / backlog

**Character and rules**
- Guided character builder; level-up automation (class and subclass features by level)
- Species, background and feat automation, e.g. ready-made entries for SRD feats such as Alert and Skilled (p.87)
- Class features with special math: Jack of All Trades (half PB, p.32), Reliable Talent (p.63), Remarkable Athlete, Danger Sense, Martial Arts, Sneak Attack dice…
- Dice-based effects: Bless and Bane, Guidance, Bardic Inspiration
- Rerolls: the Heroic Inspiration reroll (p.8), Halfling Luck (p.86), with the "only one of the two dice" rule (p.8)
- Tool proficiencies and tool checks, including tool + skill → Advantage (p.9)
- Weapon mastery properties (p.90); two-weapon fighting and the Light-weapon extra attack (p.89)
- Spells: lists, prepared spells, spell slots (including the multiclass table), concentration and Concentration saves
- Limited-use resources (Rage, Second Wind…) and their recharge on rests
- Damage types applied to targets: Resistance, Vulnerability, Immunity
- Rolled HP per level
- Inventory weight, encumbrance and currency
- The full SRD magic item catalog; item charges; cursed items
- Features that change the attunement limit, e.g. the Thief's four attunement slots (p.64)
- "On Long Rest" trigger effects, e.g. Human Resourceful granting Heroic Inspiration automatically (p.86)
- Condition durations; concentration tracking
- Choosing to fail a save without rolling (p.7)
- One-off situational modifiers on the roll sheet
- Monsters, NPCs and the SRD bestiary; initiative order and turn tracker; encounters
- 2014 rules; house-rule toggles

**Rolling and the table**
- Free dice expressions (e.g. `8d6`, `1d20+5`) for the GM and anything not modeled
- GM roll requests (prompt players, hidden DC, pass/fail summary)
- Blind rolls and GM secret rolls
- Dice animations, sounds and haptics
- Session ("game night") markers and recaps

**Platform**
- An offline action queue
- Push notifications; Screen Wake Lock
- Polish UI (and translated SRD names)
- Moving or copying characters between campaigns; character export and import
- Co-GM, GM transfer, spectator role
- Email/password sign-in; GM-issued one-time join codes
- Scale-out (SignalR backplane or a managed SignalR service)
- Native apps
- Sharing homebrew between campaigns (needs a licensing review)
- Maps and other VTT features

---

## 17. Open questions

| ID | Question | Proposed default | Decide by |
|---|---|---|---|
| OQ-1 | Does Exhaustion lower passive Perception? | No. RAW applies the penalty to D20 Tests only (p.181), and a passive score isn't a roll | M4 |
| OQ-2 | What happens to a character when its player leaves or is removed? | It is archived: hidden from the table, visible to and deletable by the GM, no longer accessible to the former player | M5 |
| OQ-3 | How long are the roll feed and change log kept? What does account deletion remove? | Kept for the campaign's lifetime. Deleting an account deletes the user's characters and shows their past rolls as "Former player" | Before inviting anyone outside your own group |
| OQ-4 | Can the GM edit a player's sheet inputs (scores, items)? | No | M5 |
| OQ-5 | Can the GM roll for a player's character, e.g. a secret Perception check? | No (blind rolls are backlog) | M5 |
| OQ-6 | Which OAuth provider comes first in M0, Google or Discord? | The one your group already uses | M0 start |
| OQ-7 | When is the first real session? It decides where the cut line falls | n/a | End of M1 |
| OQ-8 | Should R-1 match custom entries by name (current default), or by an explicit "same as" link the user picks? | By normalized name | M3 |
| OQ-9 | Should a manually chosen roll mode get its own badge, visible to the GM? | Shown in the breakdown only | M2 |
| OQ-10 | Exhaustion level 6: Dead automatically (RAW) or after GM confirmation? | Automatic, with undo available (F8) | M4 |
| OQ-11 | Should the GM get free dice in the feed? | Backlog (free dice expressions); revisit after the first session | After M5 |

---

## 18. Proposed first ADRs

ADRs live in `docs/adr/NNNN-title.md` (Context / Decision / Consequences). Each one should compare 2–3 options with their trade-offs.

| ADR | Title | Decides | Options to compare | Needed by |
|---|---|---|---|---|
| 0001 | Stack confirmation | Confirms the proposed stack and the solution layout | The stack in CLAUDE.md (.NET 10, ASP.NET Core, SignalR, EF Core + PostgreSQL, Aspire, Angular PWA, xUnit, Testcontainers, Playwright, GitHub Actions); project boundaries around `QuestMaster.Rules` | M0 |
| 0002 | SRD content source, licensing and attribution | Where SRD data comes from and how it is versioned | Hand-curated JSON from the SRD 5.2.1 PDF with page refs, vs third-party SRD datasets (provenance, accuracy, errata) | M0 |
| 0003 | Rules engine boundary and roll pipeline | How randomness and dice values enter the pure domain. **Your call, since you write the domain.** | (a) plan → dice → resolve: the domain describes the roll and resolves given dice values, so RNG and manual dice both live outside; (b) inject a random-source interface into the domain; (c) the domain returns only modifiers, and the application layer rolls | M0 |
| 0004 | Effect model and rulings register | Effect types, targets, gates, stacking (R-1), how new types and rulings are added | Closed enum + data, vs a class hierarchy, vs rule objects; where rulings live (this spec, test names, a `docs/rulings.md`) | M1 |
| 0005 | Server-authoritative math and dice | Where calculation and dice happen | Server only (chosen in the interview), vs WebAssembly, vs a TypeScript duplicate | M0 |
| 0006 | Live sync protocol | Hub contracts, groups vs per-user sends, seq, catch-up, snapshot, visibility filtering, versioned sheets | SignalR groups + per-user sends; stateful reconnect on or off; HTTP fallback for catch-up | M0 |
| 0007 | Command processing and concurrency | Serialization, idempotency store, retries, rate limiting | Per-campaign database row lock vs an in-process queue; optimistic concurrency as a safety net | M0 |
| 0008 | Persistence model | How characters, commands, rolls and events are stored | Character aggregate as a JSONB document vs normalized tables; event retention; document schema versioning | M0 |
| 0009 | Authentication and authorization | OAuth providers, session, CSRF, dev login, policies, hub auth | Same-origin cookie session (BFF style) vs JWT in the SPA; policy-based authorization; re-checking membership per hub call | M0 |
| 0010 | Hosting and deployment | Where and how it runs | Container hosts (e.g. Azure Container Apps, a small VPS), managed PostgreSQL, TLS, secrets, backups, deploy windows, the scale-out path | M0 |
| 0011 | PWA and offline | What is cached, and how new versions roll out | Angular service worker config; IndexedDB snapshot; a "new version available" prompt | M5 |
| 0012 | Frontend architecture | State management, real-time client wrapper, i18n library, UI kit | A signals-based store vs NgRx; a runtime i18n library vs `@angular/localize` | M0 |
| 0013 | Testing strategy | Test pyramid and conventions | SRD-referenced rules tests; golden scenarios; property-based tests for formulas; Testcontainers; Playwright with several contexts and network drops; deterministic dice in tests; the RNG uniformity test policy | M0 |

---

## 19. Attribution

This specification summarizes rules from the SRD and must carry its attribution statement, word for word as required on SRD p.1:

> This work includes material from the System Reference Document 5.2.1 (“SRD 5.2.1”) by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2.1 is licensed under the Creative Commons Attribution 4.0 International License, available at https://creativecommons.org/licenses/by/4.0/legalcode.

Notes:
- Wizards asks for no other attribution to Wizards or its affiliates. A statement that a work is "compatible with fifth edition" or "5E compatible" is allowed (SRD p.1).
- The rule summaries in this document are paraphrased and condensed from the SRD, not quoted. CC-BY-4.0 asks that changes be indicated, and this note does that.
- The CC license covers only the SRD material, not Wizards' trademarks.

---

## Appendix A. Interview decision log

Decisions made in the spec interview on 2026-10-18.

| # | Topic | Decision |
|---|---|---|
| 1 | Rules edition | 2024 rules; pinned to SRD 5.2.1 |
| 2 | First real use | In person, phones on the table → phone-first, flaky Wi-Fi assumed |
| 3 | Sheet depth | Smart sheet + SRD class data; no builder or level-up automation |
| 4 | Non-SRD content | Custom entries with real math, private to the campaign |
| 5 | Effect types | Closed core set + about 12 curated SRD magic items |
| 6 | Stacking | Same name → only the most potent applies (R-1) |
| 7 | Conditions | Fixed effects automatic; situational ones confirmed on the roll sheet |
| 8 | Overrides | Labeled adjustments + hard override with ⚠ and reset |
| 9 | Rolls in the MVP | Checks, saves, initiative, weapon attacks + damage with crits, death saves; HP and temp HP. Out: free dice expressions, the Inspiration reroll |
| 10 | Dice source | Server dice + labeled manual entry; the GM can disable manual entry |
| 11 | Live scope | Roll feed, GM sees all sheets live, GM changes player state, party view |
| 12 | Conflicts | Intent commands with a CommandId; plain field edits: newest wins |
| 13 | Offline | Read-only cache + catch-up; no offline actions |
| 14 | Accounts | Google/Discord OAuth + invite link/QR; dev login only outside Production |
| 15 | Ownership | Owned by the player, lives in exactly one campaign |
| 16 | Roll visibility | Public or "to GM only" |
| 17 | History | Change log + undo of the last table action |
| 18 | Where math runs | On the server; computed sheets are pushed to clients |
| 19 | Advantage/disadvantage UX | Roll sheet, 2 taps |
| 20 | GM roll requests | Backlog |
| 21 | Build order | Walking skeleton first |
| 22 | Extra sheet parts | Spell save DC + spell attacks; Short and Long Rests. Out: spell slots, resources |
| 23 | Language | English, i18n-ready |
| 24 | Hosting | One container + PostgreSQL; scale-out path recorded in an ADR |

**Defaults chosen while writing the spec (veto any of them):** Heroic Inspiration is a flag only (E5). Rulings R-2 to R-9. The party view hides death-save counts (F5). One character per player per campaign (B1). Initiative order is backlog. Equip-slot and attunement limits are enforced (C1, C2). Rolled HP is backlog (B8). Campaign-wide (not per-character) command serialization (S-3). Clients tolerate seq gaps and rely on catch-up instead of gap detection, because GM-only filtering makes gaps normal (S-8, S-9).
