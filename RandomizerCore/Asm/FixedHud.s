.macpack common

; Summary of the bug
; On a lag frame NMI is skipped, which means the hud scroll value of 0 isn't set
; We can work around this by never disabling NMI and using a soft disable instead
; and if the soft disable flag is set and NMI fires, then we need to just set the scroll to zero
; and then keep the "sprite zero" hud split check on.
; All of these changes are always on no matter the setting, 

.define LagFrameVar $f4

.if ENABLE_Z2FT
    .import UpdateSound
.else
    UpdateSound = $9000
.endif

.segment "PRG6"

.reloc
RunAudioFrameOrLagFrame:
    txa
    pha
    tya
    pha
        lda LagFrameVar
        bpl @skip
            jmp HandleLagFrame
        @skip:
        jsr UpdateSound
EarlyExitIrq:
    pla
    tay
    pla
    tax
    rts

; Put our lovely lag frame handler in bank 6 because its pretty empty
; And since its a lag frame anyway, it doesn't matter what we do since the game will do nothing
; for a whole frame anyway.
HandleLagFrame:
    ; To allow for testing between the flag on or off, I made it a soft flag 
    lda #PREVENT_HUD_FLASH_ON_LAG
    beq EarlyExitIrq
    ; check to see if rendering is even enabled (if its not then we aren't gonna scroll split)
    lda $fe
    and #$10
    beq EarlyExitIrq
    ; Check that we are in the side view mode
    lda $0736
    cmp #$0b
    bne EarlyExitIrq
    ; Check if we even have a sprite zero on the screen
    lda $200
    cmp #$f0
    bcs EarlyExitIrq
    ; keep all flags except for the nametable select to always use nametable 0
    lda $ff
    and #$fc
    sta $2000
    lda #0
    sta $2005
    sta $2005

; DISABLE THE GLOVE for now....
.if 0
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
    lda #$a0  ; x = 160
    sta $2004 ; OAMDATA

    ; Load the current health/magic bar into here
    lda $200 + $f8
    sta $2004 ; OAMDATA
    lda $200 + $f9
    sta $2004 ; OAMDATA
    lda $200 + $fa
    sta $2004 ; OAMDATA
    lda $200 + $fb
    sta $2004 ; OAMDATA
    lda $200 + $fc
    sta $2004 ; OAMDATA
    lda $200 + $fd
    sta $2004 ; OAMDATA
    lda $200 + $fe
    sta $2004 ; OAMDATA
    lda $200 + $ff
    sta $2004 ; OAMDATA

    ; That should prevent corruption
.endif

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
        jsr RunAudioFrameOrLagFrame
        pla
        sta NmiBankShadow8
        sta $5114
        pla
        sta NmiBankShadowA
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

; This exact location is needed by both lag frame handling and z2ft

.if !ENABLE_Z2FT

.org $C1a8
    ; Soft enable NMI
    clc
    ror LagFrameVar

.endif
