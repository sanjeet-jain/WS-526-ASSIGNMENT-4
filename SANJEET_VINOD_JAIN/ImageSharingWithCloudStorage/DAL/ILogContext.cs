using System.Collections.Generic;
using System.Threading.Tasks;
using ImageSharingWithCloudStorage.Models;
using Microsoft.Azure.Cosmos.Table;

namespace ImageSharingWithCloudStorage.DAL;

/**
    * Interface for logging image views in the application.
    */
public interface ILogContext
{
    public Task<CloudTable> CreateTableAsync();

    public Task AddLogEntryAsync(string user, ImageView imageView);

    public IEnumerable<LogEntry> LogsToday();
}