

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

.export PlayerBankTable

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


; .proc DrawAllMetasprites

; LoopCount = M0

;   lda #24 - 1 ; size of the different object update list
;   sta LoopCount
;   lda SpriteShuffleOffset
;   clc
;   adc #19
;   cmp #24
;   bcc :+
;     ; implicit carry set
;     sbc #24
; :
;   sta SpriteShuffleOffset
;   lda FrameCounter
;   lsr
;   lda SpriteShuffleOffset
;   bcc ObjectLoopNegative

; ObjectLoopPositive:
;     clc
;     adc #13
;     cmp #24
;     bcc :+
;       ; implicit carry set
;       sbc #24
;     :
;     ; skip index zero since we draw the player first always.
;     beq NextLoop
;       ; TODO check offscreenbits to make sure they are onscreen still
;       tay
;       ldx ObjectMetasprite,y
;       beq NextLoop
;       cpx #METASPRITES_COUNT ; todo remove this after fixing all bugs
;       bcs NextLoop
;         sta SpriteShuffleOffset
;         jsr DrawMetasprite
;         lda SpriteShuffleOffset
;   NextLoop:
;     dec LoopCount
;     bpl ObjectLoopPositive
;   jmp HandleFloateyNumbers

; ObjectLoopNegative:
;     sec
;     sbc #13
;     bcs :+
;       ; implicit carry clear
;       adc #24
;     :
;     ; skip index zero since we draw the player first always.
;     beq NextLoop2
;       ; TODO check offscreenbits to make sure they are onscreen still
;       tay
;       ldx ObjectMetasprite,y
;       beq NextLoop2
;       cpx #METASPRITES_COUNT ; todo remove this after fixing all bugs
;       bcs NextLoop2
;         sta SpriteShuffleOffset
;         jsr DrawMetasprite
;         lda SpriteShuffleOffset
;   NextLoop2:
;     dec LoopCount
;     bpl ObjectLoopNegative

; HandleFloateyNumbers:
;   sta SpriteShuffleOffset

;   ldx #7-1
; FloateyNumberLoop:
;     lda FloateyNum_Control,x     ;load control for floatey number
;     beq Skip                     ;if zero, branch to leave
;       phx
;         jsr FloateyNumberRender
;       plx
;   Skip:
;     dex
;     bpl FloateyNumberLoop

  ; lda CurrentOAMOffset
  ; pha

    ; ; put the player in slot 0 always
    ; lda #0
    ; sta CurrentOAMOffset

    ; ; draw the player first so it doesn't ever flicker
    ; ; first clear out the sprites that are reserved for the player
    ; lda #$f8
    ; sta Sprite_Y_Position + 0
    ; sta Sprite_Y_Position + 4
    ; sta Sprite_Y_Position + 8
    ; sta Sprite_Y_Position + 12

; .if ::MOUSE_DISPLAY_CURSOR
;     sta Sprite_Y_Position + 16
;     sta Sprite_Y_Position + 20
;     lda mouse
;     bmi connected
;       ; not connected so draw the alternate sprite at the middle of the screen
;       lda #256 / 2 - 8
;       sta mouse + kMouseX
;       lda #240 / 2 - 8
;       sta mouse + kMouseY

;       lda FrameCounter
;       and #%00100000
;       beq useothersprite ; switch between connected and not connected
;         ldy #1
;         bne loadmetasprite ;unconditional
;       useothersprite:
;         ldy #0
;         beq loadmetasprite
; connected:
;     ldy MouseState
; loadmetasprite:
;     ldx MouseStateMetasprite,y
;     lda MetaspriteTableLeftLo,x
;     sta R0 ; Ptr
;     lda MetaspriteTableLeftHi,x
;     sta R1 ; Ptr
;     lda #0
;     sta R3 ; Atr
;     lda mouse + kMouseX
;     sta R4 ; Xlo
;     lda #0
;     sta R5 ; Xhi
;     lda mouse + kMouseY
;     sta R6 ; Ylo
;     lda #1
;     sta R7 ; Yhi
;     jsr MetaspriteRenderLoop
; .endif

  ;   ldy #0
  ;   ldx ObjectMetasprite,y
  ;   ; unless the player is currently flickering due to damage taken
  ;   beq DoneDrawingPlayer
  ;     ; Load the bank for the player if it changed
  ;     lda PlayerBankTable,x
  ;     cmp PlayerChrBank
  ;     beq :+
  ;       sta PlayerChrBank
  ;       inc ReloadCHRBank
  ;     :
  ;     jsr DrawMetasprite
  ; DoneDrawingPlayer:

  ; Clear sprites up to the offset
  ; Calculate CurrentOAMOffset / 4 * 3 and add that to the OAM clear to jump
  ; the middle of the clear routine based on how many we need to clear
  ; pla ; CurrentOAMOffset
  ; lsr
  ; sta M0
  ; lsr
  ; ; clc - always cleared
  ; adc M0
  ; ; clc - always cleared
  ; adc #<OAMClear
  ; sta M0
  ; lda #>OAMClear
  ; adc #0
  ; sta M1
  ; lda #$f8
  ; jmp (M0)

