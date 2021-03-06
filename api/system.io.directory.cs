/*
* class Directory
*/

// Directory has no properties either, apparently!
public static DirectoryInfo GetParent(string path)
public static DirectoryInfo CreateDirectory(string path)
public static DirectoryInfo CreateDirectory(string path, DirectorySecurity directorySecurity)
public static bool Exists(string path)
public static void SetCreationTime(string path, DateTime creationTime)
public unsafe static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
public static DateTime GetCreationTime(string path)
public static DateTime GetCreationTimeUtc(string path)
public static void SetLastWriteTime(string path, DateTime lastWriteTime)
public unsafe static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
public static DateTime GetLastWriteTime(string path)
public static DateTime GetLastWriteTimeUtc(string path)
public static void SetLastAccessTime(string path, DateTime lastAccessTime)
public unsafe static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
public static DateTime GetLastAccessTime(string path)
public static DateTime GetLastAccessTimeUtc(string path)
public static DirectorySecurity GetAccessControl(string path)
public static DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
public static void SetAccessControl(string path, DirectorySecurity directorySecurity)
public static string[] GetFiles(string path)
public static string[] GetFiles(string path, string searchPattern)
public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
public static string[] GetDirectories(string path)
public static string[] GetDirectories(string path, string searchPattern)
public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
public static string[] GetFileSystemEntries(string path)
public static string[] GetFileSystemEntries(string path, string searchPattern)
public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
public static IEnumerable<string> EnumerateDirectories(string path)
public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
public static IEnumerable<string> EnumerateFiles(string path)
public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
public static IEnumerable<string> EnumerateFileSystemEntries(string path)
public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
public static string[] GetLogicalDrives()
public static string GetDirectoryRoot(string path)
public static string GetCurrentDirectory()
public static void SetCurrentDirectory(string path)
public static void Move(string sourceDirName, string destDirName)
public static void Delete(string path)
public static void Delete(string path, bool recursive)


