using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImageSharingWithCloudStorage.Models;

public class ApproveModel
{
    public IList<SelectListItem> Images { get; set; }
}