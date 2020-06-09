using System;

public class FileInfo
{
	public string path;
	public string filename;
	public string filecontent;
	public long objectID;

	public FileInfo()
	{
	}

	public FileInfo(string path, string filename, string filecontent, long ObjectID)
	{
		this.path = path;
		this.filename = filename;
		this.filecontent = filecontent;
		this.objectID = ObjectID;

	}

	

}
