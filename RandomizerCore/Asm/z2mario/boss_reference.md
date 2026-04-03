# Shadow Mario Boss - Porting Reference

A "shadow Mario" boss that replaces the original Bowser fight. The boss walks, runs, skids, jumps, shoots fireballs, and spawns bullet bills from screen edges. It has an HP system with invincibility frames on hit, a multi-state AI, and a death sequence that spawns a powerup.

The boss uses the SMB1 enemy object system (slot 0, indexed by `ObjectOffset`/X register) and is drawn as a 4-row, 8-tile (16x32 pixel) sprite using the player's CHR tiles.

---

## 1. Custom RAM Variables

These must be allocated in your target project. Originally located at `$7e12`-`$7e1c` + `$7e75`.

| Variable | Original Addr | Description |
|---|---|---|
| `boss_animation` | `$7e12` | Current animation frame index (see constants below) |
| `boss_invincibility_timer` | `$7e13` | Countdown after being hit; boss blinks and is immune |
| `boss_screech` | `$7e14` | Incremented on each hit (tracks total damage taken) |
| `boss_state` | `$7e15` | AI state: 0=side/idle, 1=running, 2=middle |
| `boss_state_timer` | `$7e16` | Countdown for current state duration |
| `boss_walking_anim` | `$7e17` | Walking animation sub-counter (cycles 0-1-2) |
| `boss_bridge` | `$7e18` | Flag: jump while running (set when transitioning) |
| `boss_shootanim` | `$7e1a` | Shoot animation timer (decrements each draw frame) |
| `boss_substate` | `$7e1b` | Sub-state for middle action (0=moving to center, 1=acting) |
| `boss_counter` | `$7e1c` | Death countdown timer |
| `boss_freeze` | `$7e75` | Freeze timer (boss pauses, e.g. after certain events) |
| `bbSpeed` | `$7e00` | Bullet bill horizontal speed (per enemy slot, array) |

Total custom RAM: ~12 bytes contiguous + 2 separate bytes.

---

## 2. SMB1 Engine RAM Variables Used

These are standard SMB1 object system variables. Your target project must provide equivalents.

### Enemy Object Arrays (indexed by X or slot number)

| Variable | Addr | Description |
|---|---|---|
| `Enemy_ID` | `$16` | Enemy type identifier |
| `Enemy_State` | `$1e` | State flags (bit 6 = falling/airborne) |
| `Enemy_Flag` | `$0f` | Nonzero = slot active |
| `Enemy_MovingDir` | `$46` | 1=right, 2=left |
| `Enemy_X_Speed` | `$58` | Horizontal speed (signed) |
| `Enemy_X_Position` | `$87` | X position (low byte) |
| `Enemy_PageLoc` | `$6e` | X position (page/high byte) |
| `Enemy_Y_Speed` | `$a0` | Vertical speed (signed) |
| `Enemy_Y_Position` | `$cf` | Y position (low byte) |
| `Enemy_Y_HighPos` | `$b6` | Y position (high byte) |
| `Enemy_X_MoveForce` | `$0401` | Horizontal sub-pixel force |
| `Enemy_Y_MoveForce` | `$0434` | Vertical sub-pixel force |
| `Enemy_BoundBoxCtrl` | `$049a` | Bounding box size selector |
| `Enemy_CollisionBits` | `$0491` | Collision result bits |
| `Enemy_SprAttrib` | `$03c5` | Sprite attribute byte |
| `Enemy_SprDataOffset` | `$06e5` | Offset into OAM sprite data |
| `Enemy_Rel_XPos` | `$03ae` | Screen-relative X (set by `RelativeEnemyPosition`) |
| `Enemy_Rel_YPos` | `$03b9` | Screen-relative Y |

### Player Variables

| Variable | Addr | Description |
|---|---|---|
| `Player_X_Position` | `$86` | Player X position |
| `Player_Y_Position` | `$ce` | Player Y position |
| `Player_Y_Speed` | `$9f` | Player vertical speed |
| `PlayerStatus` | `$0756` | 0=small, 1=big, 2=fire |
| `PlayerFacingDir` | `$33` | Player facing direction |

### Game State

| Variable | Addr | Description |
|---|---|---|
| `ObjectOffset` | `$08` | Current enemy slot being processed (X reg) |
| `FrameCounter` | `$09` | Global frame counter |
| `TimerControl` | `$0747` | Nonzero = freeze all movement |
| `BowserHitPoints` | `$0483` | Boss HP (init to 10; 0 = dying, negative = dead) |
| `EnemyFrenzyBuffer` | `$06cb` | Cleared by boss each frame to prevent frenzy spawns |
| `PseudoRandomBitReg` | `$07a7` | PRNG value |
| `PowerUpType` | `$39` | Type of powerup to spawn |

