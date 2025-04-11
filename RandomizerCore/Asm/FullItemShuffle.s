.include "z2r.inc"

; This is a list of items that are potentially replaced. The randomizer must set these values to the replacement value
;.import SHIELD_SPELL_ITEMLOC, JUMP_SPELL_ITEMLOC, LIFE_SPELL_ITEMLOC, FAIRY_SPELL_ITEMLOC
;.import REFLECT_SPELL_ITEMLOC, FIRE_SPELL_ITEMLOC, SPELL_SPELL_ITEMLOC, THUNDER_SPELL_ITEMLOC
;.import UPSTAB_ITEMLOC, DOWNSTAB_ITEMLOC, MIRROR_ITEMLOC, BAGU_ITEMLOC, WATER_ITEMLOC

; These are two unused locations in the save game that we can use for custom item get flags
LocationTableMisc = $0795
LocationTableWizard = $077a

.segment "PRG3"

DialogConditionsDefault = $b5c7
DownStabGetItem = $b4bf
UpStabGetItem = $b4d7
TalkWithStandingStillNpc = $b554
MirrorWaterGetItem = $b5ae
WizardDialogGetItem = $b518
WizardDialogCheckSecondTimeTalking = $B645

; This is in the middle of the normal get item check after it checks if its a item
; that uses the $600 flags or the $700 flags
GetItem = $e781
KillEnemy = $dd47
EnemyDeath = $e880

; This is used for item obtained for $600 table, and its good enough
; to work for our new item obtained tables
; $F7,$FB,$FD,$FE,$7F,$BF,$DF,$EF
JankPowerOfTwoMask = $c28d
; $08,$04,$02,$01,$80,$40,$20,$10
JankPowerOfTwo = $c28d

.org MirrorWaterGetItem
    jmp MirrorOrWaterLocation
FREE_UNTIL $b5b9

.reloc
; The game uses the same enemy id to set the flags for the
; mirror and the water object, so check here for which town we are in.
MirrorOrWaterLocation:
    cpy #4 ; Nabooru
    bne @Mirror
        lda #WATER_ITEMLOC
        bpl @Exit ; unconditional
@Mirror:
    lda #MIRROR_ITEMLOC
@Exit:
    jmp CheckGetItemCustomLocationMisc

; This spot is used for setting flags of several different standing NPCs
; but we can hack it just for bagu too
.org TalkWithStandingStillNpc
    jsr HandleBaguItem
    
.reloc
HandleBaguItem:
    ; X is the adjusted NPC id and Y is the Town id
    ; Bagu is x=0 y=3
    cpx #0
    bne @Exit
    cpy #3
    bne @Exit
    ; double return to skip setting the vanilla bagu flag
    ; and then use the custom get item check for this location
    pla
    pla
    lda #BAGUS_NOTE_ITEMLOC
    jmp CheckGetItemCustomLocationMisc
@Exit:
    lda $0797,y
    rts

.org DownStabGetItem
    lda #DOWNSTAB_ITEMLOC
    ldy #0
    jmp CheckGetItemCustomLocationMisc
FREE_UNTIL $b4d3

.org UpStabGetItem
    lda #UPSTAB_ITEMLOC
    ldy #1
    jmp CheckGetItemCustomLocationMisc
FREE_UNTIL $b4eb


.if _CHECK_WIZARD_MAGIC_CONTAINER
.reloc
WizardMagicContainerRequirement:
    .byte $01,$02,$03,$04,$05,$06,$07,$08
.endif

.if _DO_SPELL_SHUFFLE_WIZARD_UPDATE

; The game has a check to see if it should clear the learn spell flash timer
; in a different spot. It clears it if you already have learned the spell before
.org WizardDialogCheckSecondTimeTalking
    jsr CheckIfWeAlreadyHaveItemForTown
.reloc
CheckIfWeAlreadyHaveItemForTown:
    lda LocationTableWizard
    and JankPowerOfTwo,y
    rts

.org WizardDialogGetItem
    lda LocationTableWizard
    and JankPowerOfTwo,y
    bne @AlreadyHaveItem
        ; check if the magic container requirement is enabled
.if _CHECK_WIZARD_MAGIC_CONTAINER
        lda $0783
        cmp WizardMagicContainerRequirement,y
        bcc @EarlyExit
.endif
        ; Earning the item so move the dialog to the next state
        inc $05
        lda LocationTableWizard
        ora JankPowerOfTwo,y
        sta LocationTableWizard
        lda TownToItemTable,y
        jmp GetItemDontKillEnemy
