.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG, SwapToPRG0
.export UpdateSound

; Game variables/locations
UpdateSound = $878c

.segment "PRG7"

.reloc

.export CallUpdateSound
.proc CallUpdateSound
	; Must preserve these because in Z2R it's possible to be interrupted by NMI that may change banks.
	lda NmiBankShadow8
	pha
	lda NmiBankShadowA
	pha

	lda #6

	jsr SwapPRG

	jsr UpdateSound

	pla
	sta NmiBankShadowA
	sta PrgBankAReg
	pla
	sta NmiBankShadow8
	sta PrgBank8Reg

	rts
.endproc ; CallUpdateSound

; Do NOT call UpdateSound from overworld loading code as Z2R enables NMI during loading so UpdateSound will be called anyway.
.org $c1bf
	rts
	nop

; Get rid of original call to UpdateSound to move it after setting up IRQ
UPDATE_REFS SwapToPRG0 @ $c130
