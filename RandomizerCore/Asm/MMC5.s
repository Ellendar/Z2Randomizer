.macpack common

.import nmi
.export SwapPRG, SwapToSavedPRG, SwapToPRG0

FREE "PRG7" [$FEAA, $FFE8)

.segment "HEADER"
.org $4
.byte $10 ; 256 KB PRG-ROM

.org $6
.byte $52 ; Mapper 5
.byte $08 ; use NES 2.0 header

.org $a
.byte $70 ; reserve 8kb of PRG NVRAM (SRAM)

.segment "PRG7"

bank7_code0 = $c000

.export NmiBankShadow8, NmiBankShadowA
NmiBankShadow8 = $7b2
NmiBankShadowA = $7b3

; To conserve space in the common bank, as much reset code as possible is moved to bank 1f. This requires reset to be done in 4 parts: 1. load bank 1f at e000 (this is done in both banks f and 1f, as a non-power-on reset will have bank f at e000), 2. do most reset stuff, 3. load bank f at e000 (again in both f and 1f), 4. do reset stuff that requires bank f.
.org $ffe8
	lda #($f | PRG_BANK_ROM)
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
    
	ldx #($e | PRG_BANK_ROM)
    stx PrgBankCReg
    ldx #PRG_RAM_UNPROTECT1_VALUE
    stx PrgRamProtReg1  ; Allow writing to WRAM
    inx
    stx PrgBankModeReg  ; mode 3 is 4 8kb PRG banks
    stx ChrBankModeReg  ; mode 3 is CHR mode 8x1k banks
    lda #HORIZ_MIRROR_MODE
    sta NameTableModeReg
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

; Fix sword flashing on title screen bug

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
