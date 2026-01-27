
.include "z2r.inc"

.export Square1SfxHandler, Square2SfxHandler, NoiseSfxHandler

.segment "PRG6"

;-------------------------------------------------------------------------------------
.reloc
; .proc SFXSoundEngine
  ;jsr $92F4
  ;jsr $9408
  ;jsr $95A7
  ; We need to clock the counters each frame to keep audio synced
  ; zelda 2 already clocks the framecounter
;   lda #$ff
;   sta SND_FRAMECOUNTER      ;disable irqs and set frame counter mode???
; SFXSoundEngine:
;   lda PauseModeFlag         ;is sound already in pause mode?
;   bne InPause
;     lda PauseSoundQueue       ;if not, check pause sfx queue    
;     cmp #$01
;     bne RunSoundSubroutines   ;if queue is empty, skip pause mode routine
; InPause: lda PauseSoundBuffer      ;check pause sfx buffer
;          bne ContPau
;          lda PauseSoundQueue       ;check pause queue
;          beq SkipSoundSubroutines
;          ; Play the ding dong sound for pausing
;          sta PauseSoundBuffer      ;if queue full, store in buffer and activate
;          sta Square1SoundBuffer
;         ;  sta PauseModeFlag         ;pause mode to interrupt game sounds
;          lda #$00                  ;disable sound and clear sfx buffers
;         ;  sta SND_MASTERCTRL_REG
;          sta Square2SoundBuffer
;          sta NoiseSoundBuffer
;         ;  lda #$0f
;         ;  sta SND_MASTERCTRL_REG    ;enable sound again
;          lda #$2a                  ;store length of sound in pause counter
;          sta Squ1_SfxLenCounter
; PTone1F: lda #$44                  ;play first tone
;          bne PTRegC                ;unconditional branch
; ContPau: lda Squ1_SfxLenCounter    ;check pause length left
;          cmp #$24                  ;time to play second?
;          beq PTone2F
;          cmp #$1e                  ;time to play first again?
;          beq PTone1F
;          cmp #$18                  ;time to play second again?
;          bne DecPauC               ;only load regs during times, otherwise skip
; PTone2F: lda #$64                  ;store reg contents and play the pause sfx
; PTRegC:  ldx #$84
;          ldy #$7f
;          jsr PlaySqu1Sfx
; DecPauC: dec Squ1_SfxLenCounter    ;decrement pause sfx counter
;          bne SkipSoundSubroutines
        ;  lda #$00                  ;disable sound if in pause mode and
        ;  sta SND_MASTERCTRL_REG    ;not currently playing the pause sfx
        ;  lda PauseSoundBuffer      ;if no longer playing pause sfx, check to see
        ;  cmp #$02                  ;if we need to be playing sound again
        ;  bne SkipPIn
;          lda #$00                  ;clear pause mode to allow game sounds again
;          sta PauseModeFlag
; SkipPIn: lda #$00                  ;clear pause sfx buffer
;          sta PauseSoundBuffer
;          beq SkipSoundSubroutines

; RunSoundSubroutines:
;          jsr Square1SfxHandler  ;play sfx on square channel 1
;          jsr Square2SfxHandler  ; ''  ''  '' square channel 2
;          jsr NoiseSfxHandler    ; ''  ''  '' noise channel
;         ;  jsr MusicHandler       ;play music on all channels
;         ;  lda #$00               ;clear the music queues
;         ;  sta AreaMusicQueue
;         ;  sta EventMusicQueue

; SkipSoundSubroutines:
;           lda #$00               ;clear the sound effects queues
;           sta Square1SoundQueue
;           sta Square2SoundQueue
;           sta NoiseSoundQueue
;           ; sta PauseSoundQueue
; ;           ldy DAC_Counter        ;load some sort of counter 
; ;           lda AreaMusicBuffer
; ;           and #%00000011         ;check for specific music
; ;           beq NoIncDAC
; ;           inc DAC_Counter        ;increment and check counter
; ;           cpy #$30
; ;           bcc StrWave            ;if not there yet, just store it
; ; NoIncDAC: tya
; ;           beq StrWave            ;if we are at zero, do not decrement 
; ;           dec DAC_Counter        ;decrement counter
; ; StrWave:  sty SND_DELTA_REG+1    ;store into DMC load register (??)
;           rts                    ;we are done here
; ; .endproc

