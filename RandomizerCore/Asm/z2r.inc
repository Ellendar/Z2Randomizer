.ifndef _Z2R_INC
_Z2R_INC = 1

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

;;; Include RAM values before any constants
.include "ram.inc"

R0 = $00
R1 = $01
R2 = $02
R3 = $03
R4 = $04
R5 = $05
R6 = $06
R7 = $07

SND_REGISTER          = $4000
SND_SQUARE1_REG       = $4000
SND_SQUARE2_REG       = $4004
SND_TRIANGLE_REG      = $4008
SND_NOISE_REG         = $400c

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

NewSideviewInit = $8cec
ClearNametables = $d266
JumpEngine = $d385
EnemyFacingDirection = $DC91
LoadPlayerShieldCollisionBox = $e9d8
ObjectCollisionCheck = $e9f9
MoveEnemyHorizontally = $deb8

; These are new variables that use free memory
Z2Noise_SfxLenCounter = $07f5
Z2Squ2_SfxLenCounter = $07f7
Z2Squ1_SfxLenCounter = $07f9
Z2NoiseSoundBuffer = $07fd
Z2Square2SoundBuffer = $07fe
Z2Square1SoundBuffer = $07ff

Z2NoiseSoundQueue = $ed
Z2Square2SoundQueue = $ee
Z2Square1SoundQueue = $ef

NoiseSoundQueue = $f0
Square2SoundQueue = $f1
Square1SoundQueue = $f2

ProjectileYPosition = $30
ProjectileXVelocity = $77
ProjectileType      = $87
EnemyType           = $a1
Enemy_Flag = ProjectileType
LinkXPosition       = $4d
EnemyXPositionLo    = $4e
EnemyXPositionHi    = $3c
Enemy_HP            = $c2
RNG                 = $051b
ProjectileEnemyData = $07c0
EnemyYVelocity = $057e

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
RESV IrqSetupAlready
RESV PendingAudioCall ; 1=audio running on main thread, 0=NMI decremented it (needs re-run)
; Expanded the block break list to hold up to 10 bricks at a time instead of just 5
BREAKABLE_BLOCK_COUNT = 10
RESV BlockBreakYCoord, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0429 - Y Low byte for collision table
RESV BlockBreakColumnOffset, BREAKABLE_BLOCK_COUNT ; Vanilla addr $042E - Offset in the collision table
RESV BlockBreakAddrLo, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0433 - Collision table lo addr
RESV BlockBreakAddrHi, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0433 - Collision table hi addr


.ifdef ENABLE_Z2_MARIO

; New RAM values that aren't from vanilla SMB1 code
RESV mapMarioIndex
RESV mapMarioTimer
RESV mapMarioPrevDirection

;;;
; MARIO related values that are not directly mapped to vanilla z2 object values

RESV JustThrownHammer
; RESV ObjectOffset
ObjectOffset = $10
RESV FBall_OffscreenBits
RESV AltEntranceControl
RESV PlayerEntranceCtrl
RESV JoypadOverride
RESV Vine_Height
RESV Player_State
RESV DisableCollisionDet
RESV GameEngineSubroutine
RESV AreaType
; RESV A_B_Buttons
A_B_Buttons = $741
RESV PreviousA_B_Buttons
; RESV Left_Right_Buttons
Left_Right_Buttons = $742
; RESV Up_Down_Buttons
Up_Down_Buttons = $743
RESV PlayerSize
RESV DeathMusicLoaded
RESV EventMusicQueue
RESV EventMusicBuffer ; TODO use the vanilla music queues
RESV WarpZoneControl
RESV Player_CollisionBits ; Split Z2 and SMB1 collision bits
RESV HalfwayPage
;RESV SwimmingFlag
SwimmingFlag = $13
RESV CurrentOAMOffset
; RESV ObjectMetasprite
RESV PlayerAnimCtrl
RESV PlayerChangeSizeFlag
RESV VerticalForceDown
RESV VerticalForce
RESV JumpOrigin_Y_Position
RESV JumpOrigin_Y_HighPos
RESV DiffToHaltJump
RESV PlayerAnimTimerSet
RESV JumpspringAnimCtrl
RESV Player_XSpeedAbsolute
; RESV Whirlpool_Flag
RESV RunningSpeed
RESV MaximumLeftSpeed
RESV MaximumRightSpeed
RESV FrictionAdderLow
RESV FrictionAdderHigh
RESV Player_Pos_ForScroll
RESV Player_X_Scroll
RESV Player_X_MoveForce
RESV PlayerStandingMetatile ; added to keep this for door checks and other metatiles