@AlreadyHaveItem:
    inc $048c ; Use the 'Already have item' dialog
@EarlyExit:
    jmp DialogConditionsDefault
    FREE_UNTIL $b54e
.endif

.reloc
TownToItemTable:
    .byte SHIELD_SPELL_ITEMLOC
    .byte JUMP_SPELL_ITEMLOC
    .byte LIFE_SPELL_ITEMLOC
    .byte FAIRY_SPELL_ITEMLOC
; For some reason, when dash replaces fire it even replaces the "vanilla" spell
.if _REPLACE_FIRE_WITH_DASH
    .byte DASH_SPELL_ITEMLOC
.else
    .byte FIRE_SPELL_ITEMLOC
.endif
    .byte REFLECT_SPELL_ITEMLOC
    .byte SPELL_SPELL_ITEMLOC
    .byte THUNDER_SPELL_ITEMLOC

; Update a few hardcoded positions in the rom to change "mirror" objects in Darunia and Mido into sign objects
; Change the mirror object $21 to sign object $22. the high two bits are for X position
.org $88ee ; Mido Left
    .byte $22
.org $890c ; Mido Right
    .byte $c0 | $22
.org $8954 ; Darunia Left
    .byte $22
.org $8974 ; Darunia Right
    .byte $c0 | $22
.org $89b9 ; Old Kasuto
    .byte $c0 | $22

; Text uses the object id as an offset into a lookup table for text, so since the object id changed, we need
; to update the offset table as well to point to the original text. Subtract 1 from each of the vanilla data
.org $a29b
    .byte $2a - 1 ; Mido
.org $a33d
    .byte $19 - 1 ; Darunia
.org $a33f
    .byte $2a ; Old Kasuto

; After changing the main signs in the front of the town to be the "sign" object $22, this broke the hints
; inside the houses which use the sign object in vanilla. For consistency sake, we change the signs in the
; houses to the "invisible dialog" object $0c

.org $89a8
    .byte $0c | $80 ; Update Darunia hint sign to an invis dialog object (at Y=8)
.org $a2e5
    .byte $19 ; And use the original "sign" hint text that we moved

.org $89a8
    .byte $0c | $80 ; Update Old kasuto hint sign to an invis dialog object (at Y=9)
.org $a2e7
    .byte $16 ; And use the original "sign" hint text that we moved


.segment "PRG7"

; Patch GetItem to add stat tracking
.import CheckAddItemTimestamp
.org GetItem
    jsr CheckAddItemTimestamp

.reloc
; Check first if we've already gotten this item from a custom location before
; we call the get item for this spot
AlreadyHaveItemAtLocationExit:
    pla
    jmp DialogConditionsDefault
CheckGetItemCustomLocationMisc:
    ; Y contains the location id 0 - 4
    ; A contains the new item id
    pha
        lda LocationTableMisc
        and JankPowerOfTwo,y
        bne AlreadyHaveItemAtLocationExit
            lda LocationTableMisc
            ora JankPowerOfTwo,y
            sta LocationTableMisc
             ; restore the item id
            pla
; Add a new entry point to GetItem that sets a temporary flag to denote that this
; item should not kill the enemy at this index
GetItemDontKillEnemy:
    dec DontKillEnemyFlag
    jsr GetItem
    jmp DialogConditionsDefault

; Patch the return calls for the item get and only clear the enemy if the flag isn't set
.org $e797
    jmp GetItemReturn
.org $e7b8
    jmp GetItemReturn
.org $e7ed
    jmp GetItemReturn
.org $e86d
    jmp GetItemReturn

.reloc
GetItemReturn:
    bit DontKillEnemyFlag
    bmi @DontKillEnemy
        jmp KillEnemy
@DontKillEnemy:
    lda #0
    sta DontKillEnemyFlag
    rts

; If the item is a PBag, and there is no enemy, then we need custom
; pbag handling so that it puts the experience in the right spot.
; We pull this off by searching for an empty enemy slot and creating an enemy
; thats already in the "give exp" state

.org $e7f4
    ; Remove reloading the item id ($af) by saving it to the stack.
    tya
    pha
    jsr HandlePBagDeath
    pla
    tay
    nop
FREE_UNTIL $e7fc ; This is just an assert that we didn't go past the size

.reloc
HandlePBagDeath:
    bit DontKillEnemyFlag
    bmi @DontKillEnemy
        jmp EnemyDeath
    ; There's no enemy to kill, so just manually set the exp
