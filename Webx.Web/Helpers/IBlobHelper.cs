using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Webx.Web.Helpers
{
    public interface IBlobHelper
    {
        //Recebo a imagem através de um ficheiro
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);


        //Recebo a imagem através de um array de bytes, caso dos telemóveis
        Task<Guid> UploadBlobAsync(byte[] file, string containerName);


        //Recebo a imagem através de uma string que tem um endereço de uma imagem
        Task<Guid> UploadBlobAsync(string image, string containerName);


        //TODO: Falta fazer apagar ficheiro do Blob
    }
}
