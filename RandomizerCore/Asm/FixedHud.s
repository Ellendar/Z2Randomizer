
.macpack common

; Summary of the bug
; On a lag frame NMI is skipped, which means the hud scroll value of 0 isn't set
; We can work around this by never disabling NMI and using a soft disable instead
; and if the soft disable flag is set and NMI fires, then we need to just set the scroll to zero
; and then keep the "sprite zero" hud split check on.
; All of these changes are always on no matter the setting, 

.define LagFrameVar $f4

.segment "PRG6"

.reloc
RunAudioFrameOrLagFrame:
    txa
    pha
    tya
    pha
        lda LagFrameVar
        bpl +
            jmp HandleLagFrame
        +
        jsr $9000 ; audio handler
    pla
    tay
    pla
    tax
    rts

; Put our lovely lag frame handler in bank 6 because its pretty empty
; And since its a lag frame anyway, it doesn't matter what we do since the game will do nothing
; for a whole frame anyway.
.reloc
HandleLagFrame:
    ; set the scroll to zero if the player has that flag on
    lda #PREVENT_HUD_FLASH_ON_LAG
    beq ++
    ; check to see if rendering is even enabled (if its not then we aren't gonna scroll split)
    lda $fe
    and #$10
    beq ++
    ; Check that we are in the side view mode
    lda $0736
    cmp #$0b
    bne ++
    ; Check if we even have a sprite zero on the screen
    lda $200
    cmp #$f0
    bcs ++
    ; keep all flags except for the nametable select to always use nametable 0
    lda $ff
    and #$fc
    sta $2000
    lda #0
    sta $2005
    sta $2005
    ; Use the MMC5 "in frame" flag to wait for the frame to start before waiting for sprite zero
-
    bit $5204
    bvc -
    ; and now wait for sprite zero and switch the scroll when that finished
-
    bit $2002
    bvc -
    lda $FF
    ldx $FD
    ldy #$10
    -
        dey
        bne -
    sta $2000
    stx $2005
    sty $2005
    ++
    pla
    tay
    pla
    tax
    rts

.segment "PRG7"

.import SwapPRG, SwapToSavedPRG


.org $fffa
    .word (Nmi)

.reloc
.import NmiBankShadow8,NmiBankShadowA
NmiRunLagFrame:
    pha
        lda #$8c  ; (bank 6)
        sta $5114
        lda #$8d  ; (bank 6)
        sta $5115
        jsr RunAudioFrameOrLagFrame
        lda NmiBankShadow8
        sta $5114
        lda NmiBankShadowA
        sta $5115
    pla
    rti

; Rewrite the loading screen audio handler to fit the new lag frame handler
; Move the phx and phy to the bank
.org $C060
NmiHandleLagFrame:
    jmp NmiRunLagFrame
NmiRunTitleScreen:
    jmp $a610      ; Assumes bank 5 is banked in, which is used for title screen mostly
FREE_UNTIL $C079
.org $C079
Nmi:
    bit LagFrameVar
    bmi NmiHandleLagFrame
    bit $100
    bpl NmiHandleLagFrame ; run audio
    bvc NmiRunTitleScreen
.assert * = $C084


; Patch locations that hard disable NMI to set the soft disable instead
.org $C087
    and #$fc
; Replace a useless branch with turning on the soft disable
.org $C091
    lda #$80
    sta a:LagFrameVar

.org $C1a8
    ; Soft enable NMI
    clc
    ror LagFrameVar