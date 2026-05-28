

.include "z2r.inc"

.import METASPRITE_BIG_MARIO_SKIDDING, METASPRITE_BIG_MARIO_JUMPING, METASPRITE_FIRE_MARIO_STANDING, METASPRITE_BIG_MARIO_STANDING
.import DrawMetasprite, METASPRITE_BIG_MARIO_WALKING_1
.import SwapPRG, SwapToSavedPRG, SwapToPRG0
.import METASPRITE_SMALL_MARIO_DEATH
.import METASPRITE_FIREBALL_FRAME_1, METASPRITE_FIREBALL_FRAME_2
.import METASPRITE_HAMMER_FRAME_1, METASPRITE_HAMMER_FRAME_2

.export InjuredBoss

.import METASPRITE_BIG_MARIO_CROUCHING
CROUCHING = METASPRITE_BIG_MARIO_CROUCHING
SKIDDING = METASPRITE_BIG_MARIO_SKIDDING
JUMPING  = METASPRITE_BIG_MARIO_JUMPING
SHOOT    = METASPRITE_FIRE_MARIO_STANDING
STANDING = METASPRITE_BIG_MARIO_STANDING
WALKING = METASPRITE_BIG_MARIO_WALKING_1
mus_bossdie = 8


; Boss settings:

; Frames the boss spends standing still after each top-level state change.
STATE_SWITCH_DELAY = 50

; Right-side platform target: the great-palace dark-link arena has a 3-metatile-wide
; platform on the right, 7 metatiles above the ground. Boss will jump up here for a hammer attack
PLATFORM_TARGET_X       = $D8
PLATFORM_TOLERANCE      = 4
PLATFORM_JUMP_DISTANCE  = $20   ; trigger arcing leap when this close to target X
JUMP_X_SPEED            = 6     ; horizontal speed retained during the jump
JUMP_Y_SPEED            = <(-8) ; initial upward velocity (matches old straight-up jump height)

; Y position above which the boss is considered "on the platform" (the visual
; floor of the arena sits at $A0+, the platform sits 7 metatiles higher).
PLATFORM_TOP_Y_THRESHOLD = $80

LEFT_EDGE_TARGET = 3*16
RIGHT_EDGE_TARGET = 10*16

; Boss running model: accelerate to BOSS_MAX_RUN_SPEED while far from the
; target wall; when within BOSS_DECEL_DISTANCE pixels of the target,
; decelerate to zero and flip via MovChangeState.
BOSS_MAX_RUN_SPEED  = $50
BOSS_DECEL_DISTANCE = 3 * 16 ; in metatiles


.segment "PRG7"
; patch the vanilla enemy projectile check to prevent it from trying to run our hammer/fireball
.org $d57f
  jsr DontRunHammerFireball
.reloc
DontRunHammerFireball:
  cmp #METASPRITE_FIREBALL_FRAME_1
  bcs @HammerFireball
    sec
    sbc #1
    rts
@HammerFireball:
  pla
  pla
  rts

.segment "PRG5", "PRG7"

; Patch the spot that "kicks" mario shadow out and use it
; as an init routine
.org $985d
  jsr PatchBossInit
;   lda #$21 ; make the boss jump further out to make him land past $b0
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
  lda #$9F
  sta $2a ; force the boss to land at the right height on the ground
  lda #2
  sta Enemy_MovingDir ; force the boss to start by facing left
  rts

.segment "PRG5"
.org $988e
  jsr RunBoss
  jmp $9a77
FREE_UNTIL $9a0f

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
      ldx #0               ; Dark Link entity is always at enemy slot 0
      ; Set HP
      lda #60
      sta EnemyHP,x
      lda #JUMPING
      sta boss_animation
      lda #CHR_BIGMARIO
      sta boss_ChrBank
      inc ReloadCHRBank

      ; Face left
      ldx #0
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
        ldx #0                  ;boss is always enemy slot 0
        inx                     ;add EnemyOffset=1: SprObject index for slot 0
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
    ldx #0                    ;boss is always at enemy slot 0
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

    ; Display boss HP bar with a divisor of 7 (close enough to 60 HP)
    lda #7
    jsr $A4E9

    lda boss_freeze
    beq @notfreezed
    dec boss_freeze
    lda #STANDING
    sta boss_animation
    ; Draw every other frame (flicker)
