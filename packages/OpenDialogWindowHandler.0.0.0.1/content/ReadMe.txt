A OpenDialogWindowHandler package helps in Handling Open Dialog window to upload or import a file

Usage:

1. Include the namespace "OpenDialogWindowHandler"
Eg: using OpenDialogWindowHandler;

2. Create an object for HandleOpenDialog class.
Eg: HandleOpenDialog hodObject = new HandleOpenDialog();

2. Call fileOpenDialog method using the created object with filepath and filename as parameters
Eg: hodObject.fileOpenDialog(filepath, filename);

Note:
filepath		--> Full Folder path where the file exist
filename		--> File name that needs to be opened in the dialog or to be uploaded