.ifndef _Z2R_RAMMAP_INC
_Z2R_RAMMAP_INC = 1


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

; Define low zero page addresses even if they're not well-defined
; so they will have debug info precedence over other constants
zp_00 = $00
zp_01 = $01
zp_02 = $02
zp_03 = $03
zp_04 = $04
zp_05 = $05
zp_06 = $06
zp_07 = $07
zp_08 = $08
zp_09 = $09
zp_0a = $0a
zp_0b = $0b
zp_0c = $0c
zp_0d = $0d
zp_0e = $0e
zp_0f = $0f
zp_10 = $10
zp_11 = $11
FrameCounter = $12
FairyState = $13                         ; 0 = Not in Fairy state
zp_14 = $14
zp_15 = $15
zp_16 = $16
LinkStanding = $17                       ; 0 = Link in ducked shield position, 1 = Link standing
DarkLinkStanding = $18                   ; 0 = Dark Link ducking, 1 = Dark Link standing
LinkYPositionHi = $19                    ; [SS] 0 = Invisible, 1 = Normal, 2-FF = Fall in hole   - [FILE] Cursor position
Enemy0YPositionHi = $1a                  ; [SS] 0 = Enemy0 off the top of the screen, 1 = Normal - [FILE] Save file presence bits
Enemy1YPositionHi = $1b
Enemy2YPositionHi = $1c
Enemy3YPositionHi = $1d
Enemy4YPositionHi = $1e                  ; [SS] 0 = Enemy4 off the top of the screen, 1 = Normal - [FILE] File selection cursor
Enemy5YPositionHi = $1f
Enemy0Condition = $20
Enemy1Condition = $21
Enemy2Condition = $22
Enemy3Condition = $23
Enemy4Condition = $24
Enemy5Condition = $25
OverworldStepCounter = $26
zp_27 = $27
zp_28 = $28
LinkYPos = $29
EnemyYPositionLo = $2a
Enemy0YPositionLo = $2a
OverworldEnemyYPosition = $2a            ; [OW] base of overworld demon Y world position array, indexed by slot X
Enemy1YPositionLo = $2b
Enemy2YPositionLo = $2c
Enemy3YPositionLo = $2d
Enemy4YPositionLo = $2e
Enemy5YPositionLo = $2f
ProjectileYPosition = $30
Projectile0YPosition = $30
Projectile1YPosition = $31
Projectile2YPosition = $32
Projectile3YPosition = $33
Projectile4YPosition = $34
Projectile5YPosition = $35
LinkProjectile0YPosition = $36
LinkProjectile1YPosition = $37
zp_38 = $38
LinkProjectileYPositionRelated = $39
zp_3a = $3a
LinkXPositionHi = $3b
EnemyXPositionHi  = $3c
Enemy0XPositionHi = $3c
Enemy1XPositionHi = $3d
Enemy2XPositionHi = $3e
Enemy3XPositionHi = $3f
Enemy4XPositionHi = $40
Enemy5XPositionHi = $41
Projectile0XPositionHi = $42
Projectile1XPositionHi = $43
Projectile2XPositionHi = $44
Projectile3XPositionHi = $45
Projectile4XPositionHi = $46
Projectile5XPositionHi = $47
LinkProjectile0XPositionHi = $48
LinkProjectile1XPositionHi = $49
zp_4a = $4a
LinkProjectileXPositionHiRelated = $4b
zp_4c = $4c
LinkXPosition   = $4d
LinkXPositionLo = $4d
EnemyXPositionLo  = $4e
Enemy0XPositionLo = $4e
OverworldEnemyXPosition = $4e            ; [OW] base of overworld demon X world position array, indexed by slot X
Enemy1XPositionLo = $4f
Enemy2XPositionLo = $50
Enemy3XPositionLo = $51
Enemy4XPositionLo = $52
Enemy5XPositionLo = $53
Projectile0XPositionLo = $54
Projectile1XPositionLo = $55
Projectile2XPositionLo = $56
Projectile3XPositionLo = $57
Projectile4XPositionLo = $58
Projectile5XPositionLo = $59
LinkProjectile0XPositionLo = $5a
LinkProjectile1XPositionLo = $5b
zp_5c = $5c
LinkProjectileXPositionLoRelated = $5d
zp_5e = $5e
LinkFacingDirection = $5f
Enemy0XFacing = $60
Enemy1XFacing = $61
Enemy2XFacing = $62
Enemy3XFacing = $63
Enemy4XFacing = $64
Enemy5XFacing = $65
Projectile0XFacing = $66
Projectile1XFacing = $67
Projectile2XFacing = $68
Projectile3XFacing = $69
Projectile4XFacing = $6a
Projectile5XFacing = $6b
zp_6c = $6c
zp_6d = $6d
zp_6e = $6e
zp_6f = $6f
LinkXVelocity = $70
Enemy0XVelocity = $71
Enemy1XVelocity = $72
OverworldYPosition = $73                 ; [SS] Enemy2XVelocity - [OW] OverworldYPosition
OverworldXPosition = $74                 ; [SS] Enemy3XVelocity - [OW] OverworldXPosition
OverworldLinkYPosition = $75             ; [SS] Enemy4XVelocity - [OW] OverworldLinkYPosition
OverworldLinkXPosition = $76             ; [SS] Enemy5XVelocity - [OW] OverworldLinkXPosition
Projectile0XVelocity = $77
Projectile1XVelocity = $78
Projectile2XVelocity = $79
Projectile3XVelocity = $7a
Projectile4XVelocity = $7b
Projectile5XVelocity = $7c
LinkProjectile0XVelocity = $7d
LinkProjectile1XVelocity = $7e
zp_7f = $7f
OverworldScrollY = $7f                   ; [OW] camera Y scroll offset; OAM_Y = world_Y - OverworldScrollY
LinkAnimationFrame = $80
OverworldCurrentSlot = $80              ; [OW] current overworld demon slot index during per-frame processing (0-7)
Enemy0AnimationFrame = $81
OverworldEnemy0Type = $82                ; [SS] Enemy1AnimationFrame - [OW] OverworldEnemy0Type, 1 = Weak, 2 = Strong, 3 = Fairy
OverworldEnemy1Type = $83                ; [SS] Enemy2AnimationFrame - [OW] OverworldEnemy1Type
OverworldEnemy2Type = $84                ; [SS] Enemy3AnimationFrame - [OW] OverworldEnemy2Type
OverworldEnemy3Type = $85                ; [SS] Enemy4AnimationFrame - [OW] OverworldEnemy3Type
OverworldEnemy4Type = $86                ; [SS] Enemy5AnimationFrame - [OW] OverworldEnemy4Type
ProjectileType = $87
Enemy_Flag = ProjectileType
Projectile0Type = $87                    ; [SS] Projectile0Type - [OW] OverworldEnemy5Type
Projectile1Type = $88                    ; [SS] Projectile1Type - [OW] OverworldEnemy6Type
Projectile2Type = $89                    ; [SS] Projectile2Type - [OW] OverworldEnemy7Type
Projectile3Type = $8a
Projectile4Type = $8b
Projectile5Type = $8c
LinkProjectile0State = $8d
LinkProjectile1State = $8e
zp_8f = $8f
SpriteShuffleOffsetLink = $90
SpriteShuffleOffsetEnemy0 = $91
SpriteShuffleOffsetEnemy1 = $92
SpriteShuffleOffsetEnemy2 = $93
SpriteShuffleOffsetEnemy3 = $94
SpriteShuffleOffsetEnemy4 = $95
SpriteShuffleOffsetEnemy5 = $96
SpriteShuffleOffsetProjectile0 = $97
SpriteShuffleOffsetProjectile1 = $98
SpriteShuffleOffsetProjectile2 = $99
SpriteShuffleOffsetProjectile3 = $9a
SpriteShuffleOffsetProjectile4 = $9b
SpriteShuffleOffsetProjectile5 = $9c
SpriteShuffleOffsetLinkProjectile0 = $9d
SpriteShuffleOffsetLinkProjectile1 = $9e
DpadHorizontalState = $9f                ; 1 = Right, 2 = Left
zp_a0 = $a0
EnemyType  = $a1
Enemy0Type = $a1                         ; Enemy (Elevator) slot
Enemy1Type = $a2                         ; Enemy (Locked door) slot
Enemy2Type = $a3
Enemy3Type = $a4
Enemy4Type = $a5
Enemy5Type = $a6                         ; Enemy (Boss) slot
LinkCollisionBits = $a7                  ; 0000ABLR (above, below, left, right)
Enemy0State = $a8
Enemy1State = $a9
Enemy2State = $aa
Enemy3State = $ab
Enemy4State = $ac
Enemy5State = $ad
zp_ae = $ae
Enemy0State2 = $af
Enemy1State2 = $b0
Enemy2State2 = $b1
Enemy3State2 = $b2
Enemy4State2 = $b3
Enemy5State2 = $b4
SideviewState = $b5                      ; 0 = Link enters room, 1 = Normal, 2 = Link dies in lava, 3 = Link slides left after killing Dark Link
Enemy0Status = $b6                       ; 0 = No enemy, 1 = Alive, 2 = Exploding, 3 = Floating XP, 10 = Link takes drop exit
Enemy1Status = $b7
Enemy2Status = $b8
Enemy3Status = $b9
Enemy4Status = $ba
Enemy5Status = $bb
Enemy0Persistence = $bc                  ; Related to remembering that an enemy is defeated
Enemy1Persistence = $bd
Enemy2Persistence = $be
Enemy3Persistence = $bf
Enemy4Persistence = $c0
Enemy5Persistence = $c1
EnemyHP = $c2
Enemy0HP = $c2
Enemy1HP = $c3
Enemy2HP = $c4
Enemy3HP = $c5
Enemy4HP = $c6
Enemy5HP = $c7
SideviewExitScreen = $c8                 ; 3 = Exit screen
zp_c9 = $c9
zp_ca = $ca
zp_cb = $cb
LinkXScreenPosition = $cc
zp_cd = $cd
zp_ce = $ce
LinkProjectileXScreenPosition = $cf
JumpSpellActive = $d0                    ; 1 = Jump active
SideviewWidth = $d1                      ; With of current sidescroll screen (1-3 pages)
zp_d2 = $d2
zp_d3 = $d3
zp_d4 = $d4
zp_d5 = $d5
zp_d6 = $d6
zp_d7 = $d7
zp_d8 = $d8
zp_d9 = $d9
zp_da = $da
zp_db = $db
zp_dc = $dc
zp_dd = $dd
zp_de = $de
zp_df = $df                              ; Enemy type temporary storage

