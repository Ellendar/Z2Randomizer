
; Remove unused code after we gutted link's movement
FREE "PRG0" [$92BF, $962D)

; .segment "PRG0"
; .org (($10ea - $10) .mod $4000) + $8000
; .byte $17


.segment "PRG0", "PRG7"
.org $D3EC ; patches the main sideview routine right before checking marios code
  jsr SwapMarioCHRBanks
.reloc
SwapMarioCHRBanks:
  jsr BankSwitchMarioCHR
  jmp $8DC3

.reloc
BankSwitchMarioCHR:
  lda ReloadCHRBank
  beq +
;   .repeat 8, I
;     lda CurrentCHRBank + I
;     sta MMC5_CHR_BANK_BASE + I
;   .endrepeat
    ; brk
    ; nop
    lda PlayerChrBank
    sta MMC5_CHR_BANK_BASE + 0
    lda CurrentCHRBank ; we need to write a BG bank register due to MMC5 jank
    sta MMC5_CHR_BANK_BASE + 8
  +
  rts

; patch the link draw routine to skip drawing him with the vanilla code
.org $EC02 ; ldx $11 lda $29,x
  jsr PatchLinkDrawRoutine
  nop

.reloc
PatchLinkDrawRoutine:
  ldx $11
  beq + ; link is slot 0
    lda $29,x
    rts
+
  ; for now assume its always PRG0 (it seems to be at least in my limited testing)
  ; jsr SwapToPRG0

  ; put the player in slot 4 always
  lda #4
  sta CurrentOAMOffset
  
  ; draw the player first so it doesn't ever flicker
  ; first clear out the sprites that are reserved for the player
  ; lda #$f8
  ; sta Sprite_Y_Position + 4
  ; sta Sprite_Y_Position + 8
  ; sta Sprite_Y_Position + 12
  ; sta Sprite_Y_Position + 16
  ; sta Sprite_Y_Position + FireballOffset * 4
  ; sta Sprite_Y_Position + (FireballOffset+1) * 4
  
  ldx ObjectMetasprite
  beq @skipplayer
    ldy #0
    jsr DrawMetasprite
@skipplayer:

  ; While we are here, lets draw the fireball/hammer sprites too
  ldy #ProjectileOffset
  sty $02
@fireballLoop:
    ldx ObjectMetasprite,y
    beq @nofireball
    jsr DrawMetasprite
@nofireball:
    inc R2
    ldy R2
    cpy #ProjectileOffset+2 ; Draw two fireballs and one hammer
    bne @fireballLoop

  ; jsr SwapToSavedPRG
  ; double return
  pla
  pla
  rts


; patch end of link's loading routine
.org $90DB ; sta $69DE
  jsr SetupMarioControl
.reloc
SetupMarioControl:
  sta $69DE
  lda #8
  sta GameEngineSubroutine
  lda #1 ; Ground area type
  sta AreaType
  lda #$28                    ;store value here
  sta VerticalForceDown       ;for fractional movement downwards if necessary
  rts


; Patch link's standard routine
.org $9041
  .word (GameRoutines)

; .reloc
; NewLinkMarioProcess:
  ; TODO: Process all the other stuff to decide what parts of mario update to run
  ; jsr GameRoutines
  ; rts
; .org $9041
;   .word (GameRoutines)

.org $D4F3 ; patch the call to links main routine to set
  jsr PatchLinkMain
.reloc
PatchLinkMain:
  ldx #$00  
  stx $14
  lda $de   ; check if we can move
  beq @notfrozen
    jsr ProcFireball_Bubble    ;process fireballs and air bubbles
    rts
@notfrozen:
  ; Do some checks that happen every frame
  lda $0752 ; link is stuck in the mud
  ora $0753 ; enemy is stuck in the mud
  sta $0753
  ; disable for now
