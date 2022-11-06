﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ImageSharingWithCloudStorage.Models;

public class ImageView
    /*
     * View model for an image.
     */
{
    [ScaffoldColumn(false)]
    // Do not call this Id, it will confuse model binding when posting back to controller
    // because of the default route {controller}/{action}/{?id}
    public int Id;

    [ScaffoldColumn(false)]
    // Link to blob storage (for logging).
    public string Uri;

    [Required] [StringLength(40)] public string Caption { get; set; }

    [Required] public int TagId { get; set; }

    [Required] [StringLength(200)] public string Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
    public DateTime DateTaken { get; set; }

    [ScaffoldColumn(false)] public string TagName { get; set; }

    [ScaffoldColumn(false)] public string Username { get; set; }

    public IFormFile ImageFile { get; set; }

    public IEnumerable<SelectListItem> Tags { get; set; }
}