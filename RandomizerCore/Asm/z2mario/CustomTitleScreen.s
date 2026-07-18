.include "z2r.inc"

.import GameRoutines, PlayerGfxHandler, DrawMetasprite, PlayerBankTable
.import SwapToPRG0, SwapToSavedPRG, SwapPRG
.import ClearExtraRAMOnScreenTransition, ClearCollisionRAM, BankSwitchMarioCHR

;; Custom title screen for z2 mario
; Removes the sword sprite
; adds a pipe and mario animation
; makes the scroll split use irq instead
; redo all the star sprites to use a new custom twinkle code


NUM_SKY_STARS   = 25
NUM_WATER_STARS = 11
NUM_STARS       = NUM_SKY_STARS + NUM_WATER_STARS

STAR_TILE_C3      = $e8
STAR_TILE_C2      = $ea
STAR_TILE_C1      = $ec
SHOOTING_STAR_TILE = STAR_TILE_C2

; Timers for the title screen created in z2ft
; since the original game synced to the music, z2ft
; recreated this effect with hand timed timers
TitleStage        = $dc
TitleStageBit     = $dd
TitleStageTimerLo = $de
TitleStageTimerHi = $df

TitleScreenRamBase = $6690
SET_RES_BASE TitleScreenRamBase
RESV seed
RESV StarColorIndex, NUM_STARS
RESV StarTimer, NUM_STARS
RESV StarBaseTimer, NUM_STARS
RESV ShootingStarY
RESV ShootingStarX
RESV ShootingStarActive
RESV TitleTempIndex
RESV TitleIrqActive

; Copies of the scroll data that will be written during IRQ this frame
RESV TitleIrqPpuCtrl
RESV TitleIrqNametableHi
RESV TitleIrqNametableLo

RESV TitleState
RESV PipeYPositionLo
RESV PipeYPositionHi
RESV PipeXPosition

; If the player jumps off the screen, we'll just have mario come out of the pipe again
; put this in ZP so that it gets cleared on boot, but not on loop
Respawning = $db

TITLE_SPLIT_IRQ_LINE = 142

TITLE_PIPE_RISE_TARGET_TIMER = $01f4
TITLE_MARIO_RISE_TARGET_TIMER = $00f9
TITLE_MARIO_LEAVES_TARGET_TIMER = $03d4
TITLE_PIPE_TARGET_Y = 143
TITLE_PIPE_X = 112


FREE "PRG5" [$A7C1, $A932)

.segment "PRG5"
.org $A72E
    jsr InitTitleStars

.org $AE41
    jsr InitTitleStars

.org $A6F7
    jmp *+6

; Replace the title screen sprite zero with IRQ. With that, we are free from sprite zero! ...
; ... only in mario mode lol

; Sprite/OAM work runs after setting the split IRQ so that timing-critical IRQ
; doesn't get delayed due to running the mario code
.org $A737
    jsr RunTitleMain
    jmp $A76A
FREE_UNTIL $A76A

.org $AB6D
    jsr RunTitleMain
    jmp $ABA0
FREE_UNTIL $ABA0

.segment "PRG7"
.reloc
RunTitleMain:
    lda #$e ; use the same bank the extended 
    sta CurrentPrgBank
    jsr SwapPRG
    jsr SetTitleSplitIrq
    jsr UpdateTitleSprites
    lda #5
    sta CurrentPrgBank
    jmp SwapPRG

.segment "PRG1C", "PRG1D"

; Fake that mario doesn't collide while in the title screen (for z2 collision that is)
.org $850c
    clc
    rts

; Main loop for the title screen driven by TitleState.
.reloc
UpdateTitleSprites:
    lda #0
    sta CurrentOAMOffset

    jsr RunCurrentTitleState

    jsr DrawTitleRocks
    jsr DrawTitleStars

    ; Hide whatever OAM slots are left unclaimed this frame.
    ldx CurrentOAMOffset
    beq @Done
@HideRemaining:
        lda #$F8
        sta Sprite_Y_Position,x
        inx
        inx
        inx
        inx
        bne @HideRemaining
@Done:
    rts

.reloc
RunCurrentTitleState:    
    lda TitleState
    asl
    tax
    lda TitleStateTable+1,x
    pha
    lda TitleStateTable,x
    pha
    rts

.reloc
TitleStateTable:
    .word WaitForFirstSpawn-1
    .word CheckForSpawnMario-1
    .word PipeRises-1
    .word MarioRises-1
    .word MarioLeavesScreen-1

.reloc
WaitForFirstSpawn:
    lda TitleStage
    cmp #2
    bne +
        ; Move to the next state
        inc TitleState
    +
    rts

