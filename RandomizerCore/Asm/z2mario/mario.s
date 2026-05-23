
.include "z2r.inc"

.import SlightlyModifiedCollisionRoutine
.import PlayerBankTable
.import METASPRITE_SMALL_MARIO_DEATH, FIRE_MARIO_OFFSET, METASPRITE_FIRE_MARIO_SWIMMING_STILL_1, METASPRITE_SMALL_FIRE_SWIMMING_STILL_1
.import METASPRITE_BIG_MARIO_JUMPING, METASPRITE_BIG_MARIO_SWIMMING_1_KICK, METASPRITE_BIG_MARIO_STANDING, METASPRITE_BIG_MARIO_SKIDDING
.import METASPRITE_BIG_MARIO_WALKING_1, METASPRITE_BIG_MARIO_CLIMBING_1, METASPRITE_BIG_MARIO_CROUCHING, METASPRITE_FIRE_MARIO_STANDING
.import METASPRITE_SMALL_MARIO_JUMPING, METASPRITE_SMALL_MARIO_SWIMMING_1_KICK, METASPRITE_SMALL_MARIO_STANDING, METASPRITE_SMALL_MARIO_SKIDDING
.import METASPRITE_SMALL_MARIO_WALKING_1, METASPRITE_SMALL_MARIO_CLIMBING_1, METASPRITE_SMALL_MARIO_GROW_STANDING, METASPRITE_FIREBALL_FRAME_1
.import METASPRITE_FIREBALL_FRAME_2, METASPRITE_EXPLOSION_FRAME_1, METASPRITE_EXPLOSION_FRAME_2, METASPRITE_EXPLOSION_FRAME_3
.import METASPRITE_HAMMER_FRAME_1, METASPRITE_HAMMER_FRAME_2
.import SWIMMING_ANIMATION_FRAME_COUNT

.export GameRoutines, ProcFireball_Bubble, PlayerGfxHandler

.segment "PRG0"
; .reloc

;-------------------------------------------------------------------------------------

.proc GameRoutines
;.ifdef DEMO_CODE
  ; lda #1
  ; sta $0785 ;candle
  ; sta $078c ;key
  ; sta $078b ;hammer
  ; sta $0788 ;boots
  ; sta $0787 ;raft
  ; sta $077b ; shield spell
  ; sta $077c ; jump spell
  ; sta $077d ; life spell
  ; sta $077e ; fairy spell
  ; sta $077f ; fire spell
  ; sta $0780 ; reflect spell
  ; sta $0781 ; spell spell
  ; sta $0782 ; thunder spell
  ; lda #8
  ; sta $0783 ; magic jar count
  ; sta $0778 ; magic level

;   lda #$ff
;   sta $0773 ;infinite magic
;   sta $0774 ;infinite health
;.endif

  lda GameEngineSubroutine  ;run routine based on number (a few of these routines are
  jsr JumpEngine            ;merely placeholders as conditions for other routines)

  .word $0000 ; Entrance_GameTimerSetup
  .word $0000 ; Vine_AutoClimb
  .word $0000 ; SideExitPipeEntry
  .word $0000 ; VerticalPipeEntry
  .word $0000 ; FlagpoleSlide
  .word $0000 ; PlayerEndLevel
  .word PlayerLoseLife
  .word $0000 ; PlayerEntrance
  .word PlayerCtrlRoutine
  .word PlayerChangeSize
  .word PlayerInjuryBlink
  .word PlayerDeath
  .word PlayerFireFlower
.endproc


; ;-------------------------------------------------------------------------------------
; ; ; .reloc
; PlayerEntrance:
;   lda AltEntranceControl    ;check for mode of alternate entry
;   cmp #$02
;   beq EntrMode2             ;if found, branch to enter from pipe or with vine
;   lda #$00
;   ldy Player_Y_Position     ;if vertical position above a certain
;   cpy #$30                  ;point, nullify controller bits and continue
;   bcc AutoControlPlayer     ;with player movement code, do not return
;     lda PlayerEntranceCtrl    ;check player entry bits from header
;     cmp #$06
;     beq ChkBehPipe            ;if set to 6 or 7, execute pipe intro code
;     cmp #$07                  ;otherwise branch to normal entry
;     bne PlayerRdy
; ChkBehPipe:
;   ; lda InPipeTransition
;   lda Player_SprAttrib        ;check for sprite attributes
;   bne IntroEntr             ;branch if found
;     lda #$01
;     jmp AutoControlPlayer     ;force player to walk to the right
; IntroEntr:
;   jsr EnterSidePipe         ;execute sub to move player to the right
;   dec ChangeAreaTimer       ;decrement timer for change of area
;   bne ExitEntr              ;branch to exit if not yet expired
;     ; inc DisableIntermediate   ;set flag to skip world and lives display
;     ; jmp NextArea              ;jump to increment to next area and set modes
; EntrMode2:
;   lda JoypadOverride        ;if controller override bits set here,
;   bne VineEntr              ;branch to enter with vine
;     ; lda #3
;     ; jsr SetupPipeTransitionOverlay
;     lda #$ff                  ;otherwise, set value here then execute sub
;     jsr MovePlayerYAxis       ;to move player upwards
;     lda Player_Y_Position     ;check to see if player is at a specific coordinate
;     cmp #$91                  ;if player risen to a certain point (this requires pipes
;     bcs @ContinuePipeEntry    ;to be at specific height to look/function right) branch
;     ; .import FRAME_LAG_COUNT
;       ; lda #FRAME_LAG_COUNT
;       ; lda #0
;       ; sta PipeExitTimer
;       ; jsr SetupPipeTransitionOverlay
;       jmp PlayerRdy
; @ContinuePipeEntry:
;     rts                       ;to the last part, otherwise leave
; VineEntr:
;   lda Vine_Height
;   cmp #$60                  ;check vine height
;   bne ExitEntr              ;if vine not yet reached maximum height, branch to leave
;   lda Player_Y_Position     ;get player's vertical coordinate
;   cmp #$99                  ;check player's vertical coordinate against preset value
;   ldy #$00                  ;load default values to be written to
;   lda #$01                  ;this value moves player to the right off the vine
;   bcc OffVine               ;if vertical coordinate < preset value, use defaults
;   lda #$03
;   sta Player_State          ;otherwise set player state to climbing
;   iny                       ;increment value in Y
;   ; lda #$08                  ;set block in block buffer to cover hole, then
;   ; sta Block_Buffer_1+$b4    ;use same value to force player to climb
; OffVine:
;   sty DisableCollisionDet   ;set collision detection disable flag
;   jsr AutoControlPlayer     ;use contents of A to move player up or right, execute sub
;   lda Player_X_Position
;   cmp #$48                  ;check player's horizontal position
;   bcc ExitEntr              ;if not far enough to the right, branch to leave
; PlayerRdy:
;   lda #$08                  ;set routine to be executed by game engine next frame
;   sta GameEngineSubroutine
;   lda #$01                  ;set to face player to the right
;   sta PlayerFacingDir
;   lsr                       ;init A
;   sta AltEntranceControl    ;init mode of entry
;   sta DisableCollisionDet   ;init collision detection disable flag
;   sta JoypadOverride        ;nullify controller override bits
; ExitEntr:
;   rts                       ;leave!


;-------------------------------------------------------------------------------------
;$07 - used to hold upper limit of high byte when player falls down hole
AutoControlPlayer:
  sta SavedJoypadBits         ;override controller bits with contents of A if executing here

PlayerCtrlRoutine:
  lda GameEngineSubroutine    ;check task here
  ; prevent moving on death
  cmp #$0b
  beq SizeChk
    lda AreaType                ;are we in a water type area?
    bne SaveJoyp                ;if not, branch
      ldy Player_Y_HighPos
      dey                         ;if not in vertical area between
      bne DisJoyp                 ;status bar and bottom, branch
        lda Player_Y_Position
        cmp #$d0                    ;if nearing the bottom of the screen or
        bcc SaveJoyp                ;not in the vertical area between status bar or bottom,
DisJoyp:
          lda #$00                    ;disable controller bits
          sta SavedJoypadBits
SaveJoyp:
  lda SavedJoypadBits         ;otherwise store A and B buttons in $0a
  and #%11000000
  sta A_B_Buttons
  lda SavedJoypadBits         ;store left and right buttons in $0c
  and #%00000011
  sta Left_Right_Buttons
  lda SavedJoypadBits         ;store up and down buttons in $0b
  and #%00001100
  sta Up_Down_Buttons

  and #%00000100              ;check for pressing down
  beq SizeChk                 ;if not, branch
    lda Player_State            ;check player's state
    bne SizeChk                 ;if not on the ground, branch
      ldy Left_Right_Buttons      ;check left and right
      beq SizeChk                 ;if neither pressed, branch
        lda #$00
        sta Left_Right_Buttons      ;if pressing down while on the ground,
        sta Up_Down_Buttons         ;nullify directional bits
SizeChk:
  lda $a7   ; flip collision bits to match mario style
  eor #$ff
  sta $a7
  jsr PlayerMovementSubs      ;run movement subroutines
  ldy #$01                    ;is player small?
  lda PlayerSize
  bne ChkMoveDir
    ldy #$00                    ;check for if crouching
    lda CrouchingFlag
    beq ChkMoveDir              ;if not, branch ahead
      ldy #$02                    ;if big and crouching, load y with 2
ChkMoveDir:
  ; sty Player_BoundBoxCtrl     ;set contents of Y as player's bounding box size control
  lda #$01                    ;set moving direction to right by default
  ldy Player_X_Speed          ;check player's horizontal speed
  beq PlayerSubs              ;if not moving at all horizontally, skip this part
    bpl SetMoveDir              ;if moving to the right, use default moving direction
      asl                         ;otherwise change to move to the left
  SetMoveDir:
    sta Player_MovingDir        ;set moving direction
PlayerSubs:

  ; jsr $9610 ; update mario scroll position

  jsr ScrollHandler           ;move the screen if necessary
  ; jsr GetPlayerOffscreenBits  ;get player's offscreen bits
  ; jsr RelativePlayerPosition  ;get coordinates relative to the screen
  ; ldx #$00                    ;set offset for player object
  ; jsr BoundingBoxCore     ;get player's bounding box coordinates
  jsr PlayerBGCollision       ;do collision detection and process

  lda $a7   ; flip collision bits to match z2 style
  eor #$ff
  sta $a7

  ; This is the offset for the table that holds the Y add val for link
  ldy #$1d
  ; Jump to the middle of the original link collision routine
  ; jsr $e070 ; link original bg collision routine
  jmp SlightlyModifiedCollisionRoutine
  ; rts

;   lda Player_Y_Position
;   cmp #$40                    ;check to see if player is higher than 64th pixel
;   bcc PlayerHole              ;if so, branch ahead
;     lda GameEngineSubroutine
;     cmp #$05                    ;if running end-of-level routine, branch ahead
;     beq PlayerHole
;     cmp #$07                    ;if running player entrance routine, branch ahead
;     beq PlayerHole
;     cmp #$04                    ;if running routines $00-$03, branch ahead
;     bcc PlayerHole
;       lda Player_SprAttrib
;       and #%11011111              ;otherwise nullify player's
;       sta Player_SprAttrib        ;background priority flag
; PlayerHole:
;   lda GameEngineSubroutine
;   cmp #$0a ; Ignore falling into hole check if taking damage
;   beq ExitCtrl
;   lda Player_Y_HighPos        ;check player's vertical high byte
;   cmp #$02                    ;for below the screen
;   bmi ExitCtrl                ;branch to leave if not that far down
;   ldx #$01
;   stx ScrollLock              ;set scroll lock
;   ldy #$04
;   sty R7                     ;set value here
;   ldx #$00                    ;use X as flag, and clear for cloud level
;   ; ldy GameTimerExpiredFlag    ;check game timer expiration flag
;   ; bne HoleDie                 ;if set, branch
;     ; ldy CloudTypeOverride       ;check for cloud type override
;     ; bne ChkHoleX                ;skip to last part if found
; ; HoleDie:
;   inx                         ;set flag in X for player death
;   ldy GameEngineSubroutine
;   cpy #$0b                    ;check for some other routine running
;   beq ChkHoleX                ;if so, branch ahead
;     ldy DeathMusicLoaded        ;check value here
;     bne HoleBottom              ;if already set, branch to next part
;       iny
;       sty EventMusicQueue         ;otherwise play death music
;       sty DeathMusicLoaded        ;and set value here
; HoleBottom:
;   ldy #$06
;   sty R7
; ChkHoleX:
;   cmp R7                      ;compare vertical high byte with value set here
;   bmi ExitCtrl                ;if less, branch to leave
;     dex                         ;otherwise decrement flag in X
;     bmi CloudExit               ;if flag was clear, branch to set modes and other values
; CheckMusicFinished:
;   ldy DeathMusicLoaded        ;check to see if music is still playing
;   beq DeathMusicEnded         ;branch to leave if so
;     lda EventMusicBuffer
;     ora EventMusicQueue
;     bne ExitCtrl
; DeathMusicEnded:
;     lda #$06                    ;otherwise set to run lose life routine
;     sta GameEngineSubroutine    ;on next frame
; ExitCtrl:
;   jmp PlayerGfxHandler
;   rts                         ;leave

; CloudExit:
;   lda #$00
;   sta JoypadOverride      ;clear controller override bits if any are set
;   jsr SetEntr             ;do sub to set secondary mode
;   inc AltEntranceControl  ;set mode of entry to 3
  rts

.proc PlayerLoseLife
  ; inc DisableScreenFlag    ;disable screen and sprite 0 check
  ; lda #$00
  ; sta Sprite0HitDetectFlag
