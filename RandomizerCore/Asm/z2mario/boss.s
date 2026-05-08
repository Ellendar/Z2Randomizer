

.include "z2r.inc"

.import METASPRITE_BIG_MARIO_SKIDDING, METASPRITE_BIG_MARIO_JUMPING, METASPRITE_FIRE_MARIO_STANDING, METASPRITE_BIG_MARIO_STANDING
.import DrawMetasprite, METASPRITE_BIG_MARIO_WALKING_1
.import SwapPRG, SwapToSavedPRG, SwapToPRG0

.export InjuredBoss

.import METASPRITE_BIG_MARIO_CROUCHING
CROUCHING = METASPRITE_BIG_MARIO_CROUCHING
SKIDDING = METASPRITE_BIG_MARIO_SKIDDING
JUMPING  = METASPRITE_BIG_MARIO_JUMPING
SHOOT    = METASPRITE_FIRE_MARIO_STANDING
STANDING = METASPRITE_BIG_MARIO_STANDING
WALKING = METASPRITE_BIG_MARIO_WALKING_1
mus_bossdie = 8

; Boss RAM size for clearing loop
;BOSS_RAM_SIZE = 11  ; boss_animation through boss_freeze (12 bytes, 0-indexed = 11)

.segment "PRG5", "PRG7"

; Patch the spot that "kicks" mario shadow out and use it
; as an init routine
.org $985d
  jsr PatchBossInit
.reloc
PatchBossInit:
  sta EnemyYVelocity
  jmp InitShadowBoss

; Patch the location where it draws the boss
.org $9a7f
  jsr BossGraphicsHandler

.org $9871
  jsr BossLandedInCutscene
.reloc
BossLandedInCutscene:
  sta $0505
  lda #CROUCHING
  sta boss_animation
  rts

.segment "PRG5"
.org $988e
  jsr RunBoss
  jmp $9a77
FREE_UNTIL $9a77

; Patch bank5_Enemy_Vulnerability_Damage_Codes entry $23 (Dark Link):
; Clear bit 5 "Immune to Flying Blade and Fire" so fireballs can hit the boss.
.org $951C
  .byte $14

; Make dark mario use a different hitbox (vanilla has a jank $0e hitbox, switch
; to a simple tall enemy hitbox with $00)
.org $9540
  .byte $00

.segment "PRG5", "PRG7"
.reloc
InitShadowBoss:
      ; Clear all boss RAM
      lda #0
      ldx #BOSS_RAM_SIZE
  @loop:
      sta boss_animation,x
      dex
      bpl @loop
      ldx ObjectOffset
      ; Set HP
      lda #120
      sta Enemy_HP,x
      lda #JUMPING
      sta boss_animation
      lda #CHR_BIGMARIO
      sta boss_ChrBank
      inc ReloadCHRBank

      ; Face left
      ldx ObjectOffset
      lda #2
      sta Enemy_MovingDir,x
      rts


.segment "PRG7"
.import BlockBufferCollision
.reloc
; Trimmed down version that works just well enough for what i need
EnemyToBGCollisionDet:
        lda NmiBankShadow8
        pha
        lda NmiBankShadowA
        pha

        lda #0
        jsr SwapPRG

        lda #0                  ;set flag in A for save vertical coordinate
        ldy #1                  ;set Y to check the bottom middle of enemy object (big mario's feet position)
        ldx ObjectOffset        ;get object offset converted to enemy offset
        inx
        jsr BlockBufferCollision  ;do collision detection subroutine for sprite object
        tax

        pla
        sta NmiBankShadowA
        sta PrgBankAReg
        pla
        sta NmiBankShadow8
        sta PrgBank8Reg
    cpx #$00                  ;check to see if object bumped into anything
    bne EnemyLanding
    rts