;--------------------------------

Dump_Squ1_Regs:
      sty SND_SQUARE1_REG+1  ;dump the contents of X and Y into square 1's control regs
      stx SND_SQUARE1_REG
      rts
      
PlaySqu1Sfx:
      jsr Dump_Squ1_Regs     ;do sub to set ctrl regs for square 1, then set frequency regs

SetFreq_Squ1:
      ldx #$00               ;set frequency reg offset for square 1 sound channel

Dump_Freq_Regs:
        tay
        lda FreqRegLookupTbl+1,y  ;use previous contents of A for sound reg offset
        beq NoTone                ;if zero, then do not load
        sta SND_SQUARE1_REG+2,x      ;first byte goes into LSB of frequency divider
        lda FreqRegLookupTbl,y    ;second byte goes into 3 MSB plus extra bit for 
        ora #%00001000            ;length counter
        sta SND_SQUARE1_REG+3,x
NoTone: rts

Dump_Sq2_Regs:
      stx SND_SQUARE2_REG    ;dump the contents of X and Y into square 2's control regs
      sty SND_SQUARE2_REG+1
      rts

PlaySqu2Sfx:
      jsr Dump_Sq2_Regs      ;do sub to set ctrl regs for square 2, then set frequency regs

SetFreq_Squ2:
      ldx #$04               ;set frequency reg offset for square 2 sound channel
      bne Dump_Freq_Regs     ;unconditional branch

;--------------------------------

SwimStompEnvelopeData:
      .byte $9f, $9b, $98, $96, $95, $94, $92, $90
      .byte $90, $9a, $97, $95, $93, $92

PlayFlagpoleSlide:
       lda #$40               ;store length of flagpole sound
       sta Squ1_SfxLenCounter
       lda #$62               ;load part of reg contents for flagpole sound
       jsr SetFreq_Squ1
       ldx #$99               ;now load the rest
       bne FPS2nd

PlaySmallJump:
       lda #$26               ;branch here for small mario jumping sound
       bne JumpRegContents

PlayBigJump:
       lda #$18               ;branch here for big mario jumping sound

JumpRegContents:
       ldx #$82               ;note that small and big jump borrow each others' reg contents
       ldy #$a7               ;anyway, this loads the first part of mario's jumping sound
       jsr PlaySqu1Sfx
       lda #$28               ;store length of sfx for both jumping sounds
       sta Squ1_SfxLenCounter ;then continue on here

ContinueSndJump:
          lda Squ1_SfxLenCounter ;jumping sounds seem to be composed of three parts
          cmp #$25               ;check for time to play second part yet
          bne N2Prt
          ldx #$5f               ;load second part
          ldy #$f6
          bne DmpJpFPS           ;unconditional branch
N2Prt:    cmp #$20               ;check for third part
          bne DecJpFPS
          ldx #$48               ;load third part
FPS2nd:   ldy #$bc               ;the flagpole slide sound shares part of third part
DmpJpFPS: jsr Dump_Squ1_Regs
          bne DecJpFPS           ;unconditional branch outta here

PlayFireballThrow:
        lda #$05
        ldy #$99                 ;load reg contents for fireball throw sound
        bne Fthrow               ;unconditional branch

PlayBump:
          lda #$0a                ;load length of sfx and reg contents for bump sound
          ldy #$93
Fthrow:   ldx #$9e                ;the fireball sound shares reg contents with the bump sound
          sta Squ1_SfxLenCounter
          lda #$0c                ;load offset for bump sound
          jsr PlaySqu1Sfx

ContinueBumpThrow:    
          lda Squ1_SfxLenCounter  ;check for second part of bump sound
          cmp #$06   
          bne DecJpFPS
          lda #$bb                ;load second part directly
          sta SND_SQUARE1_REG+1
