import { dotnet } from './_framework/dotnet.js'
import { compile } from './js65/libassembler.js'

const BUNDLE_DOWNLOAD_SIZE = 80 * 1024 * 1024; // used for progress bar - doesn't have to be exact

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

function showError(msg) {
    const el = document.getElementById("error-message");
    if (!el) return;
    el.textContent = msg;
    el.style.display = "block"; // unhide
}

// Big code chunk to hook into the loading of the app so we can display progress
(() => {
    const origFetch = globalThis.fetch.bind(globalThis);

    // track cumulative progress across all boot resources
    let loadedKnownBytes = 0;
    const inFlight = new Map(); // id -> {loaded,total}
    let started = false;
    let finishTimer;

    // identify runtime/assembly payloads
    const isBootResource = (url, res) => {
        const u = (typeof url === "string" ? url : url.url || "").toLowerCase();
        const ext = /\.(wasm|dll|pdb|dat|gz|br|json|blat|bundle)$/.test(u);
        const frameworkPath = u.includes("/_framework/") || u.includes("dotnet.") || u.includes("icudt");
        const ct = res?.headers?.get("content-type") || "";
        const isWasm = ct.includes("application/wasm");
        return ext || frameworkPath || isWasm;
    };

    const updateOverall = () => {
        updateProgress(loadedKnownBytes / BUNDLE_DOWNLOAD_SIZE);
    };

    globalThis.fetch = async (input, init) => {
        const res = await origFetch(input, init).catch((e) => {
            showError("Network error while loading app. Please reload.");
            throw e;
        });

        const url = typeof input === "string" ? input : (input && input.url) || "";
        if (!isBootResource(url, res) || !res.body || res.bodyUsed) {
            return res; // leave non-boot fetches alone
        }

        started = true;
        const contentLength = parseInt(res.headers.get("Content-Length") || "0", 10);
        const id = Math.random().toString(36).slice(2);
        const reader = res.body.getReader();

        inFlight.set(id, { loaded: 0, total: contentLength });

        const stream = new ReadableStream({
            async pull(controller) {
                const { done, value } = await reader.read().catch((e) => {
                    showError("Error reading a resource stream. Please reload.");
                    throw e;
                });
                if (done) {
                    controller.close();
                    // account for any rounding misses
                    const r = inFlight.get(id);
                    if (r && r.total > 0) loadedKnownBytes += (r.total - r.loaded);
                    inFlight.delete(id);
                    updateOverall();
                    return;
                }
                controller.enqueue(value);
                const r = inFlight.get(id);
                if (r) {
                    r.loaded += value.length;
                    if (r.total > 0) loadedKnownBytes += value.length;
                    inFlight.set(id, r);
                }
                updateOverall();
            },
            cancel(reason) { try { reader.cancel(reason); } catch { } }
        });

        return new Response(stream, {
            headers: res.headers,
            status: res.status,
            statusText: res.statusText
        });
    };

    addEventListener("error", (e) => showError(`Script error: ${e.message || "unknown"}`));
    addEventListener("unhandledrejection", (e) => showError(`Load error: ${e.reason?.message || "unknown"}`));
})();

function updateProgress(ratio) {
    const percent = Math.round(Math.max(0, Math.min(1, ratio)) * 100);
    const st = document.getElementById("loading-status");
    const fill = document.getElementById("progress-fill");
    if (st) { st.textContent = `Loading… ${percent}%`; }
    if (fill) { fill.style.width = percent + "%"; }
}

window.arrayBufferToBase64 = function (buffer) {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
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
                (buf) => new Object({ "Filename": filename, "Patch": arrayBufferToBase64(buf) })
            );
        })
    ).then((loadedFiles) => {
        return JSON.stringify(loadedFiles)
    });
})();

const PreloadedPalaces = (async function () {
    return fetch("PalaceRooms.json").then((res) => res.text());
})();

window.FetchPreloadedSprites = () => PreloadedSprites;
window.FetchPalaces = () => PreloadedPalaces;

window.DownloadBinaryFile = (data, name) => {
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.style = 'display: none';
    const bindata = base64ToArrayBuffer(data);
    const blob = new Blob([bindata], { type: 'octet/stream' });
    const url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = name;
    a.click();
    window.URL.revokeObjectURL(url);
    a.remove();
};

window.DownloadTextFile = (text, name) => {
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.style = 'display: none';
    const blob = new Blob([text], { type: 'text/plain;charset=utf-8' });
    const url = window.URL.createObjectURL(blob);
    a.href = url;
    a.download = name;
    a.click();
    window.URL.revokeObjectURL(url);
    a.remove();
};

window.SetTitle = (title) => {
    document.title = title;
};

try {
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

    await dotnetRuntime.runMain(config.mainAssemblyName, [window.location.search]);
} catch (err) {
    console.error(err);
    showError("Something went wrong starting the app. Please reload.");
}
