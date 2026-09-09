# Preliminary Report — Reflec Beat

This report describes Konami’s arcade rhythm game **Reflec Beat** (2010 onward),
its mechanics, its version history, and how this project maps those ideas into a
clean-room simulator. It is intended as source-material context for the design
documents in this repository.

This simulator is a **clean-room homage**. Official product pages and the
surviving e-AMUSEMENT site for *REFLEC BEAT 悠久のリフレシア* (*The Reflesia of
Eternity*) establish the game’s battle-oriented touch-screen format and later
feature set, while mobile storefront materials for *REFLEC BEAT +* show how
Konami adapted the same core concept to consumer touch devices. Any official
screenshots referenced in this document are for interface and layout reference
only — the simulator does not use Konami code, charts, music, artwork, fonts, or
extracted assets.

Community tools such as REFLET MUSE, RMSCreator, and reflec2mouse demonstrate an
existing preservation ecosystem around Reflec Beat; this project remains a
technically independent implementation with its own charting pipeline and
runtime architecture.

---

## 1. What is Reflec Beat?

*Reflec Beat* is a touch-screen rhythm game developed and published by Konami as
part of the BEMANI family of arcade music games. It first appeared in Japanese
arcades on **4 November 2010** and was notable as the first BEMANI title
controlled entirely through a touch panel rather than buttons, pads, or motion
sensors.

> **Reference image (optional):** One official arcade gameplay screenshot showing
> the horizontal duel layout, judgment lines, and incoming objects. Caption:
> “Official REFLEC BEAT arcade gameplay — reference for original playfield layout
> only. This simulator uses no Konami assets.”

The game is structured as a **one-on-one duel**: two players (or one player
versus a CPU opponent) share a single horizontal playfield. Each side has a
judgment line; circular note objects travel toward those lines, often bouncing
off the left and right walls. The series name comes from that wall-bounce
behaviour and from the competitive act of sending objects back at the opponent.

The arcade line ran from 2010 through *The Reflesia of Eternity* (2016), the
final arcade title. Online services for older versions shut down over time, but
— contrary to a common assumption — the series was never fully extinguished:
*Reflesia* still has a live e-AMUSEMENT presence and remains network-connected in
Japanese arcades. Dormancy (little new content since the late 2010s) and limited
access outside Japan motivate fan simulators such as this one.

A separate iOS adaptation, **REFLEC BEAT plus** / **REFLEC BEAT +**, brought the
series to consumer devices with a reduced catalog and paid music packs. This
simulator targets the **arcade-style gameplay model** on Android tablets
(single-player, including optional CPU duel) — not the mobile app’s commercial
content structure or local two-player iPad mode.

---

## 2. Core gameplay loop

1. A song begins.
2. Circular **objects** spawn from the far edge of the playfield and travel
   toward the judgment line (or a designated judgment zone).
3. The player taps, holds, or follows the object when it arrives.
4. Timing is graded **JUST** → **GREAT** → **GOOD** → **MISS**.
5. The run ends with a score, an achievement rate, and (in duel charts) a
   win/loss result.

The central skill is **reading movement**, not only reading fixed columns.
Objects can enter from anywhere along the spawn edge and ricochet off the side
walls, so the player tracks angle, speed, bounce trajectory, and object type at
once. Sync and readable motion are first-class concerns — a principle this
simulator inherits directly.

---

## 3. The playfield

- Landscape orientation; a wide shared field in the arcade cabinet.
- Each player has a judgment line near their near edge (opposite sides in 2P).
- Objects generally approach from the far region; some bounce off left/right
  walls, others fall straight.
- Later versions judge **Top Objects** in circular zones above the main line
  rather than on it.

In the arcade cabinet two humans share one large touch monitor; in single-player
the CPU controls the opponent side. Chart design is partly spatial: authors time
notes to music *and* assign them to different target geometries and motion paths.

---

## 4. Object (note) types

The series added and occasionally removed object types across versions. Commonly
documented categories:

