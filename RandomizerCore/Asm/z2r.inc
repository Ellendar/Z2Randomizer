.macpack common

; janky macro res till it starts working
.macro SET_RES_BASE addr
    RES_BASE .set addr
    RES_OFFSET .set 0
.endmacro

.macro RESV name, size
    .ident(.string(name)) = RES_BASE + RES_OFFSET
    .ifnblank size
        RES_OFFSET .set RES_OFFSET + size
    .else
        RES_OFFSET .set RES_OFFSET + 1
    .endif
.endmacro

; compile time array definition
.macro DEFINE_ARRAY name
    .macro .ident(.concat(.string(name), "_APPEND")) value
        .local temp
        .define temp name
        .undefine name
        .ifblank temp
            .define name value
        .else
            .define name temp, value
        .endif
        .undefine temp
    END_MAC

    .define name
.endmacro
.define END_MAC .endmacro

;;; Standard register names
PPUCTRL = $2000
PPUMASK = $2001
PPUSTATUS = $2002
OAMADDR = $2003
OAMDATA = $2004
PPUSCROLL = $2005
PPUADDR = $2006
PPUDATA = $2007
OAMDMA = $4014

JOY1 = $4016
JOY2 = $4017

;;; MMC5 register names and associated constants
PrgBankModeReg = $5100
PRG_BANK_MODE_8KB_BANKS = 3

ChrBankModeReg = $5101
CHR_BANK_MODE_8KB_BANKS = 0
CHR_BANK_MODE_4KB_BANKS = 1
CHR_BANK_MODE_2KB_BANKS = 2
CHR_BANK_MODE_1KB_BANKS = 3

PrgRamProtReg1 = $5102
PRG_RAM_UNPROTECT1_VALUE = 2

PrgRamProtReg2 = $5103
PRG_RAM_UNPROTECT2_VALUE = 1

ExRamModeReg = $5104
EXRAM_EXT_ATTR_MODE = 1
EXRAM_RAM_MODE = 2

NameTableModeReg = $5105
HORIZ_MIRROR_MODE = $50
VERT_MIRROR_MODE = $44

FillModeTileReg = $5106
FillModeColorReg = $5107
PrgRamBankReg = $5113
PrgBank8Reg = $5114
PrgBankAReg = $5115
PrgBankCReg = $5116
PrgBankEReg = $5117
PRG_BANK_ROM = $80

SpChrBank0Reg = $5120
SpChrBank1Reg = $5121
SpChrBank2Reg = $5122
SpChrBank3Reg = $5123
SpChrBank4Reg = $5124
SpChrBank5Reg = $5125
SpChrBank6Reg = $5126
SpChrBank7Reg = $5127
BgChrBank0Reg = $5128
BgChrBank1Reg = $5129
BgChrBank2Reg = $512a
BgChrBank3Reg = $512b

VertSplitMode = $5200
DISABLE_VERT_SPLIT_MODE = 0

LineIrqTgtReg = $5203
LineIrqStatusReg = $5204
DISABLE_SCANLINE_IRQ = 0
ENABLE_SCANLINE_IRQ = $80

MulLoReg = $5205
MulHiReg = $5206

;; Common locations used in the code
PpuCtrlShadow = $ff
ScrollPosShadow = $fd

PalaceNumber = $056c
RegionNumber = $0706
WorldNumber = $0707

SideViewInit = $8CE1
ClearNametables = $d266
JumpEngine = $d385

;; Collectable items
; Hardcoded list of item ids for the new global item table.
; These should match the constants from the Collectables Enum
ITEM_CANDLE = $00
ITEM_GLOVE = $01
ITEM_RAFT = $02
ITEM_BOOTS = $03
ITEM_FLUTE = $04
ITEM_CROSS = $05
ITEM_HAMMER = $06
ITEM_MAGIC_KEY = $07

ITEM_KEY = $08
ITEM_DO_NOT_USE = $09

ITEM_SMALL_PBAG = $0a
ITEM_MEDIUM_PBAG = $0b
ITEM_LARGE_PBAG = $0c
ITEM_XL_PBAG = $0d
ITEM_MAGIC_CONTAINER = $0e
ITEM_HEART_CONTAINER = $0f
ITEM_BLUE_MAGIC_JAR = $10
ITEM_RED_MAGIC_JAR = $11
ITEM_1UP = $12
ITEM_CHILD = $13
ITEM_TROPHY = $14
ITEM_MEDICINE = $15
ITEM_DO_NOT_USE_ANTIFAIRY = $16

