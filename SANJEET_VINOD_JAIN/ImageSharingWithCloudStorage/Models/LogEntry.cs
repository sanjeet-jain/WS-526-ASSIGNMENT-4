﻿using System;
using Microsoft.Azure.Cosmos.Table;

namespace ImageSharingWithCloudStorage.Models;

public class LogEntry : TableEntity
{
    public LogEntry()
    {
    }

    public LogEntry(int imageId)
    {
        CreateKeys(imageId);
    }

    public DateTime EntryDate { get; set; }

    public string Username { get; set; }

    public string Caption { get; set; }

    public string Uri { get; set; }

    public int ImageId { get; set; }

    public void CreateKeys(int imageId)
    {
        EntryDate = DateTime.UtcNow;

        PartitionKey = EntryDate.ToString("MMddyyyy");

        ImageId = imageId;

        RowKey = string.Format("{0}-{1:10}_{2}",
            imageId,
            DateTime.MaxValue.Ticks - EntryDate.Ticks,
            Guid.NewGuid());
    }
}