;   lda #Silence             ;silence music
;   sta EventMusicQueue
;   dec NumberofLives        ;take one life from player
;   bpl StillInGame          ;if player still has lives, branch
;   ; lda #$00
;   ; sta OperMode_Task        ;initialize mode task,
;   ; lda #MODE_GAMEOVER       ;switch to game over mode
;   ; sta OperMode             ;and leave
;   rts
; StillInGame:
;   lda WorldNumber          ;multiply world number by 2 and use
;   asl                      ;as offset
;   tax
;   lda LevelNumber          ;if in area -3 or -4, increment
;   and #$02                 ;offset by one byte, otherwise
;   beq GetHalfway           ;leave offset alone
;   inx
; GetHalfway:
;   ldy HalfwayPageNybbles,x ;get halfway page number with offset
;   lda LevelNumber          ;check area number's LSB
;   lsr
;   tya                      ;if in area -2 or -4, use lower nybble
;   bcs MaskHPNyb
;   lsr                      ;move higher nybble to lower if area
;   lsr                      ;number is -1 or -3
;   lsr
;   lsr
; MaskHPNyb:
;   and #%00001111           ;mask out all but lower nybble
;   cmp ScreenLeft_PageLoc
;   beq SetHalfway           ;left side of screen must be at the halfway page,
;   bcc SetHalfway           ;otherwise player must start at the
;   lda #$00                 ;beginning of the level
; SetHalfway:
;   sta HalfwayPage          ;store as halfway page for player
;   jsr TransposePlayers     ;switch players around if 2-player game

  rts
;   jmp RunGameOver::ContinueGame         ;continue the game
.endproc

;-------------------------------------------------------------------------------------
;$00 - used to store player's vertical offscreen bits
; .reloc
.proc PlayerGfxHandler
  lda InjuryTimer             ;if player's injured invincibility timer
  beq CntPl                   ;not set, skip checkpoint and continue code
  lda FrameCounter
  lsr                         ;otherwise check frame counter and branch
  bcs ClearMarioSprite        ;to leave on every other frame (when d0 is set)
CntPl:
  lda GameEngineSubroutine    ;if executing specific game engine routine,
  cmp #$0b                    ;branch ahead to some other part
  beq PlayerKilled
  lda PlayerChangeSizeFlag    ;if grow/shrink flag set
  bne DoChangeSize            ;then branch to some other code

  jsr FindPlayerAction        ;otherwise jump and return

  lda SwimmingFlag
  beq Exit
    ; if the player is standing on the ground, don't animate the leg kicking.
    lda Player_State
    beq Exit
      ; if the player is swimming, every 8 frames switch metasprite to use the kick animation
      lda FrameCounter
      and #%00000100              ;check frame counter for d2 set (8 frames every
      bne Exit                    ;eighth frame), and branch if set to leave
        ; a bit hacky here, but we have two types of offsets. If its one of the glitchy frames, bump the metasprite by one, else
        ; add our animation extent offset
        lda ObjectMetasprite
        cmp #METASPRITE_FIRE_MARIO_SWIMMING_STILL_1
        beq GlitchySprite
        cmp #METASPRITE_SMALL_FIRE_SWIMMING_STILL_1
        beq GlitchySprite
          clc
          adc #SWIMMING_ANIMATION_FRAME_COUNT
          sta ObjectMetasprite
          bne Exit
    GlitchySprite:
      ; Go to the next frame of the glitch animation
      inc ObjectMetasprite
Exit:
  rts                         ;then leave

ClearMarioSprite:
  lda #0
  sta ObjectMetasprite
  rts
.endproc

FindPlayerAction:
  jsr ProcessPlayerAction       ;find proper offset to graphics table by player's actions
  jmp PlayerGfxProcessing       ;draw player, then process for fireball throwing

DoChangeSize:
  jsr HandleChangeSize          ;find proper offset to graphics table for grow/shrink
  jmp PlayerGfxProcessing       ;draw player, then process for fireball throwing

PlayerKilled:
  ; ldy #$0e                      ;load offset for player killed
  ; lda PlayerGfxTblOffsets,y     ;get offset to graphics table
  lda #METASPRITE_SMALL_MARIO_DEATH

PlayerGfxProcessing:
  sta ObjectMetasprite

  lda FireballThrowingTimer
  beq PlayerOffscreenChk        ;if fireball throw timer not set, skip to the end
    ldy #$00                      ;set value to initialize by default
    lda PlayerAnimTimer           ;get animation frame timer
    cmp FireballThrowingTimer     ;compare to fireball throw timer
    sty FireballThrowingTimer     ;initialize fireball throw timer
    bcs PlayerOffscreenChk        ;if animation frame timer => fireball throw timer skip to end
      sta FireballThrowingTimer     ;otherwise store animation timer into fireball throw timer

      ; Change the Mario sprite to the fire version
      lda ObjectMetasprite
      clc
      adc #FIRE_MARIO_OFFSET
      sta ObjectMetasprite

      lda Player_X_Speed
      ora Left_Right_Buttons        ;check for horizontal speed or left/right button press
      bne SUpdR                     ;if no speed or button press, branch using set value in Y
        ; Use the glitchy version of the sprite
        lda PlayerSize
        bne SmallFireMario
          lda #METASPRITE_FIRE_MARIO_SWIMMING_STILL_1
          bne SetFrame
        SmallFireMario:
          lda #METASPRITE_SMALL_FIRE_SWIMMING_STILL_1
      SetFrame:
        sta ObjectMetasprite
SUpdR:

PlayerOffscreenChk:
  ; lda SprObject_Y_Position,x  ;load vertical coordinate low
  ; sta SprObject_Rel_YPos,y    ;store here
  lda SprObject_X_Position  ;load horizontal coordinate
  sec                         ;subtract left edge coordinate
  sbc ScreenLeft_X_Pos
  ; sta SprObject_Rel_XPos    ;store result here
  sta Player_Pos_ForScroll
  rts                           ;then we are done!

PlayerGfxTblOffsets:
  ; .byte $20, $28, $c8, $18, $00, $40, $50, $58
  .byte METASPRITE_BIG_MARIO_JUMPING
  .byte METASPRITE_BIG_MARIO_SWIMMING_1_KICK
  .byte METASPRITE_BIG_MARIO_STANDING
  .byte METASPRITE_BIG_MARIO_SKIDDING
  .byte METASPRITE_BIG_MARIO_WALKING_1
  .byte METASPRITE_BIG_MARIO_CLIMBING_1
  .byte METASPRITE_BIG_MARIO_CROUCHING
  .byte METASPRITE_FIRE_MARIO_STANDING
  ; .byte $80, $88, $b8, $78, $60, $a0, $b0, $b8
  .byte METASPRITE_SMALL_MARIO_JUMPING
  .byte METASPRITE_SMALL_MARIO_SWIMMING_1_KICK
  .byte METASPRITE_SMALL_MARIO_STANDING
  .byte METASPRITE_SMALL_MARIO_SKIDDING
  .byte METASPRITE_SMALL_MARIO_WALKING_1
  .byte METASPRITE_SMALL_MARIO_CLIMBING_1
  .byte METASPRITE_SMALL_MARIO_DEATH
GrowAnimation = * - PlayerGfxTblOffsets
  .byte METASPRITE_SMALL_MARIO_GROW_STANDING

HandleChangeSize:
  ldy PlayerAnimCtrl           ;get animation frame control
  lda FrameCounter
  and #%00000011               ;get frame counter and execute this code every
  bne GorSLog                  ;fourth frame, otherwise branch ahead
    iny                          ;increment frame control
    cpy #$0a                     ;check for preset upper extent
    bcc CSzNext                  ;if not there yet, skip ahead to use
      ldy #$00                     ;otherwise initialize both grow/shrink flag
      sty PlayerChangeSizeFlag     ;and animation frame control
CSzNext:
    sty PlayerAnimCtrl           ;store proper frame control
GorSLog:
  lda PlayerSize               ;get player's size
  bne ShrinkPlayer             ;if player small, skip ahead to next part
    lda ChangeSizeOffsetAdder,y  ;get offset adder based on frame control as offset
    ldy #GrowAnimation           ;load offset for player growing

GetOffsetFromAnimCtrl:
    ; asl                        ;multiply animation frame control
    ; asl                        ;by eight to get proper amount
    ; asl                        ;to add to our offset
    clc
    adc PlayerGfxTblOffsets,y  ;add to offset to graphics table
    rts                        ;and return with result in A
ChangeSizeOffsetAdder:
  .byte $00, $01, $00, $01, $00, $01, $02, $00, $01, $02
  .byte $02, $00, $02, $00, $02, $00, $02, $00, $02, $00

ShrinkPlayer:
  tya                          ;add ten bytes to frame control as offset
  clc
  adc #$0a                     ;this thing apparently uses two of the swimming frames
  tax                          ;to draw the player shrinking
  ldy #$09                     ;load offset for small player swimming
  lda ChangeSizeOffsetAdder,x  ;get what would normally be offset adder
  bne ShrPlF                   ;and branch to use offset if nonzero
    ldy #$01                     ;otherwise load offset for big player swimming
ShrPlF:
  lda PlayerGfxTblOffsets,y    ;get offset to graphics table based on offset loaded
  rts                          ;and leave


;-------------------------------------------------------------------------------------
; .reloc
PlayerChangeSize:
  lda TimerControl    ;check master timer control
  cmp #$f8            ;for specific moment in time
  bne EndChgSize      ;branch if before or after that point
  jmp InitChangeSize  ;otherwise run code to get growing/shrinking going
EndChgSize:
  cmp #$c4            ;check again for another specific moment
  bne ExitChgSize     ;and branch to leave if before or after that point
  ; Z2Mario - clear scroll lock after you change size
;  lda #0
  dec ScrollLock
  jmp DonePlayerTask  ;otherwise do sub to init timer control and set routine
ExitChgSize:
  rts ; TODO check this RTS can be removed                 ;and then leave

FinishedInjuryBlink:
  ; Z2Mario - clear scroll lock after you change size
;  lda #0
  dec ScrollLock
  jmp DonePlayerTask
;-------------------------------------------------------------------------------------
; .reloc
PlayerInjuryBlink:
  lda TimerControl       ;check master timer control
  cmp #$f0               ;for specific moment in time
  bcs ExitBlink          ;branch if before that point
  cmp #$c8               ;check again for another specific point
  beq FinishedInjuryBlink     ;branch if at that point, and not before or after
  jmp PlayerCtrlRoutine  ;otherwise run player control routine
ExitBlink:
  bne ExitBoth           ;do unconditional branch to leave

InitChangeSize:
  ldy PlayerChangeSizeFlag  ;if growing/shrinking flag already set
  bne ExitBoth              ;then branch to leave
  sty PlayerAnimCtrl        ;otherwise initialize player's animation frame control
  inc PlayerChangeSizeFlag  ;set growing/shrinking flag
  lda Player_State
  bne +
    lda #Sfx_PowerUpGrab
    sta Square2SoundQueue       ;load grow up sound
  +
  lda PlayerSize
  eor #$01                  ;invert player's size
  sta PlayerSize
ExitBoth:
  rts                       ;leave

;-------------------------------------------------------------------------------------
;$00 - used in CyclePlayerPalette to store current palette to cycle
; .reloc
PlayerDeath:
  lda TimerControl       ;check master timer control
  cmp #$f0               ;for specific moment in time
  bcs ExitTask           ;branch to leave if before that point
  jmp PlayerCtrlRoutine  ;otherwise run player control routine

DonePlayerTask:
  lda #$00
  sta TimerControl          ;initialize master timer control to continue timers
  lda #$08
  sta GameEngineSubroutine  ;set player control routine to run next frame
ExitTask:
  rts                       ;leave

CyclePlayerPalettePreload:
  lda R0
  jmp CyclePlayerPalette

PlayerFireFlower:
  lda TimerControl       ;check master timer control
  cmp #$c0               ;for specific moment in time
  beq ResetPalFireFlower ;branch if at moment, not before or after
  lda FrameCounter       ;get frame counter
  lsr
  lsr                    ;divide by four to change every four frames

CyclePlayerPalette:
  and #$03              ;mask out all but d1-d0 (previously d3-d2)
  sta R0                ;store result here to use as palette bits
  lda Player_SprAttrib  ;get player attributes
  and #%11111100        ;save any other bits but palette bits
  ora R0                ;add palette bits
  sta Player_SprAttrib  ;store as new player attributes
  rts                   ;and leave

ResetPalFireFlower:
  jsr DonePlayerTask    ;do sub to init timer control and run player control routine

ResetPalStar:
  lda Player_SprAttrib  ;get player attributes
  and #%11111100        ;mask out palette bits to force palette 0
  sta Player_SprAttrib  ;store as new player attributes
  rts                   ;and leave

;-------------------------------------------------------------------------------------
.reloc
PlayerMovementSubs:
  lda #$00                  ;set A to init crouch flag by default
  ldy PlayerSize            ;is player small?
  bne SetCrouch             ;if so, branch
  lda Player_State          ;check state of player
  bne ProcMove              ;if not on the ground, branch

  ; ZELDA added check for on elevator. If on elevator set standing
  lda $0754
  beq NotZ2Elevator
    lda #0
    beq SetCrouch
NotZ2Elevator:
  lda Up_Down_Buttons       ;load controller bits for up and down
  and #%00000100            ;single out bit for down button
SetCrouch:
  sta CrouchingFlag         ;store value in crouch flag
  tay
  lda CrouchingFlagConvert,y
  sta $17 ; Zelda 2 CrouchingFlag
ProcMove:
  jsr PlayerPhysicsSub      ;run sub related to jumping and swimming
  lda PlayerChangeSizeFlag  ;if growing/shrinking flag set,
  bne NoMoveSub             ;branch to leave
  lda Player_State
  cmp #$03                  ;get player state
  beq MoveSubs              ;if climbing, branch ahead, leave timer unset
  ldy #$18
  sty ClimbSideTimer        ;otherwise reset timer now
MoveSubs:
  jsr JumpEngine
.word OnGroundStateSub
.word JumpSwimSub
.word FallingSub
.word $0000 ; ClimbingSub ; removed to save space

NoMoveSub: rts

CrouchingFlagConvert:
.byte $01, $00, $00, $00, $00

;-------------------------------------------------------------------------------------
;$00 - used by ClimbingSub to store high vertical adder
.reloc
OnGroundStateSub:
  jsr GetPlayerAnimSpeed     ;do a sub to set animation frame timing
  lda Left_Right_Buttons
  beq GndMove                ;if left/right controller bits not set, skip instruction
  sta PlayerFacingDir        ;otherwise set new facing direction
GndMove:
  jsr ImposeFriction         ;do a sub to impose friction on player's walk/run
JmpMove:
  jsr MovePlayerHorizontally ;do another sub to move player horizontally
  sta Player_X_Scroll        ;set returned value as player's movement speed for scroll
; .if ::USE_SMB2J_FEATURES
;     farcall BlowPlayerAround
; .endif
  rts

