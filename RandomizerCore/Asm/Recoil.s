.include "z2r.inc"

.segment "PRG7"

LinkXVelocity = $70
EnemyID = $a1
LinkYVelocityLo = $03E6
LinkYVelocityHi = $057D

LinkHitRoutine = $E2EF

; Patch when a boss hits link for a strong hit to ignore the 2x multiplier
; by just returning after checking hit
.org $E584
  jmp LinkHitRoutine
FREE_UNTIL $E59C

; Patch inside SetLinksRecoil to load from the enemy table instead
.org $E3B3
  jsr SetLinkXRecoil

; Patch inside LinkHitRoutine for setting the Y recoil
.org $E363
  jsr SetLinkYRecoil

.reloc
SetLinkXRecoil:
  tya
  and #1
  ; If the current y value is even, then we are recoiling left
  bne @InvertRecoil
    lda #$ff
@InvertRecoil:
  pha
    lda EnemyID,x
    tay
  pla
  eor RecoilTableX,y
  rts

.reloc
SetLinkYRecoil:
  lda EnemyID,x
  tay
  lda RecoilTableYLo,y
  sta LinkYVelocityLo
  lda RecoilTableYHi,y
  sta LinkYVelocityHi
  rts

.segment "PRG0"


; If we have custom knock back then we want to slow link down to the max velocity
; Vanilla doesn't check if you are going faster than max speed causing the infamous
; left+right behavior, and if we are getting launched faster than max speed, we need
; to patch this. This patch clamps the velocity so it slows you down if you are past max
.org $9402
  jmp CheckIfSlowDownNeeded

AccelerateSpeed = $9405
ClampSpeed = $9417

.reloc
CheckIfSlowDownNeeded:
  ; Skip if we are already at max speed
  beq @Continue
  php
  cpy #0
  beq @GoingRight
    ; we are going left so check if the speed is positive
    cmp #0
    bpl @PullFlagsAndContinue
    ; if its a negative number and greater than the max value, subtract 1
    plp
    bcs @Continue
      adc #1
      jmp ClampSpeed

@GoingRight:
    ; we are going right so check if the speed is greater than 0
    cmp #0
    bmi @PullFlagsAndContinue
    ; if its a positive number and greater than the max value, subtract 1
    plp
    bcc @Continue
      sbc #1
      jmp ClampSpeed
@PullFlagsAndContinue:
  plp
; Not at max speed so don't return to the "exit" location
@Continue:
  clc
  jmp AccelerateSpeed ; continue speeding up in the correct direction
