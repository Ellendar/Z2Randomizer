
.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG

.define StatDisplayState $130
.define InternalState $131

; TODO: Add patches to update stats

.segment "PRG7"
.reloc
UpdateCreditsSwitchBank:
    lda #1
    jsr SwapPRG
    jmp UpdateCredits

.import AddExtraRowOfItems
DrawPauseMenuRowSwitchBank:
    lda #0
    jsr SwapPRG
    jsr AddExtraRowOfItems
    lda #1
    jmp SwapPRG

.segment "PRG5"

RestartAfterCredits = $921C

; Hook into the final step before restarting to display our new credits scene
.org $8C01
.word (UpdateCreditsSwitchBank)

; Hook the main PRG5 entrypoint to turn off the palette cycle
.org $8BBF
    jsr DisablePaletteCycle
    bcs $8BE1

.reloc
DisablePaletteCycle:
    ; sets the carry if the main state is >= $c
    lda $0761
    cmp #$0d
    ; perform the patched instructions
    ldx $0301
    ldy #0
    rts
;;; Rendering the stats page
; Trying to fill in whatever data is needed into free space in PRG1 / 2 since they don't have many custom code patches
.segment "PRG1"

.reloc
UpdateCredits:
    lda StatDisplayState
    pha
        jsr RunFrame
    pla
    cmp StatDisplayState
    beq +
        ; Reset the internal state after every state ends
        lda #0
        sta InternalState
    +
    jmp SwapToSavedPRG

RunFrame:
    ; a == StatDisplayState
    cmp #4
    bcc +
        jsr UpdateSpritePosition
+   lda StatDisplayState
    jsr JumpEngine
.word (FadeOut)
.word (DrawBackground)
.word (RenderPlayerInfo)
.word (DrawStats)
.word (DrawTimeStamp)
.word (FadeIn)
.word (WaitForStart)
.word (FadeOut)
.word (RestartToTitle)

.reloc
DrawStats:
DrawTimeStamp:
    inc StatDisplayState
    rts

.reloc
RestartToTitle:
    inc $0761
    rts

.reloc
RenderPlayerInfo:
    ; Render the sprites
    jsr DrawPauseMenuRowSwitchBank

    ; Move those sprites to the position that we want them at
    ldx #16-1
    ldy #(16-1)*4
-       lda $200,y
        ; Don't move offscreen sprites over
        cmp #$f0
        bcs +
            sec 
            sbc #$80-10
        +
        sta $200,y
        sta $390,x
        lda $203,y
        sec 
        sbc #$80+8
        sta $203,y
        sta $3b0,x
        dey
        dey
        dey
        dey
        dex
        bpl -
    ; Create the heart and jar sprite
    ; ignore the y pos for now. we'll add it in the update
    ; x pos
    lda #6 * 8 - 1
    sta $203 + $50
    sta $203 + $58
    lda #7 * 8 - 1
    sta $203 + $54
    sta $203 + $5c
    ; tile id
    lda #$81
    sta $201 + $50
    sta $201 + $54
    lda #$83
    sta $201 + $58
    sta $201 + $5c
    ; attr
    lda #$01
    sta $202 + $50
    sta $202 + $58
    lda #$41
    sta $202 + $54
    sta $202 + $5c
    
    
    ; Draw the name(s) / hash / levels / lives
    inc StatDisplayState
    rts