DecJpFPS: bne BranchToDecLength1  ;unconditional branch


Square1SfxHandler:
       ldy Square1SoundQueue   ;check for sfx in queue
       beq CheckSfx1Buffer
       sty Square1SoundBuffer  ;if found, put in buffer
       bmi PlaySmallJump       ;small jump
       lsr Square1SoundQueue
       bcs PlayBigJump         ;big jump
       lsr Square1SoundQueue
       bcs PlayBump            ;bump
       lsr Square1SoundQueue
       bcs PlaySwimStomp       ;swim/stomp
       lsr Square1SoundQueue
       bcs PlaySmackEnemy      ;smack enemy
       lsr Square1SoundQueue
       bcc +
        jmp PlayPipeDownInj    ;pipedown/injury
       + 
       lsr Square1SoundQueue
       bcs PlayFireballThrow   ;fireball throw
       lsr Square1SoundQueue
       bcc +
        jmp PlayFlagpoleSlide  ;slide flagpole
       + 

CheckSfx1Buffer:
       lda Square1SoundBuffer   ;check for sfx in buffer 
       beq ExS1H                ;if not found, exit sub
       bmi ContinueSndJump      ;small mario jump 
       lsr
       bcs ContinueSndJump      ;big mario jump 
       lsr
       bcs ContinueBumpThrow    ;bump
       lsr
       bcs ContinueSwimStomp    ;swim/stomp
       lsr
       bcs ContinueSmackEnemy   ;smack enemy
       lsr
       bcs ContinuePipeDownInj  ;pipedown/injury
       lsr
       bcs ContinueBumpThrow    ;fireball throw
       lsr
       bcs DecrementSfx1Length  ;slide flagpole
ExS1H: rts

PlaySwimStomp:
      lda #$0e               ;store length of swim/stomp sound
      sta Squ1_SfxLenCounter
      ldy #$9c               ;store reg contents for swim/stomp sound
      ldx #$9e
      lda #$26
      jsr PlaySqu1Sfx

ContinueSwimStomp: 
      ldy Squ1_SfxLenCounter        ;look up reg contents in data section based on
      lda SwimStompEnvelopeData-1,y ;length of sound left, used to control sound's
      sta SND_SQUARE1_REG           ;envelope
      cpy #$06   
      bne BranchToDecLength1
      lda #$9e                      ;when the length counts down to a certain point, put this
      sta SND_SQUARE1_REG+2         ;directly into the LSB of square 1's frequency divider

BranchToDecLength1: 
      bne DecrementSfx1Length  ;unconditional branch (regardless of how we got here)

PlaySmackEnemy:
      lda #$0e                 ;store length of smack enemy sound
      ldy #$cb
      ldx #$9f
      sta Squ1_SfxLenCounter
      lda #$28                 ;store reg contents for smack enemy sound
      jsr PlaySqu1Sfx
      bne DecrementSfx1Length  ;unconditional branch

ContinueSmackEnemy:
        ldy Squ1_SfxLenCounter  ;check about halfway through
        cpy #$08
        bne SmSpc
        lda #$a0                ;if we're at the about-halfway point, make the second tone
        sta SND_SQUARE1_REG+2   ;in the smack enemy sound
        lda #$9f
        bne SmTick
SmSpc:  lda #$90                ;this creates spaces in the sound, giving it its distinct noise
SmTick: sta SND_SQUARE1_REG

DecrementSfx1Length:
      dec Squ1_SfxLenCounter    ;decrement length of sfx
      bne ExSfx1

StopSquare1Sfx:
        ldx #$00                ;if end of sfx reached, clear buffer
        stx Square1SoundBuffer  ;and stop making the sfx
        ; ldx #$0e
        ; stx SND_MASTERCTRL_REG
        ; ldx #$0f
        ; stx SND_MASTERCTRL_REG
ExSfx1: rts

PlayPipeDownInj:  
      lda #$2f                ;load length of pipedown sound
      sta Squ1_SfxLenCounter

