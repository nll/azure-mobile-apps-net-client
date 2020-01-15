// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.IO;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceHttpResponse
    {
        public string Content { get; private set; }
        
        public Stream Stream { get; private set; }

        public string Etag { get; private set; }

        public LinkHeaderValue Link { get; private set; }

        public MobileServiceHttpResponse(string content, string etag, LinkHeaderValue link)
        {
            this.Content = content;
            this.Etag = etag;
            this.Link = link;
        }
        
        public MobileServiceHttpResponse(Stream stream, string etag, LinkHeaderValue link)
        {
            this.Stream = stream;
            this.Etag = etag;
            this.Link = link;
        }
    }
}
