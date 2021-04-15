// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Writers
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Models;

    public class MarkdownWriter : IDocumentWriter
    {
        public async Task Write(Configuration configuration, Document document, Stream output)
        {
            var writer = new StreamWriter(output, Encoding.UTF8)
                { AutoFlush = true };

            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync($"title: {document.Title}");
            await writer.WriteLineAsync($"author: {document.Author}");
            await writer.WriteLineAsync($"start date: {document.CreatedDate}");
            await writer.WriteLineAsync($"end date: {document.ModifiedDate}");
            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync();

            var currentPage = 0;
            using var sectionIterator = document.Sections.GetEnumerator();
            sectionIterator.MoveNext();
            foreach (var annotation in document.Annotations)
            {
                // Print section header
                // TODO option for including empty headers
                while (sectionIterator.Current != null &&
                       sectionIterator.Current.Location <= annotation.Context.Location)
                {
                    var heading = sectionIterator.Current;
                    await writer.WriteLineAsync($"{new string('#', heading.Level)} {heading.Title}");
                    await writer.WriteLineAsync();
                    sectionIterator.MoveNext();
                }

                // Print page number
                if (currentPage < annotation.Context.PageNumber)
                {
                    currentPage = annotation.Context.PageNumber;
                    await writer.WriteLineAsync($"**Page {currentPage}**");
                    await writer.WriteLineAsync();
                }

                var prefix =
                    annotation.Type.Equals(AnnotationType.Highlight)
                        ? ">"
                        : "Note:";
                await writer.WriteLineAsync($"{prefix} {annotation.Content}");

                if (configuration.ExtractionContextLength > 0 && !string.IsNullOrEmpty(annotation.Context.Content))
                {
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync($"Context: {annotation.Context.Content}");
                }

                await writer.WriteLineAsync();
            }
        }
    }
}