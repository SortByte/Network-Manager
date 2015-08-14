#pragma once

extern TCHAR installedNet4Version[260];
extern TCHAR installedVC2010Version[260];
extern bool net4Installed;
extern bool kb2468871Installed;
extern bool vc2010Installed;
extern SYSTEM_INFO systemInfo;
void CheckDependencies();