MovePlayerHorizontally:
      ; lda JumpspringAnimCtrl  ;if jumpspring currently animating,
      ; bne ExXMove             ;branch to leave
      ldx #0                     ;otherwise set zero for offset to use player's stuff
      jmp MoveObjectHorizontally

;--------------------------------

FallingSub:
  lda VerticalForceDown
  sta VerticalForce      ;dump vertical movement force for falling into main one
  jmp LRAir              ;movement force, then skip ahead to process left/right movement

;--------------------------------

JumpSwimSub:
  ldy Player_Y_Speed         ;if player's vertical speed zero
  bpl DumpFall               ;or moving downwards, branch to falling
  lda A_B_Buttons
  and #A_Button              ;check to see if A button is being pressed
  and PreviousA_B_Buttons    ;and was pressed in previous frame
  bne ProcSwim               ;if so, branch elsewhere
  lda JumpOrigin_Y_Position  ;get vertical position player jumped from
  sec
  sbc Player_Y_Position      ;subtract current from original vertical coordinate
  cmp DiffToHaltJump         ;compare to value set here to see if player is in mid-jump
  bcc ProcSwim               ;or just starting to jump, if just starting, skip ahead
DumpFall:
; Force downstab hitbox??
  ; lda Player_Y_Position
  ; sta HitboxYCoord
  lda VerticalForceDown      ;otherwise dump falling into main fractional
  sta VerticalForce
ProcSwim:
  lda SwimmingFlag           ;if swimming flag not set,
  beq LRAir                  ;branch ahead to last part
  jsr GetPlayerAnimSpeed     ;do a sub to get animation frame timing
  lda Player_Y_Position
  cmp #$14                   ;check vertical position against preset value
  bcs LRWater                ;if not yet reached a certain position, branch ahead
  lda #$18
  sta VerticalForce          ;otherwise set fractional
LRWater:
  lda Left_Right_Buttons     ;check left/right controller bits (check for swimming)
  beq LRAir                  ;if not pressing any, skip
  sta PlayerFacingDir        ;otherwise set facing direction accordingly
LRAir:
  lda Left_Right_Buttons     ;check left/right controller bits (check for jumping/falling)
  beq JSMove                 ;if not pressing any, skip
  jsr ImposeFriction         ;otherwise process horizontal movement
JSMove:
; .if ::USE_SMB2J_FEATURES
;   jsr JmpMove
; .else
  jsr MovePlayerHorizontally ;do a sub to move player horizontally
  sta Player_X_Scroll        ;set player's speed here, to be used for scroll later
; .endif
  lda GameEngineSubroutine
  cmp #$0b                   ;check for specific routine selected
  bne ExitMov1               ;branch if not set to run
    lda #$28
    sta VerticalForce          ;otherwise set fractional
ExitMov1:
  jmp MovePlayerVertically   ;jump to move player vertically, then leave

;--------------------------------
; .reloc
; ClimbAdderLow:
;   .byte $0e, $04, $fc, $f2
; ClimbAdderHigh:
;   .byte $00, $00, $ff, $ff

; ClimbingSub:
;              lda Player_YMoveForceFractional
;              clc                      ;add movement force to dummy variable
;              adc Player_Y_MoveForce   ;save with carry
;              sta Player_YMoveForceFractional
;              ldy #$00                 ;set default adder here
;              lda Player_Y_Speed       ;get player's vertical speed
;              bpl MoveOnVine           ;if not moving upwards, branch
;              dey                      ;otherwise set adder to $ff
; MoveOnVine:  sty R0                   ;store adder here
;              adc Player_Y_Position    ;add carry to player's vertical position
;              sta Player_Y_Position    ;and store to move player up or down
;              lda Player_Y_HighPos
;              adc R0                   ;add carry to player's page location
;              sta Player_Y_HighPos     ;and store
;              lda Left_Right_Buttons   ;compare left/right controller bits
;              and Player_CollisionBits ;to collision flag
;              beq InitCSTimer          ;if not set, skip to end
;              ldy ClimbSideTimer       ;otherwise check timer
;              bne ExitCSub             ;if timer not expired, branch to leave
;              ldy #$18
;              sty ClimbSideTimer       ;otherwise set timer now
;              ldx #$00                 ;set default offset here
;              ldy PlayerFacingDir      ;get facing direction
;              lsr                      ;move right button controller bit to carry
;              bcs ClimbFD              ;if controller right pressed, branch ahead
;              inx
;              inx                      ;otherwise increment offset by 2 bytes
; ClimbFD:     dey                      ;check to see if facing right
;              beq CSetFDir             ;if so, branch, do not increment
;              inx                      ;otherwise increment by 1 byte
; CSetFDir:    lda Player_X_Position
;              clc                      ;add or subtract from player's horizontal position
;              adc ClimbAdderLow,x      ;using value here as adder and X as offset
;              sta Player_X_Position
;              lda Player_PageLoc       ;add or subtract carry or borrow using value here
;              adc ClimbAdderHigh,x     ;from the player's page location
;              sta Player_PageLoc
;              lda Left_Right_Buttons   ;get left/right controller bits again
;              eor #%00000011           ;invert them and store them while player
;              sta PlayerFacingDir      ;is on vine to face player in opposite direction
; ExitCSub:    rts                      ;then leave
; InitCSTimer: sta ClimbSideTimer       ;initialize timer here
;              rts

;-------------------------------------------------------------------------------------
;$00 - used to store offset to friction data
.reloc
JumpMForceData:
      .byte $20, $20, $1e, $28, $28, $0d, $04

FallMForceData:
      .byte $70, $70, $60, $90, $90, $0a, $09

PlayerYSpdData:
      .byte $fc, $fc, $fc, $fb, $fb, $fe, $ff

InitMForceData:
      .byte $00, $00, $00, $00, $00, $80, $00

MaxLeftXSpdData:
      .byte $d8, $e8, $f0

MaxRightXSpdData:
      .byte $28, $18, $10
      .byte $0c ;used for pipe intros

FrictionData:
      .byte $e4, $98, $d0

Climb_Y_SpeedData:
      .byte $00, $ff, $01

Climb_Y_MForceData:
      .byte $00, $20, $ff

PlayerPhysicsSub:
; Remove climbing data
;            lda Player_State          ;check player state
;            cmp #$03
;            bne CheckForJumping       ;if not climbing, branch
;            ldy #$00
;            lda Up_Down_Buttons       ;get controller bits for up/down
;            and Player_CollisionBits  ;check against player's collision detection bits
;            beq ProcClimb             ;if not pressing up or down, branch
;            iny
;            and #%00001000            ;check for pressing up
;            bne ProcClimb
;            iny
; ProcClimb: ldx Climb_Y_MForceData,y  ;load value here
;            stx Player_Y_MoveForce    ;store as vertical movement force
;            lda #$08                  ;load default animation timing
;            ldx Climb_Y_SpeedData,y   ;load some other value here
;            stx Player_Y_Speed        ;store as vertical speed
;            bmi SetCAnim              ;if climbing down, use default animation timing value
;            lsr                       ;otherwise divide timer setting by 2
; SetCAnim:  sta PlayerAnimTimerSet    ;store animation timer setting and leave
;            rts

CheckForJumping:
  ; ZELDA added check for on elevator
  lda $0754
  bne NoJump
        lda JumpspringAnimCtrl    ;if jumpspring animating,
        bne NoJump                ;skip ahead to something else
        lda A_B_Buttons           ;check for A button press
        and #A_Button
        beq NoJump                ;if not, branch to something else
        and PreviousA_B_Buttons   ;if button not pressed in previous frame, branch
        beq ProcJumping
NoJump: jmp X_Physics             ;otherwise, jump to something else

ProcJumping:
           lda Player_State           ;check player state
           beq InitJS                 ;if on the ground, branch
           lda SwimmingFlag           ;if swimming flag not set, jump to do something else
           beq NoJump                 ;to prevent midair jumping, otherwise continue
           lda JumpSwimTimer          ;if jump/swim timer nonzero, branch
           bne InitJS
           lda Player_Y_Speed         ;check player's vertical speed
           bpl InitJS                 ;if player's vertical speed motionless or down, branch
           jmp X_Physics              ;if timer at zero and player still rising, do not swim
InitJS:    lda #$20                   ;set jump/swim timer
           sta JumpSwimTimer
           ldy #$00                   ;initialize vertical force and dummy variable
           sty Player_YMoveForceFractional
           sty Player_Y_MoveForce
           lda Player_Y_HighPos       ;get vertical high and low bytes of jump origin
           sta JumpOrigin_Y_HighPos   ;and store them next to each other here
           lda Player_Y_Position
           sta JumpOrigin_Y_Position
           lda #$01                   ;set player state to jumping/swimming
           sta Player_State
           lda Player_XSpeedAbsolute  ;check value related to walking/running speed
           cmp #$09
           bcc ChkWtr                 ;branch if below certain values, increment Y
           iny                        ;for each amount equal or exceeded
           cmp #$10
           bcc ChkWtr
           iny
           cmp #$19
           bcc ChkWtr
           iny
           cmp #$1c
           bcc ChkWtr                 ;note that for jumping, range is 0-4 for Y
           iny
ChkWtr:    lda #$01                   ;set value here (apparently always set to 1)
           sta DiffToHaltJump
           lda SwimmingFlag           ;if swimming flag disabled, branch
           beq GetYPhy
           ldy #$05                   ;otherwise set Y to 5, range is 5-6
          ;  lda Whirlpool_Flag         ;if whirlpool flag not set, branch
          ;  beq GetYPhy
          ;  iny                        ;otherwise increment to 6
GetYPhy:   

    ; stat tracking the mario jump counter
    inc StatUpStabCount+0
    bne @nohighinc
    inc StatUpStabCount+1
@nohighinc:
           lda JumpMForceData,y       ;store appropriate jump/swim
           sta VerticalForce          ;data here
           lda FallMForceData,y
           sta VerticalForceDown
           lda InitMForceData,y
           sta Player_Y_MoveForce
           lda PlayerYSpdData,y
           sta Player_Y_Speed
           lda SwimmingFlag           ;if swimming flag disabled, branch
           beq PJumpSnd
           lda #Sfx_EnemyStomp        ;load swim/goomba stomp sound into
           sta Square1SoundQueue      ;square 1's sfx queue
           lda Player_Y_Position
           cmp #$14                   ;check vertical low byte of player position
           bcs X_Physics              ;if below a certain point, branch
           lda #$00                   ;otherwise reset player's vertical speed
           sta Player_Y_Speed         ;and jump to something else to keep player
           jmp X_Physics              ;from swimming above water level
PJumpSnd:
           lda #Sfx_BigJump           ;load big mario's jump sound by default
           ldy PlayerSize             ;is mario big?
           beq SJumpSnd
           lda #Sfx_SmallJump         ;if not, load small mario's jump sound
SJumpSnd:  sta Square1SoundQueue      ;store appropriate jump sound in square 1 sfx queue
X_Physics: ldy #$00
           sty R0                     ;init value here
           lda Player_State           ;if mario is on the ground, branch
           beq ProcPRun
           lda Player_XSpeedAbsolute  ;check something that seems to be related
           cmp #$19                   ;to mario's speed
           bcs GetXPhy                ;if =>$19 branch here
           bcc ChkRFast               ;if not branch elsewhere
ProcPRun:  iny                        ;if mario on the ground, increment Y
           lda AreaType               ;check area type
           beq ChkRFast               ;if water type, branch
           dey                        ;decrement Y by default for non-water type area
           lda Left_Right_Buttons     ;get left/right controller bits
           cmp Player_MovingDir       ;check against moving direction
           bne ChkRFast               ;if controller bits <> moving direction, skip this part
           lda A_B_Buttons            ;check for b button pressed
           and #B_Button
           bne SetRTmr                ;if pressed, skip ahead to set timer
           lda RunningTimer           ;check for running timer set
           bne GetXPhy                ;if set, branch
ChkRFast:  iny                        ;if running timer not set or level type is water,
           inc R0                     ;increment Y again and temp variable in memory
           lda RunningSpeed
           bne FastXSp                ;if running speed set here, branch
           lda Player_XSpeedAbsolute
           cmp #$21                   ;otherwise check player's walking/running speed
           bcc GetXPhy                ;if less than a certain amount, branch ahead
FastXSp:   inc R0                     ;if running speed set or speed => $21 increment $00
           jmp GetXPhy                ;and jump ahead
SetRTmr:   lda #$0a                   ;if b button pressed, set running timer
           sta RunningTimer
GetXPhy:   lda MaxLeftXSpdData,y      ;get maximum speed to the left
           sta MaximumLeftSpeed
           lda GameEngineSubroutine   ;check for specific routine running
           cmp #$07                   ;(player entrance)
           bne GetXPhy2               ;if not running, skip and use old value of Y
           ldy #$03                   ;otherwise set Y to 3
GetXPhy2:  lda MaxRightXSpdData,y     ;get maximum speed to the right
           sta MaximumRightSpeed
           ldy R0                     ;get other value in memory
           lda FrictionData,y         ;get value using value in memory as offset
           sta FrictionAdderLow
           lda #$00
           sta FrictionAdderHigh      ;init something here
           lda PlayerFacingDir
           cmp Player_MovingDir       ;check facing direction against moving direction
           beq ExitPhy                ;if the same, branch to leave
           asl FrictionAdderLow       ;otherwise multiply friction by 2
           rol FrictionAdderHigh      ;then leave
ExitPhy:   rts

;-------------------------------------------------------------------------------------
.reloc
PlayerAnimTmrData:
      .byte $02, $04, $07

GetPlayerAnimSpeed:
            ldy #$00                   ;initialize offset in Y
            lda Player_XSpeedAbsolute  ;check player's walking/running speed
            cmp #$1c                   ;against preset amount
            bcs SetRunSpd              ;if greater than a certain amount, branch ahead
            iny                        ;otherwise increment Y
            cmp #$0e                   ;compare against lower amount
            bcs ChkSkid                ;if greater than this but not greater than first, skip increment
            iny                        ;otherwise increment Y again
ChkSkid:    lda SavedJoypadBits        ;get controller bits
            and #%01111111             ;mask out A button
            beq SetAnimSpd             ;if no other buttons pressed, branch ahead of all this
            and #$03                   ;mask out all others except left and right
            cmp Player_MovingDir       ;check against moving direction
            bne ProcSkid               ;if left/right controller bits <> moving direction, branch
            lda #$00                   ;otherwise set zero value here
