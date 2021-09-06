MSBuild /nologo /verbosity:m /maxcpucount /p:Configuration="Release",Platform="Win32" "../libwtfdanmaku/libwtfdanmaku.vcxproj"
MSBuild /nologo /verbosity:m /maxcpucount /p:Configuration="Release",Platform="x64" "../libwtfdanmaku/libwtfdanmaku.vcxproj"
pushd ..
md Bililive_dm\Win32
copy libwtfdanmaku\Release\Win32\*.dll Bililive_dm\Win32\
md Bililive_dm\x64
copy libwtfdanmaku\Release\x64\*.dll Bililive_dm\x64\
