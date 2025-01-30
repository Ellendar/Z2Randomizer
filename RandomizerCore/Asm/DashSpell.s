.include "z2r.inc"

.segment "PRG0"

; Update the magic table to point to an rts (why is this needed???)
.org $8e50
  .word ($9814)

; Inside Links_Acceleration_Routine, patch the max speed compare to check if we cast dash  
.org $93ff
  jsr ReplaceFireWithDashSpell

.reloc
ReplaceFireWithDashSpell:
  pha
  lda $076f ; Current magic state
  and #$10  ; fire is on
  bne @HasFire
    pla
    cmp $93b3,y ; Table for Link's original max velocities
    rts
@HasFire:
  pla
  cmp @SecondaryVelocityTable,y
  rts
@SecondaryVelocityTable:
.byte $30, $d0