; .if ::MOUSE_DISPLAY_CURSOR
; MouseStateMetasprite:
;   .byte METASPRITE_MOUSE_POINTER
;   .byte METASPRITE_MOUSE_DISCONNECTED
;   .byte METASPRITE_MOUSE_PINCH
; .endif
; .endproc

; MoveAllSpritesOffscreen:
;   ldy #0
;   sty CurrentOAMOffset
;   lda #$f8
; OAMClear:
; .repeat 64, I
;     sta Sprite_Y_Position + I*4 ; write 248 into OAM data's Y coordinate
; .endrepeat
;   rts


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

  cpx #METASPRITES_COUNT
  bcc +
    rts ; Temp work around to prevent crashing when trying to render bad msprs
  +

  lda PlayerFacingDir,y
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

;   lda #0
;   sta VFlip
;   cpy #7
;   bcs SkipVerticalFlipCheck
;     lda ObjectVerticalFlip,y
;     bpl NoVFlip
;       ; Object has a vertical flip attribute set
;       sta VFlip
;   NoVFlip:
;       ; Object has an added Y offset
;       ; The negative flag is bit 5 in our number, so sign extend the value
;       and #%00111111
;       cmp #%00100000
;       bcc PositiveYOffset
;         ; subtracting a small negative offset here
;         ora #%11000000
;         clc
;     PositiveYOffset:
;       adc SprObject_Y_Position,y
;       bcc SetYOffset
;         ; If the carry is set, then we need to inc Yhi ... TODO
;         ; this probably doesn't handle underflow properly?
;         inc Yhi
;         bcs SetYOffset ; unconditional
; SkipVerticalFlipCheck:
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

  jsr MetaspriteRenderLoop


;   lda VFlip
;   beq DontShiftPositions
;     ldy #0
;     lda (Ptr),y
;     cmp #8 + 1
;     bcc DontShiftPositions
;     cmp #12
;     beq Flip3
;       ; otherwise flip all 4 sprites
;       lda Sprite_Tilenumber-4,x     ;with first or second row tiles
;       pha                         ;and save tiles to the stack
;         lda Sprite_Tilenumber-8,x
;         pha
;           lda Sprite_Tilenumber-12,x  ;exchange third row tiles
;           sta Sprite_Tilenumber-4,x     ;with first or second row tiles
;           lda Sprite_Tilenumber-16,x
;           sta Sprite_Tilenumber-8,x
;         pla                         ;pull first or second row tiles from stack
;         sta Sprite_Tilenumber-16,x  ;and save in third row
;       pla
;       sta Sprite_Tilenumber-12,x
;       rts
;   Flip3:
;     ; Custom flip code for bowser's front since he is weird.
;     lda Sprite_Y_Position-4,x
;     pha
;       lda Sprite_Y_Position-12,x
;       sta Sprite_Y_Position-8,x
;       sta Sprite_Y_Position-4,x
;     pla
;     sta Sprite_Y_Position-12,x
; DontShiftPositions:
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

; ;-------------------------------------------------------------------------------------
; .proc FloateyNumberRender
; Ptr = R0
; ; OrigOffset = R2
; Atr = R3
; Xlo = R4
; Xhi = R5
; Ylo = R6
; Yhi = R7

