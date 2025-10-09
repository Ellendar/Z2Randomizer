# Z2 Randomizer 5.0 FAQ

## General issues

### Windows is telling me this is a virus. What gives?

Windows will normally flag the installer as suspicious because there is no code signing. There have been some cases where Windows Defender detects this version of the Randomizer as a Trojan. This is a false positive, but getting around it would be difficult. You can ignore these warnings and install anyway. If you are doubtful, downloading the source and building yourself will get around any security notices.
If you have issues you can't resolve with windows defender, let us know in the discord!

### I use a non-Windows operating system. Can I still use the randomizer?

Yes! There are a couple available options:

- A web version is available at https://z2r.app/ . It should generate identically to the desktop version, and work on any major browser.
- Build the randomizer from source for your platform. See the instructions [here](https://github.com/Ellendar/Z2Randomizer/wiki/Building-From-Source)

### Right after installing, I'm getting a correct hash but incorrect seed. What's up?

Sometimes after installing, if you had to download the .NET8 runtimes, the wrong runtimes will be used by the application, potentially resulting in different randomness.
Restarting your computer will correct this.

### I found a bug / crash / uncompleteable seed. What should I do?

Try any of these options

- Post in #z2r-development on the Z2R discord. Feel free to ping me (Ellendar).
- DM me directly on discord. I may not be able to get back to you right away, but I can take a look at your issue.
- [Submit an Issue](https://github.com/Ellendar/Z2Randomizer/issues/new) here on GitHub. 

## Migrating from 4.0.4

### Will this overwrite my current install of 4.0.4?

No. Because this version is hosted separately from 4.0.4, it needs to be installed separately. It will be named "Z2 Randomizer Community Fork" once it is installed.

### What are all these options with question marks instead of blank or checked

This indicates the option will be off or on at random. Adjust the chance of this in the Start tab.

### How do I convert my old 4.0.4 flags to the new version?

Sadly it has been too long and the flag systems are too incompatable to convert ond flags. Check the preset flags for inspiration or create your own flagset and save it.