;   lda $050C ; timer for link being hurt
;   beq @notinjured
;     rts
; @notinjured:
;   lda $0503 ; sword slash timer
;   bne @notwaiting
;     rts
; @notwaiting:
  ; decrement timers here cause lazy
  ; Move the timers ahead by a frame as well
  lda TimerControl          ;if master timer control not set, decrement
  beq DecTimers             ;all frame and interval timers
    dec TimerControl
    bne NoDecTimers
  DecTimers:
    ldx #FRAME_TIMER_COUNT    ;load end offset for end of frame timers
    dec IntervalTimerControl  ;decrement interval timer control,
    bpl DecTimersLoop         ;if not expired, only frame timers will decrement
    lda #$14
    sta IntervalTimerControl  ;if control for interval timers expired,
    ldx #ALL_TIMER_COUNT      ;interval timers will decrement along with frame timers
  DecTimersLoop:
      lda Timers,x              ;check current timer
      beq SkipExpTimer          ;if current timer expired, branch to skip,
        dec Timers,x              ;otherwise decrement the current timer
    SkipExpTimer:
      dex                       ;move onto next timer
      bpl DecTimersLoop         ;do this until all timers are dealt with
NoDecTimers:

  ; Now do a few checks to see what state mario is in.
  jsr $903A ; link main

  ; This has to come before the A/B buttons are switched over
  jsr ProcFireball_Bubble    ;process fireballs and air bubbles

  lda A_B_Buttons            ;save current A and B button
  sta PreviousA_B_Buttons    ;into temp variable to be used on next frame
  lda #$00
  sta Left_Right_Buttons     ;nullify left and right buttons temp variable

  jsr SetPlayerDownstabbingHitbox

  ; jsr CheckSideviewTransition ; this is already run during collision detection
  jsr PlayerGfxHandler

  rts

.reloc
SetPlayerDownstabbingHitbox:
  ; jsr $ed02 ; attempt to downstab while falling
  jsr CheckForDownstab
  bcc +
    LDX      #0
    JSR      $ECEA
    CLC
    ADC      #$25 - 4 ; - 2 cause I don't know
    STA      $0480 ; ,x
    LDA      $CC ; ,x
    CLC
    ADC      #$10 ;  + 8 ; + 8 cause UGH
    STA      $047E ; ,x
  +
  rts

; Prevent recoil and playing the sword sound when hitting a well
.org $E268
  BCS $E28B
.org $E273
  jmp $E28B

.org $E4A8
  jsr PatchFireballHitcheck
.segment "PRG7"
.reloc
PatchFireballHitcheck:
  jsr $E694 ; original hitbox check
  bcc @exit
    ldy $11 ; fireball
    lda #%10000000
    sta Fireball_State,y
@exit:
  rts

; .org $E558
;   rts ; disable link shield maybe?
; .org $E563
;   rts ; disable link shield maybe?
; .org $E579
;   rts ; disable link shield maybe?

; Replace vanilla beam/fire code with our new 
.org $d50d
  ; jsr CustomProjectileCode
  jmp *+3

; .reloc
; CustomProjectileCode:
;   jsr ProcFireball_Bubble    ;process fireballs and air bubbles
;   rts
; .org $9370
;   ; TODO this is where link takes damage
;   jmp GameRoutines

; .org $937e
;   nop ; don't do this sword attack check
;   nop ; todo see what should be here instead
;   nop
; ; there's a branch here from above to call the main game routine
;   jmp GameRoutines 

; .org $93ba
;   rts ; patch the link main routine to skip some unneeded stuff

; Patch clearing out the ram for sideviews to also clear our new ram block
.org $C58F
  jsr ClearCollisionRAM
.segment "PRG7"
.reloc
ClearCollisionRAM:
  sta $07AF
  ldy #$d0
  lda #0
@loop:
    sta COLLISION_TILES-1,y
    sta COLLISION_TILES-1+$d0,y
    sta COLLISION_TILES-1+$d0*2,y
    sta COLLISION_TILES-1+$d0*3,y
    dey
    bne @loop
  rts

.org $C629
  jsr UpdateCollisionFromMap
.reloc
UpdateCollisionFromMap:
  sta $0735
  ldx #$d0
@loop:
    lda $6000-1,x
    jsr CheckForSolidMTiles
    bcc @skip1
      sta COLLISION_TILES-1,x
@skip1:
    lda $6000-1+$d0,x
    jsr CheckForSolidMTiles
    bcc @skip2
      sta COLLISION_TILES-1+$d0,x
@skip2:
    lda $6000-1+$d0*2,x
    jsr CheckForSolidMTiles
    bcc @skip3
      sta COLLISION_TILES-1+$d0*2,x
