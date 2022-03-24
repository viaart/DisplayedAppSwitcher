![Icon32x32](https://user-images.githubusercontent.com/8169082/158886944-8b8e202b-2643-40c5-ba8e-555b157b62ea.png) 
# DisplayedAppSwitcher

This application makes it predictable as to which of the two applications - JW Library or Zoom - is seen on the second monitor.

- [DisplayedAppSwitcher](#displayedappswitcher)
  - [Installation](#installation)
  - [Usage](#usage)
  - [Setup](#setup)
    - [JW Library](#jw-library)
    - [Zoom](#zoom)
  - [Problem Description](#problem-description)
  - [Solution](#solution)
  - [Known to work](#known-to-work)
- [TODO](#todo)
- [Changelog](#changelog)
- [Tips & Tricks](#tips--tricks)
  - [Zoom](#zoom-1)
  - [Windows](#windows)

## Installation

* Download the latest release from the [Releases](https://github.com/viaart/DisplayedAppSwitcher/releases) section - the `DisplayedAppSwitcher.msi` file.
    * If the Edge browser informs that it is not allowing to download the file from an unverified developer, right click the file in the list of downloads and choose `Keep`, `Show More`, `Keep Anyway`.
* Install the app and ignore the Unknown Publisher warning by clicking "More info" and then "Run anyway".
* Run either from the desktop link, or Start menu.
    * If `Net 6.0 Runtime` is not installed, the prompt will show leading to the page to get the installer.
        * Grab the `Run desktop apps` > `Download x64` one.
        * Install and try to restart DisplayedAppSwitcher.

* Periodically check for new versions, as Auto Updating is not yet implemented.

* (Optional) Make the tray icon always visible for easy access: Search for "Select which icons appear on the taskbar" setting and toggle the switch.

> To build from source, clone the repository and open `*.csproj` in "Microsoft Visual Studio 2022". Community Edition is enough to compile and use this open source project.

## Usage 

The DisplayAppSwitcher icon ![Icon16x16](https://user-images.githubusercontent.com/8169082/158886879-f4c15271-7a35-477d-9edc-95f6185cb5f6.png) sits in the taskbar and can be double clicked to perform a switch between JW Library and Zoom. There is also a context menu with available commands that becomes visible when right clicking on the tray icon.

Switches can also be done by means of keyboard shortcuts or a secondary device, such as Stream Deck, where a button can be assigned as an alias to a keyboard shortcut to further minimize human error and improve technical quality.

* Right click tray icon to see the context menu.

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

Don't minimize the main Zoom window, otherwise the secondary Zoom window will also be restored from its hidden state when Zoom is asked to go to full size - and this will cover the JW Library on the secondary screen.

## Setup

### JW Library

Make sure to turn on “Play video on a second display” in the settings in the JW Library app. Switching it off and back on helps to resolve some of the secondary window disappearance.

### Zoom

In the “General” settings of Zoom, check “Use dual monitors.” It does not look like changing this setting during the already running meeting actually modifies Zoom's behavior, so it is vital to start or join with the “Use dual monitors” turned on beforehand.

> Currently Zoom completely disables participants video grid during sharing in the “Use dual monitors” mode, there is no Side-by-side setting available.

If you switch to Zoom using this utility and Zoom's meeting is not running, Zoom's window that is internally designated for the dual screen will appear with a blank canvas. This is easily fixed by joining or starting the meeting.

Zoom's dual monitor window needs to be dragged to the secondary monitor and double clicked, anywhere inside the window, to make it full screen. It has no meeting controls, which makes it easy to distinguish it from the other Zoom windows. Double clicking has to be done every time the Zoom starts or joins a meeting.

During initial setup, other Zoom controls and windows may need to be moved to the desired locations, and Zoom seems to remember at least the monitors on which the controls should stay.

## Problem Description

1. Although the Zoom application supports dual-monitor mode, screen sharing is problematic when JW Library and Zoom, in full screen mode, are being used on the secondary display(s). When the screen share begins, Zoom’s secondary window jumps to the primary screen and completely covers it. This makes it difficult to access the JW Library app to play media.

2. It can also be cumbersome to use the Windows’s Taskbar to switch between the two apps, since there are multiple icons for both JW Library and Zoom pinned to the Taskbar, and it is not readily apparent which icon corresponds to the secondary window of either app. This creates unnecessary pauses, or misclicks. Ideally, a mechanical button or a well known keyboard shortcut to quickly perform the switch would be great, but none of them are provided by either Zoom or JW Library.

3. Most of the time, JW Library should be seen on the secondary monitor, but Zoom’s window tends to cover it due to some Z-sorting or window priority issues. JW Library second window may suddenly go to a minimized mode when Share Screen is initiated.

## Solution

DisplayedAppSwitcher provides a temporary solution, making the switching between Zoom and JW Library more predictable.

In the meanwhile, the solution proposed here is to hide Zoom’s secondary window, when JW Library is to be seen. And then bring it back when requested.

DisplayedAppSwitcher hides the secondary Zoom window when it is used to switch to JW Library. When switching back to Zoom, DisplayedAppSwitcher restores the hidden Zoom window to its previous visible state, including full screen mode. This helps to avoid all of the problems described above.

> Hopefully, Zoom will eventually find a way to fix window jumping behavior. And, perhaps JW Library will provide a more reliable way to keep itself on the top, when another application sits on the same screen. Then there will be no need for DisplayedAppSwitcher. Periodically check how everything works without DisplayedAppSwitcher.

## Known to work

* JW Library
    * 13.0.146 (157716)

* Zoom:
    * 5.9.3 (3169)
    * 5.9.7 (3931)
        * This version (or earlier) introduced a new complication in recognizing the target window when the meeting is not running, which is handled by `v1.0.2`.
    * 5.10.0 (4306)
        * All the Zoom problems described above still exist and the tool is still helpful.

# TODO

The following things could be done to improve the tool:

- [ ] Auto Building.
- [ ] Auto Update.
- [ ] Custom keyboard shortcuts.
- [ ] Different behaviors depending on application versions.
- [ ] Turn it into a universal tool with configurations.


# Changelog

- [x] Singleton - only one app allowed to run at a time - `v1.0.0.1`.
- [x] Filter out third Zoom window when the meeting is not running - `v1.0.2`.
- [x] Fix Zoom simply flashing when asked to go to the fore - `v1.0.3`.
- [x] Change keys `F5` & `F6` shortcuts to `F9` & `F10` as `F5` is clashing with the refreshing command - `v1.0.4`.

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

