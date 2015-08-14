#include "stdafx.h"
typedef INT(WINAPI *Crc32)(INT accumCRC32, const BYTE* buffer, UINT buflen);
HMODULE hNtdll = GetModuleHandle(_T("ntdll.dll"));
Crc32 ComputeCrc32 = (Crc32)GetProcAddress(hNtdll, "RtlComputeCrc32");