/*
* class FileSystemInfo
*/

// properties
public virtual string FullName;
public string Extension;
public DateTime CreationTime;
public DateTime CreationTimeUtc;
public DateTime LastAccessTime;
public DateTime LastAccessTimeUtc;
public DateTime LastWriteTime;
public DateTime LastWriteTimeUtc;
public abstract string Name { get; }
public abstract bool Exists { get; }
public FileAttributes Attributes;

// functions
public abstract void Delete();
public void Refresh();
public virtual void GetObjectData(SerializationInfo info, StreamingContext context);
