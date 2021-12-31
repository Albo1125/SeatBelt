# SeatBelt
SeatBelt is a UK-based resource for FiveM by Albo1125 that provides seat belt sounds, notifications and observations for vehicle occupants that are not wearing a seat belt. It is available at [https://github.com/Albo1125/SeatBelt](https://github.com/Albo1125/SeatBelt)

## Installation & Usage
1. Download the latest release.
2. Unzip the SeatBelt folder into your resources folder on your FiveM server.
3. Add the following to your server.cfg file:
```text
ensure SeatBelt
```
4. Install the [pNotify](https://github.com/Nick78111/pNotify) and [InteractSound](https://github.com/plunkettscott/interact-sound) resources.
5. Add a sound file of your choice to InteractSound called `SeatbeltChime.ogg`. An example sound file has been included in this release.

## Commands & Controls
* Ctrl S - toggle your seat belt while in a vehicle.

## Improvements & Licencing
Please view the license. Improvements and new feature additions are very welcome, please feel free to create a pull request. As a guideline, please do not release separate versions with minor modifications, but contribute to this repository directly. However, if you really do wish to release modified versions of my work, proper credit is always required and you should always link back to this original source and respect the licence.

## Libraries used (many thanks to their authors)
* [CitizenFX.Core.Client](https://www.nuget.org/packages/CitizenFX.Core.Client/)