; Non-timer timer.
RESV FireballThrowingTimer

RESV TimerControl
RESV IntervalTimerControl

Timers = PlayerAnimTimer
RESV PlayerAnimTimer
RESV ClimbSideTimer
RESV JumpSwimTimer
RESV RunningTimer
RESV SideCollisionTimer
RESV BlockBounceTimer
RESV ChangeAreaTimer
FRAME_TIMER_COUNT = ChangeAreaTimer - Timers
RESV StarInvincibleTimer
ALL_TIMER_COUNT = StarInvincibleTimer - Timers

RESV SpriteShuffleOffset
;RESV CurrentCHRBank
RESV PlayerChrBank
RESV ReloadCHRBank
RESV ObjectVerticalFlip
RESV Local_eb
; Used by hammers to track how long they've been alive
RESV PlayerProj_Cnt, 2

; Values use for SMB1 sfx audio engine

RESV Sfx_SecondaryCounter
RESV PauseSoundBuffer
RESV PauseModeFlag
RESV PauseSoundQueue
RESV Square1SoundBuffer
RESV Square2SoundBuffer
RESV NoiseSoundBuffer
RESV Squ1_SfxLenCounter
RESV Squ2_SfxLenCounter
RESV Noise_SfxLenCounter

; Shadow Mario Boss RAM
CUSTOM_BOSS_START = RES_OFFSET
RESV boss_animation
RESV boss_invincibility_timer
RESV boss_screech
RESV boss_state
RESV boss_state_timer
RESV boss_walking_anim
RESV boss_bridge
RESV boss_shootanim
RESV boss_substate
RESV boss_counter
RESV boss_freeze
RESV boss_ChrBank
BOSS_RAM_SIZE = RES_OFFSET - CUSTOM_BOSS_START - 1
;RESV BowserHitPoints

; MARIO SPECIFIC RAM LOCATIONS
; These are values that are hardcoded to match where Zelda 2 places them
Player_X_Speed = $70
SprObject_X_Speed = Player_X_Speed
Player_X_Position = $4D
SprObject_X_Position = Player_X_Position
Player_PageLoc = $3B
SprObject_PageLoc = Player_PageLoc
Player_Y_Position = $29
SprObject_Y_Position = Player_Y_Position
Player_Y_HighPos = $19
SprObject_Y_HighPos = Player_Y_HighPos

;Player_CollisionBits = $A7
ScrollAmount = $14
ObjectMetasprite = $80

ProjectileOffset = 13
FireballOffset = 13
FireballMetasprite = ObjectMetasprite + FireballOffset
Fireball_X_Position = SprObject_X_Position + FireballOffset
Fireball_Y_Position = SprObject_Y_Position + FireballOffset
Fireball_Y_HighPos = SprObject_Y_HighPos + FireballOffset
Fireball_PageLoc = SprObject_PageLoc + FireballOffset
Fireball_X_Speed = SprObject_X_Speed + FireballOffset
Fireball_Y_Speed = SprObject_Y_Speed + FireballOffset
RESV FireballBouncingFlag, 2
RESV Fireball_State, 2

EnemyOffset = 1   ; Enemy slot 0 = offset 1 from player
Enemy_X_Position  = SprObject_X_Position + EnemyOffset
Enemy_Y_Position  = SprObject_Y_Position + EnemyOffset
Enemy_X_Speed     = SprObject_X_Speed + EnemyOffset
Enemy_PageLoc     = SprObject_PageLoc + EnemyOffset
Enemy_Y_HighPos   = SprObject_Y_HighPos + EnemyOffset
Enemy_MovingDir   = Player_MovingDir + EnemyOffset
Enemy_State       = $A8


