; All boss-related patches
; Also see:
; BuffCarock.s for Harder Carock

.include "z2r.inc"
.import SwapCHR

; ElevatorBossFix
; ===============
.segment "PRG7"

; Screen lock released at bank 7 E7A9 (0x1e7b9)
.if !RANDOM_BOSS_ITEM
    .org $e7a9
        jsr ElevatorBossFix
.endif

.reloc
ElevatorBossFix:
    lda #$01
    eor ScrollFrozen  ; unfreeze scrolling if frozen, otherwise freeze it
    sta ScrollFrozen
    lda #$13
    cmp Enemy0Type
    bne @Exit
        lda #$01
        eor Enemy0Status
        sta Enemy0Status
        lda #$a0
        sta Enemy0YPositionLo
    @Exit:
        rts


; HandleRandomBossDrop
; ====================
.if RANDOM_BOSS_ITEM
.segment "PRG7"
.org $e79a
    ; Branch if scroll frozen
    lda ScrollFrozen
    beq +
        ; freeze scroll
        lda #0
        jsr ElevatorBossFix
        ; branch if the music is already playing
        lda $07fb
        bne +
            ; otherwise resume the previous track (palace theme)
            lda #2
            sta $eb
    +
    ; Write the "grab item" sound effect to the sfx queue
    lda #8
    sta Z2Square1SoundQueue
    ; Branch if the item we are getting is NOT a key
    cpy #8
    bne +
        ; increment number of keys and carry on
        inc Keys
        jmp $e797  ; always overwritten by full_item_shuffle anyway
    +
    ; Otherwise continue to $E7BB which is the start of the get item code
    .assert * = $E7BB

; Patch a few locations to make sure the music returns to normal after getting an item
.org $e80c
    jsr DontSwitchMusicIfInPalace1
    nop

.org $e84b
    jsr DontSwitchMusicIfInPalace2
    nop

.reloc
DontSwitchMusicIfInPalace1:
    lda $eb
    cmp #$02
    beq +
        ; Restore track 16
        lda #$10
        sta $eb
+   rts

DontSwitchMusicIfInPalace2:
    lda $eb
    cmp #$02
    beq +
        ; Restore track 0
        lda #$00
        sta $eb
+   rts
.endif


; FixHelmetheadBossRoom (author jroweboy)
; =======================================
.segment "PRG4"

.reloc
HelmetHeadGoomaFix:
    lda #<HelmetRoom
    eor MapNumber
    rts

.org $bac3
    jsr HelmetHeadGoomaFix
.org $bc83
    jsr HelmetHeadGoomaFix
.org $bd75
    jsr HelmetHeadGoomaFix

.org $9b27
; Also fix a key glitch
    nop
    nop
    nop


; FixMinibossGlitchyAppearance (author jroweboy)
; ==============================================
.segment "PRG7"

; Patch the start of the sideview initialization to check if the enemy is loaded in the first screen
; This is after switching the CHR banks for the sideview
.org $C638
    jmp CheckToOverwriteChrBank

.reloc
CheckToOverwriteChrBank:
; If A = $20 then we are horsehead/rebo
    ldx #6
@loop:
        lda $a1 - 1,x
        cmp #$20
        bne @NotHorsehead
            jsr OverwriteSpriteCHRBank
@NotHorsehead:
        dex
        bne @loop
    jmp NewSideviewInit

