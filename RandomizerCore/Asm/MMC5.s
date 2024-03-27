
; TODO common macpack or something to consolidate macros
.macro FREE_UNTIL end
  .assert * <= end
  .free end - *
.endmacro

.segment "HEADER"
.org $6
.byte $52   ; Set the mapper to MMC5


.segment "PRG7"

bank7_code0 = $c000

.org $c4d0
    lda     #$44            ; vertical mirroring
    sta     $5105
.org $ff70
bank7_reset:
    sei
    cld
    ldx     #$00
    stx     $2000
    stx     $5101           ; CHR mode 1x8k banks
    inx
    stx     $5100           ; two 16k PRG banks
    stx     $5103           ; Allow writing to WRAM
wait_ppu:
    lda     $2002
    bpl     wait_ppu
    dex
    beq     wait_ppu
    txs
    stx     $5117           ; Top bank is last bank
    lda     #2
    sta     $5102           ; Allow writing to WRAM
    lda     #$50            ; horizontal mirroring
    sta     $5105
    jsr     bank7_chr_bank_switch__load_A
    lda     #$07
    jsr     bank7_Load_Bank_A_0x8000
    jmp     bank7_code0

FREE_UNTIL $FFB1

.org $ffb1
bank7_chr_bank_switch__load_A:
    lsr
    sta     $5127
    sta     $512b
    lda     #0
    clc
    rts
FREE_UNTIL $FFC5

.org $FFC5
bank7_Load_Bank_0_at_0x8000:
    lda     #$00
    beq     bank7_Load_Bank_A_0x8000
bank7_Load_Bank_769_at_0x8000:
    lda     $0769
bank7_Load_Bank_A_0x8000:
    asl
    ora     #$80
    sta     $5115
    lda     #0
    rts

FREE_UNTIL $ffe0

.segment "PRG0"
; Clean up stuff in bank zero - make it go via bank7's routines.
.org $8149
    lda     #$50            ; horizontal mirroring
    sta     $5105
.org $8150
    jsr     $ffb1
.org $a86b
    jsr     $ffb1

.segment "PRG5"
; Clean up stuff in bank 5 - make it go via bank7's routines.
.org $a712
    lda     #$50            ; horizontal mirroring
    sta     $5105
.org $a728
    jsr     $ffb1