; For reasons I cannot explain, player_x_moveforce is not the same as sprobject_x_moveforce
SprObject_X_MoveForce = $03d6 ; Player_X_MoveForce
Player_Y_MoveForce = $03e6
SprObject_Y_MoveForce = Player_Y_MoveForce
Enemy_X_MoveForce = SprObject_X_MoveForce + EnemyOffset
Enemy_Y_MoveForce = SprObject_Y_MoveForce + EnemyOffset

Player_Y_Speed = $057D
SprObject_Y_Speed = Player_Y_Speed
Enemy_Y_Speed     = SprObject_Y_Speed + EnemyOffset

FrameCounter = $12
; CrouchingFlag = $17
RESV CrouchingFlag
Player_MovingDir = $5F
Fireball_MovingDir = $6D
PlayerFacingDir = $9F
SavedJoypadBits = $F7
ScreenLeft_PageLoc = $72a
ScreenRight_PageLoc = $72b
ScreenEdge_PageLoc = ScreenLeft_PageLoc
ScreenLeft_X_Pos = $72c
ScreenRight_X_Pos = $72d
ScreenEdge_X_Pos = ScreenLeft_X_Pos

InjuryTimer = $050c
NumberofLives = $0700
; Player_SprAttrib = $0752
; SprObject_SprAttrib = Player_SprAttrib
ScrollLock = $0728

MoveObjectHorizontally = $D1CE
CheckForSolidMTiles = $850C
CheckSideviewTransition = $E14A

VRAM_Buffer1_Offset = $302
VRAM_Buffer1 = $303

Sprite_Y_Position = $200
Sprite_Tilenumber = $201
Sprite_Attributes = $202
Sprite_X_Position = $203
Sprite_Data = Sprite_Y_Position

;; Constants

CHR_BIGMARIO = $6a
CHR_MARIOACTION = $6b
CHR_SMALLMARIO = $6c
CHR_SMALLFIRE = $6d

A_Button              = %10000000
B_Button              = %01000000
Select_Button         = %00100000
Start_Button          = %00010000
Up_Dir                = %00001000
Down_Dir              = %00000100
Left_Dir              = %00000010
Right_Dir             = %00000001

;sound effects constants
Sfx_SmallJump         = %10000000
Sfx_Flagpole          = %01000000
Sfx_Fireball          = %00100000
Sfx_PipeDown_Injury   = %00010000
Sfx_EnemySmack        = %00001000
Sfx_EnemyStomp        = %00000100
Sfx_Bump              = %00000010
Sfx_BigJump           = %00000001

Sfx_BowserFall        = %10000000
Sfx_ExtraLife         = %01000000
Sfx_PowerUpGrab       = %00100000
Sfx_TimerTick         = %00010000
Sfx_Blast             = %00001000
Sfx_GrowVine          = %00000100
Sfx_GrowPowerUp       = %00000010
Sfx_CoinGrab          = %00000001

;music constants
Silence               = %10000000

StarPowerMusic        = %01000000
PipeIntroMusic        = %00100000
CloudMusic            = %00010000
CastleMusic           = %00001000
UndergroundMusic      = %00000100
WaterMusic            = %00000010
GroundMusic           = %00000001

TimeRunningOutMusic   = %01000000
EndOfLevelMusic       = %00100000
AltGameOverMusic      = %00010000
EndOfCastleMusic      = %00001000
VictoryMusic          = %00000100
GameOverMusic         = %00000010
DeathMusic            = %00000001

.endif ; ENABLE_Z2_MARIO

.assert RES_BASE + RES_OFFSET < $01bd


RelocatedRAMBase = $041A
SET_RES_BASE RelocatedRAMBase
RESV BlockBreakState, BREAKABLE_BLOCK_COUNT ; Vanilla addr $041a - bits 0-6 = timer; bit 7 set = step block. $00 means no brick breaking
RESV BlockBreakMapPage, BREAKABLE_BLOCK_COUNT ; Vanilla addr $041F - Nametable select for the PPUADDR
RESV BlockBreakXCoord, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0424 - X Low Byte for collision table
.assert RES_BASE + RES_OFFSET < $043E

