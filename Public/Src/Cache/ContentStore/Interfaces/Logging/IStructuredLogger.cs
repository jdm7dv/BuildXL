﻿using System;
using System.Collections.Generic;
using System.Text;
using BuildXL.Cache.ContentStore.Interfaces.Tracing;

namespace BuildXL.Cache.ContentStore.Interfaces.Logging
{
    /// <summary>
    ///     Provides logging with structure for when additional (non-message) information needs to be transmitted.
    /// </summary>
    public interface IStructuredLogger : ILogger
    {
        /// <summary>
        ///     Log a message with the given severity if it is at least as high as the current severity.
        /// </summary>
        /// <param name="severity">Severity to attach to this log message</param>
        /// <param name="correlationId">The correlation id of the message</param>
        /// <param name="message">The raw string to log</param>
        void Log(Severity severity, string correlationId, string message);
    }
}
