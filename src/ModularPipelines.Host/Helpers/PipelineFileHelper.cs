using System.IO.Compression;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace ModularPipelines.Host.Helpers;

public static class PipelineFileHelper
{
	public static Task<DirectoryInfo> GetDirectory(string directoryPath)
	{
		var directoryInfo = new DirectoryInfo(directoryPath);
		return Task.FromResult(directoryInfo);
	}

	public static Task<FileInfo> GetFile(string targetFilePath)
	{
		var fileInfo = new FileInfo(targetFilePath);
		return Task.FromResult(fileInfo);
	}

	public static async Task WriteTextToFileAsync(string targetFilePath, string content)
	{
		await File.WriteAllTextAsync(targetFilePath, content);
	}

	public static async Task<string> ReadTextFromFileAsync(string targetFilePath)
	{
		return await File.ReadAllTextAsync(targetFilePath);
	}

	public static async Task ZipDirectory(string sourceDirectory, string targetFilePath)
	{
		ZipFile.CreateFromDirectory(sourceDirectory, targetFilePath);
	}
}

// public class FileInfo
// {
// 	public string Name { get; set; }
// 	public string FullName { get; set; }
// 	public string Extension { get; set; }
// 	public long Length { get; set; }
// 	public DateTime LastWriteTime { get; set; }
// }
