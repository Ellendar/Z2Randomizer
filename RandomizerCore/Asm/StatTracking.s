
.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG

.define StatDisplayState $130
.define InternalState $131

BUFFER_OFF = $301
PPUADDR_HI = $302
PPUADDR_LO = $303
BUFFER_LEN = $304
BUFFER_DAT = $305

DOT = $cf
SPACE = $f4

CONVERT = $80

; TODO: Add patches to update stats

.segment "PRG7"

;;;
; Writes the current timestamp to SRAM.
; [In a] - index of the type of timestamp to write
; Notice this does NOT preserve registers!
.reloc
.export AddTimestamp
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

.reloc
UpdateCreditsSwitchBank:
    ; This is a bit lame, but while we have BANK5 banked in,
    ; lets copy over the seed hash into RAM somewhere we can access
    ldy #11-1
    -   lda $BC1C, y
        sta $03d5, y
        dey
        bpl -
    lda #1
    jsr SwapPRG
    jmp UpdateCredits

.reloc
.import AddExtraRowOfItems
DrawPauseMenuRowSwitchBank:
    lda #0
    jsr SwapPRG
    jsr AddExtraRowOfItems
    lda #1
    jmp SwapPRG

.reloc
Experience_Convertion_and_Display_Routine = $A5A4
; returns the tiles for 4 digits in $01, $00, y, $02
TwoByteBCDConversion:
    lda #0
    jsr SwapPRG
    lda $01 ; lo byte of value. $00 has hi byte
    jsr Experience_Convertion_and_Display_Routine
    ; digits out in order: 1 0 y a
    sta $02
    lda #1
    jmp SwapPRG

.segment "PRG5"

RestartAfterCredits = $921C
LoadSaveFile = $B911
LoadSaveFileCopyPointers = $BA6C
LoadSaveFileCopyPointersSlotA = $BA6F


.org $B2B9
    jsr CheckIfNewSaveFile

.reloc
CheckIfNewSaveFile:
    lda StatTrackingSaveFileClear
    beq +
        ; New save file loaded, so clear stats entirely
        ldy #STAT_TRACKING_SIZE
        lda #0
        -
            sta StatTimer-1,y
            dey
            bne -
        sta StatTrackingSaveFileClear
    +
    jmp LoadSaveFile

.org $B45E
    jsr SetNewSaveFile

.reloc
SetNewSaveFile:
    ; a = 1
    sta $0736
    sta StatTrackingSaveFileClear ; 1 = new file, 0 = old file 
    rts

.org $B931
    jsr LoadStatsFromCheckpoint

.org $B9F5
    jsr SaveStatsToCheckpoint

.reloc
SaveStatsToCheckpoint:
  ldy #CHECKPOINT_LEN-1
-   lda Checkpoint,y
    sta Checkpoint+CHECKPOINT_LEN,y
    dey
    bpl -
  jmp LoadSaveFileCopyPointers

.reloc
LoadStatsFromCheckpoint:
  ldy #CHECKPOINT_LEN-1
-   lda Checkpoint+CHECKPOINT_LEN,y
    sta Checkpoint,y
    dey
    bpl -
  jmp LoadSaveFileCopyPointers

; Hook into the final step before restarting to display our new credits scene
.org $8C01
.word (UpdateCreditsSwitchBank)

; Hook the main PRG5 entrypoint to turn off the palette cycle for the triforce
.org $8BBF
    jsr DisablePaletteCycle
    bcs $8BE1

.reloc
DisablePaletteCycle:
    ; sets the carry if the main state is >= $c
    lda $0761
    cmp #$0d
    ; perform the patched instructions
    ldx BUFFER_OFF
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
.word (DrawAllTimestamps)
.word (FadeIn)
.word (WaitForStart)
.word (FadeOut)
.word (RestartToTitle)

.reloc
DrawStats:
    inc StatDisplayState
    rts

.reloc
RestartToTitle:
    inc $0761
    rts

