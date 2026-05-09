

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
.reloc
PatchBossInit:
  sta EnemyYVelocity
  jmp InitShadowBoss

; Patch the location where it draws the boss (effectively a nop)
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
FREE_UNTIL $9a0f


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
      ldx #0               ; Dark Link entity is always at enemy slot 0
      ; Set HP
      lda #60
      sta Enemy_HP,x
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
    ;   jsr BossGraphicsHandler
  @nodraw2:
      ldx #0
      rts

  @notfreezed:
      lda Enemy_HP,x
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

      lda TimerControl
      bne @nograv               ; skip movement if timer frozen

      jsr EnemyToBGCollisionDet ; background collision
      jsr HandleBossMovement    ; AI state machine
      jsr ClampBossX            ; keep boss inside arena [16, 236]
      jsr BossUpdateProjectiles ; move shadow fireballs/hammers + collision

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
      ; Every 24 frames, lob a shadow hammer from the boss with a random
      ; X-speed; BossSpawnHammer biases the throw toward Mario.
      lda FrameCounter
      and #%00010111
      bne @done
      jsr BossSpawnHammer
  @done:
      rts

; Right-side platform target: in vanilla Zelda 2, the great-palace dark-link
; arena has a 3-metatile-wide platform on the right, 7 metatiles above the
; ground.  Center of platform is X = $D8.  The boss walks toward $D8 from
; whichever side it's on, then jumps up to land on top.
PLATFORM_TARGET_X = $D8
PLATFORM_TOLERANCE = 4          ; arrive band: target +/- 4 pixels

MovGoToMiddle:
      jsr BossSetWalkingAnim

      ; Walk toward PLATFORM_TARGET_X.  Boss is "arrived" once within +/-4 px.
      lda Enemy_X_Position,x
      cmp #PLATFORM_TARGET_X - PLATFORM_TOLERANCE
      bcc @walk_right
      cmp #PLATFORM_TARGET_X + PLATFORM_TOLERANCE
      bcs @walk_left
      jmp @arrived

  @walk_right:
      lda #1
      sta Enemy_MovingDir,x
      sta EnemyFacingDir,x
      lda #20
      sta Enemy_X_Speed,x
      jmp MoveEnemyHorizontally

  @walk_left:
      lda #2
      sta Enemy_MovingDir,x
      sta EnemyFacingDir,x
      lda #<(-20)
      sta Enemy_X_Speed,x
      jmp MoveEnemyHorizontally

  @arrived:
      lda #0
      sta Enemy_X_Speed,x
      ; Jump onto right platform.
      ; lift under MoveD_EnemyVertically's gravity to clear ~7 metatiles.
      lda Enemy_State,x
      and #%01000000
      bne @already_airborne
      lda #<(-8)
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
; Face the player.  Sets Enemy_MovingDir to 1 (right) if the player is to the
; right of the boss, otherwise 2 (left).  Z2 enemy MovingDir conventions:
; 1 = right, 2 = left.
BossFacePlayer:
      ldx #0
      lda Player_X_Position
      cmp Enemy_X_Position,x
      lda #1                    ; default: face right
      bcs @store
        lda #2                  ; player is left of boss: face left
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
; Hooked at $9a7f, which is part of the bank5 dark-link state machine and
; runs *independently* of PatchLinkDrawRoutine, so the boss no longer
; flickers in lockstep with Mario's hurt/invincibility flicker.
;
; DrawMetasprite lives in PRG0, so we save the current bank, swap in PRG0,
; do the draw, then restore the bank that was loaded at $8000/$A000 on
; entry (typically bank5, since we were called from bank5 code).
;
; Boss sprites are written starting at OAM byte 48 - past the slots that
; PatchLinkDrawRoutine uses for the player + shield + Mario projectiles -
; so neither call clobbers the other regardless of which runs first.
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
    jsr DrawMetasprite
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

; --- Boss projectile system ---------------------------------------------------
; Boss projectiles live in SprObject indices 7 and 8 - the slots between the 6
; vanilla enemy slots and Mario's fireball/hammer pair.  DrawMetasprite reads
; SprObject_*,y directly so once a slot is populated we can render it with the
; existing METASPRITE_FIREBALL_FRAME_x / METASPRITE_HAMMER_FRAME_x metasprites.
;
; A slot is active when EnemyProjectileType,n != 0 (zero = inactive).  We never
; leave a metasprite ID stored at $87..$88 outside of our own update/draw paths
; so vanilla projectile dispatch can't mistake it for a Z2 projectile type.
;
; We co-opt one bit of SprObject_SprAttrib,n to remember the type:
;   bit 6 = 1  -> hammer (lobbed, gravity)
;   bit 6 = 0  -> fireball (straight horizontal)
; Bits 0-1 stay set to %10 = palette 3 (shadow palette) for both types.

PROJ_ATTR_FIREBALL = %00000010    ; palette 3
PROJ_ATTR_HAMMER   = %01000010    ; palette 3 + "is hammer" flag (uses OAM bit 6, harmless on render)