EnemyLanding:
    ldx ObjectOffset          ;get object offset
    lda #$00                    ;initialize vertical speed
    sta Enemy_Y_Speed,x         ;and movement force
    sta Enemy_Y_MoveForce,x
    lda Enemy_State,x
    and #%10111111 ; land the boss on the ground
    sta Enemy_State,x
    lda Enemy_Y_Position,x
    and #%11110000          ;save high nybble of vertical coordinate, and
    ; ora #%00001000          ;set d3, then store, probably used to set enemy object
    sta Enemy_Y_Position,x  ;neatly on whatever it's landing on
    rts

.segment "PRG5", "PRG7"
.reloc
RunBoss:
  lda #0
  sta $de ; unfreeze link
      lda boss_freeze
      beq @notfreezed
      dec boss_freeze
      lda #STANDING
      sta boss_animation
      ; Draw every other frame (flicker)
      lda FrameCounter
      lsr
      bcs @nodraw2
;      jsr RelativeEnemyPosition
      jsr BossGraphicsHandler
  @nodraw2:
      ldx ObjectOffset
      rts

  @notfreezed:
      lda Enemy_HP,x
      bmi AnimateKilledBoss     ; HP < 0: boss is fully dead
      bne @alive
      jmp KillBoss              ; HP == 0: playing death countdown
  @alive:

      lda #80
      sta boss_counter          ; reset death countdown each frame while alive
;      lda #0
;      sta EnemyFrenzyBuffer     ; suppress enemy frenzy spawns

      ; Draw (skip every other frame if invincible for blink effect)
      lda boss_invincibility_timer
      beq @draw
      lda FrameCounter
      lsr
      bcs @nodraw
  @draw:
;      jsr RelativeEnemyPosition
      jsr BossGraphicsHandler
  @nodraw:
      ldx ObjectOffset

      lda TimerControl
      bne @nograv               ; skip movement if timer frozen

      jsr EnemyToBGCollisionDet ; background collision
      jsr HandleBossMovement    ; AI state machine

      lda Enemy_State,x
      and #%01000000            ; check airborne flag
      beq @nograv
      jsr MoveD_EnemyVertically ; apply gravity
      lda #JUMPING
      sta boss_animation
  @nograv:

      ; Collision detection (only when not invincible)
      lda boss_invincibility_timer
      bne @decrement_timer
        jmp BossPlayerCollision

  @decrement_timer:
      dec boss_invincibility_timer
      rts
AnimateKilledBoss:
  inc $63
  rts

.reloc
HandleBossMovement:
      lda boss_state
      jsr JumpEngine
      .word MovBossSide      ; state 0
      .word MovBossRunning   ; state 1
      .word MovBossMiddle    ; state 2

.reloc
MovBossSide:
      lda #STANDING
      sta boss_animation

      ; Random fireball: every 8th frame, 1/4 chance
      lda FrameCounter
      and #%00000111
      bne @no_fireball
      lda RNG
      and #%00000011
      bne @no_fireball
      jsr BossSpawnFireball
  @no_fireball:

      ; Random jump: every 8th frame (offset 4), 1/2 chance
      lda Enemy_State,x
      and #%01000000
      bne @no_jump
      lda FrameCounter
      and #%00000111
      cmp #4
      bne @no_jump
      lda RNG
      lsr
      bcc @no_jump
      lda #<(-5)
      sta Enemy_Y_Speed,x
      jsr BossJump
  @no_jump:

      ; State transition when timer expires
      lda boss_state_timer
      bne @keep_state

      lda #0
      sta boss_substate
      lda FrameCounter
      clc
      adc Player_X_Position
      lsr
      lsr
      bcs @running
      lda #2                    ; -> state 2 (middle)
      .byte $2c                 ; skip next instruction (BIT abs trick)
  @running:
      lda #1                    ; -> state 1 (running)
      sta boss_state
      inc boss_bridge

  @keep_state:
      dec boss_state_timer
      rts