.reloc
RenderPlayerInfo:
    lda InternalState
    beq +
        jmp RenderPlayerInfoBg
    +
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
    jmp RenderPlayerInfoBg

.reloc
RenderPlayerInfoBg:
    ldx BUFFER_OFF
    ; Setup the read ptr
    lda #<PlayerInfoCommandList
    sta $04
    lda #>PlayerInfoCommandList
    sta $05
    lda #6
    sta $07
    ; Setup the next VRAM write
@VramLoop:
        ldy InternalState
        ; check if we've run out of things to render
        cpy #PlayerInfoCommandListLen
        bcc +
            inc StatDisplayState
            jmp @Exit
        +
        ; Setup the source ptr
        lda ($04),y
        iny
        sta $02
        lda ($04),y
        iny
        sta $03
        ; Setup the buffer address to write to
        lda ($04),y
        iny
        sta PPUADDR_LO,x
        lda ($04),y
        iny
        sta PPUADDR_HI,x
    
        ; Setup the src/dst lengths
        lda ($04),y
        iny
        cmp #$80
        and #$7f
        sta $06
        lda ($04),y
        iny
        sta BUFFER_LEN,x
        sty InternalState
        ldy #0
        bcs @ReadWithConvert
        ; direct read
        ; Read all bytes from source byte into $302,x
        -   lda ($02),y
            sta BUFFER_DAT,x
            iny
            inx
            dec $06
            bne -
        jmp @NextIteration0
    @ReadWithConvert:
        ; check if we are doing a single digit convert or a multi digit
        lda ($02),y
        iny
        sta $01
        lda $06
        cmp #1
        beq @CheckForSingleConvert
            lda ($02),y
            iny
            sta $00
            jmp @ConvertMulti
    @CheckForSingleConvert:
        lda BUFFER_LEN,x
        cmp #2
        bcs @ConvertMulti
            ; Do a very simple conversion of the data from dest for a single digit
            ; this keeps the "F" lives display working as expected
            lda $01
            clc
            adc #$d0
            sta BUFFER_DAT,x
            jmp @NextIteration1
    @ConvertMulti:
        tya
        pha
            lda BUFFER_LEN,x
            cmp #2
            bcc +
            jsr TwoByteBCDConversion
            lda $02
            sta BUFFER_DAT+2,x
            lda BUFFER_LEN,x
            cmp #2
            bcc +
                tya
                sta BUFFER_DAT+1,x
                lda BUFFER_LEN,x
                inx
                cmp #3
                bcc +
                    lda $00
                    sta BUFFER_DAT-1,x ; -1 to account for an extra inx
                    inx
        +
        pla
        tay
    @NextIteration1:
        inx
    @NextIteration0:
        inx
        inx
        inx
        dec $07
        beq @Exit
        jmp @VramLoop
@Exit:
    lda #$ff
    sta PPUADDR_HI,x
    stx BUFFER_OFF
    rts


.reloc
; Source, Dest, SrcLen, DstLen
; Technically I don't need src/dst len but I can reuse the function to render both
PlayerInfoCommandList:
; Player Name
.word $07A1, $2062
.byte 8, 8
; Seed Hash (copied to $3d5 every frame cause its cheap :p)
.word $03d5, $202a
.byte 11, 11
; Data that should be converted to base 10
; Heart Containers
.word $0784, $20a9
.byte 1 | CONVERT, 1
; Magic Containers
.word $0783, $20e9
.byte 1 | CONVERT, 1
; Atk
.word $0777, $21a4
.byte 1 | CONVERT, 1
; Lif
.word $0779, $21c4
.byte 1 | CONVERT, 1
; Mag
.word $0778, $21a8
.byte 1 | CONVERT, 1
; Lives
.word $0700, $21c8
.byte 1 | CONVERT, 1
; Deaths
.word $079F, $2209
.byte 1 | CONVERT, 3
; Resets
.word $079F, $2249
.byte 1 | CONVERT, 3
; Hi Stab
.word $0778, $2289
.byte 2 | CONVERT, 3
; Lo Stab
.word $0700, $22c9
.byte 2 | CONVERT, 3
; Up Stab
.word $0778, $2309
.byte 2 | CONVERT, 3
; Dw Stab
.word $0700, $2349
.byte 2 | CONVERT, 3
PlayerInfoCommandListLen = * - PlayerInfoCommandList

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
    lda #4 * 8
    sta $200 + $50
    sta $200 + $54
    ; magic container y
    lda #6 * 8
    sta $200 + $58
    sta $200 + $5c
    ; link sprite offset
    lda #$70
    sta $90
    ; link x offset
    lda #$08+4
    sta $cc
    ; link y offset
    lda #$20
    sta $29
    ; link metasprite
    lda #3
    sta $80
    jmp $EC02 ; Draw Link based on metasprite