| Object | Description |
|--------|-------------|
| **Normal Object** | Circular object that often bounces off the side walls on its way to the judgment line. The basic note. |
| **Gold Object** | Associated with **Just Reflec**; precise hits enable stronger competitive interactions. |
| **Chain Object** | Linked objects that must be hit in sequence at a shared target area. |
| **Long Object** | Sustained note held from head to tail at the judgment line. |
| **Top Object** | Green object judged in circular zones above the main line (originally 2TOP, later 3TOP). |
| **Vertical Object** | Straight-falling object (*groovin'!!* onward); may be normal, gold, long, or chained. |
| **Slide Object** | Directional hold/follow (*VOLZZA*); removed in *Reflesia*, retained on iOS. |
| **Big Bang Object** | Large full-width object (*Reflesia*). |
| **Switch Long Object** | Advanced long-note variant (*Reflesia* / White Hard). |

### Colour and side ownership

- Each side typically sees colour-coded notes (often red vs blue in arcade 2P).
- Patterns per side can differ even when total object counts match.
- Gold and rival-owned notes underpin duel pressure.

> **Reference image (optional):** A second arcade shot is useful only if object
> types are visually distinguishable (e.g. Top vs standard vs large specials).

---

## 5. Judgment and scoring

### Timing grades

Four tiers (best → worst): **JUST**, **GREAT**, **GOOD**, **MISS**.

### Achievement Rate (AR)

For original through *VOLZZA 2*, AR is approximately:

```
AR = (JUST × 3 + GREAT × 2 + GOOD × 1) / (Total Objects × 3) × 100
```

Rounded to one decimal place. Letter grades then derive from AR (thresholds
varied by version), for example AAA near 90%+, AA near 80%+, and so on.

### Score and duel outcome

In versus play the winner is decided by **score**, not AR alone. Score includes
judgment points, Just Reflec bonuses/penalties, and end-of-song bonuses. Strong
Just Reflec use can win a duel despite lower raw accuracy — the strategic layer
that separates Reflec Beat from pure accuracy games.

---

## 6. Just Reflec and duel mechanics

**Just Reflec** is the series’ signature competitive mechanic:

- Good timing fills a **Just Reflec Gauge**.
- When ready, a gold object hit with **JUST** can be reflected toward the
  opponent as a faster, harder-to-read attack.
- Successful reflects award score; missing a rival’s reflect costs score.

Gauge capacity changed over time (3 slots early; 5 from *colette*). *colette*
also introduced downward Just Reflec (bounce toward self, then opponent at higher
speed).

In single-player the CPU plays rival notes so the same loop works without a
second human. That is the duel model this simulator targets — **not** local or
online human 2P.

---

## 7. Version history

The arcade series progressed through major revisions; charting conventions and
object vocabulary changed with each. A simulator must decide which era it
approximates — this project leans on a mid-to-late vocabulary (tap, long,
vertical, top, bigbang, gold/rival) without claiming full *Reflesia* parity.

### Reflec Beat (2010)

- Released **4 November 2010** after location tests.
- Core set: Normal, Gold, Chain, Long, Top (2TOP on higher difficulties).
- e-AMUSEMENT online play; support ended **31 January 2012**.

### REFLEC BEAT limelight (2011)

- Released **16 November 2011**.
- Glass unlock system; Lincle LINK crossover with *IIDX 19*.
- Can still run offline with the appropriate kit in some setups.

### REFLEC BEAT colette (2012–2014)

- Seasonal structure (Winter → All Seasons).
- Just Reflec capacity **5**; downward Just Reflec; Clear Gauge Type B.
- e-AMUSEMENT support ended **4 July 2014**.

### REFLEC BEAT groovin'!! (2014)

- **Vertical Objects**; SPECIAL difficulty; courses and REFLEC Dojo.
- Upper expansion **20 November 2014**.
- e-AMUSEMENT support ended **21 December 2015**.

### REFLEC BEAT VOLZZA (2015–2016)

- Released **28 October 2015**; VOLZZA 2 in 2016.
- **Slide Objects**; difficulty scale expanded toward **1–12**.

### The Reflesia of Eternity (2016)

- Released **1 December 2016**; final arcade title.
- Major rewrite: song list reset then restored over time; Slide removed;
  **Big Bang** and **Switch Long** added; SPECIAL → **White Hard**; Top fixed to
  three zones; difficulty **1–15**.
- Still represented on Konami’s e-AMUSEMENT site as of this writing.

---

## 8. Mobile versions

Konami released official iOS adaptations under **REFLEC BEAT plus** (Japan) and
**REFLEC BEAT +** (international). App Store listings still describe tapping
moving objects in time with music for score — the same arcade premise on a
consumer screen.

> **Reference image (optional):** One App Store / gameplay screenshot of the iOS
> app, useful for how Konami adapted object scale, judgment lines, and HUD
> density to a smaller touch surface. Caption: “REFLEC BEAT + on iOS — interface
> reference only.”

### REFLEC BEAT plus (Japan)

- iPad launch **24 November 2011**; free base app + paid MUSIC PACKs.
- Universal app **12 August 2012** (iPhone / iPod touch).
- Local two-player versus on a single iPad.
- Updated toward arcade features over time (SPECIAL, PASTEL, FULL COMBO modes).
- New music / SPECIAL sales ceased **5 October 2022**.

### REFLEC BEAT + (International)

- Released **5 November 2012**; paid packs; reduced song list (Konami originals).
- Final update **v4.5.2** (**6 March 2017**); shop later removed; effectively
  abandoned though listings may remain.

These mobile versions are **not the commercial or content target** of this
simulator, which focuses on single-player arcade-style play on Android tablets.
They remain valuable as **readability and UX reference** (music select cards,
results hierarchy, touch targets) without adopting Konami art or 2P iPad mode.

---

## 9. Discontinuation and preservation context

Older arcade versions depended on e-AMUSEMENT; when support ended, those builds
became hard to experience without offline kits. Support windows were often short.

*The Reflesia of Eternity* is the last arcade title and did not receive a
successor, yet the franchise entered **dormancy rather than a clean shutdown**:
the official e-AMUSEMENT site remains live, and the game survived service waves
that ended online support for some sibling titles. Content updates largely wound
down around 2018–2019; fan campaigns exist because the game is in maintenance
mode.

### Related community work (not lineage)

A clean-room simulator is not an emulator of Konami software, and a hardware
bridge is not a gameplay reimplementation — but all sit in the same preservation
ecosystem:

| Project | Role |
|---------|------|
| **REFLET MUSE** / **RMSCreator** | Community simulation and chart editing. |
| **reflec2mouse** | Converts original cabinet touch-panel serial data to mouse events. |

This repository does not share code with those projects. Lessons from earlier
fan tools (e.g. needing an explicit audio offset in the chart format) informed
`*.rbchart.json` design; see `DESIGN.md` §8.

---

## 10. Mapping to this simulator

| Original concept | Simulator status | Details |
|------------------|------------------|---------|
| Touch-based timing | Implemented | Enhanced Touch, timestamped against `AudioSettings.dspTime`. |
| Judgment line | Implemented | Single-player line near the player edge. |
| Normal / Tap objects | Implemented | `tap` |
| Long objects | Implemented | `long` + `durationTicks` |
| Vertical objects | Implemented | `vertical` |
| Top objects | Implemented | `top` with `zone: 0\|1\|2` |
| Big Bang objects | Implemented | `bigbang` |
| Gold / Just Reflec | Implemented | `gold` flag + gauge / reflect in M4 duel mode |
| CPU rival | Implemented | Deterministic rival; single-player only |
| 2P human versus | **Out of scope** | Locked out in `DESIGN.md` / `PRODUCT.md` |
| Slide objects | **Out of scope** | Not on current roadmap |
| Switch Long / White Hard | **Out of scope** | Reflesia-only depth not required for current milestones |
| Online / unlocks / store | **Out of scope** | No accounts or networked play |
| Official charts / audio / art | **Never** | Original or freely licensed content only |
| Visual identity | Original default theme | Soft cinematic menus + dark neon arena; not Konami chrome |

Charts use `*.rbchart.json`: musical timing (`tick`, BPM map, `audioOffsetMs`) is
separate from presentation (spawn, bounce, approach time). See `DESIGN.md` §3.

**Screenshot strategy for this report:** keep visuals limited and purposeful —
at most one arcade shot near §1/§3, one optional object-type shot near §4, and
one iOS shot in §8 — each with an explicit clean-room caption. Do not ship those
images as game assets under `Assets/`.

---

## 11. References

1. Konami. [REFLEC BEAT arcade product page](https://www.konami.com/arcadegames/products/am_reflecbeat_1/).
2. Konami. [REFLEC BEAT limelight product page](https://www.konami.com/arcadegames/products/am_reflecbeat_ll/).
3. Konami. [REFLEC BEAT 悠久のリフレシア e-AMUSEMENT site](https://p.eagate.573.jp/game/reflec/index.html).
4. Konami. [Reflesia information page](https://p.eagate.573.jp/game/reflec/reflesia/p/information/index.html).
5. Wikipedia contributors. [Reflec Beat](https://en.wikipedia.org/wiki/Reflec_Beat).
6. REFLEC BEAT Wiki (Fandom). [Gameplay Informations](https://reflecbeat.fandom.com/wiki/Gameplay_Informations).
7. RemyWiki. Reflec Beat version pages (`remywiki.com`) — chronology and charting notes.
8. Apple App Store. [REFLEC BEAT +](https://apps.apple.com/app/reflec-beat/id514257654).
9. Zenius-I-vanisher. [REFLET MUSE / RMSCreator thread](https://zenius-i-vanisher.com/v5.2/thread?threadid=7176).
10. traP. [Pri REFLET MUSE](https://trap.jp/post/588/).
11. GitHub. [DragonMinded/reflec2mouse](https://github.com/DragonMinded/reflec2mouse).
12. Highway Games. [Reflec Beat Arcade Machine](https://www.highwaygames.com/arcade-machines/reflec-beat-arcade-machine-11913/).

No Konami copyrighted charts, music, art, fonts, or code were used in preparing
this document or this simulator.

---

## See also

- [`PRODUCT.md`](../PRODUCT.md) — goals, audience, and roadmap.
- [`DESIGN.md`](../DESIGN.md) — implementation source of truth.
- [`README.md`](../README.md) — setup and how to play.
- [`docs/authoring.md`](./authoring.md) — writing charts for the simulator.
- [`docs/architecture.md`](./architecture.md) — Core / Audio / Gameplay / UI layers.