ITEM_UPSTAB = $17
ITEM_DOWNSTAB = $18
ITEM_BAGU = $19
ITEM_MIRROR = $1a
ITEM_WATER = $1b

ITEM_SHIELD_SPELL = $1c
ITEM_JUMP_SPELL = $1d
ITEM_LIFE_SPELL = $1e
ITEM_FAIRY_SPELL = $1f
ITEM_FIRE_SPELL = $20
ITEM_REFLECT_SPELL = $21
ITEM_SPELL_SPELL = $22
ITEM_THUNDER_SPELL = $23
ITEM_DASH_SPELL = $24

;; NEW VARIABLE ALLOCATIONS
; Stack RAM
StackRAMBase = $0120
SET_RES_BASE StackRAMBase
RESV temp_room_code ; , $0120
RESV temp_room_flag ; , $0121
RESV DontKillEnemyFlag ; , $0122
RESV Global5050JarDrop ; = $123 ; $0123
RESV PreventDoubleLag
RESV PreviousFrameMetasprite
RESV StatDisplayState
RESV InternalState
RESV GameComplete ; set when you beat the game to stop the timers
RESV ElevatorYStart
RESV DripperRedCounter
RESV PpuCtrlForIrq
RESV ScrollPosForIrq

.assert RES_BASE + RES_OFFSET < $01ad

;; Stat tracking variables in SRAM

; This value is the second header byte for file 1 thats unused.
; In a perfect world we would stat track multiple times and copy
; the data over with the save files like vanilla does, but for now
; just hardcode the stats only to one file.
StatTrackingSaveFileClear = $7401

.export NmiBankShadow8, NmiBankShadowA
; Reserve these in SRAM to prevent issues when clearing ram
NmiBankShadow8 = $6380
NmiBankShadowA = $6381
SoftDisableNmi = $6382

StatTrackingBase = $6383
SET_RES_BASE StatTrackingBase

;; The follow stats are NOT checkpointed because they persist between reloads

; All Timestamp types listed below for reference
TS_COUNT .set 0
.macro TimestampId name
.ident(.string(name)) = TS_COUNT
TS_COUNT .set TS_COUNT + 1
.endmacro

TimestampId TsGlove
TimestampId TsRaft
TimestampId TsBoots
TimestampId TsHammer
TimestampId TsJumpSpell
TimestampId TsFairySpell
TimestampId TsReflectSpell
TimestampId TsThunderSpell
TimestampId TsPalace1
TimestampId TsPalace2
TimestampId TsPalace3
TimestampId TsPalace4
TimestampId TsPalace5
TimestampId TsPalace6
TimestampId TsPalaceGP
TimestampId TsTowns

; All of the values prefixed with Stat are persisted across resets and Continue without saves

; Overall gameplay timer ticks every NMI and never stops
; Used to create timestamps for when items are collected
RESV StatTimer, 3
; Ticks up whenever a death occurs
RESV StatDeaths, 1
; Ticks up whenever Up + A shortcut happens. Not in use, game naturally tracks this
;RESV StatResets, 1
; Ticks up for every stab you make based on type of stab 
RESV StatHiStabCount, 2
RESV StatLoStabCount, 2
RESV StatUpStabCount, 2
RESV StatDownStabCount, 2
; When we tick up the main timer, if the flag for "in town" or "in palace" is set
; we tick those respective timers as well.
StatTimeAtLocation = StatTimeInEncounters
RESV StatTimeInEncounters, 3
RESV StatTimeInPalace1, 3
RESV StatTimeInPalace2, 3
RESV StatTimeInPalace3, 3
RESV StatTimeInPalace4, 3
RESV StatTimeInPalace5, 3
RESV StatTimeInPalace6, 3
RESV StatTimeInPalaceGP, 3

; All of these timestamps are checkpointed such that if you game over,
; we restore from the most recent version of the checkpoint. So for any stats
; types where you can LOSE this on death thats not already checkpointed in the
; save game, we need to put them hwere
CHECKPOINT_START = RES_OFFSET
RESV TimestampCount, 1
RESV TimestampTypeList, TS_COUNT
RESV TimestampList, TS_COUNT * 3
CHECKPOINT_LEN = RES_OFFSET - CHECKPOINT_START

; Duplicate the timestamp memory here that we'll copy after saving
RESV Checkpoint, CHECKPOINT_LEN
STAT_TRACKING_SIZE = RES_OFFSET

; Reserve 256 bytes to store a sideview buffer.
; The sideview loading code is patched to quickly bank switch to the extended banks for palace data,
; or load regular sideviews from the usual location
RESV SideViewBuffer, $0100

.assert RES_OFFSET + StatTrackingBase < $6957
