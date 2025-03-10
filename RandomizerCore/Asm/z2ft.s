.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG, SwapToPRG0, NmiBankShadow8, NmiBankShadowA
.export UpdateSound

; Screen split IRQ implementation. This is not technically a part of z2ft, but due to the policy of not making the game faster than vanilla, this optimization is only enabled when z2ft is enabled to partially offset the cost of FT playback.

; Game variables/locations
UpdateSound = $878c


.segment "PRG7"

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
