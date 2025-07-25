
.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG

BUFFER_OFF = $0301
PPUADDR_HI = $0302
PPUADDR_LO = $0303
BUFFER_LEN = $0304
BUFFER_DAT = $0305

DOT = $cf
SPACE = $f4

CONVERT = $80

.segment "PRG7"
.reloc
LinkJustDied:
    inc StatDeaths
    inc $0494
    rts

; Update all locations that sets $0494 - link just died
.org $E349
    jsr LinkJustDied

.segment "PRG0"
.org $9199
    jsr LinkJustDied
.segment "PRG5"
.org $A279
    jsr LinkJustDied

.segment "PRG0"
; The game essentially "resets" the metasprite each frame at the start,
; so we patch it to store elsewhere for checking if we downstabbed.
.org $949b ; sta $80 rts
    jmp SetPreviousFrameAction
.reloc
SetPreviousFrameAction:
    ldy $80
    sty PreviousFrameMetasprite
    sta $80
    rts

.org $95FC ; sty $0400
    jsr StatTrackSwordSwipe
.reloc
StatTrackSwordSwipe:
    sty $0400
    ; Need to use X so we need to preserve it
    txa
    tay
    lda $17 ; 1 if crouching 0 if not
    asl
    tax
    inc StatHiStabCount+0,x
    bne @Exit
    inc StatHiStabCount+1,x
    tya
    tax
@Exit:
    rts

.org $9586 ; bmi $958A lda #$02
    jsr StatTrackUpStab
    nop

.reloc
StatTrackUpStab:
    ; At this point its decided we will upstab, but if we are falling we cant
    ; a = upstab anim, y = links y velocity, so if its falling, do the original lda #2
    bpl @Falling
        ; we are rising, check for upstab
        ; We are actually upstabing frfr so now we can stat track it
        ; check if we are on cooldown since the game doesn't have a cooldown builtin
        ; we do this by checking if the previous animation frame was an upstab
        cmp #8
        bne @Exit ; not an upstab
        cmp PreviousFrameMetasprite
        beq @Exit ; not a new upstab
        ldy $0400 ; current stab timer
        bne @Exit ; upstab canceled by regular stab
            inc StatUpStabCount+0
            bne @Exit
            inc StatUpStabCount+1
        jmp @Exit
@Falling:
    lda #2
@Exit:
    rts

.org $9597 ; lda #$09 sta $80
   jsr StatTrackDownStab
   nop
.reloc
StatTrackDownStab:
    lda #9
    cmp PreviousFrameMetasprite
    beq @NotNewDownstab
    ldy $0400 ; current stab timer
    bne @Exit ; downstab canceled by regular stab
        inc StatDownStabCount+0
        bne @Exit
        inc StatDownStabCount+1
@NotNewDownstab:
@Exit:
    sta $80
    rts
    

.segment "PRG4"

.org $9B0F ; dec $0794
    jsr SaveTimestampForPalace

.reloc
SaveTimestampForPalace:
    dec $0794
    lda RegionNumber
    asl
    asl
    adc PalaceNumber
    tay
    lda PalaceTable,y
    jmp AddTimestamp

.reloc
PalaceTable:
    ; region 0 - east hyrule
    .byte RealPalaceAtLocation1 + TsPalace1
    .byte RealPalaceAtLocation2 + TsPalace1
    .byte RealPalaceAtLocation3 + TsPalace1
    .byte $ff ; unused 4th palace in region 0
    ; region 1 - death mountain 
    .byte $ff ; unused 1st palace in region 1
    .byte $ff ; unused 2nd palace in region 1
    .byte $ff ; unused 3th palace in region 1
    .byte $ff ; unused 4th palace in region 1
    ; region 2 - west hyrule
    .byte RealPalaceAtLocation5 + TsPalace1
    .byte RealPalaceAtLocation6 + TsPalace1
    .byte RealPalaceAtLocationGP+ TsPalace1
    .byte $ff ; unused 4th palace in region 2
    ; region 3 - maze island
    .byte RealPalaceAtLocation4 + TsPalace1

.segment "PRG7"

.reloc
.export CheckAddItemTimestamp
CheckAddItemTimestamp:
    ; Check if the item is tracked and add timestamp if it is
    ; Item is in A, we must preserve X
    pha
        tay
        lda ItemIDToTimestampId,y
        bmi @NotTrackingItem ; Item is not tracked
.if _ALLOW_ITEM_DUPLICATES
            ldy TimestampCount
            beq @AddItemToTracker
                @loop:
                cmp TimestampTypeList - 1,y
                beq @NotTrackingItem ; Item is already tracked
                dey
                bne @loop
