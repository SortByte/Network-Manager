#pragma once
typedef INT(WINAPI *Crc32)(INT accumCRC32, const BYTE* buffer, UINT buflen);
extern Crc32 ComputeCrc32;