; $e0-$ef are music and sound variables
Z2NoiseSoundQueue = $ed
Z2Square2SoundQueue = $ee
Z2Square1SoundQueue = $ef

NoiseSoundQueue = $f0
Square2SoundQueue = $f1
Square1SoundQueue = $f2

; Potentially free zero page memory bytes at $f1-$f4 and since they are used by FDS BIOS but not the NES
Controller1ButtonsPressed = $f5
Controller2ButtonsPressed = $f6
Controller1ButtonsHeld = $f7
Controller2ButtonsHeld = $f8
; Potentially free zero page memory bytes at $f9-$fc
ScrollPosShadow = $fd
PpuCtrlShadow = $ff


Sprite_Y_Position = $200
Sprite_Tilenumber = $201
Sprite_Attributes = $202
Sprite_X_Position = $203
Sprite_Data = Sprite_Y_Position


BUFFER_OFF = $0301
PPUADDR_HI = $0302
PPUADDR_LO = $0303
BUFFER_LEN = $0304
BUFFER_DAT = $0305

LinkXSubpixel = $03d6
Enemy0XSubpixel = $03d7
Enemy1XSubpixel = $03d8
Enemy2XSubpixel = $03d9
Enemy3XSubpixel = $03da
Enemy4XSubpixel = $03db
Enemy5XSubpixel = $03dc
LinkYVelocityLo = $03e6

