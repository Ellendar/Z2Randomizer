﻿
using System.Collections.Generic;

namespace Z2Randomizer.Core.Sidescroll;

/// <summary>
/// Color combinations that may be chosen for palace bricks and
/// curtains.  Each triad is the shadow color, the main color,
/// and the highlight color in that order.  For background
/// bricks, the main color will be used.
///
/// See https://www.nesdev.org/wiki/PPU_palettes#Color_names.
/// </summary>
static class PalaceColors
{
    public static readonly int Gray    = 0x0;
    public static readonly int Azure   = 0x1;
    public static readonly int Blue    = 0x2;
    public static readonly int Violet  = 0x3;
    public static readonly int Magenta = 0x4;
    public static readonly int Rose    = 0x5;
    public static readonly int Red     = 0x6;
    public static readonly int Orange  = 0x7;
    public static readonly int Yellow  = 0x8; // green/olive-ish at dark levels
    public static readonly int Forest  = 0x9; // chartreuse per URL above
    public static readonly int Green   = 0xA;
    public static readonly int Spring  = 0xB;
    public static readonly int Cyan    = 0xC;
    public static readonly int Black   = 0xF;

    public static readonly int Darker  = 0x00;
    public static readonly int Dark    = 0x10;
    public static readonly int Light   = 0x20;
    public static readonly int Lighter = 0x30;