;    rts

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
DisableRendering:
    ; disable rendering
    lda #$40
    sta $100
    lda $FE
    and #$e0
    sta PPUMASK
    rts
EnableRendering:
    ; re-enable rendering
    lda #$c0
    sta $100
    lda $FE
    ora #$1E
    sta PPUMASK
    rts
.reloc
DrawBackground:
    jsr DisableRendering
@Loop:
    ; Since we don't have any unplanned ppu updates, we can just use
    ; hardcoded offsets into the ppu buffer to simplify the code
    lda #0
    sta $00
    lda InternalState
    jsr Multiply32
    lda #0
    jsr SetPPUAddr
    
    ; setup the read ptr
    lda #<BackgroundData
    clc
    adc $00
    sta $00
    lda #>BackgroundData
    adc $01
    sta $01
    
    ldy #0
    -   lda ($00),y
        sta PPUDATA
;        sta BUFFER_DAT + 3,y ; +3 to account for the header
        iny
        cpy #$20
        bne -
    lda #$ff
    sta PPUADDR_HI
    inc InternalState
    lda InternalState
    cmp #$20
    bne @Loop
    
    ; Done with drawing the background
    inc StatDisplayState
    jmp EnableRendering
;    rts

.reloc
Multiply32:
    pha
        lda #0
        sta $01
    pla
    ; Mult by 6
    asl
    rol $01
    asl
    rol $01
    asl
    rol $01
    asl
    rol $01
    asl
    rol $01
    sta $00
    rts

.reloc
SetPPUAddr:
    clc
    adc $00
    pha
        lda $01
        adc #$20
        sta PPUADDR
    pla
    sta PPUADDR
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
    ldy BUFFER_OFF
    lda #$3f
    sta PPUADDR_HI,y
    sta PPUADDR_HI + 4,y
    lda RandomOrderTable,x
    sta PPUADDR_LO,y
    ora #$10
    sta PPUADDR_LO + 4,y
    lda #1
    sta BUFFER_LEN,y
    sta BUFFER_LEN + 4,y
    lda #$0f
    sta BUFFER_DAT,y
    sta BUFFER_DAT + 4,y
    lda #$ff
    sta PPUADDR_HI + 8,y
    ; Move to the next
    inc InternalState
    ; and clear out the sprite zero memory while we are at it :p
    lda #$f8
    sta $200
    rts


.reloc

SprPaletteTable = $80AE
FadeIn:
    ldx InternalState
    ; Every 16 frames step to the next palette
    lda $12 ; global timer
    and #$01
    beq :>rts
    cpx #12
    bne +
        ; Fade out complete so exit
        inc StatDisplayState
        rts
    +
    ldy BUFFER_OFF
    lda #$3f
    sta PPUADDR_HI,y
    sta PPUADDR_HI+4,y
    lda RandomOrderTable,x
    tax
    sta PPUADDR_LO,y
    ora #$10
    sta PPUADDR_LO+4,y
    lda #1
    sta BUFFER_LEN,y
    sta BUFFER_LEN+4,y
    lda BGPaletteTable,x
    sta BUFFER_DAT,y
    lda SprPaletteTable,x
    sta BUFFER_DAT+4,y
    lda #$ff
    sta PPUADDR_HI + 8,y
    ; Move to the next color
    inc InternalState
    rts
