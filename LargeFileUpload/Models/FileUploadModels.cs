using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace LargeFileUpload.Models
{
    public class FileChunkMetaData
    {
        public string ChunkIdentifier { get; set; }

        public long? ChunkStart { get; set; }

        public long? ChunkEnd { get; set; }

        public long? TotalLength { get; set; }

        public bool IsFirstChunk
        {
            get { return ChunkStart == 0; }
        }

        public bool IsLastChunk
        {
            get { return ChunkEnd == TotalLength; }
        }
    }

    public class UploadProcessingResult
    {
        public bool IsComplete { get; set; }

        public string FileName { get; set; }

        public string LocalFilePath { get; set; }

        public NameValueCollection FileMetadata { get; set; }
    }
}