ContinuePipeDownInj:
         lda Squ1_SfxLenCounter  ;some bitwise logic, forces the regs
         lsr                     ;to be written to only during six specific times
         bcs NoPDwnL             ;during which d3 must be set and d1-0 must be clear
         lsr
         bcs NoPDwnL
         and #%00000010
         beq NoPDwnL
         ldy #$91                ;and this is where it actually gets written in
         ldx #$9a
         lda #$44
         jsr PlaySqu1Sfx
NoPDwnL: jmp DecrementSfx1Length

;--------------------------------

ExtraLifeFreqData:
      .byte $58, $02, $54, $56, $4e, $44

PowerUpGrabFreqData:
      .byte $4c, $52, $4c, $48, $3e, $36, $3e, $36, $30
      .byte $28, $4a, $50, $4a, $64, $3c, $32, $3c, $32
      .byte $2c, $24, $3a, $64, $3a, $34, $2c, $22, $2c

PUp_VGrow_FreqData:
      .byte $14, $04, $22, $24, $16, $04, $24, $26 ;used by both
      .byte $18, $04, $26, $28, $1a, $04, $28, $2a
      .byte $1c, $04, $2a, $2c, $1e, $04, $2c, $2e ;used by vinegrow
      .byte $20, $04, $2e, $30, $22, $04, $30, $32

PlayCoinGrab:
        lda #$35             ;load length of coin grab sound
        ldx #$8d             ;and part of reg contents
        bne CGrab_TTickRegL

PlayTimerTick:
        lda #$06             ;load length of timer tick sound
        ldx #$98             ;and part of reg contents

CGrab_TTickRegL:
        sta Squ2_SfxLenCounter 
        ldy #$7f                ;load the rest of reg contents 
        lda #$42                ;of coin grab and timer tick sound
        jsr PlaySqu2Sfx

ContinueCGrabTTick:
        lda Squ2_SfxLenCounter  ;check for time to play second tone yet
        cmp #$30                ;timer tick sound also executes this, not sure why
        bne N2Tone
        lda #$54                ;if so, load the tone directly into the reg
        sta SND_SQUARE2_REG+2
N2Tone: bne DecrementSfx2Length

PlayBlast:
        lda #$20                ;load length of fireworks/gunfire sound
        sta Squ2_SfxLenCounter
        ldy #$94                ;load reg contents of fireworks/gunfire sound
        lda #$5e
        bne SBlasJ

ContinueBlast:
        lda Squ2_SfxLenCounter  ;check for time to play second part
        cmp #$18
        bne DecrementSfx2Length
        ldy #$93                ;load second part reg contents then
        lda #$18
SBlasJ: beq +
         jmp BlstSJp            ;unconditional branch to load rest of reg contents
        +

PlayPowerUpGrab:
        lda #$36                    ;load length of power-up grab sound
        sta Squ2_SfxLenCounter

ContinuePowerUpGrab:   
        lda Squ2_SfxLenCounter      ;load frequency reg based on length left over
        lsr                         ;divide by 2
        bcs DecrementSfx2Length     ;alter frequency every other frame
        tay
        lda PowerUpGrabFreqData-1,y ;use length left over / 2 for frequency offset
        ldx #$5d                    ;store reg contents of power-up grab sound
        ldy #$7f

LoadSqu2Regs:
        jsr PlaySqu2Sfx

DecrementSfx2Length:
        dec Squ2_SfxLenCounter   ;decrement length of sfx
        bne ExSfx2

EmptySfx2Buffer:
        ldx #$00                ;initialize square 2's sound effects buffer
        stx Square2SoundBuffer

StopSquare2Sfx:
        ; ldx #$0d                ;stop playing the sfx
        ; stx SND_MASTERCTRL_REG 
        ; ldx #$0f
        ; stx SND_MASTERCTRL_REG
ExSfx2: rts