### Sprite OAM

| Variable | Addr | Description |
|---|---|---|
| `Sprite_Y_Position` | `$0200` | OAM Y position (indexed by sprite data offset) |
| `Sprite_Tilenumber` | `$0201` | OAM tile number |
| `Sprite_Attributes` | `$0202` | OAM attributes (palette, flip, priority) |
| `Sprite_X_Position` | `$0203` | OAM X position |

### Sound Queues

| Variable | Addr | Description |
|---|---|---|
| `Square1SoundQueue` | `$ff` | Sound effect queue channel 1 |
| `Square2SoundQueue` | `$fe` | Sound effect queue channel 2 |
| `AreaMusicQueue` | `$fb` | Music queue |

### Scratch RAM

The boss routines use zero-page scratch: `$00`-`$05`, `$eb`. These are assumed temporary and clobbered freely.

### Other

| Variable | Addr | Description |
|---|---|---|
| `VerticalFlipFlag` | `$0109` | Used by `DrawSpriteObject` |
| `Alt_SprDataOffset` | `$06ec` | Alternate sprite data offset (for multi-row draws) |
| `pal_type` | `$7e78` | Custom: palette type (set to 3 on boss death) |

---

## 3. Constants

```asm
; Animation frame indices (into BossGraphics table)
SKIDDING = 3
JUMPING  = 4
SHOOT    = 5*8   ; = 40, byte offset for shoot frame
STANDING = 6

; Sound effects
Sfx_Fireball    = %00100000
Sfx_BigJump     = %00000001
Sfx_Blast       = %00001000
Sfx_GrowPowerUp = %00000010
Silence         = %10000000

; Music
mus_bossdie = 8

; Enemy IDs
BulletBill_FrenzyVar = $08
BowserFlame          = $15
Bowser               = $2d
PowerUpObject        = $2e
```

---

## 4. Graphics Table

The boss is drawn using player-like CHR tiles. Each animation frame is 8 bytes (4 rows of 2 tile IDs).

```asm
BossGraphics:
;                 row0  row1  row2  row3  (each row = 2 tiles: left, right)
  .db $00, $01, $02, $03, $04, $05, $06, $07  ; frame 0: walking 1
  .db $08, $09, $0a, $0b, $0c, $0d, $0e, $0f  ; frame 1: walking 2
  .db $10, $11, $12, $13, $14, $15, $16, $17  ; frame 2: walking 3
  .db $18, $19, $1a, $1b, $1c, $1d, $1e, $1f  ; frame 3: skidding
  .db $20, $21, $22, $23, $24, $25, $26, $27  ; frame 4: jumping
  .db $08, $09, $28, $29, $2a, $2b, $0e, $0f  ; frame 5: fireball throwing (top half reuses walk2)
  .db $00, $01, $4c, $4d, $4a, $51, $4b, $52  ; frame 6: standing
```

---

## 5. Init Routine

Called when the boss enemy object is first created. Sets HP, direction, and timers.

```asm
InitBoss:
      lda Enemy_X_Position,x
      sta BowserOrigXPos        ;store original horizontal position
      lda #$df
      sta BowserFireBreathTimer ;store timer value
      lda #2
      sta Enemy_MovingDir,x     ;face left
      lda #$20
      sta BowserFeetCounter     ;set timer
      sta EnemyFrameTimer,x
      lda #10
      sta BowserHitPoints       ;10 hit points
      lsr
      sta BowserMovementSpeed   ;default movement speed = 5
      rts
```

> **Note:** `BowserOrigXPos`, `BowserFeetCounter`, `BowserMovementSpeed`, `BowserFireBreathTimer`, and `EnemyFrameTimer` are used by the original Bowser init but are NOT used by the shadow Mario `RunBoss` code. You likely only need to initialize `BowserHitPoints`, `Enemy_MovingDir`, and clear the `boss_*` RAM block to zero.

### Minimal Init for Porting

```asm
InitShadowBoss:
      ; Clear all boss RAM
      lda #0
      ldx #BOSS_RAM_SIZE
  @loop:
      sta boss_ram_start,x
      dex
      bpl @loop
      sta boss_freeze

      ; Set HP
      lda #10
      sta BowserHitPoints

      ; Face left
      ldx ObjectOffset
      lda #2
      sta Enemy_MovingDir,x
      rts
```

---

## 6. Update Routine (Main Entry Point)