; This is where the buffers for breaking blocks are stored in vanilla. We increase the number of blocks that can be broken
; at the same time by expanding the number of blocks here, and then moving the extra fields to a different location in
; stack ram
; Expanded the block break list to hold up to 10 bricks at a time instead of just 5
BREAKABLE_BLOCK_COUNT = 10
RelocatedRAMBase = $041A
SET_RES_BASE RelocatedRAMBase
RESV BlockBreakState, BREAKABLE_BLOCK_COUNT ; Vanilla addr $041a - bits 0-6 = timer; bit 7 set = step block. $00 means no brick breaking
RESV BlockBreakMapPage, BREAKABLE_BLOCK_COUNT ; Vanilla addr $041F - Nametable select for the PPUADDR
RESV BlockBreakXCoord, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0424 - X Low Byte for collision table
.assert RES_BASE + RES_OFFSET < $043E

LinkSwordAttackFrame = $0400
Enemy0HitState = $040e
LinkInAir = $0479                        ; 0 = On ground, 1 = Falling from walking off platform, 2 = Jumping
LandingAnimationTimer = $0497
HitboxXCoord = $047e
HitboxYCoord = $0480

LinkIFrameTimer = $500
timer_501 = $0501                        ; $501 through $50d are timers that are decreased each frame until 0
LinkEntrySlideTime = $0503
SwordAttackTimer = $050a
InjuryTimer = $050c
OverworldDemonTimer = $050e              ; [OW] overworld demon active flag/timer, indexed by slot X; non-zero = active
EncounterSpawnTimer = $0516
RNG = $051b
MapNumber = $0561
OverworldFacingDirection = $0562         ; [OW] Link's facing direction: $01=right, $02=left, $04=down, $08=up
OverworldFacingTerrain = $0563           ; [OW] terrain type Link is currently facing
TownNumber = $056b
PalaceNumber = $056c
PalaceRegionIndex = $056c                ; Palace index in continent (0-2)
OverworldDemonYVelocity = $056d          ; [OW] overworld demon Y velocity, indexed by slot X (signed: $01=down, $FF=up)
OverworldDemonXVelocity = $0575          ; [OW] overworld demon X velocity, indexed by slot X (signed: $01=right, $FF=left)
LinkYVelocityHi = $057d
EnemyYVelocity = $057e
Enemy0YVelocity = $057e
LinkProjectile0Timer = $05ca
LinkProjectile1Timer = $05cb             ; Always 0 since Fire projectiles don't expire
SavedDoorExit1Page = $05d1               ; accessed as $5cc,X with X = 5
SavedDoorExit1X = $05d8                  ; accessed as $5d3,X with X = 5
SavedDoorExit2Page = $05d2               ; used for New Kasuto fireplace
SavedDoorExit2X = $05d9
SmallDropCount = $05df
LargeDropCount = $05e0

