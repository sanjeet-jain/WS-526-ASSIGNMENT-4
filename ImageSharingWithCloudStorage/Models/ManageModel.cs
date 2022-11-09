using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImageSharingWithCloudStorage.Models;

public class ManageModel
{
    public IList<SelectListItem> Users { get; set; }
}