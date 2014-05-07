﻿using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Security.Application;
using ProceXSS.Common;
using ProceXSS.Configuration;
using ProceXSS.Enums;
using ProceXSS.Interface;

namespace ProceXSS
{
    public sealed class RequestCleaner : IRequestCleaner
    {
        private Regex _potentialXssAttackRegex;
        public void Clean(NameValueCollection collection, ProceXssConfigurationHandler configurationHandler, EncoderType encoderType = EncoderType.AutoDetect)
        {
            if (string.IsNullOrWhiteSpace(configurationHandler.ControlRegex))
            {
                _potentialXssAttackRegex = new Regex(RegexExecutor.POTENTIAL_XSS_ATTACK_EXPRESSION_V3, RegexOptions.IgnoreCase);
            }
            else
            {
                try
                {
                    _potentialXssAttackRegex = new Regex(HttpUtility.HtmlDecode(configurationHandler.ControlRegex),
                                                         RegexOptions.IgnoreCase);
                }
                catch
                {
                    _potentialXssAttackRegex = new Regex(RegexExecutor.POTENTIAL_XSS_ATTACK_EXPRESSION_V3,
                                                        RegexOptions.IgnoreCase);
                }
            }

            PropertyInfo readonlyProperty = ReflectionExecutor.MakeWritable(collection);

            for (int i = 0; i < collection.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(collection[i]))
                {
                    continue;
                }

                IterateCleanUp(encoderType, collection, i);
            }

            readonlyProperty.SetValue(collection, true, null);
        }

        private void IterateCleanUp(EncoderType encoderType, NameValueCollection collection, int index)
        {
            switch (encoderType)
            {
                case EncoderType.Javascript:
                    {
                        collection[collection.Keys[index]] = Encoder.JavaScriptEncode(collection[index]);
                        break;
                    }
                case EncoderType.HtmlFragment:
                    {
                        collection[collection.Keys[index]] = RegexExecutor.IsNumber(collection[index])
                                                             ? collection[index]
                                                             : Sanitizer.GetSafeHtmlFragment(collection[index]);
                        break;
                    }
                case EncoderType.Html:
                    {
                        collection[collection.Keys[index]] = Encoder.HtmlEncode(collection[index]);
                    }
                    break;
                case EncoderType.HtmlAttribute:
                    {
                        collection[collection.Keys[index]] = Encoder.HtmlAttributeEncode(collection[index]);
                    }
                    break;
                case EncoderType.AutoDetect:
                    {
                        if (RegexExecutor.IsXssAttack(_potentialXssAttackRegex, collection[index]))
                        {
                            collection[collection.Keys[index]] = Encoder.JavaScriptEncode(collection[index]);
                        }
                        break;
                    }
            }
        }
    }
}
