This project is using an offline SDF baker (SDFr) to precompute object local SDF, and maintaining a global distance field at runtime.

The current shadowing functions run at about 120 fps, while I guess it would be more suitable for static scene with a few moveable objects, which means that you'd better to use it for things that seldom change or move, to avoid efficiency issues. 

There are some slides with videos for this project [here](https://drive.google.com/file/d/10ICPY05gsmkJ11PfgKT2xEUlrhfY8__h/view?usp=sharing), originally it was not in English and was converted by google translate, please check it you'd like to, you're also welcome to ask any questions via Issues. 

Other than the slides, you may get a preview of the results from the following screenshots.

![screenshot preview](screencaps/screenshot.png)

![screenshot](screencaps/screenshot2.png)
