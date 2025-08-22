
.segment "PRG0"

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
