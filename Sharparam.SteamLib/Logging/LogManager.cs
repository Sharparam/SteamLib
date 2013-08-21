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

namespace Sharparam.SteamLib.Logging
{
    /// <summary>
    /// Provides helper methods for logging functions.
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// The <see cref="ILogProvider" /> object that <see cref="LogManager" />
        /// will use for providing <see cref="ILog" /> object to calling code.
        /// </summary>
        /// <remarks>This field must be set by code using the SharpBlade library
        /// before any library code is invoked, or code that uses logging features will fail.
        /// The <see cref="SimpleLogProvider" /> can be used for very basic logging.</remarks>
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
        public static ILogProvider LogProvider = null;
// ReSharper restore MemberCanBePrivate.Global
// ReSharper restore FieldCanBeMadeReadOnly.Global

        /// <summary>
        /// Gets a logger object associated with the specified object.
        /// </summary>
        /// <param name="sender">The object to get a logger for.</param>
        /// <returns>An <see cref="ILog" /> that provides logging features.</returns>
        public static ILog GetLogger(object sender)
        {
            if (LogProvider == null)
                throw new Exception("LogProvider is null! Assign LogProvider before calling GetLogger!");

            return LogProvider.GetLogger(sender.GetType().ToString() == "System.RuntimeType" ? (Type) sender : sender.GetType());
        }
    }
}
