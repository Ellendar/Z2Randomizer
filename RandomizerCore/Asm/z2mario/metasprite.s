

;;;;;;;;;;;;;;;;;
;
; Custom metasprite defines for adding new metasprites to the game
; 
; A metasprite is a collection of 1 or more 8x16 sprite tiles and their offset from the point on the screen where
; they should be drawn. Every moving character like koopas or mario will have many different metasprites associated
; with them for each of the different animation frames.
;
; In order to easily add and change existing metasprites, I've added several new helper macros so you can easily
; go wild with creativity when adding new characters to the game. No longer are you restricted to the original
; 16x24 box that the vanilla enemies are confined within.
;
; I wrote this because I'm sick of seeing hacks that just replace a goomba's graphics and call it a day.
; Where's all the hacks making awesome new enemies or even backporting modern mario enemies?!

; Documentation:
; Because so many of the parameters are shared between each of the frames of an animation,
; this library simplifies by using variables that you define before calling any of the macros.

; These implicit parameters have a default value that is overridable to customize the sprite in various ways
; In order to override any of the implicit parameters, you just need to prefix the variable with either just the Object
; name or both the Object and Animation names. If you prefix the parameter with the Object name, it will apply to all
; frames of the Object. To apply to only a single frame of the animation, then separate the object and animation with an
; underscore.

; As an example, if you have a sprite where its position on the screen is supposed to be 8 pixels higher
; than its in game position, you can use the Y_OFFSET implicit parameter in order to shift it up just visually.
; So if you are working with the POWERUP_1_UP and want to shift it up slightly, make a new variable called
; `POWERUP_1_UP_Y_OFFSET = -8` before calling MetaspriteBox

; As always, maybe the best documentation is just looking at the many many examples in this file used to define
; all of the metasprites in the vanilla game.

;;;;;;;;;;;;;;;;;;;;;;;;
; Functions
;
; ****
; MetaspriteBox - Define a metasprite in either a 1x2 or 2x2 box of 8x16 sprites.
;   Params:
;     Object - string - Name of the metasprite.
;     Animation - string - Name of the frame of animation
;     Spr1, Spr2, Spr3, Spr4 - number - Each sprite tile in the animation
;     Sprite Tile Params: These can be OR'd with the tile number to override the attributes just for one tile
;       SPR_FLIP_H, SPR_FLIP_V - Force this particular tile to be flipped horizontally or vertically
;       SPR_PALETTE_0, SPR_PALETTE_1, SPR_PALETTE_2, SPR_PALETTE_3 - Force this tile to use a specific palette
; 
;   Implicit Params:
;     VRAM_OFFSET - number - Which sprite bank window to use. Adds the offset to each of the sprite tiles
;             In general, the sprite bank windows are as follows:
;               0   - Mario
;               1   - Misc sprites (like powerups and coins)
;               2-3 - Enemy Sprites (Banked by area type on MMC3)
;               4-7 - MMC5 ONLY! Expanded sprite window to allow more sprites
;     BANK - number - Bank number for the actual ROM bank used
;     PALETTE - number - Base palette for the metasprite
;     X_OFFSET - number - Number of pixels to offset the entire metasprite from the origin
;     Y_OFFSET - number - Number of pixels to offset the entire metasprite from the origin
;     NO_MIRROR - bool (1 or 0) - If 1, prevents generating a mirrored sprite for the left facing sprite. 
;                 (Useful if the sprite cannot turn around and should always face in one direction)
;
; ****
; MetaspriteDuplicate - Reuse an existing metasprite. Useful for duplicate animation frames 
;                       (This saves space because it will not duplicate the data)
;   Params
;     New Metasprite - string - Full name (object + animation) for the new metasprite
;     Old Metasprite - string - Full name (object + animation) for the original metasprite
;
; ****
; MetaspriteData - Define a metasprite with a pointer to the data
;   Params:
;     Name - string - Full name (object + animation) for the new metasprite
;     Left Data - word - Pointer to the data for the metasprite when facing left
;     Right Data - word (optional) - Optional Pointer to the data for the metasprite when facing right.
;                                    If missing, the Left data will be reused for the right facing sprite
;   Data Format for the data is as follows
;     .byte Length - 1 byte for total number of bytes in the metasprite. So if there are 4 sprites, then its 4 * 4
;     .struct SpriteData  - repeated for each of the sprites in the metasprite
;        .byte TileId    - Tile Number for the sprite taking into account 8x16 mode and which window its in
;        .byte Attribute
;        .byte YPosition
;        .byte XPosition
;     .endstruct
; 
; ****
; MetaspriteVramOffset - A small helper function to simplify combining the bank with the Tile ID
;                        when defining sprite data manually
;   Params:
;     Tile Id - 6 bit number - The tile number for the sprite
;     Bank - SPRITE_BANK_N - The Sprite bank (0-3 for MMC3 or 0-7 for MMC5) to use for this sprite
;                            See also the docs for VRAM_OFFSET since this is the same thing
;
; ****
; MetaspriteReserve - Reserve a particular spot in the metasprite table but leave it to be defined until later
;   Params:
;     Name - string - Name of the reserved metasprite
;

