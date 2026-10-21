using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Prosenter
{
    //Reads a title file: a CSV with three columns (Numbering, Title, Notes) and an optional header row
    public static class TitleFileReader
    {
        private static readonly string[] NumberHeaders = { "#", "no", "no.", "num", "number", "numbering", "numero", "numéro" };
        private static readonly string[] TitleHeaders = { "title", "titre" };

        public static List<MainWindow.PresentationItem> Read(string path)
        {
            var items = new List<MainWindow.PresentationItem>();
            var isFirstRow = true;

            foreach (var fields in ParseRows(ReadAllText(path)))
            {
                var number = fields.Length > 0 ? fields[0] : string.Empty;
                var title = fields.Length > 1 ? fields[1] : string.Empty;
                var notes = fields.Length > 2 ? fields[2] : string.Empty;

                //Excel leaves rows like ",," behind where cells were cleared
                if (string.IsNullOrWhiteSpace(number) && string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(notes))
                {
                    continue;
                }

                //Headers are optional, so only skip the first row when it looks like one
                if (isFirstRow)
                {
                    isFirstRow = false;
                    if (IsHeader(number, title))
                    {
                        continue;
                    }
                }

                items.Add(new MainWindow.PresentationItem(number, title, notes));
            }

            return items;
        }

        //Excel's "CSV (Comma delimited)" uses the Windows list separator, which is a semicolon on French
        //and other systems where the comma is the decimal separator, so read the file both ways and keep
        //the one that fits: read with the wrong delimiter, the number column swallows the real one
        private static List<string[]> ParseRows(string text)
        {
            List<string[]>? commaRows = null;
            List<string[]>? semicolonRows = null;
            CsvHelperException? commaError = null;

            try
            {
                commaRows = ParseRows(text, ",");
            }
            catch (CsvHelperException ex)
            {
                commaError = ex;
            }

            try
            {
                semicolonRows = ParseRows(text, ";");
            }
            catch (CsvHelperException)
            {
            }

            var commaFits = commaRows == null ? 0 : CountFittingRows(commaRows, ";");
            var semicolonFits = semicolonRows == null ? 0 : CountFittingRows(semicolonRows, ",");

            if (semicolonRows != null && semicolonFits > commaFits)
            {
                return semicolonRows;
            }
            if (commaRows != null)
            {
                return commaRows;
            }
            throw commaError!;
        }

        private static List<string[]> ParseRows(string text, string delimiter)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = delimiter,
            };

            var rows = new List<string[]>();
            using (var reader = new StringReader(text))
            using (var parser = new CsvParser(reader, config))
            {
                while (parser.Read())
                {
                    rows.Add(parser.Record ?? Array.Empty<string>());
                }
            }
            return rows;
        }

        private static int CountFittingRows(List<string[]> rows, string otherDelimiter)
        {
            return rows.Count(fields => fields.Length >= 2 && !fields[0].Contains(otherDelimiter));
        }

        private static bool IsHeader(string number, string title)
        {
            return NumberHeaders.Contains(number.Trim().ToLowerInvariant())
                || TitleHeaders.Contains(title.Trim().ToLowerInvariant());
        }

        //Excel's "CSV (Comma delimited)" saves in the Windows ANSI code page rather than UTF-8,
        //so fall back to it when the file isn't valid UTF-8, otherwise accents like é are lost
        private static string ReadAllText(string path)
        {
            byte[] bytes;
            //Share with writers so a file that is still open in Excel can be imported
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                bytes = memory.ToArray();
            }

            try
            {
                var strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                using (var reader = new StreamReader(new MemoryStream(bytes), strictUtf8, detectEncodingFromByteOrderMarks: true))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (DecoderFallbackException)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage).GetString(bytes);
            }
        }
    }
}
