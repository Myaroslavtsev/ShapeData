This app converts shape described in .csv into many MSTS (Microsoft train simulator) / OR (Open Rails) shapes, replicated along track sections from tsection.dat
May be useful for those who are modelling their own routes and route objects.

If you need just the program, pleasse download the last release v 0.1 here:
https://disk.yandex.ru/d/mVBe7da-twulPg 

Usage: <input shape.csv> <tsection.dat> [/m:<mask>] [/f:<ffedit>] [/c:<count>] [/g:<gauge>] [/r] [/b]

1. Path and filename of .csv with the description of shape structure to create.
2. Path and filename of tsection.dat, eg. "C:\Train\Global\tsection.dat".
Paths may be omitted when files are in the current folder.
/m: Create only shapes corresponding to the mask. * and ? symbols may be used, eg. /m:"A?t*.s"
/f: Full path of ffeditc_unicode.exe if you want to compress created shapes immediately
/c: Limit the number of created shapes, eg. /c:20
/g: Convert only shapes with given track gauge. Use . as delimiter, eg. /f:1.5
/r  Don't skip road shapes. Otherwise track shapes processed only. No value needed, eg. just /r
/b  Limit shape visibility angles by setting bounding box in .sd. No value needed, eg. just /b

After conversion .ref entries are created.
After conversion a .bat to compress shapes later with ffeditc_unicode.exe is also created.

PartSampleCreationTest generates data file SampleShape.csv which can be used as a template for creating your own models.
The file recreates soviet-style wooden track with D-65 mounts. 
Note that extremely short track sections and sections with extremely large radia may require changing the model structure in .csv.
Also, auto generated points and X-overs are of low quality and require manual modelling (a kind of expected drawback).

Get further help at https://trainsim.ru/forum/

(c) Myaroslavtsev, 2025. Published under CC-BY-NC conditions.