.endif
            @AddItemToTracker:
            jsr AddTimestamp
@NotTrackingItem:
    pla
    tay
    cpy #8
    rts

.reloc
ItemIDToTimestampId:
    .byte $ff ; ITEM_CANDLE
    .byte TsGlove ; ITEM_GLOVE
    .byte TsRaft ; ITEM_RAFT
    .byte TsBoots ; ITEM_BOOTS
    .byte $ff ; ITEM_FLUTE
    .byte $ff ; ITEM_CROSS
    .byte TsHammer ; ITEM_HAMMER
    .byte $ff ; ITEM_MAGIC_KEY
    .byte $ff ; ITEM_KEY
    .byte $ff ; ITEM_DO_NOT_USE
    .byte $ff ; ITEM_SMALL_PBAG
    .byte $ff ; ITEM_MEDIUM_PBAG
    .byte $ff ; ITEM_LARGE_PBAG
    .byte $ff ; ITEM_XL_PBAG
    .byte $ff ; ITEM_MAGIC_CONTAINER
    .byte $ff ; ITEM_HEART_CONTAINER
    .byte $ff ; ITEM_BLUE_MAGIC_JAR
    .byte $ff ; ITEM_RED_MAGIC_JAR
    .byte $ff ; ITEM_1UP
    .byte $ff ; ITEM_CHILD
    .byte $ff ; ITEM_TROPHY
    .byte $ff ; ITEM_MEDICINE
    .byte $ff ; ITEM_DO_NOT_USE_ANTIFAIRY
    .byte $ff ; ITEM_UPSTAB
    .byte $ff ; ITEM_DOWNSTAB
    .byte $ff ; ITEM_BAGU
    .byte $ff ; ITEM_MIRROR
    .byte $ff ; ITEM_WATER
    .byte $ff ; ITEM_SHIELD_SPELL
    .byte TsJumpSpell ; ITEM_JUMP_SPELL
    .byte $ff ; ITEM_LIFE_SPELL
    .byte TsFairySpell ; ITEM_FAIRY_SPELL
    .byte $ff ; ITEM_FIRE_SPELL
    .byte TsReflectSpell ; ITEM_REFLECT_SPELL
    .byte $ff ; ITEM_SPELL_SPELL
    .byte TsThunderSpell ; ITEM_THUNDER_SPELL
    .byte $ff ; ITEM_DASH_SPELL

;;;
; Writes the current timestamp to SRAM.
; [In a] - index of the type of timestamp to write
; Notice this does NOT preserve registers!
.reloc
.export AddTimestamp
AddTimestamp:
  ldy TimestampCount
  sta TimestampTypeList,y
  asl ; clears carry
  adc TimestampTypeList,y
  tay
  ; double read the timestamp. this prevents an issue where reading is interrupted by NMI
  ; keep the lo value in X so we can check if it changed and read again if it does
@validate:
    lda StatTimer+0
    pha
        sta TimestampList,y
        lda StatTimer+1
        sta TimestampList+1,y
        lda StatTimer+2
        sta TimestampList+2,y
    pla
    cmp StatTimer+0
    bne @validate
  inc TimestampCount
  rts

.reloc
UpdateCreditsSwitchBank:
    ; This is a bit lame, but while we have BANK5 banked in,
    ; lets copy over the seed hash into RAM somewhere we can access
    ldy #11-1
@hash:
        lda $BC1C, y
        sta $03d5, y
        dey
        bpl @hash
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
ITEM_SPRITE_OFFSET = 4
RestartAfterCredits = $921C
LoadSaveFile = $B911
LoadSaveFileCopyPointers = $BA6C
LoadSaveFileCopyPointersSlotA = $BA6F
bank7_Display = $ef11

; Add a little blurb about press start to skip to the stats
.org $8BB0 ; JMP      L9255 - jumps to inc $0736 rts
    jmp AddPressStartToSkip
.reloc
AddPressStartToSkip:
    inc $0736
    
    ; rendering is off so we can just draw whatever we want
    lda #$23
    sta PPUADDR
    lda #$63
    sta PPUADDR
    ldy #0
@loop:
        lda PressStartString,y
        sta PPUDATA
        iny
        cpy #PressStartStringLen
        bne @loop
    rts

; Set the final timer stop command when you stop being able to move
; also show the triforce immediately to show when the timer has ended
.org $9B57 ; ldy $0504
    jsr StopTimers
