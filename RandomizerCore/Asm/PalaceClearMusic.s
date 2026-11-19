; Palace clear music fix for custom music (z2ft).
;
; Vanilla gem placement (bank 4) queues silence ($80) and the item chime, then
; queues the palace clear song ($40) about a second later when the refill
; starts, and queues the palace theme ($02) the frame the refill ends. With
; custom music the palace clear song is usually longer than the vanilla
; jingle, so it starts late and is cut off when the refill ends (after about
; half a second when Link's life and magic are nearly full).
;
; This patch starts the song when the gem is placed, does not restart it at
; the refill, and waits for the song to end before resuming the palace theme.

.include "z2r.inc"

; z2ft RAM: nonzero while a z2ft song is playing, zero once a non-looping song ends.
Z2ftSongActive = $683a
; Give up waiting after this many frames (about 10 seconds).
PALACE_CLEAR_RESUME_TIMEOUT = 600
; Frames to wait after the song ends so its last note can decay.
PALACE_CLEAR_TAIL_FRAMES = 30

.segment "PRG4"

; lda #$08 / sta $ef / lda #$80 / sta $eb at $9B12: start the song instead of silence
.org $9b16
    lda #$40

; lda #$40 / sta $eb at $9B3A: the song is already playing, so don't restart it
.org $9b3c
    nop
    nop

; ldx #$02 / stx $eb at $9B7E: resume the palace theme only after the song ends.
; The next instructions (inx / jsr $c295) still need X = 2.
.org $9b7e
    jsr PalaceClearDeferResume
    nop

.segment "PRG7"

.reloc
PalaceClearDeferResume:
    ldx #$02
    pha
    lda Z2ftSongActive
    beq @resume_now
        stx PalaceClearResumePending
        lda MapNumber
        sta PalaceClearResumeMap
        lda #0
        sta PalaceClearResumeTimer
        sta PalaceClearResumeTimer+1
        sta PalaceClearTailTimer
        pla
        rts
@resume_now:
    stx $eb
    pla
    rts

; Called once per sound update, after z2ft's UpdateSound.
.export PalaceClearMusicTick
.reloc
PalaceClearMusicTick:
    lda PalaceClearResumePending
    beq @done
    ; Link left the room: the new area picks its own music
    lda MapNumber
    cmp PalaceClearResumeMap
    bne @clear
    inc PalaceClearResumeTimer
    bne +
        inc PalaceClearResumeTimer+1
+
    lda PalaceClearResumeTimer+1
    cmp #>PALACE_CLEAR_RESUME_TIMEOUT
    bcc @check_song
    lda PalaceClearResumeTimer
    cmp #<PALACE_CLEAR_RESUME_TIMEOUT
    bcs @resume
@check_song:
    lda Z2ftSongActive
    beq @song_ended
    ; still playing: restart the tail delay
    lda #0
    sta PalaceClearTailTimer
    rts
@song_ended:
    ; the flag clears while the last note is still decaying, so wait a bit longer
    inc PalaceClearTailTimer
    lda PalaceClearTailTimer
    cmp #PALACE_CLEAR_TAIL_FRAMES
    bcc @done
@resume:
    lda #$02
    sta $eb
@clear:
    lda #0
    sta PalaceClearResumePending
@done:
    rts
