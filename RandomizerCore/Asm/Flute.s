.include "z2r.inc"

TWISTER_ENEMY_ID = $04
TWISTER_TILE = $b5   ; tile IDs for frame 0; frame 1 uses TWISTER_TILE+2
TWISTER_PAL  = $02   ; OAM palette attribute (bits 0-1)

; Exactly one of the following is defined by ROM.FluteTwisterWarp() to select the mode:
;   FLUTE_WARP_CLEARED_PALACES - warp to palaces whose crystal has been placed
;   FLUTE_WARP_VISITED_PALACES - warp to any palace entered (incl. Great Palace)
;   FLUTE_WARP_VISITED_TOWNS   - warp to any town entered
; The "visited" modes track destinations in the FluteWarpFlags bitfield
; and if none of them are defined, then this file isn't getting built at all.

; Overworld "Area Byte 0" (Y position + flags) table in work RAM, indexed by
; LocationNumber. Bit 7 set marks the slot as a palace/town entrance; when it's
; clear the sideview loader treats the slot as a generic/random-battle area.
OverworldAreaTable = $6a00
; UpdateHiddenPalaceSpot() stores the flute-hidden palace's *revealed* Area Data
; The palace is index 0 at $df68 and the town is index 1 at $df69
HiddenPalaceRevealedAreaUpdate = $df68
.ifdef FLUTE_WARP_VISITED_TOWNS
; In towns mode the hidden destination is a town (New Kasuto), so reveal $df69 instead
; TODO: test what happens if a town is behind 3 eye rock
HiddenRevealAddr = HiddenPalaceRevealedAreaUpdate + 1
.else
HiddenRevealAddr = HiddenPalaceRevealedAreaUpdate
.endif

; Weirdly the game doesn't have a power of two table except in bank 0 and 3
.segment "PRG7"
.reloc
PowersOfTwo:
  .byte $01, $02, $04, $08, $10, $20, $40, $80

.segment "PRG0", "PRG7"

.import RealPalaceNumberTable

ProcessNextOverworldEnemy = $842F
UpdateOverworldSpritePosition = $8397
AreaLoadTrigger = $85eb
OverworldPostLoad = $C72F

; Patches the place where the enemy graphics data is loaded
; so we can draw a custom tornado
.org $8377
  jsr TwisterDrawCheck

; patch the enemy movement handler (this spot checks if its a fairy)
.org $8435
  jsr TwisterVelocityCheck
  nop

; Patch the collision handler to check if we want to warp
.org $83CE
  jsr TwisterHitCheck

; When taking a cross continent flute, we load the overworld and then the dungeon.
; Hook right before we reveal the overworld and teleport to the dungeon
.org OverworldPostLoad
  jsr OverworldPostLoadFluteCheck

.segment "PRG7"

.reloc
OverworldPostLoadFluteCheck:
  lda TwisterPendingPalaceLocation
  bne @enter
    ; just a regular overworld load
    lda #$00
    sta $0726
    rts
@enter:
  sta LocationNumber          ; restore the destination area index
  sta AreaEntranceIndex
  lda #$00
  sta TwisterPendingPalaceLocation
  ; skip the main overworld game mode and move to the side view loading
  lda #5
  sta GameMode
  ; the destination continent's area byte table was just reloaded from ROM, so
  ; reveal the hidden destination now (if that's where we're headed)
RevealHiddenDestIfNeeded:
  ldy LocationNumber
  lda OverworldAreaTable, y
  bmi @done
    lda HiddenRevealAddr
    sta OverworldAreaTable, y
@done:
  rts


.segment "PRG0", "PRG7"

.reloc
TwisterDrawCheck:
  lda OverworldEnemy0Type, x
  cmp #$04
  beq @twister
    tax ; do the original
    rts
@twister:
  ; type 4: write twister tile and palette
  ; Y = slot*16 (OAM base), set before this hook
  lda FrameCounter
  and #$10
  lsr
  lsr
  adc #TWISTER_TILE
  sta $0281, y ; left sprite tile
  adc #2 ; carry is clear
  sta $0285, y ; right sprite tile
  lda #TWISTER_PAL
  sta $0282, y
  sta $0286, y
  pla
  pla
  jmp UpdateOverworldSpritePosition

.reloc
TwisterVelocityCheck:
  lda OverworldEnemy0Type, x
  cmp #$04
  beq @twister
  ; vanilla branch, so do the original cmp instruction and bail
  cmp #$03
  rts
@twister:
  ; constant +1 rightward X velocity
  lda #$01
  sta OverworldDemonXVelocity, x
  ; pop this function off the stack frame and move to the next enemy
  pla
  pla
  jmp ProcessNextOverworldEnemy