.reloc
StopTimers:
    lda GameComplete
    bmi @AlreadyDoneOnce
        ora #$80
        sta GameComplete
        ; Set the timestamp for GP
        lda #TsPalaceGP
        jsr AddTimestamp
@AlreadyDoneOnce:
    ; setup and draw the old man over an over
    lda #$d0
    sta $4e  ;monster x
    lda #$50
    sta $2a  ;monster y
    lda #$cf
    sta $cd  ;monster position on screen (fixes triforce position on first frame)
    jsr bank7_Display ; its the credits who cares about a few cycles
    lda #$03
    ldy $0504
    rts

; Patch the save check on restart to copy over from checkpoint since we
; "lost" whatever timestamps we had after a reset
.org $B963
    jsr PatchLoadStatsFromCheckpoint
.reloc
PatchLoadStatsFromCheckpoint:
    jsr LoadStatsFromCheckpoint
    txa ; This is run for each save file and X has the save file index
    jmp LoadSaveFileCopyPointersSlotA

; Hook the save file load routine to see if our special flag is set
; If its set, then this is the first time its loaded, so we reset all stat tracking data
; If its not set, then we do nothing (ie: the player reset the console during a playthrough)
.org $B2B9
    jsr CheckIfNewSaveFile

.reloc
CheckIfNewSaveFile:
    lda StatTrackingSaveFileClear
    beq @skip
        ; New save file loaded, so clear stats entirely
        ldy #STAT_TRACKING_SIZE
        lda #0
    @loop:
            sta StatTimer-1,y
            dey
            bne @loop
        sta StatTrackingSaveFileClear
@skip:
    jmp LoadSaveFile

; Patch the "Register" button to set a flag for a new save file
; NOTE: This will set the flag to clear the stats whenever any new file is registered
; meaning registering a file after starting a run will reset your stats!
;.org $B45E ; this is the "Elimination mode" end button
.org $B67F ; this is the actual Register button.
    jsr SetNewSaveFile

.reloc
SetNewSaveFile:
    ; a = 6
    sta $0725 ; sets the next screen to draw
    sta StatTrackingSaveFileClear ; nonzero = new file, 0 = old file
    rts

; Patch a few save/load locations in the game to also update the timestamp checkpoints
.org $B931
    jsr LoadStatsFromCheckpoint

.org $B9F5
    jsr SaveStatsToCheckpoint

.reloc
SaveStatsToCheckpoint:
  ldy #CHECKPOINT_LEN-1
@loop:
    lda TimestampCount,y
    sta Checkpoint,y
    dey
    bpl @loop
  jmp LoadSaveFileCopyPointers

.reloc
LoadStatsFromCheckpoint:
  ldy #CHECKPOINT_LEN-1
@loop:
    lda Checkpoint,y
    sta TimestampCount,y
    dey
    bpl @loop
  jmp LoadSaveFileCopyPointers

; Hook the common runtime code for the credits to allow skipping to the end at any time
.org $8BE1
    jsr CheckToSkipToEnd
.reloc
CheckToSkipToEnd:
    ; check if start is pressed
    lda $f5
    and #$10
    beq @skip
        ; don't let this skip code run once we've moved to the stats
        lda $0761
        ; the step right before the final scene with the blinking triforce
        ; since the blinking triforce scene already has a press start check
        cmp #12
        bcs @skip
            ; set the music to the final end song
            lda #8
            sta $eb
            ; skip ahead
            lda #13
            sta $0761
@skip:
    lda $0761
    rts

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
    beq @skip
        ; Reset the internal state after every state ends
        lda #0
        sta InternalState
@skip:
    jmp SwapToSavedPRG

RunFrame:
    ; a == StatDisplayState
    cmp #4
    bcc @skip
        jsr UpdateSpritePosition
@skip:
    lda StatDisplayState
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
    beq @skip
        jmp RenderPlayerInfoBg
@skip:
    ; Render the sprites
    jsr DrawPauseMenuRowSwitchBank

    ; Move those sprites to the position that we want them at
    ldx #16-1
    ldy #(16-1)*4
