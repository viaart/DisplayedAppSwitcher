# Displayed App Switcher

Appears as a system tray icon: ![](https://user-images.githubusercontent.com/8169082/158886879-f4c15271-7a35-477d-9edc-95f6185cb5f6.png) when run.

This tool makes it predictable as to which of the two applications - JW Library or Zoom - is seen on the second monitor.

* Supported OS:
  * Windows 10 (64bit)
  * Windows 11 (64bit)

* Requirement:
  * .NET 6.0 Runtime

## Installation

* Download the latest release from the [Releases](https://github.com/viaart/DisplayedAppSwitcher/releases) section - the `DisplayedAppSwitcher.msi` file.
    * If the Edge browser informs that it is not allowing to download the file from an unverified developer, right click the file in the list of downloads and choose `Keep`, `Show More`, `Keep Anyway`.
* Install the app and ignore the Unknown Publisher warning by clicking "More info" and then "Run anyway".
* Run the ![](https://user-images.githubusercontent.com/8169082/158886879-f4c15271-7a35-477d-9edc-95f6185cb5f6.png) Displayed App Switcher either from the desktop link, or the Start menu.
    * If `.Net 6.0 Runtime` is not installed, the prompt will show leading to the page to get the installer.
        * Grab the `Run desktop apps` > `Download x64` one.
        * Install and try to restart **Displayed App Switcher**.
* Periodically check for new versions, as Auto Updating is not yet implemented.
* Remove this tool when it becomes unnecessary.

* (Optional) Make the tray icon always visible for easy access:
  * Drag the icon from the hidden icons popup onto the system tray.
  * Or search for "Control which system tray icons appear on the taskbar" setting and toggle the switch.

## Usage 

The **Display App Switcher** icon ![Icon16x16](https://user-images.githubusercontent.com/8169082/158886879-f4c15271-7a35-477d-9edc-95f6185cb5f6.png) sits in the system tray when run. There is a context menu with available commands that becomes visible when right clicking on the tray icon.

Currently the shortcuts are assigned as follows:

* Switch to the next:
    * `Ctrl-NumPad 0`
    * Double click the tray icon.
* Switch to JW Library:
    * `Ctrl-NumPad 1`
    * `F9`
* Switch to Zoom:
    * `Ctrl-NumPad 2` 
    * `F10`

Take a note of the following:

* **Never minimize the main Zoom window**, otherwise the secondary Zoom window will also be restored from its hidden state when Zoom is asked to Exit Minimized Video - and this will cover the JW Library on the secondary screen. If that happened by accident, tap `F9` to return to a JW Library mode.
* If Zoom accidentally crashes, it will restore itself, and rejoin the meeting as Host. At that moment Zoom's secondary window will take over. It's a good idea to be ready to quickly tap `F9` while this is happening to prevent a random user seen.
* If during an attempt to share screen JW Library accidentally hides, click `F9` to bring it back. This can be done even while the Share Screen dialog is up or right after starting the share. The video doesn't start playing until the secondary window is brought back to vision.

> To further improve reliability of switching, a physical device, such as Elgato "Stream Deck" can be obtained. Buttons on the deck like that can be assigned to playback keyboard shortcuts, and with the modified pictures & labels on the buttons make it even quicker to perform an action.

## Setup

### JW Library

Make sure to turn on “Play video on a second display” in the settings in the JW Library app. Switching it off and back on helps to resolve some of the secondary window disappearance.

### Zoom

In the “General” settings of Zoom, check:

* “Use dual monitors"
* "Enter full screen automatically when starting or joining a meeting"
* "Always show meeting controls"
  
“Use dual monitors” does not change behavior during the meeting, so it is vital to start or join with the “Use dual monitors” turned on beforehand.

> Currently Zoom completely disables participants video grid during sharing in the “Use dual monitors” mode, there is no Side-by-side setting available.

If you switch to Zoom using this utility before starting the Zoom's meeting, Zoom's window that is internally designated for the dual screen will show up as a blank canvas. This is totally harmless and is easily fixed by starting the meeting or tapping `F9`.

Initially Zoom's dual monitor window needs to be dragged to the secondary monitor and double clicked, anywhere inside the window, to make it full screen. It has no meeting controls, which makes it distinguishable from the other Zoom windows. Double clicking has to be done every time the Zoom starts or joins a meeting, if "Enter full screen automatically when starting or joining a meeting" is not checked in the settings.

During initial setup, other Zoom controls and windows may need to be moved to the desired locations, including share screen toolbar, but Zoom seems to remember at least the monitors on which the controls should stay, so this is a one time action.

## Problem Description

1. Although the Zoom application supports dual-monitor mode, screen sharing is problematic when JW Library and Zoom, in full screen mode, are being used on the secondary display(s). When the screen share begins, Zoom’s secondary window jumps to the primary screen and completely covers it. This makes it difficult to access the JW Library app to play media.

2. Moreover, when screen sharing is done, Zoom window doesn't restore itself to the secondary screen behind the JW Library window.

3. When JW Library crashes, accidentally a Zoom participant may show on the wall screens.

4. Most of the time, JW Library should be seen on the secondary monitor, but Zoom’s window tends to cover it due to some Z-sorting or window priority issues. JW Library second window may suddenly go to a minimized mode when Share Screen is initiated.

5. It can be cumbersome to use the Windows’s Taskbar to switch between the two apps, since there are multiple icons for both JW Library and Zoom pinned to the Taskbar. Attempts to find the right window create unnecessary pauses, or misclicks. Ideally, a well known keyboard shortcut to quickly perform the switch would help, but none of them are provided by either Zoom or JW Library.

## Solution Overview

This tool completely hides the secondary Zoom window from the system when requested to switch to JW Library with `F9`. In fact the toolbar icon that corresponds to it also disappears. If JW Library crashes or minimizes, Zoom participant does not show on the second monitor. When switching back to Zoom with `F10`, **Displayed App Switcher** restores the hidden Zoom window to its previous visible state. 

An attempt to share screen with the hidden secondary Zoom window also helps, as there is nothing to move to the main screen, as otherwise happens when it is not hidden. Zoom window stays hidden and waits to be shown back. When requested to be shown, Zoom window automatically retains its full screen mode without any additional action. The tool simultaneously minimizes the secondary JW Library window to improve predictability. This tool never attempts to hide JW Library's secondary window (and in fact it does not seem to be possible).

**Displayed App Switcher** is _not_ maintaining the state of the applications constantly, but instead only once during actual request. This is to avoid too much meddling into the behavior of both applications.

## Still needed?

I test the need for this tool as Zoom & JW Library receive their updates. So far, the behavior stays the same.

Hopefully, Zoom will eventually find another way of what to do with the secondary window and fix window jumping behavior. And, perhaps JW Library will provide a more reliable way to keep itself on the top, when other applications might fight for staying on top of others. Then there will be no need for **Displayed App Switcher**.


## Known to Work

* Windows 10 (64bit) / 11 (64bit)

* JW Library
    * 14.3.45 (429479)
    * 13.1.58 (170209)
    * 13.0.146 (157716)

* Zoom:
  * Zoom Workplace
    * 6.0.11 (39959)
  * Original Zoom
    * 5.11.1 (6602)
    * 5.10.7 (6120)
    * 5.10.6 (5889)
    * 5.10.4 (5035)
    * 5.10.3 (4851)
    * 5.10.1 (4420)
    * 5.10.0 (4306)
    * 5.9.7 (3931)
    * 5.9.3 (3169)
    
# TODO

The following things could be done to improve the tool:

- [ ] Monitor for new Zoom PID in case it crashes to restore the last state.
- [ ] Auto Building.
- [ ] Auto Update.
- [ ] Custom keyboard shortcuts.
- [ ] Different behaviors depending on JW Library / Zoom versions.
- [ ] Turn it into a universal tool with configurations.


# Changelog

- [x] v1.3.1
  - Make setup show uninstall dialog of the previous version instead of a doing it quietly.
- [x] v1.3.0
  - Check for updates upon wake up from hibernation or sleep
- [x] v1.2.0
  - Update for Zoom Workspace
  - Check for new versions with menu and when the app starts
- [x] v1.0.4 - Change keys `F5` & `F6` shortcuts to `F9` & `F10` as `F5` is clashing with the refreshing command.
- [x] v1.0.3 - Fix Zoom simply flashing when asked to go to the fore.
- [x] v1.0.2 - Filter out third Zoom window when the meeting is not running.
- [x] v1.0.0.1 - Singleton - only one app allowed to run at a time.

# Tips & Tricks

## Zoom

To eliminate the green border that sometimes surrounds the shared screen / window, remove corresponding checkbox under the Zoom settings > Share Screen > Advanced.

![](https://user-images.githubusercontent.com/8169082/159496415-cca06725-12d1-4867-bbdb-5ddaae8a76f1.gif)

## Windows

Currently, this utility **minimizes** JW Library when showing the Zoom window and restores it back, which makes it visibly transitioning between the states. This animation can be disabled for all the windows in the system through the Advanced system settings:

* Start typing in Window search (Win+S): `Advanced System Settings` and select `View advanced system settings` (Control Panel) as soon as you see it.

* Go to `Advanced` Tab, and under the first box called `Performance`, click `Settings...`.

    ![](https://user-images.githubusercontent.com/8169082/159498065-cfe0aced-ae7e-4ffd-a08c-e45f0c370886.png)

* In the Performance Options, uncheck the `Animate windows when minimizing and maximizing`.

    ![](https://user-images.githubusercontent.com/8169082/159497943-e8f8c8eb-1a62-45a8-baf5-9e2781e4d3e3.png)

* Keep clicking OK until all the setting windows are closed.

## Build from source

To build from source, clone the repository and open `*.csproj` in "Microsoft Visual Studio 2022". Community Edition is enough to compile and use this open source project.