Called once per frame while the boss is alive. This is the top-level boss tick.

```asm
RunBoss:
      lda boss_freeze
      beq @notfreezed
      dec boss_freeze
      lda #STANDING
      sta boss_animation
      ; Draw every other frame (flicker)
      lda FrameCounter
      lsr
      bcs @nodraw2
      jsr RelativeEnemyPosition
      jsr BossGraphicsHandler
  @nodraw2:
      ldx ObjectOffset
      rts

  @notfreezed:
      lda BowserHitPoints
      bmi AnimateKilledBoss     ; HP < 0: boss is fully dead
      jeq KillBoss              ; HP == 0: playing death countdown

      lda #80
      sta boss_counter          ; reset death countdown each frame while alive
      lda #0
      sta EnemyFrenzyBuffer     ; suppress enemy frenzy spawns

      ; Draw (skip every other frame if invincible for blink effect)
      lda boss_invincibility_timer
      beq @draw
      lda FrameCounter
      lsr
      bcs @nodraw
  @draw:
      jsr RelativeEnemyPosition
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
      lda #$0a
      sta Enemy_BoundBoxCtrl,x  ; set bounding box size
      jsr GetEnemyOffscreenBits
      jsr GetEnemyBoundBox
      jmp PlayerEnemyCollision  ; check player<->boss collision

  @decrement_timer:
      dec boss_invincibility_timer
      rts
```

---

## 7. AI State Machine

`HandleBossMovement` dispatches based on `boss_state`:

```asm
HandleBossMovement:
      lda boss_state
      jsr JumpEngine
      .dw MovBossSide      ; state 0
      .dw MovBossRunning   ; state 1
      .dw MovBossMiddle    ; state 2
```

### State 0: Side/Idle (`MovBossSide`)

Boss stands in place, randomly shoots fireballs and jumps. After `boss_state_timer` expires, transitions to either state 1 (running) or state 2 (middle) based on random + player position.

```asm
MovBossSide:
      lda #STANDING
      sta boss_animation

      ; Random fireball: every 8th frame, 1/4 chance
      lda FrameCounter
      and #%00000111
      bne @no_fireball
      lda PseudoRandomBitReg
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
      lda PseudoRandomBitReg
      lsr
      bcc @no_jump
      lda #-5
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
      bcs @middle
      lda #2                    ; -> state 2 (middle)
      .db $2c                   ; skip next instruction (BIT abs trick)
  @middle:
      lda #1                    ; -> state 1 (running)
      sta boss_state
      inc boss_bridge

  @keep_state:
      dec boss_state_timer
      rts
```

### State 1: Running (`MovBossRunning`)

Boss runs toward the opposite side, accelerating then decelerating/skidding. Optionally jumps if `boss_bridge` is set.

```asm
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

MovStillMoving:
      lda boss_bridge
      beq @no_jump
      dec boss_bridge
      lda Enemy_State,x
      and #%01000000
      bne @no_jump
      lda #-4
      sta Enemy_Y_Speed,x
      jsr BossJump
  @no_jump:
      jmp MoveEnemyHorizontally

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
      jmp MoveEnemyHorizontally

MovChangeState:
      lda PseudoRandomBitReg
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
```

### State 2: Middle (`MovBossMiddle`)

Boss walks to center of arena, then stands and shoots bullet bills from screen edges. After all enemies are cleared, spawns a powerup if player needs one, then transitions to state 1 (running).

```asm
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
      lda PlayerStatus
      cmp #2
      bcs @no_need_for_p        ; player already has fire power
      sta PowerUpType
      lda #PowerUpObject
      cmp Enemy_ID+5
      beq @no_need_for_p        ; powerup already exists
      ; ... (spawns powerup in slot 5) ...
  @no_need_for_p:
      lda #0
      sta boss_bridge
      lda #1
      sta boss_state            ; -> state 1 (running)
      ; Set speed based on facing direction
      lda Enemy_MovingDir,x
      lsr
      bcs @right
      lda #-$20
      .db $2c
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
      ; ... (random Y calculation, alternates left/right side) ...
      jsr BossSpawnSideBullet
  @done:
      rts

MovGoToMiddle:
      ; Walks toward X=$40 (right-facing) or X=$B0 (left-facing)
      ; Jumps onto the center platform when reaching the target
      ; Sets boss_substate=1 and boss_state_timer when arrived
      ; ... (see full source for details) ...
```

### Helpers