.reloc
; FindFreeBossProjSlot: returns Y = 7..(7+ENEMY_PROJECTILE_COUNT-1) for an empty slot,
; or Y < 7 with N flag set if none available.  Active = EnemyProjectileType,Y != 0.
FindFreeBossProjSlot:
      ldy #EnemyProjectileOffset
  @loop:
      lda EnemyProjectileType - EnemyProjectileOffset,y
      beq @found
      iny
      cpy #EnemyProjectileOffset + ENEMY_PROJECTILE_COUNT
      bcc @loop
      ldy #0
      dey                                 ; Y = $FF, N set, BCC won't take
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
      lda Enemy_X_Position,x
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
      lda #0
      sta SprObject_Y_Speed,y             ; fireballs travel level

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

      ; Random X-speed magnitude in 1..4, sign points toward Mario.
      lda RNG
      and #$03
      clc
      adc #1
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

.reloc
; BossUpdateProjectiles: per-frame movement + offscreen kill + collision check
; for each active boss projectile.  Hammers gain +1 to Y velocity each frame
; (gravity); fireballs ignore Y velocity.
BossUpdateProjectiles:
      ldy #EnemyProjectileOffset
  @loop:
      lda EnemyProjectileType - EnemyProjectileOffset,y
      bne @active
      jmp @next
  @active:
      ; X position += X speed
      clc
      lda SprObject_X_Position,y
      adc SprObject_X_Speed,y
      sta SprObject_X_Position,y

      ; Hammer arc: integrate Y speed and apply gravity
      lda SprObject_SprAttrib,y
      and #%01000000                      ; PROJ_ATTR_HAMMER bit
      beq @move_done
        clc
        lda SprObject_Y_Position,y
        adc SprObject_Y_Speed,y
        sta SprObject_Y_Position,y
        ; Apply gravity once every 2 frames so hammers don't sink too fast
        lda FrameCounter
        lsr
        bcs @move_done
            lda SprObject_Y_Speed,y
            adc #1
            sta SprObject_Y_Speed,y
  @move_done:

      ; Offscreen kill: remove if X out of [0, 248] or Y >= $D8 (below floor).
      lda SprObject_X_Position,y
      cmp #248
      bcs @kill
      lda SprObject_Y_Position,y
      cmp #$D8
      bcs @kill

      ; Collision with player.  Boss projectile box: 8x8 at (X,Y).  Reuse the
      ; vanilla player hitbox loader for $00-$03, and hand-build $04-$07.
      lda SprObject_X_Position,y
      sta $04
      lda #8
      sta $06
      lda SprObject_Y_Position,y
      sta $05
      lda #8
      sta $07
      sty $11                             ; preserve loop slot
      jsr bank7_LoadLinkHitbox            ; Mario body in $00-$03
      jsr bank7_CollisionTest
      ldy $11
      bcc @next
      ; Hit Mario - flag him for damage and drop projectile
      ldx #0
      lda $a8,x
      ora #$10
      sta $a8,x
      jsr bank7_LinkCollision

  @kill:
      lda #0
      sta EnemyProjectileType - EnemyProjectileOffset,y
  @next:
      iny
      cpy #EnemyProjectileOffset + ENEMY_PROJECTILE_COUNT
      bcc @loop
      ldx #0
      rts

.segment "PRG7"
.reloc
; BossDrawProjectiles: invoked from BossGraphicsHandler with PRG0 already paged
; in, so DrawMetasprite is reachable.  This routine MUST live in PRG7 (fixed
; bank) so it stays callable after BossGraphicsHandler's SwapPRG(0) replaces
; PRG5 at $8000.  Animation frame toggles every 4-8 frames.
BossDrawProjectiles:
      ldy #EnemyProjectileOffset
  @loop:
      lda EnemyProjectileType - EnemyProjectileOffset,y
      beq @next
        ; Reload animation frame (frame_1 / frame_2) based on FrameCounter.
        lda SprObject_SprAttrib,y
        and #%01000000
        bne @hammer_anim
          lda FrameCounter
          and #%00000100
          beq @set_fb1
          lda #METASPRITE_FIREBALL_FRAME_2
          .byte $2c
      @set_fb1:
          lda #METASPRITE_FIREBALL_FRAME_1
          jmp @do_draw
      @hammer_anim:
          lda FrameCounter
          and #%00001000
          beq @set_h1
          lda #METASPRITE_HAMMER_FRAME_2
          .byte $2c
      @set_h1:
          lda #METASPRITE_HAMMER_FRAME_1
      @do_draw:
        sta EnemyProjectileType - EnemyProjectileOffset,y
        tax
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
    @animating:
      jsr MoveD_EnemyVertically  ; gravity decelerates upward motion then pulls boss down
      dec boss_counter
      bne @done                  ; still in animation window
        dec Enemy_HP,x
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
    lda Enemy_HP,x
    sec
    sbc #5
    bcs +
    lda #0              ; floor at 0
+
    sta Enemy_HP,x
    beq +
        ; don't play sfx for last hit
        lda #$10 ; Sword stab SFX
        sta Z2NoiseSoundQueue ; stab SFX
    +
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
             ldx #0                  ;boss is always at enemy slot 0
             rts

;.reloc
;MoveEnemyHorizontally:
;      inx                         ;increment offset for enemy offset
;      jsr MoveObjectHorizontally  ;position object horizontally according to
;      ldx ObjectOffset            ;counters, return with saved value in A,
;      rts                         ;put enemy offset back in X and leave
