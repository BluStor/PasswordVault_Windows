You can build project in Visual Studio. 
Go to project folder. 
Open bin/Debug/ folder. 
Run GateKeeperSDKTest.exe file. 
Or just press Ctrl+F5 in Visual Studio.

Your card needs to be in pairing mode before running GateKeeperSDKTest.exe. 
When you type help, you will see all list commands with examples of using.

Commands:

    List: Files and Directories
    Example: List /data

    ChangeWorkingDirectory: Set working directory
    Example: ChangeWorkingDirectory /data/test_folder

    CurrentWorkingDirectory: Get working directory
    Example: CurrentWorkingDirectory

    Put: Copy files to card
    Example: Put /data/test.txt C:/Users/User/GateKeeperSDKTest/test.txt

    Get: Download a file
    Example: Get /data/test.txt C:/Users/User/GateKeeperSDKTest/test.txt

    FreeMemory: Get free memory value on the card
    Example: FreeMemory

    Rename: Rename file
    Example: Rename /data/test.txt /data/temp123.txt

    Delete: Delete File
    Example: Delete /data/test.txt

    CreatePath: Make a directory
    Example: CreatePath /data/test_folder

    DeletePath: Delete Folder
    Example: DeletePath /data/test_folder