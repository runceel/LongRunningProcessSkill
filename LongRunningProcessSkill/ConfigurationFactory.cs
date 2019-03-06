using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LongRunningProcessSkill
{
    static class ConfigurationFactory
    {
        private static IConfiguration _configuration;

        private static readonly object _sync = new object();

        public static IConfiguration GetConfiguration(string basePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            Initialize(basePath);
            return _configuration;
        }

        private static void Initialize(string basePath)
        {
            if (_configuration != null)
            {
                return;
            }

            lock(_sync)
            {
                if (_configuration != null)
                {
                    return;
                }

                _configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
        }
    }
}
