# Preliminary Report — Reflec Beat

This report describes the original arcade rhythm game **Reflec Beat** (Konami,
2010–2018), its mechanics, its version history, and how this project maps those
ideas into a clean-room simulator. It is intended as source-material context for
the design documents in this repository.

---

## 1. What is Reflec Beat?

*Reflec Beat* is a touch-screen rhythm game developed and published by Konami
as part of the BEMANI family of arcade music games. It was first released in
Japanese arcades on **4 November 2010** and was notable for being the first
BEMANI title controlled entirely through a touch panel rather than buttons,
 pads, or motion sensors.

The game is structured as a **one-on-one duel**: two players (or one player
versus a CPU opponent) share a single horizontal playfield. Each player has a
judgment line near the bottom of the screen, one side typically coloured red and
the other blue. Circular note objects travel toward the lines, often bouncing
off the left and right walls, and players tap them the moment they arrive. The
"reflect" name comes from this wall-bounce behaviour and from the ability to
send notes back at the opponent.

Reflec Beat ran as an arcade series from 2010 until the final arcade version,
*Reflec Beat: The Reflesia of Eternity*, was released in 2016. Across that span
it received five major arcade revisions plus expanded updates. Online services
for the older versions were discontinued over the years, but — contrary to a
common assumption — the series was never fully shut down: *The Reflesia of
Eternity* remains connected to e-AMUSEMENT in Japanese arcades to this day.
The series' dormancy (no new title or major content since the late 2010s) and
its inaccessibility outside Japan are part of the motivation for this
simulator project. (The project's root documents sometimes
refer to the broader Reflec Beat franchise period as 2010–2020; this report
focuses on the arcade lineage.)

A separate iOS adaptation, **REFLEC BEAT plus**, launched in 2011, followed by
an international release as **REFLEC BEAT +**. These mobile versions were
functionally similar to the arcade series but used a reduced song list and
purchaseable song packs. This simulator targets the arcade gameplay model only.

---

## 2. Core gameplay loop

1. A song begins.
2. Circular **objects** spawn from the far edge of the playfield and travel
toward each player's judgment line.
3. The player taps, holds, or slides the object when it reaches the line or a
designated judgment zone.
4. Timing is graded from **JUST** (best) down through **GREAT**, **GOOD**, and
**MISS**.
5. The run ends with a score, an achievement rate, and (in duel charts) a win/loss
result.

The central skill is **reading the bounce trajectory**. Unlike lane-based rhythm
games, Reflec Beat objects can enter from anywhere along the top edge and
ricochet off the side walls, so the player must track angle and speed rather
than fixed columns. This makes sync and readable motion unusually important — a
principle this simulator inherits directly.

---

## 3. The playfield

- The screen is horizontal (landscape).
- Each player has a judgment line near the bottom, on opposite sides.
- Objects spawn from the top and travel downward.
- Some objects bounce off the left/right walls; others fall straight down.
- In later versions, **Top Objects** are judged in circular zones above the
judgment line rather than on it.

In the arcade cabinet the two players share one large touch monitor; in
single-player mode the CPU controls the opponent side.

---

## 4. Object (note) types

The series added and occasionally removed object types across versions. The
common types are:

| Object | Description |
|--------|-------------|
| **Normal Object** | A circular object that bounces off the side walls on its way to the judgment line. The basic note of the game. |
| **Gold Object** | A normal object with a gold rim. Hitting it with **JUST** timing lets the player perform a **Just Reflec**, sending a fast object to the opponent. Gold objects are the only objects that do not disappear immediately when touched. |
| **Chain Object** | Two or more objects linked by a line. Each object in the chain must be tapped; they land on the same judgment point. |
| **Long Object** | A sustained object that must be held from when its head reaches the line until its tail reaches the line. Comparable to a freeze arrow in *Dance Dance Revolution* or a charge note in *beatmania IIDX*. |
| **Top Object** | A green object that travels to one of several circular judgment zones above the judgment line. Originally two zones (*2TOP*), later three (*3TOP*). |
| **Vertical Object** | Introduced in *groovin'!!*. Falls straight down without bouncing off walls. Can be normal, gold, long, or chained. |
| **Slide Object** | Introduced in *VOLZZA*. Moves straight down with a left/right arrow; the player must hold and follow the slide trail. Removed from *The Reflesia of Eternity* but retained in the iOS version. |
| **Big Bang Object** | Introduced in *The Reflesia of Eternity*. A wide object that falls straight down and shakes the screen on impact. |
| **Switch Long Object** | Introduced in *The Reflesia of Eternity*. A long object with hold/release segments. Only appears on the **White Hard** difficulty mode. |