SetRunSpd:  sta RunningSpeed           ;store zero or running speed here
            jmp SetAnimSpd
ProcSkid:   lda Player_XSpeedAbsolute  ;check player's walking/running speed
            cmp #$0b                   ;against one last amount
            bcs SetAnimSpd             ;if greater than this amount, branch
            lda PlayerFacingDir
            sta Player_MovingDir       ;otherwise use facing direction to set moving direction
            lda #$00
            sta Player_X_Speed         ;nullify player's horizontal speed
            sta Player_X_MoveForce     ;and dummy variable for player
SetAnimSpd: lda PlayerAnimTmrData,y    ;get animation timer setting using Y as offset
            sta PlayerAnimTimerSet
            rts

;-------------------------------------------------------------------------------------
.reloc
ImposeFriction:
  and Player_CollisionBits  ;perform AND between left/right controller bits and collision flag
  ; cmp #$00                  ;then compare to zero (this instruction is redundant)
  bne JoypFrict             ;if any bits set, branch to next part
    lda Player_X_Speed
    beq SetAbsSpd             ;if player has no horizontal speed, branch ahead to last part
    bpl RghtFrict             ;if player moving to the right, branch to slow
    bmi LeftFrict             ;otherwise logic dictates player moving left, branch to slow
JoypFrict:
  lsr                       ;put right controller bit into carry
  bcc RghtFrict             ;if left button pressed, carry = 0, thus branch
LeftFrict:
    lda Player_X_MoveForce    ;load value set here
    clc
    adc FrictionAdderLow      ;add to it another value set here
    sta Player_X_MoveForce    ;store here
    lda Player_X_Speed
    adc FrictionAdderHigh     ;add value plus carry to horizontal speed
    sta Player_X_Speed        ;set as new horizontal speed
    cmp MaximumRightSpeed     ;compare against maximum value for right movement
    bmi XSpdSign              ;if horizontal speed greater negatively, branch
      lda MaximumRightSpeed     ;otherwise set preset value as horizontal speed
      sta Player_X_Speed        ;thus slowing the player's left movement down
      jmp SetAbsSpd             ;skip to the end
RghtFrict:
    lda Player_X_MoveForce    ;load value set here
    sec
    sbc FrictionAdderLow      ;subtract from it another value set here
    sta Player_X_MoveForce    ;store here
    lda Player_X_Speed
    sbc FrictionAdderHigh     ;subtract value plus borrow from horizontal speed
    sta Player_X_Speed        ;set as new horizontal speed
    cmp MaximumLeftSpeed      ;compare against maximum value for left movement
    bpl XSpdSign              ;if horizontal speed greater positively, branch
      lda MaximumLeftSpeed      ;otherwise set preset value as horizontal speed
      sta Player_X_Speed        ;thus slowing the player's right movement down
XSpdSign:
  cmp #$00                  ;if player not moving or moving to the right,
  bpl SetAbsSpd             ;branch and leave horizontal speed value unmodified
    eor #$ff
    clc                       ;otherwise get two's compliment to get absolute
    adc #$01                  ;unsigned walking/running speed
SetAbsSpd:
  sta Player_XSpeedAbsolute ;store walking/running speed here and leave
  rts

;-------------------------------------------------------------------------------------

ScrollHandler:
  ; lda Player_X_Scroll       ;load value saved here
  ; clc
  ; adc Platform_X_Scroll     ;add value used by left/right platforms
  ; sta Player_X_Scroll       ;save as new value here to impose force on scroll
  lda ScrollLock            ;check scroll lock flag
  bne InitScrlAmt           ;skip a bunch of code here if set
  lda Player_Pos_ForScroll
  cmp #$50                  ;check player's horizontal screen position
  bcc InitScrlAmt           ;if less than 80 pixels to the right, branch
  lda SideCollisionTimer    ;if timer related to player's side collision
  bne InitScrlAmt           ;not expired, branch
  ldy Player_X_Scroll       ;get value and decrement by one
  ; dey                       ;if value originally set to zero or otherwise
  ; bmi InitScrlAmt           ;negative for left movement, branch
  ; iny
  cpy #$02                  ;if value $01, branch and do not decrement
  bcc ChkNearMid
  dey                       ;otherwise decrement by one
ChkNearMid:
  lda Player_Pos_ForScroll
  cmp #$70                  ;check player's horizontal screen position
  bcc ScrollScreen          ;if less than 112 pixels to the right, branch
    ldy Player_X_Scroll       ;otherwise get original value undecremented
    ; fallthrough
ScrollScreen:
  tya
  sta ScrollAmount          ;save value here
  rts
  ; clc
  ; adc ScrollThirtyTwo       ;add to value already set here
  ; sta ScrollThirtyTwo       ;save as new value here
  ; tya
  ; clc
  ; adc ScreenLeft_X_Pos      ;add to left side coordinate
  ; sta ScreenLeft_X_Pos      ;save as new left side coordinate
  ; ; sta HorizontalScroll      ;save here also
  ; lda ScreenLeft_PageLoc
  ; adc #$00                  ;add carry to page location for left
  ; sta ScreenLeft_PageLoc    ;side of the screen
  ; and #$01                  ;get LSB of page location
  ; sta R0                    ;save as temp variable for PPU register 1 mirror
  ; ; lda Mirror_PPUCTRL       ;get PPU register 1 mirror
  ; ; and #%11111110            ;save all bits except d0
  ; ; ora R0                    ;get saved bit here and save in PPU register 1
  ; ; sta Mirror_PPUCTRL       ;mirror to be used to set name table later
  ; jsr GetScreenPosition     ;figure out where the right side is
  ; jmp ChkPOffscr            ;skip this part
InitScrlAmt:
  lda #$00
  sta ScrollAmount          ;initialize value here
; ChkPOffscr:
;   ldx #$00                  ;set X for player offset
;   jsr GetXOffscreenBits     ;get horizontal offscreen bits for player
;   sta R0                    ;save them here
;   ldy #$00                  ;load default offset (left side)
;   asl                       ;if d7 of offscreen bits are set,
;   bcs KeepOnscr             ;branch with default offset
;     iny                         ;otherwise use different offset (right side)
;     lda R0
;     and #%00100000              ;check offscreen bits for d5 set
;     beq InitPlatScrl            ;if not set, branch ahead of this part
; KeepOnscr:
;     lda ScreenEdge_X_Pos,y      ;get left or right side coordinate based on offset
;     sec
;     sbc X_SubtracterData,y      ;subtract amount based on offset
;     sta Player_X_Position       ;store as player position to prevent movement further
;     lda ScreenEdge_PageLoc,y    ;get left or right page location based on offset
;     sbc #$00                    ;subtract borrow
;     sta Player_PageLoc          ;save as player's page location
;     lda Left_Right_Buttons      ;check saved controller bits
;     cmp OffscrJoypadBitsData,y  ;against bits based on offset
;     beq InitPlatScrl            ;if not equal, branch
;       lda #$00
;       sta Player_X_Speed          ;otherwise nullify horizontal speed of player
; InitPlatScrl:
;   lda #$00                    ;nullify platform force imposed on scroll
;   sta Platform_X_Scroll
  rts

X_SubtracterData:
  .byte $00, $10

OffscrJoypadBitsData:
  .byte $01, $02

; ------------------------------------------------------------
.segment "PRG0", "PRG7"
.reloc
ProcessPlayerAction:
  lda Player_State      ;get player's state
  cmp #$03
  beq ActionClimbing    ;if climbing, branch here
  cmp #$02
  beq ActionFalling     ;if falling, branch here
  cmp #$01
  bne ProcOnGroundActs  ;if not jumping, branch here
  lda SwimmingFlag
  bne ActionSwimming    ;if swimming flag set, branch elsewhere
  ldy #$06              ;load offset for crouching
  lda CrouchingFlag     ;get crouching flag
  bne NonAnimatedActs   ;if set, branch to get offset for graphics table
  ldy #$00              ;otherwise load offset for jumping
  jmp NonAnimatedActs   ;go to get offset to graphics table

ProcOnGroundActs:
  ldy #$06                   ;load offset for crouching
  lda CrouchingFlag          ;get crouching flag
  bne NonAnimatedActs        ;if set, branch to get offset for graphics table
  ldy #$02                   ;load offset for standing
  lda Player_X_Speed         ;check player's horizontal speed
  ora Left_Right_Buttons     ;and left/right controller bits
  beq NonAnimatedActs        ;if no speed or buttons pressed, use standing offset
  lda Player_XSpeedAbsolute  ;load walking/running speed
  cmp #$09
  bcc ActionWalkRun          ;if less than a certain amount, branch, too slow to skid
  lda Player_MovingDir       ;otherwise check to see if moving direction
  and PlayerFacingDir        ;and facing direction are the same
  bne ActionWalkRun          ;if moving direction = facing direction, branch, don't skid
; .if ::USE_SMB2J_FEATURES
;     lda GameEngineSubroutine
;     cmp #$09                   ;if running the change size, fire flower, injure
;     bcs NoSkidS                ;or death game engine subroutines, skip this
;       ; Ported from SMB2j
;       lda #Sfx_Skid            ;otherwise play skid sound
;       sta NoiseSoundQueue
; NoSkidS:
; .endif
    iny                        ;otherwise increment to skid offset ($03)

NonAnimatedActs:
  jsr GetGfxOffsetAdder      ;do a sub here to get offset adder for graphics table
  lda #$00
  sta PlayerAnimCtrl         ;initialize animation frame control
; .if USE_LOOPING_ANIM_CYCLE
;   lda #1
;   sta PlayerAnimDirection
; .endif
  lda PlayerGfxTblOffsets,y  ;load offset to graphics table using size as offset
  rts

ActionFalling:
  ldy #$04                  ;load offset for walking/running
  jsr GetGfxOffsetAdder     ;get offset to graphics table
  jmp GetCurrentAnimOffset  ;execute instructions for falling state

ActionWalkRun:
  ldy #$04               ;load offset for walking/running
  jsr GetGfxOffsetAdder  ;get offset to graphics table
  jmp FourFrameExtent    ;execute instructions for normal state

ActionClimbing:
  ldy #$05               ;load offset for climbing
  lda Player_Y_Speed     ;check player's vertical speed
  beq NonAnimatedActs    ;if no speed, branch, use offset as-is
  jsr GetGfxOffsetAdder  ;otherwise get offset for graphics table
  jmp ThreeFrameExtent   ;then skip ahead to more code

ActionSwimming:
  ldy #$01               ;load offset for swimming
  jsr GetGfxOffsetAdder
  lda JumpSwimTimer      ;check jump/swim timer
  ora PlayerAnimCtrl     ;and animation frame control
  bne FourFrameExtent    ;if any one of these set, branch ahead
  lda A_B_Buttons
  asl                    ;check for A button pressed
  bcs FourFrameExtent    ;branch to same place if A button pressed

GetCurrentAnimOffset:
  lda PlayerAnimCtrl         ;get animation frame control
  jmp GetOffsetFromAnimCtrl  ;jump to get proper offset to graphics table

FourFrameExtent:
  lda #$03              ;load upper extent for frame control
  jmp AnimationControl  ;jump to get offset and animate player object

ThreeFrameExtent:
  lda #$02              ;load upper extent for frame control for climbing

AnimationControl:
  sta R0                    ;store upper extent here
  jsr GetCurrentAnimOffset  ;get proper offset to graphics table
  pha                       ;save offset to stack
    lda PlayerAnimTimer       ;load animation frame timer
    bne ExAnimC               ;branch if not expired
      lda PlayerAnimTimerSet    ;get animation frame timer amount
      sta PlayerAnimTimer       ;and set timer accordingly
; .if USE_LOOPING_ANIM_CYCLE
;       lda PlayerAnimDirection
;       bmi @NegativeAnimCycle
;         lda PlayerAnimCtrl
;         clc                       ;add one to animation frame control
;         adc PlayerAnimDirection
;         cmp R0                    ;compare to upper extent
;         bcc SetAnimC              ;if frame control + 1 < upper extent, use as next
;           lda #$ff
;           sta PlayerAnimDirection
;           lda R0                  ;otherwise initialize frame control
;           sec
;           sbc #2
;           jmp SetAnimC
; @NegativeAnimCycle:
;         lda PlayerAnimCtrl
;         clc                       ;add one to animation frame control
;         adc PlayerAnimDirection
;         bpl SetAnimC              ;if frame control + 1 < upper extent, use as next
;           lda #1
;           sta PlayerAnimDirection
;           ; lda #$00                  ;otherwise initialize frame control
;           ; jmp SetAnimC
; .else
      lda PlayerAnimCtrl
      clc                       ;add one to animation frame control
      adc #$01
      cmp R0                    ;compare to upper extent
      bcc SetAnimC              ;if frame control + 1 < upper extent, use as next
        lda #$00                  ;otherwise initialize frame control
; .endif
SetAnimC:
    sta PlayerAnimCtrl        ;store as new animation frame control
ExAnimC:
  pla                       ;get offset to graphics table from stack and leave
  rts

GetGfxOffsetAdder:
  lda PlayerSize  ;get player's size
  beq SzOfs       ;if player big, use current offset as-is
  tya             ;for big player
  clc             ;otherwise add eight bytes to offset
  adc #$08        ;for small player
  tay
SzOfs:
  rts             ;go back

; ChkForPlayerAttrib:
;   ldy PlayerOAMOffset         ;get sprite data offset
;   lda GameEngineSubroutine
;   cmp #$0b                    ;if executing specific game engine routine,
;   beq KilledAtt               ;branch to change third and fourth row OAM attributes
;   lda PlayerGfxOffset         ;get graphics table offset
;   cmp #$50
;   beq C_S_IGAtt               ;if crouch offset, either standing offset,
;   cmp #$b8                    ;or intermediate growing offset,
;   beq C_S_IGAtt               ;go ahead and execute code to change
;   cmp #$c0                    ;fourth row OAM attributes only
;   beq C_S_IGAtt
;   cmp #$c8
;   bne ExPlyrAt                ;if none of these, branch to leave
; KilledAtt:
;   lda Sprite_Attributes+16,y
;   and #%00111111              ;mask out horizontal and vertical flip bits
;   sta Sprite_Attributes+16,y  ;for third row sprites and save
;   lda Sprite_Attributes+20,y
;   and #%00111111
;   ora #%01000000              ;set horizontal flip bit for second
;   sta Sprite_Attributes+20,y  ;sprite in the third row
; C_S_IGAtt:
;   lda Sprite_Attributes+24,y
;   and #%00111111              ;mask out horizontal and vertical flip bits
;   sta Sprite_Attributes+24,y  ;for fourth row sprites and save
;   lda Sprite_Attributes+28,y
;   and #%00111111
;   ora #%01000000              ;set horizontal flip bit for second
;   sta Sprite_Attributes+28,y  ;sprite in the fourth row
ExPlyrAt:
  rts                         ;leave

