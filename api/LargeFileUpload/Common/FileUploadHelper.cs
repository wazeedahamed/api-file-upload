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
            _streamProvider = new MultipartFormDataStreamProvider(UserLocalPath);
        }

        #region Interface
        public async Task<UploadProcessingResult> HandleRequest(HttpRequestMessage request)
        {
            if(FileExist(request))
            {
                throw new Exception("Cannot create file when that file already exists.");
            }
            try
            {
                await request.Content.ReadAsMultipartAsync(_streamProvider);
            }
            catch (Exception ex)
            {
                File.Delete(LocalFileName);
                throw ex;
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
            string filePath = Path.Combine(UserLocalPath, partPath);

            await AppendChunkFileToFile(LocalFileName, filePath);

            if (chunkMetaData.IsLastChunk)
            {
                File.Move(filePath, FinalFilePath);
            }

            return new UploadProcessingResult()
            {
                IsComplete = chunkMetaData.IsLastChunk,
                FileName = OriginalFileName,
                LocalFilePath = chunkMetaData.IsLastChunk ? FinalFilePath : null,
                FileMetadata = _streamProvider.FormData
            };

        }

        private async Task<UploadProcessingResult> ProcessFullFile(HttpRequestMessage request)
        {
            await AppendChunkFileToFile(LocalFileName, FinalFilePath);

            return new UploadProcessingResult()
            {
                IsComplete = true,
                FileName = OriginalFileName,
                LocalFilePath = FinalFilePath,
                FileMetadata = _streamProvider.FormData
            };
        }

        private async Task AppendChunkFileToFile(string fromPath, string toPath)
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
        private Boolean FileExist(HttpRequestMessage request)
        {
            Boolean check = false;
            if (request.IsChunkUpload())
            {
                FileChunkMetaData chunkMetaData = request.GetChunkMetaData();
                check = chunkMetaData.IsFirstChunk;
            }
            else
            {
                check = true;
            }
            if(check)
            {
                if (File.Exists(Path.Combine(UserLocalPath, request.Content.Headers.ContentDisposition.FileName.Trim('"'))))
                {
                    return true;
                }
            }
            return false;
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
        private string FinalFilePath
        {
            get
            {
                return Path.Combine(UserLocalPath, OriginalFileName);
            }
        }
        #endregion
    }
}