# Z2R Diagnostics

Z2R Diagnostics traces seed generation to help you find where a seed gets stuck
and why generation keeps retrying.

## Capture a trace

Run these PowerShell commands from the repository root. Use the seed, flags, base
ROM and player options from the failing run. If it used `-c` or custom rooms,
reproduce with the same config and room files.

```powershell
$seed = 5019  # Replace with the failing seed.
$flags = 'paste the failing flag string here'
$rom = 'C:/emu/NES/roms/Zelda2.nes'
$playerOptions = 'CommandLine/DefaultPlayerOptions.json'
$output = '.artifacts/z2r-trace'
New-Item -ItemType Directory -Force $output | Out-Null
dotnet build CommandLine/CommandLine.csproj -p:Z2RTrace=true
if ($LASTEXITCODE -ne 0) { throw 'Trace build failed.' }
& ./CommandLine/bin/Debug/net10.0/Z2R.exe -f $flags -s $seed -r $rom -o $output -po $playerOptions 2> "$output/trace.log"
```

From a second terminal in the repo root, watch the trace while generation runs.
Press Ctrl+C in the second terminal to stop watching. If you need a debugger
stack, attach before stopping the generator in the first terminal.

```powershell
$trace = '.artifacts/z2r-trace/trace.log'
Get-Content $trace -Tail 30 -Wait
```

The first diagnostic line normally starts with `[Z2R:seed] event=begin`. If it
never appears, check CLI startup errors and confirm you ran the executable from
the trace build. The trace can include spoilers, room maps and RNG state.

## Find the loop

1. Find the latest `[Z2R:timing]` line with `event=begin`. If its matching
   `event=end` has yet to appear, that stage is still active. An end records
   stage exit, including cancellation and exceptions. Check `seed` for the outcome.