BGPaletteTable:
; Border
.byte $0f, $30, $12, $16
; Red Text ?
.byte $0f, $0f, $0f, $0f
; Blue Text ?
.byte $0f, $0f, $0f, $0f
; unused
.byte $0f, $0f, $0f, $0f

;; Long division routine copied from here:
;; https://codebase64.org/doku.php?id=base:24bit_division_24-bit_result

.define Remainder  <$60
.define Dividend   <$63
.define Divisor    <$66
.define DivTmp     <$69

.define Hex0        <$70
.define DecOnes     <$71
.define DecTens     <$72
.define DecHundreds <$73

.define TmpHoursTens   <$74
.define TmpHoursOnes   <$75
.define TmpMinutesOnes <$76
.define TmpMinutesTens <$77
.define TmpSecondsOnes <$78
.define TmpSecondsTens <$79

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
DrawAllTimestamps:
    jsr DisableRendering
    ; Draw the Time Spent In time stamps
    lda #8
    sta $c0
    lda #0
    sta $c1
@DrawTimeSpentLoop:
        ldy $c1
        lda @TimeSpentInTable,y
        tay
        jsr LoadNamePointer
        
        ; Calculate PPU address to draw to
        lda $c1
        jsr Multiply32
        lda #$8d
        jsr SetPPUAddr

        ; Get the read offset for this element
        lda $c1
        asl
        clc
        adc $c1
        tax
        ; read the 3 digits of the timestamp into the divide memory
        lda StatTimeInTowns,x
        sta Dividend
        lda StatTimeInTowns+1,x
        sta Dividend+1
        lda StatTimeInTowns+2,x
        sta Dividend+2

        jsr RenderTimestamp
        
        inc $c1
        lda $c1
        cmp $c0
        beq @NextStep
        jmp @DrawTimeSpentLoop
    
    ; Now draw the Journey
@NextStep:
    lda TimestampCount
    bne +
        ; Sanity check that they have any timestamps. Really only possible with hacks.
        jmp @Exit
    +
    sta $c0
    lda #0
    sta $c1

@DrawJourneyLoop:
        ; Get the read offset for this element
        ldy $c1
        ; Get the type of timestamp
        lda TimestampTypeList, y
        jsr LoadNamePointer
        
        ; We premultiply the timestamp by 16 to try and get a little more precision
        ; to deal with the fact that NES frame rate is closer to 60.1 FPS
        lda $c1
        jsr Multiply32
        inc $01
        inc $01
        lda #$8d
        jsr SetPPUAddr
        ; Get the read offset for this element
        ldy $c1
        ; Get the type of timestamp
        lda TimestampTypeList, y
        ; multiply i * 3 to get offset into timestamp list
        asl
        adc TimestampTypeList, y
        tax
        ; read the 3 digits of the timestamp into the divide memory
        ; We premultiply the timestamp by 16 to try and get a little more precision
        ; to deal with the fact that NES frame rate is closer to 60.1 FPS
        lda TimestampList,x
        sta Dividend
        lda TimestampList+1,x
        sta Dividend+1
        lda TimestampList+2,x
        sta Dividend+2
        jsr RenderTimestamp
    
        inc $c1
        lda $c1
        cmp $c0
        beq @Exit
        jmp @DrawJourneyLoop
@Exit:
    inc StatDisplayState
    jmp EnableRendering
;  rts

@TimeSpentInTable:
    .byte TsTowns
    .byte TsPalace1
    .byte TsPalace2
    .byte TsPalace3
    .byte TsPalace4
    .byte TsPalace5
    .byte TsPalace6
    .byte TsPalaceGP