;   lda FloateyNum_Y_Pos,x       ;get vertical coordinate for
;   cmp #$18                     ;floatey number, if coordinate in the
;   bcc SetupNumSpr              ;status bar, branch
;     sbc #$01
;     sta FloateyNum_Y_Pos,x       ;otherwise subtract one and store as new
; SetupNumSpr:
;   sta Ylo
;   lda #1
;   sta Yhi
;   lda #0
;   sta Atr
;   sta Xhi
;   lda FloateyNum_X_Pos,x
;   sta Xlo
;   ldy FloateyNum_Control,x
;   ldx FloateyNumMetasprites-1,y
;   lda MetaspriteTableRightLo,x
;   sta Ptr
;   lda MetaspriteTableRightHi,x
;   sta Ptr+1
;   jmp MetaspriteRenderLoop
;   ; implicit rts

; FloateyNumMetasprites:
;   .byte METASPRITE_NUMBER_100
;   .byte METASPRITE_NUMBER_200
;   .byte METASPRITE_NUMBER_400
;   .byte METASPRITE_NUMBER_500
;   .byte METASPRITE_NUMBER_800
;   .byte METASPRITE_NUMBER_1000
;   .byte METASPRITE_NUMBER_2000
;   .byte METASPRITE_NUMBER_4000
;   .byte METASPRITE_NUMBER_5000
;   .byte METASPRITE_NUMBER_8000
;   .byte METASPRITE_NUMBER_1UP

; .endproc

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

;POWERUP_VRAM_OFFSET = SPRITE_BANK_1
;; POWERUP_Y_OFFSET = 8
;POWERUP_1UP_PALETTE = $01
;POWERUP_STAR_PALETTE = $00
;POWERUP_FIREFLOWER_PALETTE = $00
;POWERUP_FIREFLOWER_Y_OFFSET = -8
;POWERUP_MUSHROOM_PALETTE = $02
;POWERUP_1UP_NO_MIRROR = 1
;POWERUP_MUSHROOM_NO_MIRROR = 1
;MetaspriteBox "POWERUP", "STAR",       $34, $36
;; Force the lower two parts of the fireflower to be palette two by using the override
;MetaspriteBox "POWERUP", "FIREFLOWER", $38, $38 | SPR_FLIP_H , $3a | SPR_PALETTE_1, $3a | SPR_FLIP_H | SPR_PALETTE_1
;MetaspriteBox "POWERUP", "MUSHROOM",   $3c, $3e
;MetaspriteBox "POWERUP", "1UP",        $3c, $3e

FIREBALL_VRAM_OFFSET = SPRITE_BANK_0
FIREBALL_PALETTE = $00
FIREBALL_X_OFFSET = 8
FIREBALL_Y_OFFSET = -12
EXPLOSION_VRAM_OFFSET = SPRITE_BANK_1
EXPLOSION_PALETTE = $00
EXPLOSION_X_OFFSET = 8
EXPLOSION_Y_OFFSET = -12  ; Vanilla offsets the explosion by -4. The other -8 is to account for position due to 8x16 sprites?
MetaspriteBox "FIREBALL", "FRAME_1", $3c
MetaspriteBox "FIREBALL", "FRAME_2", $3e
MetaspriteBox "EXPLOSION", "FRAME_1", $26, $26 | SPR_FLIP_H
MetaspriteBox "EXPLOSION", "FRAME_2", $28, $28 | SPR_FLIP_H
MetaspriteBox "EXPLOSION", "FRAME_3", $2a, $2a | SPR_FLIP_H
; Firebars will still draw Fireballs manually for now 

;COIN_VRAM_OFFSET = SPRITE_BANK_1
;COIN_PALETTE = $02
;MetaspriteBox "COIN", "FRAME_1", $2c
;MetaspriteBox "COIN", "FRAME_2", $2e
;MetaspriteBox "COIN", "FRAME_3", $30
;MetaspriteBox "COIN", "FRAME_4", $32

;MISC_VRAM_OFFSET = SPRITE_BANK_1
;MISC_PALETTE = $03
;MISC_Y_OFFSET = -8

