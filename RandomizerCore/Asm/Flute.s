.include "z2r.inc"

; Flute Twister Warp
; When Link plays the flute on the overworld and neither a river-devil tile
; (OverworldFacingTerrain==$0F) nor the three-eye-rock tile is adjacent, a twister
; spawns at the left screen edge and travels rightward. Touching it
; warps Link into the next or previous gem-placed dungeon in sequence.
; Face right/up = next palace; face left/down = previous palace.
; The anchor is the last dungeon Link entered or warped to (TwisterCurrentPalaceSlot).

TWISTER_TILE = $E0   ; tile ID for frame 0; frame 1 uses TWISTER_TILE+1
TWISTER_PAL  = $02   ; OAM palette attribute (bits 0-1)

.segment "PRG0", "PRG7"

ProcessNextOverworldEnemy = $842F
UpdateOverworldSpritePosition = $8397
AreaLoadTrigger = $85eb

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

.reloc
TwisterDrawCheck:
  lda OverworldEnemy0Type, x  ; reproduce overwritten LDA $82,x
  cmp #$04
  beq @twister
    tax ; do the original 
    rts
@twister:
  ; type 4: write twister tile and palette
  ; Y = slot*16 (OAM base), set before this hook
  lda R7                      ; animation frame (0 or 1)
  clc
  adc #TWISTER_TILE
  sta $0281, y                ; left sprite tile
  sta $0285, y                ; right sprite tile
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
  ; Direction: OverworldFacingDirection bits: $01=right, $08=up = NEXT; $02=left, $04=down = PREV
  lda OverworldFacingDirection
  ; right ($01) or up ($08)
  and #$09
  bne @go_next
  lda #$05 ; add 5 to wrap around and go backwards in the list
  bne @scan ; unconditional
@go_next:
  lda #$01 ; add 1 to go forward in the list
@scan:
  sta R7
  ; a = current palace slot we want to check
  lda TwisterCurrentPalaceSlot
  ; x = 6 is the loop counter for the number of palaces to check
  ldx #$06
@next_palace:
  ; increment or decrement the list but clamp the result between 0 and 5
  clc
  adc R7
  bmi @below_zero
  cmp #$06
  bcc @no_wrap
  sec
  sbc #$06
  jmp @no_wrap
@below_zero:
  clc
  adc #$06
@no_wrap:
  pha                         ; save candidate palace_code
    tay
    lda CrystalPlaced, y        ; gem placed at this palace?
    bne @found
  pla
  dex
  bne @next_palace
  rts ; there should always be a valid palace?? the twister shouldn't spawn otherwise
@found:
  pla ; A = target palace_code
  sta TwisterCurrentPalaceSlot
  ; clear the twister slot
  ldx OverworldCurrentSlot    ; slot index saved at $80 by LDX $80 / STX $80 in L8332
  lda #$00
  sta OverworldEnemy0Type, x
  sta OverworldDemonTimer, x
  ; set LocationNumber to palace area index ($34 + palace_code)
  lda TwisterCurrentPalaceSlot
  clc
  adc #$34
  sta LocationNumber
  
  pla
  pla
  jmp AreaLoadTrigger

.reloc
RecordPalaceEntry:
  ; reproduce overwritten STX $0748
  stx LocationNumber
  ; update anchor when entering a palace (area index $34-$39 = palace codes 0-5)
  txa
  ; negative: not a palace
  sec
  sbc #$34
  bcc @done
    ; >= 6: not a valid palace code
    cmp #$06
    bcs @done
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
  lda #$01
  sta OverworldDemonTimer, x      ; any non-zero value keeps slot active
  lda OverworldScrollY            ; camera Y
  clc
  adc #$70                        ; center-screen height
  sta OverworldEnemyYPosition, x  ; world Y
  lda ScrollPosShadow             ; camera X
  sta OverworldEnemyXPosition, x  ; world X = left screen edge (OAM X = 0)
  lda #$01
  sta OverworldDemonXVelocity, x  ; +1/frame rightward
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

; Hook: Replace BEQ L838F; RTS (3 bytes at $838C) with JSR TwisterSpawnPRG1.
; JSR pushes PC+2 = $838E; RTS inside TwisterSpawnPRG1 returns to $838F = L838F.
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

; Same hook at the same address.
.org $838C
  jsr TwisterSpawnPRG2
.assert * = $838f

.reloc
.proc TwisterSpawnPRG2
  TwisterSpawnBody
.endproc
