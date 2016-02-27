# VRChat
VRChat source code for MHack. This is an attempt to realize a new fancy way to chat with each other over the Internet. Powered by Kinect and Google Cardboard, not only voice, real time movement animation is also provided in an immersive virtual reality world.

## Feature
- Voice chatting
- Virtual reality world with characters of players
- Real time body movement capture

## Note
- The virtual reality experience is provided by Google Cardboard, with a mobile phone inside.
- Player should stay in the detection area supported by Kinect.

## Basic Setup
1. Kinect stream server [source code](https://github.com/whuchenrui/hackathon_VRchat)
2. Kinect v2
3. Google Cardboard * 2
4. Mobile phone * 2 (Either Android or iOS, should have rather modern hardware)

## Build
This is a Unity project which can be opened in unity editor 5+. Android and iOS target were tested and supported, but other platforms might also work.

The auto detection of peers is still not work right now, so you need to manually set various IP addresses in the main scene.