;; Stat tracking variables in SRAM

; This value is the second header byte for file 1 thats unused.
; In a perfect world we would stat track multiple times and copy
; the data over with the save files like vanilla does, but for now
; just hardcode the stats only to one file.
StatTrackingSaveFileClear = $7401

.export NmiBankShadow8, NmiBankShadowA
; Reserve these in SRAM to prevent issues when clearing ram
NmiBankShadow8 = $6340
NmiBankShadowA = $6341
SoftDisableNmi = $6342

; Flag to store if the dialog has ended
DialogCompleteTmp = $0523

COLLISION_TILES = $6350

;StatTrackingBase = COLLISION_TILES + $d0 * 4
; Move stat tracking to after the save ram since Z2FT (custom music) uses this block
StatTrackingBase = $773C
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

; NOTE: We can overlap this with the pause menu drawing code.
; Reserve 256 bytes to store a sideview buffer.
; This stores the command data for drawing a sideview, so its only needed temporarily
; when loading a side view, which is why its safe to alias this with the pause menu drawing.
SideViewBuffer = $7800

; Duplicate the timestamp memory here that we'll copy after saving
RESV Checkpoint, CHECKPOINT_LEN
STAT_TRACKING_SIZE = RES_OFFSET

ClearSRAMStart = RES_OFFSET

.assert RES_OFFSET + StatTrackingBase < $7800

.ifdef ENABLE_Z2_MARIO
SET_RES_BASE $6FB0
; There isn't a general purpose "attribute" mix so we can make our own
RESV SprObject_SprAttrib, 16
Player_SprAttrib = SprObject_SprAttrib
Fireball_SprAttrib = SprObject_SprAttrib + FireballOffset
;Fireball_FacingDir = PlayerFacingDir + FireballOffset

RESV Player_YMoveForceFractional, 16

; Enemy-specific arrays (6 slots each: boss=0, enemies 1-5)
; RESV Enemy_ID, 6
; RESV Enemy_State, 6
; RESV Enemy_Flag, 6
; RESV Enemy_BoundBoxCtrl, 6
; RESV Enemy_CollisionBits, 6
; RESV Enemy_SprDataOffset, 6
; RESV Enemy_Rel_XPos, 6
; RESV Enemy_Rel_YPos, 6
; RESV bbSpeed, 6              ; Bullet bill speed per slot
; Enemy_SprAttrib = SprObject_SprAttrib + EnemyOffset

ClearSRAMEnd = RES_OFFSET


;;; Update a handful of refs to point to the given address.
;;; Usage:
;;;   UPDATE_BYTE byte @ refs...
;;; Where |refs| is a space-separated list of addresses, and
;;; |byte| is a single literal to insert into each ref.
.define UPDATE_BYTE {target @ ref refs .eol} \
.pushseg .eol \
.if     $00000 <= ref - $10 .and ref - $10 < $04000 .eol \
    .segment "PRG0" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $04000 <= ref - $10 .and ref - $10 < $08000 .eol \
    .segment "PRG1" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $08000 <= ref - $10 .and ref - $10 < $0c000 .eol \
    .segment "PRG2" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $0c000 <= ref - $10 .and ref - $10 < $10000 .eol \
    .segment "PRG3" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $10000 <= ref - $10 .and ref - $10 < $14000 .eol \
    .segment "PRG4" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $14000 <= ref - $10 .and ref - $10 < $18000 .eol \
    .segment "PRG5" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $18000 <= ref - $10 .and ref - $10 < $1c000 .eol \
    .segment "PRG6" .eol \
    .org ((ref - $10) .mod $4000) + $8000 .eol \
.elseif $1c000 <= ref - $10 .and ref - $10 < $20000 .eol \
    .segment "PRG7" .eol \
    .org ((ref - $10) .mod $4000) + $c000 .eol \
.else .eol \
    .error "bad ref" .eol \
.endif .eol \
  .byte target .eol \
.popseg .eol \
UPDATE_BYTE target @ refs
.define UPDATE_BYTE {target @ .eol}

.endif ; ENABLE_Z2_MARIO


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




.endif