.reloc
CheckForSpawnMario:
    ; Only respawn mario if he's off the screen
    lda Player_Y_HighPos
    cmp #1
    beq +
        inc TitleState
    +
    jsr DrawTitleMario
    jmp DrawTitlePipe

.reloc
PipeRises:
    ; Disable the player control temporarily except start
    lda SavedJoypadBits
    and #$10
    sta SavedJoypadBits
    ; If the pipe is onscreen, keep rising
    lda PipeYPositionHi
    cmp #1
    beq @continueRise
    ; If we are in stage 2, then wait for the specific timer
    ; to sync with the first title launch. Otherwise we can respawn whenever
        jsr CheckTimerByState
        bcc @noSpawn
        ; spawn the pipe!
        lda #1
        sta PipeYPositionHi
        lda #TITLE_PIPE_TARGET_Y + 32
        sta PipeYPositionLo
        jmp DrawTitlePipe
@continueRise:
    lda PipeYPositionLo
    cmp #TITLE_PIPE_TARGET_Y
    beq @risefinished
    ; rise at 1 px / 4 frames
    lda FrameCounter
    and #3
    bne +
        dec PipeYPositionLo
    +
    jmp DrawTitlePipe
@risefinished:
    ; reached our target spot so exit. This lets future runs keep the pipe on the screen
    inc TitleState
    jmp DrawTitlePipe
@noSpawn:
    rts


.reloc
MarioRises:
    jsr DrawTitlePipe
    ; Disable the player control temporarily except start
    lda SavedJoypadBits
    and #$10
    sta SavedJoypadBits

    lda Player_Y_HighPos
    cmp #1
    beq @continueRising
        ; check if its time to start rising mario
        jsr CheckTimerByState
        bcc @noSpawn
            ; Time to move mario into position
            ldx #1
            stx Player_Y_HighPos
            jsr InitMarioData
            jmp DrawTitleMario

@continueRising:
    ; check if we need to keep rising mario
    lda FrameCounter
    and #3
    bne +
    dec Player_Y_Position
    lda Player_Y_Position
    cmp #TITLE_PIPE_TARGET_Y - 31
    bne +
        ; reached our target spot
        inc TitleState
    +
    jmp DrawTitleMario
@noSpawn:
    rts

.reloc
MarioLeavesScreen:
    ; Disable the player control temporarily except start
;     lda SavedJoypadBits
;     and #$10
;     sta SavedJoypadBits
    
;     lda FrameCounter
;     and #3
;     bne +
;     dec Player_Y_Position
;     lda Player_Y_Position
; +
    ; For now, just turn on respawning and let the player move
    lda #1
    sta TitleState
    sta Respawning
    jsr DrawTitleMario
    jmp DrawTitlePipe

.reloc
TitleTimerStage:
    .byte 2, 2, 4
.reloc
TitleTimerTargetTimerLo:
    .byte <TITLE_PIPE_RISE_TARGET_TIMER, <TITLE_MARIO_RISE_TARGET_TIMER, <TITLE_MARIO_LEAVES_TARGET_TIMER
.reloc
TitleTimerTargetTimerHi:
    .byte >TITLE_PIPE_RISE_TARGET_TIMER, >TITLE_MARIO_RISE_TARGET_TIMER, >TITLE_MARIO_LEAVES_TARGET_TIMER

; Initialize the star timers and colors. The old stars/water sparkles worked by cycling through
; different palette indicies, so they would use each of the colors from color 3 in the different palettes
; Well thats a problem when i want to use mario on the screen, and stars shouldn't twinkle brown.
; So instead, we lose one color from the star twinkle, (the deep $02 blue from before), and instead
; put all stars on the same palette index 0. We then use two different "star sprites" that use color
; 1 and 2 as well as the vanilla color 3. That way the stars can twinkle independently of each other. 
.segment "PRG5"
.reloc
InitTitleStars:
    lda Respawning
    beq +
        ; don't reinitalize if we have looped once
        rts
    +
    lda FrameCounter
    sta seed

    ldx #0
@SkyLoop:
    jsr BarebonesTitleScreenRNG
    and #7
    clc
    adc #1
    asl
    asl
    asl
    sta StarBaseTimer,x
    sta StarTimer,x
    jsr BarebonesTitleScreenRNG
    and #3
    sta StarColorIndex,x
    inx
    cpx #NUM_SKY_STARS
    bne @SkyLoop