.segment "PRG0"
;-------------------------------------------------------------------------------------
;$00 - used for downward force
;$01 - used for upward force
;$02 - used for maximum vertical speed
.reloc
MovePlayerVertically:
  ldx #$00                ;set X for player offset
  lda TimerControl
  bne NoJSChk             ;if master timer control set, branch ahead
  lda JumpspringAnimCtrl  ;otherwise check to see if jumpspring is animating
  bne ExPlayerAttr             ;branch to leave if so
NoJSChk:
  lda VerticalForce       ;dump vertical force
  sta R0
  lda #$04                ;set maximum vertical speed here
  jmp ImposeGravitySprObj ;then jump to move player vertically


GetPlayerColors:
;  lda $076F ; Spell Status
;  and #$10  ; check for Fire Spell (bit 4)
;  beq +
;
;  +

  ; ldx VRAM_Buffer1_Offset  ;get current buffer offset
  ; ldy #$00
;   lda CurrentPlayer        ;check which player is on the screen
;   beq ChkFiery
;   ldy #$04                 ;load offset for luigi
; ChkFiery:

; TODO: reimplement fire check to use spell fire
;   lda PlayerStatus         ;check player status
;   cmp #$02
;   bne StartClrGet          ;if fiery, load alternate offset for fiery player
;   ldy #$08
; StartClrGet:
  ; lda #$03                 ;do four colors
  ; sta R0
ClrGetLoop:
;   lda PlayerColors,y       ;fetch player colors and store them
;   sta VRAM_Buffer1+3,x     ;in the buffer
;   iny
;   inx
;   dec R0
;   bpl ClrGetLoop
;   ldx VRAM_Buffer1_Offset  ;load original offset from before
; ;   ldy BackgroundColorCtrl  ;if this value is four or greater, it will be set
; ;   bne SetBGColor           ;therefore use it as offset to background color
; ;   ldy AreaType             ;otherwise use area type bits from area offset as offset
; ; SetBGColor:
;   ldy #0 ; TODO fix bg color write
;   lda BackgroundColors,y   ;to background color instead
;   sta VRAM_Buffer1+3,x
;   lda #$3f                 ;set for sprite palette address
;   sta VRAM_Buffer1,x       ;save to buffer
;   lda #$10
;   sta VRAM_Buffer1+1,x
;   lda #$04                 ;write length byte to buffer
;   sta VRAM_Buffer1+2,x
;   ; lda #$00                 ;now the null terminator
;   lda #$ff ; z2 terminator
;   sta VRAM_Buffer1+7,x
;   txa                      ;move the buffer pointer ahead 7 bytes
;   clc                      ;in case we want to write anything else later
;   adc #$07
; SetVRAMOffset:
;   sta VRAM_Buffer1_Offset  ;store as new vram buffer offset
ExPlayerAttr:
  rts

; BGColorCtrl_Addr:
;       .byte $00, $09, $0a, $04

; BackgroundColors:
;       .byte $22, $22, $0f, $0f ;used by area type if bg color ctrl not set
;       .byte $0f, $22, $0f, $0f ;used by background color control if set

; PlayerColors:
;       .byte $22, $16, $27, $18 ;mario's colors
;       .byte $22, $30, $27, $19 ;luigi's colors
;       .byte $22, $37, $27, $16 ;fiery (used by both)

.export ImposeGravitySprObj
ImposeGravitySprObj:
      sta R2             ;set maximum speed here
      lda #$00           ;set value to move downwards
      ; jmp ImposeGravity  ;jump to the code that actually moves it
.proc ImposeGravity

  pha                          ;push value to stack
    lda Player_YMoveForceFractional,x
    clc                          ;add value in movement force to contents of dummy variable
    adc SprObject_Y_MoveForce,x
    sta Player_YMoveForceFractional,x
    ldy #$00                     ;set Y to zero by default
    lda SprObject_Y_Speed,x      ;get current vertical speed
    bpl AlterYP                  ;if currently moving downwards, do not decrement Y
    dey                          ;otherwise decrement Y
AlterYP:
    sty R7                       ;store Y here
    adc SprObject_Y_Position,x   ;add vertical position to vertical speed plus carry
    sta SprObject_Y_Position,x   ;store as new vertical position
    lda SprObject_Y_HighPos,x
    adc R7                       ;add carry plus contents of $07 to vertical high byte
    sta SprObject_Y_HighPos,x    ;store as new vertical high byte
    lda SprObject_Y_MoveForce,x
    clc
    adc R0                       ;add downward movement amount to contents of $0433
    sta SprObject_Y_MoveForce,x
    lda SprObject_Y_Speed,x      ;add carry to vertical speed and store
    adc #$00
    sta SprObject_Y_Speed,x
    cmp R2                       ;compare to maximum speed
    bmi ChkUpM                   ;if less than preset value, skip this part
    lda SprObject_Y_MoveForce,x
    cmp #$80                     ;if less positively than preset maximum, skip this part
    bcc ChkUpM
    lda R2
    sta SprObject_Y_Speed,x      ;keep vertical speed within maximum value
    lda #$00
    sta SprObject_Y_MoveForce,x  ;clear fractional
ChkUpM:
  pla                          ;get value from stack
  beq ExVMove                  ;if set to zero, branch to leave
  lda R2
  eor #%11111111               ;otherwise get two's compliment of maximum speed
  tay
  iny
  sty R7                       ;store two's compliment here
  lda SprObject_Y_MoveForce,x
  sec                          ;subtract upward movement amount from contents
  sbc R1                       ;of movement force, note that $01 is twice as large as $00,
  sta SprObject_Y_MoveForce,x  ;thus it effectively undoes add we did earlier
  lda SprObject_Y_Speed,x
  sbc #$00                     ;subtract borrow from vertical speed and store
  sta SprObject_Y_Speed,x
  cmp R7                       ;compare vertical speed to two's compliment
  bpl ExVMove                  ;if less negatively than preset maximum, skip this part
  lda SprObject_Y_MoveForce,x
  cmp #$80                     ;check if fractional part is above certain amount,
  bcs ExVMove                  ;and if so, branch to leave
  lda R7
  sta SprObject_Y_Speed,x      ;keep vertical speed within maximum value
  lda #$ff
  sta SprObject_Y_MoveForce,x  ;clear fractional
ExVMove:
  rts                          ;leave!
.endproc



;-------------------------------------------------------------------------------------
;$02 - modified y coordinate
;$03 - stores metatile involved in block buffer collisions
;$04 - comes in with offset to block buffer adder data, goes out with low nybble x/y coordinate
;$05 - modified x coordinate
;$06-$07 - block buffer address
.reloc
; BlockBufferChk_FBall:
;   ldy #$1a                  ;set offset for block buffer adder data
;   txa
;   clc
;   adc #$07                  ;add seven bytes to use
;   tax
;   lda #$00                  ;set A to return vertical coordinate
; BBChk_E:
;   jsr BlockBufferCollision  ;do collision detection subroutine for sprite object
;   ; ldx ObjectOffset          ;get object offset
;   cmp #$00                  ;check to see if object bumped into anything
;   rts

BlockBufferAdderData:
  .byte (BlockBuffer_Big_X_Adder      - BlockBuffer_X_Adder)
  .byte (BlockBuffer_Swimming_X_Adder - BlockBuffer_X_Adder)
  .byte (BlockBuffer_Small_X_Adder    - BlockBuffer_X_Adder)

; misc objects use hardcoded offsets
MISC_BLOCK_BUFFER_START = $16

; BLOCK_BUFFER_ADDER_NECK_OFFSET = BlockBufferNeck_X_Adder - BlockBuffer_X_Adder

BlockBuffer_X_Adder:
; Added to the sprite position to get the location to check for tile collision
;     head, foot l, r, side 1 2    3,   4
BlockBuffer_Big_X_Adder:
  .byte $08+8, $03+8, $0c+8, $02+8, $02+8, $0d+8, $0d+8 ; big
BlockBuffer_Swimming_X_Adder:
  .byte $08+8, $03+8, $0c+8, $02+8, $02+8, $0d+8, $0d+8 ; swimming
BlockBuffer_Small_X_Adder:
  .byte $08+8, $03+8, $0c+8, $02+8, $02+8, $0d+8, $0d+8 ; small/crouching
BlockBuffer_Misc_X_Adder:
  .byte $08+8, $00+8, $10+8, $04+8, $14+8, $04+8, $04+8 ; misc

BlockBuffer_Y_Adder:
  .byte $04, $20, $20, $08, $18, $08, $18 ; big
  .byte $02, $20, $20, $08, $18, $08, $18 ; swimming
  .byte $12, $20, $20, $18, $18, $18, $18 ; small/crouching
  .byte $18, $14, $14, $06, $06, $08, $10 ; misc

.export BlockBufferCollision
BlockBufferColli_Feet:
  iny            ;if branched here, increment to next set of adders
BlockBufferColli_Head:
  lda #$00       ;set flag to return vertical coordinate
  beq BlockBufferPlayerCollision ; Unconditional
BlockBufferColli_Side:
  lda #$01       ;set flag to return horizontal coordinate
BlockBufferPlayerCollision:
  ldx #$00       ;set offset for player object
BlockBufferCollision:
  pha                         ;save contents of A to stack
    sty R4                      ;save contents of Y here
    lda BlockBuffer_X_Adder,y   ;add horizontal coordinate
    clc                         ;of object to value obtained using Y as offset
    adc SprObject_X_Position,x
    ; extended to support 4 screens
    php
    lsr
    sta R5                      ;store here
    plp
    lda SprObject_PageLoc,x
    adc #$00                    ;add carry to page location
    cmp #4 ; check if the number is either 4 or negative
    bcs FailedToGetBlock
    ; and #$01                    ;get LSB, mask out all other bits
    and #3
    lsr                         ;move to carry
    ror
    ora R5                      ;get stored value
    ror                         ;rotate carry to MSB of A
    ; lsr                         ;and effectively move high nybble to
    lsr                         ;lower, LSB which became MSB will be
    lsr                         ;d4 at this point
    jsr GetBlockBufferAddr      ;get address of block buffer into $06, $07
    ldy R4                      ;get old contents of Y
    lda SprObject_Y_Position,x  ;get vertical coordinate of object
    clc
    adc BlockBuffer_Y_Adder,y   ;add it to value obtained using Y as offset
    and #%11110000              ;mask out low nybble
    sec
    sbc #$20                    ;subtract 32 pixels for the status bar
    sta R2                      ;store result here
    tay                         ;use as offset for block buffer
    lda (R6),y                  ;check current content of block buffer
    sta R3                      ;and store here
    ldy R4                      ;get old contents of Y again
  pla                         ;pull A from stack
  bne RetXC                   ;if A = 1, branch
  lda SprObject_Y_Position,x  ;if A = 0, load vertical coordinate
  jmp RetYC                   ;and jump
RetXC:
  lda SprObject_X_Position,x  ;otherwise load horizontal coordinate
RetYC:
  and #%00001111              ;and mask out high nybble
  sta R4                      ;store masked out result here
  lda R3                      ;get saved content of block buffer
  rts                         ;and leave
FailedToGetBlock:
  ; If we are leaving the map, then treat the offscreen blocks as no collision
  pla
  lda #0
  sta R3
  sta R4
  rts

; GetBlockBufferAddr:
;   lda $4d ; load link mario's lo x byte
;   sta $06
;   lda $3b ; load link mario's hi y byte
;   tay
;   lda $06 ; now divide by 4
;   lsr
;   lsr
;   lsr
;   lsr
;   clc
;   adc CollisionBufferAddrLo,y ; add it to get the actual Address of the block
;   sta $06
;   lda CollisionBufferAddrHi,y
;   sta $07
;   rts
GetBlockBufferAddr:
  pha                      ;take value of A, save
    lsr                      ;move high nybble to low
    lsr
    lsr
    lsr
    tay                      ;use nybble as pointer to high byte
    lda CollisionBufferAddrHi,y  ;of indirect here
    sta R7
  pla
  and #%00001111           ;pull from stack, mask out high nybble
  clc
  adc CollisionBufferAddrLo,y    ;add to low byte
  sta R6                   ;store here and leave
  rts

CollisionBufferAddrLo:
.byte <(COLLISION_TILES+$d0*0), <(COLLISION_TILES+$d0*1), <(COLLISION_TILES+$d0*2), <(COLLISION_TILES+$d0*3)
CollisionBufferAddrHi:
.byte >(COLLISION_TILES+$d0*0), >(COLLISION_TILES+$d0*1), >(COLLISION_TILES+$d0*2), >(COLLISION_TILES+$d0*3)
;-------------------------------------------------------------------------------------
;$06-$07 - used to store block buffer address used as indirect

; BlockBufferAddr:
; .byte <Block_Buffer_1, <Block_Buffer_2
; .byte >Block_Buffer_1, >Block_Buffer_2

; GetBlockBufferAddr:
;   pha                      ;take value of A, save
;     lsr                      ;move high nybble to low
;     lsr
;     lsr
;     lsr
;     tay                      ;use nybble as pointer to high byte
;     lda BlockBufferAddr+2,y  ;of indirect here
;     sta R7
;   pla
;   and #%00001111           ;pull from stack, mask out high nybble
;   clc
;   adc BlockBufferAddr,y    ;add to low byte
;   sta R6                   ;store here and leave
;   rts


;-------------------------------------------------------------------------------------