;     lda FrameCounter
;     lsr
;     bcs @nodraw2
; ;      jsr RelativeEnemyPosition
;     ;   jsr BossGraphicsHandler
;   @nodraw2:
      ldx #0
      rts

  @notfreezed:
      lda EnemyHP,x
      bmi AnimateKilledBoss     ; HP < 0: boss is fully dead
      bne @alive
      jmp KillBoss              ; HP == 0: playing death countdown
  @alive:

    ;   lda #80
    ;   sta boss_counter          ; reset death countdown each frame while alive
;      lda #0
;      sta EnemyFrenzyBuffer     ; suppress enemy frenzy spawns

      ; Draw (skip every other frame if invincible for blink effect)
;       lda boss_invincibility_timer
;       beq @draw
;       lda FrameCounter
;       lsr
;       bcs @nodraw
;   @draw:
;      jsr RelativeEnemyPosition
    ;   jsr BossGraphicsHandler
  @nodraw:
      ldx #0

      jsr BossUpdateProjectiles

      lda TimerControl
      bne @nograv               ; skip movement if timer frozen

      jsr EnemyToBGCollisionDet ; background collision
      jsr HandleBossMovement    ; AI state machine
      jsr ClampBossX            ; keep boss inside arena [16, 236]

      lda Enemy_State,x
      and #%01000000            ; check airborne flag
      beq @nograv
      jsr MoveD_EnemyVertically ; apply gravity
      lda #JUMPING
      sta boss_animation
  @nograv:

      ; Collision detection (only when not invincible).  While invincible:
      ;   1. Skip BossPlayerCollision entirely so the boss body cannot register
      ;      either a stomp re-hit or contact damage on Mario.
      ;   2. Force-clear bit 4 of $A8,x (the enemy-contacted-link flag) so any
      ;      contact that registered right before the boss took damage can't
      ;      fire bank7_LinkCollision later in the frame.
      lda boss_invincibility_timer
      bne @decrement_timer
        jmp BossPlayerCollision

  @decrement_timer:
      lda $a8,x
      and #%11101111
      sta $a8,x
      dec boss_invincibility_timer
      rts
AnimateKilledBoss:
  inc $63
  rts

.reloc
HandleBossMovement:
      ; If the boss is invincible (just got stomped), force-clear any pending
      ; transition delay.
      lda boss_invincibility_timer
      beq +
        lda #0
        sta boss_transition_delay
+

      lda boss_transition_delay
      beq @run_state
        lda Enemy_State,x
        and #%01000000
        bne @airborne_delay
          ; On the ground: standing pause.
          dec boss_transition_delay
          lda #STANDING
          sta boss_animation
          lda #0
          sta Enemy_X_Speed,x
          rts
    @airborne_delay:
        lda #JUMPING
        sta boss_animation
        jmp MoveEnemyHorizontally
@run_state:
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
      jsr BossFacePlayer        ; aim at Mario before shooting
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
      lda #STATE_SWITCH_DELAY
      sta boss_transition_delay

  @keep_state:
      dec boss_state_timer
      rts

.reloc

MovBossRunning:
      jsr BossSetWalkingAnim

      ; Airborne: skip the accel/decel state machine.  Just integrate the
      ; jump-off velocity so the parabola off the platform stays clean.
      lda Enemy_State,x
      and #%01000000
      beq @grounded
        jmp MoveEnemyHorizontally
@grounded:

      ; One-shot bridge jump at the start of a run (set by MovBossSide via
      ; inc boss_bridge when transitioning from idle).  Fires once on the
      ; first grounded frame, sets the airborne flag, then defers further
      ; AI work until next frame.
      lda boss_bridge
      beq @no_bridge
        dec boss_bridge
        lda #<(-4)
        sta Enemy_Y_Speed,x
        jsr BossJump
        jmp @move                    ; airborne now; let the arc start
