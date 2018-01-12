/*
* class DirectoryInfo : FileSystemInfo
*/

// properties
public override string Name
public DirectoryInfo Parent
public override bool Exists
public DirectoryInfo Root

// functions
public DirectoryInfo CreateSubdirectory(string path)
public DirectoryInfo CreateSubdirectory(string path, DirectorySecurity directorySecurity)
public void Create()
public void Create(DirectorySecurity directorySecurity)

public DirectorySecurity GetAccessControl()
public DirectorySecurity GetAccessControl(AccessControlSections includeSections)
public void SetAccessControl(DirectorySecurity directorySecurity)
public FileInfo[] GetFiles(string searchPattern)
public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
public FileInfo[] GetFiles()
public DirectoryInfo[] GetDirectories()
public FileSystemInfo[] GetFileSystemInfos(string searchPattern)
public FileSystemInfo[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
public FileSystemInfo[] GetFileSystemInfos()
public DirectoryInfo[] GetDirectories(string searchPattern)
public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
public IEnumerable<DirectoryInfo> EnumerateDirectories()
public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
public IEnumerable<FileInfo> EnumerateFiles()
public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos()
public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern)
public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
public void MoveTo(string destDirName)
public override void Delete()
public void Delete(bool recursive)
public override string ToString()
