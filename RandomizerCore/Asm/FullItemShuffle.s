

; This is a list of items that are potentially replaced. The randomizer must set these values to the replacement value
.import SHIELD_SPELL_ITEM, JUMP_SPELL_ITEM, LIFE_SPELL_ITEM, FAIRY_SPELL_ITEM, REFLECT_SPELL_ITEM, FIRE_SPELL_ITEM, SPELL_SPELL_ITEM, THUNDER_SPELL_ITEM
.import MAGIC_CONTAINER_ITEM, HEART_CONTAINER_ITEM
.import CANDLE_ITEM, GLOVE_ITEM, RAFT_ITEM, BOOTS_ITEM, FLUTE_ITEM, CROSS_ITEM, HAMMER_ITEM, MAGIC_KEY_ITEM
.import UPSTAB_ITEM, DOWNSTAB_ITEM, TROPHY_ITEM, MIRROR_ITEM, BAGU_ITEM, MEDICINE_ITEM, WATER_ITEM, CHILD_ITEM

; Hardcoded list of item ids for the new global item table.
; These should not be changed (if we need to add new ones they should go on the end)
_CURRENT_ITEM_ID .set 0
.macro ITEM_GEN Name
.ident (.sprintf("ITEM_%s", Name) = _CURRENT_ITEM_ID 
_CURRENT_ITEM_ID .set _CURRENT_ITEM_ID + 1
.endmacro

ITEM_GEN "CANDLE"
ITEM_GEN "GLOVE"
ITEM_GEN "RAFT"
ITEM_GEN "BOOTS"
ITEM_GEN "FLUTE"
ITEM_GEN "CROSS"
ITEM_GEN "HAMMER"
ITEM_GEN "MAGIC_KEY"

ITEM_GEN "KEY"
ITEM_GEN "DO_NOT_USE"

ITEM_GEN "SMALL_PBAG"
ITEM_GEN "MEDIUM_PBAG"
ITEM_GEN "LARGE_PBAG"
ITEM_GEN "XL_PBAG"
ITEM_GEN "MAGIC_CONTAINER"
ITEM_GEN "HEART_CONTAINER"
ITEM_GEN "BLUE_MAGIC_JAR"
ITEM_GEN "RED_MAGIC_JAR"
ITEM_GEN "1UP"
ITEM_GEN "CHILD"
ITEM_GEN "TROPHY"
ITEM_GEN "MEDICINE"

ITEM_GEN "UPSTAB"
ITEM_GEN "DOWNSTAB"
ITEM_GEN "MIRROR"
ITEM_GEN "BAGU"
ITEM_GEN "WATER"

ITEM_GEN "SHIELD_SPELL"
ITEM_GEN "JUMP_SPELL"
ITEM_GEN "LIFE_SPELL"
ITEM_GEN "FAIRY_SPELL"
ITEM_GEN "FIRE_SPELL"
ITEM_GEN "REFLECT_SPELL"
ITEM_GEN "SPELL_SPELL"
ITEM_GEN "THUNDER_SPELL"

.segment "PRG3"

DialogConditionsDefault = $b5c7
MirrorGetItem = $b5ae
WizardDialogGetItem = $b518
DontKillEnemyFlag = $0122

; This is in the middle of the normal get item check after it checks if its a item
; that uses the $600 flags or the $700 flags
GetItem = $e781
KillEnemy = $dd47
EnemyDeath = $e880

.org MirrorGetItem
    lda #MIRROR_ITEM
    jsr GetItemDontKillEnemy
    jmp DialogConditionsDefault
FREE_UNTIL $b5b9

.org WizardDialogGetItem
    lda TownToItemTable,y
    jsr GetItemDontKillEnemy
    jmp DialogConditionsDefault
    FREE_UNTIL $b54e

.reloc
TownToItemTable:
    .byte SHIELD_SPELL_ITEM
    .byte JUMP_SPELL_ITEM
    .byte LIFE_SPELL_ITEM
    .byte FAIRY_SPELL_ITEM
    .byte REFLECT_SPELL_ITEM
    .byte FIRE_SPELL_ITEM
    .byte SPELL_SPELL_ITEM
    .byte THUNDER_SPELL_ITEM

.segment "PRG7"

; Add a new entry point to GetItem that sets a temporary flag to denote that this
; item should not kill the enemy at this index
.reloc 
GetItemDontKillEnemy:
    dec DontKillEnemyFlag
    jmp GetItem

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
; pbag handling so that it puts the experience in the right spot
; Also remove reloading the item id by saving it to the stack.
.org $e7f4
    tya
    pha
    jsr HandlePBagDeath
    pla
    tay
    nop
FREE_UNTIL $e7fc

.reloc
HandlePBagDeath:
    bit DontKillEnemyFlag
    bmi @DontKillEnemy
        jmp EnemyDeath
    ; There's no enemy to kill, so just manually set the exp
@DontKillEnemy:
    ; don't reset the flag here since its needed in HandlePBagTimerClear
    rts

.org $e808
    jsr HandlePBagTimerClear
.reloc
HandlePBagTimerClear:
    bit DontKillEnemyFlag
    bmi @DontKillEnemy
        sta $0504,x
        rts
@DontKillEnemy:
    lda #0
    sta DontKillEnemyFlag
    rts


; Clear out the space from the original item tile table
.org $ee7f
FREE_UNTIL $eeb2
.reloc
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
    .byte $31, $31 ; Child
    .byte $2F, $2F ; Trophy
    .byte $31, $31 ; Medicine
    .byte $e1, $f5 ; Upstab
    .byte $e3, $f5 ; Downstab
    .byte $e7, $f5 ; Mirror
    .byte $e9, $f5 ; Bagu
    .byte $ed, $f5 ; Water
    .byte $fb, $fd ; Shield Spell
    .byte $d3, $d5 ; Jump Spell
    .byte $d7, $d9 ; Life Spell
    .byte $cb, $cd ; Fairy Spell
    .byte $cf, $d1 ; Fire Spell
    .byte $db, $dd ; Reflect Spell
    .byte $f1, $f3 ; Spell Spell
    .byte $f7, $f9 ; Thunder Spell

; In vanilla this table is limited only to the 6 items in $10-$16
; We can keep the first $10 items using the original, and just expand the rest
.org $f120
    lda ItemPaletteTable,x
.reloc
ItemPaletteTable:
    .byte $03 ; Blue Jar
    .byte $02 ; Red Jar
    .byte $00 ; Link Doll
    .byte $01 ; Child
    .byte $01 ; Trophy
    .byte $01 ; Medicine
    .byte $01 ; Upstab
    .byte $01 ; Downstab
    .byte $01 ; Mirror
    .byte $01 ; Bagu
    .byte $01 ; Water
    .byte $01 ; Shield Spell
    .byte $01 ; Jump Spell
    .byte $01 ; Life Spell
    .byte $01 ; Fairy Spell
    .byte $01 ; Fire Spell
    .byte $01 ; Reflect Spell
    .byte $01 ; Spell Spell
    .byte $01 ; Thunder Spell