    public static readonly int[,] bricks = {
        { Gray            , Dark|Gray       , Lighter|Gray     },
        { Azure           , Gray            , Lighter|Gray     },
        { Azure           , Gray            , Dark|Gray        },
        { Azure           , Dark|Azure      , Lighter|Azure    },
        { Azure           , Dark|Azure      , Light|Azure      },
        { Azure           , Dark|Blue       , Lighter|Violet   },
        { Azure           , Dark|Blue       , Light|Violet     },
        { Azure           , Dark|Violet     , Light|Rose       },
        { Azure           , Dark|Orange     , Lighter|Orange   },
        { Azure           , Dark|Green      , Lighter|Orange   },
        { Azure           , Dark|Spring     , Light|Forest     },
        { Azure           , Dark|Cyan       , Lighter|Spring   },
        { Azure           , Dark|Cyan       , Light|Spring     },
        { Azure           , Light|Azure     , Lighter|Azure    },
        { Azure           , Light|Blue      , Lighter|Violet   },
        { Azure           , Light|Cyan      , Lighter|Spring   },
        { Blue            , Gray            , Lighter|Gray     },
        { Blue            , Gray            , Dark|Gray        },
        { Blue            , Dark|Azure      , Lighter|Cyan     },
        { Blue            , Dark|Azure      , Light|Cyan       },
        { Blue            , Dark|Blue       , Lighter|Blue     },
        { Blue            , Dark|Blue       , Light|Blue       },
        { Blue            , Dark|Violet     , Lighter|Magenta  },
        { Blue            , Dark|Violet     , Light|Magenta    },
        { Blue            , Dark|Magenta    , Light|Red        },
        { Blue            , Dark|Orange     , Lighter|Orange   },
        { Blue            , Dark|Green      , Lighter|Orange   },
        { Blue            , Dark|Cyan       , Light|Green      },
        { Blue            , Light|Azure     , Lighter|Cyan     },
        { Blue            , Light|Blue      , Lighter|Blue     },
        { Blue            , Light|Violet    , Lighter|Magenta  },
        { Violet          , Gray            , Lighter|Gray     },
        { Violet          , Gray            , Dark|Gray        },
        { Violet          , Dark|Azure      , Light|Spring     },
        { Violet          , Dark|Blue       , Lighter|Azure    },
        { Violet          , Dark|Blue       , Light|Azure      },
        { Violet          , Dark|Violet     , Lighter|Violet   },
        { Violet          , Dark|Violet     , Light|Violet     },
        { Violet          , Dark|Magenta    , Lighter|Rose     },
        { Violet          , Dark|Magenta    , Light|Rose       },
        { Violet          , Dark|Rose       , Light|Orange     },
        { Violet          , Dark|Orange     , Lighter|Orange   },
        { Violet          , Dark|Green      , Lighter|Orange   },
        { Violet          , Light|Blue      , Lighter|Azure    },
        { Violet          , Light|Violet    , Lighter|Violet   },
        { Violet          , Light|Magenta   , Lighter|Rose     },
        { Magenta         , Gray            , Lighter|Gray     },
        { Magenta         , Gray            , Dark|Gray        },
        { Magenta         , Dark|Blue       , Light|Cyan       },
        { Magenta         , Dark|Violet     , Lighter|Blue     },
        { Magenta         , Dark|Violet     , Light|Blue       },
        { Magenta         , Dark|Magenta    , Lighter|Magenta  },
        { Magenta         , Dark|Magenta    , Light|Magenta    },
        { Magenta         , Dark|Rose       , Lighter|Red      },
        { Magenta         , Dark|Rose       , Light|Red        },
        { Magenta         , Dark|Red        , Light|Yellow     },
        { Magenta         , Dark|Orange     , Lighter|Orange   },
        { Magenta         , Dark|Green      , Lighter|Orange   },
        { Magenta         , Light|Violet    , Lighter|Blue     },
        { Magenta         , Light|Magenta   , Lighter|Magenta  },
        { Magenta         , Light|Rose      , Lighter|Red      },
        { Rose            , Gray            , Lighter|Gray     },
        { Rose            , Gray            , Dark|Gray        },
        { Rose            , Dark|Violet     , Light|Azure      },
        { Rose            , Dark|Magenta    , Lighter|Violet   },
        { Rose            , Dark|Magenta    , Light|Violet     },
        { Rose            , Dark|Rose       , Lighter|Rose     },
        { Rose            , Dark|Rose       , Light|Rose       },
        { Rose            , Dark|Red        , Lighter|Orange   },
        { Rose            , Dark|Red        , Light|Orange     },
        { Rose            , Dark|Orange     , Lighter|Orange   },
        { Rose            , Dark|Orange     , Light|Forest     },
        { Rose            , Dark|Green      , Lighter|Orange   },
        { Rose            , Light|Magenta   , Lighter|Violet   },
        { Rose            , Light|Rose      , Lighter|Rose     },
        { Rose            , Light|Red       , Lighter|Orange   },
        { Red             , Gray            , Lighter|Gray     },
        { Red             , Gray            , Dark|Gray        },
        { Red             , Dark|Magenta    , Light|Blue       },
        { Red             , Dark|Rose       , Lighter|Magenta  },
        { Red             , Dark|Rose       , Light|Magenta    },
        { Red             , Dark|Red        , Lighter|Red      },
        { Red             , Dark|Red        , Light|Red        },
        { Red             , Dark|Orange     , Lighter|Yellow   },
        { Red             , Dark|Orange     , Lighter|Orange   },
        { Red             , Dark|Orange     , Light|Yellow     },
        { Red             , Dark|Yellow     , Light|Green      },
        { Red             , Dark|Green      , Lighter|Orange   },
        { Red             , Light|Rose      , Lighter|Magenta  },
        { Red             , Light|Red       , Lighter|Red      },
        { Red             , Light|Orange    , Lighter|Yellow   },
        { Orange          , Gray            , Lighter|Gray     },
        { Orange          , Gray            , Dark|Gray        },
        { Orange          , Dark|Rose       , Light|Violet     },
        { Orange          , Dark|Red        , Lighter|Rose     },
        { Orange          , Dark|Red        , Light|Rose       },
        { Orange          , Dark|Orange     , Lighter|Orange   },
        { Orange          , Dark|Orange     , Light|Orange     },
        { Orange          , Dark|Yellow     , Lighter|Forest   },
        { Orange          , Dark|Yellow     , Light|Forest     },
        { Orange          , Dark|Forest     , Light|Spring     },
        { Orange          , Dark|Green      , Lighter|Orange   },
        { Orange          , Light|Red       , Lighter|Rose     },
        { Orange          , Light|Orange    , Lighter|Orange   },
        { Orange          , Light|Yellow    , Lighter|Forest   },
        { Yellow          , Gray            , Lighter|Gray     },
        { Yellow          , Gray            , Dark|Gray        },
        { Yellow          , Dark|Red        , Light|Magenta    },
        { Yellow          , Dark|Orange     , Lighter|Orange   },
        { Yellow          , Dark|Orange     , Lighter|Red      },
        { Yellow          , Dark|Orange     , Light|Red        },
        { Yellow          , Dark|Yellow     , Lighter|Yellow   },
        { Yellow          , Dark|Yellow     , Light|Yellow     },
        { Yellow          , Dark|Forest     , Lighter|Green    },
        { Yellow          , Dark|Forest     , Light|Green      },
        { Yellow          , Dark|Green      , Lighter|Orange   },
        { Yellow          , Dark|Green      , Light|Cyan       },
        { Yellow          , Light|Orange    , Lighter|Red      },
        { Yellow          , Light|Yellow    , Lighter|Yellow   },
        { Yellow          , Light|Forest    , Lighter|Green    },
        { Forest          , Gray            , Lighter|Gray     },
        { Forest          , Gray            , Dark|Gray        },
        { Forest          , Dark|Orange     , Lighter|Orange   },
        { Forest          , Dark|Orange     , Light|Rose       },
        { Forest          , Dark|Yellow     , Lighter|Orange   },
        { Forest          , Dark|Yellow     , Light|Orange     },
        { Forest          , Dark|Forest     , Lighter|Forest   },
        { Forest          , Dark|Forest     , Light|Forest     },
        { Forest          , Dark|Green      , Lighter|Spring   },
        { Forest          , Dark|Green      , Lighter|Orange   },
        { Forest          , Dark|Green      , Light|Spring     },
        { Forest          , Dark|Spring     , Light|Azure      },
        { Forest          , Light|Yellow    , Lighter|Orange   },
        { Forest          , Light|Forest    , Lighter|Forest   },
        { Forest          , Light|Green     , Lighter|Spring   },
        { Green           , Gray            , Lighter|Gray     },
        { Green           , Gray            , Dark|Gray        },
        { Green           , Dark|Orange     , Lighter|Orange   },
        { Green           , Dark|Yellow     , Light|Red        },
        { Green           , Dark|Forest     , Lighter|Yellow   },
        { Green           , Dark|Forest     , Light|Yellow     },
        { Green           , Dark|Green      , Lighter|Green    },
        { Green           , Dark|Green      , Lighter|Orange   },
        { Green           , Dark|Green      , Light|Green      },
        { Green           , Dark|Spring     , Lighter|Cyan     },
        { Green           , Dark|Spring     , Light|Cyan       },
        { Green           , Dark|Cyan       , Light|Blue       },
        { Green           , Light|Forest    , Lighter|Yellow   },
        { Green           , Light|Green     , Lighter|Green    },
        { Green           , Light|Spring    , Lighter|Cyan     },
        { Spring          , Gray            , Lighter|Gray     },
        { Spring          , Gray            , Dark|Gray        },
        { Spring          , Dark|Azure      , Light|Violet     },
        { Spring          , Dark|Orange     , Lighter|Orange   },
        { Spring          , Dark|Forest     , Light|Orange     },
        { Spring          , Dark|Green      , Lighter|Forest   },
        { Spring          , Dark|Green      , Lighter|Orange   },
        { Spring          , Dark|Green      , Light|Forest     },
        { Spring          , Dark|Spring     , Lighter|Spring   },
        { Spring          , Dark|Spring     , Light|Spring     },
        { Spring          , Dark|Cyan       , Lighter|Azure    },
        { Spring          , Dark|Cyan       , Light|Azure      },
        { Spring          , Light|Green     , Lighter|Forest   },
        { Spring          , Light|Spring    , Lighter|Spring   },
        { Spring          , Light|Cyan      , Lighter|Azure    },
        { Cyan            , Gray            , Lighter|Gray     },
        { Cyan            , Gray            , Dark|Gray        },
        { Cyan            , Dark|Azure      , Lighter|Blue     },
        { Cyan            , Dark|Azure      , Light|Blue       },
        { Cyan            , Dark|Blue       , Light|Magenta    },
        { Cyan            , Dark|Orange     , Lighter|Orange   },
        { Cyan            , Dark|Green      , Lighter|Orange   },
        { Cyan            , Dark|Green      , Light|Yellow     },
        { Cyan            , Dark|Spring     , Lighter|Green    },
        { Cyan            , Dark|Spring     , Light|Green      },
        { Cyan            , Dark|Cyan       , Lighter|Cyan     },
        { Cyan            , Dark|Cyan       , Light|Cyan       },
        { Cyan            , Light|Azure     , Lighter|Blue     },
        { Cyan            , Light|Spring    , Lighter|Green    },
        { Cyan            , Light|Cyan      , Lighter|Cyan     },
        { Black           , Gray            , Dark|Gray        },
        { Black           , Azure           , Dark|Cyan        },
        { Black           , Azure           , Dark|Blue        },
        { Black           , Azure           , Dark|Azure       },
        { Black           , Azure           , Gray             },
        { Black           , Blue            , Dark|Violet      },
        { Black           , Blue            , Dark|Blue        },
        { Black           , Blue            , Dark|Azure       },
        { Black           , Blue            , Gray             },
        { Black           , Violet          , Dark|Magenta     },
        { Black           , Violet          , Dark|Violet      },
        { Black           , Violet          , Dark|Blue        },
        { Black           , Violet          , Gray             },
        { Black           , Magenta         , Dark|Rose        },
        { Black           , Magenta         , Dark|Magenta     },
        { Black           , Magenta         , Dark|Violet      },
        { Black           , Magenta         , Gray             },
        { Black           , Rose            , Dark|Red         },
        { Black           , Rose            , Dark|Rose        },
        { Black           , Rose            , Dark|Magenta     },
        { Black           , Rose            , Gray             },
        { Black           , Red             , Dark|Orange      },
        { Black           , Red             , Dark|Red         },
        { Black           , Red             , Dark|Rose        },
        { Black           , Red             , Gray             },
        { Black           , Orange          , Dark|Yellow      },
        { Black           , Orange          , Dark|Orange      },
        { Black           , Orange          , Dark|Red         },
        { Black           , Orange          , Gray             },
        { Black           , Yellow          , Dark|Forest      },
        { Black           , Yellow          , Dark|Yellow      },
        { Black           , Yellow          , Dark|Orange      },
        { Black           , Yellow          , Gray             },
        { Black           , Forest          , Dark|Green       },
        { Black           , Forest          , Dark|Forest      },
        { Black           , Forest          , Dark|Yellow      },
        { Black           , Forest          , Gray             },
        { Black           , Green           , Dark|Spring      },
        { Black           , Green           , Dark|Green       },
        { Black           , Green           , Dark|Forest      },
        { Black           , Green           , Gray             },
        { Black           , Spring          , Dark|Cyan        },
        { Black           , Spring          , Dark|Spring      },
        { Black           , Spring          , Dark|Green       },
        { Black           , Spring          , Gray             },
        { Black           , Cyan            , Dark|Cyan        },
        { Black           , Cyan            , Dark|Spring      },
        { Black           , Cyan            , Dark|Azure       },
        { Black           , Cyan            , Gray             },
    };

