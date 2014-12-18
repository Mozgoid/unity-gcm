unity-gcm
============================

## About this repository

Forked from https://github.com/kobakei/unity-gcm

Plugin adapted to work with Parse notifications. Also it works with iOS parse notifications.

Notification data is now taken from the field "data" of incoming json.

Parameter "content_title" is optional now, and by default, it is equal to the application name. The "content_text" replaced by "alert" to work with the format of the Parse notifications.

## Installation

https://github.com/Mozgoid/unity-gcm/wiki/Installation

## Known Issues

Unity won't start by tapping on notification if you overrided your main activity. Change it in UnityGCMNotificationManager.java on line 41.