.reloc
MovBossRunning:
      jsr BossSetWalkingAnim

      lda Enemy_MovingDir,x
      cmp #$01
      beq MovBossRight

      ; Moving left
      lda Enemy_X_Position,x
      cmp #11*16
      bcs @StillAccer
      cmp #4*16
      bcs MovStillMoving
  @Deaccelerating:
      lda #SKIDDING
      sta boss_animation
      inc Enemy_X_Speed,x
      inc Enemy_X_Speed,x
      lda Enemy_X_Speed,x
      bmi MovStillMoving
      jmp MovChangeState
  @StillAccer:
      dec Enemy_X_Speed,x
      dec Enemy_X_Speed,x
      jmp MovStillMoving

MovBossRight:
      lda Enemy_X_Position,x
      cmp #4*16
      bcc @StillAccer
      cmp #11*16
      bcc MovStillMoving
  @Deaccelerating:
      lda #SKIDDING
      sta boss_animation
      dec Enemy_X_Speed,x
      dec Enemy_X_Speed,x
      lda Enemy_X_Speed,x
      bpl MovStillMoving
      jmp MovChangeState
  @StillAccer:
      inc Enemy_X_Speed,x
      inc Enemy_X_Speed,x

MovStillMoving:
      lda boss_bridge
      beq @no_jump
      dec boss_bridge
      lda Enemy_State,x
      and #%01000000
      bne @no_jump
      lda #<(-4)
      sta Enemy_Y_Speed,x
      jsr BossJump
  @no_jump:
      jmp MoveEnemyHorizontally

MovChangeState:
      lda RNG
      and #$f0
      sta boss_state_timer      ; random duration for next idle
      lda #STANDING
      sta boss_animation
      lda #0
      sta boss_state            ; -> state 0 (side/idle)
      lda Enemy_MovingDir,x
      eor #%00000011            ; flip direction
      sta Enemy_MovingDir,x
      rts

.reloc
MovBossMiddle:
      lda boss_substate
      beq MovGoToMiddle         ; substate 0: walk to center
      ; substate 1: middle action (shoot bullets, wait for enemies to die)

      lda #STANDING
      sta boss_animation
      lda boss_state_timer
      bne @not_ready

      ; Timer expired: check if all enemy slots 1-4 are clear
      ldy #1
  @loop:
      lda Enemy_Flag,y
      bne @notyet               ; slot still occupied, wait
      iny
      cpy #5
      bcc @loop

      ; All clear: maybe spawn powerup
;      lda Player_State
;      cmp #2
;      bcs @no_need_for_p        ; player already has fire power
;      sta PowerUpType
;      lda #PowerUpObject
;      cmp Enemy_ID+5
;      beq @no_need_for_p        ; powerup already exists
;      ; Spawn powerup in slot 5
;      ldy #5
;      lda #PowerUpObject
;      sta Enemy_ID,y
;      lda Enemy_X_Position,x
;      sta Enemy_X_Position+5
;      lda Enemy_Y_Position,x
;      sta Enemy_Y_Position+5
;      lda #0
;      sta Enemy_PageLoc+5
;      sta Enemy_X_Speed+5
;      lda #$01
;      sta Enemy_Y_HighPos+5
;      sta Enemy_Flag+5
;      lda #$80
;      sta Enemy_State+5
;      lda #$03
;      sta Enemy_BoundBoxCtrl+5
;      lda #<(-3)
;      sta Enemy_Y_Speed+5
  @no_need_for_p:
      lda #0
      sta boss_bridge
      lda #1
      sta boss_state            ; -> state 1 (running)
      ; Set speed based on facing direction
      lda Enemy_MovingDir,x
      lsr
      bcs @right
      lda #<(-$20)
      .byte $2c                 ; skip next instruction (BIT abs trick)
  @right:
      lda #$20
      sta Enemy_X_Speed,x
  @notyet:
      rts

  @not_ready:
      dec boss_state_timer
      ; Every 16 frames, spawn a bullet bill from a random Y position
      lda FrameCounter
      and #%00001111
      bne @done
      ; Random Y position
      lda RNG
      and #$70
      clc
      adc #$30
      sta $00                   ; Y position for bullet
      ; Alternate left/right side
      lda FrameCounter
      and #%00010000
      lsr
      lsr
      lsr
      lsr
      clc
      adc #1                    ; A = 1 or 2 (left or right)
      jsr BossSpawnSideBullet
  @done:
      rts

