
/*
* class FileInfo : FileSystemInfo
* see also: system.io.filesysteminfo
*/

// properties
public override string Name;
public long Length;
public string DirectoryName;
public DirectoryInfo Directory;
public bool IsReadOnly;

// methods
public FileSecurity GetAccessControl()
public FileSecurity GetAccessControl(AccessControlSections includeSections)
public void SetAccessControl(FileSecurity fileSecurity)
public StreamReader OpenText()
public StreamWriter CreateText()
public StreamWriter AppendText()
public FileInfo CopyTo(string destFileName)
public FileInfo CopyTo(string destFileName, bool overwrite)
public FileStream Create()
public override void Delete()
public void Decrypt()
public void Encrypt()
public override bool Exists
public FileStream Open(FileMode mode)
public FileStream Open(FileMode mode, FileAccess access)
public FileStream Open(FileMode mode, FileAccess access, FileShare share)
public FileStream OpenRead()
public FileStream OpenWrite()
public void MoveTo(string destFileName)
public FileInfo Replace(string destinationFileName, string destinationBackupFileName)
public FileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
public override string ToString()
