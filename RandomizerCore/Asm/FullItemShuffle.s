

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

ITEM_GEN "SHIELD_SPELL"
ITEM_GEN "JUMP_SPELL"
ITEM_GEN "LIFE_SPELL"
ITEM_GEN "FAIRY_SPELL"
ITEM_GEN "REFLECT_SPELL"
ITEM_GEN "FIRE_SPELL"
ITEM_GEN "SPELL_SPELL"

ITEM_GEN "CANDLE"
ITEM_GEN "GLOVE"
ITEM_GEN "RAFT"
ITEM_GEN "BOOTS"
ITEM_GEN "FLUTE"
ITEM_GEN "CROSS"
ITEM_GEN "HAMMER"
ITEM_GEN "MAGIC_KEY"

ITEM_GEN "UPSTAB"
ITEM_GEN "DOWNSTAB"
ITEM_GEN "TROPHY"
ITEM_GEN "MIRROR"
ITEM_GEN "BAGU"
ITEM_GEN "MEDICINE"
ITEM_GEN "WATER"
ITEM_GEN "CHILD"

ITEM_GEN "MAGIC_CONTAINER"
ITEM_GEN "HEART_CONTAINER"
ITEM_GEN "SMALL_KEY"
ITEM_GEN "BLUE_MAGIC_JAR"
ITEM_GEN "RED_MAGIC_JAR"
ITEM_GEN "SMALL_PBAG"
ITEM_GEN "MEDIUM_PBAG"
ITEM_GEN "LARGE_PBAG"
ITEM_GEN "XL_PBAG"
ITEM_GEN "1UP"

.segment "PRG3"

DialogConditionsDefault = $b5c7
MirrorGetItem = $b5ae

.org MirrorGetItem
    lda #MIRROR_ITEM
    jsr SuperGetItem
    jmp DialogConditionsDefault
FREE_UNTIL $b5b9

.segment "PRG7"

.reloc
; Global item handler for drawing this item
SuperDrawItemEnemy:
    rts

.reloc
; Global Item handler to process setting the correct flags when getting an item
SuperGetItem:
    rts


.org $ee7f
FREE_UNTIL 
bank7_table_item_tile_mapping:
; Tile Mappings for Items (2E bytes)                                           ;
;                                                                              ;
; 8C F5	Candle                                                                 ;
; 8E F5	Glove                                                                  ;
; 90 F5	Raft                                                                   ;
; 92 F5	Boot                                                                   ;
; 94 F5	Flute                                                                  ;
; 96 F5	Cross                                                                  ;
; 98 F5	Hammer                                                                 ;
; 9A F5	Magic Key                                                              ;
; 66 F5	Key                                                                    ;
; 00 F5	Invalid Index                                                          ;
; 72 F5	Experience Bag - 50 pts                                                ;
; 72 F5	Experience Bag - 100 pts                                               ;
; 72 F5	Experience Bag - 200 pts                                               ;
; 72 F5	Experience Bag - 500 pts                                               ;
; 83 83	Magic Container                                                        ;
; 81 81	Heart Container                                                        ;
; 8A F5	Blue Jar                                                               ;
; 8A F5	Red Jar                                                                ;
; A8 F5	Link Doll                                                              ;
; 31 31	Medicine                                                               ;
; 2F 2F	Trophy                                                                 ;
; 31 31	Kidnapped Child                                                        ;
; 67 67	Invalid Index ?                                                        ;