; Reserve Metasprite 0 as the "unused" sprite. This sprite won't be drawn to the screen

.include "z2r.inc"

.segment "PRG0"

.export PlayerBankTable, MetaspriteRenderLoop, METASPRITE_MARIO_SHIELD

PRINT_METASPRITE_IDS = 0

METASPRITES_COUNT .set 0

PATTERN_TABLE_0 = $00
PATTERN_TABLE_1 = $01
SPRITE_BANK_0 = $00 | PATTERN_TABLE_0
SPRITE_BANK_1 = $40 | PATTERN_TABLE_0
SPRITE_BANK_2 = $80 | PATTERN_TABLE_0
SPRITE_BANK_3 = $c0 | PATTERN_TABLE_0
SPRITE_BANK_4 = $00 | PATTERN_TABLE_1
SPRITE_BANK_5 = $40 | PATTERN_TABLE_1
SPRITE_BANK_6 = $80 | PATTERN_TABLE_1
SPRITE_BANK_7 = $c0 | PATTERN_TABLE_1


MSPR_VERTICAL_FLIP = %10000000
MSPR_OFFSET_MASK = %00111111
.define MetaspriteOffset(b) 0+((b) & MSPR_OFFSET_MASK)

; Custom values to signify that this sprite should be flipped in the metasprite
SPR_FLIP_H = %01000000 << 8
SPR_FLIP_V = %10000000 << 8
; Can be used to force a sprite palette. If NO_PALETTE is set, then the palette value
; from the object in the game will not be applied, and this value will be forced instead
SPR_NO_PALETTE = %00100000 << 8
SPR_PALETTE_0  = SPR_NO_PALETTE | %00000000 << 8
SPR_PALETTE_1  = SPR_NO_PALETTE | %00000001 << 8
SPR_PALETTE_2  = SPR_NO_PALETTE | %00000010 << 8
SPR_PALETTE_3  = SPR_NO_PALETTE | %00000011 << 8


.define MetaspriteVramOffset(b, vram) (((b) & %00111111) | vram)

.macro MetaspriteBoxBody Object, Animation, Direction, VramOffset, Palette, XOffset, YOffset, Spr1, Spr2, Spr3, Spr4
.local X1, X2, Y1, Y2, Size
.local Tile1, Tile2, Tile3, Tile4
.local Attr1, Attr2, Attr3, Attr4
.local DrawSprite1, DrawSprite2, DrawSprite3, DrawSprite4

X1 = 0+XOffset
X2 = 8+XOffset
Y1 = 8+YOffset
Y2 = 24+YOffset
Size .set 1

DrawSprite2 .set 0
DrawSprite3 .set 0
DrawSprite4 .set 0

DrawSprite1 .set 1
Tile1 .set MetaspriteVramOffset(<Spr1, VramOffset)

.ifnblank Spr2
DrawSprite2 .set 1
Size .set 2
Tile2 .set MetaspriteVramOffset(<Spr2, VramOffset)
.endif
.ifnblank Spr3
DrawSprite3 .set 1
Size .set 3
Tile3 .set MetaspriteVramOffset(<Spr3, VramOffset)
.endif
.ifnblank Spr4
DrawSprite4 .set 1
Size .set 4
Tile4 .set MetaspriteVramOffset(<Spr4, VramOffset)
.endif

Attr1 .set (Palette | ((>Spr1) >> 8))
.ifnblank Spr2
Attr2 .set (Palette | ((>Spr2) >> 8))
.endif
.ifnblank Spr3
Attr3 .set (Palette | ((>Spr3) >> 8))
.endif
.ifnblank Spr4
Attr4 .set (Palette | ((>Spr4) >> 8))
.endif

.if .xmatch(Direction, "LEFT")
; .out .sprintf("Switching tile and attr %s %s %s", Object, Animation, Direction)
.ifnblank Spr2
  Tmp .set Tile1
  Tile1 .set Tile2
  Tile2 .set Tmp
  Tmp .set Attr1
  Attr1 .set Attr2
  Attr2 .set Tmp
  Attr1 .set Attr1 ^ $40
  Attr2 .set Attr2 ^ $40
.endif
.if .blank(Spr4) .and(.not .blank(Spr3))
  DrawSprite4 .set 1
  DrawSprite3 .set 0
  Attr4 .set Attr3 ^ $40
  Tile4 .set Tile3