```asm
BossSetWalkingAnim:
      lda FrameCounter
      and #%00000011
      cmp #%00000011
      bne @init_anim
      inc boss_walking_anim
      lda boss_walking_anim
      cmp #$03
      bne @init_anim
      lda #0
      sta boss_walking_anim
  @init_anim:
      lda boss_walking_anim
      sta boss_animation
      rts

BossJump:
      lda #Sfx_BigJump
      sta Square1SoundQueue
      lda Enemy_State,x
      ora #%01000000            ; set airborne flag
      sta Enemy_State,x
      rts
```

---

## 8. Draw Routine

Draws the boss as a 16x32 pixel sprite (4 rows of 2 tiles). Uses the SMB1 `DrawSpriteObject` routine.

```asm
BossGraphicsHandler:
      lda Enemy_Y_Position,x
      sta $02                         ; Y position
      lda Enemy_Rel_XPos
      sta $05                         ; screen-relative X
      sta VerticalFlipFlag
      lda #0
      ldy boss_animation
      cpy #SKIDDING
      bne +
      lda #%00000011                  ; flip both axes when skidding
  +   eor Enemy_MovingDir,x
      sta $03                         ; horizontal flip control
      lda #1
      sta $04                         ; sprite palette

      ldy Enemy_SprDataOffset,x
      lda boss_shootanim
      bne @shootanim

      ; Normal draw: look up animation frame in BossGraphics
      lda boss_animation
      asl
      asl
      asl                             ; * 8 = byte offset
      tax
      jsr DrawBossRow                 ; row 0 (head)
      jsr DrawBossRow                 ; row 1 (torso)
      ldy Alt_SprDataOffset+1
      jsr DrawBossRow                 ; row 2 (legs top)
  @drawrest:
      ldy $06eb
      jmp DrawBossRow                 ; row 3 (legs bottom)

  @shootanim:
      ; Shoot: draw shoot frame for top 2 rows, normal for bottom 2
      dec boss_shootanim
      ldx #SHOOT                      ; = 40, offset to shoot frame
      jsr DrawBossRow
      jsr DrawBossRow
      ldy Alt_SprDataOffset+1
      jsr DrawBossRow
      lda boss_animation
      asl
      asl
      asl
      clc
      adc #6                          ; skip to bottom 2 tiles of normal frame
      tax
      jmp @drawrest

DrawBossRow:
      lda BossGraphics,x
      sta $00                         ; left tile
      lda BossGraphics+1,x
      sta $01                         ; right tile
      jmp DrawSpriteObject            ; engine routine: writes 2 sprites to OAM
```

---

## 9. Projectile Spawning

### Spawn Fireball

Fires a `BowserFlame` projectile in the boss's facing direction.

```asm
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
      lda #BowserFlame
      jsr BossSpawnEnemy
      ldx ObjectOffset
  @done:
      rts
```

### Spawn Side Bullet

Spawns a `BulletBill_FrenzyVar` from the left or right edge of the screen at a given Y position.

```asm
; A = direction (1=left edge, 2=right edge)
; $00 = Y position
BossSpawnSideBullet:
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
      lda #-60
      sta bbSpeed,y             ; speed = -60 (move right... from left)
      jmp @rest

  @rightdir:
      lda #0
      sta $01                   ; X = right edge ($00)
      lda #60
      sta bbSpeed,y             ; speed = 60 (move left... from right)

  @rest:
      lda #8
      sta $03                   ; bounding box
      lda #0
      sta $02                   ; page
      lda #BulletBill_FrenzyVar
      sty $eb
      jsr BossSpawnEnemy
      ldy $eb
      pla
      sta Enemy_MovingDir,y
      ldx ObjectOffset
  @done:
      rts
```

### Helpers

```asm
; Spawn enemy in slot Y with parameters in $00-$03
; A = enemy ID
BossSpawnEnemy:
      sta Enemy_ID,y
      lda $00
      sta Enemy_Y_Position,y
      lda $01
      sta Enemy_X_Position,y
      lda $02
      sta Enemy_PageLoc,y
      lda $03
      sta Enemy_BoundBoxCtrl,y
      lda #$01
      sta Enemy_Y_HighPos,y
      sta Enemy_Flag,y
      lsr
      sta Enemy_X_MoveForce,y
      sta Enemy_State,y
      ldx $04                   ; slot index
      jmp CheckpointEnemyID     ; engine: initialize enemy by ID

; Find first empty enemy slot (1-5), return in Y. N flag set if none found.
FindID:
      ldy #5
  @loop:
      lda Enemy_Flag,y
      beq @foundSlot
      dey
      bpl @loop
  @foundSlot:
      sty $04
      rts
```

---

