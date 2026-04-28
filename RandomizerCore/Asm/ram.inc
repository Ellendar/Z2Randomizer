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
DrawLink = $19                           ; [SS] 0 = Invisible, 1 = Normal, 2-FF = Fall in hole - [File Select] Cursor position
zp_1a = $1a
zp_1b = $1b
zp_1c = $1c
zp_1d = $1d
NameInputCursor = $1e
zp_1f = $1f
Enemy0YPositionHi = $20
Enemy1YPositionHi = $21
Enemy2YPositionHi = $22
Enemy3YPositionHi = $23
Enemy4YPositionHi = $24
Enemy5YPositionHi = $25
OverworldStepCounter = $26
zp_27 = $27
zp_28 = $28
LinkYPos = $29
Enemy0YPositionLo = $2a
Enemy1YPositionLo = $2b
Enemy2YPositionLo = $2c
Enemy3YPositionLo = $2d
Enemy4YPositionLo = $2e
Enemy5YPositionLo = $2f
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
LinkXPositionLo = $4d
Enemy0XPositionLo = $4e
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
LinkAnimationFrame = $80
Enemy0AnimationFrame = $81
OverworldEnemy0Type = $82                ; [SS] Enemy1AnimationFrame - [OW] OverworldEnemy0Type, 1 = Weak, 2 = Strong, 3 = Fairy
OverworldEnemy1Type = $83                ; [SS] Enemy2AnimationFrame - [OW] OverworldEnemy1Type
OverworldEnemy2Type = $84                ; [SS] Enemy3AnimationFrame - [OW] OverworldEnemy2Type
OverworldEnemy3Type = $85                ; [SS] Enemy4AnimationFrame - [OW] OverworldEnemy3Type
OverworldEnemy4Type = $86                ; [SS] Enemy5AnimationFrame - [OW] OverworldEnemy4Type
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

; Potentially free zero page memory bytes at $f1-$f4 and since they are used by FDS BIOS but not the NES
Controller1ButtonsPressed = $f5
Controller2ButtonsPressed = $f6
Controller1ButtonsHeld = $f7
Controller2ButtonsHeld = $f8
; Potentially free zero page memory bytes at $f9-$fc
ScrollPosShadow = $fd
PpuCtrlShadow = $ff

LinkXSubpixel = $03d6
Enemy0XSubpixel = $03d7
Enemy1XSubpixel = $03d8
Enemy2XSubpixel = $03d9
Enemy3XSubpixel = $03da
Enemy4XSubpixel = $03db
Enemy5XSubpixel = $03dc
LinkYVelocityLo = $03e6

LinkSwordAttackFrame = $0400
Enemy0HitState = $040e
LinkInAir = $0479                        ; 0 = On ground, 1 = Falling from walking off platform, 2 = Jumping
LandingAnimationTimer = $0497
HitboxXCoord = $047e
HitboxYCoord = $0480

LinkIFrameTimer = $500
timer_501 = $0501                        ; $501 through $50d are timers that are decreased each frame until 0
SwordAttackTimer = $050a
EncounterSpawnTimer = $0516
RNG = $051b
MapNumber = $0561
TownNumber = $056b
PalaceNumber = $056c
LinkYVelocityHi = $057d
Enemy0YVelocity = $057e
LinkProjectile0Timer = $05ca
LinkProjectile1Timer = $05cb             ; Always 0 since Fire projectiles don't expire
SmallDropCount = $05df
LargeDropCount = $05e0

Lives = $0700
EnterFacing = $0701                      ; 0=Enter sideview facing right (enter from the left), 1=Enter sideview facing left (enter from the right)
RegionNumber = $0706                     ; 0=West, 1=Death Mountain, 2=East, 3=Maze Island
WorldNumber = $0707                      ; 0=Caves & enemy encounters, 1=West towns, 2=East towns, 3=Palace 1/2/5, 4=Palace 3/4/6, 5=Great Palace
DialogBoxDrawing = $0726                 ; >0 when drawing dialog
ScrollFrozen = $0728                     ; 1=Screen scrolling frozen (for boss fights)
SideviewCommandY = $0730
SideviewCommandOp = $0731
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
EnterPage = $075c                        ; Page to enter a sideview on (0-3)
CurrentPrgBank = $0769
CurrentChrBank = $076e
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