; FireballBGCollision:
;       lda Fireball_Y_Position,x   ;check fireball's vertical coordinate
;       cmp #$18
;       bcc ClearBounceFlag         ;if within the status bar area of the screen, branch ahead
;       jsr BlockBufferChk_FBall    ;do fireball to background collision detection on bottom of it
;       beq ClearBounceFlag         ;if nothing underneath fireball, branch
;       jsr ChkForNonSolids         ;check for non-solid metatiles
;       beq ClearBounceFlag         ;branch if any found
;       lda Fireball_Y_Speed,x      ;if fireball's vertical speed set to move upwards,
;       bmi InitFireballExplode     ;branch to set exploding bit in fireball's state
;       lda FireballBouncingFlag,x  ;if bouncing flag already set,
;       bne InitFireballExplode     ;branch to set exploding bit in fireball's state
;       lda #$fd
;       sta Fireball_Y_Speed,x      ;otherwise set vertical speed to move upwards (give it bounce)
;       lda #$01
;       sta FireballBouncingFlag,x  ;set bouncing flag
;       lda Fireball_Y_Position,x
;       and #$f8                    ;modify vertical coordinate to land it properly
;       sta Fireball_Y_Position,x   ;store as new vertical coordinate
;       rts                         ;leave

; ClearBounceFlag:
;       lda #$00
;       sta FireballBouncingFlag,x  ;clear bouncing flag by default
;       rts                         ;leave

; InitFireballExplode:
;       lda #$80
;       sta Fireball_State,x        ;set exploding flag in fireball's state
;       lda #Sfx_Bump
;       sta Square1SoundQueue       ;load bump sound
;       rts                         ;leave

ChkForNonSolids:
       cmp #$26       ;blank metatile used for vines?
       beq NSFnd
       cmp #$c2       ;regular coin?
       beq NSFnd
       cmp #$c3       ;underwater coin?
       beq NSFnd
       cmp #$5f       ;hidden coin block?
       beq NSFnd
       cmp #$60       ;hidden 1-up block?
NSFnd: rts


;-------------------------------------------------------------------------------------
;$00-$01 - used to hold many values, essentially temp variables
;$04 - holds lower nybble of vertical coordinate from block buffer routine
;$eb - used to hold block buffer adder
.reloc
PlayerBGUpperExtent:
  .byte $20, $10

PlayerBGCollision:
  lda DisableCollisionDet   ;if collision detection disabled flag set,
  bne ExPBGCol              ;branch to leave
  ; clear the player metatile we are standing on
  sta PlayerStandingMetatile
  lda GameEngineSubroutine
  cmp #$0b                  ;if running routine #11 or $0b
  beq ExPBGCol              ;branch to leave
  cmp #$04
  bcc ExPBGCol              ;if running routines $00-$03 branch to leave
  lda #$01                  ;load default player state for swimming
  ldy SwimmingFlag          ;if swimming flag set,
  bne SetPSte               ;branch ahead to set default state
  lda Player_State          ;if player in normal state,
  beq SetFallS              ;branch to set default state for falling
  cmp #$03
  bne ChkOnScr              ;if in any other state besides climbing, skip to next part
SetFallS:
  lda #$02                  ;load default player state for falling
SetPSte:

  ldy $0754 ; zelda check for elevator state
  bne ChkOnScr
  sta Player_State          ;set whatever player state is appropriate
ChkOnScr:
  lda Player_Y_HighPos
  cmp #$01                  ;check player's vertical high byte for still on the screen
  bne ExPBGCol              ;branch to leave if not
  lda #$ff
  sta Player_CollisionBits  ;initialize player's collision flag
  lda Player_Y_Position
  cmp #$cf                  ;check player's vertical coordinate
  bcc ChkCollSize           ;if not too close to the bottom of screen, continue
ExPBGCol:
  rts                       ;otherwise leave

ChkCollSize:
  ldy #$02                    ;load default offset
  lda CrouchingFlag
  bne GBBAdr                  ;if player crouching, skip ahead
    lda PlayerSize
    bne GBBAdr                  ;if player small, skip ahead
      dey                         ;otherwise decrement offset for big player not crouching
      lda SwimmingFlag
      bne GBBAdr                  ;if swimming flag set, skip ahead
        dey                         ;otherwise decrement offset
GBBAdr:
  lda BlockBufferAdderData,y  ;get value using offset
  sta Local_eb                     ;store value here
  tay                         ;put value into Y, as offset for block buffer routine
  ldx PlayerSize              ;get player's size as offset
  lda CrouchingFlag
  beq HeadChk                 ;if player not crouching, branch ahead
    inx                         ;otherwise increment size as offset
HeadChk:
  lda Player_Y_Position       ;get player's vertical coordinate
  cmp PlayerBGUpperExtent,x   ;compare with upper extent value based on offset
  bcc DoFootCheck             ;if player is too high, skip this part
    jsr BlockBufferColli_Head   ;do player-to-bg collision detection on top of
    beq DoFootCheck             ;player, and branch if nothing above player's head
      ; jsr CheckForCoinMTiles      ;check to see if player touched coin with their head
      ; bcs AwardTouchedCoin        ;if so, branch to some other part of code
        ; Z2 - also check if we are on an elevator
        pha
          lda $754 ; Player on elevator flag == $10 if on elevator
          eor #$ff ; so invert it to make us have "upwards" speed if on ele
          ora Player_Y_Speed          ;check player's vertical speed
          tay
        pla
        cpy #0
        bpl DoFootCheck             ;if player not moving upwards, branch elsewhere
        ldy R4                      ;check lower nybble of vertical coordinate returned
        cpy #$04                    ;from collision detection routine
        bcc DoFootCheck             ;if low nybble < 4, branch
          ; we can't check solid type here since its banked out
          ; jsr CheckForSolidMTiles     ;check to see what player's head bumped on
          bcs SolidOrClimb            ;if player collided with solid metatile, branch
          ldy AreaType                ;otherwise check area type
          beq NYSpd                   ;if water level, branch ahead
          ldy BlockBounceTimer        ;if block bounce timer not expired,
          bne NYSpd                   ;branch ahead, do not process collision
          jsr PlayerHeadCollision     ;otherwise do a sub to process collision
          jmp DoFootCheck             ;jump ahead to skip these other parts

SolidOrClimb:
  ; cmp #$26               ;if climbing metatile,
  ; beq NYSpd              ;branch ahead and do not play sound
  ; Z2 Set Player_CollisionBits to fix elevator
;  lda #~$08
;  and Player_CollisionBits
;  sta Player_CollisionBits
  ; Z2 - don't set bump sound if on an elevator
  lda $754
  bne NYSpd
    lda #Sfx_Bump
    sta Square1SoundQueue  ;otherwise load bump sound
NYSpd:
  lda #$01               ;set player's vertical speed to nullify
  sta Player_Y_Speed     ;jump or swim

DoFootCheck:
  ldy Local_eb                    ;get block buffer adder offset
  lda Player_Y_Position
  cmp #$cf                   ;check to see how low player is
  bcs DoPlayerSideCheck      ;if player is too far down on screen, skip all of this
    jsr BlockBufferColli_Feet  ;do player-to-bg collision detection on bottom left of player
    ; jsr CheckForCoinMTiles     ;check to see if player touched coin with their left foot
    ; bcs AwardTouchedCoin       ;if so, branch to some other part of code
      pha                        ;save bottom left metatile to stack
        jsr BlockBufferColli_Feet  ;do player-to-bg collision detection on bottom right of player
        sta R0                     ;save bottom right metatile here
      pla
      sta R1                     ;pull bottom left metatile and save here
      bne ChkFootMTile           ;if anything here, skip this part
        lda R0                     ;otherwise check for anything in bottom right metatile
        beq DoPlayerSideCheck      ;and skip ahead if not
          ; jsr CheckForCoinMTiles     ;check to see if player touched coin with their right foot
          ; bcc ChkFootMTile           ;if not, skip unconditional jump and continue code
AwardTouchedCoin:
  ; jmp HandleCoinMetatile     ;follow the code to erase coin and award to player 1 coin
  ;implicit rts

ChkFootMTile:
  ; jsr CheckForClimbMTiles    ;check to see if player landed on climbable metatiles
  ; bcs DoPlayerSideCheck      ;if so, branch
    ldy Player_Y_Speed         ;check player's vertical speed
    bmi DoPlayerSideCheck      ;if player moving upwards, branch
  ;  cmp #$c5
  ;  bne ContChk                ;if player did not touch axe, skip ahead
      ; jmp HandleAxeMetatile      ;otherwise jump to set modes of operation
ContChk:
  ; jsr ChkInvisibleMTiles     ;do sub to check for hidden coin or 1-up blocks
  ; beq DoPlayerSideCheck      ;if either found, branch
  ;   ldy JumpspringAnimCtrl     ;if jumpspring animating right now,
  ;   bne InitSteP               ;branch ahead
      ldy R4                     ;check lower nybble of vertical coordinate returned
      cpy #$05                   ;from collision detection routine
      bcc LandPlyr               ;if lower nybble < 5, branch
        lda Player_MovingDir
        sta R0                     ;use player's moving direction as temp variable
        jmp StopPlayerMove         ;jump to impede player's movement in that direction
LandPlyr:
  ; jsr ChkForLandJumpSpring   ;do sub to check for jumpspring metatiles and deal with it
  lda #$f0
  and Player_Y_Position      ;mask out lower nybble of player's vertical position
  sta Player_Y_Position      ;and store as new vertical position to land player properly
  ; jsr HandlePipeEntry        ;do sub to process potential pipe entry
  lda #$00
  sta Player_Y_Speed         ;initialize vertical speed and fractional
  sta Player_Y_MoveForce     ;movement force to stop player's vertical movement

  ; sta StompChainCounter      ;initialize enemy stomp counter
; cancel downstab hitbox??
  lda #$f8
  sta HitboxYCoord
  lda $cc
  clc
  adc #$10
  sta HitboxXCoord
InitSteP:
  lda #$00
  sta Player_State           ;set player's state to normal
  ; fallthrough

DoPlayerSideCheck:
  ldy Local_eb       ;get block buffer adder offset
  iny
  iny           ;increment offset 2 bytes to use adders for side collisions
  lda #2
  sta R0
SideCheckLoop:
  iny                       ;move onto the next one
  sty Local_eb                   ;store it
  lda Player_Y_Position
  cmp #$20                  ;check player's vertical position
  bcc BHalf                 ;if player is in status bar area, branch ahead to skip this part
  cmp #$e4
  bcs ExSCH                 ;branch to leave if player is too far down
  jsr BlockBufferColli_Side ;do player-to-bg collision detection on one half of player
  bne CheckSideMTiles       ;branch if found something
  ; beq BHalf                 ;branch ahead if nothing found
  ; cmp #$1c                  ;otherwise check for pipe metatiles
  ; beq BHalf                 ;if collided with sideways pipe (top), branch ahead
  ; cmp #$6b
  ; beq BHalf                 ;if collided with water pipe (top), branch ahead
    ; jsr CheckForClimbMTiles   ;do sub to see if player bumped into anything climbable
    ; bcc CheckSideMTiles       ;if not, branch to alternate section of code
BHalf:
  ldy Local_eb                   ;load block adder offset
  iny                       ;increment it
  lda Player_Y_Position     ;get player's vertical position
  cmp #$08
  bcc ExSCH                 ;if too high, branch to leave
  cmp #$d0
  bcs ExSCH                 ;if too low, branch to leave
    jsr BlockBufferColli_Side ;do player-to-bg collision detection on other half of player
    bne CheckSideMTiles       ;if something found, branch
      dec R0                    ;otherwise decrement counter
      bne SideCheckLoop         ;run code until both sides of player are checked
ExSCH:
  rts                       ;leave

CheckSideMTiles:
  ; for now just always block movement
  jmp StopPlayerMove

  ; jsr ChkInvisibleMTiles     ;check for hidden or coin 1-up blocks
  ; beq ExSCH                  ;branch to leave if either found
    ; jsr CheckForClimbMTiles    ;check for climbable metatiles
    ; bcc ContSChk               ;if not found, skip and continue with code
      ; jmp HandleClimbing         ;otherwise jump to handle climbing
ContSChk:
  ; jsr CheckForCoinMTiles     ;check to see if player touched coin
  ; bcs HandleCoinMetatile     ;if so, execute code to erase coin and award to player 1 coin
    ; jsr ChkJumpspringMetatiles ;check for jumpspring metatiles
    ; bcc ChkPBtm                ;if not found, branch ahead to continue cude
    ;   lda JumpspringAnimCtrl     ;otherwise check jumpspring animation control
    ;   bne ExSCH                  ;branch to leave if set
    ;     jmp StopPlayerMove         ;otherwise jump to impede player's movement
ChkPBtm:
  ; ldy Player_State           ;get player's state
  ; cpy #$00                   ;check for player's state set to normal
  ; bne StopPlayerMove         ;if not, branch to impede player's movement
  ;   ldy PlayerFacingDir        ;get player's facing direction
  ;   dey
  ;   bne StopPlayerMove         ;if facing left, branch to impede movement
;     cmp #$6c                   ;otherwise check for pipe metatiles
;     beq PipeDwnS               ;if collided with sideways pipe (bottom), branch
;     cmp #$1f                   ;if collided with water pipe (bottom), continue
;     bne StopPlayerMove         ;otherwise branch to impede player's movement
; PipeDwnS:
  ; lda Player_SprAttrib       ;check player's attributes
  ; bne PlyrPipe               ;if already set, branch, do not play sound again
  ; lda InPipeTransition
  ; bne :+
    ; ldy #Sfx_PipeDown_Injury
    ; sty Square1SoundQueue      ;otherwise load pipedown/injury sound
    ; lda PlayerEntranceCtrl  ;;; check if we are entering the pipe in auto mode
    ; cmp #7
    ; bne :+
      ;; if we are then start a pipe transition
      ; .import SetupPipeTransitionOverlay
      ; lda #2
      ; jsr SetupPipeTransitionOverlay
  ; :
; PlyrPipe:
;   ora #%00100000
;   sta Player_SprAttrib       ;set background priority bit in player attributes
;   lda Player_X_Position
;   and #%00001111             ;get lower nybble of player's horizontal coordinate
;   beq ChkGERtn               ;if at zero, branch ahead to skip this part
;     ldy #$00                   ;set default offset for timer setting data
;     lda ScreenLeft_PageLoc     ;load page location for left side of screen
;     beq SetCATmr               ;if at page zero, use default offset
;       iny                        ;otherwise increment offset
; SetCATmr:
;   lda AreaChangeTimerData,y  ;set timer for change of area as appropriate
;   sta ChangeAreaTimer
; ChkGERtn:
;   lda GameEngineSubroutine   ;get number of game engine routine running
;   cmp #$07
;   beq ExCSM                  ;if running player entrance routine or
;   cmp #$08                   ;player control routine, go ahead and branch to leave
;   bne ExCSM
;   lda #$02
;   sta GameEngineSubroutine   ;otherwise set sideways pipe entry routine to run
ExCSM:
  rts                        ;and leave

