import { dotnet } from './_framework/dotnet.js'

const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const dotnetRuntime = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const config = dotnetRuntime.getConfig();

this.customFetchString = function(url) {
    return fetch(url).then(
        async (res) => res.text()
    );
}

this.customFetchBinary = function(url) {
    return fetch(url).then(
        async (res) => {
            let string = "";
            let buffer = await res.arrayBuffer();
            (new Uint8Array(buffer)).forEach(
                (byte) => { string += String.fromCharCode(byte) }
            )
            return btoa(string);
        }
    );
}

await dotnetRuntime.runMain(config.mainAssemblyName, [window.location.search]);