.reloc
TwisterHitCheck:
  lda OverworldEnemy0Type, x
  cmp #$04
  beq @do_warp
  ; Vanilla is checking what link is facing here
  lda OverworldFacingTerrain
  rts
@do_warp:
  jmp TwisterWarp

.reloc
TwisterWarp:
  ; clear the twister slot
  ldx OverworldCurrentSlot
  lda #$00
  sta OverworldEnemy0Type, x
  sta OverworldDemonTimer, x
.ifdef FLUTE_WARP_VISITED_TOWNS
  ; TwisterCurrentPalaceSlot holds a town code (0-7). Look up the overworld area-table
  ; index of that town's primary entrance from TownWarpLocationTable which is built
  ; and exported by the C# randomizer code.
  ldy TwisterCurrentPalaceSlot
  lda TownWarpLocationTable, y
  sta LocationNumber
  sta AreaEntranceIndex
  ; target region: codes 0-3 = West (region 0), 4-7 = East (region 2)
  tya
  and #$04
  beq @have_region
    lda #$02
@have_region:
.else
  ; TwisterCurrentPalaceSlot holds a global palace slot (0-15).
  ; LocationNumber = $34 + within-region palace code
  lda TwisterCurrentPalaceSlot
  and #$03
  clc
  adc #$34
  sta LocationNumber
  sta AreaEntranceIndex
  ; target region = global slot >> 2
  lda TwisterCurrentPalaceSlot
  lsr
  lsr
.endif
  cmp RegionNumber
  ; already in the right continent so we don't need to switch continents
  beq @same_region
    ; Different continent so we need to reload the continent
    ldx RegionNumber
    stx PreviousRegion
    sta RegionNumber
    ; the new game mode will load the WorldNumber
    lda #$00
    sta WorldNumber
    lda #$01
    sta GoingOutsideFlag
    lda LocationNumber
    sta TwisterPendingPalaceLocation
    ; AreaLoadTrigger does `inc GameMode` and we want mode 0 so set it to $ff
    lda #$ff
    sta GameMode
    ; the area byte table isn't loaded for the destination continent yet, so the
    ; hidden reveal is deferred to OverworldPostLoadFluteCheck
    bne @load ; unconditional
@same_region:
  ; still in the destination continent, so its area byte table is already live.
  ; reveal the hidden destination if we are warping there
  jsr RevealHiddenDestIfNeeded
@load:
  pla
  pla
  jmp AreaLoadTrigger

; Put the twister spawning into a macro so it can be copied between bank 1 and 2
.macro TwisterSpawnBody
  ; Refuse to spawn a second twister if one is already active.
  ldx #$07
@active_check:
  lda OverworldEnemy0Type, x
  cmp #$04
  ; already have a twister so skip processing it
  bne @skip_active
    jmp @exit
@skip_active:
  dex
  bpl @active_check

  ; Check the player direction to decide which way to walk the destination list.
  ; Direction: OverworldFacingDirection bits
  ; next     $01=right $08=up
  ; previous $02=left $04=down
  lda OverworldFacingDirection
  ; right ($01) or up ($08)
  and #$09
  bne @go_next
  lda #$ff ; add -1 to walk the list backwards
  bne @scan ; unconditional
@go_next:
  lda #$01 ; add 1 to walk the list forwards
@scan:
  sta R7

  ldy TwisterCurrentPalaceSlot
.ifdef FLUTE_WARP_VISITED_TOWNS
  ; Visited towns: walk town codes 0-7.
  ; TwisterCurrentPalaceSlot is already the town slot
  tya
.else
  ; Palace modes: walk palace numbers 0-5/6, then map back to a global slot
  lda RealPalaceNumberTable,y
.endif
  and #7
  sta R6
  ldx #7

@next_dst:
  lda R6
  clc
  adc R7
  and #7
  sta R6
  ; Make sure that we don't warp to something unavailable in the slots
  ; when using a palace warp not all bits are used in the FluteWarpFlags
.ifndef FLUTE_WARP_VISITED_TOWNS
.ifdef FLUTE_WARP_CLEARED_PALACES
  cmp #6 ; skip gp (#6 since theres no crystal) and the unused value 7
.else ; FLUTE_WARP_VISITED_PALACES
  cmp #7 ; only skip the unused value 7
.endif
  bcs @skip_dst
.endif ; .ifndef FLUTE_WARP_VISITED_TOWNS
  tay
  lda FluteWarpFlags
  and PowersOfTwo,y
  bne @found_dst
  dex
  bpl @next_dst
    ; nothing visited yet, so leave
    jmp @exit