Lives = $0700
NumberofLives = $0700
EntryFacing = $0701                      ; Sideview entry facing direction. 0=Facing right (enter from the left), 1=Facing left (enter from the right)
EnterFromElevatorAbove = $0704           ; >0 Link enters the room from the top
EnterCentered = $0705                    ; >0 Link enters centered; like at North Palace, taking an elevator exit or entering a random encounter
RegionNumber = $0706                     ; 0=West, 1=Death Mountain, 2=East, 3=Maze Island
GoingOutsideFlag = $0709                 ; non-zero routes game mode 0 (load area data) out to the overworld via go_outside
PreviousRegion = $070a                   ; RegionNumber Link came from
WorldNumber = $0707                      ; 0=Caves & enemy encounters, 1=West towns, 2=East towns, 3=Palace 1/2/5, 4=Palace 3/4/6, 5=Great Palace
ScrollDirection = $071f                  ; Current scroll direction. 1=Scrolling right, 2=Scrolling left
ColumnScrollDirection = $0720            ; Scroll direction that is updated only when scrolling crosses a column boundary
ColumnScrollX = $0721                    ; Scroll position within the current tile column ($00-$1f)
DialogBoxDrawing = $0726                 ; >0 when drawing dialog and during black screen transitions
ScrollFrozen = $0728                     ; 1=Screen scrolling frozen (for boss fights)
ScrollLock = $0728
ScreenLeft_PageLoc = $72a
ScrollPage = $072a                       ; Which page the left-most pixel of the screen is on (0-3)
ScreenRight_PageLoc = $72b
ScreenEdge_PageLoc = ScreenLeft_PageLoc
ScreenLeft_X_Pos = $72c
ScreenRight_X_Pos = $72d
ScreenEdge_X_Pos = ScreenLeft_X_Pos
ScrollX = $072c
SideviewCommandY = $0730
SideviewCommandOp = $0731
ScrollLeftPage = $0732
ScrollRightPage = $0733
ScrollLeftColumn = $0734                 ; Which left background column to update during scrolling
ScrollRightColumn = $0735                ; Which right background column to update during scrolling
GameMode = $0736
GameModeCurrent = $0737                  ; GameMode is compared to this and only triggered if it has changed

