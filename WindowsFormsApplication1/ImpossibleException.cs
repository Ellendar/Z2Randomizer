﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer;

internal class ImpossibleException : Exception
{
    public ImpossibleException() : base()
    {
        
    }

    public ImpossibleException(string message) : base(message)
    {

    }
}