@WaterLoop:
    jsr BarebonesTitleScreenRNG
    and #7
    clc
    adc #1
    asl
    asl
    sta StarBaseTimer,x
    sta StarTimer,x
    jsr BarebonesTitleScreenRNG
    and #3
    sta StarColorIndex,x
    inx
    cpx #NUM_STARS
    bne @WaterLoop

    lda #$07
    sta ShootingStarY
    lda #$28
    sta ShootingStarX
    lda #0
    sta ShootingStarActive
    sta TitleIrqActive
    sta TitleState

    ; clear z2 collision ram
    ldy #$d0
    lda #$11 ; value chosen to not match any "lava" or other weird tiles
@collisionloop:
    sta $6000-1,y
    sta $6000-1+$d0,y
    sta $6000-1+$d0*2,y
    sta $6000-1+$d0*3,y
    dey
    bne @collisionloop

    jmp SetupTitleMario ; jumping here to split the function so it fits better

.reloc
SetupTitleMario:
    ; wipes timers, size/state flags, boss ram, etc
    jsr ClearExtraRAMOnScreenTransition
    jsr ClearCollisionRAM

    lda #$28                    ;store value here
    sta VerticalForceDown       ;for fractional movement downwards
    ; Set our custom floor bytes
    ldx #0
    stx Respawning
    jsr InitMarioData
    inx
    ; page 1 == onscreen, so put him on page 2 offscreen
    ; falling forever (until its time to spawn)
    stx Player_Y_HighPos
    stx Player_SprAttrib
    stx PipeYPositionHi

    ldx #8
    ; PlayerCtrlRoutine index in the GameRoutines jump table
    stx GameEngineSubroutine
@collisionLoop:
        sta $63d7,x
        sta $63e7+$d0,x
        sta $64b0+$d0,x
        dex
        bpl @collisionLoop
    ; setup pipe collision
    sta $63c7+$d0
    sta $63c8+$d0
    sta $63d7+$d0
    sta $63d8+$d0
    sta $64e2 ; tiny baby rock on the left
    ldx #16 * 6
    @loop2:
        ; do a left side rock as well.
        sta $63c0+$d0,x
        sta $63c1+$d0,x
        ; and walls off the screen
        sta $6377,x
        sta $6457+$d0,x
        txa
        sec
        sbc #16
        tax
        bpl @loop2
    lda #CHR_BIGMARIO
    sta PlayerChrBank
    inc ReloadCHRBank
    jmp BankSwitchMarioCHR

.reloc
BarebonesTitleScreenRNG:
    lda seed
    asl a
    bcs @NoEor
        eor #$46
@NoEor:
    adc #$EB
    sta seed
    rts

;; ---
; Drawing routines

.segment "PRG7"
.reloc
InitMarioData:
    ldx #0
    stx Player_State
    stx PlayerSize        ; big mario
    stx SwimmingFlag
    stx CrouchingFlag
    stx Player_X_Speed
    stx Player_Y_Speed
    stx Player_X_MoveForce
    stx Player_Y_MoveForce
    stx LinkCollisionBits
    inx
    stx PlayerFacingDir
    stx Player_MovingDir
    stx Player_PageLoc
    stx ScreenLeft_X_Pos
    stx ScreenLeft_PageLoc
    lda #TITLE_PIPE_X+1
    sta Player_X_Position
    lda #TITLE_PIPE_TARGET_Y
    sta Player_Y_Position
    rts

.reloc
DrawTitleMario:
    jsr SwapToPRG0
        .import DecrementMarioTimers, ProcessMarioInput
        
        lda TitleState
        cmp #3 ; MarioRises skip running mario code during that to keep him from getting ejected out of the pipe
        beq +
            jsr DecrementMarioTimers
            jsr GameRoutines
        +

        jsr PlayerGfxHandler
        jsr ProcessMarioInput

        ldy ObjectMetasprite
        lda PlayerBankTable,y
        cmp PlayerChrBank
        beq @SameBank
            sta PlayerChrBank
            inc ReloadCHRBank
            jsr BankSwitchMarioCHR
        @SameBank:

        ldx ObjectMetasprite
        ldy #0
        jsr DrawMetasprite
    jmp SwapToSavedPRG

.segment "PRG1C", "PRG1D"
.reloc
DrawTitleRocks:
    ldx CurrentOAMOffset
    ldy #0
@RockLoop:
        lda RockOamTable,y
        sta Sprite_Y_Position,x
        lda RockOamTable+1,y
        sta Sprite_Tilenumber,x
        lda #1
        sta Sprite_Attributes,x
        lda RockOamTable+2,y
        sta Sprite_X_Position,x
        inx
        inx
        inx
        inx
        iny
        iny
        iny
        cpy #ROCK_TABLE_LEN
        bne @RockLoop
    stx CurrentOAMOffset
    rts