@DontKillEnemy:
    ; we create a new enemy in an empty slot that has the exp temporarily set x to be the new enemy id
    ldy #5
@FindEmptySlotLoop:
        lda $b6,y
        beq @FoundEmpty
        dey
        bpl @FindEmptySlotLoop
    ; We didn't find an empty slot, so just kill the current object?
    ; This particular case can happen if we find a PBag on the water fountain
    ; since there a lot of NPCs walking around.
    bmi @Exit ; unconditional
@FoundEmpty:
    lda $20,x ; enemy y hi
    sta $20,y
    lda $2a,x ; enemy y lo
    sta $2a,y
    lda $3c,x ; enemy x hi
    sta $3c,y
    lda $4e,x ; enemy x lo
    sta $4e,y
    ; don't reset DontKillEnemyFlag here since its needed in HandlePBagTimerClear
    tya
    tax
@Exit:
    lda #1
    sta $a1,x ; set the proper id for a dropped item
    lda #$20 * 2 ; double the usual amount of time so people are more likely to catch how much it is
    sta $0504,x ; set the timer for the numbers
    lda #3
    sta $b6,x ; set the "give exp" state
    rts

.org $e808
    jsr HandlePBagTimerClear
.reloc
HandlePBagTimerClear:
    bit DontKillEnemyFlag
    bpl @Exit
        ; restore the original enemy id and reset the flag.
        ; The code here is mostly to prevent from clearing the timer we just set
        ldx $10
        lda #0
        sta DontKillEnemyFlag
        rts
@Exit:
    ; Vanilla behavior is to clear any existing timers. They'll be set later
    sta $0504,x
    rts

; Patch the end of the get item routine to add handlers for the rest of the checks
.org $e847
    jmp ExpandedGetItem 
    nop
.reloc
ExpandedGetItem:
;    cpy #ITEM_RED_MAGIC_JAR + 1
;    bcs @NotJar 
;        
;@NotJar:
    cpy #ITEM_SHIELD_SPELL
    bcc @NotSpell
        cpy #ITEM_DASH_SPELL
        bcc @NotDashSpell
            ; Dash always replaces fire, so if we get the dash spell load fire instead
            ldy #ITEM_FIRE_SPELL
    @NotDashSpell:
        ; flash screen as if you got the spell from a wizard 
        lda #$c0
        sta $074b
        ; Convert from expanded item ID to regular spell ID 
        tya
        sec
        sbc #ITEM_SHIELD_SPELL
        pha
        ; Now check to see if this is the first spell we've learned.
        ; if it is, then we want to update the cursor to its position
        ldy #7
    @CheckAllSpells:
            lda $077b,y
            ; if nonzero, ignore the spell and restore the spell index
            bne @FoundLearnedSpell
            dey
            bpl @CheckAllSpells
        ; Update the cursor position to point to the new spell
        pla
        sta $0749
        .byte $24 ; OPCODE bit $zp (hides pla)
    @FoundLearnedSpell:
        pla
    @Continue:
        tay ; Restore the learned spell to Y here
        lda #1
        sta $077b,y
        bne @Exit ; unconditional
@NotSpell:
    cpy #ITEM_UPSTAB
    beq @ItemStab
    cpy #ITEM_DOWNSTAB
    beq @ItemStab
    cpy #ITEM_BAGU
    beq @ItemBagu
    cpy #ITEM_MIRROR
    beq @ItemMirror
    cpy #ITEM_WATER
    beq @ItemWater
    ; Red jar or blue jar
    lda #8
    sta $ef
    jmp $e84b ; Red/blue jar handler
@ItemStab:
    lda $0796
    ora @StabTable - ITEM_UPSTAB,y
    sta $0796
    bne @Exit ; unconditional
@ItemBagu:
    lda $079a
    ora #8
    sta $079a
    bne @Exit ; unconditional
@ItemMirror:
    lda $0799
    ora #1
    sta $0799
    bne @Exit ; unconditional
@ItemWater:
    lda $079b
    ora #1
    sta $079b
    ; fallthrough
@Exit:
    jmp GetItemReturn
@StabTable:
    ;     Down, Up
    .byte $04, $10

; Repoint the pointers that went to the original enemy/item tile table
.org $eef4
    lda EnemyExplosionAnimationTable,x
.org $f219
    lda CommonEnemyTileTable,x
.org $f221
    lda CommonEnemyTileTable+1,x
.org $f22e
    lda CommonEnemyTileTable+1,x 