Square2SfxHandler:
        lda Square2SoundBuffer ;special handling for the 1-up sound to keep it
        and #Sfx_ExtraLife     ;from being interrupted by other sounds on square 2
        bne ContinueExtraLife
        ldy Square2SoundQueue  ;check for sfx in queue
        beq CheckSfx2Buffer
        sty Square2SoundBuffer ;if found, put in buffer and check for the following
        bmi PlayBowserFall     ;bowser fall
        lsr Square2SoundQueue
        bcs PlayCoinGrab       ;coin grab
        lsr Square2SoundQueue
        bcs PlayGrowPowerUp    ;power-up reveal
        lsr Square2SoundQueue
        bcs PlayGrowVine       ;vine grow
        lsr Square2SoundQueue
        bcs PlayBlast          ;fireworks/gunfire
        lsr Square2SoundQueue
        bcc +
          jmp PlayTimerTick    ;timer tick
        +
        lsr Square2SoundQueue
        bcs PlayPowerUpGrab    ;power-up grab
        lsr Square2SoundQueue
        bcs PlayExtraLife      ;1-up

CheckSfx2Buffer:
        lda Square2SoundBuffer   ;check for sfx in buffer
        beq ExS2H                ;if not found, exit sub
        bmi ContinueBowserFall   ;bowser fall
        lsr
        bcs Cont_CGrab_TTick     ;coin grab
        lsr
        bcs ContinueGrowItems    ;power-up reveal
        lsr
        bcs ContinueGrowItems    ;vine grow
        lsr
        bcc +
          jmp ContinueBlast      ;fireworks/gunfire
        +
        lsr
        bcs Cont_CGrab_TTick     ;timer tick
        lsr
        bcs ContinuePowerUpGrab  ;power-up grab
        lsr
        bcs ContinueExtraLife    ;1-up
ExS2H:  rts

Cont_CGrab_TTick:
        jmp ContinueCGrabTTick

JumpToDecLength2:
        jmp DecrementSfx2Length

PlayBowserFall:    
         lda #$38                ;load length of bowser defeat sound
         sta Squ2_SfxLenCounter
         ldy #$c4                ;load contents of reg for bowser defeat sound
         lda #$18
BlstSJp: bne PBFRegs

ContinueBowserFall:
          lda Squ2_SfxLenCounter   ;check for almost near the end
          cmp #$08
          beq +
            jmp DecrementSfx2Length
          +
          ldy #$a4                 ;if so, load the rest of reg contents for bowser defeat sound
          lda #$5a
PBFRegs:  ldx #$9f                 ;the fireworks/gunfire sound shares part of reg contents here
EL_LRegs: beq +
            jmp LoadSqu2Regs         ;this is an unconditional branch outta here
          +

PlayExtraLife:
        lda #$30                  ;load length of 1-up sound
        sta Squ2_SfxLenCounter

ContinueExtraLife:
          lda Squ2_SfxLenCounter   
          ldx #$03                  ;load new tones only every eight frames
DivLLoop: lsr
          bcs JumpToDecLength2      ;if any bits set here, branch to dec the length
          dex
          bne DivLLoop              ;do this until all bits checked, if none set, continue
          tay
          lda ExtraLifeFreqData-1,y ;load our reg contents
          ldx #$82
          ldy #$7f
          bne EL_LRegs              ;unconditional branch

PlayGrowPowerUp:
        lda #$10                ;load length of power-up reveal sound
        bne GrowItemRegs

PlayGrowVine:
        lda #$20                ;load length of vine grow sound

GrowItemRegs:
        sta Squ2_SfxLenCounter   
        lda #$7f                  ;load contents of reg for both sounds directly
        sta SND_SQUARE2_REG+1
        lda #$00                  ;start secondary counter for both sounds
        sta Sfx_SecondaryCounter

ContinueGrowItems:
        inc Sfx_SecondaryCounter  ;increment secondary counter for both sounds
        lda Sfx_SecondaryCounter  ;this sound doesn't decrement the usual counter
        lsr                       ;divide by 2 to get the offset
        tay
        cpy Squ2_SfxLenCounter    ;have we reached the end yet?
        beq StopGrowItems         ;if so, branch to jump, and stop playing sounds
        lda #$9d                  ;load contents of other reg directly
        sta SND_SQUARE2_REG
        lda PUp_VGrow_FreqData,y  ;use secondary counter / 2 as offset for frequency regs
        jmp SetFreq_Squ2
        rts ; TODO check this RTS can be removed