MovGoToMiddle:
      jsr BossSetWalkingAnim

      ; Target X: $B0 if facing left, $40 if facing right
      lda Enemy_MovingDir,x
      lsr
      bcs @facing_right
      ; Facing left: target $B0
      lda Enemy_X_Position,x
      cmp #$B0
      bcc @keep_walking
      bcs @arrived
  @facing_right:
      ; Facing right: target $40
      lda Enemy_X_Position,x
      cmp #$80
      bcs @arrived
  @keep_walking:
      ; Set walk speed based on direction
      lda Enemy_MovingDir,x
      lsr
      bcs @walk_right
      lda #<(-10)
      .byte $2c                 ; skip next instruction
  @walk_right:
      lda #10
      sta Enemy_X_Speed,x
      jmp MoveEnemyHorizontally

  @arrived:
      lda #0
      sta Enemy_X_Speed,x
      ; Jump onto center platform
      lda Enemy_State,x
      and #%01000000
      bne @already_airborne
      lda #<(-6)
      sta Enemy_Y_Speed,x
      jsr BossJump
  @already_airborne:
      lda #1
      sta boss_substate         ; switch to substate 1
      lda #$80
      sta boss_state_timer      ; timer for bullet phase
      rts

.reloc
BossSetWalkingAnim:
      lda FrameCounter
      and #%00000011
      cmp #%00000011
      bne @init_anim
      inc boss_walking_anim
      lda boss_walking_anim
      cmp #WALKING+3
      bne @init_anim
      lda #WALKING
      sta boss_walking_anim
  @init_anim:
      lda boss_walking_anim
      sta boss_animation
      rts

.reloc
BossJump:
      lda #Sfx_BigJump
      sta Square1SoundQueue
      lda Enemy_State,x
      ora #%01000000            ; set airborne flag
      sta Enemy_State,x
      rts

.segment "PRG7"
.reloc
BossGraphicsHandler:
  rts

