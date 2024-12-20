﻿.macpack common
.segment "PRG4"

.define ProjectileYPosition $30
.define ProjectileXVelocity $77
.define ProjectileType      $87
.define EnemyType           $a1
.define LinkXPosition       $4d
.define EnemyXPositionLo    $4e
.define EnemyXPositionHi    $3c
.define RNG                 $051b
.define ProjectileEnemyData $07c0

EnemyYVelocity = $057e
; SinWaveVelocityIncrement = $ba1c

; Hook into carrock's update code to add the new position calculation
.org $aec4
    jmp ChooseNewCarrockXPosition
.free $aed3 - *

; Force Carrock to teleport only on the side opposite of the player
.reloc
ChooseNewCarrockXPosition:
    ; roll a random number and force carrock to only appear on the opposite side
    ; remove the MSB from the random value and we will replace it with the opposite
    ; bit from Link's MSB
    lda RNG,x
    ; By shifting links opposite side bit twice, we can force Carrock to be closer to the wall
    asl
    sta EnemyXPositionLo,x

    ; Since Link's position can be from roughly $00fb - 01ef
    ; We can offset this by flipping the high bit, and then checking if its greater than
    ; $80 - $10 (which would be like starting the range from F0 - 70)
    lda LinkXPosition
    eor #$80
    cmp #$70

    ; At this point, the carry contains the bit for the opposite side of the screen for link
    ; So shift it back into bosses position
    ror EnemyXPositionLo,x

    ; Vanilla checks to see if carrock is too close to the right edge, and prevents going
    ; offscreen by dividing the position by two if it would
    lda #$e0
    cmp EnemyXPositionLo,x
    bcs @SetXHighPos
        sbc #$10 - 1
        sta EnemyXPositionLo,x
@SetXHighPos:
    lda #1
    sta EnemyXPositionHi,x
    rts

; Inside a funciton that updates motion for (maybe?) all projectiles
.org $996f
    jsr MoveSinWaveWifiShot

.reloc
SinWaveVelocityIncrement:
    .byte $04, $fc
SinWaveMaxVelocityExtant:
    .byte $28, $d4

MoveSinWaveWifiShot:
    ; Now check to see if we set the flag for sin waves.
    ; We write b0 to ProjectileEnemyData,x if this wifi is a sin wave so we can check the high bit if its set then its a sin wav
    lda ProjectileEnemyData,x
    bpl @NotAWifiShot
        lda ProjectileEnemyData,x
        and #$01
        tay
        lda EnemyYVelocity,x
        clc
        adc SinWaveVelocityIncrement,y
        sta EnemyYVelocity,x
        cmp SinWaveMaxVelocityExtant,y
        bne @NotAtMaxVelocity
            inc ProjectileEnemyData,x
@NotAtMaxVelocity:
        lsr
        lsr
        lsr
        lsr
        cmp #$08
        bcc @PositiveVelocity
            ora #$f0
@PositiveVelocity:
        adc ProjectileYPosition,x
        sta ProjectileYPosition,x
    ldy #$12
@NotAWifiShot:
    ; Perform the original patched call
    lda $6ec0,y
    rts

.org $af08
    jsr RandomizeWifiShotType

.reloc
RandomizeWifiShotType:
    ldx $10
    lda EnemyType,x
    cmp #$22 ; check if its spawned by carock and skip it. carock code id is 0x22
    beq @CarockShot
        ; Do the original code since its not our new buffed carock
        lda #$c0
        sta ProjectileYPosition,y
        rts
@CarockShot:
    ; Fetch a random number and 50/50 chance its a straight shot
    lda #0
    bit RNG+1
    bmi @StraightShot
        bvc @HiPositionShot
            ; Shooting from the bottom should start by arcing upwards
            lda #1
    @HiPositionShot:
        sta ProjectileEnemyData,y
        lda #0
        sta EnemyYVelocity,y
        lda ProjectileEnemyData,y
        ora #$b0
        ; Firing a sin wave shot, so set the flag for sin wave and then follow through setting up the position
        sta ProjectileEnemyData,y
        bne @WriteInitCoord
@StraightShot:
    sta ProjectileEnemyData,y
    lda #$b0
    ; we are shooting straight so clear out the flag
    ; 50/50 chance that the shot starts high or low
@WriteInitCoord:
    bvc @WriteShotYCoord
        clc
        adc #$10
@WriteShotYCoord:
    sta ProjectileYPosition,y
    ; roll a random number to see if we shoot a fast beam
    lda RNG+1
    and #1
    beq @Exit
        lda ProjectileXVelocity,y
        bmi @NegativeVelocity
           clc
           adc #$10
           bne @DoneVelocityUpdate ; unconditional
@NegativeVelocity:
        sec
        sbc #$10
@DoneVelocityUpdate:
        sta ProjectileXVelocity,y
@Exit:
    rts

.segment "PRG7"

.org $d29c
; Update the general RAM clear routine that runs when transitioning between rooms (among other things)
; to also clear out the new object ram we added. In the future if we find space in $300 or $400 to fit
; the carock extra data, we can slip it in there
ClearCommonEnemyRAM:
    ldx #0
    txa
@ClearPageLoop:
    sta $300,x
    sta $400,x
    dex
    bne @ClearPageLoop
; A and X are 0
; we want to clear [$00, $e0)
@ClearZPLoop:
    sta $00,x
    inx
    cpx #$e0
    bne @ClearZPLoop
; TODO: make this new clear code the default and add an `if` for the carock part
; Now clear out the extra ram used for carock wifi beams
    ldx #5
@ClearCarockBeam:
    sta ProjectileEnemyData,x
    dex
    bpl @ClearCarockBeam
    stx $0302 ; Clear VRAM pointers by setting them to #$ff
    stx $0363
    rts
FREE_UNTIL $d2be
