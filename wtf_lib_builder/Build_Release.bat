MSBuild /nologo /verbosity:m /maxcpucount /p:Configuration="Release",Platform="Win32",SolutionDir="../" "../libwtfdanmaku/libwtfdanmaku.vcxproj"
MSBuild /nologo /verbosity:m /maxcpucount /p:Configuration="Release",Platform="x64",SolutionDir="../" "../libwtfdanmaku/libwtfdanmaku.vcxproj"
copy ..\Release\Win32\*.dll ..\Bililive_dm\Win32\
copy ..\Release\x64\*.dll ..\Bililive_dm\x64\