.endif
.ifnblank Spr4
  Tmp .set Tile3
  Tile3 .set Tile4
  Tile4 .set Tmp
  Tmp .set Attr3
  Attr3 .set Attr4
  Attr4 .set Tmp
  Attr3 .set Attr3 ^ $40
  Attr4 .set Attr4 ^ $40
.endif
.endif

.ident( .sprintf("MetaspriteData_%s_%s_%s", Object, Animation, Direction) ):
  .byte   (Size * 4)
.if DrawSprite1 <> 0
  .byte   Tile1, Attr1, Y1, X1
.endif
.if DrawSprite2 <> 0
  .byte   Tile2, Attr2, Y1, X2
.endif
.if DrawSprite3 <> 0
  .byte   Tile3, Attr3, Y2, X1
.endif
.if DrawSprite4 <> 0
  .byte   Tile4, Attr4, Y2, X2
.endif
.endmacro

.macro MetaspriteBox Object, Animation, Spr1, Spr2, Spr3, Spr4
.local VramOffset, Bank, Palette, XOffset, YOffset, Mirror

.if .defined(.ident( .sprintf("%s_%s_VRAM_OFFSET", Object, Animation) ))
  VramOffset = .ident( .sprintf("%s_%s_VRAM_OFFSET", Object, Animation) )
.elseif .defined(.ident( .sprintf("%s_VRAM_OFFSET", Object) ))
  VramOffset = .ident( .sprintf("%s_VRAM_OFFSET", Object) )
.else
  VramOffset = 0
.endif
.if .defined(.ident( .sprintf("%s_%s_BANK", Object, Animation) ))
  Bank = .ident( .sprintf("%s_%s_BANK", Object, Animation) )
.elseif .defined(.ident( .sprintf("%s_BANK", Object) ))
  Bank = .ident( .sprintf("%s_BANK", Object) )
.else
  Bank = $ff
.endif
.if .defined(.ident( .sprintf("%s_%s_PALETTE", Object, Animation) ))
  Palette = .ident( .sprintf("%s_%s_PALETTE", Object, Animation) )
.elseif .defined(.ident( .sprintf("%s_PALETTE", Object) ))
  Palette = .ident( .sprintf("%s_PALETTE", Object) )
.else
  .error .sprintf("Could not define Metasprite without the palette. Please define either %s_PALETTE or %s_%s_PALETTE", Object, Object, Animation)
.endif

.if .defined(.ident( .sprintf("%s_%s_Y_OFFSET", Object, Animation) ))
  YOffset = <.ident( .sprintf("%s_%s_Y_OFFSET", Object, Animation) )
.elseif .defined(.ident( .sprintf("%s_Y_OFFSET", Object) ))
  YOffset = <.ident( .sprintf("%s_Y_OFFSET", Object) )
.else
  YOffset = 0
.endif

.if .defined(.ident( .sprintf("%s_%s_X_OFFSET", Object, Animation) ))
  XOffset = <.ident( .sprintf("%s_%s_X_OFFSET", Object, Animation) )
.elseif .defined(.ident( .sprintf("%s_X_OFFSET", Object) ))
  XOffset = <.ident( .sprintf("%s_X_OFFSET", Object) )
.else
  XOffset = 0
.endif

.if .defined(.ident( .sprintf("%s_%s_NO_MIRROR", Object, Animation) ))
  Mirror = 0
.elseif .defined(.ident( .sprintf("%s_NO_MIRROR", Object) ))
  Mirror = 0
.else
  Mirror = 1
.endif

.ident( .sprintf("METASPRITE_%d_BANK",  METASPRITES_COUNT) ) = Bank

.ifdef METASPRITE_BODY

.if Mirror = 1
MetaspriteBoxBody Object, Animation, "LEFT", VramOffset, Palette, XOffset, YOffset, Spr1, Spr2, Spr3, Spr4
.endif
MetaspriteBoxBody Object, Animation, "RIGHT", VramOffset, Palette, XOffset, YOffset, Spr1, Spr2, Spr3, Spr4

.if PRINT_METASPRITE_IDS
.out .sprintf("#define METASPRITE_%s_%s 0x%02x", Object, Animation, METASPRITES_COUNT)
.endif
.if Mirror = 1
.ident( .sprintf("METASPRITE_LEFT_%d_LO",  METASPRITES_COUNT) ) = .lobyte(.ident( .sprintf("MetaspriteData_%s_%s_LEFT", Object, Animation) ))
.ident( .sprintf("METASPRITE_LEFT_%d_HI",  METASPRITES_COUNT) ) = .hibyte(.ident( .sprintf("MetaspriteData_%s_%s_LEFT", Object, Animation) ))
.else
.ident( .sprintf("METASPRITE_LEFT_%d_LO",  METASPRITES_COUNT) ) = .lobyte(.ident( .sprintf("MetaspriteData_%s_%s_RIGHT", Object, Animation) ))
.ident( .sprintf("METASPRITE_LEFT_%d_HI",  METASPRITES_COUNT) ) = .hibyte(.ident( .sprintf("MetaspriteData_%s_%s_RIGHT", Object, Animation) ))
.endif