@no_bridge:

      ; Compute unsigned distance from the wall the boss is heading toward.
      ;   Facing right -> target = RIGHT_EDGE_TARGET, distance = target - X
      ;   Facing left  -> target = LEFT_EDGE_TARGET,  distance = X - target
      ; If the position has already crossed the target (subtract underflows),
      ; clamp distance to 0 so the decel branch runs and we flip immediately.
      lda Enemy_MovingDir,x
      cmp #1
      beq @facing_right

      lda Enemy_X_Position,x
      sec
      sbc #LEFT_EDGE_TARGET
      bcs +
        lda #0
      +
      cmp #BOSS_DECEL_DISTANCE
      bcs @accel_left
      jmp @decel_left

@facing_right:
      lda #RIGHT_EDGE_TARGET
      sec
      sbc Enemy_X_Position,x
      bcs +
        lda #0
      +
      cmp #BOSS_DECEL_DISTANCE
      bcs @accel_right
      jmp @decel_right

@accel_left:
      ; Push X_Speed toward -BOSS_MAX_RUN_SPEED at 2/frame.
      lda Enemy_X_Speed,x
      bpl @do_dec_l                  ; positive or zero: dec unconditionally
      cmp #<(-BOSS_MAX_RUN_SPEED) + 1
      bcc @move                      ; already at/past target (more negative)
@do_dec_l:
      dec Enemy_X_Speed,x
      dec Enemy_X_Speed,x
      jmp @move

@accel_right:
      ; Push X_Speed toward +BOSS_MAX_RUN_SPEED at 2/frame.
      lda Enemy_X_Speed,x
      bmi @do_inc_r                  ; negative: inc unconditionally
      cmp #BOSS_MAX_RUN_SPEED
      bcs @move                      ; already at/past target
@do_inc_r:
      inc Enemy_X_Speed,x
      inc Enemy_X_Speed,x
      jmp @move

@decel_left:
      lda #SKIDDING
      sta boss_animation
      lda Enemy_X_Speed,x
      beq @flip                      ; already stopped
      bpl @flip                      ; wrong-sign (positive while facing left)
      inc Enemy_X_Speed,x
      inc Enemy_X_Speed,x
      lda Enemy_X_Speed,x
      bmi @move                      ; still moving left: continue
      jmp @flip                      ; crossed zero / reached zero: flip

@decel_right:
      lda #SKIDDING
      sta boss_animation
      lda Enemy_X_Speed,x
      beq @flip
      bmi @flip
      dec Enemy_X_Speed,x
      dec Enemy_X_Speed,x
      lda Enemy_X_Speed,x
      beq @flip
      bmi @flip
      jmp @move                      ; still moving right: continue

@flip:
      jmp MovChangeState

@move:
      jmp MoveEnemyHorizontally

MovChangeState:
      ; Running -> idle.
      lda #$b0
      sta boss_state_timer
      lda #STANDING
      sta boss_animation
      lda #0
      sta boss_state
      lda Enemy_MovingDir,x
      eor #%00000011
      sta Enemy_MovingDir,x
      rts

.reloc
MovBossMiddle:
      lda boss_substate
      beq MovGoToMiddle

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

  @no_need_for_p:
      ; Face boss left before jumping off platform
      lda #0
      sta boss_bridge

      lda #2
      sta Enemy_MovingDir,x
      sta EnemyFacingDir,x

      ; Small hop off the platform
      lda #<(-(20))
      sta Enemy_X_Speed,x

      lda #<(-3)
      sta Enemy_Y_Speed,x
      jsr BossJump

      lda #1
      sta boss_state
  @notyet:
      rts

  @not_ready:
      dec boss_state_timer

      lda Enemy_State,x
      and #%01000000
      beq @on_platform
        jsr MoveEnemyHorizontally
        ldx #0
  @on_platform:

      ; Every 24 frames, lob a hammer from the boss with a random speed
      lda FrameCounter
      and #%00010111
      bne @done
        jmp BossSpawnHammer
  @done:
      rts