StopGrowItems:
        jmp EmptySfx2Buffer       ;branch to stop playing sounds

;--------------------------------

BrickShatterFreqData:
        .byte $01, $0e, $0e, $0d, $0b, $06, $0c, $0f
        .byte $0a, $09, $03, $0d, $08, $0d, $06, $0c

PlayBrickShatter:
        lda #$20                 ;load length of brick shatter sound
        sta Noise_SfxLenCounter

ContinueBrickShatter:
        lda Noise_SfxLenCounter  
        lsr                         ;divide by 2 and check for bit set to use offset
        bcc DecrementSfx3Length
        tay
        ldx BrickShatterFreqData,y  ;load reg contents of brick shatter sound
        lda BrickShatterEnvData,y

PlayNoiseSfx:
        sta SND_NOISE_REG        ;play the sfx
        stx SND_NOISE_REG+2
        lda #$18
        sta SND_NOISE_REG+3

DecrementSfx3Length:
        dec Noise_SfxLenCounter  ;decrement length of sfx
        bne ExSfx3
        lda #$f0                 ;if done, stop playing the sfx
        sta SND_NOISE_REG
        lda #$00
        sta NoiseSoundBuffer
ExSfx3: rts

NoiseSfxHandler:
        ldy NoiseSoundQueue   ;check for sfx in queue
        beq CheckNoiseBuffer
        sty NoiseSoundBuffer  ;if found, put in buffer
        lsr NoiseSoundQueue
        bcs PlayBrickShatter  ;brick shatter
        lsr NoiseSoundQueue
        bcs PlayBowserFlame   ;bowser flame

CheckNoiseBuffer:
        lda NoiseSoundBuffer      ;check for sfx in buffer
        beq ExNH                  ;if not found, exit sub
        lsr
        bcs ContinueBrickShatter  ;brick shatter
        lsr
        bcs ContinueBowserFlame   ;bowser flame
ExNH:   rts

PlayBowserFlame:
        lda #$40                    ;load length of bowser flame sound
        sta Noise_SfxLenCounter

ContinueBowserFlame:
        lda Noise_SfxLenCounter
        lsr
        tay
        ldx #$0f                    ;load reg contents of bowser flame sound
        lda BowserFlameEnvData-1,y
        bne PlayNoiseSfx            ;unconditional branch here



BowserFlameEnvData:
      .byte $15, $16, $16, $17, $17, $18, $19, $19
      .byte $1a, $1a, $1c, $1d, $1d, $1e, $1e, $1f
      .byte $1f, $1f, $1f, $1e, $1d, $1c, $1e, $1f
      .byte $1f, $1e, $1d, $1c, $1a, $18, $16, $14

BrickShatterEnvData:
      .byte $15, $16, $16, $17, $17, $18, $19, $19
      .byte $1a, $1a, $1c, $1d, $1d, $1e, $1e, $1f

FreqRegLookupTbl:
      .byte $00, $88, $00, $2f, $00, $00
      .byte $02, $a6, $02, $80, $02, $5c, $02, $3a
      .byte $02, $1a, $01, $df, $01, $c4, $01, $ab
      .byte $01, $93, $01, $7c, $01, $67, $01, $53
      .byte $01, $40, $01, $2e, $01, $1d, $01, $0d
      .byte $00, $fe, $00, $ef, $00, $e2, $00, $d5
      .byte $00, $c9, $00, $be, $00, $b3, $00, $a9
      .byte $00, $a0, $00, $97, $00, $8e, $00, $86
      .byte $00, $77, $00, $7e, $00, $71, $00, $54
      .byte $00, $64, $00, $5f, $00, $59, $00, $50
      .byte $00, $47, $00, $43, $00, $3b, $00, $35
      .byte $00, $2a, $00, $23, $04, $75, $03, $57
      .byte $02, $f9, $02, $cf, $01, $fc, $00, $6a
