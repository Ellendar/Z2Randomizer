
;;; Initialization. This must come before all other modules.

;;; Tag for labels that we expect to override vanilla
.define OVERRIDE

;;; Nicer syntax for declaring free sections
.define FREE {seg [start, end)} \
    .pushseg seg .eol \
    .org start .eol \
    .free end - start .eol \
    .popseg
.define FREE {seg [start, end]} .noexpand FREE seg [start, end + 1)


;;; Relocate a block of code and update refs
;;; Usage:
;;;   RELOCATE segments [start, end) refs...
;;; Where |segments| is an optional comma-separated list of segment
;;; names, and |refs| is a space-separated list of addresses whose
;;; contents point to |start| and that need to be updated to point to
;;; whereever it eventually ended up.  If no segments are specified
;;; then the relocation will stay within the current segment.
.define RELOCATE {seg [start, end) refs .eol} \
.org start .eol \
: FREE_UNTIL end .eol \
.ifnblank seg .eol \
.pushseg seg .eol \
.endif .eol \
.reloc .eol \
: .move (end-start), :-- .eol \
.ifnblank seg .eol \
.popseg .eol \
.endif .eol \
UPDATE_REFS :- @ refs

;;; Update a handful of refs to point to the given address.
;;; Usage:
;;;   UPDATE_REFS target @ refs...
;;; Where |refs| is a space-separated list of addresses, and
;;; |target| is an address or label to insert into each ref.
.define UPDATE_REFS {target @ ref refs .eol} \
.org ref .eol \
  .word (target) .eol \
UPDATE_REFS target @ refs
.define UPDATE_REFS {target @ .eol}


.macro FREE_UNTIL end
  .assert * <= end
  .free end - *
.endmacro


.segment "HEADER" :bank $00 :size $0010 :mem $0000 :off $00000
.segment "PRG0"   :bank $00 :size $4000 :mem $8000 :off $00010
.segment "PRG1"   :bank $01 :size $4000 :mem $8000 :off $04010
.segment "PRG2"   :bank $02 :size $4000 :mem $8000 :off $08010
.segment "PRG3"   :bank $03 :size $4000 :mem $8000 :off $0c010
.segment "PRG4"   :bank $04 :size $4000 :mem $8000 :off $10010
.segment "PRG5"   :bank $05 :size $4000 :mem $8000 :off $14010
.segment "PRG6"   :bank $06 :size $4000 :mem $8000 :off $18010
.segment "PRG7"   :bank $07 :size $4000 :mem $c000 :off $1c010
.segment "CHR"    :size $20000 :off $20010 :out

; Mark unused areas in the ROM so the linker can place stuff here

FREE "PRG0" [$AA40, $c000)

FREE "PRG4" [$83DC, $8470)
FREE "PRG4" [$84f0, $8500)
FREE "PRG4" [$8508, $850C)
FREE "PRG4" [$870E, $871B)
FREE "PRG4" [$8817, $88A0)
FREE "PRG4" [$8EC3, $9400)
FREE "PRG4" [$9EE0, $a000)
FREE "PRG4" [$A1E3, $A1F8)
FREE "PRG4" [$A3FB, $A440)
FREE "PRG4" [$A539, $A640)
FREE "PRG4" [$A765, $A900)
FREE "PRG4" [$BEFD, $BF00)
FREE "PRG4" [$bf60, $c000)

FREE "PRG5" [$834e, $84d0)
FREE "PRG5" [$861f, $871b)
FREE "PRG5" [$8817, $88a0)
FREE "PRG5" [$93ae, $9400)
FREE "PRG5" [$a54f, $a600)
FREE "PRG5" [$bda1, $c000)

FREE "PRG6" [$878c, $9000)
FREE "PRG6" [$9da8, $9fff)
FREE "PRG6" [$ac09, $bfff)

; DPCM data, will affect dpcm sfx but not gameplay so its fine to use this as a last ditch
; free space for patches. Keep it disabled as much as possible
; FREE "PRG7" [$f369, $fcfb);


