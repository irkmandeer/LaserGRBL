## Experimental LaserGBRL
This is a highly experimental version of LaserGRBL with features for personal use.

This repository is for inspirational purposes only. 

**Do not use this version for any purpose, please use the official LaserGRBL**

The link to the official LaserGRBL can be found below.

### History
- 2020-05-03: [Added Engraving Progress to the Job Preview](https://github.com/irkmandeer/LaserGRBL/commit/8be50889bfb8ecf805eda2c35ce48ff0bbf7ce80)
- 2020-05-06: [Added a Progress Dialog](https://github.com/urkmandeer/LaserGRBL/commit/ba728963db221dadd3a91514c74443934652ea17)
- 2020-05-06: [Added Nearest Neighbour GCode Optimization for SVG files](https://github.com/irkmandeer/LaserGRBL/commit/3e070a0ab9183febb6b6571b2b9201a918fc6970)
- 2020-05-07: [Added CoreXY Laser and Servo Firmware](https://github.com/irkmandeer/LaserGRBL/commit/f11100a94757e9daf7e009ab9b77c778b0fe2fe8)
- 2020-05-11 [Added Optimize Menu Item, optimization fixes, and Z axis movement](https://github.com/irkmandeer/LaserGRBL/commit/d4dd2724ad20c4f77b4669bca3ad12f4ebccc889)
- 2020-05-11 [Fixed GCode Optimization deleting arcs](https://github.com/irkmandeer/LaserGRBL/commit/29b1ca080e04f05b669fed89c591f9f72cc834ae)
- 2020-05-12 [Position Laser by Double-Clicking](https://github.com/irkmandeer/LaserGRBL/commit/c1bbc93fc8862421a3a9a624385c44ab699a9663)
- 2020-05-12 [Handle Inkscape Group and Layer Visibility](https://github.com/irkmandeer/LaserGRBL/commit/f07f70e6e8c4622a5543082c8c690753f01677b0)

-------------------------------------------------------------------

# LaserGRBL [![Donation](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/pools/c/8cQ1Lo4sRA)
Official website [http://lasergrbl.com](http://lasergrbl.com)

LaserGRBL is a Windows GUI for [GRBL](https://github.com/gnea/grbl/wiki). Unlike other GUI LaserGRBL it is specifically developed for use with laser cutter and engraver. In order to use all of LaserGRBL feature, your engraver must supports laser power modulation through gcode "S" command. LaserGRBL is compatible with [Grbl v0.9](https://github.com/grbl/grbl/) and [Grbl v1.1](https://github.com/gnea/grbl/)

All downloads available at https://github.com/arkypita/LaserGRBL/releases

### Support and Donation

Do you like LaserGRBL? Support development with your donation!

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/pools/c/8cQ1Lo4sRA)

### Existing Features

- GCode file loading with engraving/cutting job preview (with alpha blending for grayscale engraving)
- Image import (jpg, bmp...) with line by line GCode generation (horizontal, vertical, and diagonal).
- Image import (jpg, bmp...) with Vectorization!
- Image import (jpg, bmp...) with 1bit dithering, best result with low power laser
- Vector file import (svg only) [Experimental]
- Different color scheme optimized for different safety glasses
- User defined buttons, power to you!
- Grbl Configuration Import/Export
- Configuration, Alarm and Error codes decoding for Grbl v1.1 (with description tooltip)
- Homing button, Feed Hold button, Resume button and Grbl Reset button
- Job time preview and realtime projection
- Jogging (for any Grbl version)
- Feed overrides (for Grbl > v1.1) with easy-to-use interface
- Support for [WiFi connection via ESP8266 WebSocket](http://lasergrbl.com/en/usage/wifi-with-esp8266/)

### Usage

* [LaserGRBL User Interface](http://lasergrbl.com/usage/user-interface/)
* [Connect to arduino-grbl](http://lasergrbl.com/usage/arduino-connection/)
* [Load G-Code and send to machine](http://lasergrbl.com/usage/load-and-send/)
* [Speed and power overrides](http://lasergrbl.com/usage/overrides/)
* [Jogging](http://lasergrbl.com/usage/jogging/)
* [Custom buttons](http://lasergrbl.com/usage/custom-buttons/)
* [Raster Image Import](http://lasergrbl.com/usage/raster-image-import/)
* [Grayscale conversion parameters](http://lasergrbl.com/usage/raster-image-import/import-parameters/)
* [Line 2 line grayscale conversion](http://lasergrbl.com/usage/raster-image-import/line-to-line-tool/)
* [1bit dithering conversion](http://lasergrbl.com/usage/raster-image-import/dithering-tool/)
* [Image vectorization](http://lasergrbl.com/usage/raster-image-import/vectorization-tool/)

### Development Roadmap

Development status and roadmap can be found here: [Roadmap](https://github.com/arkypita/LaserGRBL/issues/64)

### Missing Features

- Minimal Z axis control (LaserGRBL is for XY machine)

#### Screenshot and videos

[<img src="https://cloud.githubusercontent.com/assets/8782035/23578353/fba95768-00d4-11e7-9357-99c00a30631d.jpg">](https://www.youtube.com/watch?v=Uk2fGoNL3Yk)

![Galeon](https://cloud.githubusercontent.com/assets/8782035/21349915/dba84a5a-c6b4-11e6-965f-a74fd283267a.jpg)

![Raster2Laser](https://cloud.githubusercontent.com/assets/8782035/21425748/34400d46-c84b-11e6-99e5-6eb529a98f8f.jpg)

![Alpha](https://cloud.githubusercontent.com/assets/8782035/21351296/1df460c2-c6bc-11e6-8eee-4612bb7978fa.jpg)

![FinalWork](https://cloud.githubusercontent.com/assets/8782035/21907662/bbe988be-d910-11e6-9bdb-75b6e3404e0a.jpg)

![UserDefinedButtons](https://cloud.githubusercontent.com/assets/8782035/23375844/238e5f70-fd2a-11e6-8826-5ff7743bbea0.jpg)

### Compiling

LaserGRBL is written in C# for .NET Framework 3.5 (or higher) and can be compiled with [SharpDevelop](http://www.icsharpcode.net/opensource/sd/) and of course with [Microsoft Visual Studio](https://www.visualstudio.com) IDE/Compiler

### Licensing

LaserGRBL is free software, released under the [GPLv3 license](https://www.gnu.org/licenses/gpl-3.0.en.html).

### Credits and Contribution

LaserGRBL contains some code from:
- [ColorSlider](https://www.codeproject.com/articles/17395/owner-drawn-trackbar-slider) - Copyright Michal Brylka
- [CsPotrace](https://drawing3d.de/Downloads.aspx) - Copyright Peter Selinger, port by Wolfgang Nagl
- [Bezier2Biarc](https://github.com/domoszlai/bezier2biarc) - Copyright Laszlo
- [websocket-sharp](https://github.com/sta/websocket-sharp) - Copyright sta.blockhead
- [Expression Evaluator](https://github.com/vubiostat/expression.cs) - Copyright Will Gray, Jeremy Roberts
- [GCodeFromSVG](https://github.com/svenhb/GRBL-Plotter) - Copyright Sven Hasemann
- [MS SVG Library](https://archive.codeplex.com/?p=svg) - Microsoft Public License

Thanks to:
- Myself, for italian and english language
- Fernando Luna, sqall123, for spanish translation
- Olivier Salvador, guillaume-rico [#848](https://github.com/arkypita/LaserGRBL/pull/848) for french translation
- Gerd Vogel, for german translation
- Anders Lassen, for danish translation
- Gerson Koppe, for brasilian translation
- Alexey Golovin, Newcomere, AlexeyBond, for russian translation
- Yang Haiqiang, for chinese translation
- 00alkskodi00, for slovak translation [#670](https://github.com/arkypita/LaserGRBL/issues/670)
- ddogman, for hungarian translation [#735](https://github.com/arkypita/LaserGRBL/issues/735)
- Petr Bitnar, for czech translation
- Filippo Rivato for code contribution [#305](https://github.com/arkypita/LaserGRBL/pull/305)
- Fabio Ferretti for code contribution [#592](https://github.com/arkypita/LaserGRBL/pull/592)
- guillaume-rico for code contribution on Smoothie support
- Tobias Falkner, for code contribution [#937](https://github.com/arkypita/LaserGRBL/pull/937)