; The brick and block sprite palettes are set by the code
;MISC_BRICK_GROUND_PALETTE = $00
;MISC_BRICK_OTHER_PALETTE = $00 ; The brick palette is set by the code
;MISC_BLOCK_PALETTE = $00 ; The block palette is set by the code
;MetaspriteBox "MISC", "BRICK_OTHER", $20, $20
;MetaspriteBox "MISC", "BRICK_GROUND", $22, $22
;MetaspriteBox "MISC", "BLOCK", $24, $24 | SPR_FLIP_H
; drawn manually for now until i figure out if i have frames for the brick
; break animation with just metasprites
; MetaspriteBox "MISC", "CHUNK", $16
;MISC_BUBBLE_PALETTE = $02
;MetaspriteBox "MISC", "BUBBLE", $18
;MetaspriteBox "MISC", "SMALL_OVERLAY", $1e, $1e
;MetaspriteBox "MISC", "LARGE_OVERLAY", $1e, $1e, $1e, $1e
;MISC_STAR_FLAG_VRAM_OFFSET = SPRITE_BANK_2
;MISC_STAR_FLAG_PALETTE = $02
;MetaspriteBox "MISC", "STAR_FLAG", $3c, $3e
; MISC_FLAGPOLE_FLAG_VRAM_OFFSET = SPRITE_BANK_3
; MISC_FLAGPOLE_FLAG_PALETTE = $01
; MetaspriteBox "MISC", "FLAGPOLE_FLAG", $32, $34

; PLATFORM_VRAM_OFFSET = SPRITE_BANK_1
; PLATFORM_PALETTE = $02
; MetaspriteReserve "PLATFORM_GIRDER_SMALL"
; MetaspriteReserve "PLATFORM_GIRDER_SMALL_FRAME_2"
; MetaspriteReserve "PLATFORM_GIRDER_LARGE"
; MetaspriteReserve "PLATFORM_GIRDER_LARGE_FRAME_2"
; MetaspriteReserve "PLATFORM_CLOUD_SMALL"
; MetaspriteReserve "PLATFORM_CLOUD_SMALL_FRAME_2"
; MetaspriteReserve "PLATFORM_CLOUD_LARGE"
; MetaspriteReserve "PLATFORM_CLOUD_LARGE_FRAME_2"

; NUMBER_VRAM_OFFSET = SPRITE_BANK_1
; NUMBER_PALETTE = $02
; NUMBER_Y_OFFSET = -16

; MetaspriteBox "NUMBER", "100", $06, $10
; MetaspriteBox "NUMBER", "200", $08, $10
; MetaspriteBox "NUMBER", "400", $0a, $10
; MetaspriteBox "NUMBER", "500", $0c, $10
; MetaspriteBox "NUMBER", "800", $0e, $10
; MetaspriteBox "NUMBER", "1000", $06, $04
; MetaspriteBox "NUMBER", "2000", $08, $04
; MetaspriteBox "NUMBER", "4000", $0a, $04
; MetaspriteBox "NUMBER", "5000", $0c, $04
; MetaspriteBox "NUMBER", "8000", $0e, $04
; MetaspriteBox "NUMBER", "1UP", $12, $14


; GOOMBA_VRAM_OFFSET = SPRITE_BANK_2
; GOOMBA_PALETTE = $03
; GOOMBA_DEAD_Y_OFFSET = $08
; MetaspriteBox "GOOMBA", "WALKING_1", $22, $24
; MetaspriteBox "GOOMBA", "WALKING_2", $24 | SPR_FLIP_H, $22 | SPR_FLIP_H
; MetaspriteBox "GOOMBA", "DEAD", $26, $26 | SPR_FLIP_H


; KOOPA_VRAM_OFFSET = SPRITE_BANK_2
; KOOPA_Y_OFFSET = -8
; KOOPA_PALETTE = $00 ; Don't set a palette here so we can do red or green koopa later
; ; Koopa shell is offset by 2 pixels for the right side up animation. 
; ; We account for this in the code by adding 2 with the vertical flip flag
; KOOPA_SHELL_Y_OFFSET = 0
; KOOPA_SHELL_REVIVE_Y_OFFSET = 0
; MetaspriteBox "KOOPA", "WALKING_1", $38, $12, $30, $32
; MetaspriteBox "KOOPA", "WALKING_2", $18, $16, $34, $36
; MetaspriteBox "KOOPA", "SHELL", $1a, $1a | SPR_FLIP_H
; MetaspriteBox "KOOPA", "SHELL_REVIVE", $3a, $3a | SPR_FLIP_H
; MetaspriteBox "KOOPA", "FLYING_1", $10, $12, $30, $32
; MetaspriteBox "KOOPA", "FLYING_2", $14, $16, $34, $36