Controller1ABPressed = $0741             ; Controller1ButtonsPressed & 0b11000000
Controller1LeftRightHeld = $0742         ; Controller1ButtonsHeld & 0b00000011
Controller1UpDownHeld = $0743            ; Controller1ButtonsHeld & 0b00001100
Controller1ButtonsHeldCopy = $0744
LocationNumber = $0748                   ; Location index sideview exits to or $3e for random encounters
MagicSelectorPosition = $0749
LinkFlashingTimer = $074b                ; Link changes palette and this value is counted down until it's 0. 0b10000000 bit determines if the background changes color.
CurrentDialogType = $074c                ; 0=None, 1 = Level Up, 2 = Talking
ExpToAddHi = $0755
ExpToAddLo = $0756
DoorDepth = $075b                        ; Used as an index for saving/loading your x position when you enter/leave (nested) doors
EntryPage = $075c                        ; Page to enter a sideview on (0-3) or 4 for center
EnterAreaFlag = $075f                    ; Set to 1 when entering a screen centered. Other values are sound and music related.
CurrentPrgBank = $0769
GameEvent = $076c                        ; 0=Restart with full lives, 1=Main game loop, 2=Link dying, 3=Wake up Zelda, 4=Roll credits, 06=Show lives left then restart the scene
GameEventCurrent = $076d                 ; GameEvent is compared to this and only triggered if it has changed
CurrentCHRBank = $076e
FireSpellActive = $076f                  ; 0x10 = Fire spell active
ExpForNextLevelHi = $0770
ExpForNextLevelLo = $0771
CurrentGameSlot = $0772
CurrentMagic = $0773
CurrentLife = $0774
CurrentExpHi = $0775
CurrentExpLo = $0776
AttackLevel = $0777
MagicLevel = $0778
LifeLevel = $0779
HaveShield = $077b
HaveJump = $077c
HaveLife = $077d
HaveFairy = $077e
HaveFire = $077f
HaveReflect = $0780
HaveSpell = $0781
HaveThunder = $0782
MagicContainers = $0783
HeartContainers = $0784
HaveCandle = $0785
HaveGlove = $0786
HaveRaft = $0787
HaveBoot = $0788
HaveFlute = $0789
HaveCross = $078a
HaveHammer = $078b
HaveMagicKey = $078c
CrystalPlaced = $078d                    ; base of crystal-placed flags, indexed by palace_code (0-5); non-zero = gem placed
Keys = $0793                             ; Current number of Keys
Crystals = $0794                         ; Number of Crystals left
HaveStabs = $0796                        ; 0x04 = Have Downstab | 0x10 = Have Upstab
HaveTrophy = $0798                       ; 0x10 = Have Trophy
HaveMirror = $0799                       ; 1 = Have Mirror
HaveBagusNoteAndMedicine = $079a         ; 0x08 = Bagu's Note | 0x40 = Medicine
HaveWater = $079b                        ; 1 = Have Water
HaveChild = $079c                        ; 0x20 = Have Child
HaveBasementAccess = $079d               ; 0x08 = Have New Kasuto Basement access
ContinuesUsed = $079f

; These are new variables that use free memory
ProjectileEnemyData = $07c0
Z2Noise_SfxLenCounter = $07f5
Z2Squ2_SfxLenCounter = $07f7
Z2Squ1_SfxLenCounter = $07f9
Z2NoiseSoundBuffer = $07fd
Z2Square2SoundBuffer = $07fe
Z2Square1SoundBuffer = $07ff

SavedDoorExit1ScrollPage = $69aa         ; accessed as $69a5,X with X = 5
SavedDoorExit1ScrollX = $69b1            ; accessed as $69ac,X with X = 5
SavedDoorExit2ScrollPage = $69ab         ; used for New Kasuto fireplace
SavedDoorExit2ScrollX = $69b2

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
RESV ProjectileProcessing
RESV PendingAudioCall ; 1=audio running on main thread, 0=NMI decremented it (needs re-run)
RESV BlockBreakYCoord, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0429 - Y Low byte for collision table
RESV BlockBreakColumnOffset, BREAKABLE_BLOCK_COUNT ; Vanilla addr $042E - Offset in the collision table
RESV BlockBreakAddrLo, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0433 - Collision table lo addr
RESV BlockBreakAddrHi, BREAKABLE_BLOCK_COUNT ; Vanilla addr $0433 - Collision table hi addr

