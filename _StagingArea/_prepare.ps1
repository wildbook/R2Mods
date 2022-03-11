Copy-Item ../MiniRpcLib/bin/Release/netstandard2.0/MiniRpcLib.dll         MiniRpcLib/
Copy-Item ../Multitudes/bin/Release/netstandard2.0/Multitudes.dll         Multitudes/
Copy-Item ../TooManyFriends/bin/Release/netstandard2.0/TooManyFriends.dll TooManyFriends/

Compress-Archive -Path MiniRpcLib/*     -Destination zips/MiniRpcLib.zip     -Force
Compress-Archive -Path Multitudes/*     -Destination zips/Multitudes.zip     -Force
Compress-Archive -Path TooManyFriends/* -Destination zips/TooManyFriends.zip -Force