.ident( .sprintf("METASPRITE_RIGHT_%d_LO",  METASPRITES_COUNT) ) = .lobyte(.ident( .sprintf("MetaspriteData_%s_%s_RIGHT", Object, Animation) ))
.ident( .sprintf("METASPRITE_RIGHT_%d_HI",  METASPRITES_COUNT) ) = .hibyte(.ident( .sprintf("MetaspriteData_%s_%s_RIGHT", Object, Animation) ))


.endif

MetaspriteReserve .sprintf("%s_%s",Object, Animation)
.endmacro

.macro MetaspriteDuplicate Name, Mspr
.Local LL, LH, RL, RH, Id, Bank
Id = .ident( .concat("METASPRITE_", Mspr) )
Bank = .ident( .sprintf("METASPRITE_%d_BANK", Id) )
.ident( .sprintf("METASPRITE_%d_BANK",  METASPRITES_COUNT) ) = Bank

.ifdef METASPRITE_BODY
.if PRINT_METASPRITE_IDS
.out .sprintf("#define METASPRITE_%s 0x%02x // (duplicate of %s)", Name, METASPRITES_COUNT, Mspr)
.endif

LL = .ident(.sprintf("METASPRITE_LEFT_%d_LO", Id))
LH = .ident(.sprintf("METASPRITE_LEFT_%d_HI", Id))
RL = .ident(.sprintf("METASPRITE_RIGHT_%d_LO", Id))
RH = .ident(.sprintf("METASPRITE_RIGHT_%d_HI", Id))

.ident( .sprintf("METASPRITE_LEFT_%d_LO",  METASPRITES_COUNT) ) = LL
.ident( .sprintf("METASPRITE_LEFT_%d_HI",  METASPRITES_COUNT) ) = LH
.ident( .sprintf("METASPRITE_RIGHT_%d_LO",  METASPRITES_COUNT) ) = RL
.ident( .sprintf("METASPRITE_RIGHT_%d_HI",  METASPRITES_COUNT) ) = RH

.endif

MetaspriteReserve Name
.endmacro


;;;;
; Reserves a spot in the metasprite table for this named constant.
; This is used internally when making a new metasprite, but can also
; be used to make your own metasprite. Just reserve a slot with the name
; of the metasprite and
.macro MetaspriteReserve Name
.ident( .sprintf("METASPRITE_%s", Name) ) = METASPRITES_COUNT
.export .ident( .sprintf("METASPRITE_%s", Name) )
METASPRITES_COUNT .set METASPRITES_COUNT + 1
.endmacro

.macro MetaspriteData Name, Left, Right
.Local Id

Id = .ident(Name)

.ident( .sprintf("METASPRITE_LEFT_%d_LO",  Id) ) = .lobyte(Left)
.ident( .sprintf("METASPRITE_LEFT_%d_HI",  Id) ) = .hibyte(Left)
.ifnblank Right
.ident( .sprintf("METASPRITE_RIGHT_%d_LO", Id) ) = .lobyte(Right)
.ident( .sprintf("METASPRITE_RIGHT_%d_HI", Id) ) = .hibyte(Right)
.else
.ident( .sprintf("METASPRITE_RIGHT_%d_LO", Id) ) = .lobyte(Left)
.ident( .sprintf("METASPRITE_RIGHT_%d_HI", Id) ) = .hibyte(Left)
.endif

.endmacro

.proc DrawMetasprite
Ptr = R0
; OrigOffset = R2
Atr = R3
Xlo = R4
Xhi = R5
Ylo = R6
Yhi = R7
; LoopCount = M0
; VFlip = M1

;  cpx #METASPRITES_COUNT
;  bcc +
;    rts ; Temp work around to prevent crashing when trying to render bad msprs
;  +

  cpx #LAST_MARIO_METASPRITE
  bcc +
    ; I'm not a mario sprite so I need to use the MovingDir instead of FacingDir
    lda Player_MovingDir,y
    jmp @checkFacing
  +
  lda PlayerFacingDir,y
@checkFacing:
  lsr
  bne FacingLeft
    lda MetaspriteTableRightLo,x
    sta Ptr
    lda MetaspriteTableRightHi,x
    sta Ptr+1
    bne DrawSprite ; unconditional
  FacingLeft:
    lda MetaspriteTableLeftLo,x
    sta Ptr
    lda MetaspriteTableLeftHi,x
    sta Ptr+1
DrawSprite:

  ; sty OrigOffset

  lda SprObject_X_Position,y
  sec
  sbc ScreenLeft_X_Pos
  sta Xlo
  lda SprObject_PageLoc,y
  sbc ScreenLeft_PageLoc
  sta Xhi
  lda SprObject_Y_HighPos,y
  sta Yhi

  lda SprObject_Y_Position,y
