.include "z2r.inc"

;;; Initialization. This must come before all other modules.

.segment "HEADER" :bank $00 :size $0010 :mem $0000 :off $00000
.segment "PRG0"   :bank $00 :size $4000 :mem $8000 :off $00010
.segment "PRG1"   :bank $01 :size $4000 :mem $8000 :off $04010
.segment "PRG2"   :bank $02 :size $4000 :mem $8000 :off $08010
.segment "PRG3"   :bank $03 :size $4000 :mem $8000 :off $0c010
.segment "PRG4"   :bank $04 :size $4000 :mem $8000 :off $10010
.segment "PRG5"   :bank $05 :size $4000 :mem $8000 :off $14010
.segment "PRG6"   :bank $06 :size $4000 :mem $8000 :off $18010
.segment "PRG7"   :bank $07 :size $4000 :mem $c000 :off $1c010
.segment "PRG10"   :bank $10 :size $2000 :mem $8000 :off $20010
.segment "PRG11"   :bank $11 :size $2000 :mem $8000 :off $22010
.segment "PRG12"   :bank $12 :size $2000 :mem $8000 :off $24010
.segment "PRG13"   :bank $13 :size $2000 :mem $8000 :off $26010
.segment "PRG14"   :bank $14 :size $2000 :mem $8000 :off $28010
.segment "PRG15"   :bank $15 :size $2000 :mem $8000 :off $2a010
.segment "PRG16"   :bank $16 :size $2000 :mem $8000 :off $2c010
.segment "PRG17"   :bank $17 :size $2000 :mem $8000 :off $2e010
.segment "PRG18"   :bank $18 :size $2000 :mem $8000 :off $30010
.segment "PRG19"   :bank $19 :size $2000 :mem $8000 :off $32010
.segment "PRG1A"   :bank $1A :size $2000 :mem $8000 :off $34010
.segment "PRG1B"   :bank $1B :size $2000 :mem $8000 :off $36010
.segment "PRG1C"   :bank $1C :size $2000 :mem $8000 :off $38010 ; Using segment C and D for extended sideview data
.segment "PRG1D"   :bank $1D :size $2000 :mem $a000 :off $3a010
.segment "PRG1E"   :bank $1E :size $2000 :mem $c000 :off $3c010
.segment "PRG1F"   :bank $1F :size $2000 :mem $e000 :off $3e010
.segment "CHR"    :size $20000 :off $40010 :out

; Mark unused areas in the ROM so the linker can place stuff here

; FREE "PRG0" [$AA40, $c000)
FREE "PRG0" [$AB00, $c000) ; give room for z2edit to patch $aa40

FREE "PRG1" [$87c6, $88a0)
FREE "PRG1" [$93bb, $9400)
FREE "PRG1" [$9eb9, $a000)
FREE "PRG1" [$a933, $b480) ; $b480 is where the new map data is written for the overworlds

FREE "PRG2" [$87d3, $88a0)
FREE "PRG2" [$93c9, $9400)
FREE "PRG2" [$9f85, $a000)
FREE "PRG2" [$a933, $b480)

FREE "PRG3" [$B803, $c000)

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

; Most of bank 6 is reserved for z2ft
;FREE "PRG6" [$878c, $9000)
;FREE "PRG6" [$9da8, $a000)
;FREE "PRG6" [$ac09, $c000) TEMP
FREE "PRG6" [$ac21, $c000)

; DPCM data, will affect dpcm sfx but not gameplay so its fine to use this as a last ditch
; free space for patches. Keep it disabled as much as possible
; FREE "PRG7" [$f369, $fcfb)
FREE "PRG7" [$f3d0, $fcfb) ; allow code in the ganon laugh sfx but not the hurt sfx

; Currently these are only used as space for music tracks
;FREE "PRG10" [$a000, $c000)
;FREE "PRG11" [$a000, $c000)
;FREE "PRG12" [$a000, $c000)
;FREE "PRG13" [$a000, $c000)
;FREE "PRG14" [$a000, $c000)
;FREE "PRG15" [$a000, $c000)
;FREE "PRG16" [$a000, $c000)
;FREE "PRG17" [$a000, $c000)
;FREE "PRG18" [$a000, $c000)
;FREE "PRG19" [$a000, $c000)
;FREE "PRG1A" [$a000, $c000)
;FREE "PRG1B" [$a000, $c000)
FREE "PRG1C" [$8000, $a000)
FREE "PRG1D" [$a000, $c000)
FREE "PRG1E" [$a000, $c000)

; Most of bank 1f is reserved for z2ft
FREE "PRG1F" [$ff80, $ffe8)