### Colour and side ownership

- Each player sees their own colour-coded notes (typically red for player 1,
  blue for player 2).
- The two players usually receive different patterns, even though the total
  object count is the same.
- Gold notes and rival notes are the basis of the duel mechanics.

---

## 5. Judgment and scoring

### Timing grades

The arcade series uses four judgment tiers (best to worst):

- **JUST**
- **GREAT**
- **GOOD**
- **MISS**

A late or absent touch registers as MISS.

### Achievement Rate (AR)

The achievement rate is the accuracy metric most players use to measure a run.
For the original through *VOLZZA 2* it is approximately:

```
AR = (JUST × 3 + GREAT × 2 + GOOD × 1) / (Total Objects × 3) × 100
```

The result is rounded to one decimal place. Grades are then assigned from the
achievement rate, for example:

| Grade | Achievement Rate |
|-------|------------------|
| S / SS / AAA+ | 95.0% or higher (exact thresholds vary by version) |
| AAA | 90.0% – 94.9% |
| AA | 80.0% – 89.9% |
| A | 70.0% – 79.9% |
| B | 60.0% – 69.9% |
| C | below 60% |

### Score and duel outcome

In versus play the winner is decided by **score**, not achievement rate. Score
includes:

- judgment points per object,
- **+10** for each successful Just Reflec,
- **−10** when the player misses a Just Reflec object sent by the opponent,
- full-combo / 1-miss / 2-miss bonuses at the end of the song.

Because Just Reflec bonuses are independent of achievement rate, it is possible
to win with a lower accuracy than the opponent if Just Reflecs are used well.
This is the central strategic layer of the game.

---

## 6. Just Reflec and duel mechanics

**Just Reflec** is Reflec Beat's signature competitive mechanic.

- As the player hits objects with good timing, the **Just Reflec Gauge** fills.
- When a segment is full, the player can touch and slide a **gold object** that
  was hit with **JUST** timing.
- The object is then reflected back at the opponent as a faster, harder-to-read
  object.
- Each successful Just Reflec gives bonus score; missing one sent by the rival
  gives a score penalty.

The gauge capacity changed over the series:

- *Reflec Beat* and *limelight*: 3 slots.
- *colette* onwards: 5 slots.

*colette* also introduced the ability to **Just Reflec downward**, bouncing the
object back toward the player first and then to the opponent at higher speed.

In single-player mode the CPU opponent plays its own notes, allowing the same
duel loop to function without a second human player.

---

## 7. Version history

The arcade series went through five major versions, with several seasonal or
append updates in between.

### Reflec Beat (2010)

- Released **4 November 2010** in Japan after location tests in April–July 2010.
- First BEMANI touch-screen game.
- Core object set: Normal, Gold, Chain, Long, and Top (2TOP on higher
  difficulties).
- Supported the Konami **e-AMUSEMENT** network for online play, leaderboards,
  and unlock events.
- e-AMUSEMENT support ended **31 January 2012**.

### REFLEC BEAT limelight (2011)

- Released **16 November 2011**.
- Refined timing and introduced the **Glass** unlock system.
- Participated in the **Lincle LINK** event with *beatmania IIDX 19 Lincle*.
- Last version before some later online-service requirements; it can still run
  offline with the right kit.

### REFLEC BEAT colette (2012–2014)

- Released late 2012, structured around **seasons**:
  - Winter (Nov 2012 – Mar 2013)
  - Spring (Mar – Jun 2013)
  - Summer (Jun – Sep 2013)
  - Autumn (Sep – Dec 2013)
  - All Seasons (Dec 2013 – Jun 2014)