@skip3:
    lda $6000-1+$d0*3,x
    jsr CheckForSolidMTiles
    bcc @skip4
      sta COLLISION_TILES-1+$d0*3,x
@skip4:
    dex
    bne @loop
  rts

.reloc
SlightlyModifiedCollisionRoutine:
  jsr SwapToSavedPRG
  jsr $E095 ; skip ahead to near the end of the collision check
  jmp SwapToPRG0

; .org $E073
  ; jsr PatchCollisionCheck
  

; .segment "PRG7"
; .reloc
; PatchCollisionCheck:
  ; lda PlayerStandingMetatile
  ; sta $03 ; will be checked to see what metatile you stepped on
  ; jmp $E095 ; in collision but after the metatile collider

; Recenter mario's bg collision hitbox when checking what position he is at.
; This changes where marios "center" metatile to match better with his visuals
.org $EAB8
.byte $08 ; was $0f

; Update spiky boi jump graphics to move out of the top bank
.org $ee7b
.byte $40, $40

; In vanilla, when link goes off screen to the left, it will go to page $ff, so the
; game has code that bumps the player up a page to counter that offset
; .org $e177 ; inc Player_PageLoc
  ; nop
  ; nop

; Update palette locations
.ifdef DEMO_CODE
  UPDATE_BYTE $37 @ $285a $2a0a $40af $40bf $40cf $40df $80af $80bf $80cf $80df $c0af $c0bf $c0cf $c0df $c0ef $100af $100bf $100cf $100df $140af $140bf $140cf $140df $17c19 $1c464 $1c47c
  UPDATE_BYTE $27 @ $285b $2a0b $2a10 $40b0 $40c0 $40d0 $40e0 $80b0 $80c0 $80d0 $80e0 $c0b0 $c0c0 $c0d0 $c0e0 $c0f0 $100b0 $100c0 $100d0 $100e0 $140b0 $140c0 $140d0 $140e0 $17c1a $1c465 $1c47d
  UPDATE_BYTE $16 @ $285C $40b1 $40c1 $40d1 $80e1 $80b1 $80c1 $80d1 $80e1 $c0b1 $c0c1 $c0d1 $c0e1 $100b1 $100c1 $100d1 $100e1 $140b1 $140c1 $140d1 $140e1 $17c1b $1c466 $1c47e
.else
  UPDATE_BYTE $16 @ $285a $2a0a $40af $40bf $40cf $40df $80af $80bf $80cf $80df $c0af $c0bf $c0cf $c0df $c0ef $100af $100bf $100cf $100df $140af $140bf $140cf $140df $17c19 $1c464 $1c47c
  UPDATE_BYTE $27 @ $285b $2a0b $2a10 $40b0 $40c0 $40d0 $40e0 $80b0 $80c0 $80d0 $80e0 $c0b0 $c0c0 $c0d0 $c0e0 $c0f0 $100b0 $100c0 $100d0 $100e0 $140b0 $140c0 $140d0 $140e0 $17c1a $1c465 $1c47d
  UPDATE_BYTE $18 @ $285C $40b1 $40c1 $40d1 $80e1 $80b1 $80c1 $80d1 $80e1 $c0b1 $c0c1 $c0d1 $c0e1 $100b1 $100c1 $100d1 $100e1 $140b1 $140c1 $140d1 $140e1 $17c1b $1c466 $1c47e
.endif

; regular palette color location
UPDATE_BYTE $18 $10ea
; shield spell color location
UPDATE_BYTE $02 @ $0e9e


; Replace writing registers directly with an output buffer to mix with the regular driver output.
; RESERVE SfxOutputBuffer, 12

; SND_SQUARE1_BUFFER      = SfxOutputBuffer
; SND_SQUARE2_BUFFER      = SfxOutputBuffer + 4
; SND_NOISE_BUFFER        = SfxOutputBuffer + 8

;.macro SFXInit
;
;; .if ::USE_VANILLA_SFX
;  ldy #SFXMemoryEnd - SFXMemoryStart
;:   sta SFXMemoryStart,y     ;clear out memory used
;    dey                   ;by the sound engines
;    bpl :-
;; .endif
;
;.endmacro
;
;.macro SFXPlayback
;  jsr SFXSoundEngine
;.endmacro