;--------------------------------
;$02 - high nybble of vertical coordinate from block buffer
;$04 - low nybble of horizontal coordinate from block buffer
;$06-$07 - block buffer address

StopPlayerMove:
  jmp ImpedePlayerMove      ;stop player's movement

.export ImpedePlayerMove
.reloc
ImpedePlayerMove:
  lda #$00                  ;initialize value here
  ldy Player_X_Speed        ;get player's horizontal speed
  ldx R0                    ;check value set earlier for
  dex                       ;left side collision
  bne RImpd                 ;if right side collision, skip this part
  inx                       ;return value to X
  cpy #$00                  ;if player moving to the left,
  bmi ExIPM                 ;branch to invert bit and leave
      lda #$ff                  ;otherwise load A with value to be used later
    bne NXSpd                 ;and jump to affect movement
RImpd:
    ldx #$02                  ;return $02 to X
    cpy #$01                  ;if player moving to the right,
    bpl ExIPM                 ;branch to invert bit and leave
    lda #$01                  ;otherwise load A with value to be used here
NXSpd:
    ldy #$10
    sty SideCollisionTimer    ;set timer of some sort
    ldy #$00
    sty Player_X_Speed        ;nullify player's horizontal speed
    cmp #$00                  ;if value set in A not set to $ff,
    bpl PlatF                 ;branch ahead, do not decrement Y
      dey                       ;otherwise decrement Y now
PlatF:
    sty R0                   ;store Y as high bits of horizontal adder
    clc
    adc Player_X_Position     ;add contents of A to player's horizontal
    sta Player_X_Position     ;position to move player left or right
    lda Player_PageLoc
    adc R0                   ;add high bits and carry to
    sta Player_PageLoc        ;page location if necessary
ExIPM:
  txa                       ;invert contents of X
  eor #$ff
  and Player_CollisionBits  ;mask out bit that was set here
  sta Player_CollisionBits  ;store to clear bit
  rts


;-------------------------------------------------------------------------------------
;These apply to all routines in this section unless otherwise noted:
;$00 - used to store metatile from block buffer routine
;$02 - used to store vertical high nybble offset from block buffer routine
;$05 - used to store metatile stored in A at beginning of PlayerHeadCollision
;$06-$07 - used as block buffer address indirect
.reloc
BlockYPosAdderData:
;     Big, Small
  .byte $04, $12

PlayerHeadCollision:
  ; pha                      ;store metatile number to stack
;     lda #$11                 ;load unbreakable block object state by default
;     ldx SprDataOffset_Ctrl   ;load offset control bit here
;     ldy PlayerSize           ;check player's size
;     bne :+            ;if small, branch
;       lda #$12                 ;otherwise load breakable block object state
; :
;     sta Block_State,x        ;store into block object buffer
    ; farcall DestroyBlockMetatile ;store blank metatile in vram buffer to write to name table
    ; ldx SprDataOffset_Ctrl   ;load offset control bit
    ; lda R2                   ;get vertical high nybble offset used in block buffer routine
    ; sta Block_Orig_YPos,x    ;set as vertical coordinate for block object
    ; tay
    ; lda R6                   ;get low byte of block buffer address used in same routine
    ; sta Block_BBuf_Low,x     ;save as offset here to be used later
    ; lda (R6),y              ;get contents of block buffer at old address at $06, $07
    ; jsr BlockBumpedChk       ;do a sub to check which block player bumped head on
    ; sta R0                   ;store metatile here
;     ldy PlayerSize           ;check player's size
;     bne :+             ;if small, use metatile itself as contents of A
;       tya                      ;otherwise init A (note: big = 0)
; :
;     bcc PutMTileB            ;if no match was found in previous sub, skip ahead
;       ; ldy #$11                 ;otherwise load unbreakable state into block object buffer
;       ; sty Block_State,x        ;note this applies to both player sizes
;       lda #$c4                 ;load empty block metatile into A for now
;       ldy R0                   ;get metatile from before
;       cpy #$58                 ;is it brick with coins (with line)?
;       beq StartBTmr            ;if so, branch
;         cpy #$5d                 ;is it brick with coins (without line)?
;         bne PutMTileB            ;if not, branch ahead to store empty block metatile
; StartBTmr:
;       lda BrickCoinTimerFlag   ;check brick coin timer flag
;       bne :+             ;if set, timer expired or counting down, thus branch
;         lda #$0b
;         sta BrickCoinTimer       ;if not set, set brick coin timer
;         inc BrickCoinTimerFlag   ;and set flag linked to it
; :
;       lda BrickCoinTimer       ;check brick coin timer
;       bne :+             ;if not yet expired, branch to use current metatile
;         ldy #$c4                 ;otherwise use empty block metatile
; :
;       tya                      ;put metatile into A
; PutMTileB:
;     sta Block_Metatile,x     ;store whatever metatile be appropriate here
;     jsr InitBlock_XY_Pos     ;get block object horizontal coordinates saved
;     ldy R2                   ;get vertical high nybble offset
;     lda #$23
;     sta (R6),y               ;write blank metatile $23 to block buffer
;     lda #$10
;     sta BlockBounceTimer     ;set block bounce timer
;   pla                      ;pull original metatile from stack
;   sta R5                   ;and save here
;   ldy #$00                 ;set default offset
;   lda CrouchingFlag        ;is player crouching?
;   bne SmallBP              ;if so, branch to increment offset
;     lda PlayerSize           ;is player big?
;     beq BigBP                ;if so, branch to use default offset
; SmallBP:
;     iny                      ;increment for small or big and crouching
; BigBP:
;   lda Player_Y_Position    ;get player's vertical coordinate
;   clc
;   adc BlockYPosAdderData,y ;add value determined by size
;   and #$f0                 ;mask out low nybble to get 16-pixel correspondence
;   sta Block_Y_Position,x   ;save as vertical coordinate for block object
;   ldy Block_State,x        ;get block object state
;   cpy #$11
;   beq :+                   ;if set to value loaded for unbreakable, branch
;     jsr BrickShatter         ;execute code for breakable brick
;     jmp InvOBit              ;skip subroutine to do last part of code here
; :
;   jsr BumpBlock            ;execute code for unbreakable brick or question block
; InvOBit:
  ; lda SprDataOffset_Ctrl   ;invert control bit used by block objects
  ; eor #$01                 ;and floatey numbers
  ; sta SprDataOffset_Ctrl

  lda #$00
  sta Player_Y_Speed      ;init player's vertical speed
ExitFireballNearby:
  rts                      ;leave!

ProcFireball_Bubble:
;  lda PlayerStatus           ;check player's status
;  cmp #$02
;  bcc >rts         ;if not fiery, branch
  lda $076F
  and #$10
  beq ExitFireballNearby
  lda A_B_Buttons
  and #B_Button              ;check for b button pressed
  beq ProcFireballs          ;branch if not pressed
  and PreviousA_B_Buttons
  bne ProcFireballs          ;if button pressed in previous frame, branch

  lda JustThrownHammer
  bne ProcFireballs
;  lda FireballCounter        ;load fireball counter
;  and #%00000001             ;get LSB and use as offset for buffer
;  tax
;  lda Fireball_State,x       ;load fireball state
;  bne ProcFireballs          ;if not inactive, branch
  ldy Player_Y_HighPos       ;if player too high or too low, branch
  dey
  bne ProcFireballs

  lda CrouchingFlag          ;if player crouching, branch
  bne ProcFireballs
;  lda Player_State           ;if player's state = climbing, branch
;  cmp #$03
;  beq ProcFireballs
      ldx #0        ;load fireball counter
      ; Check to see if we have an open fireball spot to put the hammer in
      lda Fireball_State,x       ;load fireball state
      beq SpawnFireball          ;if not inactive, branch
      inx
      lda Fireball_State,x       ;load fireball state
      bne ProcFireballs          ;if not inactive, branch
SpawnFireball:
; stat tracking consider a fireball a LoStab
        inc StatLoStabCount+0
        bne @Skip
        inc StatLoStabCount+1
@Skip:
        lda #Sfx_Fireball          ;play fireball sound effect
        sta Square1SoundQueue
        lda #$02                   ;load state
        sta Fireball_State,x
        ldy PlayerAnimTimerSet     ;copy animation frame timer setting
        sty FireballThrowingTimer  ;into fireball throwing timer
        dey
        sty PlayerAnimTimer        ;decrement and store in player's animation timer
ProcFireballs:
  ldx #$00
  jsr FireballObjCore  ;process first fireball object
  ldx #$01
  jmp FireballObjCore  ;process second fireball object, then do air bubbles

FireballXSpdData:
  .byte 64
	.byte -64

FireballObjCore:
  stx ObjectOffset             ;store offset as current object
  lda Fireball_State,x         ;check for d7 = 1
  asl
  bcs FireballExplosion        ;if so, branch to get relative coordinates and draw explosion
  ; And check if its a player hammer (bit 6)
  asl
  bcs NoFBall
  ldy Fireball_State,x         ;if fireball inactive, branch to leave
  beq NoFBall
  dey                          ;if fireball state set to 1, skip this part and just run it
  beq RunFB
  lda Player_X_Position        ;get player's horizontal position
  ; adc #$04                     ;add four pixels and store as fireball's horizontal position
  adc #12 ; z2mario: Add an extra 8 px to account for hitbox jank 
  sta Fireball_X_Position,x
  lda Player_PageLoc           ;get player's page location
  adc #$00                     ;add carry and store as fireball's page location
  sta Fireball_PageLoc,x
  lda Player_Y_Position        ;get player's vertical position and store
  sta Fireball_Y_Position,x
  lda #$01                     ;set high byte of vertical position
  sta Fireball_Y_HighPos,x
  ldy PlayerFacingDir          ;get player's facing direction
  tya ; Z2 needs a value set for the knockback value in $6d
  sta Fireball_MovingDir,x
  dey                          ;decrement to use as offset here
  lda FireballXSpdData,y       ;set horizontal speed of fireball accordingly
  sta Fireball_X_Speed,x
  lda #4                       ;set vertical speed of fireball
  sta Fireball_Y_Speed,x
  
  lda #0 ; Don't add extra gravity ?
  sta SprObject_X_MoveForce + FireballOffset,x
  sta SprObject_Y_MoveForce + FireballOffset,x
;  lda #$07
;  sta Fireball_BoundBoxCtrl,x  ;set bounding box size control for fireball
  dec Fireball_State,x         ;decrement state to 1 to skip this part from now on
RunFB:
  txa                          ;add 7 to offset to use
  clc                          ;as fireball offset for next routines
  adc #FireballOffset
  tax
  lda #$50                     ;set downward movement force here
  sta $00
  lda #3                       ;set maximum speed here
  sta $02
  lda #$00
  jsr ImposeGravity            ;do sub here to impose gravity on fireball and move vertically
  jsr MoveObjectHorizontally   ;do another sub to move it horizontally

  ldy #3
  jsr $F27D ; run the vanilla z2 offscreen check

  ldx ObjectOffset             ;return fireball offset to X
  ; jsr RelativeFireballPosition ;get relative coordinates
  ; jsr GetFireballOffscreenBits ;get offscreen information
  ; jsr GetFireballBoundBox      ;get bounding box coordinates
  jsr FireballBGCollision      ;do fireball to background collision detection

  ; load the offscreen bits for this enemy
  lda $cb
  and #$FC

  ; lda FBall_OffscreenBits      ;get fireball offscreen bits
  ; and #%11001100               ;mask out certain bits
  bne EraseFB                  ;if any bits still set, branch to kill fireball
;  jsr FireballEnemyCollision   ;do fireball to enemy collision detection and deal with collisions
  jmp DrawFireball             ;draw fireball appropriately and leave
EraseFB:
  lda #$00                     ;erase fireball state
  sta Fireball_State,x
  sta FireballMetasprite,x
NoFBall:
  rts                          ;leave

FireballExplosion:
  ; And check if its a player hammer (bit 6)
  asl
  bcs NoFBall
  jsr RelativeFireballPosition
  jmp DrawExplosion_Fireball

RelativeFireballPosition:
  ldy #$00                    ;set for fireball offsets
  jsr GetProperObjOffset      ;modify X to get proper fireball offset
  ldy #$02
RelWOfs:
;  jsr GetObjRelativePosition  ;get the coordinates
  ldx ObjectOffset            ;return original offset
  rts                         ;leave

; GetFireballOffscreenBits:
;   ldy #$00                 ;set for fireball offsets
;   jsr GetProperObjOffset   ;modify X to get proper fireball offset
;   ldy #$02                 ;set other offset for fireball's offscreen bits
;   jmp GetOffScreenBitsSet  ;and get offscreen information about fireball

; GetFireballBoundBox:
;   txa         ;add seven bytes to offset
;   clc         ;to use in routines as offset for fireball
;   adc #$07
;   tax
;   ldy #$02    ;set offset for relative coordinates
;   bne FBallB  ;unconditional branch

; GetMiscBoundBox:
;   txa                       ;add nine bytes to offset
;   clc                       ;to use in routines as offset for misc object
;   adc #$09
;   tax
;   ldy #$06                  ;set offset for relative coordinates
; FBallB:
;   jsr BoundingBoxCore       ;get bounding box coordinates
;   jmp CheckRightScreenBBox  ;jump to handle any offscreen coordinates

FireballBGCollision:
  lda Fireball_Y_Position,x   ;check fireball's vertical coordinate
  cmp #$18
  bcc ClearBounceFlag         ;if within the status bar area of the screen, branch ahead
  jsr BlockBufferChk_FBall    ;do fireball to background collision detection on bottom of it
  beq ClearBounceFlag         ;if nothing underneath fireball, branch
  jsr ChkForNonSolids         ;check for non-solid metatiles
  beq ClearBounceFlag         ;branch if any found
  lda Fireball_Y_Speed,x      ;if fireball's vertical speed set to move upwards,
  bmi InitFireballExplode     ;branch to set exploding bit in fireball's state
  lda FireballBouncingFlag,x  ;if bouncing flag already set,
  bne InitFireballExplode     ;branch to set exploding bit in fireball's state
  lda #-3
  sta Fireball_Y_Speed,x      ;otherwise set vertical speed to move upwards (give it bounce)
  lda #$01
  sta FireballBouncingFlag,x  ;set bouncing flag
  lda Fireball_Y_Position,x
  and #$f8                    ;modify vertical coordinate to land it properly
  sta Fireball_Y_Position,x   ;store as new vertical coordinate
  rts                         ;leave