; PIRANHA_VRAM_OFFSET = SPRITE_BANK_3
; PIRANHA_PALETTE = $01
; PIRANHA_Y_OFFSET = -8
; MetaspriteBox "PIRANHA", "MOUTH_OPEN", $1c, $1c | SPR_FLIP_H, $3c, $3c | SPR_FLIP_H
; MetaspriteBox "PIRANHA", "MOUTH_CLOSED", $1e, $1e | SPR_FLIP_H, $3e, $3e | SPR_FLIP_H

; BULLET_VRAM_OFFSET = SPRITE_BANK_2
; BULLET_PALETTE = $03
; MetaspriteBox "BULLET", "BILL", $1c, $1e

; PODOBOO_VRAM_OFFSET = SPRITE_BANK_4
; PODOBOO_PALETTE = $02
; MetaspriteBox "PODOBOO", "UP", $18, $18 | SPR_FLIP_H


; BUZZY_BEETLE_VRAM_OFFSET = SPRITE_BANK_2
; BUZZY_BEETLE_PALETTE = $03
; ; See note about koopa shell for this offset
; BUZZY_BEETLE_SHELL_Y_OFFSET = 0
; MetaspriteBox "BUZZY_BEETLE", "WALKING_1", $00, $02
; MetaspriteBox "BUZZY_BEETLE", "WALKING_2", $04, $06
; MetaspriteBox "BUZZY_BEETLE", "SHELL", $20, $20 | SPR_FLIP_H


; CHEEP_CHEEP_VRAM_OFFSET = SPRITE_BANK_2
; CHEEP_CHEEP_PALETTE = $00
; MetaspriteBox "CHEEP_CHEEP", "SWIM_1", $2c, $2e
; MetaspriteBox "CHEEP_CHEEP", "SWIM_2", $2a, $2e


; BLOOPER_VRAM_OFFSET = SPRITE_BANK_3
; BLOOPER_PALETTE = $03
; BLOOPER_Y_OFFSET = -8
; BLOOPER_SWIM_2_Y_OFFSET = (-8 + 3) ; original code adds 3 px to the y position
; MetaspriteBox "BLOOPER", "SWIM_1", $1a, $1a | SPR_FLIP_H, $3a, $3a | SPR_FLIP_H
; MetaspriteBox "BLOOPER", "SWIM_2", $18, $18 | SPR_FLIP_H

 HAMMER_FRAME_1_VRAM_OFFSET = SPRITE_BANK_2
 HAMMER_FRAME_2_VRAM_OFFSET = SPRITE_BANK_0
 HAMMER_FRAME_1_X_OFFSET = 4
 HAMMER_FRAME_1_Y_OFFSET = -8
 HAMMER_FRAME_2_X_OFFSET = 0
 HAMMER_FRAME_2_Y_OFFSET = -8
 HAMMER_PALETTE = $01
 MetaspriteBox "HAMMER", "FRAME_1", $18
 MetaspriteBox "HAMMER", "FRAME_2", $1c, $1e

; HAMMER_BRO_VRAM_OFFSET = SPRITE_BANK_3
; HAMMER_BRO_PALETTE = $01
; HAMMER_BRO_Y_OFFSET = -8
; MetaspriteBox "HAMMER_BRO", "WALK_1", $00, $02, $20, $22
; MetaspriteBox "HAMMER_BRO", "WALK_2", $08, $0a, $24, $26
; MetaspriteBox "HAMMER_BRO", "THROW_1", $04, $06, $20, $22
; MetaspriteBox "HAMMER_BRO", "THROW_2", $04, $06, $24, $26


; LAKITU_VRAM_OFFSET = SPRITE_BANK_3
; LAKITU_PALETTE = $01
; LAKITU_NORMAL_Y_OFFSET = -16
; LAKITU_THROWING_Y_OFFSET = 0
; MetaspriteBox "LAKITU", "NORMAL", $0c, $0e, $2c, $2e
; MetaspriteBox "LAKITU", "THROWING", $30, $30 | SPR_FLIP_H