- Each season brought new songs, removed some songs, and re-rated charts.
- Timing was tightened compared with *limelight* but not as strict as the
  original *Reflec Beat*.
- Just Reflec capacity increased to **5 slots**.
- Added **downward Just Reflec**.
- Added **Clear Gauge Type B**, a reversed-life gauge that starts at 100% and
  drains toward the real-time average.
- Gold objects were redesigned for better visibility.
- e-AMUSEMENT support ended **4 July 2014**.

### REFLEC BEAT groovin'!! (2014)

- Released in 2014; an expanded version, **groovin'!! Upper**, followed on
  **20 November 2014**.
- Introduced **Vertical Objects**.
- Added **SPECIAL** difficulty charts (later replaced by White Hard in
  *Reflesia*).
- Introduced **song courses** and **REFLEC Dojo**, a Dan-rating mode similar to
  *beatmania IIDX*'s Dan class system.
- Added the **REFLEC Shop** unlock system using an in-game currency called
  "Refle".
- e-AMUSEMENT support ended **21 December 2015**.

### REFLEC BEAT VOLZZA (2015–2016)

- Announced **26 August 2015**, released **28 October 2015**.
- Expansion **VOLZZA 2** released in 2016.
- Introduced **Slide Objects**.
- Expanded the difficulty scale from the old 1–10+ range to **1–12**.
- Added unlock systems including **VOLZZA KEYs**, **Hakkyujou**, and
  **ULTIMATE SURVIVAL** events.
- Many existing 10+ charts were re-rated to 11 or 12.

### Reflec Beat: The Reflesia of Eternity (2016)

- Released **1 December 2016**.
- Theme shifted to RPG styling.
- Major system overhaul:
  - Removed almost the entire existing song list at launch; old songs were added
    back gradually through online updates.
  - Removed **Slide Objects**.
  - Added **Big Bang Objects** and **Switch Long Objects**.
  - Replaced SPECIAL difficulty with **White Hard**.
  - Changed Top Objects to a fixed three-zone layout.
  - Difficulty scale changed to **1–15**.
- Because the program was rewritten, old player data was incompatible.
- Konami later confirmed *The Reflesia of Eternity* as the **final arcade
  version** of the series, marking the end of the Reflec Beat arcade line.

### Mobile versions

In addition to the arcade series, Konami released two iOS adaptations:

**REFLEC BEAT plus** (Japan)

- Released for iPad on **24 November 2011**, with a free base app and
  purchaseable **MUSIC PACKs** (initially around ¥450 / US$3.99 each).
- Updated to a universal app on **12 August 2012**, adding iPhone and iPod touch
  support.
- Included a small set of pre-installed songs plus paid packs.
- Featured local two-player versus on a single iPad, making it one of the few
  ways to play human-versus-human Reflec Beat without two arcade cabinets.
- Was gradually updated to match arcade mechanics, including SPECIAL charts,
  PASTEL mode, and FULL COMBO mode.
- New music sales and SPECIAL chart purchases **ceased on 5 October 2022**.
  Existing downloads may still work for previous purchasers, but the app is no
  longer receiving new content.

**REFLEC BEAT +** (International)

- Released on **5 November 2012** as the first non-Japanese iOS version.
- Also a free app with paid song packs ($3.99 / £2.49 each).
- Had a **reduced song list** made up only of Konami original songs; licensed
  tracks and many Japan-exclusive packs did not appear.
- The final app update was **version 4.5.2 on 6 March 2017**.
- The in-app shop was later removed, leaving only the bundled/free content
  available. The app remains on regional App Stores for existing users but is
  effectively abandoned.

These mobile versions are **not the target of this simulator**, which focuses on
single-player arcade-style gameplay on Android tablets. They are included here
for completeness because they were the only consumer ports of the game and kept
the gameplay accessible outside arcades for several years.

---

## 8. Discontinuation and preservation context