.segment "PRG5", "PRG7"
.reloc
FindID:
      ; Find first empty enemy slot (5 down to 1), return in Y.
      ; N flag set if none found.
      ldy #5
  @loop:
      lda Enemy_Flag,y
      beq @foundSlot
      dey
      bne @loop                       ; stop at 0 (don't use boss slot)
      dey                             ; Y = $FF, N flag set
  @foundSlot:
      sty $04
      rts

.reloc
BossSpawnFireball:
      jsr FindID                ; find empty enemy slot -> Y
      bmi @done                 ; no slot available
      lda #Sfx_Fireball
      sta Square1SoundQueue
      lda #4
      sta boss_shootanim        ; trigger shoot animation
      lda Enemy_Y_Position,x
      clc
      adc #4
      sta $00                   ; Y pos
      lda Enemy_X_Position,x
      sta $01                   ; X pos
      lda #0
      sta $02                   ; page
      lda #8
      sta $03                   ; bounding box size
      lda Enemy_MovingDir,x
      cmp #$01
      bne @no
      pha
      lda $01
      clc
      adc #8                    ; offset X if facing right
      sta $01
      pla
  @no:
      sta Enemy_MovingDir,y
      lda #5 ; TODO What to spawn here? BowserFlame
      jsr BossSpawnEnemy
      ldx ObjectOffset
  @done:
      rts

.reloc
BossSpawnSideBullet:
      ; A = direction (1=left edge, 2=right edge)
      ; $00 = Y position (already set by caller)
      sta $02
      jsr FindID
      bmi @done
      lda $02
      pha
        lda #Sfx_Blast
        sta Square2SoundQueue

        lda $02
        lsr
        bcs @rightdir

        lda #$f0
        sta $01                   ; X = left edge ($F0)
        lda #<(-60)
        sta ProjectileXVelocity,y             ; speed = -60 (move right from left)
        jmp @rest

    @rightdir:
        lda #0
        sta $01                   ; X = right edge ($00)
        lda #60
        sta ProjectileXVelocity,y             ; speed = 60 (move left from right)

    @rest:
        lda #8
        sta $03                   ; bounding box
        lda #0
        sta $02                   ; page

        tya
        pha
          lda #10 ; TODO: what to use to make a bubble?
          jsr BossSpawnEnemy
        pla
        tay
      pla
      sta Enemy_MovingDir,y
      ldx ObjectOffset
  @done:
      rts

.reloc
BossSpawnEnemy:
      ; Spawn enemy in slot Y with parameters in $00-$03
      ; A = enemy ID
;      sta Enemy_ID,y
      lda $00
      sta Enemy_Y_Position,y
      lda $01
      sta Enemy_X_Position,y
      lda $02
      sta Enemy_PageLoc,y
;      lda $03
;      sta Enemy_BoundBoxCtrl,y
      lda #$01
      sta Enemy_Y_HighPos,y
;      sta Enemy_Flag,y
      lsr
      sta Enemy_X_MoveForce,y
      sta Enemy_State,y
      rts
;      ldx $04                   ; slot index
;      jmp CheckpointEnemyID     ; engine: initialize enemy by ID

.reloc
KillBoss:
      lda boss_counter
      bmi @death_complete       ; counter expired -> signal fully dead
;      lda #Silence
;      sta AreaMusicQueue
      dec boss_counter

      jsr EnemyToBGCollisionDet
      lda #STANDING
      sta boss_animation
      lda Enemy_State,x
      and #%01000000
      beq @nograv
      jsr MoveD_EnemyVertically
      lda #JUMPING
      sta boss_animation
  @nograv:
      ; Draw every other frame (flash effect)
      lda FrameCounter
      lsr
      bcs @nodraw
;      jsr RelativeEnemyPosition
      jsr BossGraphicsHandler
  @nodraw:
      ldx ObjectOffset
      rts

  @death_complete:
      ; Counter expired: signal death complete
      dec Enemy_HP,x       ; HP goes to $FF (negative)
;      lda #mus_bossdie
;      sta AreaMusicQueue
      rts

;.reloc
;AnimateKilledBoss:
;      jsr EraseEnemyObject
;
;      lda #3
;      sta pal_type              ; set palette type for post-boss
;      lda #2
;      sta PowerUpType
;      lda #PowerUpObject
;      sta Enemy_ID+5
;      lda #0
;      sta Enemy_PageLoc+5
;      sta Enemy_X_Speed+5
;      lda Enemy_X_Position,x
;      sta Enemy_X_Position+5    ; spawn at boss's position
;      lda #$01
;      sta Enemy_Y_HighPos+5
;      sta Enemy_Flag+5
;      lda Enemy_Y_Position,x
;      sta Enemy_Y_Position+5
;      lda #$80
;      sta Enemy_State+5
;      lda #$03
;      sta Enemy_BoundBoxCtrl+5
;
;      lda #<(-3)
;      sta Enemy_Y_Speed+5      ; powerup floats upward
;
;      lda #Sfx_GrowPowerUp
;      sta Square2SoundQueue
;      ldx ObjectOffset
;      rts

.reloc
StompedBoss:
      lda Player_Y_Position
      clc
      adc #10
      cmp Enemy_Y_Position,x
      bcc @valid_stomp
;        jmp StompInjury           ; player below boss -> normal stomp injury to PLAYER
        jmp DamagePlayer
  @valid_stomp:
      lda #$fd
      sta Player_Y_Speed        ; bounce player upward
      lda boss_state
      bne InjuredBoss           ; if not in state 0, just take damage
      lda #0
      sta boss_state_timer      ; if in idle state, also reset timer

InjuredBoss:
      lda boss_invincibility_timer
      bne @done                 ; already invincible, no damage
      inc boss_screech          ; track hit count
      dec Enemy_HP,x       ; reduce HP
      lda #80
      sta boss_invincibility_timer  ; 80 frames of invincibility
  @done:
      rts
.reloc
DamagePlayer:
  lda $a8,x
  ora #$10 ; set flag indicating link was hit by this enemy?
  sta $a8,x
  rts

; Vanilla hitbox primitive addresses (all in fixed PRG7, callable from anywhere)
bank7_LoadObjectHitbox = $E942  ; loads enemy hitbox into ZP $04-$07 using Enemy_X/Y_Position,x
bank7_LoadLinkHitbox   = $E975  ; loads Link/Mario body hitbox into ZP $00-$03
bank7_CollisionTest    = $E9F9  ; rectangle intersection; Carry set = overlap
bank7_LinkCollision    = $D6C1  ; processes $A8,x enemy-hit flags -> calls link hurt routine
bank7_Sword_Hit_Detection = $E677 ; check if the HitboxX/Y overlaps with the current enemy
.reloc
; Check contact between Mario and the boss each frame.
BossPlayerCollision:
    ldx ObjectOffset

    jsr bank7_LoadObjectHitbox  ; boss body hitbox -> ZP $04-$07
    jsr bank7_Sword_Hit_Detection ; compared with the sword stab routine
    bcs @hit_boss

    jsr bank7_LoadLinkHitbox    ; Mario body hitbox -> ZP $00-$03
    jsr bank7_CollisionTest
    bcs @contact_damage
    rts

@hit_boss:
    jsr InjuredBossJump
    lda #$fd
    sta Player_Y_Speed    ; bounce Mario up
@no_collision:
    rts

@contact_damage:
    ; Mario touched boss without downstabbing: hurt Mario via vanilla damage system
    lda $a8,x
    ora #$10              ; enemy-contacted-link flag
    sta $a8,x
    jmp bank7_LinkCollision ; processes $A8 flag to damage Mario; returns to RunBoss caller


.reloc
; Variant of InjuredBoss that subtracts 5 HP at once (jump attack damage).
; HP is floored at 0 so the KillBoss death-countdown path runs normally.
InjuredBossJump:
    lda boss_invincibility_timer
    bne @done             ; already invincible, no damage (caller still bounces Mario)
    inc boss_screech
    lda Enemy_HP,x
    sec
    sbc #5
    bcs +
    lda #0              ; floor at 0
+:
    sta Enemy_HP,x
    lda #$10 ; Sword stab SFX
    sta Z2NoiseSoundQueue ; stab SFX
    lda #30               ; 0.5 seconds at 60 fps
    sta boss_invincibility_timer
    ; Reset state timer if currently idle (mirrors InjuredBoss behavior)
    lda boss_state
    bne @done
    lda #0
    sta boss_state_timer
@done:
    rts

.segment "PRG7"
.reloc
.import ImposeGravitySprObj
MoveD_EnemyVertically:
        lda NmiBankShadow8
        pha
        lda NmiBankShadowA
        pha

        lda #0
        jsr SwapPRG

            ldy #$3d           ;set quick movement amount downwards
            lda #$03                ;set maximum speed in A
            sty R0                  ;set movement amount here
            inx                     ;increment X for enemy offset

            jsr ImposeGravitySprObj ;do a sub to move enemy object downwards
        pla
        sta NmiBankShadowA
        sta PrgBankAReg
        pla
        sta NmiBankShadow8
        sta PrgBank8Reg
             ldx ObjectOffset        ;get enemy object buffer offset and leave
             rts

;.reloc
;MoveEnemyHorizontally:
;      inx                         ;increment offset for enemy offset
;      jsr MoveObjectHorizontally  ;position object horizontally according to
;      ldx ObjectOffset            ;counters, return with saved value in A,
;      rts                         ;put enemy offset back in X and leave