; Clear out the space from the original enemy and item tile table
.org $ee51
FREE_UNTIL $eeb2
.reloc
EnemyExplosionAnimationTable:
    .byte $00,$02,$02,$00,$00
.reloc
CommonEnemyTileTable:
    .byte $86, $86 ; Explosion Frame 1
    .byte $88, $88 ; Explosion Frame 2
    .byte $DC, $DE ; 
    .byte $AC, $AC ; Elevator
    .byte $E4, $E6 ; Hammer / Fokkeru body frame 1
    .byte $E8, $EA ; Hammer / Fokkeru frame 2
    .byte $C0, $C2 ; Bago bago frame 1
    .byte $C0, $C4 ; Bago Bago frame 2
    .byte $BA, $BC ; Spider
    .byte $C6, $C8 ; Octorok / Spitter Frame 1
    .byte $CA, $CC ; Octorok / Spitter Frame 2
    .byte $50, $50 ; Bat Frame 1
    .byte $4E, $4E ; Bat Frame 2
    .byte $7C, $7E ; Fire Bat Head
    .byte $80, $82 ; Fire Bat Body
    .byte $B6, $B8 ; Moa
    .byte $BB, $BD ; Falling Block
    .byte $BF, $C1 ; Crumble Block 1
    .byte $C3, $C3 ; Crumble Block 2
    .byte $B2, $B2 ; Bot Frame 1
    .byte $B4, $B4 ; Bot Frame 2
    .byte $24, $24 ; Spike top frame 1
    .byte $B0, $B0 ; Spike top frame 2
ItemTileTable:
    .byte $8C, $F5 ; Candle
    .byte $8E, $F5 ; Glove
    .byte $90, $F5 ; Raft
    .byte $92, $F5 ; Boot
    .byte $94, $F5 ; Flute
    .byte $96, $F5 ; Cross
    .byte $98, $F5 ; Hammer
    .byte $9A, $F5 ; Magic Key
    .byte $66, $F5 ; Key
    .byte $00, $F5 ; Invalid Index
    .byte $72, $F5 ; Experience Bag - 50 pts
    .byte $72, $F5 ; Experience Bag - 100 pts
    .byte $72, $F5 ; Experience Bag - 200 pts
    .byte $72, $F5 ; Experience Bag - 500 pts
    .byte $83, $83 ; Magic Container
    .byte $81, $81 ; Heart Container
    .byte $8A, $F5 ; Blue Jar
    .byte $8A, $F5 ; Red Jar
    .byte $A8, $F5 ; Link Doll
    .byte $c9, $c9 ; Child
    .byte $f3, $f3 ; Trophy
    .byte $df, $df ; Medicine
    .byte $67, $67 ; Antifairy
    .byte $e1, $f5 ; Upstab
    .byte $e3, $f5 ; Downstab
    .byte $e9, $f5 ; Bagu
    .byte $e7, $f5 ; Mirror
    .byte $ed, $f5 ; Water
    .byte $fb, $fb ; Shield Spell
    .byte $d3, $d5 ; Jump Spell
    .byte $d7, $d9 ; Life Spell
    .byte $cb, $cd ; Fairy Spell
    .byte $cf, $d1 ; Fire Spell
    .byte $db, $dd ; Reflect Spell
    .byte $f1, $f1 ; Spell Spell
    .byte $f7, $f9 ; Thunder Spell
    .byte $c7, $92 ; Dash Spell

; In vanilla this table is limited only to the 6 items in $10-$16
; We can keep the first $10 items using the original, and just expand the rest
.org $f120
    lda ItemPaletteTable - $10,x
.reloc
ItemPaletteTable:
    .byte $03 ; Blue Jar
    .byte $02 ; Red Jar
    .byte $00 ; Link Doll
    .byte $01 ; Child
    .byte $01 ; Trophy
    .byte $01 ; Medicine
    .byte $ff ; DONT USE ANTIFAIRY
    .byte $01 ; Upstab
    .byte $01 ; Downstab
    .byte $01 ; Bagu
    .byte $01 ; Mirror
    .byte $01 ; Water
    .byte $01 ; Shield Spell
    .byte $01 ; Jump Spell
    .byte $01 ; Life Spell
    .byte $01 ; Fairy Spell
    .byte $01 ; Fire Spell
    .byte $01 ; Reflect Spell
    .byte $01 ; Spell Spell
    .byte $01 ; Thunder Spell
    .byte $01 ; Dash Spell
