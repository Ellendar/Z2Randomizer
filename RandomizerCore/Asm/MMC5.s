﻿.include "z2r.inc"

.export SwapPRG, SwapToSavedPRG, SwapToPRG0

FREE "PRG7" [$FEAA, $FFE8)

.segment "HEADER"
.org $04
.byte $10 ; 256 KB PRG-ROM

.org $06
.byte $52 ; Mapper 5
.byte $08 ; use NES 2.0 header

.org $0a
.byte $70 ; reserve 8kb of PRG NVRAM (SRAM)

.segment "PRG7"

; Replacements for the NMI and IRQ vectors

.org $fffa
    .word Nmi
.org $fffe
	.word IrqHdlr

bank7_code0 = $c000

.export NmiBankShadow8, NmiBankShadowA
NmiBankShadow8 = $07b2
NmiBankShadowA = $07b3

; Replace the code to wait for sprite 0 with code to set up the scanline IRQ
.org $d4b2
    jsr SetupScanlineIRQ
    jmp $D4CE
FREE_UNTIL $D4CE

.reloc
SetupScanlineIRQ:    ; but only set these the first time it lags to prevent weird issues on double lags
    lda PreventDoubleLag
    bne +
        ; Can clobber all registers
        lda PpuCtrlShadow
        ora $0746 ; Have concerns about this
        ;sta PpuCtrlShadow
        sta PpuCtrlForIrq
        lda ScrollPosShadow
        sta ScrollPosForIrq
        inc PreventDoubleLag
    +

	lda #31
	sta LineIrqTgtReg
	
	lda #ENABLE_SCANLINE_IRQ
	sta LineIrqStatusReg
    ; We have to CLI here to allow IRQ to interrupt NMI
    ; and we have to CLI on the main thread during reset
    ; to allow IRQ to fire even after NMI ends (otherwise rti will
    ; restore the I flag from the initial sei)
    cli
    rts

.reloc
.proc IrqHdlr
	pha
;	txa
;	pha
	
	lda LineIrqStatusReg
	
	lda PpuCtrlForIrq
	sta PPUCTRL
	sta PpuCtrlShadow ; Not sure about this
	lda ScrollPosForIrq
	sta PPUSCROLL
	lda #$0
	sta PPUSCROLL
	sta LineIrqStatusReg
;	pla
;	tax
	pla
	
	rti
.endproc ; IrqHdlr

; Summary of the bug
; On a lag frame NMI is skipped, which means the hud scroll value of 0 isn't set
; We can work around this by never disabling NMI and using a soft disable instead
; and if the soft disable flag is set and NMI fires, then we need to just set the scroll to zero
; and then keep the "sprite zero" hud split check on.
; All of these changes are always on no matter the setting, 

LagFrameVar = $100

.if ENABLE_Z2FT
    .import UpdateSound
.else
    UpdateSound = $9000
.endif

.segment "PRG6"


; Put our lovely lag frame handler in bank 6 because its pretty empty
; And since its a lag frame anyway, it doesn't matter what we do since the game will do nothing
; for a whole frame anyway.
HandleLagFrame:

.segment "PRG7"