## 10. Death Sequence

### KillBoss (HP == 0 countdown)

```asm
KillBoss:
      lda boss_counter
      bmi +                     ; counter expired -> signal fully dead
      lda #Silence
      sta AreaMusicQueue
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
      jsr RelativeEnemyPosition
      jsr BossGraphicsHandler
  @nodraw:
      ldx ObjectOffset
      rts

  +   ; Counter expired: signal death complete
      dec BowserHitPoints       ; HP goes to $FF (negative)
      lda #mus_bossdie
      sta AreaMusicQueue
      rts
```

### AnimateKilledBoss (HP < 0: spawn powerup and erase)

```asm
AnimateKilledBoss:
      jsr EraseEnemyObject

      lda #3
      sta pal_type              ; custom: set palette type for post-boss
      lda #2
      sta PowerUpType
      lda #PowerUpObject
      sta Enemy_ID+5
      lda #0
      sta Enemy_PageLoc+5
      sta Enemy_X_Speed+5
      lda Enemy_X_Position,x
      sta Enemy_X_Position+5    ; spawn at boss's position
      lda #$01
      sta Enemy_Y_HighPos+5
      sta Enemy_Flag+5
      lda Enemy_Y_Position,x
      sta Enemy_Y_Position+5
      lda #$80
      sta Enemy_State+5
      lda #$03
      sta Enemy_BoundBoxCtrl+5

      lda #-3
      sta Enemy_Y_Speed+5      ; powerup floats upward

      lda #Sfx_GrowPowerUp
      sta Square2SoundQueue
      ldx ObjectOffset
      rts
```

---

## 11. Damage Handling

Called when player stomps or fireballs hit the boss. Located separately from the main boss code.

```asm
StompedBoss:
      lda Player_Y_Position
      clc
      adc #10
      cmp Enemy_Y_Position,x
      bcc +
      jmp StompInjury           ; player below boss -> normal stomp injury to PLAYER
  +   lda #$fd
      sta Player_Y_Speed        ; bounce player upward
      lda boss_state
      bne InjuredBoss           ; if not in state 0, just take damage
      lda #0
      sta boss_state_timer      ; if in idle state, also reset timer

InjuredBoss:
      lda boss_invincibility_timer
      bne @done                 ; already invincible, no damage
      inc boss_screech          ; track hit count
      dec BowserHitPoints       ; reduce HP
      lda #80
      sta boss_invincibility_timer  ; 80 frames of invincibility
  @done:
      rts
```

---

## 12. External Dependencies

Your target project must provide these routines:

| Routine | Description |
|---|---|
| `EnemyToBGCollisionDet` | Check enemy object vs background tile collision. Updates `Enemy_State` flags. |
| `MoveD_EnemyVertically` | Apply gravity/vertical speed to enemy. Uses `Enemy_Y_Speed`, `Enemy_Y_Position`. |
| `MoveEnemyHorizontally` | Apply horizontal speed to enemy. Uses `Enemy_X_Speed`, `Enemy_X_Position`. |
| `RelativeEnemyPosition` | Calculate screen-relative X/Y from absolute position. Stores to `Enemy_Rel_XPos`/`Enemy_Rel_YPos`. |
| `GetEnemyOffscreenBits` | Determine if enemy is off any screen edge. |
| `GetEnemyBoundBox` | Set up bounding box coordinates from `Enemy_BoundBoxCtrl`. |
| `PlayerEnemyCollision` | Detect and handle player vs enemy collision (calls `StompedBoss` for this enemy). |
| `DrawSpriteObject` | Draw a row of 2 sprites to OAM. Reads tile IDs from `$00`/`$01`, position from `$02`/`$05`, attributes from `$03`/`$04`. Advances Y by 8 for next row. |
| `EraseEnemyObject` | Clear enemy from active slot (zero out `Enemy_Flag`, etc). |
| `JumpEngine` | Indirect jump dispatch: reads A as index, jumps to address from following `.dw` table. |
| `MoveFireballVertically` | Apply gravity to fireball projectiles (used by `RunFireball`, not included here). |
| `OffscreenBoundsCheck` | Despawn enemy if too far offscreen (used by `RunFireball`, not included here). |
| `CheckpointEnemyID` | Initialize a newly spawned enemy by its ID. X = slot index. |
| `StompInjury` | Standard SMB1 stomp injury to player (when stomp fails). |
| `DrawFirebar` | Draw a single firebar/fireball sprite (used by fireball draw, not included here). |
| `ProcFireball` / `RunFireball` | Fireball projectile update logic (not included; you need your own projectile system). |