MovGoToMiddle:
      jsr BossSetWalkingAnim

      ; If we're already airborne, the leap is in progress: keep the arc and
      ; let gravity + EnemyToBGCollisionDet handle landing on the platform.
      lda Enemy_State,x
      and #%01000000
      bne @keep_arc

      ; don't start throwing hammers until we are actually landed on the platform
      lda Enemy_Y_Position,x
      cmp #PLATFORM_TOP_Y_THRESHOLD
      bcs @on_lower_floor
        ; Standing on the platform - kick off the hammer phase.
        lda #0
        sta Enemy_X_Speed,x
        lda #1
        sta boss_substate
        lda #$80
        sta boss_state_timer
        rts

@on_lower_floor:
      ; On ground: figure out which side of the target we're on and how far.
      lda Enemy_X_Position,x
      cmp #PLATFORM_TARGET_X
      bcs @right_of_target

      ; Boss is left of target. Distance = TARGET - X.
      lda #PLATFORM_TARGET_X
      sec
      sbc Enemy_X_Position,x
      sta $00
      lda #1
      sta Enemy_MovingDir,x
      sta EnemyFacingDir,x
      lda $00
      cmp #PLATFORM_JUMP_DISTANCE
      bcs @walk_right_full
      lda #JUMP_X_SPEED
      sta Enemy_X_Speed,x
      jmp @start_jump

@right_of_target:
      lda Enemy_X_Position,x
      sec
      sbc #PLATFORM_TARGET_X
      sta $00
      lda #2
      sta Enemy_MovingDir,x
      sta EnemyFacingDir,x
      lda $00
      cmp #PLATFORM_TOLERANCE
      bcc @on_target_jump_up   ; already centered horizontally - jump straight up
      cmp #PLATFORM_JUMP_DISTANCE
      bcs @walk_left_full
      lda #<(-JUMP_X_SPEED)
      sta Enemy_X_Speed,x
      jmp @start_jump

@on_target_jump_up:
      lda #0
      sta Enemy_X_Speed,x
      jmp @start_jump

@walk_right_full:
      lda #20
      sta Enemy_X_Speed,x
      jmp MoveEnemyHorizontally

@walk_left_full:
      lda #<(-20)
      sta Enemy_X_Speed,x
      jmp MoveEnemyHorizontally

@start_jump:
      lda #JUMP_Y_SPEED
      sta Enemy_Y_Speed,x
      jsr BossJump              ; sets airborne flag
      ; Stay in substate 0 throughout the arc.  The landing-detection at the
      ; top of MovGoToMiddle flips us to substate 1 once we touch down on
      ; the platform; until then no hammers fire.

@keep_arc:
      jmp MoveEnemyHorizontally

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
; Face the player.  Sets Enemy_MovingDir to 1 (right) if the player is to the
; right of the boss, otherwise 2 (left).  Z2 enemy MovingDir conventions:
; 1 = right, 2 = left.
BossFacePlayer:
      ldx #0
      lda Player_X_Position
      cmp Enemy_X_Position,x
      lda #1 ; face right
      bcs @store
        lda #2 ; player is left of boss
    @store:
      sta Enemy_MovingDir,x
      sta EnemyFacingDir,x
      rts

.reloc
; Keep the boss inside the arena bounds [16, 236].  If Enemy_X_Speed accelerates
; far enough to drive Enemy_X_Position past either wall (including unsigned
; underflow when running left) we hard-clamp the position and zero the speed so
; MovBossRunning's deceleration logic can take over normally.
ClampBossX:
      ldx #0
      lda Enemy_X_Position,x
      cmp #16
      bcs @notLeft
        lda #16
        sta Enemy_X_Position,x
        lda #0
        sta Enemy_X_Speed,x
        beq @done
    @notLeft:
      cmp #237
      bcc @done
        lda #236
        sta Enemy_X_Position,x
        lda #0
        sta Enemy_X_Speed,x
    @done:
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
; Draw the shadow-mario boss (and, eventually, its projectiles) into OAM.
; Hooked at $9a7f, which is part of the bank5 dark-link state machine
;
; DrawMetasprite lives in PRG0, so we save the current bank, swap in PRG0,
; do the draw, then restore the bank to bank5 after
BOSS_OAM_OFFSET = 48