.reloc
RunAudioFrameOrLagFrame:
    pha
    txa
    pha
    tya
    pha
        jsr IncStatTimer
        ; To allow for testing between the flag on or off, I made it a soft flag 
        lda #PREVENT_HUD_FLASH_ON_LAG
        beq @HandleAudio
        ; check to see if rendering is even enabled (if its not then we aren't gonna scroll split)
        lda $fe
        and #$10
        beq @HandleAudio
        ; Check that we are in the side view mode
        lda $0736
        cmp #$0b
        bne @HandleAudio
        ; Check if we even have a sprite zero on the screen
        lda $200
        cmp #$f0
        bcs @HandleAudio
        ; keep all flags except for the nametable select to always use nametable 0
    
        lda $ff
        and #$fc
        sta $2000
        lda #0
        sta $2005
        sta $2005
    
    
        ; Here be dragons :) Write directly to OAM through OAMDATA to set a "lag sprite"
        ; There's two sources of corruptions when doing this that we need to avoid.
        ; The first is when you write to OAMADDR, it will corrupt the 8 bytes at that address
        ; The second is before the start of the frame you need to make sure OAMADDR is at 0
        ; So the magic trick is if we write 8 bytes starting from $f8, then we'll overwrite the corruption
        ; and also end with the address 0
    
        ; But theres one hold up, the sprites at $f8 and $fc are the life bar sprites, so instead
        ; we can start the write from $f4 and write to the end still
        lda #$f4
        sta $2003 ; OAMADDR
        ; and write a Hand sprite
        
        lda #$0e  ; y = 14
        sta $2004 ; OAMDATA
        lda #$8E  ; tile = hand sprite
        sta $2004 ; OAMDATA
        lda #1    ; attr = palette 1
        sta $2004 ; OAMDATA
        lda #248  ; x = 248
        sta $2004 ; OAMDATA
    
        ; Load the current health/magic bar into here
        ; That should prevent sprite corruption since the internal OAM ADDR ends at #0
        ldx #$f8
        -   lda $200,x
            sta $2004
            inx
            bne -
        jsr SetupScanlineIRQ
@HandleAudio:
        ; Skip processing audio during a real lag frame since thats what
        ; vanilla accomplishes with a hard disabled NMI
        lda LagFrameVar
        lsr
        bcs @skip
        
        lda NmiBankShadowA
        pha
        lda NmiBankShadow8
        pha
        lda #$8c  ; (bank 6)
        sta NmiBankShadow8
        sta $5114
        lda #$8d  ; (bank 6)
        sta NmiBankShadowA
        sta $5115
            jsr UpdateSound
        pla
        sta NmiBankShadow8
        sta $5114
        pla
        sta NmiBankShadowA
        sta $5115
@skip:
    pla
    tay
    pla
    tax
    pla
    rti

; Disable the final sta PPUCTRL in NMI if we handle that in the IRQ instead
.org $C1B1
    jmp *+3
; TODO We can't force disable or force enable these.
; I think I need to also make this a soft enable
.org $D2C1
    ldy #$30 | $80
    jsr SoftDisableNmi
.reloc
SoftDisableNmi:
    inc LagFrameVar
    sty PPUCTRL
    rts

.org $C060
FREE_UNTIL $C06f
NmiHandleLagFrame:
    jmp RunAudioFrameOrLagFrame
NmiRunTitleScreen:
    jmp $a610      ; Assumes bank 5 is banked in, which is used for title screen mostly
Nmi:
    bit LagFrameVar
    bpl NmiHandleLagFrame ; run audio
    bvc NmiRunTitleScreen
    pha
    jsr IncStatTimer
    lda #0
    sta PreventDoubleLag
.assert * = $C085

;.reloc
;NmiRunLagFrame:
;    jsr RunAudioFrameOrLagFrame
;    pla
;    rti

; Also run the stat timers during the title/menu in case they push the reset button
.pushseg
.segment "PRG5"
.org $A612
    ; Change this from and #$7c to $fc to keep NMI running
    and #$fc
    ; Then insert a quick patch to run the stat timers
    jsr IncStatTimerTitle
.reloc
IncStatTimerTitle:
    pha
    txa
    pha
        jsr IncStatTimer
    pla
    tax
    pla ; perform original patch
    ora $0747
    rts
.popseg


; Add a patch to increment the stat timer every NMI
; This is cleared when a new save file is loaded for the first time
.reloc
IncStatTimer:
    ; TODO validate these aren't needed. It shouldn't be....
;    txa
;    pha
    inc StatTimer+0
    bne @Continue
    inc StatTimer+1
    bne @Continue
    inc StatTimer+2
@Continue:
    ldx #0
    lda WorldNumber
    beq @OverworldOrEncounter
        cmp #3
        ; 1-West Town, 2-East Town: Increment the timer for the towns
        bcc @IncrementTimer
@Palace:
        ; Palaces
        lda $0706
        asl
        asl
        adc PalaceNumber
        tay
        ldx @PalaceMappingTable,y
@IncrementTimer:
        inc StatTimeAtLocation+0,x
        bne @Exit
        inc StatTimeAtLocation+1,x
        bne @Exit
        inc StatTimeAtLocation+2,x
