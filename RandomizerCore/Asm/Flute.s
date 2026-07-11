.include "z2r.inc"

TWISTER_TILE = $b5   ; tile IDs for frame 0; frame 1 uses TWISTER_TILE+2
TWISTER_PAL  = $02   ; OAM palette attribute (bits 0-1)

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

.org $85A9
  jsr RecordPalaceEntry

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
  lda #$00
  sta TwisterPendingPalaceLocation
  ; skip the main overworld game mode and move to the side view loading
  lda #5
  sta GameMode
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
  ; Direction: OverworldFacingDirection bits
  ; next     $01=right $08=up
  ; previous $02=left $04=down 
  lda OverworldFacingDirection
  ; right ($01) or up ($08)
  and #$09
  bne @go_next
  lda #$0f ; add $0f (== -1 mod 16) to walk the slot list backwards
  bne @scan ; unconditional
@go_next:
  lda #$01 ; add 1 to walk the slot list forwards
@scan:
  sta R7
  ; scan each of the possible palace global spots in the PalaceMappingTable
  lda TwisterCurrentPalaceSlot
  ldx #$10
@next_palace:
  clc
  adc R7
  ; wrap within the 0-15 global slot space
  and #$0f
  tay
  lda PalaceMappingTable, y
  cmp #$ff ; $ff is a place holder indicating no palace at this slot in the world
  beq @advance
    ; Palace exists here. Check whether its gem has been placed.
    sty R6
    ; This table layout was optimized for updating stats (since thats done every frame)
    ; and this doesn't matter how long it takes pretty much
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
    ; Check for great palace ID and skip it (palace 6)
    cmp #6
    bcs @advance
    ; A = CrystalPlaced index 0-5
    tay
    lda CrystalPlaced, y
    bne @crystalPlacedAtPalace
    ; Check next palace slot in the real locations table
    ldy R6
@advance:
  tya
  dex
  bne @next_palace
    rts ; this shouldn't happen. tornado should only spawn if a gem is placed
@crystalPlacedAtPalace:
  ; R6 = target global slot
  lda R6
  sta TwisterCurrentPalaceSlot
  ; clear the twister slot
  ldx OverworldCurrentSlot
  lda #$00
  sta OverworldEnemy0Type, x
  sta OverworldDemonTimer, x
  ; LocationNumber = $34 + within-region palace code
  lda R6
  and #$03
  clc
  adc #$34
  sta LocationNumber
  ; target region = global slot >> 2
  lda R6
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
@same_region:
  pla
  pla
  jmp AreaLoadTrigger

.reloc
RecordPalaceEntry:
  stx LocationNumber
  ; update anchor when Link enters a palace. Palace area indices are $34-$36
  ; (within-region palace codes 0-2). Combine with the region for a global slot.
  txa
  sec
  sbc #$34
  bcc @done                   ; below $34: not a palace
    cmp #$03
    bcs @done                 ; $37+: not a palace entrance
      ; A = within-region palace code (0-2); global slot = region*4 + code
      pha
      lda RegionNumber
      asl
      asl
      sta TwisterCurrentPalaceSlot
      pla
      ora TwisterCurrentPalaceSlot
      sta TwisterCurrentPalaceSlot
@done:
  rts


.macro TwisterSpawnBody
.local crystal_check, crystal_found, active_check, not_active, free_slot, init_slot, spider, doublereturn
  ; instruction before was CMP #$0F
  beq spider
  ; Check if at least one gem is placed ($078D + palace_code, palace_code = 0-5).
  ldx #$05
crystal_check:
  lda CrystalPlaced, x
  bne crystal_found
  dex
  bpl crystal_check
  bmi doublereturn ; unconditionally exit
crystal_found:
  ; Refuse to spawn a second twister if one is already active.
  ldx #$07
active_check:
  lda OverworldEnemy0Type, x
  cmp #$04
  ; already have a twister so skip processing it
  beq doublereturn
  dex
  bpl active_check
  ; Find a free demon slot (type == 0).
  ldx #$07
free_slot:
  lda OverworldEnemy0Type, x
  beq init_slot
  dex
  bpl free_slot
  inx ; set x = 0 and just overwrite slot 0 if all else fails
init_slot:
  lda #$04
  sta OverworldEnemy0Type, x      ; type 4 = twister
  lda #$40
  sta OverworldDemonTimer, x      ;
  lda OverworldScrollY            ; camera Y
  clc
  adc #$6c                        ; center-screen height
  sta OverworldEnemyYPosition, x  ; world Y
  lda ScrollPosShadow             ; camera X
  sta OverworldEnemyXPosition, x  ; world X = left screen edge (OAM X = 0)
  lda #$02
  sta OverworldDemonXVelocity, x  ; +2/frame rightward
  lda #$00
  sta OverworldDemonYVelocity, x  ; no vertical movement
  ; Twister spawned. Return from flute dispatch (don't fall into L838F).
doublereturn:
  pla
  pla
spider:
  rts                             ; returns to L838F: spider/rock encounter
.endmacro


.segment "PRG1"

; Skip east continent check
.org $8368
  jmp $836f
FREE_UNTIL $836f

.org $838C
  jsr TwisterSpawnPRG1
; The code after the patch point is where it normally branches to if the flute
; is removing a spider or spawning 3 eyed rock
.assert * = $838f

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
.org $838C
  jsr TwisterSpawnPRG2
.assert * = $838f

.reloc
.proc TwisterSpawnPRG2
  TwisterSpawnBody
.endproc