BossGraphicsHandler:
  lda boss_animation
  bne @active
    rts
@active:

  lda NmiBankShadow8
  pha
  lda NmiBankShadowA
  pha

  lda #0
  jsr SwapPRG 
    ; Boss-owned flicker: when invincible, draw on alternating frames only.
    ; This is decoupled from any player/injury flicker.
    lda boss_invincibility_timer
    beq @draw
    lda FrameCounter
    lsr
    bcs @restore
@draw:
    lda #BOSS_OAM_OFFSET
    sta CurrentOAMOffset
    ldy #1                    ; sprite slot 1 (Y for DrawMetasprite & SprAttrib,y)
    lda #2                    ; palette 3 attribute (shadow mario palette)
    sta SprObject_SprAttrib,y
    ldx boss_animation

    cpx #SKIDDING
    bne @nm_no_skid_flip
      jsr DrawMetasprite
      jmp @nm_drew
@nm_no_skid_flip:
      jsr DrawMetasprite
@nm_drew:
    jsr BossDrawProjectiles   ; shadow fireballs + hammers (palette already set per-slot)

@restore:
  pla
  sta NmiBankShadowA
  sta PrgBankAReg
  pla
  sta NmiBankShadow8
  sta PrgBank8Reg
  rts

.segment "PRG5", "PRG7"

PROJ_TYPE_HAMMER   = %00010000
PROJ_ATTR_FIREBALL = %00000001
PROJ_ATTR_HAMMER   = PROJ_ATTR_FIREBALL | PROJ_TYPE_HAMMER

.reloc
FindFreeBossProjSlot:
      ldy #EnemyProjectileOffset
  @loop:
      lda EnemyProjectileType - EnemyProjectileOffset,y
      beq @found
      iny
      cpy #EnemyProjectileOffset + ENEMY_PROJECTILE_COUNT
      bcc @loop
      ldy #$ff
  @found:
      rts

.reloc
; BossSpawnFireball: spawn a horizontal shadow fireball from the boss aimed at
; Mario.  X must hold the boss enemy slot (= 0).
BossSpawnFireball:
      jsr FindFreeBossProjSlot
      bmi @done
      lda #Sfx_Fireball
      sta Square1SoundQueue
      lda #4
      sta boss_shootanim                  ; trigger shoot animation pose

      ; Position: emit from boss center
      lda Enemy_Y_Position,x
      clc
      adc #4
      sta SprObject_Y_Position,y
      lda Enemy_MovingDir,x
      lsr
      lda Enemy_X_Position,x
      bcc +
        adc #15
      +
      sta SprObject_X_Position,y
      lda Enemy_PageLoc,x
      sta SprObject_PageLoc,y
      lda #1
      sta SprObject_Y_HighPos,y

      ; Direction follows boss facing (already aimed by BossFacePlayer)
      lda Enemy_MovingDir,x
      sta Player_MovingDir,y              ; DrawMetasprite uses MovingDir for non-mario sprites
      cmp #1
      beq @right
        lda #<(-3)                        ; left
        .byte $2c
    @right:
        lda #3                            ; right
      sta SprObject_X_Speed,y
      ; Small initial downward Y speed; gravity in BossUpdateProjectiles adds
      ; from there and BG bounce reverses Y_Speed when the floor is reached.
      lda #1
      sta SprObject_Y_Speed,y

      lda #PROJ_ATTR_FIREBALL
      sta SprObject_SprAttrib,y
      lda #METASPRITE_FIREBALL_FRAME_1
      sta EnemyProjectileType - EnemyProjectileOffset,y

      ldx #0
  @done:
      rts