;        bne @Exit ; unconditional also not needed
@OverworldOrEncounter:
@Exit:
    rts

; These values are exported from the randomizer and map from internal palace number
; to the "real palace" at this offset. This way if Palace 5 is in the location of Palace 1,
; then we increment the correct timer for Palace 5
.reloc
Palace1Offset = StatTimeInPalace1 - StatTimeAtLocation
@PalaceMappingTable:
    .byte RealPalaceAtLocation1 * 3 + Palace1Offset
    .byte RealPalaceAtLocation2 * 3 + Palace1Offset
    .byte RealPalaceAtLocation3 * 3 + Palace1Offset
    .byte $ff ; unused 4th palace in region 0
    .byte RealPalaceAtLocation4 * 3 + Palace1Offset
    .byte $ff ; unused 2th palace in region 1
    .byte $ff ; unused 3th palace in region 1
    .byte $ff ; unused 4th palace in region 1
    .byte RealPalaceAtLocation5 * 3 + Palace1Offset
    .byte RealPalaceAtLocation6 * 3 + Palace1Offset
    .byte RealPalaceAtLocationGP * 3 + Palace1Offset

; Screen split IRQ implementation. This is not technically a part of z2ft, but due to the policy of not making the game faster than vanilla, this optimization is only enabled when z2ft is enabled to partially offset the cost of FT playback.

; Patch locations that hard disable NMI to set the soft disable instead
.org $C087
    and #$fc
; Replace a useless branch with turning on the soft disable
.org $C091
    lda #$41
    sta a:LagFrameVar

.org $C1A8
    jsr SoftEnableNmi
    
.reloc
SoftEnableNmi:
.if ENABLE_Z2FT
.import CallUpdateSound
    jsr CallUpdateSound
.endif
    lda #$c0
    sta LagFrameVar
	; Copy what was overwritten from original game
	lda PPUSTATUS
    rts

; To conserve space in the common bank, as much reset code as possible is moved to bank 1f.
; This requires reset to be done in 4 parts:
;   1. load bank 1f at e000 (this is done in both banks f and 1f, as a non-power-on reset will have bank f at e000)
;   2. do most reset stuff
;   3. load bank f at e000 (again in both f and 1f)
;   4. do reset stuff that requires bank f.
.org $ffe8
	lda #($0f | PRG_BANK_ROM)
	sta PrgBankEReg
	jmp bank7_reset_part4
	
.org $fff0
bankf_reset:
	sei
	cld
	lda #$ff
	sta PrgBankEReg
	jmp bank1f_reset_part2

.org $fffc
    .word (bankf_reset)

.reloc
bank7_reset_part4:
    lda #$07
    jsr SwapPRG

    lda #$68
    sta SpChrBank7Reg
    jsr SwapCHR

	jmp bank7_code0

.segment "PRG1F"

.org $ffe8
bank1f_reset_part3:
	lda #($f | PRG_BANK_ROM)
	sta PrgBankEReg
	jmp bank7_reset_part4

.org $fff0
bank1f_reset:
	sei
	cld
	lda #$ff
	sta PrgBankEReg
	jmp bank1f_reset_part2

.org $fffc
    .word (bank1f_reset)
	.word (bank1f_reset)

.reloc
bank1f_reset_part2:
    ldx #$00
    stx PPUCTRL
    inx
    stx PrgRamProtReg2  ; Allow writing to WRAM
wait_ppu:
    lda PPUSTATUS
    bpl wait_ppu
    dex
    beq wait_ppu
    txs
    
	ldx #($0e | PRG_BANK_ROM)
    stx PrgBankCReg
    ldx #PRG_RAM_UNPROTECT1_VALUE
    stx PrgRamProtReg1  ; Allow writing to WRAM
    inx
    stx PrgBankModeReg  ; mode 3 is 4 8kb PRG banks
    stx ChrBankModeReg  ; mode 3 is CHR mode 8x1k banks
    lda #HORIZ_MIRROR_MODE
    sta NameTableModeReg
    ; Clear any pending scanline IRQs before before allowing them
	lda LineIrqStatusReg
	lda #DISABLE_SCANLINE_IRQ
	sta LineIrqStatusReg
    ; And disable Framecounter IRQ
    lda #$ff
    sta $4017
    cli
    ; Set the last sprite bank to a fixed bank for items
    lda #$00
    sta PrgRamBankReg   ; Explicitly switching to PRG RAM bank 0 works around a bug in nintendulator
    jmp bank1f_reset_part3