ARRAY_INDEX .set 0
.macro PPU_BUFFER_APPEND Src, SrcLen, Dst, DstLen
.ident(.concat("ARRAY_SRC_", .string(ARRAY_INDEX)) = Src
.ident(.concat("ARRAY_SRCLEN_", .string(ARRAY_INDEX)) = SrcLen
.ident(.concat("ARRAY_DST_", .string(ARRAY_INDEX)) = Dst
.ident(.concat("ARRAY_DSTLEN_", .string(ARRAY_INDEX)) = DstLen
ARRAY_INDEX .set CONVERT_ARRAY_INDEX + 1
.endmacro

PPU_BUFFER_APPEND $07A1, 8, $2062, 8 ; Player Name
PPU_BUFFER_APPEND $0784, 1, $20a9, 1 ; Heart Containers
PPU_BUFFER_APPEND $0783, 1, $20e9, 1 ; Magic Containers
PPU_BUFFER_APPEND $0777, 1, $21a4, 1 ; Atk
PPU_BUFFER_APPEND $0779, 1, $21c4, 1 ; Lif
PPU_BUFFER_APPEND $0778, 1, $21a8, 1 ; Mag
PPU_BUFFER_APPEND $0700, 1, $21c8, 1 ; Lives

; Data that should be converted to base 10
PPU_BUFFER_APPEND $0777, 1, $21a4, 3 ; Deaths
PPU_BUFFER_APPEND $0779, 1, $21c4, 3 ; Resets
PPU_BUFFER_APPEND $0778, 2, $21a8, 3 ; Hi Stab
PPU_BUFFER_APPEND $0700, 2, $21c8, 3 ; Lo Stab
PPU_BUFFER_APPEND $0778, 2, $21a8, 3 ; Up Stab
PPU_BUFFER_APPEND $0700, 2, $21c8, 3 ; Dw Stab

.reloc
UpdateSpritePosition:
    ldy #(16-1)*4
    ldx #16-1
-       lda $390,x
        sta $200,y
        lda $3b0,x
        sta $203,y
        dey
        dey
        dey
        dey
        dex
        bpl -
    ; heart container y
    lda #4 * 8 - 1
    sta $200 + $50
    sta $200 + $54
    ; magic container y
    lda #6 * 8 - 1
    sta $200 + $58
    sta $200 + $5c
    ; link sprite offset
    lda #$70
    sta $90
    ; link x offset
    lda #$08+4
    sta $cc
    ; link y offset
    lda #$15+8
    sta $29
    ; link metasprite
    lda #3
    sta $80
    jsr $EC02
    rts

.reloc
WaitForStart:
    ; check if start is pressed
    lda $f5
    and #$10
    beq :>rts
        inc StatDisplayState
        rts
    ; TODO we can make link animated here?
    rts

.reloc
DrawBackground:
    ; Since we don't have any unplanned ppu updates, we can just use
    ; hardcoded offsets into the ppu buffer to simplify the code
    lda #0
    sta $302
    lda InternalState
    ; Mult by 6
    asl
    rol $302
    asl
    rol $302
    asl
    rol $302
    asl
    rol $302
    asl
    rol $302
    asl
    rol $302
    sta $303
    sta $00
    lda $302
    sta $01
    clc
    adc #$20
    sta $302
    
    ; 32 bytes
    lda #$20
    sta $304
    sta $327
    
    ; Copy the address to the second location but offset by 32 bytes
    lda $303
    clc
    adc #$20
    sta $326
    lda $302
    adc #0
    sta $325
    
    ; setup the read ptr
    lda #<BackgroundData
    clc
    adc $00
    sta $00
    lda #>BackgroundData
    adc $01
    sta $01
    
    ldy #$20-1
    -   lda ($00),y
        sta $305,y
        dey
        bpl -
    ldy #$40-1
    -   lda ($00),y
        sta $305 + 3,y ; +3 to account for the header
        dey
        cpy #$20-1
        bne -
    lda #$ff
    sta $348
    inc InternalState
    lda InternalState
    cmp #$10
    bne +
        inc StatDisplayState
    +
    rts

.reloc
; random ordering chosen by fair dice roll
RandomOrderTable:
.byte 3, 14, 5, 10, 1, 7, 13, 15, 2, 9, 6,11

.reloc
FadeOut:
    ; Every 16 frames step to the next palette
    lda $12 ; global timer
    and #$01
    beq :>rts
    ldx InternalState
    cpx #12
    bne +
        ; Fade out complete so exit
        inc StatDisplayState
        rts
    +
    ldy $301
    lda #$3f
    sta $302,y
    sta $306,y
    lda RandomOrderTable,x
    sta $303,y
    ora #$10
    sta $307,y
    lda #1
    sta $304,y
    sta $308,y
    lda #$0f
    sta $305,y
    sta $309,y
    lda #$ff
    sta $30a,y
    ; Move to the next
    inc InternalState
    ; and clear out the sprite zero memory while we are at it :p
    lda #$f8
    sta $200
    rts


.reloc

SprPaletteTable = $80AE
FadeIn:
    ; Every 16 frames step to the next palette
    lda $12 ; global timer
    and #$01
    beq :>rts
    ldx InternalState
    cpx #12
    bne +
        ; Fade out complete so exit
        inc StatDisplayState
        rts
    +
    ldy $301
    lda #$3f
    sta $302,y
    sta $306,y
    lda RandomOrderTable,x
    tax
    sta $303,y
    ora #$10
    sta $307,y
    lda #1
    sta $304,y
    sta $308,y
    lda BGPaletteTable,x
    sta $305,y
    lda SprPaletteTable,x
    sta $309,y
    lda #$ff
    sta $30a,y
    ; Move to the next color
    inc InternalState
    rts
BGPaletteTable:
; Border
.byte $0f, $30, $12, $16
; Red Text
.byte $0f, $0f, $0f, $0f
; Blue Text
.byte $0f, $0f, $0f, $0f
; unused
.byte $0f, $0f, $0f, $0f


;;;
; Writes the current timestamp to SRAM.
; [In a] - index of the type of timestamp to write
; Notice this does NOT preserve registers!
.reloc
AddTimestamp:
  ldx TimestampCount
  sta TimestampTypeList,x
  asl ; clears carry
  adc TimestampTypeList,x
  tax
  ; double read the timestamp. this prevents an issue where reading is interrupted by NMI
  ; keep the lo value in X so we can check if it changed and read again if it does
-   ldy StatTimer+0
    tya
    sta TimestampList,x
    lda StatTimer+1
    sta TimestampList + 1,x
    lda StatTimer+2
    sta TimestampList + 2,x
    cpy StatTimer+0
    bne -
  inc TimestampCount
  rts

;; Long division routine copied from here:
;; https://codebase64.org/doku.php?id=base:24bit_division_24-bit_result

.define Remainder  $60
.define Dividend   $63
.define Divisor    $66
.define DivTmp     $69

.define Hex0        $70
.define DecOnes     $71
.define DecTens     $72
.define DecHundreds $73

.define TmpHoursOnes   $75
.define TmpMinutesOnes $76
.define TmpMinutesTens $77
.define TmpSecondsOnes $78
.define TmpSecondsTens $79

.reloc
LongDivision24bit:
  tya
  pha
  lda #0              ;preset remainder to 0
  sta Remainder
  sta Remainder+1
  sta Remainder+2
  ldx #24             ;repeat for each bit: ...
@Loop:
    asl Dividend      ;dividend lb & hb*2, msb -> Carry
    rol Dividend+1
    rol Dividend+2
    rol Remainder     ;remainder lb & hb * 2 + msb from carry
    rol Remainder+1
    rol Remainder+2
    lda Remainder
    sec
    sbc Divisor       ;substract divisor to see if it fits in
    tay               ;lb result -> Y, for we may need it later
    lda Remainder+1
    sbc Divisor+1
    sta DivTmp
    lda Remainder+2
    sbc Divisor+2
    bcc @Skip         ;if carry=0 then divisor didn't fit in yet

    sta Remainder+2   ;else save substraction result as new remainder,
    lda DivTmp
    sta Remainder+1
    sty Remainder	
    inc Dividend      ;and INCrement result cause divisor fit in 1 times
@Skip:
  dex
  bne @Loop
  pla
  tay
  rts

.reloc
BackgroundData:
	.byte $f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5
	.byte $f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5
	.byte $f5,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$f5,$f4,$f4,$f4,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f5,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$f5
	.byte $f5,$f5,$f4,$f4,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5
	.byte $f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5
	.byte $f5,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$ca,$cb,$f5,$ed
	.byte $e2,$e6,$de,$f5,$ec,$e9,$de,$e7,$ed,$f5,$e2,$e7,$f5,$cb,$ca,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$fc,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$fc,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$ca,$cb,$cb,$cb
	.byte $cb,$f5,$e3,$e8,$ee,$eb,$e7,$de,$f2,$f5,$cb,$cb,$cb,$cb,$ca,$f5
	.byte $f5,$f4,$c9,$f6,$f4,$f4,$fa,$f6,$f4,$f5,$f5,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$f4,$f8,$f6,$f4,$f4,$96,$fc,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$ca,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$ca,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$dd,$de,$da,$ed,$e1,$ec,$cf,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$f5,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$eb,$de,$ec,$de,$ed,$ec,$cf,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$f5,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$e1,$e2,$ec,$ed,$da,$db,$cf,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$f4,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$e5,$e8,$ec,$ed,$da,$db,$cf,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$f4,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$ee,$e9,$ec,$ed,$da,$db,$cf,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$f4,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$dd,$f0,$ec,$ed,$da,$db,$cf,$f4,$f4,$f4,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$cc,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$cc,$f4,$f4,$f4
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
	.byte $f5,$ca,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$ca,$cb,$cb,$cb
	.byte $cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$cb,$ca,$f5
	.byte $f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5
	.byte $f4,$f4,$f4,$f4,$f4,$f4,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5,$f5
	.byte $00,$00,$00,$00,$00,$00,$00,$00,$44,$11,$00,$00,$00,$00,$00,$00
	.byte $88,$aa,$22,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00
	.byte $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00
	.byte $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00

.reloc
PPUBufferSrcLo:
.repeat ARRAY_INDEX, I
    .byte .lobyte(.ident(.concat("ARRAY_SRC_", .string(I))))
.endrepeat
.reloc
PPUBufferSrcHi:
.repeat ARRAY_INDEX, I
    .byte .hibyte(.ident(.concat("ARRAY_SRC_", .string(I))))
.endrepeat
.reloc
PPUBufferSrcLen:
.repeat ARRAY_INDEX, I
    .byte .lobyte(.ident(.concat("ARRAY_SRCLEN_", .string(I))))
.endrepeat
.reloc
PPUBufferDstLo:
.repeat ARRAY_INDEX, I
    .byte .lobyte(.ident(.concat("ARRAY_DST_", .string(I))))
.endrepeat
.reloc
PPUBufferDstHi:
.repeat ARRAY_INDEX, I
    .byte .hibyte(.ident(.concat("ARRAY_DST_", .string(I))))
.endrepeat
.reloc
PPUBufferDstLen:
.repeat ARRAY_INDEX, I
    .byte .lobyte(.ident(.concat("ARRAY_DSTLEN_", .string(I))))
.endrepeat