.reloc
; BossSpawnHammer: lob a hammer from the boss with a randomised X-speed so
; successive hammers leave gaps along the floor where Mario can dodge.  Sign
; of the X-speed is biased toward Mario.  Initial Y-speed is upward; gravity
; in BossUpdateProjectiles brings it back down in an arc.
BossSpawnHammer:
      jsr FindFreeBossProjSlot
      bmi @done
      lda #Sfx_Fireball
      sta Square1SoundQueue

      ; Spawn just above the boss
      lda Enemy_Y_Position,x
      sec
      sbc #8
      sta SprObject_Y_Position,y
      lda Enemy_X_Position,x
      sta SprObject_X_Position,y
      lda Enemy_PageLoc,x
      sta SprObject_PageLoc,y
      lda #1
      sta SprObject_Y_HighPos,y

      ; Random X-speed magnitude in 0..5, sign points toward Mario.
      lda RNG
      and #7
      cmp #6
      bcc +
        and #3 ; if its 6 or 7 then just make it 2 or 3 instead
      +
      pha
        lda Player_X_Position
        cmp Enemy_X_Position,x
        bcs @aim_right
      pla
      eor #$ff
      clc
      ; negate (toward left)
      adc #1
      jmp @set_speed
  @aim_right:
      pla
  @set_speed:
      sta SprObject_X_Speed,y

      ; initial upward velocity
      lda #<(-6)
      sta SprObject_Y_Speed,y

      lda Enemy_MovingDir,x
      sta Player_MovingDir,y
      lda #PROJ_ATTR_HAMMER
      sta SprObject_SprAttrib,y
      lda #METASPRITE_HAMMER_FRAME_1
      sta EnemyProjectileType - EnemyProjectileOffset,y

      ldx #0
  @done:
      rts

