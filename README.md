# DisplayedAppSwitcher

This application helps with switching between JW Library and Zoom, when these two apps both 
have a secondary window running on the second monitor.

## Usage 

The DisplayAppSwitcher icon sits in the taskbar and can be double clicked to perform a switch.
There is also a context menu with available commands that becomes visible when right clicking on the tray icon.

Switches can also be done by means of keyboard shortcuts or a secondary device, such as Stream Deck, where a button can be assigned as an alias to a keyboard shortcut to further minimize human error and improve technical quality.

Currently the shortcuts are assigned as follows:

* Switch to the next (`Ctrl-NumPad 0`)
	* Double clicking the tray icon does the same.
	* Right clicking on “Switch” in the context menu does the same.
* Switch to JW Library (`Ctrl-NumPad 1`)
	* Right clicking on “JW Library” in the context menu does the same.
* Switch to Zoom (`Ctrl-NumPad 2`)
* Right clicking on “Zoom” in the context menu does the same.


## Setup

Make sure to turn on “Play video on a second display” in the settings in the JW Library app. Switching it off and back on helps to resolve some of the secondary window disappearance.

In the “General” settings of Zoom, check “Use dual monitors.” Make sure Zoom is started and running before attempting a switch, otherwise Zoom's window that is internally designated for the dual screen will appear with a blank canvas. (This is not harmful to Zoom in any way; it just indicates that Zoom is not in the dual-monitor mode, or that the Zoom meeting has not been started.)

Zoom's secondary window has no controls, which makes it easy to distinguish from the other windows. It needs to be dragged to the secondary monitor and double clicked, inside the window, to make it full screen. This has to be done every time the Zoom meeting starts.

## Problem Description

Although the Zoom application supports dual-monitor mode, screen sharing is problematic when JW Library and Zoom, in full screen mode, are being used on the secondary display(s). When the screen share begins, Zoom’s secondary window jumps to the primary screen and completely covers it. This makes it difficult to access the JW Library app to play media.

It can also be cumbersome to use the Windows’s Taskbar to switch between the two apps, since there are multiple icons for both JW Library and Zoom pinned to the Taskbar, and it is not readily apparent which icon corresponds to the secondary window of either app. This creates unnecessary pauses, or misclicks. Ideally, a mechanical button or a well known keyboard shortcut to quickly perform the switch would be great, but none of them are provided by either Zoom or JW Library.

Most of the time, JW Library should be seen on the secondary monitor, but Zoom’s window tends to cover it due to some Z-sorting or window priority issues. It’s quite hard to diagnose this issue.

## Solution

DisplayedAppSwitcher provides a temporary solution, making the switching between Zoom and JW Library more efficient. Hopefully, Zoom will eventually find a way to fix this behavior. And, perhaps JW Library will provide a more reliable way to keep itself on the top, when another application sits on the same screen.

In the meanwhile, the solution proposed here is to hide Zoom’s secondary window, when JW Library is to be seen. And then bring it back when requested.

DisplayedAppSwitcher hides the secondary Zoom window when it is used to switch to JW Library. When switching back to Zoom, DisplayedAppSwitcher restores the hidden Zoom window to its previous visible state, including full screen mode. This helps to avoid all of the problems described above.

## TODO

The following things could be done to improve the tool:

* Auto Update
* Settings to designate keyboard shortcuts.
* Different behaviors depending on application versions.
