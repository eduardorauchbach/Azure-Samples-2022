using Microsoft.Extensions.Logging;
using RauchTech.Extensions.Logging.Services;
using System.Runtime.CompilerServices;

namespace RauchTech.Extensions.Logging
{
    public interface ICustomLog
    {
        /// <summary>
        /// Adds Scope Keys that will show in every log, from start to end of this scope
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">strings, guids or number</param>
        void AddID(string key, object value);

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        void LogCustom(LogLevel logLevel,
                        EventId? eventId = null,
                        Exception? exception = null,
                        string? message = null,
                        [CallerMemberName] string? memberName = null,
                        [CallerLineNumber] int? sourceLineNumber = null,
                        params ValueTuple<string, object>[] args);
    }

    public interface ICustomLogFactory
    {
        CustomLog CreateLogger<T>() where T : class;
    }
}
