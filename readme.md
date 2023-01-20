﻿# INAV SIM OSD

PC OSD for INAV SITL/HITL

![INAV-Sim-OSD](img/INAV-SIM-OSD.png)

You may need to install the .NET Desktop Runtime 6 (latest version): https://dotnet.microsoft.com/en-us/download/dotnet/6.0

## SITL:
Configure a MSP-Displayport on any UART and connect via TCP to localhost (127.0.0.1) and the corresponding TCP port.
See INAV SITL documentaion for more details.

## HITL:
Configure a MSP-Displayport on any UART and connect via UART to USB module and select corresponding COM-Port.

## Font:
The HD fonts from DJI WTFOS are used. New or updated fonts can simply be placed in a new subfolder in "osd-font".

# Supported Simulators:
- RealFlight
- XPlane

# Known issues:
- Works only if simulator is in window mode (No fullscreen)
- OSD stick menu isn't working (will be fixed soon)