;-------------------------------------------------------------------------------------
.segment "PRG6"
.org $9016
  jsr CheckMarioSq1Sfx
  jsr CheckMarioSq2Sfx
  jsr CheckMarioNoiseSfx

.org $9022
  jsr ClearQueues
  nop
.reloc
ClearQueues:
  lda #0
  sta Square1SoundQueue
  sta Square2SoundQueue
  sta NoiseSoundQueue
  rts

.reloc
CheckMarioSq1Sfx:
  jsr Square1SfxHandler
  lda Square1SoundBuffer
  beq @PlayingZ2
    lda #$ff
    sta Z2Square1SoundBuffer
    rts
@PlayingZ2:
  lda #$ff
  cmp Z2Square1SoundBuffer
  bne @ActualZ1Sfx
    ; this is the fake sfx id we used to signal that this was a mario buffer
    ; but now we can clear it.
    lda #0
    sta Z2Square1SoundBuffer
    rts
@ActualZ1Sfx:
  jmp $92F4 ; Original square 1 sfx from z2
.reloc
CheckMarioSq2Sfx:
  jsr Square2SfxHandler
  lda Square2SoundBuffer
  beq @PlayingZ2
    lda #$ff
    sta Z2Square2SoundBuffer
    rts
@PlayingZ2:
  lda #$ff
  cmp Z2Square2SoundBuffer
  bne @ActualZ2Sfx
    ; this is the fake sfx id we used to signal that this was a mario buffer
    ; but now we can clear it.
    lda #0
    sta Z2Square2SoundBuffer
    rts
@ActualZ2Sfx:
  jmp $9408 ; Original square 2 sfx from z2
.reloc
CheckMarioNoiseSfx:
  jsr NoiseSfxHandler
  lda NoiseSoundBuffer
  beq @PlayingZ2
    lda #$ff
    sta Z2NoiseSoundBuffer
    rts
@PlayingZ2:
  lda #$ff
  cmp Z2NoiseSoundBuffer
  bne @ActualNoiseSfx
    ; this is the fake sfx id we used to signal that this was a mario buffer
    ; but now we can clear it.
    lda #0
    sta Z2NoiseSoundBuffer
    rts
@ActualNoiseSfx:
  jmp $95A7 ; Original "complex" sfx from z2

; .reloc
; CheckMarioSfx:
;   lda Square1SoundQueue
;   ora MarioSquare1SoundBuffer
;   ora Square2SoundQueue
;   ora MarioSquare2SoundBuffer
;   ora NoiseSoundQueue
;   ora MarioNoiseSoundBuffer
;   beq @NoMarioSfx
;     jsr SFXSoundEngine
;     sta MarioSquare1SoundPlaying
;     jmp $901C
; @NoMarioSfx:
;   ; P
;   jsr $92F4 ; Zelda 2 square 1 sfx processing
;   rts

.segment "PRG7"
; Make mario state "on ground" when riding an elevator
.org $d8cd
  jsr ElevatorMakeMarioStateStanding
.reloc
ElevatorMakeMarioStateStanding:
  sta $0479
  sta Player_State
  rts

; Make mario "downstabbing" while falling
; .org $ED02
;   jsr CheckForDownstab
;   bcc $ED1B
;   nop

; Make mario bounce off armored enemies
.org $E65E
  jsr CheckForDownstab
  bcc $E66A
  LDA      #$fc
  STA      $057D
  rts
.assert * <= $E66A

; Make mario bounce off armored enemies
.org $E714
  jsr CheckForDownstab
  bcc $E723
  LDA      $0B
  BNE      $E723
  LDA      #$fc
  STA      $057D
  nop
.assert * = $E723

.org $E9C0
  jsr InyIfDownstabbing
  nop
  nop

.reloc
InyIfDownstabbing:
  jsr CheckForDownstab
  bcc +
    iny
  +
  rts

.reloc
CheckForDownstab:
  lda Player_State
  cmp #1
  bne @notjumping
  lda Player_Y_Speed
  beq @notjumping
  bmi @notjumping
    sec
    rts
@notjumping:
  clc
  rts

.segment "PRG3"
.org $9A42
  jmp * + 4 ; skip a hardcoded metasprite update when reading signs

