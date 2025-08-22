.segment "PRG0"
; UPDATE_BYTE $00 @ $0cef

;update mario palette in overworld
.org $8cdf
  .byte $00
; update raft tiles
.org $81C0
  lda #$b1
  sta $0201
  lda #$b3
  sta $0205
; update raft tiles
.org $81DA
  lda #$b3
  sta $0201
  lda #$b1
  sta $0205

.org $8728
  jmp DrawMarioMap
.reloc
DrawMarioMap:
  sec
  sbc #4
  ; LDA #$84 - 4 ; -6
  STA Sprite_X_Position
  CLC
  ADC #$08
  STA Sprite_X_Position+4
  LDA #$6D
  STA Sprite_Y_Position
  STA Sprite_Y_Position+4
  LDA #$73
  STA Sprite_Tilenumber+4
;  LDA $0562
;  CMP #$04
;  BCS L_8763
;  LSR A
;  LDA #$00
;  ROR A
;  LSR A
;  STA Sprite_Attributes
;  STA Sprite_Attributes+4  
  LDX #$AF
  lda SavedJoypadBits
  and #Left_Dir+Right_Dir+Up_Dir+Down_Dir
  ora $7D
  BEQ L_875E
;  PHP
  ldx mapMarioIndex
  lda mapMarioTimer
  bne ++
  lda #7
  sta mapMarioTimer
  inx
  cpx #3
  bcc +
  ldx #0
  +
  stx mapMarioIndex
  ++
  dec mapMarioTimer
  lda MarioWalkingSprites,x
  tax
;  PLP
L_875E:
  STX Sprite_Tilenumber
;  PHP
  DEX
  DEX
  STX Sprite_Tilenumber+4

  ldx #$00
  stx $00
  lda $0562
  cmp #Down_Dir
  bcs @upOrDown
  ; flip the attribute if doing right
  cmp #Right_Dir
  bne +
    ldx #$40
    stx $00
  +
  stx Sprite_Attributes
  stx Sprite_Attributes+4
  jmp @checkfortileflip
@upOrDown:
  ; beq @down
  ; up
  lda mapMarioPrevDirection
  ; flip the attribute if doing right
  cmp #Right_Dir
  bne +
    ldx #$40
    stx $00
  +
  stx Sprite_Attributes
  stx Sprite_Attributes+4

; @down:
;   ldx #$00
;   stx Sprite_Attributes
;   stx Sprite_Attributes+4
@checkfortileflip:

  ; flip the sprite tiles if the direction is right
  lda $00
  bne @noflip
    lda Sprite_Tilenumber
    pha
      lda Sprite_Tilenumber+4
      sta Sprite_Tilenumber
    pla
    sta Sprite_Tilenumber+4  
@noflip:

  ; store our most recent left/right press here
  lda $0562
  and #Left_Dir+Right_Dir
  beq +
    sta mapMarioPrevDirection
+

;  PLP
;  BPL L_877C
;L_8763:
;  LDX #$7B
;  CMP #$04
;  BEQ L_876B
;  DEX
;  DEX
;L_876B:
;  STX Sprite_Tilenumber
;  LDX #$40
;  LDA $7D
;  BEQ L_8779
;  AND #$08
;  BNE L_8779
;  TAX
;L_8779:
;  STX Sprite_Attributes
L_877C:
  LDX #$0E
  LDY #$00
  STY $00
L_8782:
  LDA $00
  STA Sprite_Y_Position+8,Y
  CLC
  ADC #$10
  STA $00
  LDA #$7F
  STA Sprite_Tilenumber+8,Y
  LDA #$F8
  STA Sprite_X_Position+8,Y
  LDA #$01
  STA Sprite_Attributes+8,Y
  INY
  INY
  INY
  INY
  DEX
  BPL L_8782
  RTS
MarioWalkingSprites:
  .byte $ab, $a7, $a3