; Y position at which fireballs bounce.  This is the visual floor of the
; dark-mario arena (one metatile below the boss's typical foot Y).
FIREBALL_GROUND_Y       = $B8
FIREBALL_BOUNCE_Y_SPEED = <(-4)
PROJ_MAX_FALL_SPEED     = 4

.reloc
BossUpdateProjectiles:
      ldy #EnemyProjectileOffset
  @loop:
      sty $11
      lda EnemyProjectileType - EnemyProjectileOffset,y
      bne @active
      jmp @next
  @active:
      ; X position += X speed
      clc
      lda SprObject_X_Position,y
      adc SprObject_X_Speed,y
      sta SprObject_X_Position,y

      ; Y position += Y speed (both projectile types)
      clc
      lda SprObject_Y_Position,y
      adc SprObject_Y_Speed,y
      sta SprObject_Y_Position,y

      ; Gravity: every other frame, add 1 to Y_Speed.
      lda FrameCounter
      lsr
      bcs @after_gravity
        lda SprObject_SprAttrib,y
        and #PROJ_TYPE_HAMMER
        bne @apply_gravity ; hammer: uncapped
        lda SprObject_Y_Speed,y
        bmi @apply_gravity ; rising fireball: always pull toward 0
        cmp #PROJ_MAX_FALL_SPEED
        bcs @after_gravity ; fireball at terminal: skip
    @apply_gravity:
        clc
        lda SprObject_Y_Speed,y
        adc #1
        sta SprObject_Y_Speed,y
  @after_gravity:

      ; Fireball-only: bounce when reaching the arena floor.  Hammers (bit 4
      ; PROJ_TYPE_HAMMER set) skip this and keep falling until offscreen.
      lda SprObject_SprAttrib,y
      and #PROJ_TYPE_HAMMER
      bne @no_bounce
        lda SprObject_Y_Position,y
        cmp #FIREBALL_GROUND_Y
        bcc @no_bounce
        lda #FIREBALL_GROUND_Y
        sta SprObject_Y_Position,y
        lda #FIREBALL_BOUNCE_Y_SPEED
        sta SprObject_Y_Speed,y
  @no_bounce:

      ; Offscreen kill: remove if X out of [0, 248] or Y >= $D8 (below floor).
      lda SprObject_X_Position,y
      cmp #248
      bcs @kill
      lda SprObject_Y_Position,y
      cmp #$D8
      bcs @kill

      ; Skip collision detection if mario can't be hurt right now
      lda InjuryTimer
      bne @next
      ; Collision with player.  Boss projectile box: 8x8 at (X,Y).  Reuse the
      ; vanilla player hitbox loader for $00-$03, and hand-build $04-$07.
      lda SprObject_X_Position,y
      sta $04
      lda SprObject_Y_Position,y
      sta $05
      lda #8
      sta $06
      sta $07
      jsr bank7_LoadLinkHitbox
      jsr bank7_CollisionTest
      ldy $11
      bcc @next
      ldx #0
      lda $a8,x
      ora #$10
      sta $a8,x
      jsr bank7_LinkCollision

  @kill:
      lda #0
      sta EnemyProjectileType - EnemyProjectileOffset,y
  @next:
      ldy $11
      iny
      cpy #EnemyProjectileOffset + ENEMY_PROJECTILE_COUNT
      bcs @skip
        jmp @loop
  @skip:
      ldx #0
      rts

.segment "PRG7"
.reloc
BossDrawProjectiles:
      ldy #EnemyProjectileOffset
  @loop:
      lda EnemyProjectileType - EnemyProjectileOffset,y
      beq @next
        lda FrameCounter
        and #%00000100
        bne @use_frame_2
          lda SprObject_SprAttrib,y
          and #PROJ_TYPE_HAMMER
          beq @set_fb1
            lda #METASPRITE_HAMMER_FRAME_1
            .byte $2c
        @set_fb1:
            lda #METASPRITE_FIREBALL_FRAME_1
          jmp @set_frame
      @use_frame_2:
          lda SprObject_SprAttrib,y
          and #PROJ_TYPE_HAMMER
          beq @set_fb2
            lda #METASPRITE_HAMMER_FRAME_2
            .byte $2c
        @set_fb2:
            lda #METASPRITE_FIREBALL_FRAME_2
      @set_frame:
        sta EnemyProjectileType - EnemyProjectileOffset,y

        lda SprObject_SprAttrib,y
        and #%00111111
        sta SprObject_SprAttrib,y
        lda FrameCounter
        and #%00001000
        beq @no_flip
          lda SprObject_SprAttrib,y
          ora #%11000000 ; H + V flip
          sta SprObject_SprAttrib,y
      @no_flip:

        ; Keep the current object we are processing around in $11 for the next loop
        ldx EnemyProjectileType - EnemyProjectileOffset,y
        sty $11
        jsr DrawMetasprite
        ldy $11
  @next:
      iny
      cpy #EnemyProjectileOffset + ENEMY_PROJECTILE_COUNT
      bcc @loop
      rts

.segment "PRG5", "PRG7"
.reloc
KillBoss:
      lda boss_counter
      bne @animating
        ; Initialize death animation: shrink to small mario death pose and launch upward
        lda #METASPRITE_SMALL_MARIO_DEATH
        sta boss_animation
        lda #CHR_SMALLMARIO
        sta boss_ChrBank
        inc ReloadCHRBank
        lda #<(-6)
        sta Enemy_Y_Speed,x
        lda #0
        sta Enemy_X_Speed,x
        sta EnemyProjectileType+0
        sta EnemyProjectileType+1
        sta EnemyProjectileType+2
        sta EnemyProjectileType+3
        sta EnemyProjectileType+4
        sta EnemyProjectileType+5
        lda #$7f
        sta boss_counter
        ; vanilla boss kill behavior
        lda #$C0
        sta $074B ; set flash timer
        lda #$04 ; set sfx
        sta $EC
        lda #$14
        sta $0751
        lda #$FF
        sta $504 ; stop link from moving
        ; Clear out links magic to fix weird issues
        lda #0
        sta $076f
        sta $d0
        inc ReloadCHRBank
    @animating:
      jsr MoveD_EnemyVertically  ; gravity decelerates upward motion then pulls boss down
      dec boss_counter
      bne @done                  ; still in animation window
        dec EnemyHP,x
  @done:
      ldx #0
      rts

.reloc
StompedBoss:
      lda Player_Y_Position
      clc
      adc #10
      cmp Enemy_Y_Position,x
      bcc @valid_stomp
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
      dec EnemyHP,x       ; reduce HP
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
bank7_LoadObjectHitbox    = $E942  ; loads enemy hitbox into ZP $04-$07 using Enemy_X/Y_Position,x
bank7_LoadLinkHitbox      = $E975  ; loads Link/Mario body hitbox into ZP $00-$03
bank7_CollisionTest       = $E9F9  ; rectangle intersection; Carry set = overlap
bank7_LinkCollision       = $D6C1  ; processes $A8,x enemy-hit flags -> calls link hurt routine
.reloc
; Check contact between Mario and the boss each frame.
BossPlayerCollision:
    ldx #0

    jsr bank7_LoadObjectHitbox    ; boss body hitbox into ZP $04-$07

    ; Stomp detection: Mario must be falling (Player_Y_Speed > 0 in SMB1 convention).
    ; $00 = on the ground, $80-$FF = ascending, $01-$7F = falling.
    lda Player_Y_Speed
    beq @try_body_contact         ; standing – not a stomp
    bmi @try_body_contact         ; going up – not a stomp
    ; Mario is falling: check body overlap for stomp
    jsr bank7_LoadLinkHitbox      ; Mario body hitbox into ZP $00-$03
    jsr bank7_CollisionTest       ; carry set = overlap
    bcc @no_contact
    jsr InjuredBossJump
    lda #$fd
    sta Player_Y_Speed            ; bounce Mario up
    rts

@try_body_contact:
    jsr bank7_LoadLinkHitbox      ; Mario body hitbox into ZP $00-$03
    jsr bank7_CollisionTest
    bcs @contact_damage
@no_contact:
    rts

@contact_damage:
    ; Mario touched boss without stomping: hurt Mario via vanilla damage system
    lda $a8,x
    ora #$10                      ; enemy-contacted-link flag
    sta $a8,x
    jmp bank7_LinkCollision       ; processes $A8 flag to damage Mario; returns to RunBoss caller

.reloc
; Variant of InjuredBoss that subtracts 5 HP at once (jump attack damage).
; HP is floored at 0 so the KillBoss death-countdown path runs normally.
InjuredBossJump:
    lda boss_invincibility_timer
    bne @done             ; already invincible, no damage (caller still bounces Mario)
    inc boss_screech
    lda EnemyHP,x
    sec
    sbc #5
    bcs +
    lda #0              ; floor at 0
+
    sta EnemyHP,x
    beq +
        ; don't play sfx for last hit. works around a weird bug i didn't bother to fix
        lda #$10 ; Sword stab SFX
        sta Z2NoiseSoundQueue ; stab SFX
    +
    lda #60               ; 1 second of invincibility at 60 fps
    sta boss_invincibility_timer
    ; A successful stomp should also short-circuit any standing pause the boss
    ; was in - if the player can land a hit, the boss should react / counter
    ; immediately rather than continuing to wind down from the previous state.
    lda #0
    sta boss_transition_delay
    ; Reset state timer if currently idle (mirrors InjuredBoss behavior)
    lda boss_state
    bne @done
    sta boss_state_timer        ; A is already 0 here
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
  ldx #0                  ;boss is always at enemy slot 0
  rts


; Final cutscene fixes

.segment "PRG5"
.org $9b53
  jsr SetupMarioSpriteForCredits 
  nop
.reloc
SetupMarioSpriteForCredits:
  lda #1
  sta $1a ; put the boss back "onscreen" for the rest of the cutscenes
  lda #METASPRITE_BIG_MARIO_STANDING
  sta $80  
  rts

.org $9ba8
  lda #1 ; Make mario face right when taking the triforce

.org $9bb1
  lda #METASPRITE_BIG_MARIO_JUMPING

; Zelda waking scene mario sprite tile fixes
.org $8c0e
  .byte $14 ; Mario jumping top left
.org $8c12
  .byte $16 ; Mario jumping top right
  .byte $00 ; don't flip it
.org $8c16
  .byte $34 ; Mario jumping bot left
.org $8c1a
  .byte $36 ; Mario jumping bot right
  .byte $00 ; dont flip it

; Zelda kiss scene mario sprite tile fixes
.org $8d8e
  .byte $04 ; top back of mario walking
  .byte $20 ; not flipped
.org $8d92
  .byte $06 ; top front of mario walking
.org $8d9e
  .byte $24 ; bot back of mario walking
  .byte $20 ; not flipped
.org $8da2
  .byte $26 ; bot front of mario walking