.reloc
PipeSpriteXData:
    .byte TITLE_PIPE_X+0, TITLE_PIPE_X+8, TITLE_PIPE_X+16, TITLE_PIPE_X+24
.reloc
PipeSpriteTileTopRow:
    .byte $ae+0, $ae+2, $ae+4, $ae+6
.reloc
PipeSpriteTileBotRow:
    .byte $b6+0, $b6+2, $b6+4, $b6+6
.reloc
DrawTitlePipe:
    lda PipeYPositionHi ; don't draw pipe if its hidden
    cmp #1
    beq +
        rts
    +
    ldy CurrentOAMOffset
    ; check if we are already at the target
    lda PipeYPositionLo
    cmp #TITLE_PIPE_TARGET_Y
    beq +
        ; if not, draw a row of overlay sprites to hide the bottom
        ldx #3
@overlayloop:
            lda PipeSpriteXData,x
            sta Sprite_X_Position,y
            lda #TITLE_PIPE_TARGET_Y+32
            sta Sprite_Y_Position,y
            lda #$ba
            sta Sprite_Tilenumber,y
            lda #$21 ; background it to force the pipe to hide
            sta Sprite_Attributes,y
            iny
            iny
            iny
            iny
            dex
            bpl @overlayloop
    +
    ; Draw the top row of tiles. This one is always visible
    ldx #3
@toploop:
        lda PipeSpriteXData,x
        sta Sprite_X_Position,y
        lda PipeYPositionLo
        sta Sprite_Y_Position,y
        lda PipeSpriteTileTopRow,x
        sta Sprite_Tilenumber,y
        lda #$03
        sta Sprite_Attributes,y
        iny
        iny
        iny
        iny
        dex
        bpl @toploop
    ; bottom row, only display this is the pipe is high enough
    lda PipeYPositionLo
    cmp #TITLE_PIPE_TARGET_Y+16
    bcs @exit
    ; carry is clear
    adc #16
    sta $00
    ldx #3
@botloop:
        lda PipeSpriteXData,x
        sta Sprite_X_Position,y
        lda $00
        sta Sprite_Y_Position,y
        lda PipeSpriteTileBotRow,x
        sta Sprite_Tilenumber,y
        lda #$03
        sta Sprite_Attributes,y
        iny
        iny
        iny
        iny
        dex
        bpl @botloop
@exit:
    sty CurrentOAMOffset
    rts

.reloc
CheckTimerByState:
    ; Skip checking timers if this is a respawn
    lda Respawning
    bne @yesSpawn
    ldx TitleState
    lda TitleStage
    cmp TitleTimerStage-2,x
    bne @noSpawn
    lda TitleStageTimerLo
    cmp TitleTimerTargetTimerLo-2,x
    bne @noSpawn
    lda TitleStageTimerHi
    cmp TitleTimerTargetTimerHi-2,x
    bne @noSpawn
@yesSpawn:
        sec
        rts
@noSpawn:
    clc
    rts

.reloc
DrawTitleStars:
    ldy CurrentOAMOffset
    lda ShootingStarActive
    bne @SSMove
        lda FrameCounter
        bne @SSDraw
        lda #1
        sta ShootingStarActive
        bne @SSDraw
@SSMove:
    lda FrameCounter
    and #3
    bne @SSCheckLimit
        lda ShootingStarY
        clc
        adc #2
        sta ShootingStarY
        lda ShootingStarX
        clc
        adc #3
        sta ShootingStarX
@SSCheckLimit:
    lda ShootingStarY
    cmp #$37
    bcc @SSDraw
        lda #0
        sta ShootingStarActive
        lda #$F8
        sta ShootingStarY
@SSDraw:
    lda ShootingStarY
    sta Sprite_Y_Position,y
    lda #SHOOTING_STAR_TILE
    sta Sprite_Tilenumber,y
    lda #$20 ; background
    sta Sprite_Attributes,y
    lda ShootingStarX
    sta Sprite_X_Position,y
    iny
    iny
    iny
    iny
@Done:
    ldx #0
@StarLoop:
    dec StarTimer,x
    bne @CheckColor
        lda StarBaseTimer,x
        sta StarTimer,x
        inc StarColorIndex,x
        lda StarColorIndex,x
        and #3
        sta StarColorIndex,x
