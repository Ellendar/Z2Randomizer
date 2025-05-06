.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG, SwapToPRG0, NmiBankShadow8, NmiBankShadowA
.export UpdateSound

; Screen split IRQ implementation. This is not technically a part of z2ft, but due to the policy of not making the game faster than vanilla, this optimization is only enabled when z2ft is enabled to partially offset the cost of FT playback.

; Game variables/locations
UpdateSound = $878c

PpuCtrlShadow = $ff
ScrollPosShadow = $fd

; These are new variables that use free memory
PpuCtrlForIrq = $6c7
ScrollPosForIrq = $6c8

.segment "PRG7"

; Replace the code to wait for sprite 0 with code to set up the scanline IRQ
.org $d4b2
	; Can clobber all registers
	lda PpuCtrlShadow
	ora $746 ; Have concerns about this
	;sta PpuCtrlShadow
	sta PpuCtrlForIrq
	lda ScrollPosShadow
	sta ScrollPosForIrq
	
	lda $200
	clc
	adc #$11
	sta LineIrqTgtReg
	
	lda #ENABLE_SCANLINE_IRQ
	sta LineIrqStatusReg
	
	cli

	; Exact fit

.reloc

.proc IrqHdlr
	; $22 bytes
	pha
	txa
	pha
	
	lda LineIrqStatusReg
	;lda #DISABLE_SCANLINE_IRQ
	;sta LineIrqStatusReg
	
	ldx PpuCtrlForIrq
	lda ScrollPosForIrq
	sta PPUSCROLL
	lda #$0
	sta PPUSCROLL
	stx PPUCTRL
	
	stx PpuCtrlShadow ; Not sure about this
	
	pla
	tax
	pla
	
	rti
.endproc ; IrqHdlr

.org $fffe
	.word IrqHdlr

.org $c1a8
	jsr CallUpdateSound

.reloc

.proc CallUpdateSound
	; Must preserve these because in Z2R it's possible to be interrupted by NMI that may change banks.
	lda NmiBankShadow8
	pha
	lda NmiBankShadowA
	pha

	lda #$6

	jsr SwapPRG

	jsr UpdateSound

	pla
	sta NmiBankShadowA
	sta PrgBankAReg
	pla
	sta NmiBankShadow8
	sta PrgBank8Reg

	; Copy what was overwritten from original game
	lda PPUSTATUS

	; Copy what was overwritten from Z2R's HUD fixes
	clc
	ror $f4

	rts
.endproc ; CallUpdateSound

; Do NOT call UpdateSound from overworld loading code as Z2R enables NMI during loading so UpdateSound will be called anyway.
;.org $c1bf
;	rts
;	nop

; Get rid of original call to UpdateSound to move it after setting up IRQ
UPDATE_REFS SwapToPRG0 @ $c130
