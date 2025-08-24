import { dotnet } from './_framework/dotnet.js'
import { compile } from 'js65/libassembler.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

dotnetRuntime.setModuleImports("js65.js65.js", { compile: compile });

const config = dotnetRuntime.getConfig();

// this.customFetchString = function(url) {
//     return fetch(url).then(
//         async (res) => res.text()
//     );
// }
//
// this.customFetchBinary = function(url) {
//     return fetch(url).then(
//         async (res) => {
//             let string = "";
//             let buffer = await res.arrayBuffer();
//             (new Uint8Array(buffer)).forEach(
//                 (byte) => { string += String.fromCharCode(byte) }
//             )
//             return btoa(string);
//         }
//     );
// }

// const db = new Dexie("FilesystemDatabase");
//
// // imitate a poor man's filesystem with indexeddb.
// // Create a table with a compound index on the path + filename field
// db.version(1).stores({
//     fs: "++id, [path+filename]"
// });
window.arrayBufferToBase64 = function( buffer ) {
    let binary = '';
    const bytes = new Uint8Array( buffer );
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode( bytes[ i ] );
    }
    return window.btoa( binary );
}
window.base64ToArrayBuffer = function (base64) {
    const binaryString = atob(base64);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
}

const PreloadedSprites = (async function () {
    const ipsManifest = await fetch("ips-manifest.txt");
    const text = await ipsManifest.text();
    const filenames = text.split(/\n/).map(line => line.trim()).filter(line => line.length > 0);

    return Promise.all(filenames
            .map(async (filename) => {
                return await fetch("Sprites/" + filename).then(
                    (res) => res.arrayBuffer()
                ).then(
                    (buf) => new Object({"Filename": filename, "Patch": arrayBufferToBase64(buf)})
                );
            })
    ).then((loadedFiles) => {
        return JSON.stringify(loadedFiles)
    });
})();

const PreloadedPalaces = (async function() {
    return fetch("PalaceRooms.json").then((res) => res.text());
})();

window.FetchPreloadedSprites = () => PreloadedSprites;
window.FetchPalaces = () => PreloadedPalaces;

window.DownloadFile = (data, name) => {
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.style = 'display: none';
    const bindata = base64ToArrayBuffer(data);
    const blob = new Blob([bindata], {type: 'octet/stream'}),
        url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = name;
    a.click();
    window.URL.revokeObjectURL(url);
    a.remove();
};

window.SetTitle = (title) => {
    document.title = title;
};

await dotnetRuntime.runMain(config.mainAssemblyName, [window.location.search]);
