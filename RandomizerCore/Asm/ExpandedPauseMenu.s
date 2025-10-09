.include "z2r.inc"

.segment "PRG0"

.org $a307
    jsr CheckIfLevelingUp
    nop
    nop ; Remove original compare

.reloc
CheckIfLevelingUp:
    lda $074c ; When 74c is 0 we are doing the normal pause screen
    lsr       ; so set the carry if 74c is 1
    lda $0525 ; Then reload the row draw count
    bcc @skip
    cmp #$0a
    rts
@skip:
    cmp #$0b ; extend the menu just a little bit
    rts

.org $a5e8
    lda PauseMenuRow1Tiles,y
    sta $02
    lda PauseMenuRow1Tiles+1,y
    sta $03
    lda PauseMenuRow2Tiles,y
    sta $04
    lda PauseMenuRow2Tiles+1,y
    sta $05


.org $9b5a
; In vanilla, this is a pointer table to the background tiles used when drawing the pause
; menu. Its split into two consecutive tables for the first row and then the second row of tiles
FREE_UNTIL $9B82

LevelUp_Pane__HorizontalBarWithCorner = $9BAA
LevelUp_Pane__HorizontalBarNoCorner = $9BB8
LevelUp_Pane__BlankLine = $9BC6

.reloc
PauseMenuRow1Tiles:
    .word    LevelUp_Pane__HorizontalBarWithCorner
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__BlankLine
    .word    LevelUp_Pane__HorizontalBarWithCorner
    .word    ($787E) ; Number_of_Keys
    .word    LevelUp_Pane__BlankLine
.reloc
PauseMenuRow2Tiles:
    .word    ($780E) ; Shield
    .word    ($781C) ; Jump
    .word    ($782A) ; Life
    .word    ($7838) ; Fairy
    .word    ($7846) ; Fire
    .word    ($7854) ; Reflect
    .word    ($7862) ; Spell
    .word    ($7870) ; Thunder
    .word    ($7800) ; Number_of_Lives
    .word    ($788C) ; Number_of_Crystals
    .word    LevelUp_Pane__HorizontalBarWithCorner

.org $9DDB
    jsr AddExtraRowOfItems

DrawPauseMenuRowOfItems = $9B19

.reloc
.export AddExtraRowOfItems
AddExtraRowOfItems:
    jsr DrawPauseMenuRowOfItems
    ; $01 is the base X offset for the items
    ; 0c and 0d are about to be scratched for a jump table use, so we can use them freely
    ldy #7
@Loop:
    lda PresenceTableForExtraItems,y
    tax
    lda $700,x
    ; load the byte to compare with the mask value
    and BitMaskForExtraItems,y
    cmp BitMaskForExtraItems,y
    bne @Next
    
    ; We have the item so draw it!
    tya
    asl
    asl
    pha
    tax
    lda #$b7 + 16
    sta $204 + $20,x
    tya
    asl
    adc #$e1
    sta $205 + $20, x
    lda #1
    sta $206 + $20, x
    pla
    asl
    adc $01
    sta $207 + $20,x

@Next:
    dey
    bpl @Loop
    rts

.reloc
PresenceTableForExtraItems:
    .byte <$0796 ; Upstab
    .byte <$0796 ; Downstab
    .byte <$0798 ; Trophy
    .byte <$0799 ; Mirror
    .byte <$079a ; Bagu
    .byte <$079a ; Medicine
    .byte <$079b ; Water
    .byte <$079c ; Child

BitMaskForExtraItems:
    .byte $04 ; Upstab
    .byte $10 ; Downstab
    .byte $10 ; Trophy
    .byte $01 ; Mirror
    .byte $08 ; Bagu
    .byte $40 ; Medicine
    .byte $01 ; Water
    .byte $20 ; Child

; Update the lives display to get rid of the extra line
.org $9C1A + 4
    .byte $f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4,$f4

