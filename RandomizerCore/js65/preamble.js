// If the user doesn't provide any segment or header information
// this provides a barebones NES compile target
export const Sim = {
    segments: [{
            name: 'code',
            default: true,
            offset: 0x00,
            size: 0xfd00,
            memory: 0x200,
            free: [[0x0200, 0xfd00]],
        }],
};
export const NesNrom = {
    segments: [{
            name: 'header',
            size: 0x10,
            offset: 0x00,
            memory: 0x00,
        }, {
            name: 'code',
            default: true,
            size: 0x8000,
            offset: 0x00010,
            memory: 0x8000,
            free: [[0x8000, 0x10000]],
        }, {
            name: 'chr',
            size: 0x2000,
            offset: 0x08010,
            memory: 0x00,
        }]
};
export const Targets = new Map([
    ['sim', Sim],
    ['nes-nrom', NesNrom],
]);
