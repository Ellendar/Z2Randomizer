.include "z2r.inc"

TWISTER_ENEMY_ID = $04
TWISTER_TILE = $b5   ; tile IDs for frame 0; frame 1 uses TWISTER_TILE+2
TWISTER_PAL  = $02   ; OAM palette attribute (bits 0-1)

; Overworld "Area Byte 0" (Y position + flags) table in work RAM, indexed by
; LocationNumber. Bit 7 set marks the slot as a palace/town entrance; when it's
; clear the sideview loader treats the slot as a generic/random-battle area.
OverworldAreaTable = $6a00
; UpdateHiddenPalaceSpot() stores the flute-hidden palace's *revealed* Area Data
; The palace is index 0 at $df68 and the town is index 1 at $df69
HiddenPalaceRevealedAreaUpdate = $df68

.segment "PRG0", "PRG7"

.import PalaceMappingTable

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
  sta LocationNumber          ; restore the palace area index ($34 + palace code)
  sta AreaEntranceIndex
  lda #$00
  sta TwisterPendingPalaceLocation
  ; skip the main overworld game mode and move to the side view loading
  lda #5
  sta GameMode
  ; the destination continent's area byte table was just reloaded from ROM, so
  ; reveal the hidden palace now (if that's where we're headed)
RevealHiddenPalaceIfNeeded:
  ldy LocationNumber
  lda OverworldAreaTable, y
  bmi @done
    lda HiddenPalaceRevealedAreaUpdate
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
    ; hidden palace reveal is deferred to OverworldPostLoadFluteCheck
    bne @load ; unconditional
@same_region:
  ; still in the destination continent, so its area byte table is already live.
  ; reveal the palace destination if we are warping there
  jsr RevealHiddenPalaceIfNeeded
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
  bne @skip
    jmp @exit
@skip:
  dex
  bpl @active_check

  ; Check the player direction and find a valid palace to warp to
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
  lda #$01 ; add 1 to walk the slot list forwards
@scan:
  sta R7

  ; skip through the list of crystals placed to find the next palace to go to
  ldy TwisterCurrentPalaceSlot
  lda PalaceMappingTable,y
  jsr @ConvertPalaceIdToMapping
  ldx #5
@next_palace:
  clc
  adc R7
  and #7
  cmp #6 ; skip over the great palace
  bcs @next_palace
  tay
  lda CrystalPlaced, y
  bne @crystalplaced
  tya
  dex
  bpl @next_palace
    ; We haven't placed any crystals, so leave
    jmp @exit
@crystalplaced:
  sty R7 ; use this as a temp storage for the target palace to warp to
  ; store the next palace idx that we will warp to
  ; now that we know what palace we want to go to, loop through the palace mapping table to find the
  ; actual location for it.
  ldy #$0f
@next_mapping_entry:
  lda PalaceMappingTable, y
  cmp #$ff ; $ff is a place holder indicating no palace at this slot in the world
  beq @advance
  jsr @ConvertPalaceIdToMapping
  ; Check if this palace number == the actual palace we are looking for
  cmp R7
  beq @foundPalaceToWarpTo
  ; haven't found the palace mapping table entry for this palace yet, so loop again
@advance:
  dey
  bpl @next_mapping_entry
    ; we couldn't find the mapping? that doesn't seem possible...
    jmp @exit
@foundPalaceToWarpTo:
  sty TwisterCurrentPalaceSlot

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
@ConvertPalaceIdToMapping:
  ; This table layout was optimized for updating stats (since thats done every frame)
  ; and this doesn't matter how long it takes pretty much. So we need to convert
  sec
  sbc #Palace1Offset
  ; div by 3 to get the palace number 0 - 6
  sta R5
  lsr
  lsr
  adc R5
  ror
  lsr
  adc R5
  ror
  lsr
  adc R5
  ror
  lsr
  adc R5
  ror
  lsr
  rts
.endmacro


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