RESV TwisterCurrentPalaceSlot ; global palace slot (RegionNumber*4 + within-region palace code) of last dungeon Link entered or warped to
RESV TwisterPendingPalaceLocation ; flute cross-continent warp: non-zero = palace LocationNumber to enter once the destination region's overworld has been reloaded

.ifdef ENABLE_Z2_MARIO

; New RAM values that aren't from vanilla SMB1 code
RESV mapMarioIndex
RESV mapMarioTimer
RESV mapMarioPrevDirection

;;;
; MARIO related values that are not directly mapped to vanilla z2 object values

ObjectOffset = $10
; RESV FBall_OffscreenBits
; RESV AltEntranceControl
; RESV PlayerEntranceCtrl
; RESV JoypadOverride
; RESV Vine_Height
RESV Player_State
; RESV DisableCollisionDet
RESV GameEngineSubroutine
; RESV AreaType
; RESV A_B_Buttons
A_B_Buttons = $741
RESV PreviousA_B_Buttons
; RESV Left_Right_Buttons
Left_Right_Buttons = $742
; RESV Up_Down_Buttons
Up_Down_Buttons = $743
RESV PlayerSize
; RESV DeathMusicLoaded
RESV EventMusicQueue
RESV EventMusicBuffer ; TODO use the vanilla music queues
; RESV WarpZoneControl
RESV Player_CollisionBits ; Split Z2 and SMB1 collision bits
; RESV HalfwayPage
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
RESV PlayerIsDownstabbing ; Simplify downstab checks by just storing if we set the hitbox

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
; RESV BlockBounceTimer
FRAME_TIMER_COUNT = SideCollisionTimer - Timers
; RESV StarInvincibleTimer
ALL_TIMER_COUNT = SideCollisionTimer - Timers ; StarInvincibleTimer - Timers

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
; RESV PauseSoundBuffer
; RESV PauseModeFlag
; RESV PauseSoundQueue
RESV Square1SoundBuffer
RESV Square2SoundBuffer
RESV NoiseSoundBuffer
RESV Squ1_SfxLenCounter
RESV Squ2_SfxLenCounter
RESV Noise_SfxLenCounter
RESV TailWag_CtlHold
RESV TailWag_FreqLo
RESV TailWag_FreqHi

RESV JustThrownHammer
RESV TailSwingTimer
; SMB3 raccoon flutter: nonzero = Y-velocity cap active
RESV TailWagCount
; Per-enemy "hit by hammer" flag. Persistent across the enemy's hit-flash so
; later per-enemy recoil setters ($9AF9 etc., which run after ProjectileProcessing
; is cleared) can still know to suppress the knockback. Cleared when the flash
; timer at $040E,x decrements to zero (see bank7_Display patch).
RESV HammerHitFlag, 6

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
RESV boss_transition_delay      ; nonzero = boss is paused mid-state-switch (frames remaining)
BOSS_RAM_SIZE = RES_OFFSET - CUSTOM_BOSS_START - 1

; After a boss dies, the game renders a shadow during the flashing animation
; and this is our custom value to say "hey its time to copy the mario sprites"
; and also doubles as the direction to cast the shadow.
RESV MarioShadowXOffset

EnemyProjectileOffset = 7
EnemyProjectileType        = ObjectMetasprite     + EnemyProjectileOffset
EnemyProjectile_X_Position = SprObject_X_Position + EnemyProjectileOffset
EnemyProjectile_Y_Position = SprObject_Y_Position + EnemyProjectileOffset
EnemyProjectile_PageLoc    = SprObject_PageLoc    + EnemyProjectileOffset
EnemyProjectile_Y_HighPos  = SprObject_Y_HighPos  + EnemyProjectileOffset
EnemyProjectile_X_Speed    = SprObject_X_Speed    + EnemyProjectileOffset
EnemyProjectile_Y_Speed    = SprObject_Y_Speed    + EnemyProjectileOffset
EnemyProjectile_SprAttrib  = SprObject_SprAttrib  + EnemyProjectileOffset
ENEMY_PROJECTILE_COUNT     = 6

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

; CrouchingFlag = $17
RESV CrouchingFlag
Player_MovingDir = $5F
Fireball_MovingDir = $6D
PlayerFacingDir = $9F
EnemyFacingDir = $A0
SavedJoypadBits = $F7

.assert RES_BASE + RES_OFFSET < $01bd
.endif ; ENABLE_Z2_MARIO

.endif ; _Z2R_RAMMAP_INC