SetYOffset:
  sta Ylo

;   lda VFlip
;   beq DontSetFlipBit
;     lda #OAM_FLIP_V
;     .byte $2c ; masks the next lda #0
; DontSetFlipBit:
  lda #0
  ora SprObject_SprAttrib,y
  sta Atr

  txa
  pha
  tya
  pha
    jsr MetaspriteRenderLoop
  pla
  tay
  pla
  cmp #LAST_MARIO_METASPRITE
  bcs @exit
  ; Check if this is the boss
  cpy #0
  beq @isplayer
    ; Rendering dark mario so use the dark mario pattern table
    ldx CurrentOAMOffset
    lda #%11000000
    ora Sprite_Tilenumber - 16,x
    sta Sprite_Tilenumber - 16,x
    lda #%11000000
    ora Sprite_Tilenumber - 12,x
    sta Sprite_Tilenumber - 12,x
    lda #%11000000
    ora Sprite_Tilenumber - 8,x
    sta Sprite_Tilenumber - 8,x
    lda #%11000000
    ora Sprite_Tilenumber - 4,x
    sta Sprite_Tilenumber - 4,x
    rts
  @isplayer:
  ; We are rendering a mario sprite, so check if we are stuck in the mud
  lda $0752
  and #$20
  beq @exit
    ; stuck in the mud, so update bottom two sprite
    ; x = next oam sprite id, so we can offset by 8 to hit bottom two sprites
    ldx CurrentOAMOffset
    ; if we are small then its the most recent two sprites. if we are big then its 4 sprites back
    lda PlayerSize
    beq + ; If we are large
      lda Sprite_Attributes - 8, x
      ora #$20
      sta Sprite_Attributes - 8, x
      lda Sprite_Attributes - 4, x
      ora #$20
      sta Sprite_Attributes - 4, x
      rts
    +
      lda Sprite_Attributes - 16, x
      ora #$20
      sta Sprite_Attributes - 16, x
      lda Sprite_Attributes - 12, x
      ora #$20
      sta Sprite_Attributes - 12, x
@exit:
  rts

.endproc

.proc MetaspriteRenderLoop
Ptr = R0
Atr = R3
Xlo = R4
Xhi = R5
Ylo = R6
Yhi = R7

  ldx CurrentOAMOffset
  ldy #0
  lda (Ptr),y
  tay
  bpl RenderLoop

; Offscreen sprites end up here

Skip4:   ; X Offscreen
    dey
Skip3:   ; Y Offscreen
    dey
Skip2:
    dey
    ; Move this sprite offscreen
    lda #$f8
    sta Sprite_Y_Position,x
    inx
    inx
    inx
    inx
    dey
    beq LoopEnded
RenderLoop:
    ; load the x position and make sure its on screen
    clc
    lda (Ptr),y
    bpl PositiveX
      adc Xlo
      sta Sprite_X_Position,x
      lda Xhi
      adc #$ff
      beq ContinueAfterX
      bne Skip4
  PositiveX:
    adc Xlo
    sta Sprite_X_Position,x
    lda Xhi
    adc #0
    bne Skip4
  ContinueAfterX:
    dey

    ; load the y position and also make sure its on screen
    clc
    lda (Ptr),y
    bpl PositiveY
      ; NegativeY
      adc Ylo
      sta Sprite_Y_Position,x
      lda Yhi
      adc #$ff
      cmp #1      ; page 1 is the "main" y position
      beq ContinueAfterY
      bne Skip3
  PositiveY:
    adc Ylo
    sta Sprite_Y_Position,x
    lda Yhi
    adc #0
    cmp #1      ; page 1 is the "main" y position
    bne Skip3
  ContinueAfterY:
    dey

    ; Mix attributes but if the NO_PALETTE bit is set, prevent
    ; the palette from changing.
    lda (Ptr),y
    bit NoPaletteBitMask
    beq AllowPaletteChange
      ; No palette change bit set, so pull the byte and
      ; mask off the palette from the attribute byte
      lda Atr
      and #%11111100
      ora (Ptr),y
      bne WritePalette ; unconditional
AllowPaletteChange:
    ora Atr
WritePalette:
    sta Sprite_Attributes,x
    dey

    ; set the tile number and move to the next sprite
    lda (Ptr),y
    sta Sprite_Tilenumber,x
    inx
    inx
    inx
    inx
    dey
    bne RenderLoop
LoopEnded:
  stx CurrentOAMOffset
  rts

NoPaletteBitMask:
  .byte (SPR_NO_PALETTE >> 8)
.endproc


.export DrawMetasprite

METASPRITE_BODY = 1

MetaspriteReserve "NULL"