.reloc
LoadNamePointer:
    ; multiply i * 8 to get the offset into the timestamp name list
    asl
    asl
    asl
    ; a = index into name list
    clc
    adc #<TsNameList
    sta $c4
    lda #>TsNameList
    adc #0
    sta $c5
    rts

.reloc
RenderTimestamp:
  ; copy name of timestamp
  ldy #0
@CopyNameLoop:
    lda ($c4),y
    sta PPUDATA
    iny
    cpy #8
    bne @CopyNameLoop

  lda #SPACE
  sta PPUDATA

  ; mult the dividend by 16
  jsr Multiply16

  ; 60.1 frm/sec * 60 sec/min * 60 min/hr = 216360 or 34d28 in hex
  ; multiplied by 16 is 34d280
  lda #$80
  sta Divisor
  lda #$d2
  sta Divisor+1
  lda #$34
  sta Divisor+2
  jsr LongDivision24bit
  ; results in Dividend, Remainder will be divided again to find minutes
;  jsr Divide16
  lda Dividend
  sta $01
  lda Dividend+1
  sta $00
  jsr TwoByteBCDConversion
;  sta Hex0
;  jsr HexToDecimal8Bit
  ; if theres a ten's place for the hours, then just write 9:99:99
;  lda DecTens
  sty TmpHoursTens
  lda $02
  sta TmpHoursOnes

  ; Update the minutes
  lda Remainder
  sta Dividend
  lda Remainder + 1
  sta Dividend + 1
  lda Remainder + 2
  sta Dividend + 2
  ; 60.1 frm/sec * 60 sec/min = 3606 or 0e16 in hex * 16 = e160
  lda #$60
  sta Divisor
  lda #$e1
  sta Divisor+1
  lda #$00
  sta Divisor+2
  jsr LongDivision24bit
;  jsr Divide16

  lda Dividend
  sta $01
  lda Dividend+1
  sta $00
  jsr TwoByteBCDConversion
;  sta Hex0
;  jsr HexToDecimal8Bit
;  lda DecTens
  sty TmpMinutesTens
  lda $02
  sta TmpMinutesOnes

  ; update the seconds
  lda Remainder
  sta Dividend
  lda Remainder + 1
  sta Dividend + 1
  lda Remainder + 2
  sta Dividend + 2
  ; 60.1 frm/sec * 16 = 960 or $3c0 in hex
  lda #$c0
  sta Divisor
  lda #$03
  sta Divisor+1
  lda #$00
  sta Divisor+2
  jsr LongDivision24bit
;  jsr Divide16

  lda Dividend
  sta $01
  lda Dividend+1
  sta $00
  jsr TwoByteBCDConversion
  sty TmpSecondsTens
  lda $02
  sta TmpSecondsOnes

  ; Finally draw write the numbers to the screen
@WriteNumber:
  lda TmpHoursTens
  sta PPUDATA
  lda TmpHoursOnes
  sta PPUDATA
  lda #DOT
  sta PPUDATA
  lda TmpMinutesTens
  sta PPUDATA
  lda TmpMinutesOnes
  sta PPUDATA
  lda #DOT
  sta PPUDATA
  lda TmpSecondsTens
  sta PPUDATA
  lda TmpSecondsOnes
  sta PPUDATA
  rts

.reloc
Multiply16:
  asl Dividend
  rol Dividend+1
  rol Dividend+2
  asl Dividend
  rol Dividend+1
  rol Dividend+2
  asl Dividend
  rol Dividend+1
  rol Dividend+2
  asl Dividend
  rol Dividend+1
  rol Dividend+2
  rts
;.reloc
;Divide16:
;  lsr Dividend+2
;  ror Dividend+1
;  ror Dividend
;  lsr Dividend+2
;  ror Dividend+1
;  ror Dividend
;  lsr Dividend+2
;  ror Dividend+1
;  ror Dividend
;  lsr Dividend+2
;  ror Dividend+1
;  ror Dividend
;  rts

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
