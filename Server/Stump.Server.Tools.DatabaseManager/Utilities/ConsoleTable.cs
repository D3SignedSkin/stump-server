using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stump.Server.Tools.DatabaseManager.Utilities
{
    public class ConsoleTable
    {
        private readonly List<string> m_headers;
        private readonly List<string[]> m_rows = new List<string[]>();

        public ConsoleTable(params string[] headers)
        {
            if (headers == null || headers.Length == 0)
                throw new ArgumentException("At least one header is required.", nameof(headers));

            m_headers = headers.ToList();
        }

        public void AddRow(params object[] values)
        {
            if (values == null)
                values = Array.Empty<object>();

            if (values.Length != m_headers.Count)
                throw new ArgumentException("Row value count must match header count.", nameof(values));

            m_rows.Add(values.Select(v => v?.ToString() ?? string.Empty).ToArray());
        }

        public void Write(TextWriter writer)
        {
            if (writer == null)
                writer = Console.Out;

            if (m_rows.Count == 0)
            {
                writer.WriteLine("(aucun résultat)");
                return;
            }

            var widths = new int[m_headers.Count];
            for (int i = 0; i < m_headers.Count; i++)
            {
                widths[i] = m_headers[i].Length;
            }

            foreach (var row in m_rows)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    widths[i] = Math.Max(widths[i], row[i]?.Length ?? 0);
                }
            }

            string headerLine = string.Join(" | ", m_headers.Select((h, i) => h.PadRight(widths[i])));
            string separator = string.Join("-+-", widths.Select(w => new string('-', w)));

            writer.WriteLine(headerLine);
            writer.WriteLine(separator);

            foreach (var row in m_rows)
            {
                var line = string.Join(" | ", row.Select((cell, index) => (cell ?? string.Empty).PadRight(widths[index])));
                writer.WriteLine(line);
            }
        }
    }
}
