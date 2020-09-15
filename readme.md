This project is using an offline SDF baker (SDFr) to precompute object local SDF, and maintaining a global distance field at runtime.

The current shadowing functions run at about 120 fps, while I guess it would be more suitable for static scene with a few moveable objects, which means that you'd better to use it for things that seldom change or move, to avoid efficiency issues. 

There are some slides with videos in this repo to illustrate the project better, originally it was not in English and was converted by google translate, please check it you'd like to. Anyway, you could get a preview of the result from the following screenshots.

![screenshot preview](screencaps/screenshot.png)

![screenshot](screencaps/screenshot2.png)
