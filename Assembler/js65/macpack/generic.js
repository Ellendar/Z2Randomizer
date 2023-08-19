export const text = `
; Original from cc65
; This software is provided 'as-is', without any express or implied warranty.
; In no event will the authors be held liable for any damages arising from
; the use of this software.

; Permission is granted to anyone to use this software for any purpose,
; including commercial applications, and to alter it and redistribute it
; freely, subject to the following restrictions:

; 1. The origin of this software must not be misrepresented; you must not
; claim that you wrote the original software. If you use this software in
; a product, an acknowledgment in the product documentation would be
; appreciated but is not required.

; 2. Altered source versions must be plainly marked as such, and must not
; be misrepresented as being the original software.

; 3. This notice may not be removed or altered from any source distribution.


; add - Add without carry
.macro  add     Arg1, Arg2
        clc
        .if .paramcount = 2
                adc     Arg1, Arg2
        .else
                adc     Arg1
        .endif
.endmacro

; sub - subtract without borrow
.macro  sub     Arg1, Arg2
        sec
        .if .paramcount = 2
                sbc     Arg1, Arg2
        .else
                sbc     Arg1
        .endif
.endmacro

; bge - jump if unsigned greater or equal
.macro  bge     Arg
        bcs     Arg
.endmacro

; blt - Jump if unsigned less
.macro  blt     Arg
        bcc     Arg
.endmacro

; bgt - jump if unsigned greater
.macro  bgt     Arg
        .local  L
        beq     L
        bcs     Arg
L:
.endmacro

; ble - jump if unsigned less or equal
.macro  ble     Arg
        beq     Arg
        bcc     Arg
.endmacro

; bnz - jump if not zero
.macro  bnz     Arg
        bne     Arg
.endmacro

; bze - jump if zero
.macro  bze     Arg
        beq     Arg
.endmacro

`;
