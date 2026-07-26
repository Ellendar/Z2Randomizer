import { dotnet } from './_framework/dotnet.js'
import { compile } from './js65/libassembler.js'

const BUNDLE_DOWNLOAD_SIZE = 71 * 1024 * 1024; // used for progress bar - doesn't have to be exact

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

    // This is a custom cache workaround for GitHub Pages arbitrarily
    // sending different ETags for the same files. (It might depend on
    // which CDN handles the request.)
    //     Instead rely on GitHub Pages setting Last - Modified to be the
    // same for all files when the page is updated, including this script.
    const deployTimestampPromise = (async () => {
        try {
            const r = await origFetch(import.meta.url, { cache: "default" });
            return Date.parse(r.headers.get("Last-Modified") || "") || 0;
        } catch {
            return 0;
        }
    })();

    // track cumulative progress across all boot resources
    let completedBytes = 0;
    const inFlight = new Map(); // id -> {loaded,total}
    let started = false;
    let finishTimer;

    // identify runtime/assembly payloads
    const isBootResource = (url, res) => {
        const u = (typeof url === "string" ? url : url.url || "").toLowerCase();

        // Skip hot reload endpoints entirely
        if (u.includes("hotreload")) {
            return false;
        }

        const ext = /\.(wasm|dll|pdb|dat|gz|br|json|blat|bundle)$/.test(u);
        const frameworkPath = u.includes("/_framework/") || u.includes("dotnet.") || u.includes("icudt");
        const ct = res?.headers?.get("content-type") || "";
        const isWasm = ct.includes("application/wasm");
        return ext || frameworkPath || isWasm;
    };

    const updateOverall = () => {
        let loaded = completedBytes;
        for (const r of inFlight.values()) {
            loaded += r.loaded;
        }
        updateProgress(loaded / BUNDLE_DOWNLOAD_SIZE);
    };

    const fetchError = (e) => {
        showError("Network error while loading app. Please reload.");
        throw e;
    };

    globalThis.fetch = async (input, init) => {
        const deployTs = await deployTimestampPromise;
        const url = typeof input === "string" ? input : (input && input.url) || "";
        const absoluteUrl = new URL(url, window.location.href).href;

        var fetchResult;

        if (deployTs !== 0) {
            // check if transferSize is non-zero (there was no local cache)
            const cachedFetchResult = await origFetch(input, { ...init, cache: "force-cache" }).catch(fetchError);
            const perfEntries = performance?.getEntriesByName(absoluteUrl, "resource");
            const lastEntry = perfEntries?.[perfEntries.length - 1];
            const wasNetworkRequest = lastEntry?.transferSize > 0;

            var staleCache = false;
            if (!wasNetworkRequest) {
                // if we have a reference timestamp, ensure the cached copy isn't
                // much older than the deploy timestamp.
                const lm = Date.parse(cachedFetchResult.headers.get("Last-Modified"));
                staleCache = !lm || lm < deployTs - 180_000;
            }

            if (!staleCache) {
                fetchResult = cachedFetchResult;
            }
        }

        if (!fetchResult) {
            fetchResult = await origFetch(input, { ...init, cache: "default" }).catch(fetchError);
        }

        if (!isBootResource(url, fetchResult) || !fetchResult.body || fetchResult.bodyUsed) {
            return fetchResult; // leave non-boot fetches alone
        }

        started = true;
        const contentLength = parseInt(fetchResult.headers.get("Content-Length") || "0", 10);
        const id = Math.random().toString(36).slice(2);
        const reader = fetchResult.body.getReader();

        inFlight.set(id, { loaded: 0, total: contentLength });

        const stream = new ReadableStream({
            async pull(controller) {
                const { done, value } = await reader.read().catch((e) => {
                    showError("Error reading a resource stream. Please reload.");
                    throw e;
                });

                if (done) {
                    const r = inFlight.get(id);

                    if (r) {
                        completedBytes += r.loaded;
                        inFlight.delete(id);
                    }

                    updateOverall();
                    controller.close();
                    return;
                }

                controller.enqueue(value);

                const current = inFlight.get(id);
                if (current) {
                    current.loaded += value.length;
                    inFlight.set(id, current);
                }
                updateOverall();
            },
            cancel(reason) { try { reader.cancel(reason); } catch { } }
        });

        return new Response(stream, {
            headers: fetchResult.headers,
            status: fetchResult.status,
            statusText: fetchResult.statusText
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
