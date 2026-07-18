.include "z2r.inc"

;; Custom title screen for z2 mario
; Removes the sword sprite
; adds a pipe and mario animation
; makes the scroll split use irq instead
; redo all the star sprites to use a new custom twinkle code

.segment "PRG5"

NUM_SKY_STARS   = 25
NUM_WATER_STARS = 11
NUM_STARS       = NUM_SKY_STARS + NUM_WATER_STARS

STAR_TILE_C3      = $e8
STAR_TILE_C2      = $ea
STAR_TILE_C1      = $ec
SHOOTING_STAR_TILE = STAR_TILE_C2

seed               = $6690
StarColorIndex     = $6691
StarTimer          = $66B5
StarBaseTimer      = $66D9
ShootingStarY      = $66FD
ShootingStarX      = $66FE
ShootingStarActive = $66FF
TitleTempIndex     = $6700

TitleIrqActive     = $6701

; Copies of the scroll data that will be written during IRQ this frame
TitleIrqPpuCtrl     = $6702
TitleIrqNametableHi = $6703
TitleIrqNametableLo = $6704


TITLE_SPLIT_IRQ_LINE = 142

FREE "PRG5" [$A7C1, $A932)

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
    jsr SetTitleSplitIrq
    jsr UpdateTitleSprites
    jmp $A76A
FREE_UNTIL $A76A

.org $AB6D
    jsr SetTitleSplitIrq
    jsr UpdateTitleSprites
    jmp $ABA0
FREE_UNTIL $ABA0

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
TitleSplitIrqBody:
    pha
        ; burn some cycles to hide the scroll split
        pha
        pla
        pha
        pla
        pha
        pla
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


; New palette for the stars
.org $AF51
    .byte $10, $30, $21


.reloc
RockOamTable:
    .byte $7F, $F0, $F8
    .byte $3F, $EE, $F8
    .byte $4F, $F6, $F0
    .byte $4F, $F4, $F8
    .byte $5F, $EE, $E8
    .byte $5F, $F0, $F0
    .byte $5F, $F0, $F8
    .byte $6F, $EE, $D0
    .byte $6F, $F0, $D8
    .byte $6F, $F4, $E0
    .byte $6F, $F2, $E8
    .byte $6F, $F6, $F0
    .byte $6F, $F0, $F8
    .byte $7F, $F0, $D0
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

; Initialize the star timers and colors. The old stars/water sparkles worked by cycling through
; different palette indicies, so they would use each of the colors from color 3 in the different palettes
; Well thats a problem when i want to use mario on the screen, and stars shouldn't twinkle brown.
; So instead, we lose one color from the star twinkle, (the deep $02 blue from before), and instead
; put all stars on the same palette index 0. We then use two different "star sprites" that use color
; 1 and 2 as well as the vanilla color 3. That way the stars can twinkle independently of each other. 
.reloc
InitTitleStars:
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

    ; Shooting star: starting position copied from vanilla entry 27.
    lda #$07
    sta ShootingStarY
    lda #$28
    sta ShootingStarX
    lda #0
    sta ShootingStarActive
    sta TitleIrqActive
    rts

.reloc
UpdateTitleSprites:
    lda #0
    sta CurrentOAMOffset

    jsr DrawTitleOverlay
    jsr DrawTitlePipe
    jsr DrawTitleMario
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
DrawTitleOverlay:
    ; TODO: sprites that mask the pipe rising/lowering out of the ground.
    rts

.reloc
DrawTitlePipe:
    ; TODO: the pipe itself.
    rts

.reloc
DrawTitleMario:
    ; TODO: mario's title screen sprite
    rts

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