ClearBounceFlag:
  lda #$00
  sta FireballBouncingFlag,x  ;clear bouncing flag by default
  rts                         ;leave

InitFireballExplode:
  lda #$80
  sta Fireball_State,x        ;set exploding flag in fireball's state
  lda #Sfx_Bump
  sta Square1SoundQueue       ;load bump sound
  rts                         ;leave

;FireballEnemyCollision:
;      lda Fireball_State,x  ;check to see if fireball state is set at all
;      beq ExitFBallEnemy    ;branch to leave if not
;      asl
;      bcs ExitFBallEnemy    ;branch to leave also if d7 in state is set
;      lda FrameCounter
;      lsr                   ;get LSB of frame counter
;      bcs ExitFBallEnemy    ;branch to leave if set (do routine every other frame)
;      txa
;      asl                   ;multiply fireball offset by four
;      asl
;      clc
;      adc #$1c              ;then add $1c or 28 bytes to it
;      tay                   ;to use fireball's bounding box coordinates
;      ldx #$04
;
;FireballEnemyCDLoop:
;  stx $01                     ;store enemy object offset here
;  tya
;  pha                         ;push fireball offset to the stack
;  lda Enemy_State,x
;  and #%00100000              ;check to see if d5 is set in enemy state
;  bne NoFToECol               ;if so, skip to next enemy slot
;  lda Enemy_Flag,x            ;check to see if buffer flag is set
;  beq NoFToECol               ;if not, skip to next enemy slot
;  lda Enemy_ID,x              ;check enemy identifier
;  cmp #$24
;  bcc GoombaDie               ;if < $24, branch to check further
;  cmp #$2b
;  bcc NoFToECol               ;if in range $24-$2a, skip to next enemy slot
;GoombaDie:
;  cmp #Goomba                 ;check for goomba identifier
;  bne NotGoomba               ;if not found, continue with code
;  lda Enemy_State,x           ;otherwise check for defeated state
;  cmp #$02                    ;if stomped or otherwise defeated,
;  bcs NoFToECol               ;skip to next enemy slot
;NotGoomba:
;  lda EnemyOffscrBitsMasked,x ;if any masked offscreen bits set,
;  bne NoFToECol               ;skip to next enemy slot
;  txa
;  asl                         ;otherwise multiply enemy offset by four
;  asl
;  clc
;  adc #$04                    ;add 4 bytes to it
;  tax                         ;to use enemy's bounding box coordinates
;  jsr SprObjectCollisionCore  ;do fireball-to-enemy collision detection
;  ldx ObjectOffset            ;return fireball's original offset
;  bcc NoFToECol               ;if carry clear, no collision, thus do next enemy slot
;  lda #%10000000
;  sta Fireball_State,x        ;set d7 in enemy state
;  ldx $01                     ;get enemy offset
;  jsr HandleEnemyFBallCol     ;jump to handle fireball to enemy collision
;NoFToECol:
;  pla                         ;pull fireball offset from stack
;  tay                         ;put it in Y
;  ldx $01                     ;get enemy object offset
;  dex                         ;decrement it
;  bpl FireballEnemyCDLoop     ;loop back until collision detection done on all enemies
;
;ExitFBallEnemy:
;  ldx ObjectOffset                 ;get original fireball offset and leave
;  rts

DrawFireball:
  lda FrameCounter         ;get frame counter
  lsr                      ;divide by four
  lsr
  pha                      ;save result to stack
    ;and #$01                 ;mask out all but last bit
    ; eor #FIREBALL_TILE1                 ;set either tile $64 or $65 as fireball tile
    ; sta Sprite_Tilenumber,y  ;thus tile changes every four frames
    lsr
    lda #METASPRITE_FIREBALL_FRAME_1
    bcc :+
      lda #METASPRITE_FIREBALL_FRAME_2
    :
   sta FireballMetasprite,x
    ; sta R0
  pla                      ;get from stack
  lsr                      ;divide by four again
  lsr
  lda #$00                 ;load value $02 to set palette in attrib byte
  bcc FireA                ;if last bit shifted out was not set, skip this
  ora #%11000000           ;otherwise flip both ways every eight frames
FireA:
  sta Fireball_SprAttrib,x  ;store attribute byte and leave
  rts
;   jsr GetFBOAMOffset
;   ldx R0
;   jmp DrawMetasprite

; GetFBOAMOffset:
;   lda ObjectOffset
;   clc
;   adc #14
;   tay
;   rts

ExplosionTiles:
  .byte METASPRITE_EXPLOSION_FRAME_1
  .byte METASPRITE_EXPLOSION_FRAME_2
  .byte METASPRITE_EXPLOSION_FRAME_3

DrawExplosion_Fireball:
  ; ldy Alt_SprDataOffset,x  ;get OAM data offset of alternate sort for fireball's explosion
  lda Fireball_State,x     ;load fireball state
  inc Fireball_State,x     ;increment state for next frame
  lsr                      ;divide by 2
  and #%00000111           ;mask out all but d3-d1
  cmp #$03                 ;check to see if time to kill fireball
  bcs KillFireBall         ;branch if so, otherwise continue to draw explosion
  ;fallthrough
  tay                         ;use whatever's in A for offset
  ; prevent rotation of the fireball from bleeding into the explosion
  lda #1
  sta Fireball_SprAttrib,x
  lda ExplosionTiles,y        ;get tile number using offset
  sta FireballMetasprite,x
  rts
  ; jsr GetFBOAMOffset
  ; ldx R0
  ; jmp DrawMetasprite

KillFireBall:
  lda #$00                    ;clear fireball state to kill it
  sta FireballMetasprite,x
  sta Fireball_State,x
  rts
  ; jsr GetFBOAMOffset
  ; ldx R0
  ; jmp DrawMetasprite

ObjOffsetData:
  .byte $07, $16, $0d

GetProperObjOffset:
  txa                  ;move offset to A
  clc
  adc ObjOffsetData,y  ;add amount of bytes to offset depending on setting in Y
  tax                  ;put back in X and leave
  rts

.export ProcHammerTime
.proc ProcHammerTime
  lda $078b ; Check for hammer in inventory
  bne +
    jmp UpdateHammers
  +
  lda PlayerSize ; Have to be big to throw hammers?
  beq +
    jmp UpdateHammers
  +
    ; and that the player is pushing up + b
    lda SavedJoypadBits
    and #B_Button | Up_Dir     ;check for up + b button pressed
    cmp #B_Button | Up_Dir
    bne UpdateHammers          ;branch if not pressed
    and #B_Button              ;check if b button just pressed
    and PreviousA_B_Buttons
    bne UpdateHammers          ;if button pressed in previous frame, branch
      ldy Player_Y_HighPos       ;if player too high or too low, branch
      dey
      bne UpdateHammers

      ldx #0
      ; Check to see if we have an open fireball spot to put the hammer in
      lda Fireball_State,x       ;load fireball state
      beq SpawnHammer          ;if not inactive, branch
      inx
      lda Fireball_State,x       ;load fireball state
      bne UpdateHammers          ;if not inactive, branch
SpawnHammer:
; stat tracking consider a hammer a HiStab
        inc StatHiStabCount+0
        bne @Skip
        inc StatHiStabCount+1
@Skip:
        inc JustThrownHammer
        ; make mario use the throw animation frame
        ldy PlayerAnimTimerSet     ;copy animation frame timer setting
        sty FireballThrowingTimer  ;into fireball throwing timer
        dey
        sty PlayerAnimTimer        ;decrement and store in player's animation timer
        ; Spawn new hammer
        lda #$41
        sta Fireball_State,x
        lda #Sfx_Fireball          ;play fireball sound effect
        sta Square1SoundQueue
        
        ldy PlayerFacingDir          ;get player's facing direction
        lda Player_X_Position        ;get player's horizontal position
        clc
        adc HammerXPosition-1,y
        sta Fireball_X_Position,x
        lda Player_PageLoc           ;get player's page location
        adc #0
        sta Fireball_PageLoc,x
        lda Player_Y_Position        ;get player's vertical position and store
        sec
        sbc #$08
        sta Fireball_Y_Position,x
        lda #$01                     ;set high byte of vertical position
        sbc #0
        sta Fireball_Y_HighPos,x
        tya ; Z2 needs a value set for the knockback value in $6d
        sta Fireball_MovingDir,x
        dey                          ;decrement to use as offset here
        lda HammerXVelocity,y       ;set horizontal speed of hammer
        clc
        adc Player_X_Speed           ; add the player speed to the hammer
        sta Fireball_X_Speed,x
        lda #-3                       ;set vertical speed of hammer
        sta Fireball_Y_Speed,x
        
        lda #0 ; Don't add extra gravity ?
        sta SprObject_X_MoveForce + FireballOffset,x
        sta SprObject_Y_MoveForce + FireballOffset,x
        ; Clear frame count
        sta PlayerProj_Cnt,x

UpdateHammers:
  bit Fireball_State
  bvc NotHammer1
    ldx #0
    ; Process hammer 1
    jsr MoveHammer
NotHammer1:
  bit Fireball_State+1
  bvc NotHammer2
    ldx #1
    jmp MoveHammer
NotHammer2:
  rts
  
HammerXVelocity:
  .byte $10, -$10
HammerXPosition:
  .byte $10, $00
.endproc
  
.proc MoveHammer
  lda Fireball_State,x         ;check for d7 = 1
  asl
  bcs Erase        ;if so, branch to get relative coordinates and draw explosion

;  stx ObjectOffset
;  txa                          ;add 7 to offset to use
;  clc                          ;as fireball offset for next routines
;  adc #FireballOffset
;  tax
;  lda #$50                     ;set downward movement force here
;  sta $00
;  lda #8                       ;set maximum speed here
;  sta $02
;  lda #$00
;  jsr ImposeGravity            ;do sub here to impose gravity on fireball and move vertically
  
  ; Hammer specific Y velocity code
  ldy #0 ; use Y as the hibit
  lda Fireball_Y_Speed,x
  bpl +
    ; speed is negative, so dey to sign extend
    dey
  +
  clc
  adc Fireball_Y_Position,x
  sta Fireball_Y_Position,x
  tya
  adc Fireball_Y_HighPos,x
  sta Fireball_Y_HighPos,x
  ; Check if the hammer is offscreen quickly
  cmp #2
  bcs Erase
  
	; Hammer specific X velocity code
  ldy #0
	lda Fireball_X_Speed,x
	asl
	asl
	asl
	asl
	adc SprObject_X_MoveForce,x
	sta SprObject_X_MoveForce,x

	php
	lda Fireball_X_Speed,x
  lsr
  lsr
  lsr
  lsr
	cmp #%00001000  ; Check the sign bit
	bcc Positive    ; If the value was not negatively signed, jump ahead
	ora #%11110000  ; Otherwise, apply a sign extension
  dey
Positive:
	plp		 ; Restore carry bit

	adc Fireball_X_Position,x
	sta Fireball_X_Position,x
  tya
  adc Fireball_PageLoc,x
  sta Fireball_PageLoc,x

  ; Every 8 frames, increase the Y velocity by one
  inc PlayerProj_Cnt,x
	lda PlayerProj_Cnt,x
	and #$07
	bne DontInc
    inc Fireball_Y_Speed,x ; Increase Y velocity (gravity)
DontInc:
  
;  jsr MoveObjectHorizontally   ;do another sub to move it horizontally

;  ldx ObjectOffset
;; Quick check if we went offscreen vertically
;  lda Fireball_Y_HighPos,x
;  cmp #2
;  bcs Erase
  ; don't bother doing horizontal offscreen check :p
;  ldy #3
;  jsr $F27D ; run the vanilla z2 offscreen check
;  ; load the offscreen bits for this enemy
;  lda $cb
;  and #$FC
;  bne Erase
    jmp DrawHammer
Erase:
  lda #$00                     ;erase hammer state
  sta Fireball_State,x
  sta FireballMetasprite,x
  rts
  
.endproc

OAM_FLIP_V            = %10000000
OAM_FLIP_H            = %01000000

.proc DrawHammer
  lda #0
  sta Fireball_SprAttrib,x
  lda #1
  sta Fireball_MovingDir,x
  ldy #METASPRITE_HAMMER_FRAME_1
  lda TimerControl
  bne ForceHPose
    lda Fireball_State,x            ;otherwise get hammer's state
    and #%00111111              ;mask out d7
    cmp #$01                    ;check to see if set to 1 yet
    beq GetHPose                ;if so, branch
ForceHPose:
    lda #0
    beq RenderH                 ;do unconditional branch to rendering part
GetHPose:
    lda FrameCounter            ;get frame counter
    lsr                         ;move d3-d2 to d1-d0
    lsr
    and #%00000011              ;mask out all but d1-d0 (changes every four frames)
RenderH:
  sta R2
  ; if bit 0 is set, then use the horizontal hammer sprite
  lsr
  bcc CheckForVerticalFlip
    ldy #METASPRITE_HAMMER_FRAME_2
CheckForVerticalFlip:
  lsr
  bcc WriteMetasprite
    ; if bit 1 is also set, then apply vertical flip too
    ; but if only bit 2 is set, apply the horizontal flag
    lda R2
    lsr
    bcs :+
      lda Fireball_SprAttrib,x
      ora #OAM_FLIP_H 
      sta Fireball_SprAttrib,x
    :
    lda Fireball_SprAttrib,x
    ora #OAM_FLIP_V
    sta Fireball_SprAttrib,x

    ; and use the sprite that faces the other way
    lda #2
    sta Fireball_MovingDir,x
;    sta Fireball_FacingDir,x
WriteMetasprite:
  tya
  sta FireballMetasprite,x
  rts
.endproc

BlockBufferChk_FBall:
  ldy #$1a                  ;set offset for block buffer adder data
  txa
  clc
  adc #FireballOffset       ;add seven bytes to use
  tax
  lda #$00                  ;set A to return vertical coordinate
BBChk_E:
  jsr BlockBufferCollision  ;do collision detection subroutine for sprite object
  ldx ObjectOffset          ;get object offset
  cmp #$00                  ;check to see if object bumped into anything
  rts