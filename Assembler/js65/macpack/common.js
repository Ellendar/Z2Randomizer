export const text = `
;;; Standard register names
PPUCTRL = $2000
PPUMASK = $2001
PPUSTATUS = $2002
OAMADDR = $2003
OAMDATA = $2004
PPUSCROLL = $2005
PPUADDR = $2006
PPUDATA = $2007
OAMDMA = $4014

JOY1 = $4016
JOY2 = $4017

;;; MMC5 register names and associated constants
PrgBankModeReg = $5100
PRG_BANK_MODE_8KB_BANKS = 3

ChrBankModeReg = $5101
CHR_BANK_MODE_8KB_BANKS = 0
CHR_BANK_MODE_4KB_BANKS = 1
CHR_BANK_MODE_2KB_BANKS = 2
CHR_BANK_MODE_1KB_BANKS = 3

PrgRamProtReg1 = $5102
PRG_RAM_UNPROTECT1_VALUE = 2

PrgRamProtReg2 = $5103
PRG_RAM_UNPROTECT2_VALUE = 1

ExRamModeReg = $5104
EXRAM_EXT_ATTR_MODE = 1
EXRAM_RAM_MODE = 2

NameTableModeReg = $5105
HORIZ_MIRROR_MODE = $50
VERT_MIRROR_MODE = $44

FillModeTileReg = $5106
FillModeColorReg = $5107
PrgRamBankReg = $5113
PrgBank8Reg = $5114
PrgBankAReg = $5115
PrgBankCReg = $5116
PrgBankEReg = $5117
PRG_BANK_ROM = $80

SpChrBank0Reg = $5120
SpChrBank1Reg = $5121
SpChrBank2Reg = $5122
SpChrBank3Reg = $5123
SpChrBank4Reg = $5124
SpChrBank5Reg = $5125
SpChrBank6Reg = $5126
SpChrBank7Reg = $5127
BgChrBank0Reg = $5128
BgChrBank1Reg = $5129
BgChrBank2Reg = $512a
BgChrBank3Reg = $512b

VertSplitMode = $5200
DISABLE_VERT_SPLIT_MODE = 0

LineIrqTgtReg = $5203
LineIrqStatusReg = $5204
DISABLE_SCANLINE_IRQ = 0
ENABLE_SCANLINE_IRQ = $80

MulLoReg = $5205
MulHiReg = $5206

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

`;
