# Backy

Backy is basically a simple Backup&Restore app which operates on folders (as opposed to your entire computer) and also offers some very simple Version Control capabilities.  
I decided to develop it after I couldn't find that one software that had exactly what I needed. It seemed to me that all apps out there had some piece of the puzzle missing. So I went ahead and started coding.

The app's concept is very simple:   
 1. Choose a source folder - the content of that folder will be backed up.
 2. Choose an empty target folder - it will contain the actual backup.
 3. Run backup (either manually, scheduled or automatically when the app detects changes on the source folder).
 4. On the first time, the app will detect that there is no backup yet, and will basically copy all files from the source folder to the target folder.
 5. On the following runs, the app will only copy new/modified files. It will mark files that were deleted or renamed.
 6. Version control is achieved by:
    1. Deleted files are only marked by the app as deleted. The actual copy remains as it is in the target folder. (a feature to permanently delete a deleted file is currently being considered).
    2. Modified files are copied to a new location, thus preserving the original files.
    3. Renamed files are marked as renamed and thus the previous name is also being preserved.
    4. The app offers a View window in which the user can "Browse" the backup folder and see how it looked after each backup and from there the user can open, copy or restore individual files.


Here are some screen shots:  
Main backup window during running of backup:   
![main window during run](https://raw.githubusercontent.com/yaronthurm/Backy/master/Screenshots/main_window_during_backup.PNG)

Main backup window after backup completed:   
![main window after running](https://raw.githubusercontent.com/yaronthurm/Backy/master/Screenshots/main_window_backup_finished.PNG)

View window to show latest backup (and enable restore):   
![view window](https://raw.githubusercontent.com/yaronthurm/Backy/master/Screenshots/view_of_the_latest_backup.PNG)