BIG_MARIO_VRAM_OFFSET = SPRITE_BANK_0
BIG_MARIO_BANK = CHR_BIGMARIO
BIG_MARIO_PALETTE = $00
BIG_MARIO_X_OFFSET = 8
BIG_MARIO_Y_OFFSET = -8
MetaspriteBox "BIG_MARIO", "STANDING",  $00, $02, $20, $22
MetaspriteBox "BIG_MARIO", "WALKING_1", $04, $06, $24, $26
MetaspriteBox "BIG_MARIO", "WALKING_2", $08, $0a, $28, $2a
MetaspriteBox "BIG_MARIO", "WALKING_3", $0c, $0e, $2c, $2e
MetaspriteBox "BIG_MARIO", "SKIDDING",  $10, $12, $30, $32
MetaspriteBox "BIG_MARIO", "JUMPING",   $14, $16, $34, $36
MetaspriteBox "BIG_MARIO", "CROUCHING", $18, $1a, $38, $3a

BIG_MARIO_SWIMMING_1_KICK_BANK = CHR_MARIOACTION
BIG_MARIO_SWIMMING_2_KICK_BANK = CHR_MARIOACTION
BIG_MARIO_SWIMMING_3_KICK_BANK = CHR_MARIOACTION
BIG_MARIO_SWIMMING_1_HOLD_BANK = CHR_MARIOACTION
BIG_MARIO_SWIMMING_2_HOLD_BANK = CHR_MARIOACTION
BIG_MARIO_SWIMMING_3_HOLD_BANK = CHR_MARIOACTION
MetaspriteBox "BIG_MARIO", "SWIMMING_1_KICK", $00, $02, $20, $22
MetaspriteBox "BIG_MARIO", "SWIMMING_2_KICK", $04, $06, $20, $26
MetaspriteBox "BIG_MARIO", "SWIMMING_3_KICK", $04, $06, $2a, $22
MetaspriteBox "BIG_MARIO", "SWIMMING_1_HOLD", $00, $02, $24, $22
MetaspriteBox "BIG_MARIO", "SWIMMING_2_HOLD", $04, $06, $24, $26
MetaspriteBox "BIG_MARIO", "SWIMMING_3_HOLD", $04, $06, $28, $22

.export FIRE_MARIO_OFFSET
FIRE_MARIO_OFFSET = METASPRITES_COUNT - METASPRITE_BIG_MARIO_STANDING
FIRE_MARIO_VRAM_OFFSET = SPRITE_BANK_0
FIRE_MARIO_BANK = CHR_MARIOACTION
FIRE_MARIO_PALETTE = $00
FIRE_MARIO_X_OFFSET = 8
FIRE_MARIO_Y_OFFSET = -8
MetaspriteBox "FIRE_MARIO", "STANDING",  $00, $02, $2c, $2e
; MetaspriteBox "BIG_MARIO", "FIRE_WALKING_1", $00, $02, $2c, $2e
MetaspriteDuplicate "FIRE_MARIO_WALKING_1", "FIRE_MARIO_STANDING"
MetaspriteBox "FIRE_MARIO", "WALKING_2", $00, $02, $30, $32
MetaspriteBox "FIRE_MARIO", "WALKING_3", $00, $02, $14, $16
MetaspriteBox "FIRE_MARIO", "SKIDDING",  $00, $02, $34, $36
MetaspriteBox "FIRE_MARIO", "JUMPING",   $00, $02, $10, $12
FIRE_MARIO_CROUCHING_BANK = CHR_SMALLFIRE
MetaspriteBox "FIRE_MARIO", "CROUCHING", $00, $02, $18, $1a

MetaspriteBox "FIRE_MARIO", "SWIMMING_1_KICK", $00, $02, $20, $22
MetaspriteBox "FIRE_MARIO", "SWIMMING_2_KICK", $04, $06, $20, $26
MetaspriteBox "FIRE_MARIO", "SWIMMING_3_KICK", $04, $06, $2a, $22
MetaspriteBox "FIRE_MARIO", "SWIMMING_1_HOLD", $00, $02, $24, $22
MetaspriteBox "FIRE_MARIO", "SWIMMING_2_HOLD", $04, $06, $24, $26
MetaspriteBox "FIRE_MARIO", "SWIMMING_3_HOLD", $04, $06, $28, $22

BIG_MARIO_CLIMBING_1_BANK = CHR_MARIOACTION
BIG_MARIO_CLIMBING_2_BANK = CHR_MARIOACTION
MetaspriteBox "BIG_MARIO", "CLIMBING_1",   $00, $02, $08, $0a
MetaspriteBox "BIG_MARIO", "CLIMBING_2",   $04, $06, $0c, $0e