2. Find the most specific tag whose counter keeps climbing. Use the
   [sampling schedule](#read-sampled-counters) to compare snapshots of that loop.

   | What repeats | Where to look |
   |---|---|
   | `config.item-allocation` with `phase=initial-roll` or `phase=palace-room-reroll` | `RandomizerConfiguration.Export`. Each line is a roll rejected by `HasEnoughSpaceToAllocateItems`. Compare the start/max container counts and item-pool settings. The late loop rerolls only palace room counts. |
   | `palace.retry`, with the same `palace.validation` failure | The outer palace-set loop in `Hyrule.Randomize`. Follow `failed check=...` to `Palaces.ValidatePalaces` and the related `palace.validation.bfs` lines. `palace.generate` tracks attempts at one palace. |
   | `overworld.connections` or `overworld.connector` | `Hyrule.ProcessOverworld`. Check the connector name, endpoints and excluded continents. |
   | `terrain.retry` or `terrain.reject` | Look at the world, biome and rejection reason. `encoding-retry` can repeat inside a single `Terraform` call. |
   | `overworld.items` and `reach` | Find the repeated failure class, missing items and their locations. `overworld.retry` climbing means the generator is also rebuilding the overworld. |
   | `reach.loop`, `items.loop` or `palace.bfs.loop` | Check whether reachable counts, obtained items or the pending queue are changing within the traversal. |

3. Search the C# code for the tag to find where it is emitted. These commands
   show recent connector retries and their probe locations.

   ```powershell
   Select-String -Path $trace -SimpleMatch '[Z2R:overworld.connector]' | Select-Object -Last 20
   rg -n --fixed-strings 'overworld.connector' RandomizerCore -g '*.cs'
   ```

4. If the trace stops inside a stage and stays quiet, break into the process
   with the debugger. Inspect the active stack within that stage and break again
   after resuming briefly. Repeated stops at the same loop let you inspect its
   exit condition and state. A quiet trace can also mean a single expensive
   operation or a loop without a probe. Add a sampled probe if the stack alone
   leaves the failure unclear.

## Read sampled counters

**Sampled retry probes report at attempts 1, 2, every power of two, and every
multiple of 1,000.** The sequence starts `1, 2, 4, 8, …, 512, 1000, 1024, 2000, …`.
Early samples show how a loop starts. The thousand-attempt samples keep long
runs visible while limiting trace output.

- **Expect gaps.** If the last line says `attempt=512`, the loop has reached
  at least 512 attempts. Its next scheduled line is at 1,000 if that counter
  keeps advancing. If the trace stays quiet, check the stage before deciding
  where to pause in the debugger.
- **Compare the state alongside the count.** Look at the values that matter to
  the exit condition, such as reachable locations or connector candidates. If
  the count climbs while those values keep repeating, inspect what each retry
  can change and verify that the exit condition can be met.
- **Compare samples from the same loop.** Match fields such as `phase`, `world`
  and `reason`. Terrain rejection counts are per world instance and reason.
  The `reach` field `consecutive` starts over after a successful check or a
  change of failure class, even when the outer attempt counter keeps climbing.

Palace-validation failures report every time. Traversal probes such as
`palace.bfs.loop` sample both the call number and progress within that call.
They start reporting on otherwise unsampled calls at 1,000 passes or dequeues.

### Example of a stuck configuration

In one run, the config stage stayed active and the trace repeated these lines
with climbing attempt numbers. The excerpt omits the remaining fields.

```text
[Z2R:timing] stage=config event=begin
[Z2R:config.item-allocation] attempt=1 phase=palace-room-reroll reason=insufficient-item-space hearts=3/8 magic=4/8 mixItems=False ...
[Z2R:config.item-allocation] attempt=2 phase=palace-room-reroll reason=insufficient-item-space hearts=3/8 magic=4/8 mixItems=False ...
```

The phase points to the second allocation loop in `RandomizerConfiguration.Export`.
In this run, the fifth Heart Container in the item pool needed an extra overworld
slot. The pools were separate, so rerolling palace room counts could never free
that slot.

## Trace reference

Each stderr line begins with a tag such as `[Z2R:reach]`. `Trace` escapes embedded
newlines and `Block` prefixes each line. Keep the full trace so you can correlate
the retry and failure lines across tags.

| Tag | Context |
|---|---|
| `seed` | Seed/flags, success/hash, cancellation, link failure or exception |
| `timing` | Stage begin/end and elapsed milliseconds, with invariant decimal formatting |
| `rng` | Main/difficulty state at stage boundaries, core draw deltas; music stream receipt when used |
| `config.item-allocation` | Sampled rejected allocation rolls, phase, start/max containers, item-pool settings and palace room counts |
| `palace` | Palace-set packing result |
| `palace.retry` | Outer palace-set attempt, previous count and packing result |
| `palace.generate` | Individual palace attempt, style, requested size, validity and actual room count |
| `palace.validation` | First failing check among enemy-data limits, rooms/boss, glove, raft and glove candidate |
| `palace.validation.bfs` | Item-room reachability, visited room count and available requirements |
| `palace.validation.rooms` | Rooms that cannot be cleared with the candidate palace item |
| `palace.bfs` | Per-palace item-BFS call/dequeue counts, obtained items and `map:item:visited` lists |
| `palace.bfs.loop` | Queue progress within an item-BFS call |
| `palace.enemies` | Duplicate enemy-layout retries |
| `overworld.retry` | Outer overworld attempt |
| `overworld.connections` | Continent-connection attempt and owning overworld attempt |
| `overworld.connector` | Connector rejection loop, current/other endpoint and excluded continents |
| `overworld.terrain` | Terrain round, retry limit and each continent's `AllReached` value |
| `terrain`, `terrain.retry` | Continent terraform begin/end, cumulative attempts and retry limit |
| `terrain.reject` | Sampled connector, routing, cave-exit and map-size failures by continent and biome. `encoding-retry` reports an oversized map that will be regenerated inside the same Terraform call. `map-too-large` reports a rejected final encoding. Both include byte count and capacity. |
| `terrain.visitation` | Explicitly requested visitation dump |
| `overworld.items` | Item/spell shuffle attempt, limit, overworld and terrain round |
| `reach` | Failure class, consecutive count, per-continent reachability, heart/magic counts and missing items/locations |
| `reach.loop`, `items.loop` | Reachability/item convergence progress |
| `items` | Item-allocation mismatch details |
| `music`, `music.retry` | Music importer details and packing attempts |

Timings include tracing overhead. Use an ordinary build for benchmarking.
RNG counts measure core xoshiro 64-bit draws, including rejection retries and
`NextBytes` chunks. Draw counters stay cumulative across `SetState`. Music uses a
separate RNG stream. Compare RNG states and per-stage deltas to locate divergence
between fixed-input runs.

## Add or verify a probe

Rebuild without `-p:Z2RTrace=true` for ordinary Debug or Release builds. The
compiler omits diagnostic calls, argument evaluation, counters and timing/RNG
objects from those builds. NLog, the `Statistics` project and `UNSAFE_DEBUG`
operate independently of the trace build switch.

When adding probes, call `Diag.Trace`, `Diag.Block` or `Diag.Retry` directly so the
compiler can remove argument evaluation. Put telemetry-only locals, fields,
increments and scope objects inside `#if Z2R_DIAGNOSTICS`. Put the sampling check
before expensive snapshots such as room lists. Use the result of the existing
termination check and values already computed by the loop.

Run the tests in both build modes.

```powershell
dotnet test Tests/Tests.csproj
dotnet test Tests/Tests.csproj -p:Z2RTrace=true
```

The tests inspect compiled generator calls (including async methods and lambdas),
check storage removal and conditional argument evaluation, and verify sampling,
line formatting and RNG receipt behavior. For generator changes, also compare
fixed-input ROMs with tracing disabled/enabled; instrumentation must preserve bytes.
