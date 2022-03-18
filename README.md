![Icon32x32](https://user-images.githubusercontent.com/8169082/158886944-8b8e202b-2643-40c5-ba8e-555b157b62ea.png) 
# DisplayedAppSwitcher

This application helps with switching between JW Library and Zoom, when these two apps both have a secondary window running on the second monitor.

## Usage 

The DisplayAppSwitcher icon ![Icon16x16](https://user-images.githubusercontent.com/8169082/158886879-f4c15271-7a35-477d-9edc-95f6185cb5f6.png) sits in the taskbar and can be double clicked to perform a switch. There is also a context menu with available commands that becomes visible when right clicking on the tray icon.

Switches can also be done by means of keyboard shortcuts or a secondary device, such as Stream Deck, where a button can be assigned as an alias to a keyboard shortcut to further minimize human error and improve technical quality.

Currently the shortcuts are assigned as follows:

* Switch to the next:
    * `Ctrl-NumPad 0`
	* Right click on “Switch” in the context menu.
    * Double click the tray icon.
* Switch to JW Library:
    * `Ctrl-NumPad 1`
    * `F5`
	* Right click on “JW Library” in the context menu.
* Switch to Zoom:
    * `Ctrl-NumPad 2` 
    * `F6`
    * Right click on “Zoom” in the context menu.


## Setup

### JW Library

Make sure to turn on “Play video on a second display” in the settings in the JW Library app. Switching it off and back on helps to resolve some of the secondary window disappearance.

### Zoom

In the “General” settings of Zoom, check “Use dual monitors.” It does not look like changing this setting during the already running meeting actually modifies Zoom's behavior, so it is vital to start or join with the “Use dual monitors” turned on beforehand.

> Currently Zoom completely disables participants video grid during sharing in the “Use dual monitors” mode, there is no Side-by-side setting available.

Make sure Zoom is started and running before attempting a switch to Zoom using this utility, otherwise Zoom's window that is internally designated for the dual screen will appear with a blank canvas. (This is not harmful to Zoom in any way; it just indicates that Zoom is not in the dual-monitor mode, or that the Zoom meeting has not been started.)

Zoom's dual monitor window needs to be dragged to the secondary monitor and double clicked, inside the window, to make it full screen. It has no meeting controls, which makes it easy to distinguish it from the other Zoom windows. This has to be done every time the Zoom meeting starts.

### DisplayedAppSwitcher

* Download the latest release from the [Releases](https://github.com/viaart/DisplayedAppSwitcher/releases) section - either `setup.exe` or the `*.msi` file.
   * If the browser informs that it is not allowing to download the file from an unverified developer, try another browser (I've never been a .Net developer, so please bear with me while I'm working out that developer recognition).
* Install the app and ignore the Unknown Publisher warning and let it "Run Anyway".
* Run either from the desktop link, or Start menu.
    * If `Net 6.0 Runtime` is not installed, the prompt will show leading to the page to get the installer.
        * Grab the `Run desktop apps` > `Download x64` one.
        * Install and try to restart DisplayedAppSwitcher.

> Alternatively, use "Visual Studio 2022" for open source licenses to compile and run from the manually built location.

## Problem Description

1. Although the Zoom application supports dual-monitor mode, screen sharing is problematic when JW Library and Zoom, in full screen mode, are being used on the secondary display(s). When the screen share begins, Zoom’s secondary window jumps to the primary screen and completely covers it. This makes it difficult to access the JW Library app to play media.

2. It can also be cumbersome to use the Windows’s Taskbar to switch between the two apps, since there are multiple icons for both JW Library and Zoom pinned to the Taskbar, and it is not readily apparent which icon corresponds to the secondary window of either app. This creates unnecessary pauses, or misclicks. Ideally, a mechanical button or a well known keyboard shortcut to quickly perform the switch would be great, but none of them are provided by either Zoom or JW Library.

3. Most of the time, JW Library should be seen on the secondary monitor, but Zoom’s window tends to cover it due to some Z-sorting or window priority issues. It’s quite hard to diagnose this issue.

## Solution

DisplayedAppSwitcher provides a temporary solution, making the switching between Zoom and JW Library more efficient. Hopefully, Zoom will eventually find a way to fix this behavior. And, perhaps JW Library will provide a more reliable way to keep itself on the top, when another application sits on the same screen.

In the meanwhile, the solution proposed here is to hide Zoom’s secondary window, when JW Library is to be seen. And then bring it back when requested.

DisplayedAppSwitcher hides the secondary Zoom window when it is used to switch to JW Library. When switching back to Zoom, DisplayedAppSwitcher restores the hidden Zoom window to its previous visible state, including full screen mode. This helps to avoid all of the problems described above.

## Known to work

* JW Library
    * 13.0.146 (157716)

* Zoom:
    * 5.9.3 (3169)
    * 5.9.7 (3931)

# TODO

The following things could be done to improve the tool:

- [ ] Auto Update
- [ ] Custom keyboard shortcuts.
- [ ] Different behaviors depending on application versions.


# Changelog

- [x] Singleton - only one app allowed to run at a time - `v1.0.0.1`.