SMALL_MARIO_VRAM_OFFSET = SPRITE_BANK_0
SMALL_MARIO_BANK = CHR_SMALLMARIO
SMALL_MARIO_PALETTE = $00
SMALL_MARIO_X_OFFSET = 8
SMALL_MARIO_Y_OFFSET = 8
MetaspriteBox "SMALL_MARIO", "STANDING",   $00, $02
MetaspriteBox "SMALL_MARIO", "WALKING_1",  $04, $06
MetaspriteBox "SMALL_MARIO", "WALKING_2",  $08, $0a
MetaspriteBox "SMALL_MARIO", "WALKING_3",  $0c, $0e
MetaspriteBox "SMALL_MARIO", "SKIDDING",   $10, $12
MetaspriteBox "SMALL_MARIO", "JUMPING",    $14, $16

MetaspriteBox "SMALL_MARIO", "SWIMMING_1_KICK", $20, $22
MetaspriteBox "SMALL_MARIO", "SWIMMING_2_KICK", $28, $2a
MetaspriteBox "SMALL_MARIO", "SWIMMING_3_KICK", $30, $32
MetaspriteBox "SMALL_MARIO", "SWIMMING_1_HOLD", $24, $26
MetaspriteBox "SMALL_MARIO", "SWIMMING_2_HOLD", $2c, $2e
MetaspriteDuplicate "SMALL_MARIO_SWIMMING_3_HOLD", "SMALL_MARIO_SWIMMING_3_KICK"

SMALL_FIRE_VRAM_OFFSET = SPRITE_BANK_0
SMALL_FIRE_BANK = CHR_SMALLFIRE
SMALL_FIRE_PALETTE = $00
SMALL_FIRE_X_OFFSET = 8
SMALL_FIRE_Y_OFFSET = -8
MetaspriteBox "SMALL_FIRE", "STANDING",   $00, $02, $20, $22
MetaspriteBox "SMALL_FIRE", "WALKING_1",  $00, $02, $04, $06
MetaspriteBox "SMALL_FIRE", "WALKING_2",  $00, $02, $08, $0a
MetaspriteBox "SMALL_FIRE", "WALKING_3",  $00, $02, $0c, $0e
MetaspriteBox "SMALL_FIRE", "SKIDDING",   $00, $02, $10, $12
MetaspriteBox "SMALL_FIRE", "JUMPING",    $00, $02, $14, $16

.export SWIMMING_ANIMATION_FRAME_COUNT
SWIMMING_ANIMATION_FRAME_COUNT = METASPRITE_BIG_MARIO_SWIMMING_1_HOLD - METASPRITE_BIG_MARIO_SWIMMING_1_KICK
MetaspriteBox "SMALL_FIRE", "SWIMMING_1_KICK", $00, $02, $34, $36
MetaspriteBox "SMALL_FIRE", "SWIMMING_2_KICK", $00, $02, $30, $32
MetaspriteBox "SMALL_FIRE", "SWIMMING_3_KICK", $00, $02, $34, $36
MetaspriteBox "SMALL_FIRE", "SWIMMING_1_HOLD", $00, $02, $18, $1a
MetaspriteBox "SMALL_FIRE", "SWIMMING_2_HOLD", $00, $02, $2c, $2e
MetaspriteDuplicate "SMALL_FIRE_SWIMMING_3_HOLD", "SMALL_FIRE_SWIMMING_3_KICK"

MetaspriteBox "SMALL_MARIO", "CLIMBING_1", $18, $1a
MetaspriteBox "SMALL_MARIO", "CLIMBING_2", $38, $3a
MetaspriteBox "SMALL_MARIO", "DEATH",      $34, $36

SMALL_MARIO_GROW_STANDING_BANK = CHR_SMALLMARIO
BIG_MARIO_GROW_INTERMEDIATE_BANK = CHR_MARIOACTION
BIG_MARIO_GROW_STANDING_BANK = CHR_BIGMARIO
MetaspriteDuplicate "SMALL_MARIO_GROW_STANDING", "SMALL_MARIO_STANDING"
MetaspriteBox "BIG_MARIO", "GROW_INTERMEDIATE", $18, $1a, $38, $3a
MetaspriteDuplicate "BIG_MARIO_GROW_STANDING", "BIG_MARIO_STANDING"

; Glitchy looking sprite used when mario fires a fireball with no momentum in the water.
FIRE_MARIO_SWIMMING_STILL_1_BANK = CHR_MARIOACTION
MetaspriteDuplicate "FIRE_MARIO_SWIMMING_STILL_1", "FIRE_MARIO_STANDING"
; This second sprite is a unique one with the "Hold" foot sprite overwriting the walking sprite
FIRE_MARIO_SWIMMING_STILL_2_BANK = CHR_MARIOACTION
MetaspriteBox "FIRE_MARIO", "SWIMMING_STILL_2", $00, $02, $24, $2e
; Glitchy looking sprite used when mario fires a fireball with no momentum in the water.
SMALL_FIRE_SWIMMING_STILL_1_BANK = CHR_SMALLFIRE
MetaspriteDuplicate "SMALL_FIRE_SWIMMING_STILL_1", "FIRE_MARIO_STANDING"
; This second sprite is a unique one with the "Hold" foot sprite overwriting the walking sprite
SMALL_FIRE_SWIMMING_STILL_2_BANK = CHR_SMALLFIRE
MetaspriteBox "SMALL_FIRE", "SWIMMING_STILL_2", $00, $02, $28, $2a