@loop:
        lda $0200 + ITEM_SPRITE_OFFSET,y
        ; Don't move offscreen sprites over
        cmp #$f0
        bcs @offscreen
            sec 
            sbc #$80-10
    @offscreen:
        sta $0200 + ITEM_SPRITE_OFFSET,y
        sta $0390,x
        lda $0203 + ITEM_SPRITE_OFFSET,y
        sec 
        sbc #$80+8
        sta $0203 + ITEM_SPRITE_OFFSET,y
        sta $03b0,x
        dey
        dey
        dey
        dey
        dex
        bpl @loop
    ; Create the heart and jar sprite
    ; ignore the y pos for now. we'll add it in the update
    ; x pos
    lda #6 * 8 - 1
    sta $0203 + $60
    sta $0203 + $68
    lda #7 * 8 - 1
    sta $0203 + $64
    sta $0203 + $6c
    ; tile id
    lda #$81
    sta $0201 + $60
    sta $0201 + $64
    lda #$83
    sta $0201 + $68
    sta $0201 + $6c
    ; attr
    lda #$01
    sta $0202 + $60
    sta $0202 + $68
    lda #$41
    sta $0202 + $64
    sta $0202 + $6c
    
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
        ; clear out $00 before we start processing numbers
        ldy #0
        sty $00
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
            sta $03
            jsr TwoByteBCDConversion
            ; digit 4
            lda $03 ; write len
            cmp #4
            bcc @digit3
                lda $01
                cmp #$d0
                bne @notzero
                    lda #DOT ; use a dot instead if the 4'th number is zero
            @notzero:
                sta BUFFER_DAT,x
            inx
        @digit3:
            lda $03 ; write len
            cmp #3
            bcc @digit2
                lda $00
                sta BUFFER_DAT,x
                inx
        @digit2:
            lda $03 ; write len
            cmp #2
            bcc @digit1
                tya
                sta BUFFER_DAT,x
                inx
        @digit1:
            lda $02
            sta BUFFER_DAT,x
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
.word StatDeaths, $2209
.byte 1 | CONVERT, 3
; Resets
.word $079F, $2249
.byte 1 | CONVERT, 3
; Hi Stab
.word StatHiStabCount, $2288
.byte 2 | CONVERT, 4
; Lo Stab
.word StatLoStabCount, $22c8
.byte 2 | CONVERT, 4
; Up Stab
.word StatUpStabCount, $2308
.byte 2 | CONVERT, 4
; Dw Stab
.word StatDownStabCount, $2348
.byte 2 | CONVERT, 4
PlayerInfoCommandListLen = * - PlayerInfoCommandList

.reloc
UpdateSpritePosition:
    ldy #(16-1)*4
    ldx #16-1
-       lda $390,x
        sta $200 + ITEM_SPRITE_OFFSET,y
        lda $3b0,x
        sta $203 + ITEM_SPRITE_OFFSET,y
        dey
        dey
        dey
        dey
        dex
        bpl -
    ; heart container y
    lda #4 * 8
    sta $200 + $60
    sta $200 + $64
    ; magic container y
    lda #6 * 8
    sta $200 + $68
    sta $200 + $6c
    ; link sprite offset
    lda #$80
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
.byte 0, 3, 14, 5, 10, 1, 7, 13, 15, 2, 9, 6, 11
PALETTE_FADE_LEN = * - RandomOrderTable 

.reloc
FadeOut:
    ; Every 16 frames step to the next palette
    lda $12 ; global timer
    and #$01
    beq :>rts
    ldx InternalState
    cpx #PALETTE_FADE_LEN
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
        ; start drawing from ppu addr $208d
        lda #$8d
        jsr SetPPUAddr

        ; Get the read offset for this element
        lda $c1
        asl
        clc
        adc $c1
        tax
        ; read the 3 digits of the timestamp into the divide memory
        lda StatTimeAtLocation,x
        sta Dividend
        lda StatTimeAtLocation+1,x
        sta Dividend+1
        lda StatTimeAtLocation+2,x
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
        ; start drawing from ppu addr $21ad
        inc $01
        lda #$ad
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
    .byte $f5,$cc,$dd,$de,$da,$ed,$e1,$ec,$cf,$d0,$d0,$d0,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$f5,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$eb,$de,$ec,$de,$ed,$ec,$cf,$d0,$d0,$d0,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$f5,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$e1,$e2,$ec,$ed,$da,$db,$cf,$d0,$d0,$d0,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$f4,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$e5,$e8,$ec,$ed,$da,$db,$cf,$d0,$d0,$d0,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$f4,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$ee,$e9,$ec,$ed,$da,$db,$cf,$d0,$d0,$d0,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$f4,$f5,$f5,$f5,$f5,$f5,$f4,$f4,$f4,$f4,$cc,$f4,$f4,$f4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$cc,$f5
    .byte $f5,$cc,$dd,$f0,$ec,$ed,$da,$db,$cf,$d0,$d0,$d0,$cc,$f4,$f4,$f4
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
