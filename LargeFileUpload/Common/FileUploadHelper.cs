using LargeFileUpload.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace LargeFileUpload.Common
{
    public class FileUploadHelper
    {
        private readonly string _uploadPath;
        private readonly MultipartFormDataStreamProvider _streamProvider;

        public FileUploadHelper(string uploadPath)
        {
            _uploadPath = uploadPath;
            _streamProvider = new MultipartFormDataStreamProvider(_uploadPath);
        }

        #region Interface
        public async Task<UploadProcessingResult> HandleRequest(HttpRequestMessage request)
        {
            try
            {
                await request.Content.ReadAsMultipartAsync(_streamProvider);
            }
            catch (Exception ex)
            {
                File.Delete(LocalFileName);
                throw;
            }

            return await ProcessFile(request);
        }
        #endregion

        #region Private implementation
        private async Task<UploadProcessingResult> ProcessFile(HttpRequestMessage request)
        {
            // Requires Create/Write/Delete access to the folder contents
            if (request.IsChunkUpload())
            {
                return await ProcessChunk(request);
            }
            else
            {
                return await ProcessFullFile(request);
            }
        }

        private async Task<UploadProcessingResult> ProcessChunk(HttpRequestMessage request)
        {
            //use the unique identifier sent from client to identify the file
            FileChunkMetaData chunkMetaData = request.GetChunkMetaData();
            string partPath = string.Format("{0}.part", OriginalFileName);
            string filePath = Path.Combine(_uploadPath, partPath);

            await AppendTempFileToFile(LocalFileName, filePath);

            if (chunkMetaData.IsLastChunk)
            {
                string finalPath = Path.Combine(_uploadPath, OriginalFileName);
                File.Move(filePath, finalPath);
            }

            return new UploadProcessingResult()
            {
                IsComplete = chunkMetaData.IsLastChunk,
                FileName = OriginalFileName,
                LocalFilePath = chunkMetaData.IsLastChunk ? Path.Combine(_uploadPath, OriginalFileName) : null,
                FileMetadata = _streamProvider.FormData
            };

        }

        private async Task<UploadProcessingResult> ProcessFullFile(HttpRequestMessage request)
        {
            string finalPath = Path.Combine(_uploadPath, OriginalFileName);
            await AppendTempFileToFile(LocalFileName, finalPath);

            return new UploadProcessingResult()
            {
                IsComplete = true,
                FileName = OriginalFileName,
                LocalFilePath = finalPath,
                FileMetadata = _streamProvider.FormData
            };
        }

        private async Task AppendTempFileToFile(string fromPath, string toPath)
        {
            //append chunks to construct original file
            using (FileStream fileStream = new FileStream(toPath, FileMode.Append))
            {
                FileInfo localFileInfo = new FileInfo(fromPath);
                using (FileStream localFileStream = localFileInfo.OpenRead())
                {
                    await localFileStream.CopyToAsync(fileStream);
                    await localFileStream.FlushAsync();
                    localFileStream.Close();
                }
                await fileStream.FlushAsync();
                fileStream.Close();

                //delete chunk
                localFileInfo.Delete();
            }
        }
        #endregion

        #region Properties
        private string LocalFileName
        {
            get
            {
                MultipartFileData fileData = _streamProvider.FileData.FirstOrDefault();
                return fileData.LocalFileName;
            }
        }
        private string OriginalFileName
        {
            get
            {
                MultipartFileData fileData = _streamProvider.FileData.FirstOrDefault();
                return fileData.Headers.ContentDisposition.Name.Trim('"');
            }
        }
        private string UserLocalPath
        {
            get
            {
                return _uploadPath;
            }
        }
        #endregion
    }
}