.segment "PRG7"

.org $c4d0
    lda     #VERT_MIRROR_MODE
    sta     NameTableModeReg

.reloc
SwapCHR:
    ; 1kb sprite banks are 20-27 and the 1kb bg banks are 28-2b
    asl
    asl
    sta SpChrBank0Reg
    adc #1
    sta SpChrBank1Reg
    adc #1
    sta SpChrBank2Reg
    adc #1
    sta SpChrBank3Reg
    adc #1
    sta SpChrBank4Reg
    sta BgChrBank0Reg
    adc #1
    sta SpChrBank5Reg
    sta BgChrBank1Reg
    adc #1
    sta SpChrBank6Reg
    sta BgChrBank2Reg
    adc #1
    ; sta SpChrBank7Reg ; Reserve the last sprite bank for new custom item tiles
    sta BgChrBank3Reg
    lda #0
    rts

.reloc
SwapToPRG0:
    lda #$00
    beq SwapPRG
SwapToSavedPRG:
    lda $0769
SwapPRG:
    asl
    ora #$80
    sta NmiBankShadow8
    sta PrgBank8Reg
    clc
    adc #1
    sta NmiBankShadowA
    sta PrgBankAReg
    lda #0
    rts


.segment "PRG7"
; Patch the save and exit command list to add a clear ram chunk
.org $FE80
    .word (ClearPartialDevRAM)

.reloc
ClearPartialDevRAM:
    ; Clear the stack ram we use for variables, but leave 16 untouched for things that
    ; should persist across resets. Because why not.
    lda #0
    ldx #$c0 - $20
@Loop2:
        sta $100 + $20 - 1,x
        dex
        bne @Loop2
    jmp $CF05

.segment "PRG0"
; Clean up stuff in bank zero - make it go via bank7's routines.
.org $8149
    lda     #HORIZ_MIRROR_MODE
    sta     NameTableModeReg
.org $8150
    jsr     SwapCHR
.org $a86b
    jsr     SwapCHR

.segment "PRG5"
; Clean up stuff in bank 5 - make it go via bank7's routines.
.org $a712
    lda     #HORIZ_MIRROR_MODE
    sta     NameTableModeReg
.org $a728
    jsr     SwapCHR

; Patch power off/ soft reset to clear out some stack ram for use
.org $a6a3
    jsr ClearStackRAM

.segment "PRG5","PRG7"
.reloc
ClearStackRAM:
    lda #0
    ldx #$d0 - $20
    ; clear stack RAM we can use for variables
@Loop2:
        sta $100 + $20 - 1,x
        dex
        bne @Loop2
    jmp $d281 ; clear rest of ram

;;;;; Fix sword flashing on title screen bug
; Switch the OAM position of an unused sprite and the glitchy sword tile
; and move that just off to the left. This sprite just doesn't seem to glitch?

.org $A7C1 + 22 * 4
    .byte $80,$E8,$20,$70

.org $A7C1 + 26 * 4
    .byte $8F,$EA,$02,$78

; Star twinkle animation, twinkle one less so the sword doesn't twinkle.
.org $a8cf
    ldx #$6b - 4
.org $a8d7
    inc $0267 + 4,x
    lda $0267 + 4,x
    and #$23
    sta $0267 + 4,x

.segment "PRG7"
; Update the pointers to the bank switches