@found_dst:
.ifdef FLUTE_WARP_VISITED_TOWNS
  ; town code is used directly as the slot
  lda R6
  sta TwisterCurrentPalaceSlot
.else
  ; R6 = target palace number. Loop the mapping table to find its global slot.
  ldy #$0f
@next_mapping_entry:
  lda RealPalaceNumberTable, y
  cmp #$ff ; $ff means no palace at this slot
  beq @advance
  cmp R6
  beq @foundSlot
@advance:
  dey
  bpl @next_mapping_entry
    ; we couldn't find the mapping? that shouldn't be possible...
    jmp @exit
@foundSlot:
  sty TwisterCurrentPalaceSlot
.endif

@spawn:
  ; Find a free encounter slot
  ldx #$07
@free_slot:
  lda OverworldEnemy0Type, x
  beq @init_slot
  dex
  bpl @free_slot
  inx ; set x = 0 and just overwrite slot 0 if all else fails
@init_slot:
  lda #TWISTER_ENEMY_ID
  sta OverworldEnemy0Type, x
  lda #$40
  sta OverworldDemonTimer, x
  lda OverworldScrollY
  clc
  ; center-screen height where the player is
  adc #$6c
  sta OverworldEnemyYPosition, x
  ; spawn it at the left edge of the screen
  lda ScrollPosShadow
  sta OverworldEnemyXPosition, x
  ; horizontally move at 2px right a frame
  lda #$02
  sta OverworldDemonXVelocity, x
  lda #$00
  sta OverworldDemonYVelocity, x
@exit:
  ; And also check if its time to do spider/3 eye rock encounter
  lda OverworldFacingTerrain
  rts
.endmacro


; If our flute mode needs to save visited locations, then include the following code
.if .defined(FLUTE_WARP_VISITED_PALACES) .or .defined(FLUTE_WARP_VISITED_TOWNS)
.segment "PRG7"

.ifdef FLUTE_WARP_VISITED_TOWNS
.reloc
; Maps from vanilla town ID to the "actual" town present at this spot.
TownWarpLocationTable:
  .byte RealTownAtLocation0, RealTownAtLocation1, RealTownAtLocation2, RealTownAtLocation3
  .byte RealTownAtLocation4, RealTownAtLocation5, RealTownAtLocation6, RealTownAtLocation7
.endif

.org $CBB4
  jsr FluteMarkVisited

.reloc
FluteMarkVisited:
  sta GameMode ; the store we replaced (A == 0 here)
.ifdef FLUTE_WARP_VISITED_TOWNS
  ; 1 = West town, 2 = East town, >=3 = palace
  lda WorldNumber
  cmp #1
  beq @town
  cmp #2
  bne @done
@town:
  ; already region-adjusted to 0-7
  ldy TownNumber
  lda PowersOfTwo,y
  ora FluteWarpFlags
  sta FluteWarpFlags
@done:
  rts
.else ; FLUTE_WARP_VISITED_PALACES
  lda WorldNumber
  cmp #3
  bcc @done ; < 3 means we entered a town/cave, not a palace
    lda RegionNumber
    asl
    asl
    adc PalaceRegionIndex
    tay
    lda RealPalaceNumberTable,y
    cmp #7 ; guard against an unexpected empty slot
    bcs @done
    tay
    lda PowersOfTwo,y
    ora FluteWarpFlags
    sta FluteWarpFlags
@done:
  rts
.endif
.endif


.segment "PRG1"

; Skip east continent check
.org $8368
  jmp $836f
FREE_UNTIL $836f

.org $8387
  jsr TwisterSpawnPRG1

.reloc
.proc TwisterSpawnPRG1
  TwisterSpawnBody
.endproc


.segment "PRG2"

; Skip east continent check
.org $8368
  jmp $836f
FREE_UNTIL $836f

; Same hook at the same address.
.org $8387
  jsr TwisterSpawnPRG2

.reloc
.proc TwisterSpawnPRG2
  TwisterSpawnBody
.endproc

; patch the flute SFX to play the warp whistle tune
.segment "PRG6"
; note durations. the note lasts for the difference between two entries.
; So the first note lasts for $7f - $70 == $0f frames. These are read from
; right to left.
; when the next value >= current value then it doesn't process any more notes
.org $9644
.byte $24, $24, $2C, $34, $60, $70, $7F
; list of pitch index. The pitches in the table aren't in a particular order,
; so look around in it or just replace unused ones if you want to fix the tuning :p
.byte $30, $3c, $4a, $4c, $4e, $30, $2c
; volume table (in the lower nybble). Make the sound fade a little less than vanilla
.org $9670
.byte $51, $51, $52, $52, $53, $55
