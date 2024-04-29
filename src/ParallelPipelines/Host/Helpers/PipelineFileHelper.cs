using System.IO.Compression;
using System.Text.Json;

namespace ParallelPipelines.Host.Helpers;

public static class PipelineFileHelper
{
	public static DirectoryInfo GitRootDirectory { get; set; } = null!;

	internal static async Task PopulateGitRootDirectory()
	{
		var result =
			await PipelineCliHelper.RunCliCommandAsync("git", "rev-parse --show-toplevel", CancellationToken.None);
		var gitRootDirectory = await GetDirectory(result!.StandardOutput.Trim().ReplaceLineEndings(string.Empty));
		if (gitRootDirectory.Exists is false)
		{
			throw new DirectoryNotFoundException("Git root directory not found");
		}

		GitRootDirectory = gitRootDirectory;
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

	public static async Task<FileInfo> CreateFileIfMissingAndGetFile(string targetFilePath)
	{
		var fileInfo = new FileInfo(targetFilePath);
		if (fileInfo.Exists is false)
		{
			await fileInfo.Create().DisposeAsync();
		}
		fileInfo.Refresh();
		return fileInfo;
	}

	public static async Task<FileInfo> CreateFileIfMissingAndGetFile(this DirectoryInfo directoryInfo, string relativePath)
	{
		var fileInfo = await CreateFileIfMissingAndGetFile(directoryInfo.FullName + Path.DirectorySeparatorChar + relativePath);
		return fileInfo;
	}

	public static async Task<FileInfo> GetFile(this DirectoryInfo directoryInfo, string relativePath)
	{
		var fileInfo = await GetFile(directoryInfo.FullName + Path.DirectorySeparatorChar + relativePath);
		return fileInfo;
	}

	public static async Task<T?> ReadFileAsJson<T>(this DirectoryInfo directoryInfo, string relativePath)
	{
		var fileInfo = await GetFile(directoryInfo.FullName + Path.DirectorySeparatorChar + relativePath);
		return await fileInfo.ReadFileAsJson<T>();
	}

	public static async Task<T?> ReadFileAsJson<T>(this FileInfo fileInfo)
	{
		if (fileInfo.Exists is false)
		{
			throw new FileNotFoundException($"File not found: {fileInfo.FullName}");
		}

		await using var fileStream = fileInfo.OpenRead();
		var deserialized = JsonSerializer.Deserialize<T>(fileStream);
		return deserialized;
	}

	public static async Task<DirectoryInfo> GetDirectory(this DirectoryInfo currentDirectoryInfo, string relativePath)
	{
		var directoryInfo =
			await GetDirectory(currentDirectoryInfo.FullName + Path.DirectorySeparatorChar + relativePath);
		return directoryInfo;
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

	public static string GetFullNameUnix(this FileInfo fileInfo)
	{
		return fileInfo.FullName.Replace('\\', '/');
	}
}