.reloc
OverwriteSpriteCHRBank:
    ; We are loading that enemy, so switch the sprite banks based on which palace we are in
    lda WorldNumber ; 3 = palace group 1,2,5 ; 4 = palace group 3,4,6
    cmp #$03 ; we are fighting a Horsehead since this is palace set 4
    beq @LoadHorsehead
        cmp #4
        bne @Exit
        lda #$18 * 4 + 4 ; CHR bank for rebo as mini boss
        bne @WriteCHRBanks ; unconditional (we can't use BIT $abs here safely)
@LoadHorsehead:
    lda #$0a * 4 + 4 ; CHR bank for horsehead as mini boss
@WriteCHRBanks:
    ; switch two banks which is enough for both mini bosses
    sta SpChrBank4Reg
    clc
    adc #1
    sta SpChrBank5Reg
    ; due to an MMC5 issue, we need to write a bg bank as well.
    lda CurrentCHRBank
    asl
    asl
    ; clc ; carry is clear here
    adc #4
    sta BgChrBank0Reg
@Exit:
    rts

; Patch the enemy loading routine to check if the enemy is horsehead/rebo
.org $D68B
    jsr CheckIfHorseheadReboshark
.reloc
CheckIfHorseheadReboshark:
    ; If A = $20 then we are horsehead/rebo
    cmp #$20
    bne @Exit
        jsr OverwriteSpriteCHRBank
@Exit:
    jmp EnemyFacingDirection


; Aggressive Thunderbird
; ======================
.segment "PRG5"
CheckThunderbirdHP := $a400
CheckThunderbirdFrame := $a407
.if AGGRESSIVE_TBIRD
.org CheckThunderbirdHP
    bpl CheckThunderbirdFrame
FREE_UNTIL CheckThunderbirdFrame
.endif


; UpdateAllBossHpDivisor (author jroweboy)
; =========================================
; The function in bank 4 $9C45 (file offset 0x11c55) and bank 5 $A4E9 (file offset 0x164f9)
; are divide functions that are used to display the HP bar for bosses and split it into 8 segments.
; Inputs - A = divisor; X = enemy slot
;
; This function updates all the call sites to these two functions to match the HP for the boss.
.segment "PRG4"
.reloc
Bank4BossHpDivisorLo:
    .byte BOSS_0_HP_DIVISOR_LO, BOSS_1_HP_DIVISOR_LO, BOSS_2_HP_DIVISOR_LO
    .byte BOSS_3_HP_DIVISOR_LO, BOSS_4_HP_DIVISOR_LO, BOSS_5_HP_DIVISOR_LO
    .byte BOSS_6_HP_DIVISOR_LO

.reloc
Bank4BossHpDivisorHi:
    .byte BOSS_0_HP_DIVISOR_HI, BOSS_1_HP_DIVISOR_HI, BOSS_2_HP_DIVISOR_HI
    .byte BOSS_3_HP_DIVISOR_HI, BOSS_4_HP_DIVISOR_HI, BOSS_5_HP_DIVISOR_HI
    .byte BOSS_6_HP_DIVISOR_HI

.define BossHpLo $00
.define BossHpHi $01
.define DivisorLo $02
.define DivisorHi $03

.org $9C45
    tay
    lda $C2,x ; Boss HP
    sta BossHpHi
    lda Bank4BossHpDivisorLo,y
    jsr DoDivisionByRepeatedSubtraction
    nop
.assert * = $9C51

.org $9C7A
    jmp HandleOverHP

.reloc
HandleOverHP:
    dey ; 1 or below means that the boss is 100% or less HP, so no over health
    bmi @Exit
        ; Change the tile ID to represent over health on a boss.
        ldx #$1c
        lda #$c5
@overhp:
        sta $02c1, x
        dex
        dex
        dex
        dex
        dey
        bpl @overhp  
@Exit:
    ; Do the original code
    ldx $10
    rts

.reloc
DoDivisionByRepeatedSubtraction:
    sta DivisorLo
    lda Bank4BossHpDivisorHi,y
    sta DivisorHi
    ldy #0
    sty BossHpLo
    clc ; intentionally subtract an extra 1 which makes the math line up better
    @loop:
        lda BossHpLo
        sbc DivisorLo
        sta BossHpLo
        lda BossHpHi
        sbc DivisorHi
        sta BossHpHi
        iny
        bcs @loop
    rts

.org $BAD3
FixHelmetHeadHpDivisorOnNonWest:
    ; Skip over a vanilla check for gooma/helmethead split which breaks the HP divisor update
    jmp $BADA


; BossKillFixes (author initsu)
; =============================
.segment "PRG7"
HookIntoSpawnBossItem := $de1a
.org HookIntoSpawnBossItem
    jmp BossKillFixes

.reloc
BossKillFixes:
    sta $af,x ; command overwritten by jmp

    ; Patch boss death to spawn the key on the frozen page instead of the boss's actual page
    ; This prevents softlocks when Rebonack dies off-screen.
    lda ScrollLeftPage
    sta EnemyXPositionHi,x

    ; Restore red palette color that is set to black for Link's shadow during boss explosions
    ldx #$00
    ldy PpuBuffer2Length
@CopyLoop:
    lda ResetRedPalettePayload,x
    sta PpuBuffer2Data,y
    inx
    iny
    cpx #$08
    bne @CopyLoop
    lda #$02
    sta PpuMacroSelector ; setting PPU macro 2
    dey
    sty PpuBuffer2Length
    ldx $10
    rts

.reloc
ResetRedPalettePayload:
    ; 8 byte palette payload for PPU macro
    .byte $3f, $18, $04, $0f, $06, $16, $30, $ff


; FixThunderbirdThunderDeath (author bkpkt_patrick)
; =================================================
; Don't soft-lock if tbird is killed by thunder
.segment "PRG7"

ExplosionHandler := $dcae
TbirdDeathHandler := $a3db
TbirdDeathFlag := $6e3f

.org $d5f5
    .word FixedTbirdExplosionHandler

.reloc
FixedTbirdExplosionHandler:
    lda WorldNumber
    cmp #$05
    bne @Explosion

    lda EnemyType,x
    cmp #$22
    bne @Explosion

    ; let an in-progress tbird death sequence continue
    lda TbirdDeathFlag
    bmi @Explosion

    ; Run the death setup skipped on a thunder-based kill.
    jmp TbirdDeathHandler

    @Explosion:
        jmp ExplosionHandler


; DarkenThunderbirdRoom (author bkpkt_patrick)
; ============================================
.if DARKEN_TBIRD_ROOM
.segment "PRG0"

; Hook into the thunder cast routine to check if in tbird room
.org $91e6
    jsr DarkenThunderbirdRoomIfLoaded

.segment "PRG7"

.reloc
DarkenThunderbirdRoomIfLoaded:
    lda WorldNumber
    cmp #$05
    bne @Done

    ; An empty slot can retain its previous enemy type.
    lda Enemy5Status
    cmp #$01
    bne @Done

    lda Enemy5Type
    cmp #$22
    beq DarkenThunderbirdRoom

    @Done:
        ; Re-run the instruction replaced by the hook.
        lda FireSpellActive
        rts

DarkenThunderbirdRoom:
    ; The flash command is already in this buffer. Append the background
    ; color so the same NMI processes both commands.
    ldy PpuBufferLength
    ldx #$00
    @CopyCommand:
        lda DarkenThunderbirdRoomCommand,x
        sta PpuAddrHi,y
        inx
        iny
        cpx #$06
        bne @CopyCommand

    dey
    sty PpuBufferLength

    lda FireSpellActive
    rts

.reloc
DarkenThunderbirdRoomCommand:
    .byte $3f, $05, $02, $0f, $0f, $ff
.endif


; Carock Flickering HP Bar Fix (author initsu)
; ============================================
; Carock gets shuffled around in all potential enemy sprite slots.
; This is normal for sideview enemies, but not for bosses.
; Other bosses override the shuffled sprite offset with a fixed number
; to prevent the enemy sprites overwriting the HP bar sprites.
.segment "PRG4"
.org $ae7b
    jsr PinCarockSpriteSlot
.reloc
PinCarockSpriteSlot:
    lda #$58
    sta SpriteShuffleOffsetEnemy0,x
    jmp $b20d  ; continue to original jsr


; BossEnterGoingLeftFix (author initsu)
; =====================================
.segment "PRG4"

REBO_UNHORSED = $0a
REBONAK = $20
BARBA = $21
CAROCK = $22

; Replacing this subroutine to remove hardcoding of page 1
bank4_Enemy_Init_Routines_Horsehead__Rebonack := $bca1
.org bank4_Enemy_Init_Routines_Horsehead__Rebonack
FREE_UNTIL $bcbc

CheckItemPresenceBitXInRoom := $c2a6

.reloc
BossCheckPresenceBit:
    ; output: carry set if this boss is already dead (slot freed)
    lda EnemyXPositionHi,x
    tax
    jsr CheckItemPresenceBitXInRoom
    bne @Alive
        ldx EnemyIndex
        sta EnemyStatus,x              ; A is 0 here from jsr
        sec
        rts
    @Alive:
    ldx EnemyIndex
    clc
    rts

.reloc
BossCheckBitsAndSetup:
    jsr BossCheckPresenceBit
    bcs @BossCheckBitsAndSetupDone
    ldy EnemyType,x
    lda #$02
    sta EnemyVulnerabilityDamageCodes,x
    lda #$07
    sta EnemySizeCodesRamCopy,y
@BossCheckBitsAndSetupDone:
    rts

.org $9485
    .word BossCheckBitsAndSetup
.org $bc7e
    jsr BossCheckBitsAndSetup

; replace vanilla hardcoded page 1 code
P346RebonackInitRoutine := $af91
.org P346RebonackInitRoutine
    jsr BossCheckPresenceBit
    bcs @Done
    lda #$00
    sta EnemySizeCodesRamCopy+REBO_UNHORSED   ; reset "size code" in case another Rebo has previously been killed in the palace
    sta $0504,x                     ; clear timer (like vanilla)
    lda #$ac                        ; rebo vanilla X start position
    sta EnemyYPositionLo,x
    inc EnemyXPositionHi,x          ; push Rebo one page right (off-screen if coming from the left)
@Done:
    rts
FREE_UNTIL $afb3                    ; $afb3 is used in a couple of places as the closest RTS - must keep

P346BarbaInitRoutine := $b0fe
P346CarockInitRoutine := $b107    ; also drop-through from Barba routine
.org P346CarockInitRoutine
    jmp BossCheckPresenceBit
FREE_UNTIL $b115

P125BossRoutineStart := $be8b   ; (Horsehead/Helmethead/Gooma)
P125BossRoutineMain := $be9c
P125BossRoutineEnd := $bef0
.org P125BossRoutineStart
    jsr BossFixedScrollCheck
    bcs P125BossRoutineEnd
    jsr ScrollScreenXToZero
    bne P125BossRoutineMain    ; skip setup when ScrollFrozen != 0
.assert * = $be95

P346BossRoutineStart := $b20d   ; (Carock/Rebonack/Barba)
P346BossRoutineMain := $b225
P346BossRoutineEnd := $b23c
.org P346BossRoutineStart
    jsr P346CallScrollCheck
    bcs P346BossRoutineEnd
    jsr ScrollScreenXToZero
    bne P346BossRoutineMain    ; skip setup when ScrollFrozen != 0
.assert * = $b217

.reloc
; Finer scroll position check to determine if boss should trigger
; output: Carry clear <=> Boss should be triggered
BossFixedScrollCheck:
    lda ScrollFrozen
    bne @AlreadyFrozen
    lda ScrollLeftPage
    cmp EnemyXPositionHi,x       ; trigger when the screen scrolls onto the current enemy's page
    bne @NotInPosition
    ;lda LinkXPositionLo         ; not supporting page 0 bosses just yet (they also softlock if they fall off the screen edge)
    ;bmi @NotInPosition
    ;cmp #$57                    ; additional check to make page 0 bosses work without triggering instantly (since you enter with the screen scrolled already in the page 0 center)
    ;bcc @NotInPosition
    lda ScrollLeftX
    cmp #$04                     ; need at least 3 pixel margin so dash speed can't skip the trigger point
    rts                          ; result of CMP determines if boss will spawn
@AlreadyFrozen:
    clc                          ; carry clear = continue boss routine
    rts
@NotInPosition:
    sec                          ; carry set = don't spawn boss
    rts

bank4_Enemy_Routines_Rebonak := $afbc
bank4_Update_Boss_HP_Bar_Segments := $9c45
bank7_Display := $ef11
.org bank4_Enemy_Routines_Rebonak
    lda ScrollFrozen
    beq @RebonakPostDraw                   ; skip more code here to not draw Rebo pre-fight
    lda #$02                               ; Rebo's index in the boss table
    jsr bank4_Update_Boss_HP_Bar_Segments
    lda EnemySuspendTimer
    jsr RebonakSuspendedScrollHook
    nop
    nop
@RebonakPostDraw:
.assert * = $afce

.reloc
RebonakSuspendedScrollHook:
    ; lda EnemySuspendTimer (before calling)
    beq @RebonakContinue
        pla                                ; remove top call stack layer
        pla
        jmp ScrollScreenXToZero            ; reached while Rebo is suspended (timer before he charges in the first time)
@RebonakContinue:
    jmp bank7_Display

; output: ScrollFrozen in A
ScrollScreenXToZero:
    lda FrameCounter
    and #$01
    bne @NoScroll                ; slow down screen scroll to every other frame
    clc                          ; ensure sbc subtracts 1 regardless of carry state on entry
    lda ScrollLeftX
    sbc #$00                     ; A-=1 (due to sbc with carry clear)
    bcs @SetScrollX
        lda #$00                 ; clamp to 0
@SetScrollX:
    sta ScrollLeftX
    sta ScrollPosShadow
@NoScroll:
    lda ScrollFrozen
    rts

.reloc
P346CallScrollCheck:
    lda EnemyType,x
    cmp #REBONAK
    bne @NoPageOffset
    dec EnemyXPositionHi,x
    jsr BossFixedScrollCheck
    inc EnemyXPositionHi,x
    rts
@NoPageOffset:
    jmp BossFixedScrollCheck

.org $b220 ; Screen lock set at bank 4 B220 (0x13230)
    jsr ElevatorBossFix          ; will freeze the scrolling (and more)

.org $be99 ; Screen lock set at bank 4 BE99 (0x13ea9)
    jsr ElevatorBossFix          ; will freeze the scrolling (and more)

HELMETHEAD_HEAD = $22                ; Unambiguous in P125

ENEMY_STATUS_ALIVE = 1

; Do not kill every other enemy when a P125 boss dies, only floating heads.
P125BossDefeatSweep := $beb1
P125BossDefeatFinish := $bee8
bank7_ApplyDamage := $e726
bank7_BuildSpriteScreenX_And_OffscreenMask := $f27d

.org P125BossDefeatSweep
    ldx #$05
@P125BossDefeatSweepLoop:
    lda #$00
    sta Projectile0Type,x
    lda EnemyType,x
    cmp #HELMETHEAD_HEAD
    bne @P125BossDefeatSweepNext
    stx EnemyIndex
    lda EnemyStatus,x
    cmp #ENEMY_STATUS_ALIVE
    bne @P125BossDefeatSweepNext
    inx
    ldy #$01
    jsr bank7_BuildSpriteScreenX_And_OffscreenMask
    ldx EnemyIndex
    ldy #$0a                        ; do 10 damage to floating heads per frame (vanilla)
    jsr bank7_ApplyDamage
@P125BossDefeatSweepNext:
    dex
    bpl @P125BossDefeatSweepLoop
    bmi P125BossDefeatFinish
FREE_UNTIL P125BossDefeatFinish

; (P346 bosses do not need to be patched, they don't clear anything on death)

; Do not allow crystal to be placed while boss is alive
EnemyTouchingLinkDetection := $e4d9
PalaceCrystalUnplacedRoutine := $9aeb
.org $9af4  ; was jsr EnemyTouchingLinkDetection
    jsr CrystalCheckIfBossDead

.reloc
CrystalCheckIfBossDead:
    ldx #1   ; unsure how to not hardcode page 1 here
    jsr CheckItemPresenceBitXInRoom
    beq @BossDead
    ldx zp_10
    pla      ; skip one return layer
    pla
    rts
@BossDead:
    ldx zp_10
    jmp EnemyTouchingLinkDetection


.segment "PRG5"

DrawThunderbird := $9ebf
ThunderbirdMainRoutineStart := $a359
ThunderbirdMainRoutine := $a36b
ThunderbirdNoRoutine := $a3bd

; Do a much finer scroll position check so we can get into position
; when entering from the right as well.
.org $94d1
    .word ThunderbirdFixedScrollCheck

; When entering from the right, Thunderbird would be visible before the combat starts.
; While it's not a problem, it feels a bit jank, so lets not draw Thunderbird pre-battle.
.org $95a9
    .word ConditionalDrawThunderbird

; Only clear projectiles when Thunderbird dies. Saves a lot of bytes - no logic change
; (Also makes it possible to put a King Bot in the empty space behind Thunderbird ;) )
ThunderbirdBossDefeatSweep := $a37e
ThunderbirdBossDefeatFinish := $a3b5
.org ThunderbirdBossDefeatSweep
    ldx #$05
    lda #$00
@ThunderbirdBossDefeatLoop:
    sta Projectile0Type,x
    dex
    bpl @ThunderbirdBossDefeatLoop
    bmi ThunderbirdBossDefeatFinish
FREE_UNTIL ThunderbirdBossDefeatFinish

.org ThunderbirdMainRoutineStart
FREE_UNTIL ThunderbirdMainRoutine

.reloc
ThunderbirdFixedScrollCheck:
    lda ScrollLeftPage
    cmp #$01
    bne @DontSpawn

    lda ScrollLeftX
    cmp #$04                     ; need at least 3 pixel margin so dash speed can't skip the trigger point
    bcs @DontSpawn

    lda ScrollFrozen
    bne @TimerRunning
        jsr ElevatorBossFix      ; will freeze the scrolling (and more)
        lda #$90
        sta $0504,x              ; set timer for Thunderbird to begin

    @TimerRunning:
    lda ScrollLeftX
    beq @NoScroll

    lda FrameCounter
    and #$01 
    bne @NoScroll                ; slow down screen scroll to every other frame

    lda ScrollLeftX
    sbc #$00                     ; subtracts 1 because carry is cleared by cmp
    bcs @SetScrollX
        lda #$00                 ; clamp to 0
    @SetScrollX:
    sta ScrollLeftX
    sta ScrollPosShadow
    @NoScroll:
        jmp ThunderbirdMainRoutine
    @DontSpawn:
        jmp ThunderbirdNoRoutine

.reloc
ConditionalDrawThunderbird:
    lda ScrollFrozen
    beq @NotInBattle
        jmp DrawThunderbird
    @NotInBattle:
        rts