; SPINY_VRAM_OFFSET = SPRITE_BANK_2
; SPINY_PALETTE = $02
; SPINY_EGG_1_VRAM_OFFSET = SPRITE_BANK_3
; SPINY_EGG_2_VRAM_OFFSET = SPRITE_BANK_3
; MetaspriteBox "SPINY", "WALK_1", $08, $0a
; MetaspriteBox "SPINY", "WALK_2", $0c, $0e
; MetaspriteBox "SPINY", "EGG_1", $10, $10 | SPR_FLIP_H | SPR_FLIP_V
; MetaspriteBox "SPINY", "EGG_2", $12, $12 | SPR_FLIP_H | SPR_FLIP_V


; JUMPSPRING_VRAM_OFFSET = SPRITE_BANK_3
; JUMPSPRING_PALETTE = $02
; JUMPSPRING_Y_OFFSET = -8
; MetaspriteBox "JUMPSPRING", "FRAME_1", $16, $16 | SPR_FLIP_H , $36, $36 | SPR_FLIP_H
; MetaspriteBox "JUMPSPRING", "FRAME_2", $38, $38 | SPR_FLIP_H
; MetaspriteBox "JUMPSPRING", "FRAME_3", $14, $14

; BOWSER_VRAM_OFFSET = SPRITE_BANK_4
; BOWSER_PALETTE = $01
; BOWSER_Y_OFFSET = -8
; MetaspriteBox "BOWSER", "FRONT_MOUTH_OPEN", $08, $0a, $28
; MetaspriteBox "BOWSER", "FRONT_MOUTH_CLOSED", $0c, $0e, $28
; MetaspriteBox "BOWSER", "REAR_WALK_1", $04, $06, $20, $22
; MetaspriteBox "BOWSER", "REAR_WALK_2", $04, $06, $24, $26

; MetaspriteReserve "BOWSER_FLAME"

; PEACH_VRAM_OFFSET = SPRITE_BANK_4
; PEACH_PALETTE = $02
; PEACH_Y_OFFSET = -8
; MetaspriteBox "PEACH", "STANDING", $14, $16, $34, $34 | SPR_FLIP_H
; TOAD_VRAM_OFFSET = SPRITE_BANK_4
; TOAD_PALETTE = $02
; TOAD_Y_OFFSET = -8
; MetaspriteBox "TOAD", "STANDING", $10, $10 | SPR_FLIP_H, $12, $12 | SPR_FLIP_H

; .if ::MOUSE_DISPLAY_CURSOR
; MOUSE_VRAM_OFFSET = SPRITE_BANK_5
; MOUSE_PALETTE = $02
; MOUSE_NO_MIRROR = 1
; MOUSE_Y_OFFSET = -8
; MetaspriteBox "MOUSE", "POINTER", $00, $02
; MetaspriteBox "MOUSE", "PINCH", $04, $06
; MetaspriteBox "MOUSE", "DISCONNECTED", $08, $0a
; .endif


; Extra sprite tiles that aren't using metasprites

; These aren't actually sprite tiles, they are the replacement tile for
; the rope when using a balance platform
BALANCE_PLATFORM_ROPE_1 = $7d
BALANCE_PLATFORM_ROPE_2 = $7e

; Brick chunks have a really custom movement and making them a metasprite
; wasn't worth the trouble for now.
BRICK_CHUNK_TILE = MetaspriteVramOffset {$16}, {SPRITE_BANK_1}

; Used by the "small platform" which is the elevator platforms
; PLATFORM_GIRDER = $5b
PLATFORM_GIRDER = MetaspriteVramOffset {$1a}, {SPRITE_BANK_1}

; Vine drawing is probably possible to do as a metasprite but not worth the trouble
VINE_TILE_1 = MetaspriteVramOffset {$28}, {SPRITE_BANK_3}
VINE_TILE_2 = MetaspriteVramOffset {$2a}, {SPRITE_BANK_3}

; Used when drawing firebars (not when shooting fireballs which are metasprites)
FIREBALL_TILE1 = MetaspriteVramOffset {$3c}, {SPRITE_BANK_0}
FIREBALL_TILE2 = MetaspriteVramOffset {$3e}, {SPRITE_BANK_0}


;;;;;;;;;;;
; NULL Metasprite needs to be reserved in slot 0 to allow disabling drawing a sprite before its deleted

