import defaultErrorMap from "./locales/en.js";
import type { ZodErrorMap } from "./ZodError.js";
export { defaultErrorMap };
export declare function setErrorMap(map: ZodErrorMap): void;
export declare function getErrorMap(): ZodErrorMap;