UPDATE_REFS SwapCHR @ $C342 $C3B9 $C633 $CA10 $D045 $D050
UPDATE_REFS SwapPRG @ $C00B $C035 $C1C4 $C33F $C38B $CD5D $CF24
UPDATE_REFS SwapToSavedPRG @ $C1DB $C24A $C250 $C4D6 $C63C $C645 $CB36 $CF2A $CF3D $CF50
UPDATE_REFS SwapToSavedPRG @ $CFFD $D127 $D502 $DFD3 $DFDC $DFF0 $DFF9 $E002 $E01C $E025 $E071 $E1DE
UPDATE_REFS SwapToPRG0 @ $C1CA $C1CE $C256 $C350 $C636 $C68B $C9EF $CCFA $D0FD $D121 $D3EA $D508
UPDATE_REFS SwapToPRG0 @ $DFD9 $DFE2 $DFF6 $DFFF $E017 $E022 $E02B $E077 $E1E4 $FF4A

.segment "PRG0"

; move the scrolling code that loads metatiles from the current area
; from the fixed bank to bank 0 (the only place that calls it)
; Normally we are only able to read the metatiles from the fixed bank
; since MMC1 is 16k / 16k banking but because we are on mmc5, we can
; and load the pointers into other banks that MMC1 couldn't
; This clears up about 200 bytes from the fixed bank ($feea, $ff70)

.org $9AFD
    jsr LoadAreaBGMetatile

.reloc
LoadAreaBGMetatile:
; if the code is in a000 or higher that would switch this bank out from under itself
.assert * > $a000
    lda $0769
    asl
    ora #$80
    sta NmiBankShadow8
    sta PrgBank8Reg
    ; Store the loop counter back into $01
@Loop:
        stx $01
        ; Load background tile id from the column buffer high bits
        ; Transform it as follows to get the attribute and metatile
        ; xx000000 -> attr and xx000000 -> 00000xx0 -> tileid
        lda $0464,x
        and #$c0
        sta $03
        asl a
        rol a
        rol a
        asl a
        tay
        ; Read from the fixed address for the backgrounds of the "current" banks
        lda $8500,y
        sta $0e
        lda $8501,y
        sta $0f
        ; Write 00xxxx00 as 0000xxxx into $02
        lda $0464,x
        and #$3f
        asl a
        asl a
        sta $02
        ; And then add 2 if 740 is even. This selects the left or right side of the metatile
        lda $0740
        and #$01
        eor #$01
        asl a
        adc $02
        ; Use as the offset from the pointer to read the actual tile from the metatile
        tay
        ldx $00
        lda ($0e),y
        sta $03a7,x
        iny
        lda ($0e),y
        sta $03a8,x
        ; $03 is the attribute in the format xx000000
        ; $04 is set to 0 before entering
        ldy $04
        ; $05 is set by the caller
        ; Branch if we are loading the attributes for the right side 
        lda $05
        bne @LoadRightAttribute
@LoadLeftAttribute:
        ; Every other attribute (when the loop counter $01 is odd)
        ; moves to the next byte in the attribute buffer
        lda $01
        lsr a
        bcs @BottomLeftAttribute
@TopLeftAttribute:
        rol $03
        rol $03
        rol $03
        jmp @WriteAttribute
@LoadRightAttribute:
        ; Every other attribute (when the loop counter $01 is odd)
        ; moves to the next byte in the attribute buffer
        lda $01
        lsr a
        bcs @BottomRightAttribute
@TopRightAttribute:
        lsr $03
        lsr $03
        lsr $03
        lsr $03
        jmp @WriteAttribute
@BottomLeftAttribute:
        lsr $03
        lsr $03
        ; fallthrough to move to the next attribute byte in the buffer
        ; The bottom right attribute doesn't need shifting
@BottomRightAttribute:
        inc $04
        ; Write attribute data stored in $03 to the attribute buffer
@WriteAttribute:
        lda $0471,y
        ora $03
        sta $0471,y
        inc $00
        inc $00
        ldx $01
        inx
        cpx #$0d
        bcc @Loop
    ; switch back to 0 just in case
    jmp SwapToPRG0


; Move sprites out of the last fixed bank
; Instead of using the blank tile sprites to cover the right 8px on the overworld, just use
; any other full color sprite (since the palette is all black this is fine)
.org $878c
    lda #$7f

; link's overworld sprite has an invisible half that needs moved outta the new sprite bank as well
.org $8739
    lda #$73