@CheckColor:
    lda StarColorIndex,x
    beq @NextStar
        cpy #0
        beq @NextStar ; ran outta oam slots, but keep processing sprites

        stx TitleTempIndex
        tax
        lda TileForColor-1,x
        pha
        ldx TitleTempIndex
        lda StarPosTableY,x
        sta Sprite_Y_Position,y
        pla
        sta Sprite_Tilenumber,y
        lda Sprite_Y_Position,y
        cmp #TITLE_SPLIT_IRQ_LINE
        bcs +
            lda #$20 ; background for the stars
            bne @writeAttrs
        +
        lda #0 ; foreground for the water
    @writeAttrs:
        sta Sprite_Attributes,y
        lda StarPosTableX,x
        sta Sprite_X_Position,y
        iny
        iny
        iny
        iny
@NextStar:
    inx
    cpx #NUM_STARS
    bne @StarLoop
    sty CurrentOAMOffset
    rts

; ----
; IRQ handler
.reloc
SetTitleSplitIrq:
    lda PpuCtrlShadow
    and #$FC
    ora $36
    sta TitleIrqPpuCtrl
    lda $27
    sta TitleIrqNametableHi
    lda $28
    sta TitleIrqNametableLo

    ; write JMP IrqHdlr to RAM
    lda #$4c
    sta IRQTrampoline
    lda #<TitleSplitIrqBody
    sta IRQTrampolineAddrLo
    lda #>TitleSplitIrqBody
    sta IRQTrampolineAddrHi
    lda #TITLE_SPLIT_IRQ_LINE
    sta LineIrqTgtReg
    lda #1
    sta TitleIrqActive
    lda #ENABLE_SCANLINE_IRQ
    sta LineIrqStatusReg
    cli
    rts

.reloc
RockOamTable:
    .byte $7F, $F0, $F8
    .byte $3F, $EE, $F8
    .byte $4F, $F6, $F0
    .byte $4F, $F4, $F8
    .byte $5F, $EE, $E8
    .byte $5F, $F0, $F0
    .byte $5F, $F0, $F8
    ; .byte $6F, $EE, $D0
    .byte $6F, $F0, $D8
    .byte $6F, $F4, $E0
    .byte $6F, $F2, $E8
    .byte $6F, $F6, $F0
    .byte $6F, $F0, $F8
    ; .byte $7F, $F0, $D0
    .byte $7F, $F2, $D8
    .byte $7F, $F4, $E0
    .byte $7F, $F0, $E8
    .byte $7F, $F2, $F0
ROCK_TABLE_LEN = (* - RockOamTable)

StarPosTableY:
    .byte $07, $07, $0F, $0F, $5F, $17, $1F, $1F, $5F, $1F, $4F, $1F, $1F
    .byte $27, $27, $27, $2F, $2F, $37, $37, $3F, $3F, $47, $47, $4F ; sky (25)
    .byte $AF, $B4, $9F, $AD, $92, $95, $8F, $95, $98, $78, $93      ; water (11)

StarPosTableX:
    .byte $50, $A0, $20, $78, $40, $E0, $10, $28, $60, $A0, $80, $E0, $E8
    .byte $78, $C8, $E8, $28, $D8, $10, $C8, $A8, $18, $08, $48, $E0 ; sky (25)
    .byte $40, $20, $58, $48, $70, $88, $90, $B0, $8A, $95, $8E      ; water (11)

TileForColor:
    .byte STAR_TILE_C1, STAR_TILE_C2, STAR_TILE_C3


.segment "PRG7"
.reloc
TitleSplitIrqBody:
    pha
        ; burn some cycles to hide the scroll split - 29 cycles for 6 bytes
        clc
        lda #$2a
        nop
        bcc *-2
        lda TitleIrqPpuCtrl
        sta PpuCtrlShadow
        sta PPUCTRL
        lda TitleIrqNametableHi
        sta PPUADDR
        lda TitleIrqNametableLo
        sta PPUADDR
        lda PPUDATA
        lda PPUDATA
        lda PPUSTATUS
        lda #0
        sta TitleIrqActive
        sta LineIrqStatusReg
    pla
	rti

.segment "PRG5"

; New palette for the stars
.org $AF51
    .byte $10, $30, $21
.org $AF59
    .byte $16, $27, $18
.org $AF5d
    .byte $29, $1a, $0f
; The vanilla title screen uses Player_Y_Position ($29) to hold part of the scroll
; so relocate all of those to use $3a instead.
.org $AD09
    .byte $3A
.org $AD2B
    .byte $3A
.org $AD2F
    .byte $3A
.org $AD47
    .byte $3A
.org $AD53
    .byte $3A
.org $AD59
    .byte $3A
.org $ADE7
    .byte $3A