Reflec Beat's arcade hardware and online services depended on Konami's
e-AMUSEMENT infrastructure. Once e-AMUSEMENT support ended for a given version,
the game became effectively unplayable in its original form unless an offline
kit was available. Support windows were relatively short; even popular versions
such as *colette* and *groovin'!!* had their online services shut down within a
few years of release.

*The Reflesia of Eternity* was the last arcade version, and Konami did not
announce a successor. However, the series was **not** dissolved: as of this
writing, *Reflesia* is still online. Its official e-AMUSEMENT page
(`p.eagate.573.jp/game/reflec/reflesia`) remains live and continues to post
notices, and the game is still network-connected in Japanese arcades — notably
surviving the March 2022 service-termination wave that ended online support
for sibling titles such as *BeatStream*, *MÚSECA*, and *DanceEvolution*.

That said, the series is dormant rather than active: meaningful content
updates wound down around 2018–2019, some licensed songs have since been
removed, and fan campaigns (e.g. "SAVE REFLEC BEAT") exist precisely because
the game is in maintenance mode. Fan projects and simulators exist mainly to
keep the gameplay style accessible outside Japanese arcades and on modern
consumer hardware — not because the original is entirely gone.

This simulator is a **clean-room homage** — it reimplements the gameplay concept
but does not use any Konami audio, charts, artwork, fonts, or code.

---

## 9. Mapping to this simulator

The following table shows which original concepts appear in this simulator and
where to read more about the implementation.

| Original concept | Simulator status | Details |
|------------------|------------------|---------|
| Touch-based timing | Implemented | Core input captured through Unity's touch API, timestamped against `AudioSettings.dspTime`. See `DESIGN.md` §4 and `docs/tech-stack.md`. |
| Judgment line | Implemented | Single-player judgment line near the bottom of the screen. |
| Normal / Tap objects | Implemented | `tap` note type. See `DESIGN.md` §3.3. |
| Long objects | Implemented | `long` note type with `durationTicks`. |
| Vertical objects | Implemented | `vertical` note type. |
| Top objects | Implemented | `top` note type with `zone: 0|1|2`. |
| Big Bang objects | Implemented | `bigbang` note type. |
| Gold objects / Just Reflec | Implemented | `gold` note flag and CPU-rival duel logic in M4. See `PRODUCT.md` and `DESIGN.md` §3. |
| CPU rival | Implemented | Deterministic CPU rival for single-player duel mode. |
| 2P human versus | **Out of scope** | Explicitly excluded; see `DESIGN.md` §1 locked decisions. |
| Slide objects | **Out of scope** | Not currently planned; could be reconsidered post-M6. |
| Switch Long / White Hard | **Out of scope** | Reflesia-only mechanics not in the current roadmap. |
| Online services / unlock events | **Out of scope** | No accounts, profiles, unlocks, or networked play. |
| Themes / skins / visual polish | **Out of scope until M6+** | Placeholder visuals by design; see `DESIGN.md` §1. |

The simulator's chart format (`*.rbchart.json`) deliberately separates **musical
timing** (`tick`, BPM map, `audioOffsetMs`) from **presentation** (spawn point,
bounce path, approach time). This is a response to known weaknesses in earlier
fan simulators such as REFLET MUSE, which lacked an explicit offset field and
made precise syncing difficult. See `DESIGN.md` §8.

---

## 10. References

The facts in this report are drawn from publicly available sources, including:

- Konami's official arcade product pages for the Reflec Beat series.
- Wikipedia entries for *Reflec Beat* and *Reflec Beat Volzza*.
- RemyWiki pages for each arcade version (`remywiki.com`).
- The Reflec Beat Wiki on Fandom (`reflecbeat.fandom.com`).
- Community discussions on Zenius-I-vanisher and other rhythm-game forums.

No Konami copyrighted charts, music, art, fonts, or code were used in the
preparation of this document or of this simulator.

---

## See also

- [`PRODUCT.md`](../PRODUCT.md) — simulator goals, audience, and roadmap.
- [`DESIGN.md`](../DESIGN.md) — implementation source of truth.
- [`README.md`](../README.md) — setup and how to play the simulator.
- [`docs/authoring.md`](./authoring.md) — how to write charts for the simulator.
