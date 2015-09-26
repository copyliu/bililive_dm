MSBuild /nologo /verbosity:m /maxcpucount /p:Configuration="Debug",Platform="Win32",SolutionDir="../" "../libwtfdanmaku/libwtfdanmaku.vcxproj"
MSBuild /nologo /verbosity:m /maxcpucount /p:Configuration="Debug",Platform="x64",SolutionDir="../" "../libwtfdanmaku/libwtfdanmaku.vcxproj"
copy ..\Debug\Win32\*.dll ..\Bililive_dm\Win32\
copy ..\Debug\x64\*.dll ..\Bililive_dm\x64\