# Tele-Rehabilitation With Unity
Microprocessors' code to be uploaded using the Arduino IDE

## Toolchain
* [Arduino IDE](https://www.arduino.cc/en/Main/OldSoftwareReleases) version 1.6.8
* [Arduino SAMD Boards](https://learn.adafruit.com/adafruit-feather-m0-basic-proto/setup) version 1.6.13
* [Adafruit SAMD Boards](https://learn.adafruit.com/adafruit-feather-m0-basic-proto/using-with-arduino-ide) version 1.6.8
* [Adafruit BNO055](https://github.com/adafruit/Adafruit_BNO055) version 1.1.3
* [Adafruit Unified Sensor](https://github.com/adafruit/Adafruit_Sensor) version 1.0.2
* [RF24 (TMRH20)](https://github.com/nRF24/RF24) version 1.1.6 

It is essential to firstly install the Arduino and Adafruit SAMD Boards in the Arduino IDE following the links placed above.
The libraries Adafruit BNO055, Adafruit Unified Sensor, and RF24, are included in the */libraries/* folder. You just need to copy+paste them into the Arduino libraries folder (typically *Your_User/Documents/Arduino/libraries/*)

## Code

The code that must be uploaded to the Feather M0 boards (master and nodes) is included in the */code/rframe24/* folder. In order to upload it correctly, you must change the *module_id* in line 25, being:

*module_id = 0* for master

*module_id = 1....7* for nodes.
