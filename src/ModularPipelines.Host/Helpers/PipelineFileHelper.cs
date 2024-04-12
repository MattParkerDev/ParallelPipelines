using System.IO.Compression;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace ModularPipelines.Host.Helpers;

public static class PipelineFileHelper
{
	private static DirectoryInfo? _gitRootDirectory { get; set; }
	public static DirectoryInfo GitRootDirectory => _gitRootDirectory ?? GetGitRootDirectory().Result;

	private static async Task<DirectoryInfo> GetGitRootDirectory()
	{
		if (_gitRootDirectory is not null)
		{
			return _gitRootDirectory;
		}

		var result =
			await PipelineCliHelper.RunCliCommandAsync("git", "rev-parse --show-toplevel", CancellationToken.None);
		var gitRootDirectory = await GetDirectory(result!.StandardOutput.Trim().ReplaceLineEndings(string.Empty));
		if (gitRootDirectory.Exists is false)
		{
			throw new DirectoryNotFoundException("Git root directory not found");
		}

		_gitRootDirectory = gitRootDirectory;

		return _gitRootDirectory;
	}

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

	public static Task<FileInfo> GetFile(this DirectoryInfo directoryInfo, string relativePath)
	{
		var fileInfo = new FileInfo(directoryInfo.FullName + Path.DirectorySeparatorChar + relativePath);
		return Task.FromResult(fileInfo);
	}

	public static Task<DirectoryInfo> GetDirectory(this DirectoryInfo currentDirectoryInfo, string relativePath)
	{
		var directoryInfo =
			new DirectoryInfo(currentDirectoryInfo.FullName + Path.DirectorySeparatorChar + relativePath);
		return Task.FromResult(directoryInfo);
	}

	public static Task<FileInfo> ZipDirectoryToFile(this DirectoryInfo sourceDirectory, string targetZipFilePath)
	{
		ZipFile.CreateFromDirectory(sourceDirectory.FullName, targetZipFilePath);
		return GetFile(targetZipFilePath);
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
