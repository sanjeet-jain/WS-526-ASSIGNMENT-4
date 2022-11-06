using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageSharingWithCloudStorage.DAL;

public interface IImageStorage
{
    public Task SaveFileAsync(IFormFile imageFile, int imageId);
    public Task DeleteFileAsync(int imageId);

    public string ImageUri(IUrlHelper urlHelper, int imageId);
}