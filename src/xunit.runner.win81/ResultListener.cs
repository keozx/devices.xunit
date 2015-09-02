﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runners;
using Xunit.Runners.UI;


namespace Xunit.Runners
{
    class ResultListener : IResultChannel
    {
        readonly Func<TextWriter> writerFunc;
        TextWriter writer;
        int failed;
        int skipped;
        int passed;

        public ResultListener(Func<TextWriter> writerFunc)
        {
            if (writerFunc == null) throw new ArgumentNullException(nameof(writerFunc));
            this.writerFunc = writerFunc;
        }

        public void RecordResult(TestResultViewModel result)
        {
            if (writer == null)
                return;

            if (result.TestCase.Result == TestState.Passed)
            {
                writer.Write("\t[PASS] ");
                passed++;
            }
            else if (result.TestCase.Result == TestState.Skipped)
            {
                writer.Write("\t[SKIPPED] ");
                skipped++;
            }
            else if (result.TestCase.Result == TestState.Failed)
            {
                writer.Write("\t[FAIL] ");
                failed++;
            }
            else
            {
                writer.Write("\t[INFO] ");
            }
            writer.Write(result.TestCase.DisplayName);

            var message = result.ErrorMessage;
            if (!string.IsNullOrEmpty(message))
            {
                writer.Write(" : {0}", message.Replace("\r\n", "\\r\\n"));
            }
            writer.WriteLine();

            var stacktrace = result.ErrorStackTrace;
            if (!string.IsNullOrEmpty(result.ErrorStackTrace))
            {
                var lines = stacktrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                    writer.WriteLine("\t\t{0}", line);
            }
        }

        public Task<bool> OpenChannel(string message = null)
        {
            writer = writerFunc();
            var r = OpenWriter(message);

            return Task.FromResult(r);
        }

        public Task CloseChannel()
        {
            var total = passed + failed; // ignored are *not* run
            writer.WriteLine("Tests run: {0} Passed: {1} Failed: {2} Skipped: {3}", total, passed, failed, skipped);

            writer.Dispose();
            writer = null;

            return Task.FromResult(true);
        }


        bool OpenWriter(string message)
        {
            if (writer == null)
            {
                // TODO: Add options support and use TcpTextWriter
                writer = new StringWriter();
            }
            return true;
        }


        
    }
}