    public static readonly int[,] curtains = {
        { Gray            , Azure           , Black            },
        { Gray            , Blue            , Black            },
        { Gray            , Violet          , Black            },
        { Gray            , Magenta         , Black            },
        { Gray            , Rose            , Black            },
        { Gray            , Red             , Black            },
        { Gray            , Orange          , Black            },
        { Gray            , Yellow          , Black            },
        { Gray            , Forest          , Black            },
        { Gray            , Green           , Black            },
        { Gray            , Spring          , Black            },
        { Gray            , Cyan            , Black            },
        { Dark|Gray       , Gray            , Black            },
        { Dark|Gray       , Gray            , Cyan             },
        { Dark|Gray       , Gray            , Spring           },
        { Dark|Gray       , Gray            , Green            },
        { Dark|Gray       , Gray            , Forest           },
        { Dark|Gray       , Gray            , Yellow           },
        { Dark|Gray       , Gray            , Orange           },
        { Dark|Gray       , Gray            , Red              },
        { Dark|Gray       , Gray            , Rose             },
        { Dark|Gray       , Gray            , Magenta          },
        { Dark|Gray       , Gray            , Violet           },
        { Dark|Gray       , Gray            , Blue             },
        { Dark|Gray       , Gray            , Azure            },
        { Dark|Gray       , Dark|Azure      , Black            },
        { Dark|Gray       , Dark|Blue       , Black            },
        { Dark|Gray       , Dark|Violet     , Black            },
        { Dark|Gray       , Dark|Magenta    , Black            },
        { Dark|Gray       , Dark|Rose       , Black            },
        { Dark|Gray       , Dark|Red        , Black            },
        { Dark|Gray       , Dark|Orange     , Black            },
        { Dark|Gray       , Dark|Yellow     , Black            },
        { Dark|Gray       , Dark|Forest     , Black            },
        { Dark|Gray       , Dark|Green      , Black            },
        { Dark|Gray       , Dark|Spring     , Black            },
        { Dark|Gray       , Dark|Cyan       , Black            },
        { Dark|Azure      , Azure           , Black            },
        { Dark|Azure      , Blue            , Black            },
        { Dark|Azure      , Cyan            , Black            },
        { Dark|Blue       , Azure           , Black            },
        { Dark|Blue       , Blue            , Black            },
        { Dark|Blue       , Violet          , Black            },
        { Dark|Violet     , Blue            , Black            },
        { Dark|Violet     , Violet          , Black            },
        { Dark|Violet     , Magenta         , Black            },
        { Dark|Magenta    , Violet          , Black            },
        { Dark|Magenta    , Magenta         , Black            },
        { Dark|Magenta    , Rose            , Black            },
        { Dark|Rose       , Magenta         , Black            },
        { Dark|Rose       , Rose            , Black            },
        { Dark|Rose       , Red             , Black            },
        { Dark|Red        , Rose            , Black            },
        { Dark|Red        , Red             , Black            },
        { Dark|Red        , Orange          , Black            },
        { Dark|Orange     , Red             , Black            },
        { Dark|Orange     , Orange          , Black            },
        { Dark|Orange     , Yellow          , Black            },
        { Dark|Yellow     , Orange          , Black            },
        { Dark|Yellow     , Yellow          , Black            },
        { Dark|Yellow     , Forest          , Black            },
        { Dark|Forest     , Yellow          , Black            },
        { Dark|Forest     , Forest          , Black            },
        { Dark|Forest     , Green           , Black            },
        { Dark|Green      , Forest          , Black            },
        { Dark|Green      , Green           , Black            },
        { Dark|Green      , Spring          , Black            },
        { Dark|Spring     , Green           , Black            },
        { Dark|Spring     , Spring          , Black            },
        { Dark|Spring     , Cyan            , Black            },
        { Dark|Cyan       , Azure           , Black            },
        { Dark|Cyan       , Spring          , Black            },
        { Dark|Cyan       , Cyan            , Black            },
        { Light|Azure     , Dark|Azure      , Azure            },
        { Light|Azure     , Dark|Blue       , Violet           },
        { Light|Azure     , Dark|Violet     , Rose             },
        { Light|Azure     , Dark|Spring     , Forest           },
        { Light|Azure     , Dark|Cyan       , Spring           },
        { Light|Blue      , Dark|Azure      , Cyan             },
        { Light|Blue      , Dark|Blue       , Blue             },
        { Light|Blue      , Dark|Violet     , Magenta          },
        { Light|Blue      , Dark|Magenta    , Red              },
        { Light|Blue      , Dark|Cyan       , Green            },
        { Light|Violet    , Dark|Azure      , Spring           },
        { Light|Violet    , Dark|Blue       , Azure            },
        { Light|Violet    , Dark|Violet     , Violet           },
        { Light|Violet    , Dark|Magenta    , Rose             },
        { Light|Violet    , Dark|Rose       , Orange           },
        { Light|Magenta   , Dark|Blue       , Cyan             },
        { Light|Magenta   , Dark|Violet     , Blue             },
        { Light|Magenta   , Dark|Magenta    , Magenta          },
        { Light|Magenta   , Dark|Rose       , Red              },
        { Light|Magenta   , Dark|Red        , Yellow           },
        { Light|Rose      , Dark|Violet     , Azure            },
        { Light|Rose      , Dark|Magenta    , Violet           },
        { Light|Rose      , Dark|Rose       , Rose             },
        { Light|Rose      , Dark|Red        , Orange           },
        { Light|Rose      , Dark|Orange     , Forest           },
        { Light|Red       , Dark|Magenta    , Blue             },
        { Light|Red       , Dark|Rose       , Magenta          },
        { Light|Red       , Dark|Red        , Red              },
        { Light|Red       , Dark|Orange     , Yellow           },
        { Light|Red       , Dark|Yellow     , Green            },
        { Light|Orange    , Dark|Rose       , Violet           },
        { Light|Orange    , Dark|Red        , Rose             },
        { Light|Orange    , Dark|Orange     , Orange           },
        { Light|Orange    , Dark|Yellow     , Forest           },
        { Light|Orange    , Dark|Forest     , Spring           },
        { Light|Yellow    , Dark|Red        , Magenta          },
        { Light|Yellow    , Dark|Orange     , Red              },
        { Light|Yellow    , Dark|Yellow     , Yellow           },
        { Light|Yellow    , Dark|Forest     , Green            },
        { Light|Yellow    , Dark|Green      , Cyan             },
        { Light|Forest    , Dark|Orange     , Rose             },
        { Light|Forest    , Dark|Yellow     , Orange           },
        { Light|Forest    , Dark|Forest     , Forest           },
        { Light|Forest    , Dark|Green      , Spring           },
        { Light|Forest    , Dark|Spring     , Azure            },
        { Light|Green     , Dark|Yellow     , Red              },
        { Light|Green     , Dark|Forest     , Yellow           },
        { Light|Green     , Dark|Green      , Green            },
        { Light|Green     , Dark|Spring     , Cyan             },
        { Light|Green     , Dark|Cyan       , Blue             },
        { Light|Spring    , Dark|Azure      , Violet           },
        { Light|Spring    , Dark|Forest     , Orange           },
        { Light|Spring    , Dark|Green      , Forest           },
        { Light|Spring    , Dark|Spring     , Spring           },
        { Light|Spring    , Dark|Cyan       , Azure            },
        { Light|Cyan      , Dark|Azure      , Blue             },
        { Light|Cyan      , Dark|Blue       , Magenta          },
        { Light|Cyan      , Dark|Green      , Yellow           },
        { Light|Cyan      , Dark|Spring     , Green            },
        { Light|Cyan      , Dark|Cyan       , Cyan             },
        { Lighter|Gray    , Gray            , Cyan             },
        { Lighter|Gray    , Gray            , Spring           },
        { Lighter|Gray    , Gray            , Green            },
        { Lighter|Gray    , Gray            , Forest           },
        { Lighter|Gray    , Gray            , Yellow           },
        { Lighter|Gray    , Gray            , Orange           },
        { Lighter|Gray    , Gray            , Red              },
        { Lighter|Gray    , Gray            , Rose             },
        { Lighter|Gray    , Gray            , Magenta          },
        { Lighter|Gray    , Gray            , Violet           },
        { Lighter|Gray    , Gray            , Blue             },
        { Lighter|Gray    , Gray            , Azure            },
        { Lighter|Gray    , Dark|Gray       , Gray             },
        { Lighter|Gray    , Light|Azure     , Dark|Azure       },
        { Lighter|Gray    , Light|Blue      , Dark|Blue        },
        { Lighter|Gray    , Light|Violet    , Dark|Violet      },
        { Lighter|Gray    , Light|Magenta   , Dark|Magenta     },
        { Lighter|Gray    , Light|Rose      , Dark|Rose        },
        { Lighter|Gray    , Light|Red       , Dark|Red         },
        { Lighter|Gray    , Light|Orange    , Dark|Orange      },
        { Lighter|Gray    , Light|Yellow    , Dark|Yellow      },
        { Lighter|Gray    , Light|Forest    , Dark|Forest      },
        { Lighter|Gray    , Light|Green     , Dark|Green       },
        { Lighter|Gray    , Light|Spring    , Dark|Spring      },
        { Lighter|Gray    , Light|Cyan      , Dark|Cyan        },
        { Lighter|Azure   , Dark|Azure      , Azure            },
        { Lighter|Azure   , Light|Blue      , Dark|Violet      },
        { Lighter|Azure   , Light|Cyan      , Dark|Spring      },
        { Lighter|Azure   , Light|Cyan      , Spring           },
        { Lighter|Blue    , Dark|Blue       , Blue             },
        { Lighter|Blue    , Light|Azure     , Dark|Cyan        },
        { Lighter|Blue    , Light|Azure     , Cyan             },
        { Lighter|Blue    , Light|Violet    , Dark|Magenta     },
        { Lighter|Violet  , Dark|Violet     , Violet           },
        { Lighter|Violet  , Light|Blue      , Dark|Azure       },
        { Lighter|Violet  , Light|Blue      , Azure            },
        { Lighter|Violet  , Light|Magenta   , Dark|Rose        },
        { Lighter|Magenta , Dark|Magenta    , Magenta          },
        { Lighter|Magenta , Light|Violet    , Dark|Blue        },
        { Lighter|Magenta , Light|Violet    , Blue             },
        { Lighter|Magenta , Light|Rose      , Dark|Red         },
        { Lighter|Rose    , Dark|Rose       , Rose             },
        { Lighter|Rose    , Light|Magenta   , Dark|Violet      },
        { Lighter|Rose    , Light|Magenta   , Violet           },
        { Lighter|Rose    , Light|Red       , Dark|Orange      },
        { Lighter|Red     , Dark|Red        , Red              },
        { Lighter|Red     , Light|Rose      , Dark|Magenta     },
        { Lighter|Red     , Light|Rose      , Magenta          },
        { Lighter|Red     , Light|Orange    , Dark|Yellow      },
        { Lighter|Orange  , Dark|Azure      , Cyan             },
        { Lighter|Orange  , Dark|Blue       , Cyan             },
        { Lighter|Orange  , Dark|Violet     , Cyan             },
        { Lighter|Orange  , Dark|Magenta    , Cyan             },
        { Lighter|Orange  , Dark|Rose       , Cyan             },
        { Lighter|Orange  , Dark|Red        , Cyan             },
        { Lighter|Orange  , Dark|Orange     , Cyan             },
        { Lighter|Orange  , Dark|Orange     , Spring           },
        { Lighter|Orange  , Dark|Orange     , Green            },
        { Lighter|Orange  , Dark|Orange     , Forest           },
        { Lighter|Orange  , Dark|Orange     , Yellow           },
        { Lighter|Orange  , Dark|Orange     , Orange           },
        { Lighter|Orange  , Dark|Orange     , Red              },
        { Lighter|Orange  , Dark|Orange     , Rose             },
        { Lighter|Orange  , Dark|Orange     , Magenta          },
        { Lighter|Orange  , Dark|Orange     , Violet           },
        { Lighter|Orange  , Dark|Orange     , Blue             },
        { Lighter|Orange  , Dark|Orange     , Azure            },
        { Lighter|Orange  , Dark|Yellow     , Cyan             },
        { Lighter|Orange  , Dark|Yellow     , Magenta          },
        { Lighter|Orange  , Dark|Forest     , Cyan             },
        { Lighter|Orange  , Dark|Green      , Cyan             },
        { Lighter|Orange  , Dark|Green      , Spring           },
        { Lighter|Orange  , Dark|Green      , Green            },
        { Lighter|Orange  , Dark|Green      , Forest           },
        { Lighter|Orange  , Dark|Green      , Yellow           },
        { Lighter|Orange  , Dark|Green      , Orange           },
        { Lighter|Orange  , Dark|Green      , Red              },
        { Lighter|Orange  , Dark|Green      , Rose             },
        { Lighter|Orange  , Dark|Green      , Magenta          },
        { Lighter|Orange  , Dark|Green      , Violet           },
        { Lighter|Orange  , Dark|Green      , Blue             },
        { Lighter|Orange  , Dark|Green      , Azure            },
        { Lighter|Orange  , Dark|Spring     , Cyan             },
        { Lighter|Orange  , Dark|Cyan       , Cyan             },
        { Lighter|Orange  , Light|Red       , Dark|Rose        },
        { Lighter|Orange  , Light|Red       , Rose             },
        { Lighter|Orange  , Light|Yellow    , Dark|Forest      },
        { Lighter|Yellow  , Dark|Yellow     , Yellow           },
        { Lighter|Yellow  , Light|Orange    , Dark|Red         },
        { Lighter|Yellow  , Light|Orange    , Red              },
        { Lighter|Yellow  , Light|Forest    , Dark|Green       },
        { Lighter|Forest  , Dark|Forest     , Forest           },
        { Lighter|Forest  , Light|Yellow    , Dark|Orange      },
        { Lighter|Forest  , Light|Yellow    , Orange           },
        { Lighter|Forest  , Light|Green     , Dark|Spring      },
        { Lighter|Green   , Dark|Green      , Green            },
        { Lighter|Green   , Light|Forest    , Dark|Yellow      },
        { Lighter|Green   , Light|Forest    , Yellow           },
        { Lighter|Green   , Light|Spring    , Dark|Cyan        },
        { Lighter|Spring  , Dark|Spring     , Spring           },
        { Lighter|Spring  , Light|Green     , Dark|Forest      },
        { Lighter|Spring  , Light|Green     , Forest           },
        { Lighter|Spring  , Light|Cyan      , Dark|Azure       },
        { Lighter|Cyan    , Dark|Cyan       , Cyan             },
        { Lighter|Cyan    , Light|Azure     , Dark|Blue        },
        { Lighter|Cyan    , Light|Spring    , Dark|Green       },
        { Lighter|Cyan    , Light|Spring    , Green            },
    };
}