MetaspriteData "METASPRITE_NULL", $0000

;;;;;;;;;;;
; Girder Platform

; Y_OFFSET .set 0
; X_OFFSET .set 0
; PALETTE  .set 2

; MetaspriteData "METASPRITE_PLATFORM_GIRDER_SMALL", MetaspritePlatformGirderSmall
; MetaspriteData "METASPRITE_PLATFORM_GIRDER_SMALL_FRAME_2", MetaspritePlatformGirderSmallFrame2

; MetaspritePlatformGirderSmall:
;   .byte  4 * 4
; .repeat 4, I
;   .byte  MetaspriteVramOffset{$1a}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (I * 8)
; .endrepeat

; MetaspritePlatformGirderSmallFrame2:
;   .byte  4 * 4
; .repeat 4, I
;   .byte  MetaspriteVramOffset{$1a}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (((I + 2) .mod 4) * 8)
; .endrepeat

; MetaspriteData "METASPRITE_PLATFORM_GIRDER_LARGE", MetaspritePlatformGirderLarge
; MetaspriteData "METASPRITE_PLATFORM_GIRDER_LARGE_FRAME_2", MetaspritePlatformGirderLargeFrame2

; MetaspritePlatformGirderLarge:
;   .byte  6 * 4
; .repeat 6, I
;   .byte  MetaspriteVramOffset{$1a}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (I * 8)
; .endrepeat

; MetaspritePlatformGirderLargeFrame2:
;   .byte  6 * 4
; .repeat 6, I
;   .byte  MetaspriteVramOffset{$1a}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (((I + 3) .mod 6) * 8)
; .endrepeat

; ;;;;;;;;;;;
; ; Cloud Platform

; Y_OFFSET .set 0
; X_OFFSET .set 0
; PALETTE  .set 2

; MetaspriteData "METASPRITE_PLATFORM_CLOUD_SMALL", MetaspritePlatformCloudSmall
; MetaspriteData "METASPRITE_PLATFORM_CLOUD_SMALL_FRAME_2", MetaspritePlatformCloudSmallFrame2

; MetaspritePlatformCloudSmall:
;   .byte  4 * 4
; .repeat 4, I
;   .byte  MetaspriteVramOffset{$1c}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (I * 8)
; .endrepeat

; MetaspritePlatformCloudSmallFrame2:
;   .byte  4 * 4
; .repeat 4, I
;   .byte  MetaspriteVramOffset{$1c}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (((I + 2) .mod 4) * 8)
; .endrepeat

; MetaspriteData "METASPRITE_PLATFORM_CLOUD_LARGE", MetaspritePlatformCloudLarge
; MetaspriteData "METASPRITE_PLATFORM_CLOUD_LARGE_FRAME_2", MetaspritePlatformCloudLargeFrame2

; MetaspritePlatformCloudLarge:
;   .byte  6 * 4
; .repeat 6, I
;   .byte  MetaspriteVramOffset{$1c}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (I * 8)
; .endrepeat

; MetaspritePlatformCloudLargeFrame2:
;   .byte  6 * 4
; .repeat 6, I
;   .byte  MetaspriteVramOffset{$1c}, {SPRITE_BANK_1}, PALETTE,  0 + Y_OFFSET,  0 + X_OFFSET + (((I + 3) .mod 6) * 8)
; .endrepeat


; Y_OFFSET .set -4
; X_OFFSET .set 0
; PALETTE  .set 2
; MetaspriteData "METASPRITE_BOWSER_FLAME", MetaspriteBowserFlame, MetaspriteBowserFlame
; MetaspriteBowserFlame:
;   .byte 3 * 4
;   .byte  MetaspriteVramOffset{$2d}, {SPRITE_BANK_4}, PALETTE,   0 + Y_OFFSET,  0 + X_OFFSET
;   .byte  MetaspriteVramOffset{$2f}, {SPRITE_BANK_4}, PALETTE,   0 + Y_OFFSET,  8 + X_OFFSET
;   .byte  MetaspriteVramOffset{$31}, {SPRITE_BANK_4}, PALETTE,   0 + Y_OFFSET, 16 + X_OFFSET





;;;;
; Internal use
; Generate the lookup tables for the metasprites used by the renderer.
; All metasprites must be defined before this part

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