; Update this with the first and last metasprite if more are added before or after
TOTAL_MARIO_METASPRITES = METASPRITE_SMALL_FIRE_SWIMMING_STILL_2 - METASPRITE_BIG_MARIO_STANDING + 1

LAST_MARIO_METASPRITE = METASPRITES_COUNT

FIREBALL_VRAM_OFFSET = SPRITE_BANK_0
FIREBALL_PALETTE = $01
FIREBALL_X_OFFSET = 8
FIREBALL_Y_OFFSET = -12
EXPLOSION_VRAM_OFFSET = SPRITE_BANK_1
EXPLOSION_PALETTE = $00
EXPLOSION_X_OFFSET = 8
EXPLOSION_Y_OFFSET = -12  ; Vanilla offsets the explosion by -4. The other -8 is to account for position due to 8x16 sprites?
MetaspriteBox "FIREBALL", "FRAME_1", $3c
MetaspriteBox "FIREBALL", "FRAME_2", $3e
; The Explosion CHR is pasted over part of links downstab animation
MetaspriteBox "EXPLOSION", "FRAME_1", $00, $00 | SPR_FLIP_H
MetaspriteBox "EXPLOSION", "FRAME_2", $02, $02 | SPR_FLIP_H
MetaspriteBox "EXPLOSION", "FRAME_3", $04, $04 | SPR_FLIP_H


HAMMER_FRAME_1_VRAM_OFFSET = SPRITE_BANK_2
HAMMER_FRAME_2_VRAM_OFFSET = SPRITE_BANK_0
HAMMER_FRAME_1_X_OFFSET = 4
HAMMER_FRAME_1_Y_OFFSET = -8
HAMMER_FRAME_2_X_OFFSET = 0
HAMMER_FRAME_2_Y_OFFSET = -8
HAMMER_PALETTE = $01
MetaspriteBox "HAMMER", "FRAME_1", $18
MetaspriteBox "HAMMER", "FRAME_2", $1c, $1e

MARIO_SHIELD_VRAM_OFFSET = SPRITE_BANK_1
MARIO_SHIELD_Y_OFFSET = -8
MARIO_SHIELD_PALETTE = $00
MetaspriteBox "MARIO", "SHIELD", $06

;;;;;;;;;;;
; NULL Metasprite needs to be reserved in slot 0 to allow disabling drawing a sprite before its deleted

MetaspriteData "METASPRITE_NULL", $0000


;;;;
; Internal use
; Generate the lookup tables for the metasprites used by the renderer.
; All metasprites must be defined before this part
.export MetaspriteTableLeftLo, MetaspriteTableLeftHi, MetaspriteTableRightLo, MetaspriteTableRightHi
MetaspriteTableLeftLo:
.repeat METASPRITES_COUNT, I
  .byte .ident(.sprintf("METASPRITE_LEFT_%d_LO", I))
.endrepeat

MetaspriteTableLeftHi:
.repeat METASPRITES_COUNT, I
  .byte .ident(.sprintf("METASPRITE_LEFT_%d_HI", I))
.endrepeat

MetaspriteTableRightLo:
.repeat METASPRITES_COUNT, I
  .byte .ident(.sprintf("METASPRITE_RIGHT_%d_LO", I))
.endrepeat
MetaspriteTableRightHi:
.repeat METASPRITES_COUNT, I
  .byte .ident(.sprintf("METASPRITE_RIGHT_%d_HI", I))
.endrepeat

PlayerBankTable = PlayerBankTableReal - METASPRITE_BIG_MARIO_STANDING
PlayerBankTableReal:
.repeat TOTAL_MARIO_METASPRITES+METASPRITE_BIG_MARIO_STANDING, I
.if I >= METASPRITE_BIG_MARIO_STANDING
  .byte .lobyte(.ident(.sprintf("METASPRITE_%d_BANK", I)))
.endif
.endrepeat
TOTAL_MARIO_SPRITE_BANK = * - PlayerBankTableReal
.assert TOTAL_MARIO_SPRITE_BANK = TOTAL_MARIO_METASPRITES, error, .sprintf("Total number of Mario Metasprites (%d) does not match the mario bank table (%d)! Update the bank table to match ", TOTAL_MARIO_METASPRITES, TOTAL_MARIO_SPRITE_BANK)




