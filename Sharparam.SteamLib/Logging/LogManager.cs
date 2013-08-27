/* LogManager.cs
 *
 * Copyright © 2013 by Adam Hellberg.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using log4net;
using log4net.Config;

namespace Sharparam.SteamLib.Logging
{
    /// <summary>
    /// Provides helper methods for logging functions.
    /// </summary>
    public static class LogManager
    {
        private static bool _loaded;

        /// <summary>
        /// Loads a configuration for the log4net library.
        /// </summary>
        /// <param name="file">
        /// The configuration file to load.
        /// If null, tries to automatically load a config file based on assembly name,
        /// falls back to using default log4net configuration.
        /// </param>
        public static void LoadConfig(string file = null)
        {
            if (log4net.LogManager.GetRepository().Configured)
            {
                _loaded = true;
                return;
            }

            if (file == null)
            {
                if (File.Exists(AppDomain.CurrentDomain.FriendlyName + ".config"))
                    XmlConfigurator.Configure();
                else
                    BasicConfigurator.Configure();
            }
            else
            {
                if (File.Exists(file))
                    XmlConfigurator.Configure(new FileInfo(file));
                else
                {
                    LoadConfig();
                    return;
                }
            }

            _loaded = true;
        }

        /// <summary>
        /// Gets a logger object associated with the specified object.
        /// </summary>
        /// <param name="sender">The object to get a logger for.</param>
        /// <returns>An <see cref="ILog" /> that provides logging features.</returns>
        public static ILog GetLogger(object sender)
        {
            if (!_loaded)
                LoadConfig();

            return log4net.LogManager.GetLogger(sender.GetType().ToString() == "System.RuntimeType"
                                                 ? (Type)sender
                                                 : sender.GetType());
        }
    }
}
