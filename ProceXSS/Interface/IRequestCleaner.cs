﻿using System.Collections.Specialized;
using ProceXSS.Configuration;
using ProceXSS.Enums;

namespace ProceXSS.Interface
{
    public interface IRequestCleaner
    {
        void Clean(NameValueCollection collection, IXssConfigurationHandler configurationHandler, EncoderType encoderType = EncoderType.AutoDetect);
    }
}
